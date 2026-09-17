# LiteQuest — 模块化任务系统 (Godot 4.7 / C#)

> itch.io 上架页文案，复制粘贴到项目后台

---

## 标题（Page Title）

**LiteQuest — Modular Quest Framework for Godot 4.7 / C#**

## 短描述（One-Sentence Summary）

A code-driven quest framework for Godot 4.7+ with .NET / C#. 5 objective types, quest chains, event-driven runtime, JSON save/load, and a custom inspector plugin with validation hints.

## 标签（Tags）

`godot` `godot-4` `csharp` `quest-system` `rpg` `framework` `tool` `utility`

## 封面图（Cover Art）

使用 `Screenshots/litequest_overview.png`（1920×1080）

## 分类

- Engine Compatibility: **Godot 4.x**
- Godot Version: **4.7**
- Programming Language: **C# / .NET**
- License: **MIT**

---

## 长描述（Long Description）

LiteQuest is a lightweight, fully decoupled quest system for **Godot 4.7+ with .NET / C#**. Define quests as `.tres` resources, drive them at runtime through an event-based `QuestManager`, and edit quest chains visually with a custom inspector plugin — complete with live validation hints and full Undo/Redo support.

### ✨ Features

**5 Objective Types**
- `Collect` — item pickup counting
- `Kill` — enemy death counting
- `Damage` — cumulative damage thresholds
- `ReachLocation` — Area2D trigger one-shots
- `Custom` — manually completed bespoke objectives

**Quest Chains**
- Prerequisite gating — a quest can only start after all its prerequisites reach `Completed`
- Optional auto-start of follow-up quests the moment their prerequisite finishes

**Event-Driven Runtime**
- C# events for `OnQuestStarted`, `OnProgressChanged`, `OnQuestCompleted`, `OnRewardsGranted`
- Your wallet, XP, and UI systems subscribe — LiteQuest never depends on your implementations
- Type-safe report methods (`ReportCollect` / `ReportKill` / `ReportDamage` / `EnterTrigger`) filter by objective type, so a damage number can never inflate a Kill quest (and vice versa)

**JSON Save / Load**
- Versioned schema stored in `user://quest_save.json`
- Enums serialized as strings for human-readable files
- Forward-compatible merge — new quests added in updates get sensible `NotStarted` defaults on old saves
- Corrupt-save fallback to a fresh game

**Quest Tracker HUD + Modal Quest Dialog**
- Live `(current / target)` progress with completion fade-out animations
- Four-state quest dialog: prerequisites blocked / available / in progress / completed message
- Proximity NPC interaction with game pause

**Editor Plugin**
- Prerequisite quests become a checkbox list of every quest in your project
- Contextual 💡 hints change based on the selected objective type
- ⚠️ validation warnings flag misconfigurations (e.g. `ReachLocation` with `TargetCount ≠ 1`)
- Full UndoRedo support

### 📦 What's Included

```
LiteQuestGodot/
├── Scripts/          ← Reusable framework (data layer + runtime + save + UI)
├── Demo/             ← Fully playable 2D demo scene
├── Assets/           ← 6 example QuestDefinition .tres files
├── addons/
│   └── quest_inspector/  ← Custom inspector plugin
├── Screenshots/      ← 5x 1920×1080 store images
├── project.godot
└── README.md         ← Full API reference + integration guide
```

### 🎮 Demo Controls

| Key | Action |
|-----|--------|
| WASD | Move player |
| J | Attack nearest slime |
| C | Give yourself a Coin |
| X | Give yourself a Crystal |
| E | Talk to the merchant NPC |
| F2 | Save game |
| F3 | Load game |
| F4 | Delete save |

### 🔧 Requirements

- Godot Engine **4.7 or newer**, **.NET / C# build** (mono)
- A project using `Godot.NET.Sdk/4.7.x`
- **C# is required** — GDScript-only projects cannot use this asset

### 📖 Quick Start (3 Steps)

**1. Create a quest resource**
Right-click in FileSystem → **New Resource → QuestDefinition** → save as `MyQuest.tres`. Or copy one of the examples in `Assets/Quests/`.

**2. Wire up QuestManager**
```csharp
var manager = new QuestManager();
AddChild(manager);

manager.RegisterAll(GD.Load<QuestDefinition>("res://MyQuest.tres"));
manager.StartAutoStartQuests();

manager.OnRewardsGranted += (q, gold, exp) =>
{
    MyWallet.AddGold(gold);
    MyPlayer.AddExp(exp);
};
```

**3. Report events from your gameplay**
```csharp
// Enemy death
enemy.Died += id => questManager.ReportKill(id, 1);

// Item pickup
questManager.ReportCollect("Coin", 1);

// Area2D trigger
gate.BodyEntered += body => questManager.EnterTrigger("TownGate");
```

### 📜 License

MIT — do whatever you want, attribution appreciated but not required.

### 🐛 Bug Reports / Contributions

GitHub Issues and PRs welcome.

---

## 商店截图清单（Screenshots）

按 itch.io 推荐顺序排列：

1. **Cover / Hero**: `litequest_overview.png` — 1920×1080, 整体概览
2. **Gameplay**: `litequest_combat.png` — 战斗进行中，进度追踪栏可见
3. **Gameplay**: `litequest_dialog.png` — NPC 对话模态框
4. **Gameplay**: `litequest_completion.png` — 任务完成反馈（绿色字）
5. **Gameplay**: `litequest_gate.png` — 到达位置触发完成
6. **Editor**: 手动 Inspector 截图 — 💡 提示 + ⚠️ 校验
