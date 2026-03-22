using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using CountyIdle.Core;
using CountyIdle.Models;

namespace CountyIdle.UI;

public partial class SettingsPanel : PopupPanelBase
{
	private const string RootRowPath = "PaperRoot/RootRow";
	private const string ContentColumnPath = RootRowPath + "/ContentColumn";
	private const string AudioPagePath = ContentColumnPath + "/ContentScroll/ContentPages/AudioPage";
	private const string AudioSettingsPath = AudioPagePath + "/AudioSettings";
	private const string ShortcutPagePath = ContentColumnPath + "/ContentScroll/ContentPages/ShortcutPage";
	private const string ShortcutSettingsPath = ShortcutPagePath + "/ShortcutSettings";
	private const string ShortcutVisualGridPath = ShortcutPagePath + "/ShortcutVisualGrid";
	private const string LanguagePagePath = ContentColumnPath + "/ContentScroll/ContentPages/LanguagePage";
	private const string LanguageSettingsPath = LanguagePagePath + "/LanguageSettings";
	private const string ShortcutPreviewGridPath = AudioPagePath + "/ShortcutPreviewGrid";

	private static readonly (string Code, string Label)[] LanguageOptions =
	{
		("zh_CN", "华夏正音"),
		("en", "异邦番语")
	};

