using Godot;
using CountyIdle.Core;
using CountyIdle.Models;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string SettingsPanelScenePath = "res://scenes/ui/SettingsPanel.tscn";
    private const string MasterAudioBusName = "Master";
    private const float MuteThreshold = 0.001f;
    private const float MuteDb = -80.0f;
    // 720p 是当前项目确认的基础分辨率下限，窗口不允许再缩到更小。
    private static readonly Vector2I MinimumWindowSize = new(1280, 720);

    private readonly ClientSettingsSystem _clientSettingsSystem = new();
    private ClientSettings _clientSettings = new();
    private SettingsPanel? _settingsPanel;
    private Button _settingsButton = null!;

    private void InitializeClientSettings()
    {
        _clientSettings = _clientSettingsSystem.Load(out _);
        ApplyClientSettings(_clientSettings);
    }

    private void CreateSettingsPanel()
    {
        var panelScene = GD.Load<PackedScene>(SettingsPanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _settingsPanel = panelScene.Instantiate<SettingsPanel>();
        // 设置卷在主场景启动时就会预先挂入树中，先显式隐藏，避免欢迎页阶段出现首帧透出。
        _settingsPanel.Hide();
        _settingsPanel.PreviewRequested += OnClientSettingsPreviewRequested;
        _settingsPanel.ApplyRequested += OnClientSettingsApplyRequested;
        AddChild(_settingsPanel);
        MoveChild(_settingsPanel, GetChildCount() - 1);
        _settingsPanel.Hide();
    }

    private void BindSettingsButtonEvent()
    {
        _settingsButton.Pressed += OpenSettingsPanel;
    }

    private void OpenSettingsPanel()
    {
        CloseBlockingOverlayPopups(_settingsPanel);
        _settingsPanel?.Open(_clientSettings.Clone());
    }

    private void OnClientSettingsApplyRequested(ClientSettings nextSettings)
    {
        _clientSettings = _clientSettingsSystem.Normalize(nextSettings);
        ApplyClientSettings(_clientSettings);
        _clientSettingsSystem.Save(_clientSettings, out var saveMessage);
        AppendLog(saveMessage);
    }

    private void OnClientSettingsPreviewRequested(ClientSettings nextSettings)
    {
        _clientSettings = _clientSettingsSystem.Normalize(nextSettings);
        ApplyClientSettings(_clientSettings);
    }

    private void ApplyClientSettings(ClientSettings settings)
    {
        TranslationServer.SetLocale(settings.Language);
        ApplyDisplayMode(settings.IsFullscreen, settings.ResolutionWidth, settings.ResolutionHeight);
        ApplyContentZoom(settings.FontScale);
        ApplyAudioBusVolume(MasterAudioBusName, settings.MasterVolume);
        ApplyNamedBusVolume(settings.BgmVolume, "Music", "BGM", "Bgm");
        ApplyNamedBusVolume(settings.SfxVolume, "SFX", "Sfx", "UI", "Ui");
    }

    private static void ApplyDisplayMode(bool isFullscreen, int width, int height)
    {
        // 先声明窗口的最小可缩放边界，避免运行时被手动拖到 720p 以下造成布局溢出。
        DisplayServer.WindowSetMinSize(MinimumWindowSize);

        // 全屏时直接铺满当前屏幕；窗口化时再按所选分辨率回到居中窗口。
        if (isFullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            return;
        }

        var targetSize = new Vector2I(width, height);
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(targetSize);

        var currentScreen = DisplayServer.WindowGetCurrentScreen();
        var screenSize = DisplayServer.ScreenGetSize(currentScreen);
        var centeredPosition = (screenSize - targetSize) / 2;
        DisplayServer.WindowSetPosition(centeredPosition);
    }

    private void ApplyContentZoom(float contentZoom)
    {
        var window = GetWindow();
        if (window == null)
        {
            return;
        }

        // 在 stretch=disabled 下，缩放倍率只控制内容放大/缩小；更高分辨率则负责展示更多区域。
        window.ContentScaleFactor = contentZoom;
    }

    private static void ApplyNamedBusVolume(float volumeLinear, params string[] busNames)
    {
        foreach (var busName in busNames)
        {
            var busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex < 0)
            {
                continue;
            }

            ApplyAudioBusVolume(busIndex, volumeLinear);
            return;
        }
    }

    private static void ApplyAudioBusVolume(string busName, float volumeLinear)
    {
        var busIndex = AudioServer.GetBusIndex(busName);
        if (busIndex < 0)
        {
            return;
        }

        ApplyAudioBusVolume(busIndex, volumeLinear);
    }

    private static void ApplyAudioBusVolume(int busIndex, float volumeLinear)
    {
        if (busIndex < 0)
        {
            return;
        }

        var safeVolume = Mathf.Clamp(volumeLinear, 0.0f, 1.0f);
        var isMute = safeVolume <= MuteThreshold;
        AudioServer.SetBusMute(busIndex, isMute);
        AudioServer.SetBusVolumeDb(busIndex, isMute ? MuteDb : Mathf.LinearToDb(safeVolume));
    }

    private void UnbindClientSettingEvents()
    {
        if (_settingsButton != null)
        {
            _settingsButton.Pressed -= OpenSettingsPanel;
        }

        if (_settingsPanel == null)
        {
            return;
        }

        _settingsPanel.PreviewRequested -= OnClientSettingsPreviewRequested;
        _settingsPanel.ApplyRequested -= OnClientSettingsApplyRequested;
    }
}
