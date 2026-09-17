using Godot;

namespace LiteQuest.QuestSystem.Gameplay;

/// <summary>
/// 演示用玩家：方向键 / WASD 移动的蓝色方块（CharacterBody2D）。
/// 必须是 PhysicsBody2D 才能触发 LocationTrigger（Area2D.BodyEntered）。
/// 相机挂在玩家身上跟随移动。
/// </summary>
public partial class DemoPlayer : CharacterBody2D
{
    [Export] public float MoveSpeed { get; set; } = 260f;

    public override void _Ready()
    {
        // 蓝色方块（32×32）
        var body = new Polygon2D
        {
            Color = new Color(0.3f, 0.55f, 1.0f),
            Polygon = new[]
            {
                new Vector2(-16, -16),
                new Vector2(16, -16),
                new Vector2(16, 16),
                new Vector2(-16, 16)
            }
        };
        AddChild(body);

        var collision = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = new Vector2(32, 32) }
        };
        AddChild(collision);

        var nameLabel = new Label { Text = "玩家" };
        nameLabel.Position = new Vector2(-20, -42);
        nameLabel.AddThemeFontSizeOverride("font_size", 13);
        AddChild(nameLabel);

        var camera = new Camera2D { Enabled = true };
        AddChild(camera);
    }

    public override void _PhysicsProcess(double delta)
    {
        // 裸键轮询，无需在 project.godot 配置 InputMap
        float x = (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right) ? 1f : 0f)
                - (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left) ? 1f : 0f);
        float y = (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down) ? 1f : 0f)
                - (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up) ? 1f : 0f);
        var direction = new Vector2(x, y).Normalized();

        Velocity = direction * MoveSpeed;
        MoveAndSlide();
    }
}
