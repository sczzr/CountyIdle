using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Core;

/// <summary>
/// 存档系统：负责 SQLite 存档与旧版 JSON 迁移。
/// </summary>
public class SaveSystem
{
    // SQLite 数据库路径
    private const string DatabasePath = "user://countyidle.db";
    // 旧版 JSON 存档路径（用于迁移）
    private const string LegacySavePath = "user://savegame.json";
    // 存档预览图目录
    private const string PreviewDirectoryPath = "user://save_previews";
    private const string PrimarySlotKey = "default";
    private const string PrimarySlotName = "主存档";
    private const int AutoSaveSlotCount = 3;
    private static readonly string[] AutoSaveSlotKeys =
    {
        "autosave",
        "autosave_2",
        "autosave_3"
    };

    private static readonly string[] AutoSaveSlotNames =
    {
        "自动存档 1",
        "自动存档 2",
        "自动存档 3"
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    // SQLite 存档仓库
    private readonly SqliteSaveRepository _repository;

    public string DefaultSlotKey => PrimarySlotKey;
    public IReadOnlyList<string> AutoSaveSlotKeysView => AutoSaveSlotKeys;

    /// <summary>
    /// 初始化存档仓库并定位数据库文件。
    /// </summary>
    public SaveSystem()
    {
        var globalDatabasePath = ProjectSettings.GlobalizePath(DatabasePath);
        _repository = new SqliteSaveRepository(globalDatabasePath);
    }

    /// <summary>
    /// 保存到默认槽。
    /// </summary>
    public bool Save(GameState state, out string message)
    {
        return SaveToSlot(state, PrimarySlotKey, PrimarySlotName, out message);
    }

    /// <summary>
    /// 保存到指定槽（非自动槽）。
    /// </summary>
    public bool SaveToSlot(GameState state, string slotKey, string slotName, out string message)
    {
        if (IsAutoSaveSlotKey(slotKey))
        {
            message = "自动存档槽不能手动覆盖。";
            return false;
        }

        return SaveToSlotInternal(state, slotKey, slotName, false, out message);
    }

    /// <summary>
    /// 新建存档槽并保存当前状态。
    /// </summary>
    public bool SaveToNewSlot(GameState state, string slotName, out string slotKey, out string message)
    {
        slotKey = string.Empty;
        var normalizedSlotName = NormalizeSlotName(slotName, string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedSlotName))
        {
            message = "新建存档槽失败：请输入槽位名称。";
            return false;
        }

        slotKey = $"slot_{Guid.NewGuid():N}";
        return SaveToSlot(state, slotKey, normalizedSlotName, out message);
    }

