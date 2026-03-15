using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Core;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle;

public partial class Main : Control
{

    private enum MapTab
    {
        Sect,
        World,
        WorldSite,
        Prefecture,
        EventPanel,
        ReportPanel,
        Expedition
    }

    private const string MainLayoutPath = "RootMargin/MainLayout";
    private const string BackgroundPath = "Background";
    private const string TopBarPath = $"{MainLayoutPath}/TopBar";
    private const string BodyRowPath = $"{MainLayoutPath}/BodyRow";
    private const string LeftPanelPath = $"{BodyRowPath}/LeftPanel";
    private const string RightPanelPath = $"{BodyRowPath}/RightPanel";
    private const string BottomBarPath = $"{MainLayoutPath}/BottomBar";
    private const string CenterPanelContentPath = $"{BodyRowPath}/CenterPanel/PanelContent";
    private const string CenterTopTabRowPath = $"{CenterPanelContentPath}/TopTabRow";
    private const string CenterMapPagesPath = $"{CenterPanelContentPath}/MapViewport/MapPages";
    private const string CenterReportDetailPagesPath = $"{CenterMapPagesPath}/ReportPanelView/ReportScroll/DetailPages";
    private const string LegacyLayoutPath = "RootMargin";

    private const string LegacyBackgroundTexturePath = "res://assets/ui/background/background_2.png";

    private const int JobAdjustStep = 1;
    private const int MineUnlockTechLevel = 1;
    private const string MineLockedText = "🔒 需科技 锻造术(T1)";
    private const string MineUnlockedText = "↑ 扩建传法院 (木16 石22 金22)";
    private static readonly double[] SupportedTimeScales = { 1.0, 2.0, 4.0 };
    private static readonly string[] ChronicleTimeLabels = { "子时", "丑时", "寅时", "卯时", "辰时", "巳时", "午时", "未时", "申时", "酉时", "戌时", "亥时" };

    private readonly Queue<string> _logs = new();
    private readonly GameCalendarSystem _gameCalendarSystem = new();
    private readonly SaveSystem _saveSystem = new();

    private GameLoop _gameLoop = null!;
    private RichTextLabel _logLabel = null!;
    private Label _populationLabel = null!;
    private Label _happinessLabel = null!;
    private Label _foodLabel = null!;
    private Label _woodLabel = null!;
    private Label _goldLabel = null!;
    private Label _threatLabel = null!;
    private Label _techLabel = null!;
    private Label _calendarLabel = null!;
    private Label? _calendarDetailLabel;
    private ProgressBar? _calendarQuarterProgress;
    private ProgressBar? _calendarDayProgress;
    private Button _exploreButton = null!;
    private Button? _speedX1Button;
    private Button? _speedX2Button;
    private Button? _speedX4Button;
    private Button _saveButton = null!;
    private Button _loadButton = null!;
    private Button _resetButton = null!;
    private Button? _worldMapButton;
    private Button? _prefectureMapButton;
    private Button? _eventPanelButton;
    private Button? _reportPanelButton;
    private Button? _expeditionMapButton;
    private Button? _mapZoomResetButton;
    private HSlider? _mapZoomSlider;
    private Node? _worldPanelVisualFx;
    private Node? _topTabVisualFx;
    private bool _isUpdatingMapZoomSlider;
    private Control? _worldMapView;
    private StrategicMapViewSystem? _worldMapRenderer;
    private Control? _worldSiteMapView;
    private Control? _prefectureMapView;
    private Control? _countyTownMapView;
    private Control? _eventPanelView;
    private Control? _reportPanelView;
    private Control? _expeditionMapView;
    private Button? _mineUpgradeButton;
    private RichTextLabel? _peakOverviewLabel;
    private Button? _peakPrevButton;
    private Button? _peakNextButton;
    private Label? _peakCurrentTitleLabel;
    private Label? _peakDetailCounterLabel;
    private Label? _peakCurrentSummaryLabel;
    private Label? _peakSupportStatusLabel;
    private Button? _peakSupportButton;
    private Button? _peakSupportResetButton;
    private RichTextLabel? _peakCurrentDetailLabel;
    private Control _legacyLayoutRoot = null!;
    private TextureRect? _backgroundTextureRect;
    private SectMapViewSystem? _sectMapRenderer;


    private int _timeScaleIndex;
    private MapTab _currentMapTab = MapTab.Sect;
    private int _selectedPeakIndex = SectOrganizationRules.GetDefaultPeakIndex();
    private int _lastCalendarGameMinute = -1;
    private int _lastObservedHourSettlements = -1;

