# LiteQuest System — Modular Quest Framework for Godot 4.7 (C#)

A code-driven quest framework for Godot **4.7+ with .NET / C#**. Define quests as `.tres` resources, drive them at runtime with an event-based `QuestManager`, and edit quest chains through a custom inspector plugin. A fully playable 2D demo scene shows every integration point.

---

## Features

- **5 objective types** — `Collect`, `Kill`, `Damage`, `ReachLocation`, `Custom`
- **Quest chains** — prerequisite gating (all prerequisites must be `Completed` before a quest can start) + optional auto-start of follow-up quests
- **Event-driven runtime** — C# events for quest started / progress changed / completed / rewards granted; your systems stay fully decoupled
- **Gold / EXP rewards** — delivered as events, so the quest system never depends on your wallet/XP implementation
- **JSON save / load** — versioned save schema in `user://`, enum-as-string for readable files, forward-compatible merge (new quests added in updates get sensible defaults), corrupt-save fallback to a new game
- **Quest tracker HUD** — runtime quest list with live `(current/target)` progress and completion fade-out
- **Modal quest-giver dialog** — proximity interaction, game pause, four state branches (prerequisites not met / available / in progress / completed message)
- **Editor plugin** — prerequisite quests become a checkbox list of every quest in the project; contextual hints and validation warnings; full Undo/Redo support

## Requirements

- Godot Engine **4.7+**, **.NET (mono) build** — C# is required; GDScript-only projects cannot use this asset
- A project using the .NET SDK (`Godot.NET.Sdk/4.7.x`)

## Installation

1. Copy the `Scripts/`, `Demo/`, `Assets/`, and `addons/quest_inspector/` folders into your project root.
2. Let Godot import the files and build the C# solution once (the editor prompts automatically).
3. Enable the plugin: **Project → Project Settings → Plugins → QuestInspector → Enable**.

## Quick Start (3 minutes)

### 1. Create a quest resource

In the FileSystem dock, right-click → **New Resource → QuestDefinition**, save it as `MyQuest.tres`, then fill the exported fields. Or copy one of the examples in `Assets/Quests/`.

Key fields: `QuestName`, `Description`, `CompletionMessage`, `ObjectiveType`, `TargetId`, `TargetCount`, `AutoStart`, `PrerequisiteQuests`, `GoldReward`, `ExpReward`.

### 2. Add a QuestManager to your scene

Add it as a child node (or register it as an Autoload singleton). When quests complete, subscribe to reward events:

```csharp
var manager = new QuestManager();
AddChild(manager);

var quest = GD.Load<QuestDefinition>("res://Assets/Quests/MyQuest.tres");
manager.RegisterAll(quest);
manager.StartAutoStartQuests();

manager.OnRewardsGranted += (q, gold, exp) =>
{
    MyWallet.AddGold(gold);   // your own systems
    MyPlayer.AddExp(exp);
};
```

### 3. Report game events as progress

Call the matching report method from wherever your gameplay happens — pickup, enemy death, damage numbers, trigger areas:

```csharp
// Collect  → on item pickup
questManager.ReportCollect("Coin", 1);

// Kill     → in the enemy Died event
questManager.ReportKill("Slime", 1);

// Damage   → in the enemy Damaged event (pass the damage amount)
questManager.ReportDamage("Slime", damage);

// ReachLocation → from an Area2D body_entered callback
questManager.EnterTrigger("TownGate");

// Custom   → complete manually for bespoke objectives
questManager.CompleteQuest(myCustomQuest);
```

`TargetId` on the quest must match the id string you pass here. Each report method only advances quests of the matching objective type, so a `Damaged` event can never inflate a Kill quest (and vice versa). All in-progress quests matching that id and type advance simultaneously; quests that reach `TargetCount` auto-complete.

### 4. Show it to the player

Add the tracker HUD and (optionally) the quest-giver dialog:

```csharp
AddChild(new QuestTrackerUI());   // subscribes to QuestManager.Instance automatically
```

For NPC interaction, see `Demo/DemoController.cs` — it spawns a `QuestGiverNPC`, opens `QuestDialogUI` when the player presses E within range, and the dialog itself handles pause and state-specific content.

## Save / Load

```csharp
// Save
QuestSaveSystem.Save(questManager.GetProgressSnapshot());

// Load — order matters:
// 1. RegisterAll(...) your quest definitions first
// 2. restore the snapshot
// 3. rebuild any UI from the restored state
var data = QuestSaveSystem.Load();          // null when no save exists
if (data != null)
{
    questManager.LoadFromSnapshot(data.Quests);
    tracker.RebuildFromManager();
    questManager.StartAutoStartQuests();   // activates auto-start quests added after the save
}
```