    /// <summary>
    /// 复制现有存档槽到新槽（包含预览图）。
    /// </summary>
    public bool CopySlotToNewSlot(string sourceSlotKey, string targetSlotName, out string slotKey, out string message)
    {
        slotKey = string.Empty;

        try
        {
            if (!_repository.TryLoadLatestSnapshot(sourceSlotKey, out var snapshot, out var sourceSummary))
            {
                message = "复制失败：未找到所选存档槽。";
                return false;
            }

            var state = JsonSerializer.Deserialize<GameState>(snapshot!.GameStateJson) ?? new GameState();
            var normalizedTargetName = NormalizeCopiedSlotName(targetSlotName, sourceSummary!.SlotName);
            slotKey = $"slot_{Guid.NewGuid():N}";

            if (!SaveToSlotInternal(state, slotKey, normalizedTargetName, false, out message))
            {
                return false;
            }

            var previewCopied = TryCopyPreviewFile(sourceSlotKey, slotKey);
            message = previewCopied
                ? $"已复制存档槽为“{normalizedTargetName}”，并同步复制截图预览。"
                : $"已复制存档槽为“{normalizedTargetName}”。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"复制失败：{ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 自动存档（按轮换索引写入固定槽）。
    /// </summary>
    public bool SaveAutoSlot(GameState state, int rotationIndex, out string slotKey, out string message)
    {
        var normalizedIndex = NormalizeAutoSaveIndex(rotationIndex);
        slotKey = AutoSaveSlotKeys[normalizedIndex];
        var slotName = AutoSaveSlotNames[normalizedIndex];
        return SaveToSlotInternal(state, slotKey, slotName, true, out message);
    }

    /// <summary>
    /// 读取默认槽；若不存在则尝试读取最新快照。
    /// </summary>
    public bool TryLoad(out GameState state, out string message)
    {
        state = new GameState();

        try
        {
            var migrationMessage = string.Empty;
            if (TryMigrateLegacyJsonIfNeeded(out var migratedMessage))
            {
                migrationMessage = migratedMessage;
            }

            if (_repository.TryLoadLatestSnapshot(PrimarySlotKey, out var snapshot, out var slotSummary) ||
                _repository.TryLoadLatestSnapshot(out snapshot, out slotSummary))
            {
                state = JsonSerializer.Deserialize<GameState>(snapshot!.GameStateJson) ?? new GameState();
                message = string.IsNullOrWhiteSpace(migrationMessage)
                    ? $"读档成功（SQLite：{slotSummary!.SlotName}）。"
                    : $"{migrationMessage} 读档成功（SQLite：{slotSummary!.SlotName}）。";
                return true;
            }

            message = "未找到存档，已使用初始状态。";
            return false;
        }
        catch (Exception ex)
        {
            message = $"读档失败：{ex.Message}";
            state = new GameState();
            return false;
        }
    }

    /// <summary>
    /// 读取指定存档槽。
    /// </summary>
    public bool TryLoadSlot(string slotKey, out GameState state, out string message)
    {
        state = new GameState();

        try
        {
            if (!_repository.TryLoadLatestSnapshot(slotKey, out var snapshot, out var slotSummary))
            {
                message = "未找到所选存档槽。";
                return false;
            }

            state = JsonSerializer.Deserialize<GameState>(snapshot!.GameStateJson) ?? new GameState();
            message = $"读档成功（SQLite：{slotSummary!.SlotName}）。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"读档失败：{ex.Message}";
            state = new GameState();
            return false;
        }
    }

    /// <summary>
    /// 列出所有存档槽摘要（并补齐预览图路径）。
    /// </summary>
    public IReadOnlyList<SaveSlotSummary> ListSlots()
    {
        try
        {
            var slots = _repository.ListSlots();
            foreach (var slot in slots)
            {
                slot.PreviewImagePath = ProjectSettings.GlobalizePath(GetPreviewPath(slot.SlotKey));
            }

            return slots;
        }
        catch
        {
            return Array.Empty<SaveSlotSummary>();
        }
    }

    /// <summary>
    /// 重命名存档槽（保护槽不可改名）。
    /// </summary>
    public bool RenameSlot(string slotKey, string slotName, out string message)
    {
        if (IsProtectedSlotKey(slotKey))
        {
            message = "受保护槽位不能重命名。";
            return false;
        }

        var normalizedSlotName = NormalizeSlotName(slotName, string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedSlotName))
        {
            message = "重命名失败：请输入槽位名称。";
            return false;
        }

        try
        {
            if (!_repository.RenameSlot(slotKey, normalizedSlotName))
            {
                message = "重命名失败：未找到所选存档槽。";
                return false;
            }

            message = $"已重命名存档槽为“{normalizedSlotName}”。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"重命名失败：{ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 删除存档槽（保护槽不可删除）。
    /// </summary>
    public bool DeleteSlot(string slotKey, out string message)
    {
        if (IsProtectedSlotKey(slotKey))
        {
            message = "受保护槽位不能删除。";
            return false;
        }

        try
        {
            if (!_repository.DeleteSlot(slotKey))
            {
                message = "删除失败：未找到所选存档槽。";
                return false;
            }

            DeletePreviewFile(slotKey);
            message = "已删除所选存档槽。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"删除失败：{ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 保存存档预览图（PNG）。
    /// </summary>
    public bool SavePreview(string slotKey, Image image, out string message)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            message = "存档截图生成失败：槽位无效。";
            return false;
        }

        try
        {
            Directory.CreateDirectory(ProjectSettings.GlobalizePath(PreviewDirectoryPath));
            var previewPath = ProjectSettings.GlobalizePath(GetPreviewPath(slotKey));
            var saveError = image.SavePng(previewPath);
            if (saveError != Error.Ok)
            {
                message = $"存档截图生成失败：{saveError}";
                return false;
            }

            message = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            message = $"存档截图生成失败：{ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 若存在旧版 JSON 且数据库为空，则迁移到 SQLite。
    /// </summary>
    private bool TryMigrateLegacyJsonIfNeeded(out string message)
    {
        message = string.Empty;

        if (_repository.HasAnySnapshots() || !Godot.FileAccess.FileExists(LegacySavePath))
        {
            return false;
        }

        using var file = Godot.FileAccess.Open(LegacySavePath, Godot.FileAccess.ModeFlags.Read);
        var content = file.GetAsText();
        var legacyState = JsonSerializer.Deserialize<GameState>(content) ?? new GameState();
        var legacyJson = JsonSerializer.Serialize(legacyState, JsonOptions);

        _repository.SaveSnapshot(
            PrimarySlotKey,
            PrimarySlotName,
            false,
            legacyJson,
            legacyState.GameMinutes,
            legacyState.Population,
            legacyState.Gold,
            legacyState.TechLevel,
            legacyState.Happiness,
            legacyState.Threat,
            legacyState.ExplorationDepth,
            legacyState.GetWarehouseUsed(),
            legacyState.WarehouseCapacity);

        message = "已将旧版 JSON 存档迁移到 SQLite。";
        return true;
    }

    /// <summary>
    /// 规范化槽位名称（去空白，回退默认名）。
    /// </summary>
    private static string NormalizeSlotName(string? slotName, string fallbackName)
    {
        var trimmedName = (slotName ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(trimmedName))
        {
            return trimmedName;
        }

        return fallbackName;
    }

    /// <summary>
    /// 复制槽位时的命名规范（避免与源名相同）。
    /// </summary>
    private static string NormalizeCopiedSlotName(string? requestedSlotName, string sourceSlotName)
    {
        var normalizedRequestedName = NormalizeSlotName(requestedSlotName, string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedRequestedName) ||
            string.Equals(normalizedRequestedName, sourceSlotName, StringComparison.Ordinal))
        {
            return $"{sourceSlotName} 副本";
        }

        return normalizedRequestedName;
    }

    /// <summary>
    /// 生成预览图相对路径。
    /// </summary>
    private static string GetPreviewPath(string slotKey)
    {
        return $"{PreviewDirectoryPath}/{SanitizeSlotKey(slotKey)}.png";
    }

    /// <summary>
    /// 清理槽位 key 中的非法文件名字符。
    /// </summary>
    private static string SanitizeSlotKey(string slotKey)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(slotKey.Select(character => Array.IndexOf(invalidChars, character) >= 0 ? '_' : character));
    }

    /// <summary>
    /// 删除指定槽位的预览图文件。
    /// </summary>
    private static void DeletePreviewFile(string slotKey)
    {
        var previewPath = ProjectSettings.GlobalizePath(GetPreviewPath(slotKey));
        if (File.Exists(previewPath))
        {
            File.Delete(previewPath);
        }
    }

    /// <summary>
    /// 复制预览图文件（若存在）。
    /// </summary>
    private static bool TryCopyPreviewFile(string sourceSlotKey, string targetSlotKey)
    {
        var sourcePreviewPath = ProjectSettings.GlobalizePath(GetPreviewPath(sourceSlotKey));
        if (!File.Exists(sourcePreviewPath))
        {
            return false;
        }

        var targetPreviewPath = ProjectSettings.GlobalizePath(GetPreviewPath(targetSlotKey));
        try
        {
            var previewDirectory = Path.GetDirectoryName(targetPreviewPath);
            if (!string.IsNullOrWhiteSpace(previewDirectory))
            {
                Directory.CreateDirectory(previewDirectory);
            }

            File.Copy(sourcePreviewPath, targetPreviewPath, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 是否为保护槽（默认槽或自动槽）。
    /// </summary>
    public bool IsProtectedSlotKey(string? slotKey)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            return false;
        }

        if (string.Equals(slotKey, PrimarySlotKey, StringComparison.Ordinal))
        {
            return true;
        }

        return IsAutoSaveSlotKey(slotKey);
    }

    /// <summary>
    /// 判断是否自动存档槽。
    /// </summary>
    private static bool IsAutoSaveSlotKey(string? slotKey)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            return false;
        }

        foreach (var autoSaveSlotKey in AutoSaveSlotKeys)
        {
            if (string.Equals(slotKey, autoSaveSlotKey, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 轮换索引标准化（负数归零）。
    /// </summary>
    private static int NormalizeAutoSaveIndex(int rotationIndex)
    {
        if (rotationIndex < 0)
        {
            return 0;
        }

        return rotationIndex % AutoSaveSlotCount;
    }

    /// <summary>
    /// 实际写入存档快照的内部入口。
    /// </summary>
    private bool SaveToSlotInternal(GameState state, string slotKey, string slotName, bool isAutosave, out string message)
    {
        try
        {
            var json = JsonSerializer.Serialize(state, JsonOptions);
            var slotSummary = _repository.SaveSnapshot(
                slotKey,
                NormalizeSlotName(slotName, PrimarySlotName),
                isAutosave,
                json,
                state.GameMinutes,
                state.Population,
                state.Gold,
                state.TechLevel,
                state.Happiness,
                state.Threat,
                state.ExplorationDepth,
                state.GetWarehouseUsed(),
                state.WarehouseCapacity);
            message = isAutosave
                ? $"自动存档已更新（SQLite：{slotSummary.SlotName}）。"
                : $"存档成功（SQLite：{slotSummary.SlotName}）。";
            return true;
        }
        catch (Exception ex)
        {
            message = isAutosave
                ? $"自动存档失败：{ex.Message}"
                : $"存档失败：{ex.Message}";
            return false;
        }
    }
}
