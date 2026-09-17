using System.Collections.Generic;
using Godot;

namespace LiteQuest.QuestSystem.UI;

/// <summary>
/// 屏幕右上角的任务追踪栏。纯代码构建 Control 树（无需手动编辑 .tscn）。
///
/// 订阅 QuestManager 的三个事件实时刷新：
///   OnQuestStarted    → 新增一行 "○ 任务名 (0/目标)"
///   OnProgressChanged → 更新 "(当前/目标)"
///   OnQuestCompleted  → 变 "✓ 任务名 +奖励"，停留后淡出移除
/// </summary>
public partial class QuestTrackerUI : CanvasLayer
{
    private VBoxContainer _rows;
    private Label _emptyLabel;
    private readonly Dictionary<QuestDefinition, Label> _rowByQuest = new();
    // 完成淡出动画。读档重建 UI 时要先 Kill，否则动画回调会操作已释放的 Label
    private readonly List<Tween> _activeTweens = new();

    public override void _Ready()
    {
        Layer = 10;
        BuildPanel();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
            QuestManager.Instance.OnProgressChanged += HandleProgressChanged;
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
            QuestManager.Instance.OnRewardsGranted += HandleRewardsGranted;
        }
        else
        {
            GD.PrintErr("[QuestTrackerUI] QuestManager.Instance 为空，请先把 QuestManager 加入场景");
        }

        UpdateEmptyHint();
    }

    public override void _ExitTree()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
        QuestManager.Instance.OnProgressChanged -= HandleProgressChanged;
        QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        QuestManager.Instance.OnRewardsGranted -= HandleRewardsGranted;
    }

    private void BuildPanel()
    {
        // 半透明深色背景面板，锚定右上角，固定矩形
        var panel = new PanelContainer();
        AddChild(panel);

        var bg = new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.05f, 0.08f, 0.72f),
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = 14,
            ContentMarginRight = 14,
            ContentMarginTop = 10,
            ContentMarginBottom = 10
        };
        panel.AddThemeStyleboxOverride("panel", bg);

        panel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
        panel.OffsetLeft = -350;
        panel.OffsetTop = 16;
        panel.OffsetRight = -16;
        panel.OffsetBottom = 460;

        var vbox = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        panel.AddChild(vbox);

        var title = new Label
        {
            Text = "任务追踪",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 18);
        title.AddThemeColorOverride("font_color", new Color(1.0f, 0.92f, 0.55f));
        vbox.AddChild(title);

        var sep = new HSeparator();
        vbox.AddChild(sep);

        _rows = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        vbox.AddChild(_rows);

        _emptyLabel = new Label
        {
            Text = "（暂无进行中的任务）",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _emptyLabel.AddThemeFontSizeOverride("font_size", 13);
        _emptyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        _rows.AddChild(_emptyLabel);
    }

    private void HandleQuestStarted(QuestDefinition quest)
    {
        if (_rowByQuest.ContainsKey(quest)) return;
        CreateRow(quest, 0);
        UpdateEmptyHint();
    }

    private Label CreateRow(QuestDefinition quest, int current)
    {
        var label = new Label();
        label.AddThemeFontSizeOverride("font_size", 15);
        label.AddThemeColorOverride("font_color", new Color(0.92f, 0.92f, 0.92f));
        SetRowText(label, quest, current);
        _rows.AddChild(label);
        _rowByQuest[quest] = label;
        return label;
    }

    /// <summary>
    /// 读档后按 QuestManager 当前状态重建整个列表。
    /// 已完成的任务不重新显示（否则每次读档都播一遍完成动画）；进行中的任务补行并显示真实进度。
    /// </summary>
    public void RebuildFromManager()
    {
        var manager = QuestManager.Instance;
        if (manager == null) return;

        // 停掉所有完成淡出动画，避免回调操作即将释放的节点
        foreach (var t in _activeTweens)
        {
            if (GodotObject.IsInstanceValid(t)) t.Kill();
        }
        _activeTweens.Clear();

        // 清空旧行（保留 _emptyLabel）
        foreach (var child in _rows.GetChildren())
        {
            if (child == _emptyLabel) continue;
            child.QueueFree();
        }
        _rowByQuest.Clear();

        // 只恢复"进行中"的任务；NotStarted 等启动事件、Completed 不回显
        foreach (var quest in manager.GetAllDefinitions())
        {
            if (manager.GetStatus(quest) == QuestStatus.InProgress)
                CreateRow(quest, manager.GetCurrentProgress(quest));
        }

        UpdateEmptyHint();
    }

    private void HandleProgressChanged(QuestDefinition quest, int oldCurrent, int newCurrent)
    {
        if (_rowByQuest.TryGetValue(quest, out var label))
            SetRowText(label, quest, newCurrent);
    }

    private void HandleRewardsGranted(QuestDefinition quest, int gold, int exp)
    {
        if (_rowByQuest.TryGetValue(quest, out var label))
            label.Text = $"✓ {quest.QuestName}   奖励 +{gold} 金币 / +{exp} 经验";
    }

    private void HandleQuestCompleted(QuestDefinition quest)
    {
        if (!_rowByQuest.TryGetValue(quest, out var label)) return;

        label.AddThemeColorOverride("font_color", new Color(0.45f, 0.9f, 0.5f));
        // 若奖励事件先到会已写奖励文本；否则给一个完成兜底文案
        if (!label.Text.StartsWith("✓"))
            label.Text = $"✓ {quest.QuestName}";

        _rowByQuest.Remove(quest);

        // 停留约 2 秒后淡出并移除
        var tween = CreateTween();
        _activeTweens.Add(tween);
        tween.TweenInterval(2.0);
        tween.TweenProperty(label, "modulate:a", 0.0f, 0.6);
        tween.TweenCallback(Callable.From(() =>
        {
            _activeTweens.Remove(tween);
            if (GodotObject.IsInstanceValid(label)) label.QueueFree();
            UpdateEmptyHint();
        }));
    }

    private static void SetRowText(Label label, QuestDefinition quest, int current)
    {
        label.Text = $"○ {quest.QuestName}  ({current}/{quest.TargetCount})";
    }

    private void UpdateEmptyHint()
    {
        if (_emptyLabel != null)
            _emptyLabel.Visible = _rowByQuest.Count == 0;
    }
}
