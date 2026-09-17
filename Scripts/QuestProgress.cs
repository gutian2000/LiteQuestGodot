using System.Text.Json.Serialization;

namespace LiteQuest.QuestSystem;

/// <summary>
/// 单个任务的运行时状态快照。
///
/// 为什么独立成类而不是 QuestManager 的匿名嵌套类：
///   1. Unity 版用 private class QuestRuntimeData，Godot 版提取出来方便 Day 6 存档/读档
///   2. 序列化友好（System.Text.Json 可以直接 serialize）
///   3. 未来 QuestManager.GetProgressSnapshot() 可以把整个系统状态导出给 UI 或存档
/// </summary>
public class QuestProgress
{
    /// <summary>
    /// QuestDefinition 的 ResourcePath。
    /// 存档/读档时用这个重新关联 Resource，因为 Resource 本身不应该被修改。
    /// </summary>
    [JsonPropertyName("quest_id")]
    public string QuestResourcePath { get; set; } = "";

    [JsonPropertyName("status")]
    public QuestStatus Status { get; set; } = QuestStatus.NotStarted;

    [JsonPropertyName("current")]
    public int CurrentProgress { get; set; } = 0;

    [JsonPropertyName("completed_at")]
    public long CompletedAtUnixMs { get; set; } = 0;
}
