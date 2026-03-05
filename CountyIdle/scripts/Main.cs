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
        World,
        Prefecture,
        CountyTown
    }

    private const string MainLayoutPath = "RootMargin/MainLayout";
    private const string TopBarPath = $"{MainLayoutPath}/TopBar";
    private const string BodyRowPath = $"{MainLayoutPath}/BodyRow";
    private const string LeftPanelPath = $"{BodyRowPath}/LeftPanel";
    private const string RightPanelPath = $"{BodyRowPath}/RightPanel";
    private const string BottomBarPath = $"{MainLayoutPath}/BottomBar";
    private const string LegacyLayoutPath = "RootMargin";
    private const string FigmaLayoutPath = "FigmaLayout";

    private const string FigmaRootPath = $"{FigmaLayoutPath}/RootMargin/MainColumn";
    private const string FigmaTopBarPath = $"{FigmaRootPath}/HUDTopBar/BarPadding/MainRow";
    private const string FigmaBottomBarPath = $"{FigmaRootPath}/BottomBar/PanelPadding/MainColumn";
    private const string FigmaCenterPath = $"{FigmaRootPath}/BodyRow/CenterView/PanelPadding/MainColumn";
    private const string FigmaTimelinePath = $"{FigmaRootPath}/BodyRow/TimelinePanel/PanelPadding/MainColumn";
    private const string FigmaEquipmentPath = $"{FigmaRootPath}/BodyRow/EquipmentPanel/PanelPadding/MainColumn";

    private const int JobAdjustStep = 1;
    private const double SettlementDurationSeconds = 60.0;
    private const double LanternPulseDurationSeconds = 0.42;
    private const double LanternResetDurationSeconds = 0.14;
    private const int MineUnlockTechLevel = 1;
    private const string MineLockedText = "🔒 需科技 锻造术(T1)";
    private const string MineUnlockedText = "↑ 扩建学宫 (木16 石22 金22)";

    private static readonly Color BaseButtonModulate = Colors.White;
    private static readonly Color LanternGlowModulate = new(1.0f, 0.86f, 0.64f, 1.0f);

    private readonly Queue<string> _logs = new();
    private readonly Dictionary<Button, Tween> _buttonPulseTweens = new();
    private readonly Dictionary<JobType, Label> _jobCountLabels = new();
    private readonly Dictionary<JobType, Label> _jobTitleLabels = new();
    private readonly Dictionary<JobType, Label> _jobStatusLabels = new();
    private readonly Dictionary<JobType, Button> _jobPriorityButtons = new();
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
    private Label _cycleLabel = null!;
    private Label? _countdownLabel;
    private ProgressBar? _settlementProgress;
    private Button _exploreButton = null!;
    private Button? _speedX1Button;
    private Button? _speedX2Button;
    private Button _saveButton = null!;
    private Button _loadButton = null!;
    private Button _resetButton = null!;
    private Button? _worldMapButton;
    private Button? _prefectureMapButton;
    private Button? _countyTownMapButton;
    private Control? _worldMapView;
    private Control? _prefectureMapView;
    private Control? _countyTownMapView;
    private Button? _mineUpgradeButton;
    private Control _legacyLayoutRoot = null!;
    private Control _figmaLayoutRoot = null!;
    private CountyTownMapViewSystem? _countyTownMapRenderer;

    private Button? _figmaBuildAgricultureButton;
    private Button? _figmaBuildWorkshopButton;
    private Button? _figmaCraftToolsButton;
    private Label? _figmaCenterTitleLabel;
    private Label? _figmaCenterDescriptionLabel;
    private Label? _figmaEquipmentTitleLabel;
    private Label? _figmaEquipmentSummaryLabel;
    private RichTextLabel? _figmaNotificationLabel;

    private bool _isUsingFigmaLayout;
    private bool _isSpeedX2;
    private JobType? _priorityJobType = JobType.Farmer;
    private double _settlementCountdownSeconds = SettlementDurationSeconds;

    public override void _Ready()
    {
        BindUiNodes();
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

        _settlementCountdownSeconds -= delta;
        while (_settlementCountdownSeconds <= 0)
        {
            _settlementCountdownSeconds += SettlementDurationSeconds;
        }

        UpdateSettlementCountdownUi();
    }

    public override void _ExitTree()
    {
        foreach (var tween in _buttonPulseTweens.Values)
        {
            tween.Kill();
        }

        _buttonPulseTweens.Clear();
    }

    private void SetupGameLoop()
    {
        _gameLoop = new GameLoop();
        AddChild(_gameLoop);
        _gameLoop.SetTimeScale(_isSpeedX2 ? 2.0 : 1.0);
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
        _populationLabel.Text = $"👥 人口 {state.Population}";
        _happinessLabel.Text = $"💖 民心 {state.Happiness:0.#}";

        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var managementBoost = IndustryRules.GetManagementBoost(state);

        var foodDelta = (state.Farmers * 0.20 * state.FoodProductionMultiplier * managementBoost * toolCoverage) - (state.Population * 0.06);
        var woodDelta = state.Farmers * 0.09 * state.IndustryProductionMultiplier * managementBoost * toolCoverage;
        var goldDelta = (state.Merchants * 0.17 * state.TradeProductionMultiplier * managementBoost * toolCoverage) - (state.Population * 0.03);

        _foodLabel.Text = $"🌾 粮 {state.Food:0} {FormatSigned(foodDelta)}/s";
        _woodLabel.Text = $"🪵 木 {state.Wood:0} {FormatSigned(woodDelta)}/s";
        _goldLabel.Text = $"💰 金 {state.Gold:0} {FormatSigned(goldDelta)}/s";
        _threatLabel.Text = $"⚔ 威胁 {state.Threat:0}%";
        _techLabel.Text = $"📜 科技 T{Math.Max(state.TechLevel + 1, 1)}";

        var era = (state.HourSettlements / 12) + 1;
        var cycle = (state.HourSettlements % 12) + 1;
        _cycleLabel.Text = $"纪元 {era} · 第 {cycle} 周期";

        _settlementCountdownSeconds = SettlementDurationSeconds - (state.GameMinutes % 60);
        if (_settlementCountdownSeconds <= 0)
        {
            _settlementCountdownSeconds = SettlementDurationSeconds;
        }

        _exploreButton.Text = _isUsingFigmaLayout
            ? (state.ExplorationEnabled ? "⏸ 探险中" : "▶ 探险")
            : (state.ExplorationEnabled ? "⏸" : "▶");

        _countyTownMapRenderer?.RefreshMap(state.Population, state.HousingCapacity, state.ElitePopulation);

        RefreshJobPanels(state);
        RefreshMineButtonState(state);
        UpdateFigmaPanels(state);
        UpdateSettlementCountdownUi();
    }

    private static string FormatSigned(double value)
    {
        return value >= 0 ? $"+{value:0.#}" : $"{value:0.#}";
    }

    private void UpdateSettlementCountdownUi()
    {
        if (_countdownLabel == null || _settlementProgress == null)
        {
            return;
        }

        var seconds = Math.Max((int)Math.Ceiling(_settlementCountdownSeconds), 0);
        _countdownLabel.Text = $"距离大暑结算还有 {seconds} 秒";
        _settlementProgress.Value = SettlementDurationSeconds - _settlementCountdownSeconds;
    }

    private void RefreshJobPanels(GameState state)
    {
        if (_jobCountLabels.Count == 0)
        {
            return;
        }

        _jobCountLabels[JobType.Farmer].Text = state.Farmers.ToString();
        _jobCountLabels[JobType.Worker].Text = state.Workers.ToString();
        _jobCountLabels[JobType.Merchant].Text = state.Merchants.ToString();
        _jobCountLabels[JobType.Scholar].Text = state.Scholars.ToString();

        _jobTitleLabels[JobType.Farmer].Text = "🌾 农工岗";
        _jobTitleLabels[JobType.Worker].Text = "⛏ 匠役岗";
        _jobTitleLabels[JobType.Merchant].Text = "💰 商贾岗";
        _jobTitleLabels[JobType.Scholar].Text = "📜 学士岗";

        var commerceCap = IndustryRules.GetCommerceCapacity(state);
        var researchCap = IndustryRules.GetResearchCapacity(state);
        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var managementBoost = IndustryRules.GetManagementBoost(state);

        _jobStatusLabels[JobType.Farmer].Text = $"领队: 张三 (+15%) · 工具 {state.IndustryTools:0}";
        _jobStatusLabels[JobType.Worker].Text = $"无领队 · 组织效率 x{managementBoost:0.00}";
        _jobStatusLabels[JobType.Merchant].Text = $"无领队 · 商贸容量 {commerceCap} · 覆盖 {toolCoverage * 100:0}%";
        _jobStatusLabels[JobType.Scholar].Text = researchCap > 0 ? $"无领队 · 研修容量 {researchCap}" : "全员休沐中";
    }

    private void UpdateFigmaPanels(GameState state)
    {
        if (!_isUsingFigmaLayout)
        {
            return;
        }

        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var managementBoost = IndustryRules.GetManagementBoost(state);
        var unassigned = state.GetUnassignedPopulation();

        if (_figmaBuildAgricultureButton != null)
        {
            _figmaBuildAgricultureButton.Text = $"扩建农坊 ({state.AgricultureBuildings})";
        }

        if (_figmaBuildWorkshopButton != null)
        {
            _figmaBuildWorkshopButton.Text = $"扩建工坊 ({state.WorkshopBuildings})";
        }

        if (_figmaCraftToolsButton != null)
        {
            _figmaCraftToolsButton.Text = $"锻造工具 ({state.IndustryTools:0})";
        }

        if (_figmaCenterTitleLabel != null)
        {
            _figmaCenterTitleLabel.Text = "郡县营建中枢";
        }

        if (_figmaCenterDescriptionLabel != null)
        {
            _figmaCenterDescriptionLabel.Text =
                $"空闲人口 {unassigned} · 管理加成 x{managementBoost:0.00} · 工具覆盖 {toolCoverage * 100:0}%";
        }

        if (_figmaEquipmentTitleLabel != null)
        {
            _figmaEquipmentTitleLabel.Text = "产业基建总览";
        }

        if (_figmaEquipmentSummaryLabel != null)
        {
            _figmaEquipmentSummaryLabel.Text =
                $"农坊 {state.AgricultureBuildings} · 工坊 {state.WorkshopBuildings} · 学宫 {state.ResearchBuildings}";
        }

        if (_figmaNotificationLabel != null)
        {
            _figmaNotificationLabel.Text =
                $"探险：{(state.ExplorationEnabled ? "进行中" : "暂停")}\n" +
                $"空闲人口：{unassigned}\n" +
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
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        var coloredMessage = ColorizeLogMessage(message);
        _logs.Enqueue($"[{hours:00}:{minutes:00}] {coloredMessage}");

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
            return $"[color=#df6f6f]{message}[/color]";
        }

        if (message.Contains("获得", StringComparison.Ordinal) ||
            message.Contains("胜利", StringComparison.Ordinal) ||
            message.Contains("成功", StringComparison.Ordinal))
        {
            return $"[color=#59c995]{message}[/color]";
        }

        if (message.Contains("存档", StringComparison.Ordinal) ||
            message.Contains("读档", StringComparison.Ordinal))
        {
            return $"[color=#7c9dff]{message}[/color]";
        }

        return $"[color=#6f82b8]{message}[/color]";
    }

    private void BindUiNodes()
    {
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

        SetSpeedMode(false);
        if (!_isUsingFigmaLayout)
        {
            SetMapTab(MapTab.CountyTown);
        }
    }

    private void BindLegacyUiNodes()
    {
        _populationLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/PopulationLabel");
        _happinessLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/HappinessLabel");
        _foodLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/FoodLabel");
        _woodLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/WoodLabel");
        _goldLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/GoldLabel");
        _threatLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/ThreatLabel");
        _techLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/StatsRow/TechLabel");
        _cycleLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/CycleBox/CycleLabel");
        _countdownLabel = GetNode<Label>($"{TopBarPath}/BarContent/MainRow/CycleBox/CountdownLabel");
        _settlementProgress = GetNode<ProgressBar>($"{TopBarPath}/BarContent/MainRow/CycleBox/SettlementProgress");

        _logLabel = GetNode<RichTextLabel>($"{RightPanelPath}/PanelContent/MainVBox/LogBox/LogVBox/LogContent/LogLabel");
        _figmaNotificationLabel = null;

        _exploreButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/ExploreButton");
        _speedX1Button = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/SpeedX1Button");
        _speedX2Button = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/SpeedRow/SpeedX2Button");
        _saveButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/SaveButton");
        _loadButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/LoadButton");
        _resetButton = GetNode<Button>($"{BottomBarPath}/BarPadding/MainRow/ActionRow/ResetButton");

        _worldMapButton = GetNode<Button>($"{BodyRowPath}/CenterPanel/PanelContent/MapViewport/TopTabRow/WorldMapButton");
        _prefectureMapButton = GetNode<Button>($"{BodyRowPath}/CenterPanel/PanelContent/MapViewport/TopTabRow/PrefectureMapButton");
        _countyTownMapButton = GetNode<Button>($"{BodyRowPath}/CenterPanel/PanelContent/MapViewport/TopTabRow/CountyTownMapButton");

        _worldMapView = GetNode<Control>($"{BodyRowPath}/CenterPanel/PanelContent/MapViewport/MapPages/WorldMapView");
        _prefectureMapView = GetNode<Control>($"{BodyRowPath}/CenterPanel/PanelContent/MapViewport/MapPages/PrefectureMapView");
        _countyTownMapView = GetNode<Control>($"{BodyRowPath}/CenterPanel/PanelContent/MapViewport/MapPages/CountyTownMapView");
        _countyTownMapRenderer = GetNode<CountyTownMapViewSystem>($"{BodyRowPath}/CenterPanel/PanelContent/MapViewport/MapPages/CountyTownMapView");
        _mineUpgradeButton = GetNodeOrNull<Button>($"{BodyRowPath}/CenterPanel/PanelContent/InfrastructureScroll/DetailPages/MineCard/CardVBox/UpgradeButton");

        _jobCountLabels.Clear();
        _jobTitleLabels.Clear();
        _jobStatusLabels.Clear();
        _jobPriorityButtons.Clear();

        _jobCountLabels[JobType.Farmer] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/FarmerRow/RowVBox/ControlRow/Stepper/StepperRow/CountPanel/CountLabel");
        _jobCountLabels[JobType.Worker] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/WorkerRow/RowVBox/ControlRow/Stepper/StepperRow/CountPanel/CountLabel");
        _jobCountLabels[JobType.Merchant] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/MerchantRow/RowVBox/ControlRow/Stepper/StepperRow/CountPanel/CountLabel");
        _jobCountLabels[JobType.Scholar] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/ScholarRow/RowVBox/ControlRow/Stepper/StepperRow/CountPanel/CountLabel");

        _jobTitleLabels[JobType.Farmer] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/FarmerRow/RowVBox/HeaderRow/JobLabel");
        _jobTitleLabels[JobType.Worker] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/WorkerRow/RowVBox/HeaderRow/JobLabel");
        _jobTitleLabels[JobType.Merchant] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/MerchantRow/RowVBox/HeaderRow/JobLabel");
        _jobTitleLabels[JobType.Scholar] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/ScholarRow/RowVBox/HeaderRow/JobLabel");

        _jobStatusLabels[JobType.Farmer] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/FarmerRow/RowVBox/ControlRow/EliteBox/EliteLabel");
        _jobStatusLabels[JobType.Worker] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/WorkerRow/RowVBox/ControlRow/EliteBox/EliteLabel");
        _jobStatusLabels[JobType.Merchant] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/MerchantRow/RowVBox/ControlRow/EliteBox/EliteLabel");
        _jobStatusLabels[JobType.Scholar] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/ScholarRow/RowVBox/ControlRow/EliteBox/EliteLabel");
        _jobPriorityButtons[JobType.Farmer] = GetNode<Button>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/FarmerRow/RowVBox/HeaderRow/PriorityLabel");
        _jobPriorityButtons[JobType.Worker] = GetNode<Button>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/WorkerRow/RowVBox/HeaderRow/PriorityLabel");
        _jobPriorityButtons[JobType.Merchant] = GetNode<Button>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/MerchantRow/RowVBox/HeaderRow/PriorityLabel");
        _jobPriorityButtons[JobType.Scholar] = GetNode<Button>($"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/ScholarRow/RowVBox/HeaderRow/PriorityLabel");
        ApplyPriorityButtonTexts();

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
        _cycleLabel = GetNode<Label>($"{FigmaTopBarPath}/EraLabel");

        _countdownLabel = null;
        _settlementProgress = null;

        _figmaNotificationLabel = GetNode<RichTextLabel>($"{FigmaTimelinePath}/NotificationSection/NotificationContent");
        _logLabel = GetNode<RichTextLabel>($"{FigmaTimelinePath}/TimelineSection/TimelineContent");

        _exploreButton = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/LeftActions/SpeedButton");
        _speedX1Button = null;
        _speedX2Button = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/LeftActions/BurstButton");
        _saveButton = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/RightActions/SaveButton");
        _loadButton = GetNode<Button>($"{FigmaBottomBarPath}/ActionRow/RightActions/HelpButton");
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
        _worldMapView = null;
        _prefectureMapView = null;
        _countyTownMapView = null;
        _mineUpgradeButton = null;
        _countyTownMapRenderer = null;

        _jobCountLabels.Clear();
        _jobTitleLabels.Clear();
        _jobStatusLabels.Clear();
        _jobPriorityButtons.Clear();
        _priorityJobType = null;
    }

    private void ApplyLayoutSwitch()
    {
        _legacyLayoutRoot.Visible = !_useFigmaLayout;
        _figmaLayoutRoot.Visible = _useFigmaLayout;
    }

    private void BindUiEvents()
    {
        _exploreButton.Pressed += () => _gameLoop.ToggleExploration();

        if (_speedX1Button != null)
        {
            _speedX1Button.Pressed += () =>
            {
                SetSpeedMode(false);
                AppendLog("时间倍率切换至 x1。");
            };
        }

        if (_speedX2Button != null)
        {
            _speedX2Button.Pressed += () =>
            {
                SetSpeedMode(!_isSpeedX2);
                AppendLog(_isSpeedX2 ? "时间倍率切换至 x2。" : "时间倍率切换至 x1。");
            };
        }

        if (_worldMapButton != null && _prefectureMapButton != null && _countyTownMapButton != null)
        {
            _worldMapButton.Pressed += () => SetMapTab(MapTab.World);
            _prefectureMapButton.Pressed += () => SetMapTab(MapTab.Prefecture);
            _countyTownMapButton.Pressed += () => SetMapTab(MapTab.CountyTown);
        }

        _saveButton.Pressed += () =>
        {
            _saveSystem.Save(_gameLoop.State, out var msg);
            AppendLog(msg);
        };

        _loadButton.Pressed += () =>
        {
            _saveSystem.TryLoad(out var state, out var msg);
            _gameLoop.LoadState(state);
            AppendLog(msg);
        };

        _resetButton.Pressed += () => _gameLoop.ResetState();

        if (_isUsingFigmaLayout)
        {
            BindFigmaIndustryButtons();
            return;
        }

        BindJobButtons(
            JobType.Farmer,
            $"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/FarmerRow/RowVBox/ControlRow/Stepper/StepperRow/MinusButton",
            $"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/FarmerRow/RowVBox/ControlRow/Stepper/StepperRow/PlusButton");
        BindJobButtons(
            JobType.Worker,
            $"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/WorkerRow/RowVBox/ControlRow/Stepper/StepperRow/MinusButton",
            $"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/WorkerRow/RowVBox/ControlRow/Stepper/StepperRow/PlusButton");
        BindJobButtons(
            JobType.Merchant,
            $"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/MerchantRow/RowVBox/ControlRow/Stepper/StepperRow/MinusButton",
            $"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/MerchantRow/RowVBox/ControlRow/Stepper/StepperRow/PlusButton");
        BindJobButtons(
            JobType.Scholar,
            $"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/ScholarRow/RowVBox/ControlRow/Stepper/StepperRow/MinusButton",
            $"{LeftPanelPath}/PanelContent/JobsVBox/JobsPadding/JobsList/ScholarRow/RowVBox/ControlRow/Stepper/StepperRow/PlusButton");
        BindJobPriorityButtons();

        BindLegacyIndustryButtons();
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

        _saveButton.Text = "💾 存档";
        _loadButton.Text = "📂 读档";
        _resetButton.Text = "↺ 重置";
    }

    private void BindLegacyIndustryButtons()
    {
        var agricultureButton = GetNodeOrNull<Button>($"{BodyRowPath}/CenterPanel/PanelContent/InfrastructureScroll/DetailPages/MulberryCard/CardVBox/UpgradeButton");
        if (agricultureButton != null)
        {
            agricultureButton.Pressed += () => _gameLoop.BuildIndustryBuilding(IndustryBuildingType.Agriculture);
        }

        var workshopButton = GetNodeOrNull<Button>($"{BodyRowPath}/CenterPanel/PanelContent/InfrastructureScroll/DetailPages/LumberCard/CardVBox/UpgradeButton");
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

    private void SetSpeedMode(bool isX2)
    {
        _isSpeedX2 = isX2;

        if (_gameLoop != null)
        {
            _gameLoop.SetTimeScale(_isSpeedX2 ? 2.0 : 1.0);
        }

        if (_speedX1Button != null)
        {
            _speedX1Button.ButtonPressed = !_isSpeedX2;
            _speedX1Button.Text = "▶ x1";
        }

        if (_speedX2Button != null)
        {
            _speedX2Button.ButtonPressed = _isSpeedX2;
            _speedX2Button.Text = _speedX1Button == null
                ? (_isSpeedX2 ? "⏭ x2: 开" : "⏭ x2: 关")
                : "⏭ x2";
        }
    }

    private void SetMapTab(MapTab mapTab)
    {
        if (_worldMapView == null || _prefectureMapView == null || _countyTownMapView == null ||
            _worldMapButton == null || _prefectureMapButton == null || _countyTownMapButton == null)
        {
            return;
        }

        _worldMapView.Visible = mapTab == MapTab.World;
        _prefectureMapView.Visible = mapTab == MapTab.Prefecture;
        _countyTownMapView.Visible = mapTab == MapTab.CountyTown;

        _worldMapButton.ButtonPressed = mapTab == MapTab.World;
        _prefectureMapButton.ButtonPressed = mapTab == MapTab.Prefecture;
        _countyTownMapButton.ButtonPressed = mapTab == MapTab.CountyTown;
    }

    private void BindJobButtons(JobType jobType, string minusButtonPath, string plusButtonPath)
    {
        GetNode<Button>(minusButtonPath).Pressed += () => _gameLoop.AdjustJob(jobType, -JobAdjustStep);
        GetNode<Button>(plusButtonPath).Pressed += () => _gameLoop.AdjustJob(jobType, JobAdjustStep);
    }

    private void BindJobPriorityButtons()
    {
        foreach (var entry in _jobPriorityButtons)
        {
            var jobType = entry.Key;
            entry.Value.Pressed += () => OnJobPriorityPressed(jobType);
        }
    }

    private void OnJobPriorityPressed(JobType jobType)
    {
        if (_priorityJobType == jobType)
        {
            _priorityJobType = null;
            AppendLog($"{GetJobPriorityDisplayName(jobType)}恢复默认顺位。");
        }
        else
        {
            _priorityJobType = jobType;
            AppendLog($"{GetJobPriorityDisplayName(jobType)}设为优先调配。");
        }

        ApplyPriorityButtonTexts();
    }

    private void ApplyPriorityButtonTexts()
    {
        if (_jobPriorityButtons.Count == 0)
        {
            return;
        }

        foreach (var entry in _jobPriorityButtons)
        {
            var isPriority = _priorityJobType == entry.Key;
            entry.Value.Text = isPriority ? "★ 优先调配" : GetDefaultPriorityText(entry.Key);
        }
    }

    private static string GetDefaultPriorityText(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "★ 优先保供",
            JobType.Scholar => "⏸ 停工",
            _ => "☆ 默认顺位"
        };
    }

    private static string GetJobPriorityDisplayName(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "农工岗",
            JobType.Worker => "匠役岗",
            JobType.Merchant => "商贾岗",
            JobType.Scholar => "学士岗",
            _ => "岗位"
        };
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
