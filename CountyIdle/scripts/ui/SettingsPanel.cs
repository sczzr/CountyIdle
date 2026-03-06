using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.UI;

public partial class SettingsPanel : Control
{
    private static readonly (string Code, string Label)[] LanguageOptions =
    {
        ("zh_CN", "简体中文"),
        ("en", "English")
    };

    private static readonly Vector2I[] ResolutionOptions =
    {
        new Vector2I(1280, 720),
        new Vector2I(1600, 900),
        new Vector2I(1920, 1080),
        new Vector2I(2560, 1440)
    };

    private static readonly float[] FontScaleOptions =
    {
        0.90f,
        1.00f,
        1.10f,
        1.20f,
        1.30f
    };

    private OptionButton _languageOption = null!;
    private OptionButton _resolutionOption = null!;
    private OptionButton _fontScaleOption = null!;
    private HSlider _volumeSlider = null!;
    private Label _volumeValueLabel = null!;
    private Button _closeButton = null!;
    private Button _cancelButton = null!;
    private Button _applyButton = null!;

    public event Action<ClientSettings>? ApplyRequested;

    public override void _Ready()
    {
        _languageOption = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/LanguageRow/LanguageOption");
        _resolutionOption = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/ResolutionRow/ResolutionOption");
        _fontScaleOption = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/FontScaleRow/FontScaleOption");
        _volumeSlider = GetNode<HSlider>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/VolumeRow/VolumeSlider");
        _volumeValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/VolumeRow/VolumeValue");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _cancelButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CancelButton");
        _applyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/ApplyButton");

        PopulateOptionItems();
        BindEvents();
        Hide();
    }

    public void Open(ClientSettings currentSettings)
    {
        ApplySettingsToInputs(currentSettings);
        Show();
    }

    private void BindEvents()
    {
        _closeButton.Pressed += Hide;
        _cancelButton.Pressed += Hide;
        _applyButton.Pressed += OnApplyPressed;
        _volumeSlider.ValueChanged += value => _volumeValueLabel.Text = $"{value:0}%";
    }

    private void PopulateOptionItems()
    {
        _languageOption.Clear();
        for (var i = 0; i < LanguageOptions.Length; i += 1)
        {
            _languageOption.AddItem(LanguageOptions[i].Label, i);
        }

        _resolutionOption.Clear();
        for (var i = 0; i < ResolutionOptions.Length; i += 1)
        {
            var option = ResolutionOptions[i];
            _resolutionOption.AddItem($"{option.X} × {option.Y}", i);
        }

        _fontScaleOption.Clear();
        for (var i = 0; i < FontScaleOptions.Length; i += 1)
        {
            _fontScaleOption.AddItem($"{Mathf.RoundToInt(FontScaleOptions[i] * 100.0f)}%", i);
        }

        _volumeSlider.MinValue = 0;
        _volumeSlider.MaxValue = 100;
        _volumeSlider.Step = 1;
    }

    private void ApplySettingsToInputs(ClientSettings settings)
    {
        SelectLanguage(settings.Language);
        SelectResolution(settings.ResolutionWidth, settings.ResolutionHeight);
        SelectFontScale(settings.FontScale);

        var volumePercent = Mathf.Clamp(Mathf.RoundToInt(settings.MasterVolume * 100.0f), 0, 100);
        _volumeSlider.Value = volumePercent;
        _volumeValueLabel.Text = $"{volumePercent}%";
    }

    private void SelectLanguage(string languageCode)
    {
        var selectedIndex = 0;
        for (var i = 0; i < LanguageOptions.Length; i += 1)
        {
            if (!string.Equals(LanguageOptions[i].Code, languageCode, StringComparison.Ordinal))
            {
                continue;
            }

            selectedIndex = i;
            break;
        }

        _languageOption.Select(selectedIndex);
    }

    private void SelectResolution(int width, int height)
    {
        var selectedIndex = 0;
        for (var i = 0; i < ResolutionOptions.Length; i += 1)
        {
            var option = ResolutionOptions[i];
            if (option.X != width || option.Y != height)
            {
                continue;
            }

            selectedIndex = i;
            break;
        }

        _resolutionOption.Select(selectedIndex);
    }

    private void SelectFontScale(float fontScale)
    {
        var selectedIndex = 0;
        var minimumDiff = float.MaxValue;

        for (var i = 0; i < FontScaleOptions.Length; i += 1)
        {
            var diff = Mathf.Abs(FontScaleOptions[i] - fontScale);
            if (diff >= minimumDiff)
            {
                continue;
            }

            minimumDiff = diff;
            selectedIndex = i;
        }

        _fontScaleOption.Select(selectedIndex);
    }

    private void OnApplyPressed()
    {
        var selectedLanguage = LanguageOptions[Mathf.Clamp(_languageOption.Selected, 0, LanguageOptions.Length - 1)].Code;
        var selectedResolution = ResolutionOptions[Mathf.Clamp(_resolutionOption.Selected, 0, ResolutionOptions.Length - 1)];
        var selectedFontScale = FontScaleOptions[Mathf.Clamp(_fontScaleOption.Selected, 0, FontScaleOptions.Length - 1)];
        var selectedVolume = Mathf.Clamp((float)_volumeSlider.Value / 100.0f, 0.0f, 1.0f);

        var settings = new ClientSettings
        {
            Language = selectedLanguage,
            ResolutionWidth = selectedResolution.X,
            ResolutionHeight = selectedResolution.Y,
            FontScale = selectedFontScale,
            MasterVolume = selectedVolume
        };

        ApplyRequested?.Invoke(settings);
        Hide();
    }
}
