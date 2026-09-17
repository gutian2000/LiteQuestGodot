using System.Collections.Generic;
using Godot;
using LiteQuest.QuestSystem;
using LiteQuest.QuestSystem.Gameplay;
using LiteQuest.QuestSystem.Save;
using LiteQuest.QuestSystem.UI;

namespace LiteQuest.QuestSystem.Demo;

/// <summary>
/// LiteQuest 功能演示场景的总控（对应 Demo/DemoScene.tscn 的根节点）。
///
/// 一个文件展示全部 5 种目标类型 + 任务链 + NPC 接任务 + 存档：
///   · AutoStart 的 Collect / Kill / Damage / ReachLocation 任务自动出现
///   · AutoStart=false 的 NPC 任务需靠近商人按 E，前置完成后才能接
///   · F2/F3/F4 存档/读档/删档
///
/// 接入自己的项目时，把本文件的 BuildWorld/输入部分替换成你的游戏内容，
/// 任务系统本身（Scripts/ 下的类）与演示无关，可直接复用。
/// </summary>
public partial class DemoController : Node2D
{
    private const int AttackDamage = 6;
    private const float AttackRange = 130f;
    private const float NpcInteractRange = 150f;

    private QuestDefinition _npcQuest;
    private QuestTrackerUI _tracker;
    private QuestDialogUI _dialog;
    private QuestManager _manager;
    private DemoPlayer _player;
    private QuestGiverNPC _questGiver;
    private LocationTrigger _gate;
    private readonly List<Enemy> _enemies = new();

    public override void _Ready()
    {
        GD.Print("LiteQuest Demo ready.");

        // 顺序很关键：先加 QuestManager（其 _Ready 设置 Instance），再加 UI（其 _Ready 订阅事件）
        _manager = new QuestManager();
        AddChild(_manager);

        _tracker = new QuestTrackerUI();
        AddChild(_tracker);

        _dialog = new QuestDialogUI();
        AddChild(_dialog);

        // 1) 注册全部 6 个任务（Collect 链 3 个 + Kill/Damage/Reach 3 个）
        var coins = GD.Load<QuestDefinition>("res://Assets/Quests/CollectCoinsQuest.tres");
        var crystals = GD.Load<QuestDefinition>("res://Assets/Quests/CollectCrystalsQuest.tres");
        _npcQuest = GD.Load<QuestDefinition>("res://Assets/Quests/CollectCrystalsFromNPCQuest.tres");
        var killSlimes = GD.Load<QuestDefinition>("res://Assets/Quests/KillSlimesQuest.tres");
        var damageSlimes = GD.Load<QuestDefinition>("res://Assets/Quests/DamageSlimesQuest.tres");
        var reachGate = GD.Load<QuestDefinition>("res://Assets/Quests/ReachTownGateQuest.tres");
        _manager.RegisterAll(coins, crystals, _npcQuest, killSlimes, damageSlimes, reachGate);

        _manager.OnRewardsGranted += (q, gold, exp) =>
            GD.Print($"[奖励] {q.QuestName} 完成，获得 {gold} 金币 / {exp} 经验");

        // 截图模式强制干净新游戏
        if (!string.IsNullOrEmpty(ParseShotArg()))
            QuestSaveSystem.DeleteSave();

        // 2) 有存档 → 恢复；无存档 → 新游戏
        var save = QuestSaveSystem.Load();
        if (save != null)
        {
            _manager.LoadFromSnapshot(save.Quests);
            _tracker.RebuildFromManager();
            // 版本更新新增的 AutoStart 任务（旧存档里没有）在此自动激活
            _manager.StartAutoStartQuests();
        }
        else
        {
            _manager.StartAutoStartQuests();
        }

        // 3) 生成演示世界：玩家 + 商人 + 3 只史莱姆 + 城门触发器
        BuildWorld();

        BuildHelpOverlay();

        // 4) 截图导演模式：--shot <overview|combat|dialog|completion|gate>
        //    自动布置画面 → viewport 存 PNG → 退出，供商店素材批量出图
        var shotName = ParseShotArg();
        if (shotName != null)
            RunShotDirector(shotName);
    }

