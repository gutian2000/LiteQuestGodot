using System;
using System.Collections.Generic;
using Godot;

namespace LiteQuest.QuestSystem.Editor;

/// <summary>
/// 编辑器上下文里识别 C# 资源类型的辅助器。
///
/// 为什么不用 <c>obj is QuestDefinition</c>：
/// 在 Godot 编辑器进程中，自定义 C# Resource 经 .tres 反序列化后，CLR 互操作
/// 包装层可能返回基类 Godot.Resource（native 侧脚本与字段均正常，Inspector 显示
/// 也正常，游戏 F5 运行时加载同样是强类型）。这是 Godot 4 C# 编辑器桥接的已知
/// 不对称。因此编辑器插件一律改用 native 身份（脚本的 res:// 路径）判断，
/// 属性通过 GodotObject.Get/Set(StringName) 以 Variant 读写，不依赖 CLR 强类型。
/// </summary>
internal static class QuestScriptIdentity
{
    public const string QuestDefinitionScriptPath = "res://Scripts/QuestDefinition.cs";
    public const string CollectibleDefinitionScriptPath = "res://Scripts/CollectibleDefinition.cs";

    /// <summary>判断对象附加的 C# 脚本是否为指定 res:// 路径。</summary>
    public static bool IsScript(GodotObject obj, string scriptResourcePath)
    {
        if (obj == null || string.IsNullOrEmpty(scriptResourcePath)) return false;
        return obj.GetScript().AsGodotObject() is CSharpScript cs
               && cs.ResourcePath == scriptResourcePath;
    }

    /// <summary>递归扫描 res://，返回所有挂载指定脚本的 .tres 资源（以基类 Resource 返回）。</summary>
    public static List<Resource> ScanResourcesWithScript(string scriptResourcePath)
    {
        var result = new List<Resource>();
        Scan("res://", scriptResourcePath, result);
        return result;
    }

    private static void Scan(string dirPath, string scriptPath, List<Resource> result)
    {
        var dir = DirAccess.Open(dirPath);
        if (dir == null) return;

        dir.ListDirBegin();
        string name;
        while ((name = dir.GetNext()) != "")
        {
            if (name == "." || name == "..") continue;
            // 跳过 .godot 等隐藏目录，避免扫到编辑器缓存
            if (name.StartsWith(".", StringComparison.Ordinal)) continue;

            // 仅在缺少结尾斜杠时补一个；不能 TrimEnd，否则会砍掉 "res://" 协议里的斜杠
            string basePath = dirPath.EndsWith('/') ? dirPath : dirPath + "/";
            string full = basePath + name;
            if (dir.CurrentIsDir())
            {
                Scan(full, scriptPath, result);
            }
            else if (name.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
            {
                var res = ResourceLoader.Load(full);
                if (res != null && IsScript(res, scriptPath))
                    result.Add(res);
            }
        }
    }
}
