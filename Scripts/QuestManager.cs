using System;
using System.Collections.Generic;
using Godot;

namespace LiteQuest.QuestSystem;

/// <summary>
/// 任务运行时管理器。对应 Unity 版 QuestManager (MonoBehaviour 单例)。
///
/// Unity → Godot 映射：
///   MonoBehaviour Instance     → Node 单例（Main 场景 AddChild 或注册为 AutoLoad）
///   UnityEvent&lt;QuestDefinition&gt; → C# event Action&lt;QuestDefinition&gt;
///   ScriptableObject 引用比对  → Resource.ResourcePath 字符串比对
///
/// 职责：
///   1. 注册 QuestDefinition，维护 ResourcePath → QuestProgress 运行时状态
///   2. StartQuest：检查任务链（前置任务必须全部 Completed）
///   3. ReportCollect/ReportKill/ReportDamage/EnterTrigger：按事件类型过滤推进 5 种目标
///   4. CompleteQuest：发奖励事件 + 自动启动任务链下游（AutoStart 的后续任务）
///   5. GetProgressSnapshot / LoadFromSnapshot：为 Day 6 存档/读档预留
/// </summary>
public partial class QuestManager : Node
{
    public static QuestManager Instance { get; private set; }

    private readonly Dictionary<string, QuestProgress> _progressByPath = new();
    private readonly Dictionary<string, QuestDefinition> _definitionByPath = new();

    // ===== 事件回调（UI / 奖励系统 / 日志都挂这里）=====
    public event Action<QuestDefinition> OnQuestStarted;
    public event Action<QuestDefinition, int, int> OnProgressChanged; // quest, oldCurrent, newCurrent
    public event Action<QuestDefinition> OnQuestCompleted;
    public event Action<QuestDefinition, int, int> OnRewardsGranted;   // quest, gold, exp

    public override void _Ready()
    {
        Instance = this;
    }

    // ===== 注册 =====

    public void RegisterQuest(QuestDefinition quest)
    {
        if (quest == null) return;
        string path = quest.ResourcePath;
        if (string.IsNullOrEmpty(path))
        {
            GD.PrintErr("[QuestManager] Quest has no ResourcePath: " + quest.QuestName);
            return;
        }
        _definitionByPath[path] = quest;
        if (!_progressByPath.ContainsKey(path))
        {
            _progressByPath[path] = new QuestProgress
            {
                QuestResourcePath = path,
                Status = QuestStatus.NotStarted,
                CurrentProgress = 0,
                CompletedAtUnixMs = 0
            };
        }
    }

    public void RegisterAll(params QuestDefinition[] quests)
    {
        if (quests == null) return;
        foreach (var q in quests) RegisterQuest(q);
    }

    /// <summary>启动所有 AutoStart=true 且当前可启动（无前置或前置已完成）的任务。</summary>
    public void StartAutoStartQuests()
    {
        // 拷贝一份，StartQuest 不会改字典结构，但保持遍历安全
        var defs = new List<QuestDefinition>(_definitionByPath.Values);
        foreach (var q in defs)
        {
            if (q.AutoStart && CanStart(q))
                StartQuest(q);
        }
    }

    public IReadOnlyCollection<QuestDefinition> GetAllDefinitions() => _definitionByPath.Values;

    // ===== 查询 =====

    public QuestStatus GetStatus(QuestDefinition quest)
    {
        if (quest == null || !_progressByPath.TryGetValue(quest.ResourcePath, out var p))
            return QuestStatus.NotStarted;
        return p.Status;
    }

    public int GetCurrentProgress(QuestDefinition quest)
    {
        if (quest == null || !_progressByPath.TryGetValue(quest.ResourcePath, out var p))
            return 0;
        return p.CurrentProgress;
    }

    public bool IsCompleted(QuestDefinition quest) => GetStatus(quest) == QuestStatus.Completed;
    public bool IsInProgress(QuestDefinition quest) => GetStatus(quest) == QuestStatus.InProgress;

    /// <summary>任务是否可以启动：必须 NotStarted 且所有前置任务都 Completed。</summary>
    public bool CanStart(QuestDefinition quest)
    {
        if (quest == null) return false;
        if (GetStatus(quest) != QuestStatus.NotStarted) return false;
        if (!quest.HasPrerequisites) return true;
        foreach (var pre in quest.GetPrerequisiteQuests())
        {
            if (!IsCompleted(pre)) return false;
        }
        return true;
    }

    // ===== 启动 =====

    public bool StartQuest(QuestDefinition quest)
    {
        if (quest == null) return false;
        if (!_progressByPath.ContainsKey(quest.ResourcePath)) RegisterQuest(quest);

        if (!CanStart(quest))
        {
            GD.Print("[QuestManager] Cannot start quest (prerequisites not met or already active): " + quest.QuestName);
            return false;
        }

        var p = _progressByPath[quest.ResourcePath];
        p.Status = QuestStatus.InProgress;
        p.CurrentProgress = 0;
        OnQuestStarted?.Invoke(quest);
        return true;
    }

    /// <summary>上报收集进度（拾取物品）。只推进 Collect 类型任务。</summary>
    public void ReportCollect(string targetId, int amount = 1)
        => Advance(targetId, amount, QuestObjectiveType.Collect);

    /// <summary>上报击杀（敌人死亡事件）。只推进 Kill 类型任务，每次计数 +amount。</summary>
    public void ReportKill(string targetId, int amount = 1)
        => Advance(targetId, amount, QuestObjectiveType.Kill);

