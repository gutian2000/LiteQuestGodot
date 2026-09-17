using System;
using Godot;

namespace LiteQuest.QuestSystem.Gameplay;

/// <summary>
/// 最简敌人：有 EnemyId 和生命值，受伤/死亡时抛事件。
///
/// 与任务系统的接线（在 Main 里完成，保持解耦）：
///   enemy.Damaged += (id, amount) => questManager.ReportDamage(id, amount);  // Damage 任务累计伤害
///   enemy.Died    += id          => questManager.ReportKill(id, 1);         // Kill   任务累计击杀
///
/// 注意：敌人死亡那一下的伤害也会计入 Damage 任务（先 Damaged 后 Died），符合直觉。
/// </summary>
public partial class Enemy : Node2D
{
    /// <summary>对应 QuestDefinition.TargetId（如 "Slime"）。</summary>
    [Export] public string EnemyId { get; set; } = "Slime";

    [Export] public int MaxHealth { get; set; } = 20;

    /// <summary>受伤：参数为敌人 id 与本次伤害值。</summary>
    public event Action<string, int> Damaged;

    /// <summary>死亡：参数为敌人 id（每个敌人只触发一次）。</summary>
    public event Action<string> Died;

    public bool IsDead { get; private set; }
    public int CurrentHealth { get; private set; }

    private Polygon2D _body;
    private Label _hpLabel;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        BuildVisual();
        RefreshLabel();
    }

    private void BuildVisual()
    {
        // 40×40 绿色方块代表史莱姆
        _body = new Polygon2D
        {
            Color = new Color(0.35f, 0.8f, 0.4f),
            Polygon = new[]
            {
                new Vector2(-20, -20),
                new Vector2(20, -20),
                new Vector2(20, 20),
                new Vector2(-20, 20)
            }
        };
        AddChild(_body);

        _hpLabel = new Label();
        _hpLabel.Position = new Vector2(-34, -52);
        _hpLabel.AddThemeFontSizeOverride("font_size", 13);
        _hpLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.2f, 0.2f));
        AddChild(_hpLabel);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        RefreshLabel();
        FlashHit();

        Damaged?.Invoke(EnemyId, amount);

        if (CurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        IsDead = true;
        _body.Color = new Color(0.45f, 0.45f, 0.45f); // 变灰表示尸体
        Died?.Invoke(EnemyId);

        // 短暂停留后淡出移除，给玩家"击杀反馈"时间
        var tween = CreateTween();
        tween.TweenInterval(0.8);
        tween.TweenProperty(this, "modulate:a", 0.0f, 0.5);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    private void FlashHit()
    {
        if (IsDead) return;
        _body.Color = new Color(0.95f, 0.35f, 0.3f);
        var tween = CreateTween();
        tween.TweenInterval(0.08);
        tween.TweenCallback(Callable.From(() =>
        {
            if (!IsDead && GodotObject.IsInstanceValid(_body))
                _body.Color = new Color(0.35f, 0.8f, 0.4f);
        }));
    }

    private void RefreshLabel()
    {
        if (_hpLabel != null)
            _hpLabel.Text = $"{EnemyId}  {CurrentHealth}/{MaxHealth}";
    }
}