Saves live at `user://quest_save.json` (`QuestSaveSystem.GetAbsoluteSavePath()` gives the OS path). `DeleteSave()` is provided for debugging / new-game flows.

## Demo Scene

Open and run `Demo/DemoScene.tscn` (it is configured as the project's main scene).

| Key | Action |
|---|---|
| WASD / Arrow keys | Move the player |
| J | Attack nearest slime in range (6 damage, 130px range) |
| E | Talk to the merchant when nearby (150px) |
| C / X | Simulate picking up a coin / crystal |
| F2 / F3 / F4 | Save / load / delete save |

The demo exercises all 6 included quests: two auto-start collect chains, a quest-gated NPC chain, Kill and Damage quests driven by one enemy's events, and a ReachLocation gate at the east edge of the map.

## Project Structure

```
Scripts/
├── QuestDefinition.cs         # Resource: quest data (.tres)
├── CollectibleDefinition.cs   # Resource: collectible item data
├── QuestObjectiveType.cs      # Enum: Collect/Kill/Damage/ReachLocation/Custom
├── QuestStatus.cs             # Enum: NotStarted/InProgress/Completed
├── QuestProgress.cs           # Runtime snapshot record (serialization-friendly)
├── QuestManager.cs            # Runtime core: register/start/progress/complete, events, snapshots
├── Gameplay/
│   ├── Enemy.cs               # Example enemy with Damaged/Died events
│   ├── DemoPlayer.cs          # Example moving player (CharacterBody2D)
│   ├── LocationTrigger.cs     # Example Area2D one-shot trigger
│   └── QuestGiverNPC.cs       # Example quest NPC
├── UI/
│   ├── QuestTrackerUI.cs      # HUD tracker panel
│   └── QuestDialogUI.cs       # Modal quest-giver dialog
└── Save/
    └── QuestSaveSystem.cs     # JSON persistence to user://
Demo/
├── DemoController.cs          # Demo bootstrap (study this for integration)
└── DemoScene.tscn
Assets/Quests/                 # 6 example quest resources
Assets/Collectibles/           # 2 example collectible resources
addons/quest_inspector/        # Editor plugin (custom inspector + icon)
```

## Runtime API Overview

`QuestManager` (static `Instance` available after `_Ready`):

| Member | Description |
|---|---|
| `RegisterAll(params QuestDefinition[])` | Register quest resources |
| `StartAutoStartQuests()` | Start every `AutoStart` quest whose prerequisites are met |
| `CanStart(quest)` / `StartQuest(quest)` | Prerequisite-gated manual start |
| `ReportCollect(targetId, n=1)` / `ReportKill(targetId, n=1)` | Advance Collect / Kill quests (per-event count) |
| `ReportDamage(targetId, damage)` | Advance Damage quests (cumulative damage) |
| `EnterTrigger(triggerId)` | Advance a ReachLocation quest (one-shot) |
| `CompleteQuest(quest)` | Manual completion (Custom objectives / scripts) |
| `GetStatus` / `GetCurrentProgress` / `IsCompleted` / `IsInProgress` | Queries |
| `GetProgressSnapshot()` / `LoadFromSnapshot(list)` | Save/load boundary |
| `OnQuestStarted` / `OnProgressChanged` / `OnQuestCompleted` / `OnRewardsGranted` | Events |

## Notes

- The editor plugin uses script-path + `Variant` introspection internally (a workaround for a Godot 4.7 mono editor-process type-wrapping quirk); game/runtime code uses normal strong typing — this is fully transparent to users of the asset.
- UI and demo visuals are intentionally simple primitives (colored polygons) so the framework contains no art dependencies.
- The inspector plugin is optional: everything works without it, you just lose the checkbox/validation UX.

## License

See the licensing terms on the asset store page where you purchased this package. Example quests and demo code may be used freely in your own projects.

---

# LiteQuest System —— Godot 4.7 (C#) 模块化任务框架

面向 **Godot 4.7+ .NET（mono）版本** 的代码驱动任务框架。任务以 `.tres` 资源定义，运行时由事件驱动的 `QuestManager` 推进，并附带一个自定义检查器插件方便编辑任务链。可玩的 2D 演示场景覆盖全部接入点。

## 功能特性

- **5 种目标类型**：收集、击杀、伤害累计、到达位置、自定义
- **任务链**：前置闸门（前置任务全部完成才能接取）+ 后续任务可选自动接取
- **事件驱动运行时**：任务开始 / 进度变化 / 完成 / 奖励发放均为 C# 事件，与你的业务系统完全解耦
- **金币 / 经验奖励**：以事件形式下发，任务系统不依赖你的钱包和经验实现
- **JSON 存档/读档**：带版本号的存档结构、枚举存字符串可读、向前兼容合并（更新后新增的任务自动补默认状态）、损坏存档自动回退新游戏
- **任务追踪 HUD**：实时显示 `(当前/目标)`，完成后打钩淡出
- **任务发布者弹窗**：靠近交互、暂停世界、四种状态分支（前置未满足 / 可接取 / 进行中 / 完成寄语）
- **编辑器插件**：前置任务变为项目内任务复选框列表、上下文提示与校验警告、完整撤销/重做

## 环境要求

- Godot Engine **4.7+ 的 .NET（mono）版本**，必须使用 C#
- 项目已启用 .NET SDK（`Godot.NET.Sdk/4.7.x`）

## 安装

1. 将 `Scripts/`、`Demo/`、`Assets/`、`addons/quest_inspector/` 复制到项目根目录。
2. 等待 Godot 导入并编译 C# 解决方案。
3. 启用插件：**项目 → 项目设置 → 插件 → QuestInspector → 启用**。

## 快速上手（3 分钟）

**1. 创建任务资源**：文件系统面板右键 → 新建资源 → `QuestDefinition`，保存为 `.tres` 后在检查器填写字段（或直接参考 `Assets/Quests/` 下的示例）。

**2. 场景中加入 QuestManager**（作为子节点或 AutoLoad 单例）：

```csharp
var manager = new QuestManager();
AddChild(manager);
manager.RegisterAll(GD.Load<QuestDefinition>("res://Assets/Quests/MyQuest.tres"));
manager.StartAutoStartQuests();

manager.OnRewardsGranted += (q, gold, exp) => { /* 发放给你的钱包/经验系统 */ };
```

**3. 在游戏事件里上报进度**：

```csharp
manager.ReportCollect("Coin", 1);   // 拾取
manager.ReportKill("Slime", 1);     // 击杀
manager.ReportDamage("Slime", dmg); // 伤害（传伤害值）
manager.EnterTrigger("TownGate");   // 到达位置
manager.CompleteQuest(customQuest); // 自定义目标手动完成
```

任务的 `TargetId` 必须与上报的 id 一致；匹配的进行中任务会同时推进，达到 `TargetCount` 自动完成。

**4. 加入界面**：`AddChild(new QuestTrackerUI());` 即可显示追踪栏；NPC 对话的完整接法见 `Demo/DemoController.cs`。

## 存档/读档

```csharp
QuestSaveSystem.Save(manager.GetProgressSnapshot());

var data = QuestSaveSystem.Load();   // 无存档返回 null
if (data != null)
{
    manager.LoadFromSnapshot(data.Quests); // 顺序：先 RegisterAll，再恢复快照
    tracker.RebuildFromManager();
    manager.StartAutoStartQuests();
}
```

存档位于 `user://quest_save.json`，可用 `QuestSaveSystem.GetAbsoluteSavePath()` 查看系统绝对路径；`QuestSaveSystem.DeleteSave()` 用于删档调试。

## 演示场景操作

运行 `Demo/DemoScene.tscn`（已设为主场景）：**WASD/方向键**移动、**J** 攻击附近史莱姆、**E** 与商人对话、**C/X** 模拟拾取硬币/水晶、**F2/F3/F4** 存档/读档/删档。

6 个示例任务覆盖：两条自动开始的收集链、一条 NPC 前置任务链、由同一敌人事件驱动的击杀与伤害任务、地图东侧的到达位置任务。

## 目录结构与运行时 API

见上方英文版 **Project Structure** 与 **Runtime API Overview** 两节（类名、事件名在中英文环境中完全一致）。

## 说明

- 编辑器插件内部使用脚本路径 + `Variant` 自省（规避 Godot 4.7 mono 编辑器进程的类型包装问题）；游戏运行时代码全部为正常强类型，对使用者透明。
- UI 与演示视觉刻意使用纯色多边形，素材零美术依赖。
- 检查器插件为可选项：禁用后功能不受影响，仅失去复选框与校验提示体验。
