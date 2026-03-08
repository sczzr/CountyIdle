using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Core;

public class SaveSystem
{
    private const string DatabasePath = "user://countyidle.db";
    private const string LegacySavePath = "user://savegame.json";
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

    private readonly SqliteSaveRepository _repository;

    public string DefaultSlotKey => PrimarySlotKey;
    public IReadOnlyList<string> AutoSaveSlotKeysView => AutoSaveSlotKeys;

    public SaveSystem()
    {
        var globalDatabasePath = ProjectSettings.GlobalizePath(DatabasePath);
        _repository = new SqliteSaveRepository(globalDatabasePath);
    }

    public bool Save(GameState state, out string message)
    {
        return SaveToSlot(state, PrimarySlotKey, PrimarySlotName, out message);
    }

    public bool SaveToSlot(GameState state, string slotKey, string slotName, out string message)
    {
        if (IsAutoSaveSlotKey(slotKey))
        {
            message = "自动存档槽不能手动覆盖。";
            return false;
        }

        return SaveToSlotInternal(state, slotKey, slotName, false, out message);
    }

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

    public bool SaveAutoSlot(GameState state, int rotationIndex, out string slotKey, out string message)
    {
        var normalizedIndex = NormalizeAutoSaveIndex(rotationIndex);
        slotKey = AutoSaveSlotKeys[normalizedIndex];
        var slotName = AutoSaveSlotNames[normalizedIndex];
        return SaveToSlotInternal(state, slotKey, slotName, true, out message);
    }

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

    private static string NormalizeSlotName(string? slotName, string fallbackName)
    {
        var trimmedName = (slotName ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(trimmedName))
        {
            return trimmedName;
        }

        return fallbackName;
    }

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

    private static string GetPreviewPath(string slotKey)
    {
        return $"{PreviewDirectoryPath}/{SanitizeSlotKey(slotKey)}.png";
    }

    private static string SanitizeSlotKey(string slotKey)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(slotKey.Select(character => Array.IndexOf(invalidChars, character) >= 0 ? '_' : character));
    }

    private static void DeletePreviewFile(string slotKey)
    {
        var previewPath = ProjectSettings.GlobalizePath(GetPreviewPath(slotKey));
        if (File.Exists(previewPath))
        {
            File.Delete(previewPath);
        }
    }

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

    private static int NormalizeAutoSaveIndex(int rotationIndex)
    {
        if (rotationIndex < 0)
        {
            return 0;
        }

        return rotationIndex % AutoSaveSlotCount;
    }

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
