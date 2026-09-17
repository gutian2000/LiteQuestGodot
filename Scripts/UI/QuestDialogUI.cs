using Godot;
using LiteQuest.QuestSystem.Gameplay;

namespace LiteQuest.QuestSystem.UI;

/// <summary>
/// NPC 任务对话框。打开时暂停游戏世界，按任务当前状态显示不同内容：
///   NotStarted + 前置满足 → 任务描述 + 奖励预览 + [接受任务]
///   NotStarted + 前置未满足 → 列出未完成的前置任务
///   InProgress            → 描述 + 当前进度
///   Completed             → CompletionMessage（完成寄语）
/// E / Esc / 按钮均可关闭。ProcessMode.Always 保证暂停时 UI 仍能收输入。
/// </summary>
public partial class QuestDialogUI : CanvasLayer
{
    private QuestManager _manager;
    private QuestGiverNPC _npc;

    private ColorRect _dim;
    private PanelContainer _panel;
    private Label _titleLabel;
    private Label _bodyLabel;
    private Label _metaLabel;
    private HBoxContainer _buttonRow;

    public bool IsOpen { get; private set; }

    public override void _Ready()
    {
        Layer = 12;
        ProcessMode = ProcessModeEnum.Always; // 游戏暂停时对话框仍响应
        BuildLayout();
        Visible = false;
    }

    private void BuildLayout()
    {
        // 半透明遮罩，吃掉背后的鼠标
        _dim = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.55f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        _dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_dim);

        var center = new CenterContainer();
        center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(center);

        _panel = new PanelContainer();
        _panel.CustomMinimumSize = new Vector2(520, 0);
        var bg = new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.08f, 0.12f, 0.97f),
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10,
            ContentMarginLeft = 22,
            ContentMarginRight = 22,
            ContentMarginTop = 18,
            ContentMarginBottom = 18
        };
        _panel.AddThemeStyleboxOverride("panel", bg);
        center.AddChild(_panel);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 12);
        _panel.AddChild(vbox);

        _titleLabel = new Label();
        _titleLabel.AddThemeFontSizeOverride("font_size", 20);
        _titleLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.92f, 0.55f));
        vbox.AddChild(_titleLabel);

        var sep = new HSeparator();
        vbox.AddChild(sep);

        _bodyLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(470, 0)
        };
        _bodyLabel.AddThemeFontSizeOverride("font_size", 15);
        vbox.AddChild(_bodyLabel);

        _metaLabel = new Label();
        _metaLabel.AddThemeFontSizeOverride("font_size", 14);
        _metaLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.85f, 1.0f));
        vbox.AddChild(_metaLabel);

        _buttonRow = new HBoxContainer();
        _buttonRow.Alignment = BoxContainer.AlignmentMode.End;
        _buttonRow.AddThemeConstantOverride("separation", 10);
        vbox.AddChild(_buttonRow);
    }

    public void Open(QuestGiverNPC npc, QuestManager manager)
    {
        _npc = npc;
        _manager = manager;
        IsOpen = true;
        Visible = true;
        GetTree().Paused = true;
        Rebuild();
    }

    public void Close()
    {
        IsOpen = false;
        Visible = false;
        if (GetTree() != null)
            GetTree().Paused = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsOpen) return;
        if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;
        if (key.Keycode is Key.Escape or Key.E)
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Rebuild()
    {
        // 清空旧按钮
        foreach (var child in _buttonRow.GetChildren())
            child.QueueFree();

        var quest = _npc.Quest;
        _titleLabel.Text = _npc.NpcName;

        var status = _manager.GetStatus(quest);

        switch (status)
        {
            case QuestStatus.Completed:
                _bodyLabel.Text = quest.CompletionMessage;
                _metaLabel.Text = $"任务「{quest.QuestName}」已完成";
                AddLeaveButton("再见");
                break;

            case QuestStatus.InProgress:
                _bodyLabel.Text = quest.Description;
                _metaLabel.Text = $"任务进行中：{_manager.GetCurrentProgress(quest)}/{quest.TargetCount}";
                AddLeaveButton("稍后再来");
                break;

            default: // NotStarted
                if (_manager.CanStart(quest))
                {
                    _bodyLabel.Text = quest.Description;
                    _metaLabel.Text = $"奖励：{quest.GoldReward} 金币 / {quest.ExpReward} 经验";
                    AddActionButton("接受任务", new Color(0.3f, 0.7f, 0.4f), () =>
                    {
                        if (_manager.StartQuest(quest))
                            Rebuild(); // 接取后刷新为"进行中"视图
                    });
                    AddLeaveButton("拒绝");
                }
                else
                {
                    _bodyLabel.Text = "「你现在还接不了这个任务，先证明你的实力吧。」";
                    var names = new System.Text.StringBuilder();
                    foreach (var pre in quest.GetPrerequisiteQuests())
                    {
                        if (!_manager.IsCompleted(pre))
                            names.Append("  · ").Append(pre.QuestName).Append('\n');
                    }
                    _metaLabel.Text = "需先完成：\n" + names;
                    AddLeaveButton("离开");
                }
                break;
        }
    }

    private Button MakeButton(string text, Color color)
    {
        var btn = new Button { Text = text, CustomMinimumSize = new Vector2(120, 36) };
        var normal = new StyleBoxFlat
        {
            BgColor = color,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };
        btn.AddThemeStyleboxOverride("normal", normal);
        btn.AddThemeStyleboxOverride("hover", (StyleBoxFlat)normal.Duplicate());
        return btn;
    }

    private void AddActionButton(string text, Color color, System.Action onClick)
    {
        var btn = MakeButton(text, color);
        btn.Pressed += () => onClick();
        _buttonRow.AddChild(btn);
    }

    private void AddLeaveButton(string text)
    {
        var btn = MakeButton(text, new Color(0.3f, 0.3f, 0.36f));
        btn.Pressed += Close;
        _buttonRow.AddChild(btn);
    }
}
