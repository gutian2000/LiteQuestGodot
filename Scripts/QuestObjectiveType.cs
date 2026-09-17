namespace LiteQuest.QuestSystem;

/// <summary>
/// 任务目标类型。
/// 每种类型决定了 QuestManager 如何推进进度。
/// </summary>
public enum QuestObjectiveType
{
    /// <summary>收集物：ReportCollect("Coin", 1) 推进</summary>
    Collect,

    /// <summary>击杀：ReportKill("Slime", 1) 推进，由敌人死亡事件触发</summary>
    Kill,

    /// <summary>伤害累计：ReportDamage("Boss", 50) 推进，由伤害事件触发</summary>
    Damage,

    /// <summary>到达位置：EnterTrigger("DungeonEntrance") 推进，由 Area2D body_entered 触发</summary>
    ReachLocation,

    /// <summary>自定义：由调用方手动调用 CompleteQuest 完成</summary>
    Custom
}