    /// <summary>上报伤害（敌人受伤事件，传本次伤害值）。只推进 Damage 类型任务。</summary>
    public void ReportDamage(string targetId, int damage)
        => Advance(targetId, damage, QuestObjectiveType.Damage);

    /// <summary>进入触发区域。只推进 ReachLocation 类型任务，一次性置为完成。</summary>
    public void EnterTrigger(string triggerId)
        => Advance(triggerId, 1, QuestObjectiveType.ReachLocation);

    /// <summary>
    /// 按目标类型过滤推进。关键设计：同一次攻击会先后触发 Damaged 和 Died 两个事件，
    /// 只有按 ObjectiveType 过滤，才能保证"伤害值"不会被错误累加进 Kill 任务、
    /// "击杀计数"也不会推进 Damage 任务。targetId 相同的多个同类进行中任务会同时推进。
    /// </summary>
    private void Advance(string targetId, int amount, QuestObjectiveType eventType)
    {
        if (string.IsNullOrEmpty(targetId) || amount <= 0) return;

        // 先收集匹配任务，避免 CompleteQuest → TryStartFollowUpQuests 嵌套触发时遍历被改
        var toAdvance = new List<(QuestDefinition def, QuestProgress prog)>();
        foreach (var pair in _definitionByPath)
        {
            var def = pair.Value;
            var prog = _progressByPath[pair.Key];
            if (prog.Status != QuestStatus.InProgress) continue;
            if (def.ObjectiveType != eventType) continue; // 事件类型与任务类型必须一致
            if (def.TargetId != targetId) continue;
            toAdvance.Add((def, prog));
        }

        foreach (var (def, prog) in toAdvance)
        {
            int oldCurrent = prog.CurrentProgress;

            if (eventType == QuestObjectiveType.ReachLocation)
            {
                // 到达位置：一次性触发（TargetCount 应配为 1）
                if (prog.CurrentProgress == 0)
                {
                    prog.CurrentProgress = 1;
                    OnProgressChanged?.Invoke(def, 0, 1);
                }
            }
            else
            {
                // Collect / Kill 按次计数，Damage 按伤害值累计，统一为累加并钳制
                prog.CurrentProgress = Math.Min(def.TargetCount, prog.CurrentProgress + amount);
                OnProgressChanged?.Invoke(def, oldCurrent, prog.CurrentProgress);
            }

            // Custom 类型没有上报通道，由调用方直接 CompleteQuest
            if (prog.Status == QuestStatus.InProgress && prog.CurrentProgress >= def.TargetCount)
                CompleteQuest(def);
        }
    }

    // ===== 完成 =====

    public bool CompleteQuest(QuestDefinition quest)
    {
        if (quest == null) return false;
        if (!_progressByPath.TryGetValue(quest.ResourcePath, out var p)) return false;
        if (p.Status == QuestStatus.Completed) return false; // 幂等

        p.Status = QuestStatus.Completed;
        p.CompletedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (quest.GoldReward > 0 || quest.ExpReward > 0)
            OnRewardsGranted?.Invoke(quest, quest.GoldReward, quest.ExpReward);
        OnQuestCompleted?.Invoke(quest);

        // 任务链下游：自动启动以本任务为前置且 AutoStart=true 的后续任务
        TryStartFollowUpQuests(quest);
        return true;
    }

    private void TryStartFollowUpQuests(QuestDefinition justCompleted)
    {
        foreach (var def in _definitionByPath.Values)
        {
            if (!def.AutoStart) continue;
            if (GetStatus(def) != QuestStatus.NotStarted) continue;

            bool depends = false;
            foreach (var pre in def.GetPrerequisiteQuests())
            {
                if (pre != null && pre.ResourcePath == justCompleted.ResourcePath)
                {
                    depends = true;
                    break;
                }
            }
            if (depends && CanStart(def))
                StartQuest(def);
        }
    }

    // ===== 存档快照（Day 6 使用）=====

    public List<QuestProgress> GetProgressSnapshot()
    {
        var list = new List<QuestProgress>(_progressByPath.Count);
        foreach (var p in _progressByPath.Values) list.Add(p);
        return list;
    }

    /// <summary>
    /// 从快照恢复。调用方需先 RegisterAll 重新加载 QuestDefinition 资源。
    /// 合并策略：已注册但快照里没有的任务（版本更新后新增的任务）补 NotStarted，
    /// 快照里有但当前未注册的条目丢弃，保证进度上报永远不会因缺键崩溃。
    /// </summary>
    public void LoadFromSnapshot(List<QuestProgress> snapshot)
    {
        var byPath = new Dictionary<string, QuestProgress>();
        if (snapshot != null)
        {
            foreach (var p in snapshot)
            {
                if (p == null || string.IsNullOrEmpty(p.QuestResourcePath)) continue;
                byPath[p.QuestResourcePath] = p;
            }
        }

        _progressByPath.Clear();
        foreach (var path in _definitionByPath.Keys)
        {
            if (byPath.TryGetValue(path, out var saved))
                _progressByPath[path] = saved;
            else
                _progressByPath[path] = new QuestProgress
                {
                    QuestResourcePath = path,
                    Status = QuestStatus.NotStarted,
                    CurrentProgress = 0,
                    CompletedAtUnixMs = 0
                };
        }
    }
}