    // 通过环境变量传截图指令（Godot 命令行的用户参数解析在不同调用方式下不稳定，
    // 环境变量零歧义）：$env:LITEQUEST_SHOT="overview"
    private static string ParseShotArg()
        => System.Environment.GetEnvironmentVariable("LITEQUEST_SHOT");

    private void BuildWorld()
    {
        _player = new DemoPlayer { Position = new Vector2(150, 320) };
        AddChild(_player);

        // 商人 NPC：发布需要手动接取的 Collect Crystals for NPC 任务
        _questGiver = new QuestGiverNPC
        {
            NpcName = "商人",
            Quest = _npcQuest,
            Position = new Vector2(290, 320)
        };
        AddChild(_questGiver);

        var slimePositions = new[]
        {
            new Vector2(480, 200),
            new Vector2(580, 360),
            new Vector2(470, 480)
        };
        foreach (var pos in slimePositions)
        {
            var enemy = new Enemy
            {
                EnemyId = "Slime",
                MaxHealth = 20,
                Position = pos
            };
            // 事件 → 任务推进：受伤只累计 Damage 任务，死亡只累计 Kill 任务（按类型过滤，互不串线）
            enemy.Damaged += (id, amount) => _manager.ReportDamage(id, amount);
            enemy.Died += id =>
            {
                _manager.ReportKill(id, 1);
                GD.Print("[战斗] 史莱姆被击杀");
            };
            _enemies.Add(enemy);
            AddChild(enemy);
        }

        _gate = new LocationTrigger
        {
            TriggerId = "TownGate",
            DisplayName = "城门",
            Position = new Vector2(1020, 320)
        };
        _gate.Triggered += triggerId => _manager.EnterTrigger(triggerId);
        AddChild(_gate);
    }

    #region 截图导演（仅 --shot 模式使用，与正常游戏逻辑无关）

    private async void RunShotDirector(string shot)
    {
        const int W = 1920, H = 1080;
        GetWindow().Size = new Vector2I(W, H);

        string dir = ProjectSettings.GlobalizePath("res://Screenshots");
        DirAccess.MakeDirRecursiveAbsolute(dir);

        // 等窗口尺寸生效 + 首帧渲染稳定
        await ToSignal(GetTree().CreateTimer(0.4), "timeout");

        switch (shot)
        {
            case "overview":
                AddShotCamera(new Vector2(430, 330), 1.1f);
                await WaitFrames(10);
                break;

            case "combat":
                AddShotCamera(new Vector2(500, 290), 1.0f);
                _player.GlobalPosition = new Vector2(410, 210);
                _enemies[0].TakeDamage(6); // 14/20
                _enemies[1].TakeDamage(6); // 14/20
                _enemies[0].TakeDamage(6); // 8/20（这只保持受击闪红）
                await ToSignal(GetTree().CreateTimer(0.04), "timeout");
                break;

            case "dialog":
                for (int i = 0; i < 5; i++) _manager.ReportCollect("Coin", 1);
                for (int i = 0; i < 3; i++) _manager.ReportCollect("Crystal", 1);
                await ToSignal(GetTree().CreateTimer(0.4), "timeout");
                _dialog.Open(_questGiver, _manager);
                await ToSignal(GetTree().CreateTimer(0.4), "timeout");
                break;

            case "completion":
                AddShotCamera(new Vector2(430, 330), 1.1f);
                for (int i = 0; i < 5; i++) _manager.ReportCollect("Coin", 1);
                await ToSignal(GetTree().CreateTimer(0.5), "timeout"); // 绿字停留中（约2s后才淡出）
                break;

            case "gate":
                AddShotCamera(new Vector2(1000, 320), 1.0f);
                _player.GlobalPosition = new Vector2(1020, 320);
                await ToSignal(GetTree().CreateTimer(0.7), "timeout"); // 等 body_entered + 完成绿字
                break;

            default:
                GD.PrintErr($"[Shot] 未知画面: {shot}");
                GetTree().Quit();
                return;
        }

        CapturePng(System.IO.Path.Combine(dir, $"litequest_{shot}.png"));
        GetTree().Quit();
    }

