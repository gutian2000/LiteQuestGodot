using Godot;

namespace LiteQuest.QuestSystem.Gameplay;

/// <summary>
/// 任务发布 NPC（商人）。持有一个需要手动接取的 QuestDefinition（AutoStart=false）。
/// 玩家靠近后按交互键，由 Main 打开 QuestDialogUI。
/// </summary>
public partial class QuestGiverNPC : Node2D
{
    /// <summary>NPC 显示名。</summary>
    public string NpcName { get; set; } = "商人";

    /// <summary>该 NPC 发布的任务（运行时由 Main 从 .tres 加载后赋值）。</summary>
    public QuestDefinition Quest { get; set; }

    public override void _Ready()
    {
        // 紫色 36×36 方块代表商人
        var body = new Polygon2D
        {
            Color = new Color(0.65f, 0.4f, 0.95f),
            Polygon = new[]
            {
                new Vector2(-18, -18),
                new Vector2(18, -18),
                new Vector2(18, 18),
                new Vector2(-18, 18)
            }
        };
        AddChild(body);

        var nameLabel = new Label { Text = NpcName };
        nameLabel.Position = new Vector2(-24, -46);
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.9f, 1.0f));
        AddChild(nameLabel);

        var hint = new Label { Text = "[E] 对话" };
        hint.Position = new Vector2(-28, 22);
        hint.AddThemeFontSizeOverride("font_size", 12);
        hint.AddThemeColorOverride("font_color", new Color(1.0f, 0.95f, 0.5f));
        AddChild(hint);
    }
}
