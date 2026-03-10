using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Core;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle;

public partial class Main : Control
{
    [Export]
    private bool _useFigmaLayout;

    private enum MapTab
    {
        Sect,
        World,
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
    private const string FigmaLayoutPath = "FigmaLayout";

    private const string FigmaRootPath = $"{FigmaLayoutPath}/RootMargin/MainColumn";
    private const string FigmaTopBarPath = $"{FigmaRootPath}/HUDTopBar/BarPadding/MainRow";
    private const string FigmaBottomBarPath = $"{FigmaRootPath}/BottomBar/PanelPadding/MainColumn";
    private const string FigmaCenterPath = $"{FigmaRootPath}/BodyRow/CenterView/PanelPadding/MainColumn";
    private const string FigmaTimelinePath = $"{FigmaRootPath}/BodyRow/TimelinePanel/PanelPadding/MainColumn";
    private const string FigmaEquipmentPath = $"{FigmaRootPath}/BodyRow/EquipmentPanel/PanelPadding/MainColumn";
    private const string LegacyBackgroundTexturePath = "res://assets/ui/background/background_2.png";

    private const int JobAdjustStep = 1;
    private const double LanternPulseDurationSeconds = 0.42;
    private const double LanternResetDurationSeconds = 0.14;
    private const float MapZoomStep = 0.1f;
    private const int MineUnlockTechLevel = 1;
    private const string MineLockedText = "🔒 需科技 锻造术(T1)";
    private const string MineUnlockedText = "↑ 扩建传法院 (木16 石22 金22)";

    private static readonly Color BaseButtonModulate = Colors.White;
    private static readonly Color LanternGlowModulate = new(1.0f, 0.86f, 0.64f, 1.0f);
    private static readonly double[] SupportedTimeScales = { 1.0, 2.0, 4.0 };
    private static readonly string[] ChronicleTimeLabels = { "子时", "丑时", "寅时", "卯时", "辰时", "巳时", "午时", "未时", "申时", "酉时", "戌时", "亥时" };

    private readonly Queue<string> _logs = new();
    private readonly Dictionary<Button, Tween> _buttonPulseTweens = new();
    private readonly Dictionary<JobType, PanelContainer> _jobRows = new();
    private readonly Dictionary<JobType, Label> _jobCountLabels = new();
    private readonly Dictionary<JobType, Label> _jobTitleLabels = new();
    private readonly Dictionary<JobType, Label> _jobStatusLabels = new();
    private readonly Dictionary<JobType, Label> _jobDetailLabels = new();
    private readonly Dictionary<JobType, Button> _jobPriorityButtons = new();
    private readonly Dictionary<JobType, StyleBoxFlat> _jobRowBaseStyles = new();
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
    private Button? _countyTownMapButton;
    private Button? _eventPanelButton;
    private Button? _reportPanelButton;
    private Button? _expeditionMapButton;
    private Button? _mapZoomOutButton;
    private Button? _mapZoomInButton;
    private Button? _mapZoomResetButton;
    private Label? _mapZoomLabel;
    private Control? _worldMapView;
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
    private Control _figmaLayoutRoot = null!;
    private TextureRect? _backgroundTextureRect;
    private SectMapViewSystem? _sectMapRenderer;

    private Button? _figmaBuildAgricultureButton;
    private Button? _figmaBuildWorkshopButton;
    private Button? _figmaCraftToolsButton;
    private Label? _figmaCenterTitleLabel;
    private Label? _figmaCenterDescriptionLabel;
    private Label? _figmaEquipmentTitleLabel;
    private Label? _figmaEquipmentSummaryLabel;
    private RichTextLabel? _figmaNotificationLabel;

    private bool _isUsingFigmaLayout;
    private int _timeScaleIndex;
    private MapTab _currentMapTab = MapTab.Sect;
    private JobType? _inspectedJobType;
    private int _selectedPeakIndex = SectOrganizationRules.GetDefaultPeakIndex();
    private int _lastCalendarGameMinute = -1;
    private int _lastObservedHourSettlements = -1;

