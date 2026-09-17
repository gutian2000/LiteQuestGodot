using Godot;
using Godot.Collections;

namespace LiteQuest.QuestSystem;

/// <summary>
/// 任务定义资产，对应 Unity 版的 QuestDefinition (ScriptableObject)。
///
/// Unity → Godot 映射：
///   ScriptableObject       → Resource
///   [CreateAssetMenu]      → [GlobalClass]（让它出现在"新建资源"菜单）
///   public 字段            → [Export] 字段（Godot 4 C# 导出字段比属性更可靠）
///   [Header]               → [ExportCategory]
///   [TextArea]             → [Export(PropertyHint.MultilineText)]
///   OnValidate()           → 在 Godot 编辑器里直接填默认值
///
/// ⚠️ Godot 4 C# 注意事项：
///   1. 导出集合必须用 Godot.Collections.Array<T>，不能用 Resource[] 或 List<T>
///   2. 用字段而不是 auto-property，避免序列化空值 bug
///   3. 集合默认值必须用 new()，不能用 Array.Empty<T>()（全局共享引用）
/// </summary>
[GlobalClass]
public partial class QuestDefinition : Resource
{
    // ===== 基础信息 =====

    /// <summary>任务名称，显示在追踪栏和任务面板里。</summary>
    [Export]
    public string QuestName = "Collect Coins";

    /// <summary>任务面板里显示的描述文字。</summary>
    [Export(PropertyHint.MultilineText)]
    public string Description = "Collect the required items.";

    /// <summary>完成后显示的消息。</summary>
    [Export(PropertyHint.MultilineText)]
    public string CompletionMessage = "Quest complete!";

    // ===== 目标配置 =====

    /// <summary>目标类型：收集/击杀/伤害累计/到达位置/自定义。</summary>
    [ExportCategory("Objective")]
    [Export]
    public QuestObjectiveType ObjectiveType = QuestObjectiveType.Collect;

    /// <summary>
    /// 目标 Id。
    /// Collect  → 必须和 CollectibleDefinition.TargetId 一致
    /// Kill     → 敌人的 EnemyId
    /// Damage   → 敌人的 EnemyId
    /// ReachLoc → Area2D 触发器的 TriggerId
    /// Custom   → 任意字符串
    /// </summary>
    [Export]
    public string TargetId = "Coin";

    /// <summary>
    /// 目标数量/阈值（必须 >= 1）。
    /// Collect/Kill  → 需要的数量
    /// Damage        → 累计伤害阈值
    /// ReachLoc      → 固定 1
    /// Custom        → 固定 1
    /// </summary>
    [Export]
    public int TargetCount = 5;

    // ===== 自动开始 =====

    /// <summary>勾选后进入场景自动开始任务；不勾选则需要 NPC 或脚本手动 StartQuest。</summary>
    [ExportCategory("Auto Start & Prerequisite")]
    [Export]
    public bool AutoStart = true;

    // ===== 任务链（前置任务） =====

    /// <summary>
    /// 前置任务列表。所有前置任务都 Completed 后，此任务才能被 StartQuest 成功启动。
    /// 为空表示无前置条件。
    /// 必须用 Godot.Collections.Array<Resource>，Godot 才能正确序列化。
    /// </summary>
    [Export]
    public Array<Resource> PrerequisiteQuests = new();

    // ===== 奖励 =====

    /// <summary>完成时奖励的金币数量。</summary>
    [ExportCategory("Rewards")]
    [Export]
    public int GoldReward = 0;

    /// <summary>完成时奖励的经验值。</summary>
    [Export]
    public int ExpReward = 0;

    // ===== 辅助方法 =====

    /// <summary>检查是否有前置任务。</summary>
    public bool HasPrerequisites => PrerequisiteQuests != null && PrerequisiteQuests.Count > 0;

    /// <summary>
    /// 把 Array<Resource> 强制转换为 QuestDefinition[]。
    /// </summary>
    public QuestDefinition[] GetPrerequisiteQuests()
    {
        if (!HasPrerequisites) return System.Array.Empty<QuestDefinition>();
        var result = new QuestDefinition[PrerequisiteQuests.Count];
        for (int i = 0; i < PrerequisiteQuests.Count; i++)
            result[i] = (QuestDefinition)PrerequisiteQuests[i];
        return result;
    }
}
