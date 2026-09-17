using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace LiteQuest.QuestSystem.Editor;

/// <summary>
/// 前置任务多选的属性编辑器（EditorProperty）。
///
/// 为什么必须继承 EditorProperty 而不是普通 VBoxContainer + AddCustomControl：
/// 只有 EditorProperty 会在用户修改 / Ctrl+Z 撤销 / Ctrl+Y 重做 / 外部改动时，
/// 被检查器回调 _UpdateProperty()，从而让复选框勾选状态与底层数据保持同步。
/// 修改通过 EmitChanged 提交，由检查器自动登记 UndoRedo，无需手动操作。
///
/// 编辑器进程不做 QuestDefinition 强转：任务资源以基类 Resource 持有，
/// 靠 ResourcePath 去重/比较，靠 Get("QuestName") 读显示名。
/// </summary>
[Tool]
public partial class QuestPrerequisiteEditor : EditorProperty
{
    private readonly VBoxContainer _bottom;
    private bool _updating;

    public QuestPrerequisiteEditor()
    {
        _bottom = new VBoxContainer();
        // 官方写法：必须先 AddChild 再 SetBottomEditor。
        // 只调 SetBottomEditor 会让引擎布局一个"无父节点"的控件，
        // 触发 container.cpp: p_child->get_parent() != this 报错。
        AddChild(_bottom);
        // 让复选框列表占满属性标签下方的整行
        SetBottomEditor(_bottom);
    }

    /// <summary>检查器在属性变化（含撤销/重做）后回调，这里按当前数据重建勾选状态。</summary>
    public override void _UpdateProperty()
    {
        Rebuild();
    }

    private void Rebuild()
    {
        _updating = true;
        foreach (var child in _bottom.GetChildren())
            child.Free();

        var title = new Label
        {
            Text = "前置任务（勾选项必须全部 Completed 后本任务才能启动）"
        };
        title.AddThemeFontSizeOverride("font_size", 12);
        _bottom.AddChild(title);

        var obj = GetEditedObject();
        var allQuests = QuestScriptIdentity.ScanResourcesWithScript(
            QuestScriptIdentity.QuestDefinitionScriptPath);

        var current = new HashSet<string>();
        Variant arrVar = obj.Get(GetEditedProperty());
        if (arrVar.VariantType == Variant.Type.Array)
        {
            foreach (Variant item in arrVar.AsGodotArray())
            {
                if (item.AsGodotObject() is Resource r && !string.IsNullOrEmpty(r.ResourcePath))
                    current.Add(r.ResourcePath);
            }
        }

        string selfPath = (obj as Resource)?.ResourcePath ?? "";

        int count = 0;
        foreach (var q in allQuests)
        {
            if (!string.IsNullOrEmpty(selfPath) && q.ResourcePath == selfPath) continue;

            string qName = q.Get("QuestName").AsString();
            var cb = new CheckBox
            {
                Text = string.IsNullOrEmpty(qName) ? q.ResourcePath : qName,
                ButtonPressed = current.Contains(q.ResourcePath)
            };
            var captured = q;
            cb.Toggled += on => OnToggled(captured, on);
            _bottom.AddChild(cb);
            count++;
        }

        if (count == 0)
        {
            var none = new Label { Text = "（未扫描到其他 QuestDefinition 资产）" };
            none.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            _bottom.AddChild(none);
        }

        _updating = false;
    }

    private void OnToggled(Resource toggled, bool on)
    {
        // 重建时程序化设置 ButtonPressed 也会触发 Toggled，需忽略
        if (_updating) return;

        var obj = GetEditedObject();
        Variant oldVar = obj.Get(GetEditedProperty());

        var newArr = new Array<Resource>();
        if (oldVar.VariantType == Variant.Type.Array)
        {
            foreach (Variant item in oldVar.AsGodotArray())
            {
                if (item.AsGodotObject() is Resource r)
                    newArr.Add(r);
            }
        }

        if (on)
        {
            bool exists = false;
            foreach (var r in newArr)
            {
                if (r != null && r.ResourcePath == toggled.ResourcePath)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists) newArr.Add(toggled);
        }
        else
        {
            var filtered = new Array<Resource>();
            foreach (var r in newArr)
            {
                if (r != null && r.ResourcePath == toggled.ResourcePath) continue;
                filtered.Add(r);
            }
            newArr = filtered;
        }

        // 交给检查器：写入属性 + 自动登记撤销/重做，随后会回调 _UpdateProperty 刷新
        EmitChanged(GetEditedProperty(), newArr);
    }
}