    public override void _Ready()
    {
        InitializeClientSettings();
        BindUiNodes();
        BindBackgroundResizeEvents();
        ConfigureDualMapMode();
        CreateSettingsPanel();
        CreateWarehousePanel();
        CreateTaskPanel();
        CreateDisciplePanel();
        CreateSectOrganizationPanel();
        CreateSaveSlotsPanel();
        BindUiEvents();
        BindLanternHoverEffects();
        SetupGameLoop();
        LoadInitialState();
        ApplyLayoutSwitch();
    }

    public override void _Process(double delta)
    {
        if (_gameLoop == null)
        {
            return;
        }

        _sectMapRenderer?.SetResidentClock(_gameLoop.State.GameMinutes, GetCurrentTimeScaleFloat());
        UpdateCalendarUi();
    }

    public override void _ExitTree()
    {
        foreach (var tween in _buttonPulseTweens.Values)
        {
            tween.Kill();
        }

        _buttonPulseTweens.Clear();
        UnbindBackgroundResizeEvents();
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

        _exploreButton.Text = _isUsingFigmaLayout
            ? (state.ExplorationEnabled ? "⏸ 探险中" : "▶ 探险")
            : (state.ExplorationEnabled ? "历练中" : "历练");

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
        UpdateFigmaPanels(state);
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
        if (_jobCountLabels.Count == 0)
        {
            return;
        }

        foreach (var jobType in Enum.GetValues<JobType>())
        {
            var panelInfo = SectTaskRules.GetJobPanelInfo(state, jobType);
            _jobCountLabels[jobType].Text = IndustryRules.GetAssigned(state, jobType).ToString();
            _jobTitleLabels[jobType].Text = panelInfo.TitleText;
            _jobTitleLabels[jobType].TooltipText = panelInfo.DetailText;
            _jobStatusLabels[jobType].Text = panelInfo.SummaryText;
            _jobStatusLabels[jobType].TooltipText = panelInfo.DetailText;

            if (_jobDetailLabels.TryGetValue(jobType, out var detailLabel))
            {
                var isInspected = _inspectedJobType == jobType;
                detailLabel.Visible = isInspected;
                detailLabel.Text = panelInfo.DetailText;
            }

            if (_jobRows.TryGetValue(jobType, out var row))
            {
                row.TooltipText = panelInfo.DetailText;
            }

            if (_jobPriorityButtons.TryGetValue(jobType, out var priorityButton))
            {
                priorityButton.TooltipText = panelInfo.DetailText;
            }
        }

        if (_peakOverviewLabel != null)
        {
            _peakOverviewLabel.Text = SectOrganizationRules.BuildPeakOverviewText();
            _peakOverviewLabel.TooltipText = "按设定文档汇总当前可见九峰与附属部门结构。";
        }

        RefreshPeakDetailPanel(state);
        ApplyPriorityButtonTexts();
        ApplyJobRowSelectionStyles();
    }

    private void UpdateFigmaPanels(GameState state)
    {
        if (!_isUsingFigmaLayout)
        {
            return;
        }

        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var managementBoost = IndustryRules.GetManagementBoost(state);
        var activeDirection = SectGovernanceRules.GetActiveDevelopmentDefinition(state);
        var activeLaw = SectGovernanceRules.GetActiveLawDefinition(state);
        var activeTalentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(state);

        if (_figmaBuildAgricultureButton != null)
        {
            _figmaBuildAgricultureButton.Text = $"扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} ({state.AgricultureBuildings})";
        }

        if (_figmaBuildWorkshopButton != null)
        {
            _figmaBuildWorkshopButton.Text = $"扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ({state.WorkshopBuildings})";
        }

        if (_figmaCraftToolsButton != null)
        {
            _figmaCraftToolsButton.Text = $"锻制工器 ({state.IndustryTools:0})";
        }

        if (_figmaCenterTitleLabel != null)
        {
            _figmaCenterTitleLabel.Text = "宗主中枢";
        }

        if (_figmaCenterDescriptionLabel != null)
        {
            _figmaCenterDescriptionLabel.Text =
                $"方向 {activeDirection.DisplayName} · 法令 {activeLaw.DisplayName} · 育才 {activeTalentPlan.DisplayName}\n" +
                $"治宗重心 {SectTaskRules.BuildGovernanceHeadline(state)} · {SectTaskRules.BuildGovernanceExecutionSummary(state)} · 管理加成 x{managementBoost:0.00} · 工器覆盖 {toolCoverage * 100:0}%";
        }

        if (_figmaEquipmentTitleLabel != null)
        {
            _figmaEquipmentTitleLabel.Text = "产业基建总览";
        }

        if (_figmaEquipmentSummaryLabel != null)
        {
            _figmaEquipmentSummaryLabel.Text =
                $"战备评分 {state.AvgGearScore:0.0} · 传说 {state.LegendaryGearCount} · 史诗 {state.EpicGearCount}\n" +
                $"精良 {state.RareGearCount} · 普通 {state.CommonGearCount}";
        }

        if (_figmaNotificationLabel != null)
        {
            _figmaNotificationLabel.Text =
                $"探险：{(state.ExplorationEnabled ? "进行中" : "暂停")}\n" +
                $"治理态势：{SectTaskRules.BuildGovernanceExecutionSummary(state)}\n" +
                $"威胁等级：{state.Threat:0}%";
        }
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
        _figmaLayoutRoot = GetNode<Control>(FigmaLayoutPath);
        _isUsingFigmaLayout = _useFigmaLayout;

        if (_isUsingFigmaLayout)
        {
            BindFigmaUiNodes();
        }
        else
        {
            BindLegacyUiNodes();
        }

        SetSpeedScale(1.0);
        if (!_isUsingFigmaLayout)
        {
            SetMapTab(MapTab.Sect);
        }
    }