    private async System.Threading.Tasks.Task WaitFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private void AddShotCamera(Vector2 pos, float zoom)
    {
        var cam = new Camera2D
        {
            Position = pos,
            Zoom = new Vector2(zoom, zoom),
            Enabled = true
        };
        AddChild(cam);
    }

    private void CapturePng(string absolutePath)
    {
        var image = GetViewport().GetTexture().GetImage();
        var err = image.SavePng(absolutePath);
        GD.Print(err == Error.Ok
            ? $"[Shot] 已保存: {absolutePath} ({image.GetWidth()}x{image.GetHeight()})"
            : $"[Shot] 保存失败: {err}");
    }

    #endregion

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;

        // 对话框打开时世界已暂停，游戏按键全部忽略（E/Esc 由对话框自己处理）
        if (_dialog.IsOpen) return;

        switch (key.Keycode)
        {
            case Key.C:
                _manager?.ReportCollect("Coin", 1);
                break;
            case Key.X:
                _manager?.ReportCollect("Crystal", 1);
                break;
            case Key.E:
                TryInteractNpc();
                break;
            case Key.J:
                AttackNearestEnemy();
                break;
            case Key.F2:
                QuestSaveSystem.Save(_manager.GetProgressSnapshot());
                GD.Print("[存档] 文件位置: " + QuestSaveSystem.GetAbsoluteSavePath());
                break;
            case Key.F3:
                LoadGame();
                break;
            case Key.F4:
                // 删档后不自动重载，需要重启游戏才是干净的新游戏流程
                QuestSaveSystem.DeleteSave();
                break;
        }
    }

    /// <summary>攻击玩家附近最近的存活史莱姆，演示 Kill / Damage 两类任务的事件源。</summary>
    private void AttackNearestEnemy()
    {
        Enemy nearest = null;
        float nearestDist = AttackRange;
        foreach (var enemy in _enemies)
        {
            if (enemy.IsDead) continue;
            float d = enemy.GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = enemy;
            }
        }

        if (nearest == null)
        {
            GD.Print("[战斗] 附近没有可攻击的敌人（靠近史莱姆后再按 J）");
            return;
        }
        nearest.TakeDamage(AttackDamage);
    }

    /// <summary>读档：快照恢复 + UI 重建。游戏进行中按 F3 可随时回到上次存档点。</summary>
    private void LoadGame()
    {
        var save = QuestSaveSystem.Load();
        if (save == null)
        {
            GD.Print("[读档] 没有存档文件");
            return;
        }
        _manager.LoadFromSnapshot(save.Quests);
        _tracker.RebuildFromManager();
        _manager.StartAutoStartQuests();
        GD.Print("[读档] 任务状态已恢复");
    }

    private void TryInteractNpc()
    {
        if (_questGiver == null || _dialog == null) return;

        float dist = _questGiver.GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (dist > NpcInteractRange)
        {
            GD.Print("[NPC] 附近没有可对话的 NPC（靠近商人后再按 E）");
            return;
        }
        _dialog.Open(_questGiver, _manager);
    }

    /// <summary>屏幕底部按键说明（独立 CanvasLayer，不影响追踪栏）。</summary>
    private void BuildHelpOverlay()
    {
        var layer = new CanvasLayer { Layer = 9 };
        AddChild(layer);

        var label = new Label
        {
            Text = "WASD/方向键 移动    J 攻击史莱姆    E 与商人对话    C 拾硬币    X 拾水晶    |    F2 存档    F3 读档    F4 删档"
        };
        label.AddThemeFontSizeOverride("font_size", 15);
        label.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.85f));
        layer.AddChild(label);

        label.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        label.OffsetTop = -48;
        label.OffsetBottom = -16;
        label.OffsetLeft = 20;
        label.OffsetRight = -20;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
    }
}
