using Godot;
using LiteQuest.QuestSystem;

namespace LiteQuest.QuestSystem.Editor;

/// <summary>
/// Objective 分类顶部的动态提示块：蓝色 💡 语义说明 + 红色 ⚠️ 校验警告。
///
/// 刷新机制：不能订阅 Resource.Changed（Inspector 编辑 int/enum 属性时该信号不会自动发出）。
/// 改为在进入场景树时向上找到所属 EditorInspector，连接其 property_edited 信号——
/// 用户在检查器提交任意属性后都会回调，按属性名过滤出 TargetCount / ObjectiveType 再刷新。
/// 控件离开树时断开，避免信号泄漏。
/// </summary>
[Tool]
public partial class QuestObjectiveHints : VBoxContainer
{
    private GodotObject _obj;
    private EditorInspector _inspector;
    private EditorInspector.PropertyEditedEventHandler _onPropertyEdited;
    private Label _hint;
    private Label _warn;

    public QuestObjectiveHints() { }

    public QuestObjectiveHints(GodotObject obj)
    {
        _obj = obj;

        _hint = MakeLabel(new Color(0.7f, 0.85f, 1.0f));
        _warn = MakeLabel(new Color(1.0f, 0.5f, 0.3f));
        AddChild(_hint);
        AddChild(_warn);

        Refresh();
    }

    public override void _EnterTree()
    {
        // AddCustomControl 把控件挂进 EditorInspector 的内部容器，向上按类型找到它
        Node p = GetParent();
        while (p != null)
        {
            if (p is EditorInspector ei)
            {
                _inspector = ei;
                // lambda 参数类型由委托推断（string 或 StringName 均兼容），用 ToString 比较
                _onPropertyEdited = property =>
                {
                    string n = property.ToString();
                    if (n == "TargetCount" || n == "ObjectiveType")
                        Refresh();
                };
                _inspector.PropertyEdited += _onPropertyEdited;
                break;
            }
            p = p.GetParent();
        }
    }

    public override void _ExitTree()
    {
        if (_inspector != null)
        {
            if (_onPropertyEdited != null)
                _inspector.PropertyEdited -= _onPropertyEdited;
            _inspector = null;
        }
        base._ExitTree();
    }

    private void Refresh()
    {
        if (_obj == null) return;

        int objectiveType = _obj.Get("ObjectiveType").AsInt32();
        int targetCount = _obj.Get("TargetCount").AsInt32();

        _hint.Text = "💡 " + (objectiveType switch
        {
            (int)QuestObjectiveType.Collect       => "Collect: TargetId 必须匹配某个 CollectibleDefinition.TargetId",
            (int)QuestObjectiveType.Kill          => "Kill: TargetId 填敌人 EnemyId（死亡时 ReportKill(id,1)）",
            (int)QuestObjectiveType.Damage        => "Damage: TargetId 填敌人 EnemyId（受伤时 ReportDamage(id,dmg)）",
            (int)QuestObjectiveType.ReachLocation => "ReachLocation: TargetCount 应固定为 1",
            (int)QuestObjectiveType.Custom        => "Custom: 需调用方手动 CompleteQuest",
            _ => ""
        });

        string warning = "";
        if (targetCount < 1)
            warning = "⚠️ TargetCount 必须 ≥ 1";
        else if (objectiveType == (int)QuestObjectiveType.ReachLocation && targetCount != 1)
            warning = "⚠️ ReachLocation 类型 TargetCount 应固定为 1";

        _warn.Text = warning;
        _warn.Visible = warning.Length > 0;
    }

    private static Label MakeLabel(Color color)
    {
        var label = new Label();
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }
}