    public override void _Ready()
    {
        InitializeClientSettings();
        BindUiNodes();
        ConfigureDualMapMode();
        CreateSettingsPanel();
        CreateWarehousePanel();
        CreateTaskPanel();
        CreateDisciplePanel();
        CreateSectOrganizationPanel();
        CreateSaveSlotsPanel();
        BindUiEvents();
        ConfigureLegacyFocusNavigation();
        BindLanternHoverEffects();
        SetupGameLoop();
        LoadInitialState();
    }

    public override void _Process(double delta)
    {
        if (_gameLoop == null)
        {
            return;
        }

        _sectMapRenderer?.SetResidentClock(_gameLoop.State.GameMinutes, GetCurrentTimeScaleFloat());
        UpdateCalendarUi();
        RefreshMapZoomUi();
    }

    public override void _ExitTree()
    {
        UnbindClientSettingEvents();
        UnbindWarehousePanelEvents();
        UnbindTaskPanelEvents();
        UnbindDisciplePanelEvents();
        UnbindSectOrganizationPanelEvents();
        UnbindSaveSlotsPanelEvents();
        UnbindSectTileInspectorEvents();
    }

    private void SetupGameLoop()
    {
        _gameLoop = new GameLoop();
        AddChild(_gameLoop);
        _gameLoop.SetTimeScale(GetCurrentTimeScale());
        _gameLoop.Events.StateChanged += OnStateChanged;
        _gameLoop.Events.LogAdded += AppendLog;
    }

    private void LoadInitialState()
    {
        if (_saveSystem.TryLoad(out var state, out var msg))
        {
            _gameLoop.LoadState(state);
            AppendLog(msg);
            return;
        }

        _gameLoop.ResetState();
        AppendLog(msg);
    }

    private void OnStateChanged(GameState state)
    {
        _populationLabel.Text = $"人丁 {state.Population}";
        _happinessLabel.Text = $"民心 {state.Happiness:0.#}";

        var economyPreview = EconomySystem.BuildHourPreview(state);
        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var managementBoost = IndustryRules.GetManagementBoost(state);
        var activeDirection = SectGovernanceRules.GetActiveDevelopmentDefinition(state);
        var activeLaw = SectGovernanceRules.GetActiveLawDefinition(state);
        var activeTalentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(state);
        var gatheringEstimate = MaterialRules.EstimateGathering(state);
        var primaryProcessingEstimate = MaterialRules.EstimatePrimaryProcessing(
            state,
            gatheringEstimate.TimberGain,
            gatheringEstimate.RawStoneGain);

        var woodDelta = InventoryRules.PredictDelta(state, nameof(GameState.Wood), primaryProcessingEstimate.WoodGain);
        var stoneDelta = InventoryRules.PredictDelta(state, nameof(GameState.Stone), primaryProcessingEstimate.StoneGain);
        var visibleFoodDelta = InventoryRules.PredictDelta(state, nameof(GameState.Food), economyPreview.FoodDeltaRaw);
        var visibleGoldDelta = InventoryRules.PredictDelta(state, nameof(GameState.Gold), economyPreview.GoldDeltaRaw);
        var visibleContributionDelta = InventoryRules.PredictDelta(state, nameof(GameState.ContributionPoints), economyPreview.ContributionDeltaRaw);

        _foodLabel.Text = $"{MaterialSemanticRules.GetDisplayName(nameof(GameState.Food))} {state.Food:0}";
        _foodLabel.TooltipText =
            $"下一时辰预计：{MaterialSemanticRules.GetDisplayName(nameof(GameState.Food))}{FormatSigned(visibleFoodDelta)}，当前库存 {state.Food:0}。";
        _woodLabel.Text = $"木石 {state.Wood:0}";
        _woodLabel.TooltipText =
            $"下一时辰预计：{MaterialSemanticRules.GetDisplayName(nameof(GameState.Wood))}{FormatSigned(woodDelta)}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.Stone))}{FormatSigned(stoneDelta)}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.Timber))}库存 {state.Timber:0}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.RawStone))}库存 {state.RawStone:0}。";
        _goldLabel.Text = $"灵石 {state.Gold:0}";
        _goldLabel.TooltipText =
            $"功绩 {state.ContributionPoints:0}（{FormatSigned(visibleContributionDelta)}/时），灵石流转 {FormatSigned(visibleGoldDelta)}/时。";
        _threatLabel.Text = $"危兆 {state.Threat:0}%";
        _techLabel.Text = $"研修 T{Math.Max(state.TechLevel + 1, 1)}";
        _exploreButton.Text = state.ExplorationEnabled ? "历练中" : "历练";

        _sectMapRenderer?.RefreshMap(state.Population, state.HousingCapacity, state.ElitePopulation);
        _sectMapRenderer?.RefreshResidents(state);
        _sectMapRenderer?.SetResidentClock(state.GameMinutes, GetCurrentTimeScaleFloat());
        RefreshWarehousePanelPopup(state);
        RefreshTaskPanelPopup(state);
        RefreshDisciplePanelPopup(state);
        RefreshSectOrganizationPanelPopup(state);
        HandleAutoSaveFromState(state);

        RefreshJobPanels(state);
        RefreshMineButtonState(state);
        RefreshSectChroniclePanel(state);
        UpdateCalendarUi(force: true);
        RefreshMapOperationalLinkUi(state);
    }

