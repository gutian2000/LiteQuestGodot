using Godot;

namespace LiteQuest.QuestSystem;

/// <summary>
/// 收集物定义资产，对应 Unity 版的 CollectibleDefinition (ScriptableObject)。
///
/// Unity → Godot 映射：
///   public Sprite icon  → Texture2D Icon（Godot 用 Texture2D）
///
/// ⚠️ Godot 4 C#：用字段而不是 auto-property，避免序列化空值 bug。
/// </summary>
[GlobalClass]
public partial class CollectibleDefinition : Resource
{
    [ExportCategory("Identity")]
    [Export] public string DisplayName = "Coin";
    [Export] public string TargetId = "Coin";

    [ExportCategory("Presentation")]
    [Export] public Texture2D Icon;
    [Export(PropertyHint.MultilineText)] public string Description = "A collectible item.";
}