	private static readonly (string Code, string Label)[] QualityOptions =
	{
		("low", "凡胎"),
		("medium", "结丹"),
		("high", "大乘")
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

	private enum SettingsTab
	{
		AudioVisual,
		Shortcuts,
		Language
	}

	private Control _frame = null!;
	private Button _tabAudioVisual = null!;
	private Button _tabShortcuts = null!;
	private Button _tabLanguage = null!;
	private VBoxContainer _audioSettings = null!;
	private Control _audioPage = null!;
	private Control _shortcutPage = null!;
	private Control _languagePage = null!;
	private Control _volumeRow = null!;
	private Control _fullscreenRow = null!;
	private Control _qualityRow = null!;
	private Control _resolutionRow = null!;
	private Control _bgmRow = null!;
	private Control _sfxRow = null!;
	private Control _zoomRow = null!;
	private Control _shortcutPreviewGrid = null!;
	private Label _titleLabel = null!;
	private Label _subtitleLabel = null!;
	private OptionButton _languageOption = null!;
	private Button _fullscreenSealButton = null!;
	private Button _autoSaveSealButton = null!;
	private Button _damageTextSealButton = null!;
	private Button _bloodGoreSealButton = null!;
	private OptionButton _resolutionOption = null!;
	private OptionButton _qualityOption = null!;
	private OptionButton _autoSaveIntervalOption = null!;
	private Button _qualityLowButton = null!;
	private Button _qualityMediumButton = null!;
	private Button _qualityHighButton = null!;
	private Button _resolution1080Button = null!;
	private Button _resolution2KButton = null!;
	private Button _resolution4KButton = null!;
	private Button _languageZhButton = null!;
	private Button _languageEnButton = null!;
	private HSlider _zoomSlider = null!;
	private Label _zoomValueLabel = null!;
	private HSlider _masterVolumeSlider = null!;
	private Label _masterVolumeValueLabel = null!;
	private HSlider _bgmVolumeSlider = null!;
	private Label _bgmVolumeValueLabel = null!;
	private HSlider _sfxVolumeSlider = null!;
	private Label _sfxVolumeValueLabel = null!;
	private Label _summarySettingsValueLabel = null!;
	private Label _summaryWarehouseValueLabel = null!;
	private Label _summarySpeedValueLabel = null!;
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
	private readonly List<Button> _qualityChoiceButtons = new();
	private readonly List<Button> _resolutionChoiceButtons = new();
	private readonly List<Button> _languageChoiceButtons = new();
	private readonly List<Vector2I> _resolutionOptions = new();
	private readonly List<int> _autoSaveIntervalOptions = new();
	private ClientSettings _editingSettings = new();
	private ShortcutAction _pendingShortcutAction = ShortcutAction.None;
	private SettingsTab _currentTab = SettingsTab.AudioVisual;

	public event Action<ClientSettings>? PreviewRequested;
	public event Action<ClientSettings>? ApplyRequested;

	public override void _Ready()
	{
		// 确保设置卷轴覆盖全屏，避免初始帧尺寸未正确拉伸导致露底。
		AnchorLeft = 0;
		AnchorTop = 0;
		AnchorRight = 1;
		AnchorBottom = 1;
		OffsetLeft = 0;
		OffsetTop = 0;
		OffsetRight = 0;
		OffsetBottom = 0;

		_frame = GetNode<Control>("PaperRoot");
		_tabAudioVisual = GetNode<Button>($"{RootRowPath}/NavColumn/TabAudioVisual");
		_tabShortcuts = GetNode<Button>($"{RootRowPath}/NavColumn/TabShortcuts");
		_tabLanguage = GetNode<Button>($"{RootRowPath}/NavColumn/TabLanguage");
		_audioSettings = GetNode<VBoxContainer>(AudioSettingsPath);
		_audioPage = GetNode<Control>(AudioPagePath);
		_shortcutPage = GetNode<Control>(ShortcutPagePath);
		_languagePage = GetNode<Control>(LanguagePagePath);
		_volumeRow = GetNode<Control>($"{AudioSettingsPath}/VolumeRow");
		_fullscreenRow = GetNode<Control>($"{AudioSettingsPath}/FullscreenRow");
		_qualityRow = GetNode<Control>($"{AudioSettingsPath}/QualityRow");
		_resolutionRow = GetNode<Control>($"{AudioSettingsPath}/ResolutionRow");
		_bgmRow = GetNode<Control>($"{AudioSettingsPath}/BgmRow");
		_sfxRow = GetNode<Control>($"{AudioSettingsPath}/SfxRow");
		_zoomRow = GetNode<Control>($"{AudioSettingsPath}/ZoomRow");
		_shortcutPreviewGrid = GetNode<Control>(ShortcutPreviewGridPath);
		_titleLabel = GetNode<Label>($"{ContentColumnPath}/HeaderRow/HeadingColumn/TitleLabel");
		_subtitleLabel = GetNode<Label>($"{ContentColumnPath}/HeaderRow/HeadingColumn/SubtitleLabel");
		_languageOption = GetNode<OptionButton>($"{LanguageSettingsPath}/LanguageRow/LanguageOption");
		_fullscreenSealButton = GetNode<Button>($"{AudioSettingsPath}/FullscreenRow/FullscreenSealButton");
		_autoSaveSealButton = GetNode<Button>($"{LanguageSettingsPath}/AutoSaveRow/AutoSaveSealButton");
		_damageTextSealButton = GetNode<Button>($"{LanguageSettingsPath}/DamageTextRow/DamageTextSealButton");
		_bloodGoreSealButton = GetNode<Button>($"{LanguageSettingsPath}/BloodGoreRow/BloodGoreSealButton");
		_resolutionOption = GetNode<OptionButton>($"{AudioSettingsPath}/ResolutionRow/ResolutionOption");
		_qualityOption = GetNode<OptionButton>($"{AudioSettingsPath}/QualityRow/QualityOption");
		_autoSaveIntervalOption = GetNode<OptionButton>($"{LanguageSettingsPath}/AutoSaveIntervalRow/AutoSaveIntervalOption");
		_qualityLowButton = GetNode<Button>($"{AudioSettingsPath}/QualityRow/QualityChoices/QualityLowButton");
		_qualityMediumButton = GetNode<Button>($"{AudioSettingsPath}/QualityRow/QualityChoices/QualityMediumButton");
		_qualityHighButton = GetNode<Button>($"{AudioSettingsPath}/QualityRow/QualityChoices/QualityHighButton");
		_resolution1080Button = GetNode<Button>($"{AudioSettingsPath}/ResolutionRow/ResolutionChoices/Resolution1080Button");
		_resolution2KButton = GetNode<Button>($"{AudioSettingsPath}/ResolutionRow/ResolutionChoices/Resolution2KButton");
		_resolution4KButton = GetNode<Button>($"{AudioSettingsPath}/ResolutionRow/ResolutionChoices/Resolution4KButton");
		_languageZhButton = GetNode<Button>($"{LanguageSettingsPath}/LanguageRow/LanguageChoices/LanguageZhButton");
		_languageEnButton = GetNode<Button>($"{LanguageSettingsPath}/LanguageRow/LanguageChoices/LanguageEnButton");
		_zoomSlider = GetNode<HSlider>($"{AudioSettingsPath}/ZoomRow/ZoomSlider");
		_zoomValueLabel = GetNode<Label>($"{AudioSettingsPath}/ZoomRow/ZoomValue");
		_masterVolumeSlider = GetNode<HSlider>($"{AudioSettingsPath}/VolumeRow/VolumeSlider");
		_masterVolumeValueLabel = GetNode<Label>($"{AudioSettingsPath}/VolumeRow/VolumeValue");
		_bgmVolumeSlider = GetNode<HSlider>($"{AudioSettingsPath}/BgmRow/BgmSlider");
		_bgmVolumeValueLabel = GetNode<Label>($"{AudioSettingsPath}/BgmRow/BgmValue");
		_sfxVolumeSlider = GetNode<HSlider>($"{AudioSettingsPath}/SfxRow/SfxSlider");
		_sfxVolumeValueLabel = GetNode<Label>($"{AudioSettingsPath}/SfxRow/SfxValue");
		_summarySettingsValueLabel = GetNode<Label>($"{ShortcutPreviewGridPath}/SummarySettingsCard/CardMargin/CardColumn/SummarySettingsValue");
		_summaryWarehouseValueLabel = GetNode<Label>($"{ShortcutPreviewGridPath}/SummaryWarehouseCard/CardMargin/CardColumn/SummaryWarehouseValue");
		_summarySpeedValueLabel = GetNode<Label>($"{ShortcutPreviewGridPath}/SummarySpeedCard/CardMargin/CardColumn/SummarySpeedValue");
		_openSettingsKeyButton = GetNode<Button>($"{ShortcutSettingsPath}/OpenSettingsKeyRow/OpenSettingsKeyOption");
		_openWarehouseKeyButton = GetNode<Button>($"{ShortcutSettingsPath}/OpenWarehouseKeyRow/OpenWarehouseKeyOption");
		_toggleExplorationKeyButton = GetNode<Button>($"{ShortcutSettingsPath}/ToggleExplorationKeyRow/ToggleExplorationKeyOption");
		_toggleSpeedKeyButton = GetNode<Button>($"{ShortcutSettingsPath}/ToggleSpeedKeyRow/ToggleSpeedKeyOption");
		_quickSaveKeyButton = GetNode<Button>($"{ShortcutSettingsPath}/QuickSaveKeyRow/QuickSaveKeyOption");
		_quickLoadKeyButton = GetNode<Button>($"{ShortcutSettingsPath}/QuickLoadKeyRow/QuickLoadKeyOption");
		_quickResetKeyButton = GetNode<Button>($"{ShortcutSettingsPath}/QuickResetKeyRow/QuickResetKeyOption");
		_closeButton = GetNode<Button>($"{ContentColumnPath}/HeaderRow/CloseButton");
		_cancelButton = GetNode<Button>($"{ContentColumnPath}/FooterRow/CancelButton");
		_applyButton = GetNode<Button>($"{ContentColumnPath}/FooterRow/ApplyButton");
		_visualFx = GetNodeOrNull<Node>("VisualFx");

		InitializePopupHint($"{ContentColumnPath}/HintLabel");
		PopulateOptionItems();
		BuildShortcutButtonMap();
		BuildChoiceButtonGroups();
		ConfigureReferenceMatchedAudioLayout();
		BindEvents();
		SwitchTab(SettingsTab.AudioVisual, animate: false);
		Hide();
	}

	public void Open(ClientSettings currentSettings)
	{
		_editingSettings = currentSettings.Clone();
		_pendingShortcutAction = ShortcutAction.None;
		PopulateOptionItems();
		ApplySettingsToInputs(_editingSettings);
		SwitchTab(SettingsTab.AudioVisual, animate: false);
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

		var capturingAction = _pendingShortcutAction;
		var statusMessage = AssignShortcut(capturingAction, keyEvent.Keycode.ToString());
		_pendingShortcutAction = ShortcutAction.None;
		ShowPopupStatusMessage(statusMessage);
		UpdateShortcutButtonTexts();
		RefreshPopupHint();
		CallVisualFxForShortcut(capturingAction);
		GetViewport().SetInputAsHandled();
	}

	private void PopulateOptionItems()
	{
		_languageOption.Clear();
		for (var i = 0; i < LanguageOptions.Length; i += 1)
		{
			_languageOption.AddItem(LanguageOptions[i].Label, i);
		}

		_qualityOption.Clear();
		for (var i = 0; i < QualityOptions.Length; i += 1)
		{
			_qualityOption.AddItem(QualityOptions[i].Label, i);
		}

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

		_autoSaveIntervalOptions.Clear();
		_autoSaveIntervalOption.Clear();
		foreach (var option in ClientSettingsSystem.GetAutoSaveIntervalOptions())
		{
			_autoSaveIntervalOptions.Add(option);
			_autoSaveIntervalOption.AddItem($"每 {option} 次时辰结算", option);
		}

		_zoomSlider.MinValue = ClientSettingsSystem.MinContentZoom * 100.0f;
		_zoomSlider.MaxValue = ClientSettingsSystem.MaxContentZoom * 100.0f;
		_zoomSlider.Step = 5;

		ConfigureVolumeSlider(_masterVolumeSlider);
		ConfigureVolumeSlider(_bgmVolumeSlider);
		ConfigureVolumeSlider(_sfxVolumeSlider);
	}

	private static void ConfigureVolumeSlider(Godot.Range slider)
	{
		slider.MinValue = 0;
		slider.MaxValue = 100;
		slider.Step = 1;
	}

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
		_shortcutButtons[ShortcutAction.OpenSettings] = GetNode<Button>($"{ShortcutVisualGridPath}/UiSection/UiGrid/OpenSettingsCard/Row/OpenSettingsKeyOption");
		_shortcutButtons[ShortcutAction.OpenWarehouse] = GetNode<Button>($"{ShortcutVisualGridPath}/UiSection/UiGrid/OpenWarehouseCard/Row/OpenWarehouseKeyOption");
		_shortcutButtons[ShortcutAction.ToggleExploration] = GetNode<Button>($"{ShortcutVisualGridPath}/MovementSection/MovementGrid/ToggleExplorationCard/Row/ToggleExplorationKeyOption");
		_shortcutButtons[ShortcutAction.ToggleSpeed] = GetNode<Button>($"{ShortcutVisualGridPath}/MovementSection/MovementGrid/ToggleSpeedCard/Row/ToggleSpeedKeyOption");
		_shortcutButtons[ShortcutAction.QuickSave] = _quickSaveKeyButton;
		_shortcutButtons[ShortcutAction.QuickLoad] = _quickLoadKeyButton;
		_shortcutButtons[ShortcutAction.QuickReset] = GetNode<Button>($"{ShortcutVisualGridPath}/UiSection/UiGrid/QuickResetCard/Row/QuickResetKeyOption");
	}

