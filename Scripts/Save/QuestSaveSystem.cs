using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace LiteQuest.QuestSystem.Save;

/// <summary>
/// 存档文件的顶层包装。
/// 加版本号是存档系统的标准做法：未来字段变动时可按 Version 做迁移，而不是直接崩。
/// </summary>
public class QuestSaveData
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("saved_at")]
    public long SavedAtUnixMs { get; set; }

    [JsonPropertyName("quests")]
    public List<QuestProgress> Quests { get; set; } = new();
}

/// <summary>
/// 任务存档/读档。用 Godot FileAccess 写 user:// 目录（跨平台：Windows 在 %APPDATA%/Godot/app_userdata/&lt;项目&gt;）。
/// JSON 用 System.Text.Json（.NET 内置，零第三方依赖），枚举转字符串提升可读性。
///
/// 恢复顺序（调用方必须遵守）：
///   1. RegisterAll(...) 重新加载所有 QuestDefinition
///   2. LoadFromSnapshot(data.Quests) 恢复运行时状态
///   3. UI.RebuildFromManager() 按恢复后的状态重建界面
/// </summary>
public static class QuestSaveSystem
{
    private const string SavePath = "user://quest_save.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() } // status 存 "InProgress" 而不是 1
    };

    /// <summary>写入存档。返回 false 表示文件 IO 失败（错误已打印）。</summary>
    public static bool Save(IReadOnlyList<QuestProgress> snapshot)
    {
        var data = new QuestSaveData
        {
            Version = 1,
            SavedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Quests = new List<QuestProgress>(snapshot)
        };

        try
        {
            string json = JsonSerializer.Serialize(data, JsonOptions);
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PrintErr($"[QuestSave] 无法打开存档文件写入: {SavePath} / {FileAccess.GetOpenError()}");
                return false;
            }
            file.StoreString(json);
            GD.Print($"[QuestSave] 已保存 {data.Quests.Count} 个任务状态 → {SavePath}");
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr("[QuestSave] 保存异常: " + e);
            return false;
        }
    }

    /// <summary>读取存档；不存在或解析失败返回 null（调用方按"新游戏"处理）。</summary>
    public static QuestSaveData Load()
    {
        if (!FileAccess.FileExists(SavePath))
            return null;

        try
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"[QuestSave] 无法打开存档文件读取: {FileAccess.GetOpenError()}");
                return null;
            }
            string json = file.GetAsText();
            var data = JsonSerializer.Deserialize<QuestSaveData>(json, JsonOptions);
            if (data == null)
            {
                GD.PrintErr("[QuestSave] 存档内容反序列化为 null");
                return null;
            }
            GD.Print($"[QuestSave] 已读取存档（version={data.Version}，{data.Quests.Count} 个任务）");
            return data;
        }
        catch (Exception e)
        {
            // 损坏的存档不应卡死游戏：打印后按新游戏处理
            GD.PrintErr("[QuestSave] 读档异常（将按新游戏启动）: " + e);
            return null;
        }
    }

    public static bool HasSave() => FileAccess.FileExists(SavePath);

    /// <summary>删除存档（调试/重开新游戏用）。</summary>
    public static void DeleteSave()
    {
        if (FileAccess.FileExists(SavePath))
        {
            DirAccess.RemoveAbsolute(SavePath);
            GD.Print("[QuestSave] 存档已删除");
        }
    }

    /// <summary>返回存档在操作系统上的绝对路径（方便用户找到文件查看）。</summary>
    public static string GetAbsoluteSavePath() => ProjectSettings.GlobalizePath(SavePath);
}
