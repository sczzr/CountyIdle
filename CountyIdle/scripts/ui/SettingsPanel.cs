using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.UI;

public partial class SettingsPanel : PopupPanelBase
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

    private enum ShortcutAction
    {
        None,
        OpenSettings,
        OpenWarehouse,
        ToggleExploration,
        ToggleSpeed,
        QuickSave,
        QuickLoad,
        QuickReset
    }

    private OptionButton _languageOption = null!;
    private OptionButton _resolutionOption = null!;
    private OptionButton _fontScaleOption = null!;
    private HSlider _volumeSlider = null!;
    private Label _volumeValueLabel = null!;
    private Button _openSettingsKeyButton = null!;
    private Button _openWarehouseKeyButton = null!;
    private Button _toggleExplorationKeyButton = null!;
    private Button _toggleSpeedKeyButton = null!;
    private Button _quickSaveKeyButton = null!;
    private Button _quickLoadKeyButton = null!;
    private Button _quickResetKeyButton = null!;
    private Button _closeButton = null!;
    private Button _cancelButton = null!;
    private Button _applyButton = null!;

    private readonly Dictionary<ShortcutAction, Button> _shortcutButtons = new();
    private ClientSettings _editingSettings = new();
    private ShortcutAction _pendingShortcutAction = ShortcutAction.None;

    public event Action<ClientSettings>? ApplyRequested;

    public override void _Ready()
    {
        _languageOption = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/LanguageRow/LanguageOption");
        _resolutionOption = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/ResolutionRow/ResolutionOption");
        _fontScaleOption = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/FontScaleRow/FontScaleOption");
        _volumeSlider = GetNode<HSlider>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/VolumeRow/VolumeSlider");
        _volumeValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/VolumeRow/VolumeValue");
        _openSettingsKeyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/OpenSettingsKeyRow/OpenSettingsKeyOption");
        _openWarehouseKeyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/OpenWarehouseKeyRow/OpenWarehouseKeyOption");
        _toggleExplorationKeyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/ToggleExplorationKeyRow/ToggleExplorationKeyOption");
        _toggleSpeedKeyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/ToggleSpeedKeyRow/ToggleSpeedKeyOption");
        _quickSaveKeyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/QuickSaveKeyRow/QuickSaveKeyOption");
        _quickLoadKeyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/QuickLoadKeyRow/QuickLoadKeyOption");
        _quickResetKeyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/SettingsRows/QuickResetKeyRow/QuickResetKeyOption");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _cancelButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CancelButton");
        _applyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/ApplyButton");

        InitializePopupHint("CenterLayer/Dialog/Margin/MainColumn/HintLabel");
        PopulateOptionItems();
        BuildShortcutButtonMap();
        BindEvents();
        Hide();
    }

    public void Open(ClientSettings currentSettings)
    {
        _editingSettings = currentSettings.Clone();
        _pendingShortcutAction = ShortcutAction.None;
        ApplySettingsToInputs(_editingSettings);
        OpenPopup();
    }

    public override void _Process(double delta)
    {
        TickPopupStatus(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible)
        {
            return;
        }

        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        if (_pendingShortcutAction == ShortcutAction.None)
        {
            if (!TryHandlePopupClose(keyEvent))
            {
                return;
            }

            GetViewport().SetInputAsHandled();
            return;
        }

        if (keyEvent.Keycode == Key.Escape)
        {
            CancelShortcutCapture();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (keyEvent.Keycode == Key.None)
        {
            return;
        }

        var statusMessage = AssignShortcut(_pendingShortcutAction, keyEvent.Keycode.ToString());
        _pendingShortcutAction = ShortcutAction.None;
        ShowPopupStatusMessage(statusMessage);
        UpdateShortcutButtonTexts();
        RefreshPopupHint();
        GetViewport().SetInputAsHandled();
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

    private void BuildShortcutButtonMap()
    {
        _shortcutButtons.Clear();
        _shortcutButtons[ShortcutAction.OpenSettings] = _openSettingsKeyButton;
        _shortcutButtons[ShortcutAction.OpenWarehouse] = _openWarehouseKeyButton;
        _shortcutButtons[ShortcutAction.ToggleExploration] = _toggleExplorationKeyButton;
        _shortcutButtons[ShortcutAction.ToggleSpeed] = _toggleSpeedKeyButton;
        _shortcutButtons[ShortcutAction.QuickSave] = _quickSaveKeyButton;
        _shortcutButtons[ShortcutAction.QuickLoad] = _quickLoadKeyButton;
        _shortcutButtons[ShortcutAction.QuickReset] = _quickResetKeyButton;
    }

    private void BindEvents()
    {
        _closeButton.Pressed += OnCloseRequested;
        _cancelButton.Pressed += OnCloseRequested;
        _applyButton.Pressed += OnApplyPressed;
        _volumeSlider.ValueChanged += value => _volumeValueLabel.Text = $"{value:0}%";

        _openSettingsKeyButton.Pressed += () => BeginShortcutCapture(ShortcutAction.OpenSettings);
        _openWarehouseKeyButton.Pressed += () => BeginShortcutCapture(ShortcutAction.OpenWarehouse);
        _toggleExplorationKeyButton.Pressed += () => BeginShortcutCapture(ShortcutAction.ToggleExploration);
        _toggleSpeedKeyButton.Pressed += () => BeginShortcutCapture(ShortcutAction.ToggleSpeed);
        _quickSaveKeyButton.Pressed += () => BeginShortcutCapture(ShortcutAction.QuickSave);
        _quickLoadKeyButton.Pressed += () => BeginShortcutCapture(ShortcutAction.QuickLoad);
        _quickResetKeyButton.Pressed += () => BeginShortcutCapture(ShortcutAction.QuickReset);
    }

    private void ApplySettingsToInputs(ClientSettings settings)
    {
        SelectLanguage(settings.Language);
        SelectResolution(settings.ResolutionWidth, settings.ResolutionHeight);
        SelectFontScale(settings.FontScale);

        var volumePercent = Mathf.Clamp(Mathf.RoundToInt(settings.MasterVolume * 100.0f), 0, 100);
        _volumeSlider.Value = volumePercent;
        _volumeValueLabel.Text = $"{volumePercent}%";

        UpdateShortcutButtonTexts();
        RefreshPopupHint();
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

    private void BeginShortcutCapture(ShortcutAction shortcutAction)
    {
        ClearPopupStatusMessage();
        _pendingShortcutAction = _pendingShortcutAction == shortcutAction ? ShortcutAction.None : shortcutAction;
        UpdateShortcutButtonTexts();
        RefreshPopupHint();
    }

    private void CancelShortcutCapture()
    {
        _pendingShortcutAction = ShortcutAction.None;
        UpdateShortcutButtonTexts();
        RefreshPopupHint();
    }

    private string AssignShortcut(ShortcutAction shortcutAction, string keyName)
    {
        var previousValue = GetShortcutValue(shortcutAction);
        var conflictedAction = FindShortcutActionByKey(keyName, shortcutAction);
        SetShortcutValue(shortcutAction, keyName);

        if (conflictedAction != ShortcutAction.None)
        {
            SetShortcutValue(conflictedAction, previousValue);
            return $"已绑定 {GetShortcutActionLabel(shortcutAction)} 为 {keyName}，并将 {GetShortcutActionLabel(conflictedAction)} 交换为 {previousValue}。";
        }

        return $"已绑定 {GetShortcutActionLabel(shortcutAction)} 为 {keyName}。";
    }

    private ShortcutAction FindShortcutActionByKey(string keyName, ShortcutAction ignoreAction)
    {
        foreach (var shortcutAction in _shortcutButtons.Keys)
        {
            if (shortcutAction == ignoreAction)
            {
                continue;
            }

            if (string.Equals(GetShortcutValue(shortcutAction), keyName, StringComparison.OrdinalIgnoreCase))
            {
                return shortcutAction;
            }
        }

        return ShortcutAction.None;
    }

    private void UpdateShortcutButtonTexts()
    {
        foreach (var pair in _shortcutButtons)
        {
            pair.Value.Text = _pendingShortcutAction == pair.Key
                ? "按任意键...（Esc 取消）"
                : GetShortcutValue(pair.Key);
        }
    }

    protected override string GetPopupHintText()
    {
        if (_pendingShortcutAction != ShortcutAction.None)
        {
            return "正在录制快捷键：请按下一个按键，若与现有动作冲突会自动交换。按 Esc 取消录制。";
        }

        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return "调整语言、分辨率、字体大小、声音与快捷键。点击快捷键按钮后按任意键录制，点击“应用”后立即生效并自动保存。";
    }

    private string GetShortcutActionLabel(ShortcutAction shortcutAction)
    {
        return shortcutAction switch
        {
            ShortcutAction.OpenSettings => "打开设置",
            ShortcutAction.OpenWarehouse => "打开仓储",
            ShortcutAction.ToggleExploration => "探险开关",
            ShortcutAction.ToggleSpeed => "倍速切换",
            ShortcutAction.QuickSave => "快速存档",
            ShortcutAction.QuickLoad => "快速读档",
            ShortcutAction.QuickReset => "快速重置",
            _ => "快捷键"
        };
    }

    protected override void OnPopupClosing()
    {
        CancelShortcutCapture();
    }

    private string GetShortcutValue(ShortcutAction shortcutAction)
    {
        return shortcutAction switch
        {
            ShortcutAction.OpenSettings => _editingSettings.OpenSettingsKey,
            ShortcutAction.OpenWarehouse => _editingSettings.OpenWarehouseKey,
            ShortcutAction.ToggleExploration => _editingSettings.ToggleExplorationKey,
            ShortcutAction.ToggleSpeed => _editingSettings.ToggleSpeedKey,
            ShortcutAction.QuickSave => _editingSettings.QuickSaveKey,
            ShortcutAction.QuickLoad => _editingSettings.QuickLoadKey,
            ShortcutAction.QuickReset => _editingSettings.QuickResetKey,
            _ => string.Empty
        };
    }

    private void SetShortcutValue(ShortcutAction shortcutAction, string keyName)
    {
        switch (shortcutAction)
        {
            case ShortcutAction.OpenSettings:
                _editingSettings.OpenSettingsKey = keyName;
                break;
            case ShortcutAction.OpenWarehouse:
                _editingSettings.OpenWarehouseKey = keyName;
                break;
            case ShortcutAction.ToggleExploration:
                _editingSettings.ToggleExplorationKey = keyName;
                break;
            case ShortcutAction.ToggleSpeed:
                _editingSettings.ToggleSpeedKey = keyName;
                break;
            case ShortcutAction.QuickSave:
                _editingSettings.QuickSaveKey = keyName;
                break;
            case ShortcutAction.QuickLoad:
                _editingSettings.QuickLoadKey = keyName;
                break;
            case ShortcutAction.QuickReset:
                _editingSettings.QuickResetKey = keyName;
                break;
        }
    }

    private void OnCloseRequested()
    {
        ClosePopup();
    }

    private void OnApplyPressed()
    {
        _editingSettings.Language = LanguageOptions[Mathf.Clamp(_languageOption.Selected, 0, LanguageOptions.Length - 1)].Code;

        var selectedResolution = ResolutionOptions[Mathf.Clamp(_resolutionOption.Selected, 0, ResolutionOptions.Length - 1)];
        _editingSettings.ResolutionWidth = selectedResolution.X;
        _editingSettings.ResolutionHeight = selectedResolution.Y;
        _editingSettings.FontScale = FontScaleOptions[Mathf.Clamp(_fontScaleOption.Selected, 0, FontScaleOptions.Length - 1)];
        _editingSettings.MasterVolume = Mathf.Clamp((float)_volumeSlider.Value / 100.0f, 0.0f, 1.0f);

        ApplyRequested?.Invoke(_editingSettings.Clone());
        ClosePopup();
    }
}