	private void BuildChoiceButtonGroups()
	{
		_qualityChoiceButtons.Clear();
		_qualityChoiceButtons.Add(_qualityLowButton);
		_qualityChoiceButtons.Add(_qualityMediumButton);
		_qualityChoiceButtons.Add(_qualityHighButton);

		_resolutionChoiceButtons.Clear();
		_resolutionChoiceButtons.Add(_resolution1080Button);
		_resolutionChoiceButtons.Add(_resolution2KButton);
		_resolutionChoiceButtons.Add(_resolution4KButton);

		_languageChoiceButtons.Clear();
		_languageChoiceButtons.Add(_languageZhButton);
		_languageChoiceButtons.Add(_languageEnButton);
	}

	private void BindEvents()
	{
		_tabAudioVisual.Pressed += () => SwitchTab(SettingsTab.AudioVisual);
		_tabShortcuts.Pressed += () => SwitchTab(SettingsTab.Shortcuts);
		_tabLanguage.Pressed += () => SwitchTab(SettingsTab.Language);
		_closeButton.Pressed += OnCloseRequested;
		_cancelButton.Pressed += OnCloseRequested;
		_applyButton.Pressed += OnApplyPressed;
		_fullscreenSealButton.Toggled += OnFullscreenToggled;
		_autoSaveSealButton.Toggled += OnAutoSaveToggled;
		_damageTextSealButton.Toggled += OnDamageTextToggled;
		_bloodGoreSealButton.Toggled += OnBloodGoreToggled;
		_masterVolumeSlider.ValueChanged += value => OnVolumeSliderChanged(value, VolumeField.Sfx);
		_bgmVolumeSlider.ValueChanged += value => OnVolumeSliderChanged(value, VolumeField.Bgm);
		_sfxVolumeSlider.ValueChanged += value => OnVolumeSliderChanged(value, VolumeField.Sfx);
		_zoomSlider.ValueChanged += OnZoomSliderChanged;
		_resolutionOption.ItemSelected += OnResolutionSelected;
		_qualityOption.ItemSelected += OnQualitySelected;
		_autoSaveIntervalOption.ItemSelected += OnAutoSaveIntervalSelected;
		_qualityLowButton.Pressed += () => SelectQualityByIndex(0);
		_qualityMediumButton.Pressed += () => SelectQualityByIndex(1);
		_qualityHighButton.Pressed += () => SelectQualityByIndex(2);
		_resolution1080Button.Pressed += () => SelectResolutionChoiceByIndex(0);
		_resolution2KButton.Pressed += () => SelectResolutionChoiceByIndex(1);
		_resolution4KButton.Pressed += () => SelectResolutionChoiceByIndex(2);
		_languageZhButton.Pressed += () => SelectLanguageByIndex(0);
		_languageEnButton.Pressed += () => SelectLanguageByIndex(1);

		foreach (var pair in _shortcutButtons)
		{
			var action = pair.Key;
			pair.Value.Pressed += () => BeginShortcutCapture(action);
		}
	}

