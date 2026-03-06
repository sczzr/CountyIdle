namespace CountyIdle.Models;

public class ClientSettings
{
    public const string DefaultLanguage = "zh_CN";
    public const int DefaultResolutionWidth = 1600;
    public const int DefaultResolutionHeight = 900;
    public const float DefaultFontScale = 1.0f;
    public const float DefaultMasterVolume = 0.8f;
    public const string DefaultOpenSettingsKey = "F1";
    public const string DefaultOpenWarehouseKey = "B";
    public const string DefaultToggleExplorationKey = "E";
    public const string DefaultToggleSpeedKey = "Tab";
    public const string DefaultQuickSaveKey = "F5";
    public const string DefaultQuickLoadKey = "F9";
    public const string DefaultQuickResetKey = "R";

    public string Language { get; set; } = DefaultLanguage;
    public int ResolutionWidth { get; set; } = DefaultResolutionWidth;
    public int ResolutionHeight { get; set; } = DefaultResolutionHeight;
    public float FontScale { get; set; } = DefaultFontScale;
    public float MasterVolume { get; set; } = DefaultMasterVolume;
    public string OpenSettingsKey { get; set; } = DefaultOpenSettingsKey;
    public string OpenWarehouseKey { get; set; } = DefaultOpenWarehouseKey;
    public string ToggleExplorationKey { get; set; } = DefaultToggleExplorationKey;
    public string ToggleSpeedKey { get; set; } = DefaultToggleSpeedKey;
    public string QuickSaveKey { get; set; } = DefaultQuickSaveKey;
    public string QuickLoadKey { get; set; } = DefaultQuickLoadKey;
    public string QuickResetKey { get; set; } = DefaultQuickResetKey;

    public ClientSettings Clone()
    {
        return (ClientSettings)MemberwiseClone();
    }
}