    private void ConfigureLegacyBackground()
    {
        if (_backgroundTextureRect == null)
        {
            return;
        }

        _backgroundTextureRect.Visible = true;
        _backgroundTextureRect.MouseFilter = MouseFilterEnum.Ignore;
        _backgroundTextureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _backgroundTextureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
        _backgroundTextureRect.ZIndex = -100;
        _backgroundTextureRect.SelfModulate = Colors.White;

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

    private void BindBackgroundResizeEvents()
    {
        if (_backgroundTextureRect == null)
        {
            return;
        }

        Resized += RefreshLegacyBackgroundLayout;
        GetViewport().SizeChanged += RefreshLegacyBackgroundLayout;
        RefreshLegacyBackgroundLayout();
    }

    private void UnbindBackgroundResizeEvents()
    {
        if (_backgroundTextureRect == null)
        {
            return;
        }

        Resized -= RefreshLegacyBackgroundLayout;

        var viewport = GetViewport();
        if (viewport != null)
        {
            viewport.SizeChanged -= RefreshLegacyBackgroundLayout;
        }
    }

    private void RefreshLegacyBackgroundLayout()
    {
        if (_backgroundTextureRect == null)
        {
            return;
        }

        _backgroundTextureRect.SetAnchorsPreset(LayoutPreset.FullRect);
        _backgroundTextureRect.SetOffsetsPreset(LayoutPreset.FullRect);
        _backgroundTextureRect.Position = Vector2.Zero;
        _backgroundTextureRect.Size = GetViewportRect().Size;
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
        _figmaNotificationLabel = null;

        _exploreButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/ExploreButton");
        _speedX1Button = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/SpeedX1Button");
        _speedX2Button = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/SpeedX2Button");
        _speedX4Button = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/SpeedX4Button");
        _saveButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/SaveButton");
        _loadButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/LoadButton");
        _settingsButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/SettingsButton");
        _resetButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/ResetButton");

        _worldMapButton = GetNode<Button>($"{CenterTopTabRowPath}/WorldMapButton");
        _prefectureMapButton = GetNode<Button>($"{CenterTopTabRowPath}/PrefectureMapButton");
        _countyTownMapButton = GetNode<Button>($"{CenterTopTabRowPath}/CountyTownMapButton");
        _eventPanelButton = GetNode<Button>($"{CenterTopTabRowPath}/EventPanelButton");
        _reportPanelButton = GetNode<Button>($"{CenterTopTabRowPath}/ReportPanelButton");
        _expeditionMapButton = GetNode<Button>($"{CenterTopTabRowPath}/ExpeditionMapButton");
        _mapZoomOutButton = GetNode<Button>($"{CenterTopTabRowPath}/MapZoomOutButton");
        _mapZoomInButton = GetNode<Button>($"{CenterTopTabRowPath}/MapZoomInButton");
        _mapZoomResetButton = GetNode<Button>($"{CenterTopTabRowPath}/MapZoomResetButton");
        _mapZoomLabel = GetNode<Label>($"{CenterTopTabRowPath}/MapZoomLabel");
        BindMapOperationalLegacyNodes();

        _worldMapView = GetNode<Control>($"{CenterMapPagesPath}/WorldMapView");
        _prefectureMapView = GetNode<Control>($"{CenterMapPagesPath}/PrefectureMapView");
        _countyTownMapView = GetNode<Control>($"{CenterMapPagesPath}/CountyTownMapView");
        _eventPanelView = GetNode<Control>($"{CenterMapPagesPath}/EventPanelView");
        _reportPanelView = GetNode<Control>($"{CenterMapPagesPath}/ReportPanelView");
        _expeditionMapView = GetNode<Control>($"{CenterMapPagesPath}/ExpeditionMapView");
        _sectMapRenderer = GetNode<SectMapViewSystem>($"{CenterMapPagesPath}/CountyTownMapView");
        _mineUpgradeButton = GetNodeOrNull<Button>($"{CenterReportDetailPagesPath}/MineCard/CardVBox/UpgradeButton");
        BindSectTileInspectorNodes();
        BindSectChronicleNodes();
        ClearLegacyJobsPaddingBindings();

        _figmaBuildAgricultureButton = null;
        _figmaBuildWorkshopButton = null;
        _figmaCraftToolsButton = null;
        _figmaCenterTitleLabel = null;
        _figmaCenterDescriptionLabel = null;
        _figmaEquipmentTitleLabel = null;
        _figmaEquipmentSummaryLabel = null;
    }

    private void BindFigmaUiNodes()
    {
        _populationLabel = GetNode<Label>($"{FigmaTopBarPath}/ResourceRow/PopulationLabel");
        _happinessLabel = GetNode<Label>($"{FigmaTopBarPath}/ResourceRow/MoraleLabel");
        _foodLabel = GetNode<Label>($"{FigmaTopBarPath}/ResourceRow/FoodLabel");
        _woodLabel = GetNode<Label>($"{FigmaTopBarPath}/ResourceRow/StoneLabel");
        _goldLabel = GetNode<Label>($"{FigmaTopBarPath}/ResourceRow/MoneyLabel");
        _threatLabel = GetNode<Label>($"{FigmaTopBarPath}/ResourceRow/FireLabel");
        _techLabel = GetNode<Label>($"{FigmaTopBarPath}/ResourceRow/QiLabel");
        _calendarLabel = GetNode<Label>($"{FigmaTopBarPath}/CalendarBox/EraLabel");
        _calendarDetailLabel = GetNode<Label>($"{FigmaTopBarPath}/CalendarBox/DetailLabel");
        _calendarQuarterProgress = GetNode<ProgressBar>($"{FigmaTopBarPath}/CalendarBox/QuarterProgressRow/QuarterProgress");
        _calendarDayProgress = GetNode<ProgressBar>($"{FigmaTopBarPath}/CalendarBox/DayProgressRow/DayProgress");

        _figmaNotificationLabel = GetNode<RichTextLabel>($"{FigmaTimelinePath}/NotificationSection/NotificationContent");
        _logLabel = GetNode<RichTextLabel>($"{FigmaTimelinePath}/TimelineSection/TimelineContent");

        _exploreButton = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/LeftActions/ExploreButton");
        _speedX1Button = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/LeftActions/SpeedX1Button");
        _speedX2Button = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/LeftActions/SpeedX2Button");
        _speedX4Button = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/LeftActions/SpeedX4Button");
        _saveButton = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/RightActions/SaveButton");
        _loadButton = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/RightActions/LoadButton");
        _settingsButton = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/RightActions/SettingsButton");
        _resetButton = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/RightActions/AlertButton");

        _figmaBuildAgricultureButton = GetNode<Button>($"{FigmaCenterPath}/TopActionRow/LeftActionButton");
        _figmaBuildWorkshopButton = GetNode<Button>($"{FigmaCenterPath}/TopActionRow/MiddleActionButton");
        _figmaCraftToolsButton = GetNode<Button>($"{FigmaCenterPath}/TopActionRow/RightActionButton");

        _figmaCenterTitleLabel = GetNode<Label>($"{FigmaCenterPath}/CenterInfoBox/CenterInfoColumn/CenterTitleLabel");
        _figmaCenterDescriptionLabel = GetNode<Label>($"{FigmaCenterPath}/CenterInfoBox/CenterInfoColumn/CenterDescriptionLabel");
        _figmaEquipmentTitleLabel = GetNode<Label>($"{FigmaEquipmentPath}/TitleLabel");
        _figmaEquipmentSummaryLabel = GetNode<Label>($"{FigmaEquipmentPath}/SummaryPanel/SummaryLabel");

        _worldMapButton = null;
        _prefectureMapButton = null;
        _countyTownMapButton = null;
        _eventPanelButton = null;
        _reportPanelButton = null;
        _expeditionMapButton = null;
        _mapZoomOutButton = null;
        _mapZoomInButton = null;
        _mapZoomResetButton = null;
        _mapZoomLabel = null;
        _worldMapView = null;
        _prefectureMapView = null;
        _countyTownMapView = null;
        _eventPanelView = null;
        _reportPanelView = null;
        _expeditionMapView = null;
        _mineUpgradeButton = null;
        _sectMapRenderer = null;
        ClearMapOperationalNodes();
        ClearLegacyJobsPaddingBindings();
        ClearSectTileInspectorNodes();
        ClearSectChronicleNodes();
    }

    private void ApplyLayoutSwitch()
    {
        _legacyLayoutRoot.Visible = !_useFigmaLayout;
        _figmaLayoutRoot.Visible = _useFigmaLayout;
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

        if (_worldMapButton != null &&
            _prefectureMapButton != null &&
            _countyTownMapButton != null &&
            _eventPanelButton != null &&
            _reportPanelButton != null &&
            _expeditionMapButton != null)
        {
            _worldMapButton.Pressed += () => SetMapTab(MapTab.World);
            _prefectureMapButton.Pressed += () => SetMapTab(MapTab.Prefecture);
            _countyTownMapButton.Pressed += () => SetMapTab(MapTab.Sect);
            _eventPanelButton.Pressed += () => SetMapTab(MapTab.EventPanel);
            _reportPanelButton.Pressed += () => SetMapTab(MapTab.ReportPanel);
            _expeditionMapButton.Pressed += () => SetMapTab(MapTab.Expedition);
        }

        if (_mapZoomOutButton != null && _mapZoomInButton != null && _mapZoomResetButton != null)
        {
            _mapZoomOutButton.Pressed += () => AdjustCurrentMapZoom(-MapZoomStep);
            _mapZoomInButton.Pressed += () => AdjustCurrentMapZoom(MapZoomStep);
            _mapZoomResetButton.Pressed += ResetCurrentMapZoom;
        }

        BindMapOperationalEvents();

        _saveButton.Pressed += OpenSaveSlotsPanelForSave;
        _loadButton.Pressed += OpenSaveSlotsPanelForLoad;

        _resetButton.Pressed += () => _gameLoop.ResetState();

        if (_isUsingFigmaLayout)
        {
            BindFigmaIndustryButtons();
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
        _jobCountLabels.Clear();
        _jobTitleLabels.Clear();
        _jobStatusLabels.Clear();
        _jobDetailLabels.Clear();
        _jobRows.Clear();
        _jobPriorityButtons.Clear();
        _jobRowBaseStyles.Clear();
        _inspectedJobType = null;
    }

    private void BindFigmaIndustryButtons()
    {
        if (_figmaBuildAgricultureButton != null)
        {
            _figmaBuildAgricultureButton.Pressed += () => _gameLoop.BuildIndustryBuilding(IndustryBuildingType.Agriculture);
        }

        if (_figmaBuildWorkshopButton != null)
        {
            _figmaBuildWorkshopButton.Pressed += () => _gameLoop.BuildIndustryBuilding(IndustryBuildingType.Workshop);
        }

        if (_figmaCraftToolsButton != null)
        {
            _figmaCraftToolsButton.Pressed += _gameLoop.CraftIndustryTools;
        }

        _saveButton.Text = "【存】";
        _loadButton.Text = "【读】";
        _settingsButton.Text = "【设】";
        _resetButton.Text = "【重】";
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
            _prefectureMapView == null ||
            _countyTownMapView == null ||
            _eventPanelView == null ||
            _reportPanelView == null ||
            _expeditionMapView == null ||
            _worldMapButton == null ||
            _prefectureMapButton == null ||
            _countyTownMapButton == null ||
            _eventPanelButton == null ||
            _reportPanelButton == null ||
            _expeditionMapButton == null)
        {
            return;
        }

        _worldMapView.Visible = mapTab == MapTab.World;
        _prefectureMapView.Visible = mapTab == MapTab.Prefecture;
        _countyTownMapView.Visible = mapTab == MapTab.Sect;
        _eventPanelView.Visible = mapTab == MapTab.EventPanel;
        _reportPanelView.Visible = mapTab == MapTab.ReportPanel;
        _expeditionMapView.Visible = mapTab == MapTab.Expedition;

        _worldMapButton.ButtonPressed = mapTab == MapTab.World;
        _prefectureMapButton.ButtonPressed = mapTab == MapTab.Prefecture;
        _countyTownMapButton.ButtonPressed = mapTab == MapTab.Sect;
        _eventPanelButton.ButtonPressed = mapTab == MapTab.EventPanel;
        _reportPanelButton.ButtonPressed = mapTab == MapTab.ReportPanel;
        _expeditionMapButton.ButtonPressed = mapTab == MapTab.Expedition;

        _currentMapTab = mapTab;
        RefreshMapZoomUi();
        RefreshMapOperationalLinkUi();
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

        mapView.SetZoom(mapView.DefaultZoom);
        RefreshMapZoomUi();
    }

    private IMapZoomView? GetActiveMapZoomView()
    {
        return _currentMapTab switch
        {
            MapTab.World => _worldMapView as IMapZoomView,
            MapTab.Sect => _countyTownMapView as IMapZoomView,
            _ => null
        };
    }

    private void ConfigureDualMapMode()
    {
        if (_countyTownMapButton != null)
        {
            _countyTownMapButton.Text = "山门沙盘";
            _countyTownMapButton.TooltipText = "查看天衍峰卷内六边形山门图、门人活动与场所状态";
        }

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
        }

        if (view != null)
        {
            view.Visible = false;
        }
    }

