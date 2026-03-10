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
        _settingsPanel.PreviewRequested += OnClientSettingsPreviewRequested;
        _settingsPanel.ApplyRequested += OnClientSettingsApplyRequested;
        AddChild(_settingsPanel);
        MoveChild(_settingsPanel, GetChildCount() - 1);
    }

    private void BindSettingsButtonEvent()
    {
        _settingsButton.Pressed += OpenSettingsPanel;
    }

    private void OpenSettingsPanel()
    {
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
        ApplyResolution(settings.ResolutionWidth, settings.ResolutionHeight);
        ApplyFontScale(settings.FontScale);
        ApplyMasterVolume(settings.MasterVolume);
    }

    private static void ApplyResolution(int width, int height)
    {
        var targetSize = new Vector2I(width, height);
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(targetSize);

        var currentScreen = DisplayServer.WindowGetCurrentScreen();
        var screenSize = DisplayServer.ScreenGetSize(currentScreen);
        var centeredPosition = (screenSize - targetSize) / 2;
        DisplayServer.WindowSetPosition(centeredPosition);
    }

    private void ApplyFontScale(float fontScale)
    {
        var window = GetWindow();
        if (window == null)
        {
            return;
        }

        window.ContentScaleFactor = fontScale;
    }

    private static void ApplyMasterVolume(float volumeLinear)
    {
        var busIndex = AudioServer.GetBusIndex(MasterAudioBusName);
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
