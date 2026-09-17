using System;
using Godot;

namespace LiteQuest.QuestSystem.Gameplay;

/// <summary>
/// 位置触发器：玩家（PhysicsBody2D）进入区域时抛一次 Triggered。
///
/// 接线（Main 里）：
///   trigger.Triggered += triggerId => questManager.EnterTrigger(triggerId);
/// ReachLocation 任务的 TargetId 与 TriggerId 一致、TargetCount 固定为 1。
/// </summary>
public partial class LocationTrigger : Area2D
{
    /// <summary>对应 QuestDefinition.TargetId（如 "TownGate"）。</summary>
    [Export] public string TriggerId { get; set; } = "TownGate";

    [Export] public string DisplayName { get; set; } = "Town Gate";

    /// <summary>进入触发区（每个触发器只触发一次）。</summary>
    public event Action<string> Triggered;

    private bool _fired;
    private Polygon2D _zone;

    public override void _Ready()
    {
        // 半透明黄色方形区域（80×80）
        _zone = new Polygon2D
        {
            Color = new Color(1.0f, 0.85f, 0.3f, 0.35f),
            Polygon = new[]
            {
                new Vector2(-40, -40),
                new Vector2(40, -40),
                new Vector2(40, 40),
                new Vector2(-40, 40)
            }
        };
        AddChild(_zone);

        var shape = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = new Vector2(80, 80) }
        };
        AddChild(shape);

        var label = new Label { Text = DisplayName };
        label.Position = new Vector2(-40, -70);
        label.AddThemeFontSizeOverride("font_size", 14);
        label.AddThemeColorOverride("font_color", new Color(0.25f, 0.2f, 0.05f));
        AddChild(label);

        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_fired) return;
        _fired = true;

        // 视觉反馈：变绿
        if (_zone != null)
            _zone.Color = new Color(0.4f, 0.9f, 0.45f, 0.45f);

        GD.Print($"[Trigger] 进入区域: {TriggerId}");
        Triggered?.Invoke(TriggerId);
    }
}