    private void RefreshMapZoomUi()
    {
        if (_mapZoomOutButton == null || _mapZoomInButton == null || _mapZoomResetButton == null || _mapZoomLabel == null)
        {
            return;
        }

        var mapView = GetActiveMapZoomView();
        var canZoom = mapView != null;
        _mapZoomOutButton.Disabled = !canZoom;
        _mapZoomInButton.Disabled = !canZoom;
        _mapZoomResetButton.Disabled = !canZoom;

        if (!canZoom || mapView == null)
        {
            _mapZoomLabel.Text = "--";
            return;
        }

        _mapZoomLabel.Text = $"{(int)Mathf.Round(mapView.Zoom * 100f)}%";
    }

    private void BindJobButtons(JobType jobType, string minusButtonPath, string plusButtonPath)
    {
        var minusButton = GetNode<Button>(minusButtonPath);
        var plusButton = GetNode<Button>(plusButtonPath);
        minusButton.Text = "法";
        plusButton.Text = "旨";
        minusButton.TooltipText = "岗位已改为宗主定调，点击打开宗主中枢。";
        plusButton.TooltipText = "岗位已改为宗主定调，点击打开宗主中枢。";
        minusButton.Pressed += () => OpenTaskPanelForJob(jobType);
        plusButton.Pressed += () => OpenTaskPanelForJob(jobType);
    }

