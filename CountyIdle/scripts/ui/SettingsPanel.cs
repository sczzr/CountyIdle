using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Core;
using CountyIdle.Models;

namespace CountyIdle.UI;

public partial class SettingsPanel : PopupPanelBase
{
    /// <summary>
    /// 机宜卷可滚动设置区路径；低分辨率下通过滚动承载完整条目。
    /// </summary>
    private const string SettingsRowsPath = "CenterLayer/Dialog/Margin/MainColumn/SettingsScroll/SettingsRows";

    private static readonly (string Code, string Label)[] LanguageOptions =
    {
        ("zh_CN", "简体中文"),
        ("en", "English")
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

    private PanelContainer _dialog = null!;
    private OptionButton _languageOption = null!;
    private CheckBox _fullscreenCheckBox = null!;
    private OptionButton _resolutionOption = null!;
    private HSlider _zoomSlider = null!;
    private Label _zoomValueLabel = null!;
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
    private Node? _visualFx;

    private readonly Dictionary<ShortcutAction, Button> _shortcutButtons = new();
    private readonly List<Vector2I> _resolutionOptions = new();
    private ClientSettings _editingSettings = new();
    private ShortcutAction _pendingShortcutAction = ShortcutAction.None;

    public event Action<ClientSettings>? PreviewRequested;
    public event Action<ClientSettings>? ApplyRequested;

    public override void _Ready()
    {
        _dialog = GetNode<PanelContainer>("CenterLayer/Dialog");
        _languageOption = GetNode<OptionButton>($"{SettingsRowsPath}/LanguageRow/LanguageOption");
        _fullscreenCheckBox = GetNode<CheckBox>($"{SettingsRowsPath}/FullscreenRow/FullscreenCheckBox");
        _resolutionOption = GetNode<OptionButton>($"{SettingsRowsPath}/ResolutionRow/ResolutionOption");
        _zoomSlider = GetNode<HSlider>($"{SettingsRowsPath}/ZoomRow/ZoomSlider");
        _zoomValueLabel = GetNode<Label>($"{SettingsRowsPath}/ZoomRow/ZoomValue");
        _volumeSlider = GetNode<HSlider>($"{SettingsRowsPath}/VolumeRow/VolumeSlider");
        _volumeValueLabel = GetNode<Label>($"{SettingsRowsPath}/VolumeRow/VolumeValue");
        _openSettingsKeyButton = GetNode<Button>($"{SettingsRowsPath}/OpenSettingsKeyRow/OpenSettingsKeyOption");
        _openWarehouseKeyButton = GetNode<Button>($"{SettingsRowsPath}/OpenWarehouseKeyRow/OpenWarehouseKeyOption");
        _toggleExplorationKeyButton = GetNode<Button>($"{SettingsRowsPath}/ToggleExplorationKeyRow/ToggleExplorationKeyOption");
        _toggleSpeedKeyButton = GetNode<Button>($"{SettingsRowsPath}/ToggleSpeedKeyRow/ToggleSpeedKeyOption");
        _quickSaveKeyButton = GetNode<Button>($"{SettingsRowsPath}/QuickSaveKeyRow/QuickSaveKeyOption");
        _quickLoadKeyButton = GetNode<Button>($"{SettingsRowsPath}/QuickLoadKeyRow/QuickLoadKeyOption");
        _quickResetKeyButton = GetNode<Button>($"{SettingsRowsPath}/QuickResetKeyRow/QuickResetKeyOption");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _cancelButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CancelButton");
        _applyButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/ApplyButton");
        _visualFx = GetNodeOrNull<Node>("VisualFx");

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
        // 每次打开设置卷时都重建分辨率候选，确保切换显示器后能读取当前屏幕上限。
        PopulateOptionItems();
        ApplySettingsToInputs(_editingSettings);
        OpenPopup();
        CallVisualFx("play_open");
    }

    public void ClosePanel()
    {
        ClosePopup();
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
        CallVisualFxForShortcut(_pendingShortcutAction);
        GetViewport().SetInputAsHandled();
    }

    private void PopulateOptionItems()
    {
        _languageOption.Clear();
        for (var i = 0; i < LanguageOptions.Length; i += 1)
        {
            _languageOption.AddItem(LanguageOptions[i].Label, i);
        }

        // 分辨率候选会结合当前屏幕上限动态生成，而不是只显示固定白名单。
        RebuildResolutionOptions();
        _resolutionOption.Clear();
        var currentScreenMaxResolution = ClientSettingsSystem.GetCurrentScreenMaxResolution();
        for (var i = 0; i < _resolutionOptions.Count; i += 1)
        {
            var option = _resolutionOptions[i];
            var optionLabel = option == currentScreenMaxResolution
                ? $"{option.X} × {option.Y}（当前屏幕上限）"
                : $"{option.X} × {option.Y}";
            _resolutionOption.AddItem(optionLabel, i);
        }

        _zoomSlider.MinValue = ClientSettingsSystem.MinContentZoom * 100.0f;
        _zoomSlider.MaxValue = ClientSettingsSystem.MaxContentZoom * 100.0f;
        _zoomSlider.Step = 5;

        _volumeSlider.MinValue = 0;
        _volumeSlider.MaxValue = 100;
        _volumeSlider.Step = 1;
    }

    /// <summary>
    /// 结合项目基础档位与当前屏幕上限，构建本次可选的分辨率列表。
    /// </summary>
    private void RebuildResolutionOptions()
    {
        _resolutionOptions.Clear();
        foreach (var resolution in ClientSettingsSystem.GetAvailableResolutions())
        {
            _resolutionOptions.Add(resolution);
        }
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
        _fullscreenCheckBox.Toggled += OnFullscreenToggled;
        _volumeSlider.ValueChanged += OnVolumeSliderChanged;
        _zoomSlider.ValueChanged += OnZoomSliderChanged;
        _resolutionOption.ItemSelected += OnResolutionSelected;

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
        _fullscreenCheckBox.ButtonPressed = settings.IsFullscreen;

        var zoomPercent = Mathf.Clamp(Mathf.RoundToInt(settings.FontScale * 100.0f),
            Mathf.RoundToInt(ClientSettingsSystem.MinContentZoom * 100.0f),
            Mathf.RoundToInt(ClientSettingsSystem.MaxContentZoom * 100.0f));
        _zoomSlider.Value = zoomPercent;
        _zoomValueLabel.Text = $"{zoomPercent}%";

        var volumePercent = Mathf.Clamp(Mathf.RoundToInt(settings.MasterVolume * 100.0f), 0, 100);
        _volumeSlider.Value = volumePercent;
        _volumeValueLabel.Text = $"{volumePercent}%";

        UpdateShortcutButtonTexts();
        RefreshPopupHint();
        CallVisualFxForShortcut(_pendingShortcutAction);
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
        if (_resolutionOptions.Count == 0)
        {
            return;
        }

        var selectedIndex = 0;
        var hasExactMatch = false;
        for (var i = 0; i < _resolutionOptions.Count; i += 1)
        {
            var option = _resolutionOptions[i];
            if (option.X == width && option.Y == height)
            {
                selectedIndex = i;
                hasExactMatch = true;
                break;
            }

            // 若旧设置超出当前屏幕上限，则自动退到“不超过目标值”的最高可用档。
            if (option.X <= width && option.Y <= height)
            {
                selectedIndex = i;
            }
        }

        _resolutionOption.Select(selectedIndex);
        if (!hasExactMatch)
        {
            var selectedResolution = _resolutionOptions[selectedIndex];
            _editingSettings.ResolutionWidth = selectedResolution.X;
            _editingSettings.ResolutionHeight = selectedResolution.Y;
        }
    }

    private void BeginShortcutCapture(ShortcutAction shortcutAction)
    {
        ClearPopupStatusMessage();
        _pendingShortcutAction = _pendingShortcutAction == shortcutAction ? ShortcutAction.None : shortcutAction;
        UpdateShortcutButtonTexts();
        RefreshPopupHint();
        CallVisualFxForShortcut(_pendingShortcutAction);
    }

    private void CancelShortcutCapture()
    {
        _pendingShortcutAction = ShortcutAction.None;
        UpdateShortcutButtonTexts();
        RefreshPopupHint();
        CallVisualFxForShortcut(_pendingShortcutAction);
    }

    private string AssignShortcut(ShortcutAction shortcutAction, string keyName)
    {
        var previousValue = GetShortcutValue(shortcutAction);
        var conflictedAction = FindShortcutActionByKey(keyName, shortcutAction);
        SetShortcutValue(shortcutAction, keyName);

        if (conflictedAction != ShortcutAction.None)
        {
            SetShortcutValue(conflictedAction, previousValue);
            return $"已将 {GetShortcutActionLabel(shortcutAction)} 改录为 {keyName}，并把 {GetShortcutActionLabel(conflictedAction)} 调换为 {previousValue}。";
        }

        return $"已将 {GetShortcutActionLabel(shortcutAction)} 改录为 {keyName}。";
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
                ? "录入符令…（Esc 作罢）"
                : GetShortcutValue(pair.Key);
        }
    }