	private enum VolumeField
	{
		Master,
		Bgm,
		Sfx
	}

	/// <summary>
	/// 按参考图收口音画页：只显示四条主项，并把画质行放在分辨率之前。
	/// 隐藏旧控件但不删除节点，以保持现有脚本路径、设置存档和回滚空间稳定。
	/// </summary>
	private void ConfigureReferenceMatchedAudioLayout()
	{
		_bgmRow.Visible = false;
		_sfxRow.Visible = false;
		_zoomRow.Visible = false;
		_shortcutPreviewGrid.Visible = false;

		_audioSettings.MoveChild(_volumeRow, 0);
		_audioSettings.MoveChild(_fullscreenRow, 1);
		_audioSettings.MoveChild(_qualityRow, 2);
		_audioSettings.MoveChild(_resolutionRow, 3);
	}

	private void ApplySettingsToInputs(ClientSettings settings)
	{
		SelectLanguage(settings.Language);
		SelectResolution(settings.ResolutionWidth, settings.ResolutionHeight);
		SelectQuality(settings.GraphicsQualityPreset);
		SelectAutoSaveInterval(settings.AutoSaveInterval);
		_fullscreenSealButton.ButtonPressed = settings.IsFullscreen;
		_autoSaveSealButton.ButtonPressed = settings.AutoSaveEnabled;
		_damageTextSealButton.ButtonPressed = settings.ShowDamageText;
		_bloodGoreSealButton.ButtonPressed = settings.EnableBloodGore;

		var zoomPercent = Mathf.Clamp(Mathf.RoundToInt(settings.FontScale * 100.0f),
			Mathf.RoundToInt(ClientSettingsSystem.MinContentZoom * 100.0f),
			Mathf.RoundToInt(ClientSettingsSystem.MaxContentZoom * 100.0f));
		_zoomSlider.Value = zoomPercent;
		UpdateZoomDisplay(zoomPercent);

		var sfxVolumePercent = Mathf.Clamp(Mathf.RoundToInt(settings.SfxVolume * 100.0f), 0, 100);
		_masterVolumeSlider.Value = sfxVolumePercent;
		UpdateVolumeDisplay(_masterVolumeValueLabel, sfxVolumePercent);

		var bgmVolumePercent = Mathf.Clamp(Mathf.RoundToInt(settings.BgmVolume * 100.0f), 0, 100);
		_bgmVolumeSlider.Value = bgmVolumePercent;
		UpdateVolumeDisplay(_bgmVolumeValueLabel, bgmVolumePercent);

		_sfxVolumeSlider.Value = sfxVolumePercent;
		UpdateVolumeDisplay(_sfxVolumeValueLabel, sfxVolumePercent);

		UpdateShortcutButtonTexts();
		RefreshLanguagePageState();
		RefreshPopupHint();
		CallVisualFx("sync_fullscreen_seal", _fullscreenSealButton.ButtonPressed);
		CallVisualFx("sync_toggle_seal", _autoSaveSealButton.GetPath().ToString(), _autoSaveSealButton.ButtonPressed);
		CallVisualFx("sync_toggle_seal", _damageTextSealButton.GetPath().ToString(), _damageTextSealButton.ButtonPressed);
		CallVisualFx("sync_toggle_seal", _bloodGoreSealButton.GetPath().ToString(), _bloodGoreSealButton.ButtonPressed);
		RefreshChoiceButtonStates();
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
		RefreshChoiceButtonStates();
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

		RefreshChoiceButtonStates();
	}
	private void SwitchTab(SettingsTab tab, bool animate = true)
	{
		if (_currentTab == tab && _audioPage.Visible == (tab == SettingsTab.AudioVisual))
		{
			UpdateHeading(tab);
			CallVisualFx("apply_tab_button_state", GetTabVisualName(tab));
			return;
		}

		if (_pendingShortcutAction != ShortcutAction.None)
		{
			CancelShortcutCapture();
		}

		_currentTab = tab;
		_audioPage.Visible = tab == SettingsTab.AudioVisual;
		_shortcutPage.Visible = tab == SettingsTab.Shortcuts;
		_languagePage.Visible = tab == SettingsTab.Language;
		UpdateHeading(tab);
		CallVisualFx("apply_tab_button_state", GetTabVisualName(tab));
		if (animate)
		{
			CallVisualFx("play_tab_switch", GetPagePath(tab));
		}
	}

