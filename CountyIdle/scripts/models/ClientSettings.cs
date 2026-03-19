namespace CountyIdle.Models;

/// <summary>
/// 客户端设置模型（语言、显示模式、窗口分辨率、画面缩放、音量与快捷键）。
/// </summary>
public class ClientSettings
{
    // 默认语言与显示参数
    public const string DefaultLanguage = "zh_CN";
    public const bool DefaultIsFullscreen = false;
    public const int DefaultResolutionWidth = 1280;
    public const int DefaultResolutionHeight = 720;
    // 历史字段名仍保留为 FontScale，但现已承接“画面缩放”含义，兼容旧设置存档。
    public const float DefaultFontScale = 1.0f;
    public const float DefaultMasterVolume = 0.8f;
    // 默认快捷键
    public const string DefaultOpenSettingsKey = "F1";
    public const string DefaultOpenWarehouseKey = "B";
    public const string DefaultToggleExplorationKey = "E";
    public const string DefaultToggleSpeedKey = "Tab";
    public const string DefaultQuickSaveKey = "F5";
    public const string DefaultQuickLoadKey = "F9";
    public const string DefaultQuickResetKey = "R";

    // 当前语言
    public string Language { get; set; } = DefaultLanguage;
    // 是否使用全屏模式
    public bool IsFullscreen { get; set; } = DefaultIsFullscreen;
    // 当前分辨率宽度
    public int ResolutionWidth { get; set; } = DefaultResolutionWidth;
    // 当前分辨率高度
    public int ResolutionHeight { get; set; } = DefaultResolutionHeight;
    // 画面缩放倍率（沿用历史 FontScale 字段名以兼容旧档）
    public float FontScale { get; set; } = DefaultFontScale;
    // 主音量
    public float MasterVolume { get; set; } = DefaultMasterVolume;
    // 打开设置卷快捷键
    public string OpenSettingsKey { get; set; } = DefaultOpenSettingsKey;
    // 打开仓储卷快捷键
    public string OpenWarehouseKey { get; set; } = DefaultOpenWarehouseKey;
    // 探险开关快捷键
    public string ToggleExplorationKey { get; set; } = DefaultToggleExplorationKey;
    // 倍速切换快捷键
    public string ToggleSpeedKey { get; set; } = DefaultToggleSpeedKey;
    // 快速存档快捷键
    public string QuickSaveKey { get; set; } = DefaultQuickSaveKey;
    // 快速读档快捷键
    public string QuickLoadKey { get; set; } = DefaultQuickLoadKey;
    // 快速重置快捷键
    public string QuickResetKey { get; set; } = DefaultQuickResetKey;

    /// <summary>
    /// 浅拷贝一份设置（用于 UI 状态发布）。
    /// </summary>
    public ClientSettings Clone()
    {
        return (ClientSettings)MemberwiseClone();
    }
}
