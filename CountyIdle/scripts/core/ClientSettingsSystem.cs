using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Core;

/// <summary>
/// 客户端设置的读写与兜底规范化。
/// </summary>
public class ClientSettingsSystem
{
    // 客户端设置存档路径（Godot user://）
    private const string SavePath = "user://client_settings.json";
    // 画面缩放的软边界：既允许缩小以查看更多内容，也允许放大便于阅读。
    public const float MinContentZoom = 0.75f;
    public const float MaxContentZoom = 1.50f;
    // JSON 美化输出，便于人工检查
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    // 常用分辨率候选（运行时还会叠加当前屏幕上限）
    private static readonly Vector2I[] SupportedResolutions =
    {
        new Vector2I(1280, 720),
        new Vector2I(1366, 768),
        new Vector2I(1600, 900),
        new Vector2I(1920, 1080),
        new Vector2I(2560, 1440)
    };
    // 快捷键冲突时的候补池（确保最终可用）
    private static readonly string[] ShortcutFallbackPool =
    {
        ClientSettings.DefaultOpenSettingsKey,
        ClientSettings.DefaultOpenWarehouseKey,
        ClientSettings.DefaultToggleExplorationKey,
        ClientSettings.DefaultToggleSpeedKey,
        ClientSettings.DefaultQuickSaveKey,
        ClientSettings.DefaultQuickLoadKey,
        ClientSettings.DefaultQuickResetKey,
        "F2",
        "F3",
        "F4",
        "F6",
        "F7",
        "F8",
        "G",
        "T"
    };