    protected override string GetPopupHintText()
    {
        if (_pendingShortcutAction != ShortcutAction.None)
        {
            return "正在录入符令：请按下一枚按键；若与旧符相冲，会自动换置。按 Esc 可止录。";
        }

        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        if (_editingSettings.IsFullscreen)
        {
            return "当前为全屏：会直接铺满所在屏幕；分辨率用于记忆窗口化尺寸，缩放滑条负责放大/缩小画面内容。";
        }

        return "更高分辨率会显示更多内容而不是把画面放大；缩放滑条可单独调整画面内容大小。";
    }

    private string GetShortcutActionLabel(ShortcutAction shortcutAction)
    {
        return shortcutAction switch
        {
            ShortcutAction.OpenSettings => "启机宜卷",
            ShortcutAction.OpenWarehouse => "启库藏卷",
            ShortcutAction.ToggleExploration => "历练开关",
            ShortcutAction.ToggleSpeed => "流光切换",
            ShortcutAction.QuickSave => "速录主卷",
            ShortcutAction.QuickLoad => "速启主卷",
            ShortcutAction.QuickReset => "速归初局",
            _ => "符令"
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

    private void CallVisualFx(string methodName, params Variant[] args)
    {
        _visualFx?.Call(methodName, args);
    }

    private void CallVisualFxForShortcut(ShortcutAction shortcutAction)
    {
        if (shortcutAction == ShortcutAction.None)
        {
            return;
        }

        var buttonPath = shortcutAction switch
        {
            ShortcutAction.OpenSettings => $"{SettingsRowsPath}/OpenSettingsKeyRow/OpenSettingsKeyOption",
            ShortcutAction.OpenWarehouse => $"{SettingsRowsPath}/OpenWarehouseKeyRow/OpenWarehouseKeyOption",
            ShortcutAction.ToggleExploration => $"{SettingsRowsPath}/ToggleExplorationKeyRow/ToggleExplorationKeyOption",
            ShortcutAction.ToggleSpeed => $"{SettingsRowsPath}/ToggleSpeedKeyRow/ToggleSpeedKeyOption",
            ShortcutAction.QuickSave => $"{SettingsRowsPath}/QuickSaveKeyRow/QuickSaveKeyOption",
            ShortcutAction.QuickLoad => $"{SettingsRowsPath}/QuickLoadKeyRow/QuickLoadKeyOption",
            ShortcutAction.QuickReset => $"{SettingsRowsPath}/QuickResetKeyRow/QuickResetKeyOption",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(buttonPath))
        {
            CallVisualFx("pulse_shortcut", buttonPath);
        }
    }

    private void OnCloseRequested()
    {
        ClosePopup();
    }

    private void OnApplyPressed()
    {
        _editingSettings.Language = LanguageOptions[Mathf.Clamp(_languageOption.Selected, 0, LanguageOptions.Length - 1)].Code;
        _editingSettings.IsFullscreen = _fullscreenCheckBox.ButtonPressed;

        var selectedResolution = _resolutionOptions[Mathf.Clamp(_resolutionOption.Selected, 0, _resolutionOptions.Count - 1)];
        _editingSettings.ResolutionWidth = selectedResolution.X;
        _editingSettings.ResolutionHeight = selectedResolution.Y;
        _editingSettings.FontScale = Mathf.Clamp((float)_zoomSlider.Value / 100.0f,
            ClientSettingsSystem.MinContentZoom,
            ClientSettingsSystem.MaxContentZoom);
        _editingSettings.MasterVolume = Mathf.Clamp((float)_volumeSlider.Value / 100.0f, 0.0f, 1.0f);

        ApplyRequested?.Invoke(_editingSettings.Clone());
        ClosePopup();
    }

    private void OnVolumeSliderChanged(double value)
    {
        _volumeValueLabel.Text = $"{value:0}%";
        var nextVolume = Mathf.Clamp((float)value / 100.0f, 0.0f, 1.0f);
        if (Mathf.Abs(_editingSettings.MasterVolume - nextVolume) < 0.0001f)
        {
            return;
        }

        _editingSettings.MasterVolume = nextVolume;
        PreviewRequested?.Invoke(_editingSettings.Clone());
    }

    private void OnZoomSliderChanged(double value)
    {
        _zoomValueLabel.Text = $"{value:0}%";
        var nextZoom = Mathf.Clamp((float)value / 100.0f,
            ClientSettingsSystem.MinContentZoom,
            ClientSettingsSystem.MaxContentZoom);
        if (Mathf.Abs(_editingSettings.FontScale - nextZoom) < 0.0001f)
        {
            return;
        }

        _editingSettings.FontScale = nextZoom;
        PreviewRequested?.Invoke(_editingSettings.Clone());
    }

    private void OnFullscreenToggled(bool isButtonPressed)
    {
        if (_editingSettings.IsFullscreen == isButtonPressed)
        {
            return;
        }

        _editingSettings.IsFullscreen = isButtonPressed;
        RefreshPopupHint();
        PreviewRequested?.Invoke(_editingSettings.Clone());
    }

    private void OnResolutionSelected(long index)
    {
        if (_resolutionOptions.Count == 0)
        {
            return;
        }

        var selectedIndex = Mathf.Clamp((int)index, 0, _resolutionOptions.Count - 1);
        var selectedResolution = _resolutionOptions[selectedIndex];
        if (_editingSettings.ResolutionWidth == selectedResolution.X &&
            _editingSettings.ResolutionHeight == selectedResolution.Y)
        {
            return;
        }

        _editingSettings.ResolutionWidth = selectedResolution.X;
        _editingSettings.ResolutionHeight = selectedResolution.Y;
        PreviewRequested?.Invoke(_editingSettings.Clone());
    }

}