    private static string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private void UpdateCalendarUi(bool force = false)
    {
        if (_gameLoop == null)
        {
            return;
        }

        var gameMinutes = Math.Max(_gameLoop.State.GameMinutes, 0);
        if (!force && gameMinutes == _lastCalendarGameMinute)
        {
            return;
        }

        _lastCalendarGameMinute = gameMinutes;

        var calendarInfo = _gameCalendarSystem.Describe(gameMinutes);
        _calendarLabel.Text = calendarInfo.DateText;
        _calendarLabel.TooltipText = calendarInfo.HeaderText;

        if (_calendarDetailLabel != null)
        {
            _calendarDetailLabel.Text = calendarInfo.DetailText;
        }

        if (_calendarQuarterProgress != null)
        {
            _calendarQuarterProgress.MaxValue = 100.0;
            _calendarQuarterProgress.Value = calendarInfo.QuarterProgressPercent;
            _calendarQuarterProgress.TooltipText = calendarInfo.QuarterProgressText;
        }

        if (_calendarDayProgress != null)
        {
            _calendarDayProgress.MaxValue = 100.0;
            _calendarDayProgress.Value = calendarInfo.DayProgressPercent;
            _calendarDayProgress.TooltipText = calendarInfo.DayProgressText;
        }
    }

    private void RefreshJobPanels(GameState state)
    {
        if (_peakOverviewLabel != null)
        {
            _peakOverviewLabel.Text = SectOrganizationRules.BuildPeakOverviewText();
            _peakOverviewLabel.TooltipText = "按设定文档汇总当前可见九峰与附属部门结构。";
        }

        RefreshPeakDetailPanel(state);
    }

    private void AppendLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var totalMinutes = _gameLoop?.State.GameMinutes ?? 0;
        var coloredMessage = ColorizeLogMessage(message);
        _logs.Enqueue($"[{GetChronicleTimeText(totalMinutes)}] {coloredMessage}");

        while (_logs.Count > 14)
        {
            _logs.Dequeue();
        }

        _logLabel.Text = string.Join("\n", _logs);
    }

    private static string ColorizeLogMessage(string message)
    {
        if (message.Contains("警告", StringComparison.Ordinal) ||
            message.Contains("不足", StringComparison.Ordinal) ||
            message.Contains("失败", StringComparison.Ordinal))
        {
            return $"[color=#9e2a22]{message}[/color]";
        }

        if (message.Contains("获得", StringComparison.Ordinal) ||
            message.Contains("胜利", StringComparison.Ordinal) ||
            message.Contains("成功", StringComparison.Ordinal))
        {
            return $"[color=#8a6a3b]{message}[/color]";
        }

        if (message.Contains("存档", StringComparison.Ordinal) ||
            message.Contains("读档", StringComparison.Ordinal))
        {
            return $"[color=#6b5f54]{message}[/color]";
        }

        return $"[color=#4a3f35]{message}[/color]";
    }

    private static string GetChronicleTimeText(int totalMinutes)
    {
        var normalizedHours = ((totalMinutes / 60) % 24 + 24) % 24;
        return ChronicleTimeLabels[(normalizedHours / 2) % ChronicleTimeLabels.Length];
    }

    private void BindUiNodes()
    {
        _backgroundTextureRect = GetNodeOrNull<TextureRect>(BackgroundPath);
        ConfigureLegacyBackground();
        _legacyLayoutRoot = GetNode<Control>(LegacyLayoutPath);
        BindLegacyUiNodes();

        SetSpeedScale(1.0);
        SetMapTab(MapTab.Sect);
    }