    /// <summary>
    /// 读取客户端设置，并在异常时回退到默认配置。
    /// </summary>
    public ClientSettings Load(out string message)
    {
        if (!FileAccess.FileExists(SavePath))
        {
            message = "未找到客户端设置，已使用默认配置。";
            return new ClientSettings();
        }

        try
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            var content = file.GetAsText();
            var loaded = JsonSerializer.Deserialize<ClientSettings>(content);
            message = "客户端设置读取成功。";
            return Normalize(loaded);
        }
        catch (Exception ex)
        {
            message = $"客户端设置读取失败：{ex.Message}，已回退默认配置。";
            return new ClientSettings();
        }
    }

    /// <summary>
    /// 保存客户端设置（会先规范化）。
    /// </summary>
    public bool Save(ClientSettings settings, out string message)
    {
        try
        {
            var sanitized = Normalize(settings);
            var json = JsonSerializer.Serialize(sanitized, JsonOptions);
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            file.StoreString(json);
            message = "基础设置已保存（含快捷键）。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"基础设置保存失败：{ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 对设置值做边界与一致性校验，确保可安全使用。
    /// </summary>
    public ClientSettings Normalize(ClientSettings? settings)
    {
        var normalized = settings?.Clone() ?? new ClientSettings();

        if (!IsSupportedLanguage(normalized.Language))
        {
            normalized.Language = ClientSettings.DefaultLanguage;
        }

        var resolution = new Vector2I(normalized.ResolutionWidth, normalized.ResolutionHeight);
        if (!IsSupportedResolution(resolution))
        {
            normalized.ResolutionWidth = ClientSettings.DefaultResolutionWidth;
            normalized.ResolutionHeight = ClientSettings.DefaultResolutionHeight;
        }

        normalized.FontScale = Mathf.Clamp(normalized.FontScale, MinContentZoom, MaxContentZoom);
        normalized.MasterVolume = Mathf.Clamp(normalized.MasterVolume, 0.0f, 1.0f);
        NormalizeShortcuts(normalized);
        return normalized;
    }

    /// <summary>
    /// 读取当前窗口所在屏幕的最大分辨率；失败时回退到项目默认分辨率。
    /// </summary>
    public static Vector2I GetCurrentScreenMaxResolution()
    {
        var currentScreen = DisplayServer.WindowGetCurrentScreen();
        var screenSize = DisplayServer.ScreenGetSize(currentScreen);
        if (screenSize.X <= 0 || screenSize.Y <= 0)
        {
            return new Vector2I(ClientSettings.DefaultResolutionWidth, ClientSettings.DefaultResolutionHeight);
        }

        return screenSize;
    }

    /// <summary>
    /// 获取当前设备可用的分辨率列表：保留 720p 下限，并自动补入当前屏幕上限。
    /// </summary>
    public static IReadOnlyList<Vector2I> GetAvailableResolutions()
    {
        var availableResolutions = new List<Vector2I>();
        var currentScreenMaxResolution = GetCurrentScreenMaxResolution();

        foreach (var supportedResolution in SupportedResolutions)
        {
            if (supportedResolution.X < ClientSettings.DefaultResolutionWidth ||
                supportedResolution.Y < ClientSettings.DefaultResolutionHeight)
            {
                continue;
            }

            if (supportedResolution.X > currentScreenMaxResolution.X ||
                supportedResolution.Y > currentScreenMaxResolution.Y)
            {
                continue;
            }

            availableResolutions.Add(supportedResolution);
        }

        if (currentScreenMaxResolution.X >= ClientSettings.DefaultResolutionWidth &&
            currentScreenMaxResolution.Y >= ClientSettings.DefaultResolutionHeight &&
            !availableResolutions.Contains(currentScreenMaxResolution))
        {
            availableResolutions.Add(currentScreenMaxResolution);
        }

        if (availableResolutions.Count == 0)
        {
            availableResolutions.Add(new Vector2I(ClientSettings.DefaultResolutionWidth, ClientSettings.DefaultResolutionHeight));
        }

        availableResolutions.Sort(static (left, right) =>
        {
            var areaComparison = (left.X * left.Y).CompareTo(right.X * right.Y);
            return areaComparison != 0 ? areaComparison : left.X.CompareTo(right.X);
        });

        return availableResolutions;
    }

    /// <summary>
    /// 当前仅支持中文/英文两种语言码。
    /// </summary>
    private static bool IsSupportedLanguage(string languageCode)
    {
        return string.Equals(languageCode, "zh_CN", StringComparison.Ordinal) ||
               string.Equals(languageCode, "en", StringComparison.Ordinal);
    }

    /// <summary>
    /// 是否属于当前设备允许的分辨率候选。
    /// </summary>
    private static bool IsSupportedResolution(Vector2I resolution)
    {
        foreach (var supported in GetAvailableResolutions())
        {
            if (supported == resolution)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 规范化快捷键，避免冲突与空值。
    /// </summary>
    private static void NormalizeShortcuts(ClientSettings settings)
    {
        var usedKeys = new HashSet<Key>();
        settings.OpenSettingsKey = NormalizeUniqueShortcut(settings.OpenSettingsKey, ClientSettings.DefaultOpenSettingsKey, usedKeys);
        settings.OpenWarehouseKey = NormalizeUniqueShortcut(settings.OpenWarehouseKey, ClientSettings.DefaultOpenWarehouseKey, usedKeys);
        settings.ToggleExplorationKey = NormalizeUniqueShortcut(settings.ToggleExplorationKey, ClientSettings.DefaultToggleExplorationKey, usedKeys);
        settings.ToggleSpeedKey = NormalizeUniqueShortcut(settings.ToggleSpeedKey, ClientSettings.DefaultToggleSpeedKey, usedKeys);
        settings.QuickSaveKey = NormalizeUniqueShortcut(settings.QuickSaveKey, ClientSettings.DefaultQuickSaveKey, usedKeys);
        settings.QuickLoadKey = NormalizeUniqueShortcut(settings.QuickLoadKey, ClientSettings.DefaultQuickLoadKey, usedKeys);
        settings.QuickResetKey = NormalizeUniqueShortcut(settings.QuickResetKey, ClientSettings.DefaultQuickResetKey, usedKeys);
    }

    /// <summary>
    /// 确保单个快捷键不与已占用键冲突，必要时回退候补池。
    /// </summary>
    private static string NormalizeUniqueShortcut(string rawKey, string fallbackKey, HashSet<Key> usedKeys)
    {
        if (TryParseKey(rawKey, out var parsedKey) && usedKeys.Add(parsedKey))
        {
            return parsedKey.ToString();
        }

        if (TryParseKey(fallbackKey, out var fallbackParsed) && usedKeys.Add(fallbackParsed))
        {
            return fallbackParsed.ToString();
        }

        foreach (var candidate in ShortcutFallbackPool)
        {
            if (!TryParseKey(candidate, out var candidateKey))
            {
                continue;
            }

            if (usedKeys.Add(candidateKey))
            {
                return candidateKey.ToString();
            }
        }

        return fallbackKey;
    }

    /// <summary>
    /// 尝试把字符串解析为 Godot 的 Key 枚举。
    /// </summary>
    private static bool TryParseKey(string? keyName, out Key key)
    {
        key = Key.None;
        if (string.IsNullOrWhiteSpace(keyName))
        {
            return false;
        }

        if (!Enum.TryParse<Key>(keyName, true, out var parsedKey))
        {
            return false;
        }

        if (parsedKey == Key.None)
        {
            return false;
        }

        key = parsedKey;
        return true;
    }
}
