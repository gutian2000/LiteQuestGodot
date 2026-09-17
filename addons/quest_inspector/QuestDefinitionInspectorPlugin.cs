using Godot;
using LiteQuest.QuestSystem;

namespace LiteQuest.QuestSystem.Editor;

/// <summary>
/// QuestDefinition 的自定义 Inspector。
///   1. _ParseProperty 拦截 PrerequisiteQuests：用复选框列表替代默认 Array&lt;Resource&gt; 编辑器
///   2. _ParseCategory 在 Objective / Auto Start &amp; Prerequisite 分类顶部插入提示与校验
///
/// 注意：编辑器进程里不使用 <c>obj is QuestDefinition</c>（CLR 包装可能退化为基类 Resource），
/// 一律通过 QuestScriptIdentity 的脚本路径识别 + GodotObject.Get 以 Variant 读值。
/// </summary>
[Tool]
public partial class QuestDefinitionInspectorPlugin : EditorInspectorPlugin
{
    private const string ScriptPath = QuestScriptIdentity.QuestDefinitionScriptPath;

    public override bool _CanHandle(GodotObject obj)
        => QuestScriptIdentity.IsScript(obj, ScriptPath);

    public override bool _ParseProperty(GodotObject obj, Variant.Type type, string name,
        PropertyHint hint, string hintString, PropertyUsageFlags usage, bool wide)
    {
        // 拦截 PrerequisiteQuests：用复选框列表替代默认 Array<Resource> 编辑器。
        // 用 AddPropertyEditor 注册为受管 EditorProperty，撤销/重做时会回调其 _UpdateProperty。
        if (name == "PrerequisiteQuests" && QuestScriptIdentity.IsScript(obj, ScriptPath))
        {
            AddPropertyEditor(name, new QuestPrerequisiteEditor());
            return true;
        }
        return false;
    }

    public override void _ParseCategory(GodotObject obj, string category)
    {
        if (!QuestScriptIdentity.IsScript(obj, ScriptPath)) return;

        if (category == "Objective")
        {
            // 动态提示块：订阅资源 Changed，数值/类型改动时即时刷新警告
            AddCustomControl(new QuestObjectiveHints(obj));
        }

        if (category == "Auto Start & Prerequisite")
        {
            bool autoStart = obj.Get("AutoStart").AsBool();
            int prerequisiteCount = CountPrerequisites(obj);

            if (prerequisiteCount == 0 && autoStart)
            {
                var i = new Label { Text = "ℹ️ 无前置任务 + AutoStart：场景加载即自动启动" };
                i.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 0.6f));
                AddCustomControl(i);
            }
        }
    }

    private static int CountPrerequisites(GodotObject obj)
    {
        Variant v = obj.Get("PrerequisiteQuests");
        return v.VariantType == Variant.Type.Array ? v.AsGodotArray().Count : 0;
    }
}