    private void BindJobPriorityButtons()
    {
        foreach (var entry in _jobPriorityButtons)
        {
            var jobType = entry.Key;
            entry.Value.Pressed += () => OnJobPriorityPressed(jobType);
        }
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

    private void BindJobRowInteractions()
    {
        foreach (var entry in _jobRows)
        {
            var jobType = entry.Key;
            entry.Value.GuiInput += @event => OnJobRowGuiInput(jobType, @event);
        }
    }

    private void OnJobRowGuiInput(JobType jobType, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton ||
            mouseButton.ButtonIndex != MouseButton.Left ||
            !mouseButton.Pressed)
        {
            return;
        }

        var panelInfo = SectTaskRules.GetJobPanelInfo(_gameLoop.State, jobType);
        if (_inspectedJobType == jobType)
        {
            _inspectedJobType = null;
            AppendLog($"收起 {panelInfo.ActiveRoleName} 执行摘要。");
        }
        else
        {
            _inspectedJobType = jobType;
            _selectedPeakIndex = SectOrganizationRules.GetRecommendedPeakIndex(jobType);
            AppendLog($"展开 {panelInfo.ActiveRoleName} 执行摘要，定位至 {SectOrganizationRules.GetPeakTitle(_selectedPeakIndex)}。");
        }

        RefreshJobPanels(_gameLoop.State);
    }

