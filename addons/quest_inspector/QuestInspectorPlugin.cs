using Godot;

namespace LiteQuest.QuestSystem.Editor;

/// <summary>
/// 插件入口。在 _EnterTree 注册 InspectorPlugin，_ExitTree 注销。
/// 启用方式：项目设置 → 插件 → 勾选 QuestInspector。
/// </summary>
[Tool]
public partial class QuestInspectorPlugin : EditorPlugin
{
    private QuestDefinitionInspectorPlugin _inspector;

    public override void _EnterTree()
    {
        _inspector = new QuestDefinitionInspectorPlugin();
        AddInspectorPlugin(_inspector);
    }

    public override void _ExitTree()
    {
        if (_inspector != null)
        {
            RemoveInspectorPlugin(_inspector);
            _inspector = null;
        }
    }
}