private void ConfigureLegacyBackground()
    {
        if (_backgroundTextureRect == null)
        {
            return;
        }

        var absolutePath = ProjectSettings.GlobalizePath(LegacyBackgroundTexturePath);
        if (FileAccess.FileExists(absolutePath))
        {
            var image = Image.LoadFromFile(absolutePath);
            if (image != null && !image.IsEmpty())
            {
                _backgroundTextureRect.Texture = ImageTexture.CreateFromImage(image);
                return;
            }
        }

        var texture = GD.Load<Texture2D>(LegacyBackgroundTexturePath);
        if (texture != null)
        {
            _backgroundTextureRect.Texture = texture;
            return;
        }

        GD.PushWarning($"主界面背景图加载失败：{LegacyBackgroundTexturePath}");
    }

    private void BindLegacyUiNodes()
    {
        _populationLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/PopulationLabel");
        _happinessLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/HappinessLabel");
        _foodLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/FoodLabel");
        _woodLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/WoodLabel");
        _goldLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/GoldLabel");
        _threatLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/CycleBox/InfoRow/ThreatLabel");
        _techLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/CycleBox/InfoRow/TechLabel");
        _calendarLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/CycleBox/InfoRow/CycleLabel");
        _calendarDetailLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/CycleBox/CountdownLabel");
        _calendarQuarterProgress = GetNode<ProgressBar>($"{TopBarPath}/BarContent/MainRow/CycleBox/QuarterProgressRow/QuarterProgress");
        _calendarDayProgress = GetNode<ProgressBar>($"{TopBarPath}/BarContent/MainRow/CycleBox/DayProgressRow/DayProgress");

        _logLabel = GetNode<RichTextLabel>($"{RightPanelPath}/PanelContent/MainVBox/LogBox/LogVBox/LogContent/LogLabel");

        _exploreButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/ExploreButton");
        _speedX1Button = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/SpeedX1Button");
        _speedX2Button = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/SpeedX2Button");
        _speedX4Button = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/SpeedX4Button");
        _saveButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/SaveButton");
        _loadButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/LoadButton");
        _settingsButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/SettingsButton");
        _resetButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/ResetButton");

        _worldMapButton = GetNode<Button>($"{CenterTopTabRowPath}/WorldMapButton");
        _prefectureMapButton = GetNodeOrNull<Button>($"{CenterTopTabRowPath}/PrefectureMapButton");
        _eventPanelButton = GetNodeOrNull<Button>($"{CenterTopTabRowPath}/EventPanelButton");
        _reportPanelButton = GetNodeOrNull<Button>($"{CenterTopTabRowPath}/ReportPanelButton");
        _expeditionMapButton = GetNodeOrNull<Button>($"{CenterTopTabRowPath}/ExpeditionMapButton");
        _mapZoomResetButton = GetNode<Button>($"{CenterTopTabRowPath}/MapZoomResetButton");
        _mapZoomSlider = GetNode<HSlider>($"{CenterTopTabRowPath}/MapZoomSlider");
        _worldPanelVisualFx = GetNodeOrNull<Node>($"{CenterPanelContentPath}/VisualFx");
        _topTabVisualFx = GetNodeOrNull<Node>($"{CenterPanelContentPath}/TopTabVisualFx");
        BindMapOperationalLegacyNodes();

        _worldMapView = GetNode<Control>($"{CenterMapPagesPath}/WorldMapView");
        _worldMapRenderer = _worldMapView as StrategicMapViewSystem;
        _worldSiteMapView = GetNode<Control>($"{CenterMapPagesPath}/SecondaryMapView");
        _prefectureMapView = GetNodeOrNull<Control>($"{CenterMapPagesPath}/PrefectureMapView");
        _countyTownMapView = GetNode<Control>($"{CenterMapPagesPath}/CountyTownMapView");
        _eventPanelView = GetNodeOrNull<Control>($"{CenterMapPagesPath}/EventPanelView");
        _reportPanelView = GetNodeOrNull<Control>($"{CenterMapPagesPath}/ReportPanelView");
        _expeditionMapView = GetNodeOrNull<Control>($"{CenterMapPagesPath}/ExpeditionMapView");
        _sectMapRenderer = GetNode<SectMapViewSystem>($"{CenterMapPagesPath}/CountyTownMapView");
        _mineUpgradeButton = GetNodeOrNull<Button>($"{CenterReportDetailPagesPath}/MineCard/CardVBox/UpgradeButton");
        BindSectTileInspectorNodes();
        BindWorldSitePanelNodes();
        BindSectChronicleNodes();
        ClearLegacyJobsPaddingBindings();

    }

    private void BindUiEvents()
    {
        _exploreButton.Pressed += () => _gameLoop.ToggleExploration();
        BindSettingsButtonEvent();
        BindWarehouseButtonEvent();
        BindTaskButtonEvent();
        BindDiscipleButtonEvent();
        BindSectOrganizationButtonEvent();
        BindDiscipleMapInspectionEvent();
        BindSectTileInspectorEvents();

        if (_speedX1Button != null)
        {
            _speedX1Button.Pressed += () =>
            {
                ApplySpeedScale(1.0);
            };
        }

        if (_speedX2Button != null)
        {
            _speedX2Button.Pressed += () =>
            {
                ApplySpeedScale(2.0);
            };
        }

        if (_speedX4Button != null)
        {
            _speedX4Button.Pressed += () =>
            {
                ApplySpeedScale(4.0);
            };
        }

        if (_worldMapButton != null)

        {

            _worldMapButton.Pressed += () =>

            {

                var targetTab = _currentMapTab == MapTab.World ? MapTab.Sect : MapTab.World;

                SetMapTab(targetTab);

            };

        }

        if (_mapZoomResetButton != null)
        {
            _mapZoomResetButton.Pressed += ResetCurrentMapZoom;
        }

        if (_mapZoomSlider != null)
        {
            _mapZoomSlider.ValueChanged += OnMapZoomSliderChanged;
        }

        BindMapOperationalEvents();

        _saveButton.Pressed += OpenSaveSlotsPanelForSave;
        _loadButton.Pressed += OpenSaveSlotsPanelForLoad;

        _resetButton.Pressed += () => _gameLoop.ResetState();

        {
            return;
        }

        BindLegacyIndustryButtons();
    }

    private void ClearLegacyJobsPaddingBindings()
    {
        _peakOverviewLabel = null;
        _peakPrevButton = null;
        _peakNextButton = null;
        _peakCurrentTitleLabel = null;
        _peakDetailCounterLabel = null;
        _peakCurrentSummaryLabel = null;
        _peakSupportStatusLabel = null;
        _peakSupportButton = null;
        _peakSupportResetButton = null;
        _peakCurrentDetailLabel = null;
    }

    private void BindLegacyIndustryButtons()
    {
        var agricultureButton = GetNodeOrNull<Button>($"{CenterReportDetailPagesPath}/MulberryCard/CardVBox/UpgradeButton");
        if (agricultureButton != null)
        {
            agricultureButton.Pressed += () => _gameLoop.BuildIndustryBuilding(IndustryBuildingType.Agriculture);
        }

        var workshopButton = GetNodeOrNull<Button>($"{CenterReportDetailPagesPath}/LumberCard/CardVBox/UpgradeButton");
        if (workshopButton != null)
        {
            workshopButton.Pressed += () => _gameLoop.BuildIndustryBuilding(IndustryBuildingType.Workshop);
        }

        if (_mineUpgradeButton != null)
        {
            _mineUpgradeButton.Disabled = false;
            _mineUpgradeButton.Pressed += OnMineUpgradePressed;
        }
    }

    private void ApplySpeedScale(double scale)
    {
        SetSpeedScale(scale);
        AppendLog($"时辰刻度切换至 {GetTimeScaleText(GetCurrentTimeScale())}。");
    }

    private void CycleSpeedScale()
    {
        var nextIndex = (_timeScaleIndex + 1) % SupportedTimeScales.Length;
        SetSpeedScale(SupportedTimeScales[nextIndex]);
        AppendLog($"时辰刻度切换至 {GetTimeScaleText(GetCurrentTimeScale())}。");
    }

    private void SetSpeedScale(double scale)
    {
        var targetIndex = 0;
        for (var index = 0; index < SupportedTimeScales.Length; index++)
        {
            if (Math.Abs(SupportedTimeScales[index] - scale) < 0.001)
            {
                targetIndex = index;
                break;
            }
        }

        _timeScaleIndex = targetIndex;

        if (_gameLoop != null)
        {
            _gameLoop.SetTimeScale(GetCurrentTimeScale());
        }

        if (_speedX1Button != null)
        {
            _speedX1Button.ButtonPressed = Math.Abs(GetCurrentTimeScale() - 1.0) < 0.001;
            _speedX1Button.Text = "常速";
        }

        if (_speedX2Button != null)
        {
            _speedX2Button.ButtonPressed = Math.Abs(GetCurrentTimeScale() - 2.0) < 0.001;
            _speedX2Button.Text = "倍速";
        }

        if (_speedX4Button != null)
        {
            _speedX4Button.ButtonPressed = Math.Abs(GetCurrentTimeScale() - 4.0) < 0.001;
            _speedX4Button.Text = "疾速";
        }
    }

    private static string GetTimeScaleText(double scale)
    {
        if (Math.Abs(scale - 1.0) < 0.001)
        {
            return "常速";
        }

        if (Math.Abs(scale - 2.0) < 0.001)
        {
            return "倍速";
        }

        if (Math.Abs(scale - 4.0) < 0.001)
        {
            return "疾速";
        }

        return $"x{scale:0}";
    }

    private double GetCurrentTimeScale()
    {
        return SupportedTimeScales[Math.Clamp(_timeScaleIndex, 0, SupportedTimeScales.Length - 1)];
    }

    private float GetCurrentTimeScaleFloat()
    {
        return (float)GetCurrentTimeScale();
    }

    private void SetMapTab(MapTab mapTab)
    {
        if (mapTab is MapTab.Prefecture or MapTab.EventPanel or MapTab.ReportPanel or MapTab.Expedition)
        {
            mapTab = MapTab.Sect;
        }

        if (_worldMapView == null ||
            _worldSiteMapView == null ||
            _countyTownMapView == null ||
            _worldMapButton == null)

        {

            return;

        }

        _worldMapView.Visible = mapTab == MapTab.World;
        _worldSiteMapView.Visible = mapTab == MapTab.WorldSite;
        _countyTownMapView.Visible = mapTab == MapTab.Sect;
        if (_prefectureMapView != null)
        {
            _prefectureMapView.Visible = mapTab == MapTab.Prefecture;
        }

        if (_eventPanelView != null)
        {
            _eventPanelView.Visible = mapTab == MapTab.EventPanel;
        }

        if (_reportPanelView != null)
        {
            _reportPanelView.Visible = mapTab == MapTab.ReportPanel;
        }

        if (_expeditionMapView != null)
        {
            _expeditionMapView.Visible = mapTab == MapTab.Expedition;
        }

        _worldMapButton.ButtonPressed = mapTab == MapTab.World;

        if (_worldMapButton != null)

        {

            if (mapTab == MapTab.World)

            {

                _worldMapButton.Text = "返回山门沙盘";

                _worldMapButton.TooltipText = "返回天衍峰山门沙盘，继续山门布局与内务经营。";

            }

            else

            {

                _worldMapButton.Text = "返回世界地图";

                _worldMapButton.TooltipText = "查看卷外山河舆图，选择外域点位进入不同区域。";

            }

        }
        if (_prefectureMapButton != null)
        {
            _prefectureMapButton.ButtonPressed = false;
        }

        if (_eventPanelButton != null)
        {
            _eventPanelButton.ButtonPressed = false;
        }

        if (_reportPanelButton != null)
        {
            _reportPanelButton.ButtonPressed = false;
        }

        if (_expeditionMapButton != null)
        {
            _expeditionMapButton.ButtonPressed = false;
        }

        _currentMapTab = mapTab;
        CallWorldPanelVisualFx("play_tab_switch", mapTab.ToString());
        CallTopTabVisualFx("play_tab_emphasis", mapTab.ToString());
        if (mapTab == MapTab.World)
        {
            ApplyWorldSiteInspectorSummary(_worldMapRenderer?.SelectedWorldSite);
        }
        else if (mapTab == MapTab.WorldSite)
        {
            ApplyWorldSiteInspectorSummary(_worldMapRenderer?.SelectedWorldSite);
            RefreshWorldSitePanel();
        }
        RefreshMapZoomUi();
        RefreshMapOperationalLinkUi();
    }

    private void CallWorldPanelVisualFx(string methodName, params Variant[] args)
    {
        _worldPanelVisualFx?.Call(methodName, args);
    }

    private void CallTopTabVisualFx(string methodName, params Variant[] args)
    {
        _topTabVisualFx?.Call(methodName, args);
    }

    private void AdjustCurrentMapZoom(float delta)
    {
        var mapView = GetActiveMapZoomView();
        if (mapView == null)
        {
            return;
        }

        mapView.AdjustZoom(delta);
        RefreshMapZoomUi();
    }

    private void ResetCurrentMapZoom()
    {
        var mapView = GetActiveMapZoomView();
        if (mapView == null)
        {
            return;
        }

        mapView.ResetView();
        RefreshMapZoomUi();
    }

    private IMapZoomView? GetActiveMapZoomView()
    {
        return _currentMapTab switch
        {
            MapTab.World => _worldMapView as IMapZoomView,
            MapTab.WorldSite => _worldSiteSandboxMapView?.Visible == true ? _worldSiteSandboxMapView : null,
            MapTab.Sect => _countyTownMapView as IMapZoomView,
            _ => null
        };
    }

    private void ConfigureDualMapMode()
    {
        if (_worldMapButton != null)
        {
            _worldMapButton.Text = "展 全景舆图";
            _worldMapButton.TooltipText = "展开卷外山河舆图，查看世界灵脉、奇观与外部形势";
        }

        HideMapEntry(_prefectureMapButton, _prefectureMapView);
        HideMapEntry(_eventPanelButton, _eventPanelView);
        HideMapEntry(_reportPanelButton, _reportPanelView);
        HideMapEntry(_expeditionMapButton, _expeditionMapView);
    }

    private static void HideMapEntry(CanvasItem? button, CanvasItem? view)
    {
        if (button != null)
        {
            button.Visible = false;
            if (button is BaseButton baseButton)
            {
                baseButton.Disabled = true;
                baseButton.ButtonPressed = false;
            }
        }

        if (view != null)
        {
            view.Visible = false;
        }
    }

    private void RefreshMapZoomUi()
    {
        if (_mapZoomResetButton == null || _mapZoomSlider == null)
        {
            return;
        }

        var mapView = GetActiveMapZoomView();
        var canZoom = mapView != null;
        _mapZoomResetButton.Disabled = !canZoom;
        _mapZoomSlider.Editable = canZoom;

        if (!canZoom || mapView == null)
        {
            SetMapZoomSliderValue(0.0);
            return;
        }

        if (!Mathf.IsEqualApprox((float)_mapZoomSlider.MinValue, mapView.MinZoom))
        {
            _mapZoomSlider.MinValue = mapView.MinZoom;
        }

        if (!Mathf.IsEqualApprox((float)_mapZoomSlider.MaxValue, mapView.MaxZoom))
        {
            _mapZoomSlider.MaxValue = mapView.MaxZoom;
        }

        SetMapZoomSliderValue(mapView.Zoom);
        _mapZoomSlider.TooltipText = $"{(int)Mathf.Round(mapView.Zoom * 100f)}%";
    }

    private void OnMapZoomSliderChanged(double value)
    {
        if (_isUpdatingMapZoomSlider)
        {
            return;
        }

        var mapView = GetActiveMapZoomView();
        if (mapView == null)
        {
            return;
        }

        mapView.SetZoom((float)value);
        RefreshMapZoomUi();
    }

    private void SetMapZoomSliderValue(double value)
    {
        if (_mapZoomSlider == null)
        {
            return;
        }

        if (Math.Abs(_mapZoomSlider.Value - value) < 0.0001)
        {
            return;
        }

        _isUpdatingMapZoomSlider = true;
        _mapZoomSlider.Value = value;
        _isUpdatingMapZoomSlider = false;
    }

    private void BindPeakDetailButtons()
    {
        if (_peakPrevButton != null)
        {
            _peakPrevButton.TooltipText = "浏览上一座主峰及其附属部门。";
            _peakPrevButton.Pressed += () => CyclePeakDetail(-1);
        }

        if (_peakNextButton != null)
        {
            _peakNextButton.TooltipText = "浏览下一座主峰及其附属部门。";
            _peakNextButton.Pressed += () => CyclePeakDetail(1);
        }

        if (_peakSupportButton != null)
        {
            _peakSupportButton.TooltipText = "将当前浏览峰脉设为本季协同峰。";
            _peakSupportButton.Pressed += ApplySelectedPeakSupport;
        }

        if (_peakSupportResetButton != null)
        {
            _peakSupportResetButton.TooltipText = "撤销单峰协同，恢复诸峰均衡轮转。";
            _peakSupportResetButton.Pressed += () => _gameLoop.ResetPeakSupport();
        }
    }

    private void CyclePeakDetail(int delta)
    {
        var peakCount = SectOrganizationRules.GetPeakCount();
        if (peakCount <= 0)
        {
            return;
        }

        _selectedPeakIndex = ((_selectedPeakIndex + delta) % peakCount + peakCount) % peakCount;
        RefreshPeakDetailPanel(_gameLoop?.State);
    }

    private void ApplySelectedPeakSupport()
    {
        var supportType = SectOrganizationRules.GetSupportTypeForPeakIndex(_selectedPeakIndex);
        _gameLoop.SetPeakSupport(supportType);
    }

    private void RefreshPeakDetailPanel(GameState? state = null)
    {
        if (_peakCurrentTitleLabel == null ||
            _peakDetailCounterLabel == null ||
            _peakCurrentSummaryLabel == null ||
            _peakCurrentDetailLabel == null)
        {
            return;
        }

        var peakCount = SectOrganizationRules.GetPeakCount();
        if (peakCount <= 0)
        {
            return;
        }

        _selectedPeakIndex = SectOrganizationRules.NormalizePeakIndex(_selectedPeakIndex);
        _peakCurrentTitleLabel.Text = SectOrganizationRules.GetPeakTitle(_selectedPeakIndex);
        _peakCurrentTitleLabel.TooltipText = "点击四条职司摘要时会自动跳到推荐峰脉。";
        _peakDetailCounterLabel.Text = $"{_selectedPeakIndex + 1}/{peakCount}";
        _peakCurrentSummaryLabel.Text = SectOrganizationRules.GetPeakSummary(_selectedPeakIndex);
        _peakCurrentSummaryLabel.TooltipText = SectOrganizationRules.BuildPeakDetailText(_selectedPeakIndex);
        _peakCurrentDetailLabel.Text = SectOrganizationRules.BuildPeakDetailText(_selectedPeakIndex);
        _peakCurrentDetailLabel.TooltipText = "峰脉详情来自《浮云宗-天衍峰》设定整理。";

        var currentState = state ?? (_gameLoop != null ? _gameLoop.State : null);
        var selectedSupportType = SectOrganizationRules.GetSupportTypeForPeakIndex(_selectedPeakIndex);
        var selectedSupportDefinition = SectPeakSupportRules.GetDefinition(selectedSupportType);

        if (_peakSupportStatusLabel != null)
        {
            var activeSupportText = currentState != null
                ? SectPeakSupportRules.BuildActiveSupportStatus(currentState)
                : SectPeakSupportRules.BuildSelectionPreview(SectPeakSupportType.Balanced);
            _peakSupportStatusLabel.Text =
                $"当前协同：{activeSupportText}\n候选峰令：{SectPeakSupportRules.BuildSelectionPreview(selectedSupportType)}";
            _peakSupportStatusLabel.TooltipText = selectedSupportDefinition.Description;
        }

        if (_peakSupportButton != null)
        {
            var isCurrentSupport = currentState != null && SectPeakSupportRules.GetActiveSupport(currentState) == selectedSupportType;
            _peakSupportButton.Text = isCurrentSupport ? "当前已协同" : $"立 {selectedSupportDefinition.DisplayName} 协同";
            _peakSupportButton.Disabled = isCurrentSupport;
            _peakSupportButton.TooltipText = selectedSupportDefinition.Description;
        }

        if (_peakSupportResetButton != null)
        {
            var isBalanced = currentState == null || SectPeakSupportRules.GetActiveSupport(currentState) == SectPeakSupportType.Balanced;
            _peakSupportResetButton.Disabled = isBalanced;
        }

        if (_peakPrevButton != null)
        {
            _peakPrevButton.Disabled = peakCount <= 1;
        }

        if (_peakNextButton != null)
        {
            _peakNextButton.Disabled = peakCount <= 1;
        }
    }

    private void OnMineUpgradePressed()
    {
        if (_gameLoop.State.TechLevel < MineUnlockTechLevel)
        {
            AppendLog("矿坑扩建未解锁：需要科技等级 T1。");
            return;
        }

        _gameLoop.BuildIndustryBuilding(IndustryBuildingType.Research);
    }

    private void RefreshMineButtonState(GameState state)
    {
        if (_mineUpgradeButton == null)
        {
            return;
        }

        _mineUpgradeButton.Disabled = false;
        _mineUpgradeButton.Text = state.TechLevel >= MineUnlockTechLevel ? MineUnlockedText : MineLockedText;
    }

    private void BindLanternHoverEffects()
    {
        GetNodeOrNull<Node>("LanternFx")?.Call("bind_hover_fx");
    }
}