    private void OnJobPriorityPressed(JobType jobType)
    {
        OpenTaskPanelForJob(jobType);
        AppendLog($"{SectTaskRules.GetJobButtonText(jobType)}已转到宗主中枢。");
    }

    private void ApplyPriorityButtonTexts()
    {
        if (_jobPriorityButtons.Count == 0)
        {
            return;
        }

        foreach (var entry in _jobPriorityButtons)
        {
            entry.Value.Text = SectTaskRules.GetJobButtonText(entry.Key);
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

    private void RegisterJobRow(JobType jobType, string rowPath, string detailLabelPath)
    {
        var row = GetNode<PanelContainer>(rowPath);
        var detailLabel = GetNode<Label>(detailLabelPath);
        detailLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        detailLabel.Visible = false;
        row.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
        row.TooltipText = "点击查看职司摘要并定位关联峰脉。";
        _jobRows[jobType] = row;
        _jobDetailLabels[jobType] = detailLabel;

        if (row.GetThemeStylebox("panel") is StyleBoxFlat styleBox)
        {
            _jobRowBaseStyles[jobType] = (StyleBoxFlat)styleBox.Duplicate();
        }
    }

    private void ApplyJobRowSelectionStyles()
    {
        foreach (var entry in _jobRows)
        {
            if (!_jobRowBaseStyles.TryGetValue(entry.Key, out var baseStyle))
            {
                continue;
            }

            var isInspected = _inspectedJobType == entry.Key;
            var displayStyle = (StyleBoxFlat)baseStyle.Duplicate();
            var borderWidth = isInspected ? 2 : 1;
            displayStyle.BorderWidthLeft = borderWidth;
            displayStyle.BorderWidthTop = borderWidth;
            displayStyle.BorderWidthRight = borderWidth;
            displayStyle.BorderWidthBottom = borderWidth;
            displayStyle.BorderColor = isInspected
                ? new Color(0.831373f, 0.662745f, 0.32549f, 1.0f)
                : baseStyle.BorderColor;
            entry.Value.AddThemeStyleboxOverride("panel", displayStyle);
        }
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
        var buttons = new List<Button>();
        CollectButtons(this, buttons);

        foreach (var button in buttons)
        {
            button.SelfModulate = BaseButtonModulate;
            button.MouseEntered += () => StartLanternPulse(button);
            button.MouseExited += () => StopLanternPulse(button);
            button.FocusEntered += () => StartLanternPulse(button);
            button.FocusExited += () => StopLanternPulse(button);
        }
    }

    private static void CollectButtons(Node node, List<Button> result)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Button button)
            {
                result.Add(button);
            }

            CollectButtons(child, result);
        }
    }

    private void StartLanternPulse(Button button)
    {
        StopLanternPulse(button, smoothReset: false);

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);
        tween.SetLoops();
        tween.TweenProperty(button, "self_modulate", LanternGlowModulate, LanternPulseDurationSeconds);
        tween.TweenProperty(button, "self_modulate", BaseButtonModulate, LanternPulseDurationSeconds);

        _buttonPulseTweens[button] = tween;
    }

    private void StopLanternPulse(Button button, bool smoothReset = true)
    {
        if (_buttonPulseTweens.TryGetValue(button, out var tween))
        {
            tween.Kill();
            _buttonPulseTweens.Remove(button);
        }

        if (!smoothReset)
        {
            button.SelfModulate = BaseButtonModulate;
            return;
        }

        var resetTween = CreateTween();
        resetTween.SetTrans(Tween.TransitionType.Sine);
        resetTween.SetEase(Tween.EaseType.Out);
        resetTween.TweenProperty(button, "self_modulate", BaseButtonModulate, LanternResetDurationSeconds);
    }
}



