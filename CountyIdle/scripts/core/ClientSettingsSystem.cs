using System;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Core;

public class ClientSettingsSystem
{
    private const string SavePath = "user://client_settings.json";
    private const float MinFontScale = 0.85f;
    private const float MaxFontScale = 1.30f;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Vector2I[] SupportedResolutions =
    {
        new Vector2I(1280, 720),
        new Vector2I(1600, 900),
        new Vector2I(1920, 1080),
        new Vector2I(2560, 1440)
    };

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

    public bool Save(ClientSettings settings, out string message)
    {
        try
        {
            var sanitized = Normalize(settings);
            var json = JsonSerializer.Serialize(sanitized, JsonOptions);
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            file.StoreString(json);
            message = "基础设置已保存。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"基础设置保存失败：{ex.Message}";
            return false;
        }
    }

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

        normalized.FontScale = Mathf.Clamp(normalized.FontScale, MinFontScale, MaxFontScale);
        normalized.MasterVolume = Mathf.Clamp(normalized.MasterVolume, 0.0f, 1.0f);
        return normalized;
    }

    private static bool IsSupportedLanguage(string languageCode)
    {
        return string.Equals(languageCode, "zh_CN", StringComparison.Ordinal) ||
               string.Equals(languageCode, "en", StringComparison.Ordinal);
    }

    private static bool IsSupportedResolution(Vector2I resolution)
    {
        foreach (var supported in SupportedResolutions)
        {
            if (supported == resolution)
            {
                return true;
            }
        }

        return false;
    }
}