	private void SelectQuality(string qualityCode)
	{
		var selectedIndex = 0;
		for (var i = 0; i < QualityOptions.Length; i += 1)
		{
			if (!string.Equals(QualityOptions[i].Code, qualityCode, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			selectedIndex = i;
			break;
		}

		_qualityOption.Select(selectedIndex);
		RefreshChoiceButtonStates();
	}

	private void SelectAutoSaveInterval(int interval)
	{
		var selectedIndex = 0;
		for (var i = 0; i < _autoSaveIntervalOptions.Count; i += 1)
		{
			if (_autoSaveIntervalOptions[i] != interval)
			{
				continue;
			}

			selectedIndex = i;
			break;
		}

		_autoSaveIntervalOption.Select(selectedIndex);
	}

	private void SelectLanguageByIndex(int selectedIndex)
	{
		_languageOption.Select(Mathf.Clamp(selectedIndex, 0, LanguageOptions.Length - 1));
		_editingSettings.Language = LanguageOptions[Mathf.Clamp(selectedIndex, 0, LanguageOptions.Length - 1)].Code;
		RefreshChoiceButtonStates();
		RefreshPopupHint();
	}

	private void SelectQualityByIndex(int selectedIndex)
	{
		_qualityOption.Select(Mathf.Clamp(selectedIndex, 0, QualityOptions.Length - 1));
		_editingSettings.GraphicsQualityPreset = QualityOptions[Mathf.Clamp(selectedIndex, 0, QualityOptions.Length - 1)].Code;
		RefreshChoiceButtonStates();
		RefreshPopupHint();
	}

	private void SelectResolutionChoiceByIndex(int selectedIndex)
	{
		if (_resolutionOptions.Count == 0)
		{
			return;
		}

		var safeIndex = selectedIndex switch
		{
			0 => FindBestResolutionIndex(1920, 1080),
			1 => FindBestResolutionIndex(2560, 1440),
			2 => _resolutionOptions.Count - 1,
			_ => FindBestResolutionIndex(1920, 1080)
		};
		_resolutionOption.Select(safeIndex);
		var selectedResolution = _resolutionOptions[safeIndex];
		_editingSettings.ResolutionWidth = selectedResolution.X;
		_editingSettings.ResolutionHeight = selectedResolution.Y;
		RefreshChoiceButtonStates();
		PreviewRequested?.Invoke(_editingSettings.Clone());
	}

	private void RefreshChoiceButtonStates()
	{
		ApplyChoiceButtonState(_languageChoiceButtons, _languageOption.Selected);
		ApplyChoiceButtonState(_qualityChoiceButtons, _qualityOption.Selected);

		var resolutionSelectedIndex = ResolveResolutionChoiceState(_editingSettings.ResolutionWidth, _editingSettings.ResolutionHeight);
		ApplyChoiceButtonState(_resolutionChoiceButtons, resolutionSelectedIndex);
	}

	private void ApplyChoiceButtonState(IReadOnlyList<Button> buttons, int selectedIndex)
	{
		for (var i = 0; i < buttons.Count; i += 1)
		{
			CallVisualFx("sync_choice_button", buttons[i].GetPath().ToString(), i == selectedIndex);
		}
	}

	private int FindBestResolutionIndex(int targetWidth, int targetHeight)
	{
		var bestIndex = 0;
		for (var i = 0; i < _resolutionOptions.Count; i += 1)
		{
			var option = _resolutionOptions[i];
			if (option.X == targetWidth && option.Y == targetHeight)
			{
				return i;
			}

			if (option.X <= targetWidth && option.Y <= targetHeight)
			{
				bestIndex = i;
			}
		}

		return Mathf.Clamp(bestIndex, 0, _resolutionOptions.Count - 1);
	}

	private static int ResolveResolutionChoiceState(int width, int height)
	{
		if (width >= 2560 && height >= 1440)
		{
			return 2;
		}

		if (width >= 1920 && height >= 1080)
		{
			return 1;
		}

		return 0;
	}

	private void UpdateHeading(SettingsTab tab)
	{
		switch (tab)
		{
			case SettingsTab.AudioVisual:
				_titleLabel.Text = "天衍機宜";
				_subtitleLabel.Text = "SECRETS AND STRATEGIES OF TIANYAN";
				break;
			case SettingsTab.Shortcuts:
				_titleLabel.Text = "御物之道";
				_subtitleLabel.Text = "THE WAY OF CONTROL AND BINDING";
				break;
			case SettingsTab.Language:
				_titleLabel.Text = "言辞教化";
				_subtitleLabel.Text = "WORDS AND CULTIVATION OF THE MORTAL WORLD";
				break;
		}
	}

	private void BeginShortcutCapture(ShortcutAction shortcutAction)
	{
		if (_currentTab != SettingsTab.Shortcuts)
		{
			SwitchTab(SettingsTab.Shortcuts, animate: true);
		}

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
		CallVisualFx("clear_shortcut_focus");
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

		UpdateShortcutPreviewCards();
	}

	private void UpdateShortcutPreviewCards()
	{
		_summarySettingsValueLabel.Text = _editingSettings.OpenSettingsKey;
		_summaryWarehouseValueLabel.Text = _editingSettings.OpenWarehouseKey;
		_summarySpeedValueLabel.Text = _editingSettings.ToggleSpeedKey;
	}

	private static void UpdateVolumeDisplay(Label targetLabel, double value)
	{
		var displayValue = Mathf.Clamp(Mathf.RoundToInt((float)value), 0, 100);
		targetLabel.Text = FormatSealDigits(displayValue);
	}

	private void UpdateZoomDisplay(double value)
	{
		_zoomValueLabel.Text = $"{value:0}%";
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

		return _currentTab switch
		{
			SettingsTab.AudioVisual when _editingSettings.IsFullscreen => "当前已启用须弥幻境；界面会直接铺满所在屏幕，乾坤视野只用于记忆窗口化尺寸。",
			SettingsTab.AudioVisual => "万象声色页现按参考卷面收口为金石交鸣、须弥幻境、灵光显像、乾坤视野四项，收录后沿用当前卷面裁定。",
			SettingsTab.Shortcuts => "敕令符节页可逐项改录快捷键；若新旧符令冲突，会在落卷前自动互换。",
			SettingsTab.Language when !_editingSettings.AutoSaveEnabled => "言辞教化当前停用了自动存档，只保留手录分卷与速录主卷；若要久战，请记得自行收录。",
			SettingsTab.Language when _editingSettings.EnableBloodGore => "言辞教化当前允许更直白的煞气留痕表现；若需清净卷面，可在此页再行封存。",
			SettingsTab.Language => "言辞教化会在收录后生效；可裁定语言、自动存档、气血浮沉与煞气留痕。",
			_ => string.Empty
		};
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
			ShortcutAction.OpenSettings => $"{ShortcutVisualGridPath}/UiSection/UiGrid/OpenSettingsCard/Row/OpenSettingsKeyOption",
			ShortcutAction.OpenWarehouse => $"{ShortcutVisualGridPath}/UiSection/UiGrid/OpenWarehouseCard/Row/OpenWarehouseKeyOption",
			ShortcutAction.ToggleExploration => $"{ShortcutVisualGridPath}/MovementSection/MovementGrid/ToggleExplorationCard/Row/ToggleExplorationKeyOption",
			ShortcutAction.ToggleSpeed => $"{ShortcutVisualGridPath}/MovementSection/MovementGrid/ToggleSpeedCard/Row/ToggleSpeedKeyOption",
			ShortcutAction.QuickSave => $"{ShortcutSettingsPath}/QuickSaveKeyRow/QuickSaveKeyOption",
			ShortcutAction.QuickLoad => $"{ShortcutSettingsPath}/QuickLoadKeyRow/QuickLoadKeyOption",
			ShortcutAction.QuickReset => $"{ShortcutVisualGridPath}/UiSection/UiGrid/QuickResetCard/Row/QuickResetKeyOption",
			_ => string.Empty
		};

		if (!string.IsNullOrEmpty(buttonPath))
		{
			CallVisualFx("pulse_shortcut", buttonPath);
		}
	}

	private static string GetTabVisualName(SettingsTab tab)
	{
		return tab switch
		{
			SettingsTab.AudioVisual => "AudioVisual",
			SettingsTab.Shortcuts => "Shortcuts",
			SettingsTab.Language => "Language",
			_ => "AudioVisual"
		};
	}

	private static string GetPagePath(SettingsTab tab)
	{
		return tab switch
		{
			SettingsTab.AudioVisual => AudioPagePath,
			SettingsTab.Shortcuts => ShortcutPagePath,
			SettingsTab.Language => LanguagePagePath,
			_ => AudioPagePath
		};
	}

	private void OnCloseRequested()
	{
		ClosePopup();
	}

	private void OnApplyPressed()
	{
		_editingSettings.Language = LanguageOptions[Mathf.Clamp(_languageOption.Selected, 0, LanguageOptions.Length - 1)].Code;
		_editingSettings.IsFullscreen = _fullscreenSealButton.ButtonPressed;
		_editingSettings.GraphicsQualityPreset = QualityOptions[Mathf.Clamp(_qualityOption.Selected, 0, QualityOptions.Length - 1)].Code;
		_editingSettings.AutoSaveEnabled = _autoSaveSealButton.ButtonPressed;
		_editingSettings.ShowDamageText = _damageTextSealButton.ButtonPressed;
		_editingSettings.EnableBloodGore = _bloodGoreSealButton.ButtonPressed;

		if (_resolutionOptions.Count > 0)
		{
			var selectedResolution = _resolutionOptions[Mathf.Clamp(_resolutionOption.Selected, 0, _resolutionOptions.Count - 1)];
			_editingSettings.ResolutionWidth = selectedResolution.X;
			_editingSettings.ResolutionHeight = selectedResolution.Y;
		}

		if (_autoSaveIntervalOptions.Count > 0)
		{
			_editingSettings.AutoSaveInterval = _autoSaveIntervalOptions[Mathf.Clamp(_autoSaveIntervalOption.Selected, 0, _autoSaveIntervalOptions.Count - 1)];
		}

		_editingSettings.FontScale = Mathf.Clamp((float)_zoomSlider.Value / 100.0f,
			ClientSettingsSystem.MinContentZoom,
			ClientSettingsSystem.MaxContentZoom);
		_editingSettings.BgmVolume = Mathf.Clamp((float)_bgmVolumeSlider.Value / 100.0f, 0.0f, 1.0f);
		_editingSettings.SfxVolume = Mathf.Clamp((float)_masterVolumeSlider.Value / 100.0f, 0.0f, 1.0f);

		ApplyRequested?.Invoke(_editingSettings.Clone());
		ClosePopup();
	}

	private void OnVolumeSliderChanged(double value, VolumeField volumeField)
	{
		var nextVolume = Mathf.Clamp((float)value / 100.0f, 0.0f, 1.0f);
		switch (volumeField)
		{
			case VolumeField.Master:
				UpdateVolumeDisplay(_masterVolumeValueLabel, value);
				if (Mathf.Abs(_editingSettings.MasterVolume - nextVolume) < 0.0001f)
				{
					return;
				}

				_editingSettings.MasterVolume = nextVolume;
				break;
			case VolumeField.Bgm:
				UpdateVolumeDisplay(_bgmVolumeValueLabel, value);
				if (Mathf.Abs(_editingSettings.BgmVolume - nextVolume) < 0.0001f)
				{
					return;
				}

				_editingSettings.BgmVolume = nextVolume;
				break;
			case VolumeField.Sfx:
				UpdateVolumeDisplay(_sfxVolumeValueLabel, value);
				if (Mathf.Abs(_editingSettings.SfxVolume - nextVolume) < 0.0001f)
				{
					return;
				}

				_editingSettings.SfxVolume = nextVolume;
				break;
		}

		PreviewRequested?.Invoke(_editingSettings.Clone());
	}

	private void OnZoomSliderChanged(double value)
	{
		UpdateZoomDisplay(value);
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
		CallVisualFx("sync_fullscreen_seal", isButtonPressed);
		if (_editingSettings.IsFullscreen == isButtonPressed)
		{
			RefreshPopupHint();
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

	private void OnQualitySelected(long index)
	{
		var selectedIndex = Mathf.Clamp((int)index, 0, QualityOptions.Length - 1);
		_editingSettings.GraphicsQualityPreset = QualityOptions[selectedIndex].Code;
		RefreshChoiceButtonStates();
		RefreshPopupHint();
	}

	private void OnAutoSaveToggled(bool isButtonPressed)
	{
		CallVisualFx("sync_toggle_seal", _autoSaveSealButton.GetPath().ToString(), isButtonPressed);
		if (_editingSettings.AutoSaveEnabled == isButtonPressed)
		{
			RefreshLanguagePageState();
			RefreshPopupHint();
			return;
		}

		_editingSettings.AutoSaveEnabled = isButtonPressed;
		RefreshLanguagePageState();
		RefreshPopupHint();
	}

	private void OnDamageTextToggled(bool isButtonPressed)
	{
		CallVisualFx("sync_toggle_seal", _damageTextSealButton.GetPath().ToString(), isButtonPressed);
		_editingSettings.ShowDamageText = isButtonPressed;
		RefreshPopupHint();
	}

	private void OnBloodGoreToggled(bool isButtonPressed)
	{
		CallVisualFx("sync_toggle_seal", _bloodGoreSealButton.GetPath().ToString(), isButtonPressed);
		_editingSettings.EnableBloodGore = isButtonPressed;
		RefreshPopupHint();
	}

	private void OnAutoSaveIntervalSelected(long index)
	{
		if (_autoSaveIntervalOptions.Count == 0)
		{
			return;
		}

		var selectedIndex = Mathf.Clamp((int)index, 0, _autoSaveIntervalOptions.Count - 1);
		_editingSettings.AutoSaveInterval = _autoSaveIntervalOptions[selectedIndex];
		RefreshPopupHint();
	}

	private void RefreshLanguagePageState()
	{
		_autoSaveIntervalOption.Disabled = !_editingSettings.AutoSaveEnabled;
		_autoSaveIntervalOption.Modulate = _editingSettings.AutoSaveEnabled ? Colors.White : new Color(1, 1, 1, 0.45f);
	}

	private static string FormatSealDigits(int value)
	{
		var safeValue = Mathf.Clamp(value, 0, 100);
		var builder = new StringBuilder();
		foreach (var digit in safeValue.ToString())
		{
			builder.Append(digit switch
			{
				'0' => '〇',
				'1' => '一',
				'2' => '二',
				'3' => '三',
				'4' => '四',
				'5' => '五',
				'6' => '六',
				'7' => '七',
				'8' => '八',
				'9' => '九',
				_ => digit
			});
		}

		return builder.ToString();
	}
}
