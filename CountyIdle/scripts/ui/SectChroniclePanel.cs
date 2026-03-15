using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class SectChroniclePanel : PopupPanelBase
{
    private enum ChroniclePage
    {
        Chronicle,
        Report
    }

    private sealed class LogFilterBinding
    {
        public LogFilterBinding(SectChronicleLogCategory category, Button button)
        {
            Category = category;
            Button = button;
        }

        public SectChronicleLogCategory Category { get; }
        public Button Button { get; }
    }

    private sealed class ReportCardBinding
    {
        public ReportCardBinding(PanelContainer container, Label titleLabel, Label statusLabel, Label detailLabel)
        {
            Container = container;
            TitleLabel = titleLabel;
            StatusLabel = statusLabel;
            DetailLabel = detailLabel;
        }

        public PanelContainer Container { get; }
        public Label TitleLabel { get; }
        public Label StatusLabel { get; }
        public Label DetailLabel { get; }
    }

    private static readonly Color InkColor = new(0.176471f, 0.145098f, 0.12549f, 1.0f);
    private static readonly Color CalmColor = new(0.352941f, 0.301961f, 0.247059f, 0.96f);
    private static readonly Color GoodColor = new(0.333333f, 0.47451f, 0.321569f, 1.0f);
    private static readonly Color WarningColor = new(0.619608f, 0.164706f, 0.133333f, 1.0f);

    private const string RootPath = "Overlay/Center/Frame/RootColumn";
    private const string HeaderPath = RootPath + "/HeaderPanel/HeaderMargin/HeaderRow";
    private const string ContentStackPath = RootPath + "/BodyMargin/ContentStack";
    private const string ChronicleTabPath = ContentStackPath + "/ChronicleTab";
    private const string ReportTabPath = ContentStackPath + "/ReportTab";

    private readonly GameCalendarSystem _calendarSystem = new();
    private readonly List<string> _recentLogs = new();
    private readonly List<SectChronicleSettlementSnapshot> _settlementSnapshots = new();
    private readonly Dictionary<ChroniclePage, Button> _pageButtons = new();
    private readonly List<LogFilterBinding> _logFilters = new();

    private Label _headerTitleLabel = null!;
    private Label _headerMetaLabel = null!;
    private Button _chronicleTabButton = null!;
    private Button _reportTabButton = null!;
    private Button _closeButton = null!;
    private Label _chronicleSummaryLabel = null!;
    private Label _filterHintLabel = null!;
    private Label _primaryAlertLabel = null!;
    private Label _secondaryAlertLabel = null!;
    private RichTextLabel _logLabel = null!;
    private Label _reportSummaryLabel = null!;
    private RichTextLabel _trendLabel = null!;
    private Label _hintLabel = null!;
    private Control _chronicleTabContent = null!;
    private Control _reportTabContent = null!;
    private ReportCardBinding _populationCard = null!;
    private ReportCardBinding _storageCard = null!;
    private ReportCardBinding _expeditionCard = null!;
    private ReportCardBinding _researchCard = null!;
    private ReportCardBinding _quarterCard = null!;
    private ReportCardBinding _yearCard = null!;

    private GameState _state = new();
    private ChroniclePage _activePage = ChroniclePage.Chronicle;
    private SectChronicleLogCategory _activeLogCategory = SectChronicleLogCategory.All;

    public override void _Ready()
    {
        BindUiNodes();
        BindEvents();
        SwitchPage(ChroniclePage.Chronicle);
        InitializePopupHint(_hintLabel);
        Hide();
    }

    public override void _Process(double delta)
    {
        TickPopupStatus(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!TryHandlePopupClose(@event))
        {
            return;
        }

        GetViewport().SetInputAsHandled();
    }

    public void Open(
        GameState state,
        IReadOnlyList<string> recentLogs,
        IReadOnlyList<SectChronicleSettlementSnapshot> settlementSnapshots)
    {
        RefreshState(state, recentLogs, settlementSnapshots);
        OpenPopup();
    }

    public void ClosePanel()
    {
        ClosePopup();
    }

    public void RefreshState(
        GameState state,
        IReadOnlyList<string>? recentLogs = null,
        IReadOnlyList<SectChronicleSettlementSnapshot>? settlementSnapshots = null)
    {
        _state = state.Clone();
        SectNamingRules.EnsureDefaults(_state);

        if (recentLogs != null)
        {
            _recentLogs.Clear();
            foreach (var line in recentLogs)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _recentLogs.Add(line);
                }
            }
        }

        if (settlementSnapshots != null)
        {
            _settlementSnapshots.Clear();
            _settlementSnapshots.AddRange(settlementSnapshots);
        }

        RefreshChronicle();
        RefreshReport();
        RefreshPopupHint();
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return _activePage == ChroniclePage.Chronicle
            ? "宗门见闻页会汇总当前警讯与近时札记，供宗主快速断局。按 Esc 可收卷。"
            : "宗务报表页展示当前人口、库藏、护山与传承快照，不直接改写小时结算。按 Esc 可收卷。";
    }

    private void BindUiNodes()
    {
        _headerTitleLabel = GetNode<Label>($"{HeaderPath}/TitleColumn/TitleLabel");
        _headerMetaLabel = GetNode<Label>($"{HeaderPath}/TitleColumn/HeaderMetaLabel");
        _chronicleTabButton = GetNode<Button>($"{HeaderPath}/TabRow/ChronicleTabButton");
        _reportTabButton = GetNode<Button>($"{HeaderPath}/TabRow/ReportTabButton");
        _closeButton = GetNode<Button>($"{HeaderPath}/CloseButton");
        _chronicleTabContent = GetNode<Control>(ChronicleTabPath);
        _reportTabContent = GetNode<Control>(ReportTabPath);
        _chronicleSummaryLabel = GetNode<Label>($"{ChronicleTabPath}/ChronicleSummaryPanel/ChronicleSummaryMargin/ChronicleSummaryLabel");
        _filterHintLabel = GetNode<Label>($"{ChronicleTabPath}/FilterRow/FilterHintLabel");
        _primaryAlertLabel = GetNode<Label>($"{ChronicleTabPath}/AlertGrid/PrimaryAlertCard/PrimaryAlertMargin/PrimaryAlertLabel");
        _secondaryAlertLabel = GetNode<Label>($"{ChronicleTabPath}/AlertGrid/SecondaryAlertCard/SecondaryAlertMargin/SecondaryAlertLabel");
        _logLabel = GetNode<RichTextLabel>($"{ChronicleTabPath}/LogPanel/LogMargin/LogColumn/LogLabel");
        _reportSummaryLabel = GetNode<Label>($"{ReportTabPath}/ReportSummaryPanel/ReportSummaryMargin/ReportSummaryLabel");
        _trendLabel = GetNode<RichTextLabel>($"{ReportTabPath}/TrendPanel/TrendMargin/TrendColumn/TrendLabel");
        _hintLabel = GetNode<Label>($"{RootPath}/HintPanel/HintMargin/HintLabel");

        _populationCard = BindReportCard("PopulationCard");
        _storageCard = BindReportCard("StorageCard");
        _expeditionCard = BindReportCard("ExpeditionCard");
        _researchCard = BindReportCard("ResearchCard");
        _quarterCard = BindReportCard("QuarterCard");
        _yearCard = BindReportCard("YearCard");

        _pageButtons.Clear();
        _pageButtons[ChroniclePage.Chronicle] = _chronicleTabButton;
        _pageButtons[ChroniclePage.Report] = _reportTabButton;

        _logFilters.Clear();
        _logFilters.Add(new LogFilterBinding(SectChronicleLogCategory.All, GetNode<Button>($"{ChronicleTabPath}/FilterRow/AllFilterButton")));
        _logFilters.Add(new LogFilterBinding(SectChronicleLogCategory.Governance, GetNode<Button>($"{ChronicleTabPath}/FilterRow/GovernanceFilterButton")));
        _logFilters.Add(new LogFilterBinding(SectChronicleLogCategory.Resources, GetNode<Button>($"{ChronicleTabPath}/FilterRow/ResourcesFilterButton")));
        _logFilters.Add(new LogFilterBinding(SectChronicleLogCategory.Expedition, GetNode<Button>($"{ChronicleTabPath}/FilterRow/ExpeditionFilterButton")));
        _logFilters.Add(new LogFilterBinding(SectChronicleLogCategory.Archive, GetNode<Button>($"{ChronicleTabPath}/FilterRow/ArchiveFilterButton")));
    }

    private ReportCardBinding BindReportCard(string cardName)
    {
        var cardPath = $"{ReportTabPath}/ReportGrid/{cardName}";
        var container = GetNode<PanelContainer>(cardPath);
        var titleLabel = GetNode<Label>($"{cardPath}/CardMargin/CardColumn/TitleLabel");
        var statusLabel = GetNode<Label>($"{cardPath}/CardMargin/CardColumn/StatusLabel");
        var detailLabel = GetNode<Label>($"{cardPath}/CardMargin/CardColumn/DetailLabel");
        return new ReportCardBinding(container, titleLabel, statusLabel, detailLabel);
    }

    private void BindEvents()
    {
        _chronicleTabButton.Pressed += () => SwitchPage(ChroniclePage.Chronicle);
        _reportTabButton.Pressed += () => SwitchPage(ChroniclePage.Report);
        _closeButton.Pressed += ClosePopup;

        foreach (var filter in _logFilters)
        {
            var category = filter.Category;
            filter.Button.Pressed += () => SwitchLogCategory(category);
        }
    }

    private void SwitchPage(ChroniclePage page)
    {
        _activePage = page;
        _chronicleTabContent.Visible = page == ChroniclePage.Chronicle;
        _reportTabContent.Visible = page == ChroniclePage.Report;

        foreach (var (tabPage, button) in _pageButtons)
        {
            button.ButtonPressed = tabPage == page;
        }

        RefreshPopupHint();
    }

    private void RefreshChronicle()
    {
        var calendarInfo = _calendarSystem.Describe(_state.GameMinutes);
        var summary = SectChronicleRules.BuildSummary(_state, calendarInfo);
        var sectName = SectNamingRules.GetName(_state, SectNamingRules.SectNameKey);
        _headerTitleLabel.Text = $"{sectName} · 见闻报表卷";
        _headerMetaLabel.Text =
            $"{calendarInfo.DateText} · {calendarInfo.DetailText} · 门人 {_state.Population:0} · 危兆 {_state.Threat:0}%";
        _chronicleSummaryLabel.Text = SectChronicleRules.BuildChronicleOverviewText(_state, calendarInfo);
        _primaryAlertLabel.Text = summary.PrimaryAlertText;
        _secondaryAlertLabel.Text = summary.SecondaryAlertText;
        _primaryAlertLabel.TooltipText = summary.PrimaryAlertText;
        _secondaryAlertLabel.TooltipText = summary.SecondaryAlertText;
        _logLabel.Text = BuildLogText(out var visibleLogCount);
        _logLabel.TooltipText = "按时间倒序展示主界面近时札记，保留原有色彩标记。";
        _filterHintLabel.Text = SectChronicleRules.BuildLogOverviewText(_activeLogCategory, visibleLogCount, _recentLogs.Count);
        RefreshFilterButtons();
    }

    private void RefreshReport()
    {
        var calendarInfo = _calendarSystem.Describe(_state.GameMinutes);
        var cards = SectChronicleRules.BuildReportCards(_state, calendarInfo);
        var quarterCard = SectChronicleRules.BuildQuarterReportCard(_state, calendarInfo, _settlementSnapshots);
        var yearCard = SectChronicleRules.BuildYearReportCard(_state, calendarInfo, _settlementSnapshots);

        _reportSummaryLabel.Text = SectChronicleRules.BuildReportOverviewText(_state, calendarInfo);
        _trendLabel.Text = SectChronicleRules.BuildSettlementTrendText(_settlementSnapshots);
        _trendLabel.TooltipText = "按最近时辰结算倒序展示关键涨跌，帮助快速判断宗门走势。";

        ApplyReportCard(_populationCard, cards.Count > 0 ? cards[0] : null, "门人盘面");
        ApplyReportCard(_storageCard, cards.Count > 1 ? cards[1] : null, "库藏供养");
        ApplyReportCard(_expeditionCard, cards.Count > 2 ? cards[2] : null, "护山外务");
        ApplyReportCard(_researchCard, cards.Count > 3 ? cards[3] : null, "传承营造");
        ApplyReportCard(_quarterCard, quarterCard, "季度摘要");
        ApplyReportCard(_yearCard, yearCard, "年度摘要");
    }

    private string BuildLogText(out int visibleLogCount)
    {
        if (_recentLogs.Count == 0)
        {
            visibleLogCount = 0;
            return "[color=#5b4d42]卷中暂无札记。[/color]";
        }

        var orderedLines = new List<string>(_recentLogs.Count);
        for (var index = _recentLogs.Count - 1; index >= 0; index--)
        {
            var line = _recentLogs[index];
            if (_activeLogCategory != SectChronicleLogCategory.All &&
                SectChronicleRules.ClassifyLogEntry(line) != _activeLogCategory)
            {
                continue;
            }

            orderedLines.Add($"• {line}");
        }

        visibleLogCount = orderedLines.Count;
        if (orderedLines.Count == 0)
        {
            return "[color=#5b4d42]当前分类下暂无札记。[/color]";
        }

        return string.Join("\n", orderedLines);
    }

    private void SwitchLogCategory(SectChronicleLogCategory category)
    {
        _activeLogCategory = category;
        RefreshChronicle();
        RefreshPopupHint();
    }

    private void RefreshFilterButtons()
    {
        foreach (var filter in _logFilters)
        {
            filter.Button.ButtonPressed = filter.Category == _activeLogCategory;
        }
    }

    private void ApplyReportCard(ReportCardBinding binding, SectChronicleReportCard? card, string fallbackTitle)
    {
        binding.TitleLabel.Text = card?.TitleText ?? fallbackTitle;
        binding.StatusLabel.Text = card?.StatusText ?? "暂无状态";
        binding.DetailLabel.Text = card?.DetailText ?? "当前尚无可展示的数据快照。";

        var toneColor = ResolveToneColor(card?.Tone ?? SectChronicleCardTone.Neutral);
        binding.TitleLabel.AddThemeColorOverride("font_color", InkColor);
        binding.StatusLabel.AddThemeColorOverride("font_color", toneColor);
        binding.DetailLabel.AddThemeColorOverride("font_color", CalmColor);
        binding.Container.Modulate = Colors.White;
    }

    private static Color ResolveToneColor(SectChronicleCardTone tone)
    {
        return tone switch
        {
            SectChronicleCardTone.Good => GoodColor,
            SectChronicleCardTone.Warning => WarningColor,
            _ => CalmColor
        };
    }
}
