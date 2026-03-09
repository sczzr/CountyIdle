using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class DisciplePanel : PopupPanelBase
{
    private static readonly Color PaperBackgroundColor = new(0.956f, 0.945f, 0.918f, 1f);
    private static readonly Color InkBlackColor = new(0.173f, 0.173f, 0.173f, 1f);
    private static readonly Color InkGrayColor = new(0.478f, 0.478f, 0.478f, 1f);
    private static readonly Color CinnabarColor = new(0.651f, 0.192f, 0.165f, 1f);
    private static readonly Color CeladonColor = new(0.439f, 0.553f, 0.506f, 1f);
    private static readonly Color BorderGoldColor = new(0.773f, 0.627f, 0.349f, 1f);

    private enum FilterMode
    {
        All,
        Elite,
        Farmer,
        Worker,
        Merchant,
        Scholar,
        Reserve
    }

    private enum SortMode
    {
        Roster,
        Realm,
        Potential,
        Mood,
        Contribution
    }

    private sealed class MetricBinding
    {
        public MetricBinding(Label valueLabel)
        {
            ValueLabel = valueLabel;
        }

        public Label ValueLabel { get; }
    }

    private Label _summaryLabel = null!;
    private Label _governanceLabel = null!;
    private OptionButton _filterOption = null!;
    private OptionButton _sortOption = null!;
    private Tree _rosterTree = null!;
    private Label _profileNameLabel = null!;
    private Label _profileMetaLabel = null!;
    private Label _profileStatusLabel = null!;
    private Label _annotationLabel = null!;
    private Button _closeButton = null!;
    private Label _hintLabel = null!;
    private FlowContainer _traitFlow = null!;
    private Label _rootCircleLabel = null!;
    private Label _realmStatusLabel = null!;
    private ProgressBar _realmProgressBar = null!;
    private Label _realmProgressHintLabel = null!;
    private Label _combatSealLabel = null!;
    private Label _combatSealHintLabel = null!;
    private ProgressBar _qiSeaProgressBar = null!;
    private Label _qiSeaHintLabel = null!;
    private DiscipleRadarChart _radarChart = null!;
    private readonly Dictionary<string, MetricBinding> _metrics = new();
    private readonly List<DiscipleProfile> _allProfiles = new();
    private readonly List<DiscipleProfile> _visibleProfiles = new();
    private readonly Dictionary<int, TreeItem> _rosterItems = new();

    private GameState _state = new();
    private int _selectedDiscipleId = 1;
    private FilterMode _filterMode;
    private SortMode _sortMode;

    public override void _Ready()
    {
        BuildUi();
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

    public void Open(GameState state, int? preferredDiscipleId = null, JobType? preferredJobType = null)
    {
        RefreshState(state, preferredDiscipleId, preferredJobType);
        OpenPopup();
    }

    public void RefreshState(GameState state, int? preferredDiscipleId = null, JobType? preferredJobType = null)
    {
        _state = state.Clone();
        PopulationRules.EnsureDefaults(_state);
        SectGovernanceRules.EnsureDefaults(_state);

        _allProfiles.Clear();
        _allProfiles.AddRange(DiscipleRosterSystem.BuildRoster(_state));
        if (_allProfiles.Count > 0 && _allProfiles.All(profile => profile.Id != _selectedDiscipleId))
        {
            _selectedDiscipleId = _allProfiles[0].Id;
        }

        if (preferredDiscipleId.HasValue)
        {
            _selectedDiscipleId = preferredDiscipleId.Value;
        }

        if (preferredJobType.HasValue)
        {
            SetFilterMode(ResolveFilterMode(preferredJobType));
        }
        else if (preferredDiscipleId.HasValue)
        {
            var preferredProfile = _allProfiles.FirstOrDefault(profile => profile.Id == preferredDiscipleId.Value);
            if (preferredProfile != null && !MatchesFilter(preferredProfile))
            {
                SetFilterMode(FilterMode.All);
            }
        }

        RefreshSummary();
        RebuildDiscipleList();
        RefreshPopupHint();
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return "弟子谱会按当前经营态势派生生成名册，用于查看门人属性、培养方向与当前差事；不直接改写小时结算。按 Esc 可收卷。";
    }

    private void BuildUi()
    {
        MouseFilter = Control.MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var overlay = new ColorRect
        {
            Color = new Color(0.10f, 0.09f, 0.08f, 0.92f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(overlay);

        var center = new CenterContainer
        {
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        overlay.AddChild(center);

        var wrapper = new Control
        {
            CustomMinimumSize = new Vector2(1180, 720)
        };
        center.AddChild(wrapper);

        var topTrim = new ColorRect
        {
            Color = BorderGoldColor
        };
        topTrim.SetAnchorsPreset(LayoutPreset.TopWide);
        topTrim.OffsetLeft = 12;
        topTrim.OffsetTop = 12;
        topTrim.OffsetRight = -12;
        topTrim.OffsetBottom = 22;
        wrapper.AddChild(topTrim);

        var bottomTrim = new ColorRect
        {
            Color = BorderGoldColor
        };
        bottomTrim.SetAnchorsPreset(LayoutPreset.BottomWide);
        bottomTrim.OffsetLeft = 12;
        bottomTrim.OffsetTop = -22;
        bottomTrim.OffsetRight = -12;
        bottomTrim.OffsetBottom = -12;
        wrapper.AddChild(bottomTrim);

        var frameRow = new HBoxContainer();
        frameRow.SetAnchorsPreset(LayoutPreset.FullRect);
        frameRow.OffsetLeft = 12;
        frameRow.OffsetTop = 24;
        frameRow.OffsetRight = -12;
        frameRow.OffsetBottom = -24;
        frameRow.AddThemeConstantOverride("separation", 0);
        wrapper.AddChild(frameRow);

        var leftRoller = new PanelContainer
        {
            CustomMinimumSize = new Vector2(24, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        leftRoller.AddThemeStyleboxOverride("panel", CreateRollerStyle());
        frameRow.AddChild(leftRoller);

        var paperFrame = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        paperFrame.AddThemeStyleboxOverride("panel", CreatePaperStyle());
        frameRow.AddChild(paperFrame);

        var rightRoller = new PanelContainer
        {
            CustomMinimumSize = new Vector2(24, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        rightRoller.AddThemeStyleboxOverride("panel", CreateRollerStyle());
        frameRow.AddChild(rightRoller);

        var paperMargin = CreateMarginContainer(24, 24, 24, 22);
        paperFrame.AddChild(paperMargin);

        var rootColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        rootColumn.AddThemeConstantOverride("separation", 14);
        paperMargin.AddChild(rootColumn);

        var titleRow = new HBoxContainer();
        titleRow.AddThemeConstantOverride("separation", 12);
        rootColumn.AddChild(titleRow);

        var titleColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        titleColumn.AddThemeConstantOverride("separation", 4);
        titleRow.AddChild(titleColumn);

        var titleLabel = new Label
        {
            Text = "浮云宗·弟子谱"
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 26);
        titleLabel.AddThemeColorOverride("font_color", InkBlackColor);
        titleColumn.AddChild(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "卷中分峰录名，可按峰脉、职司与修为检索门人根基。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        subtitleLabel.AddThemeFontSizeOverride("font_size", 13);
        subtitleLabel.AddThemeColorOverride("font_color", InkGrayColor);
        titleColumn.AddChild(subtitleLabel);

        _closeButton = CreateCloseButton("✖");
        _closeButton.Pressed += ClosePopup;
        titleRow.AddChild(_closeButton);

        var divider = new ColorRect
        {
            Color = InkBlackColor,
            CustomMinimumSize = new Vector2(0, 1)
        };
        rootColumn.AddChild(divider);

        var contentRow = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        contentRow.AddThemeConstantOverride("separation", 18);
        rootColumn.AddChild(contentRow);

        var sidebar = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(300, 0)
        };
        sidebar.AddThemeConstantOverride("separation", 16);
        contentRow.AddChild(sidebar);

        var separator = new ColorRect
        {
            Color = new Color(0.83f, 0.81f, 0.76f, 1f),
            CustomMinimumSize = new Vector2(2, 0)
        };
        contentRow.AddChild(separator);

        sidebar.AddChild(CreateSidebarSectionTitle("峰内名录"));

        var controlsColumn = new VBoxContainer();
        controlsColumn.AddThemeConstantOverride("separation", 8);
        sidebar.AddChild(controlsColumn);

        controlsColumn.AddChild(CreateSidebarControlTitle("筛选目录"));
        _filterOption = new OptionButton();
        _filterOption.AddItem("全部弟子");
        _filterOption.AddItem("真传名册");
        _filterOption.AddItem("阵材职司");
        _filterOption.AddItem("阵务职司");
        _filterOption.AddItem("外事职司");
        _filterOption.AddItem("推演职司");
        _filterOption.AddItem("待命轮值");
        StylePaperOptionButton(_filterOption);
        _filterOption.ItemSelected += OnFilterSelected;
        controlsColumn.AddChild(_filterOption);

        controlsColumn.AddChild(CreateSidebarControlTitle("排序卷序"));
        _sortOption = new OptionButton();
        _sortOption.AddItem("名册顺序");
        _sortOption.AddItem("修为优先");
        _sortOption.AddItem("潜力优先");
        _sortOption.AddItem("心境优先");
        _sortOption.AddItem("贡献优先");
        StylePaperOptionButton(_sortOption);
        _sortOption.ItemSelected += OnSortSelected;
        controlsColumn.AddChild(_sortOption);

        var rosterFrame = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        rosterFrame.AddThemeStyleboxOverride(
            "panel",
            CreateInsetPaperStyle(
                new Color(PaperBackgroundColor, 0.45f),
                new Color(InkGrayColor, 0.25f)));
        sidebar.AddChild(rosterFrame);

        var rosterMargin = CreateMarginContainer(6, 6, 6, 6);
        rosterFrame.AddChild(rosterMargin);

        _rosterTree = new Tree
        {
            Columns = 1,
            HideRoot = true,
            CustomMinimumSize = new Vector2(0, 300),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        StyleRosterTree(_rosterTree);
        _rosterTree.ItemSelected += OnRosterTreeItemSelected;
        rosterMargin.AddChild(_rosterTree);

        var sidebarFooter = new VBoxContainer();
        sidebarFooter.AddThemeConstantOverride("separation", 8);
        sidebar.AddChild(sidebarFooter);

        _summaryLabel = CreateInfoLabel(InkGrayColor, 12);
        _governanceLabel = CreateInfoLabel(InkGrayColor, 11);
        _hintLabel = CreateInfoLabel(new Color(CeladonColor, 0.95f), 11);
        sidebarFooter.AddChild(_summaryLabel);
        sidebarFooter.AddChild(_governanceLabel);
        sidebarFooter.AddChild(_hintLabel);

        var mainPanel = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        mainPanel.AddThemeConstantOverride("separation", 26);
        contentRow.AddChild(mainPanel);

        var headerColumn = new VBoxContainer();
        headerColumn.AddThemeConstantOverride("separation", 10);
        mainPanel.AddChild(headerColumn);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", 16);
        headerColumn.AddChild(headerRow);

        var nameSection = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        nameSection.AddThemeConstantOverride("separation", 4);
        headerRow.AddChild(nameSection);

        _profileNameLabel = new Label();
        _profileNameLabel.AddThemeFontSizeOverride("font_size", 40);
        _profileNameLabel.AddThemeColorOverride("font_color", InkBlackColor);
        nameSection.AddChild(_profileNameLabel);

        _profileMetaLabel = CreateInfoLabel(InkGrayColor, 14);
        nameSection.AddChild(_profileMetaLabel);

        var headerRight = new VBoxContainer();
        headerRight.AddThemeConstantOverride("separation", 10);
        headerRow.AddChild(headerRight);

        var rootCircleFrame = new PanelContainer
        {
            CustomMinimumSize = new Vector2(110, 110)
        };
        rootCircleFrame.AddThemeStyleboxOverride(
            "panel",
            CreateCircleStyle(
                new Color(1f, 1f, 1f, 0f),
                new Color(InkGrayColor, 0.92f)));
        headerRight.AddChild(rootCircleFrame);

        _rootCircleLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _rootCircleLabel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _rootCircleLabel.AddThemeFontSizeOverride("font_size", 12);
        _rootCircleLabel.AddThemeColorOverride("font_color", InkBlackColor);
        rootCircleFrame.AddChild(_rootCircleLabel);

        var headerLine = new ColorRect
        {
            Color = InkBlackColor,
            CustomMinimumSize = new Vector2(0, 2)
        };
        headerColumn.AddChild(headerLine);

        var bodyRow = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        bodyRow.AddThemeConstantOverride("separation", 28);
        mainPanel.AddChild(bodyRow);

        var talentPanel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(410, 0)
        };
        talentPanel.AddThemeStyleboxOverride(
            "panel",
            CreateInsetPaperStyle(
                new Color(0f, 0f, 0f, 0.02f),
                new Color(0.82f, 0.80f, 0.76f, 1f)));
        bodyRow.AddChild(talentPanel);

        var talentMargin = CreateMarginContainer(20, 18, 20, 18);
        talentPanel.AddChild(talentMargin);

        var talentColumn = new VBoxContainer();
        talentColumn.AddThemeConstantOverride("separation", 10);
        talentMargin.AddChild(talentColumn);

        var talentTitle = new Label
        {
            Text = "先天根基评估",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        talentTitle.AddThemeFontSizeOverride("font_size", 16);
        talentTitle.AddThemeColorOverride("font_color", InkBlackColor);
        talentColumn.AddChild(talentTitle);

        _radarChart = new DiscipleRadarChart();
        talentColumn.AddChild(_radarChart);

        var metricGrid = new GridContainer
        {
            Columns = 2
        };
        metricGrid.AddThemeConstantOverride("h_separation", 10);
        metricGrid.AddThemeConstantOverride("v_separation", 10);
        talentColumn.AddChild(metricGrid);

        AddMetricTile(metricGrid, "悟性", "Insight");
        AddMetricTile(metricGrid, "潜力", "Potential");
        AddMetricTile(metricGrid, "根骨", "Health");
        AddMetricTile(metricGrid, "匠艺", "Craft");
        AddMetricTile(metricGrid, "神魂", "Mood");
        AddMetricTile(metricGrid, "心境", "HeartState");

        var dynamicStatus = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        dynamicStatus.AddThemeConstantOverride("separation", 24);
        bodyRow.AddChild(dynamicStatus);

        var realmBox = new VBoxContainer();
        realmBox.AddThemeConstantOverride("separation", 8);
        dynamicStatus.AddChild(realmBox);

        _realmStatusLabel = new Label();
        _realmStatusLabel.AddThemeFontSizeOverride("font_size", 16);
        _realmStatusLabel.AddThemeColorOverride("font_color", InkBlackColor);
        realmBox.AddChild(_realmStatusLabel);

        _realmProgressBar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 100,
            ShowPercentage = false
        };
        StyleInkProgressBar(_realmProgressBar, InkBlackColor, new Color(0.91f, 0.89f, 0.84f, 1f));
        realmBox.AddChild(_realmProgressBar);

        _realmProgressHintLabel = CreateInfoLabel(InkGrayColor, 12);
        _realmProgressHintLabel.HorizontalAlignment = HorizontalAlignment.Right;
        realmBox.AddChild(_realmProgressHintLabel);

        var combatBox = new VBoxContainer();
        combatBox.AddThemeConstantOverride("separation", 12);
        dynamicStatus.AddChild(combatBox);

        var combatTitle = new Label
        {
            Text = "综合战力评定"
        };
        combatTitle.AddThemeFontSizeOverride("font_size", 16);
        combatTitle.AddThemeColorOverride("font_color", InkBlackColor);
        combatBox.AddChild(combatTitle);

        var sealPanel = new PanelContainer();
        sealPanel.AddThemeStyleboxOverride(
            "panel",
            CreateInsetPaperStyle(
                new Color(CinnabarColor, 0.05f),
                CinnabarColor,
                2));
        combatBox.AddChild(sealPanel);

        var sealMargin = CreateMarginContainer(18, 14, 18, 14);
        sealPanel.AddChild(sealMargin);

        var sealRow = new HBoxContainer();
        sealRow.AddThemeConstantOverride("separation", 12);
        sealMargin.AddChild(sealRow);

        _combatSealLabel = new Label();
        _combatSealLabel.AddThemeFontSizeOverride("font_size", 36);
        _combatSealLabel.AddThemeColorOverride("font_color", CinnabarColor);
        sealRow.AddChild(_combatSealLabel);

        _combatSealHintLabel = CreateInfoLabel(InkGrayColor, 13);
        sealRow.AddChild(_combatSealHintLabel);

        var qiSeaBox = new VBoxContainer();
        qiSeaBox.AddThemeConstantOverride("separation", 8);
        dynamicStatus.AddChild(qiSeaBox);

        var qiSeaTitle = new Label
        {
            Text = "灵力储备（气海）"
        };
        qiSeaTitle.AddThemeFontSizeOverride("font_size", 16);
        qiSeaTitle.AddThemeColorOverride("font_color", InkBlackColor);
        qiSeaBox.AddChild(qiSeaTitle);

        _qiSeaProgressBar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 100,
            ShowPercentage = false
        };
        StyleInkProgressBar(_qiSeaProgressBar, CeladonColor, new Color(0.91f, 0.89f, 0.84f, 1f));
        qiSeaBox.AddChild(_qiSeaProgressBar);

        _qiSeaHintLabel = CreateInfoLabel(InkGrayColor, 12);
        _qiSeaHintLabel.HorizontalAlignment = HorizontalAlignment.Right;
        qiSeaBox.AddChild(_qiSeaHintLabel);

        var footerRow = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        footerRow.AddThemeConstantOverride("separation", 32);
        mainPanel.AddChild(footerRow);

        var traitColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        traitColumn.AddThemeConstantOverride("separation", 12);
        footerRow.AddChild(traitColumn);

        var traitTitle = new Label
        {
            Text = "性情印记："
        };
        traitTitle.AddThemeFontSizeOverride("font_size", 16);
        traitTitle.AddThemeColorOverride("font_color", InkBlackColor);
        traitColumn.AddChild(traitTitle);

        _traitFlow = new FlowContainer();
        _traitFlow.AddThemeConstantOverride("h_separation", 10);
        _traitFlow.AddThemeConstantOverride("v_separation", 10);
        traitColumn.AddChild(_traitFlow);

        var annotationPanel = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        annotationPanel.AddThemeStyleboxOverride(
            "panel",
            CreateInsetPaperStyle(
                new Color(0.97f, 0.96f, 0.94f, 1f),
                new Color(0.83f, 0.81f, 0.76f, 1f)));
        footerRow.AddChild(annotationPanel);

        var annotationMargin = CreateMarginContainer(18, 18, 18, 18);
        annotationPanel.AddChild(annotationMargin);

        var annotationColumn = new VBoxContainer();
        annotationColumn.AddThemeConstantOverride("separation", 12);
        annotationMargin.AddChild(annotationColumn);

        var annotationHeader = new Label
        {
            Text = "【衍天批注】"
        };
        annotationHeader.AddThemeFontSizeOverride("font_size", 14);
        annotationHeader.AddThemeColorOverride("font_color", BorderGoldColor);
        annotationColumn.AddChild(annotationHeader);

        _profileStatusLabel = CreateInfoLabel(InkBlackColor, 14);
        annotationColumn.AddChild(_profileStatusLabel);

        _annotationLabel = CreateInfoLabel(new Color(0.25f, 0.25f, 0.25f, 1f), 16);
        annotationColumn.AddChild(_annotationLabel);

        _filterOption.Select(0);
        _sortOption.Select(0);
    }

    private void RefreshSummary()
    {
        var talentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(_state);
        var law = SectGovernanceRules.GetActiveLawDefinition(_state);
        var direction = SectGovernanceRules.GetActiveDevelopmentDefinition(_state);
        var commuteMinutes = PopulationRules.GetCommuteMinutes(_state);

        _summaryLabel.Text =
            $"卷册总录：门人 {_state.Population} · 真传 {_state.ElitePopulation} · 现役 {_state.GetAssignedPopulation()} · 待命 {_state.GetUnassignedPopulation()} · 伤病 {_state.SickPopulation}";
        _governanceLabel.Text =
            $"当前治宗：{direction.DisplayName} / {law.DisplayName} / {talentPlan.DisplayName} · 平均通勤 {commuteMinutes:0} 分。";
    }

    private void RebuildDiscipleList()
    {
        _visibleProfiles.Clear();
        _visibleProfiles.AddRange(_allProfiles.Where(MatchesFilter));
        SortProfiles(_visibleProfiles);

        _rosterItems.Clear();
        _rosterTree.Clear();
        var root = _rosterTree.CreateItem();

        if (_visibleProfiles.Count == 0)
        {
            var emptyItem = _rosterTree.CreateItem(root);
            emptyItem.SetText(0, "当前筛选下暂无弟子收录。");
            emptyItem.SetSelectable(0, false);
            ClearDetail();
            return;
        }

        foreach (var peakGroup in BuildPeakSections(_visibleProfiles))
        {
            var peakItem = _rosterTree.CreateItem(root);
            peakItem.SetText(0, $"◈ {peakGroup.Key}");
            peakItem.SetSelectable(0, false);
            peakItem.Collapsed = false;

            foreach (var hallGroup in peakGroup
                         .GroupBy(ResolveRosterHallTitle)
                         .OrderBy(group => ResolveRosterHallOrder(group.Key))
                         .ThenBy(group => group.Key))
            {
                var hallItem = _rosterTree.CreateItem(peakItem);
                hallItem.SetText(0, hallGroup.Key);
                hallItem.SetSelectable(0, false);
                hallItem.Collapsed = false;

                foreach (var branchGroup in hallGroup
                             .GroupBy(ResolveRosterBranchTitle)
                             .OrderBy(group => ResolveRosterBranchOrder(group.Key))
                             .ThenBy(group => group.Key))
                {
                    var branchItem = _rosterTree.CreateItem(hallItem);
                    branchItem.SetText(0, branchGroup.Key);
                    branchItem.SetSelectable(0, false);
                    branchItem.Collapsed = false;

                    foreach (var rankGroup in branchGroup
                             .GroupBy(ResolveRosterRankTitle)
                             .OrderBy(group => ResolveRosterRankOrder(group.Key))
                             .ThenBy(group => group.Key))
                    {
                        var rankItem = _rosterTree.CreateItem(branchItem);
                        rankItem.SetText(0, $"{rankGroup.Key} ({rankGroup.Count()})");
                        rankItem.SetSelectable(0, false);
                        rankItem.Collapsed = false;

                        foreach (var profile in rankGroup)
                        {
                            var discipleItem = _rosterTree.CreateItem(rankItem);
                            discipleItem.SetText(0, BuildListText(profile));
                            discipleItem.SetMetadata(0, profile.Id);
                            discipleItem.SetTooltipText(0, $"{profile.DutyDisplayName} · {profile.RealmName} · {profile.LinkedPeakSummary}");
                            _rosterItems[profile.Id] = discipleItem;
                        }
                    }
                }
            }
        }

        var selectedIndex = _visibleProfiles.FindIndex(profile => profile.Id == _selectedDiscipleId);
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
            _selectedDiscipleId = _visibleProfiles[0].Id;
        }

        SelectRosterTreeItem(_selectedDiscipleId);
        RefreshDetail(_visibleProfiles[selectedIndex]);
    }

    private void RefreshDetail(DiscipleProfile profile)
    {
        _profileNameLabel.Text = profile.Name;
        _profileMetaLabel.Text =
            $"骨龄：{profile.Age}  |  籍录：{ResolveRosterPeakTitle(profile)} / {ResolveRosterHallTitle(profile)}  |  职：{profile.DutyDisplayName}  |  谱位：{profile.RankName}";
        _rootCircleLabel.Text = ResolveRootSummary(profile);
        _realmStatusLabel.Text = $"修为境界：{profile.RealmName}";
        _realmProgressBar.Value = ResolveRealmProgress(profile);
        _realmProgressHintLabel.Text = $"进度：{ResolveRealmProgressText(profile)}";
        _combatSealLabel.Text = ResolveCombatSeal(profile);
        _combatSealHintLabel.Text = $"（{ResolveCombatSealHint(profile)}）";
        _qiSeaProgressBar.Value = ResolveQiSeaProgress(profile);
        _qiSeaHintLabel.Text = $"蓄量：{ResolveQiSeaText(profile)}";
        _profileStatusLabel.Text =
            $"当前差事：{profile.CurrentAssignment}\n居所：{profile.ResidenceName}\n关联峰脉：{profile.LinkedPeakSummary}";
        _annotationLabel.Text = BuildAnnotation(profile);
        RefreshTraits(profile);

        SetMetric("Insight", profile.Insight);
        SetMetric("Potential", profile.Potential);
        SetMetric("Health", profile.Health);
        SetMetric("Craft", profile.Craft);
        SetMetric("Mood", profile.Mood);
        SetMetric("HeartState", ResolveHeartState(profile));

        _radarChart.SetStats(
            ("悟性", profile.Insight),
            ("潜力", profile.Potential),
            ("根骨", profile.Health),
            ("匠艺", profile.Craft),
            ("神魂", profile.Mood),
            ("心境", ResolveHeartState(profile)));
    }

    private void ClearDetail()
    {
        _profileNameLabel.Text = "当前无可显示弟子";
        _profileMetaLabel.Text = "请稍后再看，或切换筛选条件。";
        _rootCircleLabel.Text = "未录\n灵根";
        _realmStatusLabel.Text = "修为境界：暂无";
        _realmProgressBar.Value = 0;
        _realmProgressHintLabel.Text = "进度：未启";
        _combatSealLabel.Text = "待录";
        _combatSealHintLabel.Text = "（暂无评定）";
        _qiSeaProgressBar.Value = 0;
        _qiSeaHintLabel.Text = "蓄量：未启";
        _profileStatusLabel.Text = string.Empty;
        _annotationLabel.Text = string.Empty;
        RefreshTraits(null);

        SetMetric("Insight", 0);
        SetMetric("Potential", 0);
        SetMetric("Health", 0);
        SetMetric("Craft", 0);
        SetMetric("Mood", 0);
        SetMetric("HeartState", 0);
        _radarChart.SetStats(
            ("悟性", 0),
            ("潜力", 0),
            ("根骨", 0),
            ("匠艺", 0),
            ("神魂", 0),
            ("心境", 0));
    }

    private void RefreshTraits(DiscipleProfile? profile)
    {
        foreach (var child in _traitFlow.GetChildren())
        {
            child.QueueFree();
        }

        var traits = profile?.TraitSummary
            .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Take(4)
            .ToArray() ?? Array.Empty<string>();

        if (traits.Length == 0)
        {
            _traitFlow.AddChild(CreateTraitTag("暂无特征"));
            return;
        }

        foreach (var trait in traits)
        {
            _traitFlow.AddChild(CreateTraitTag(trait));
        }
    }

    private void OnFilterSelected(long index)
    {
        _filterMode = (FilterMode)(int)index;
        RebuildDiscipleList();
        RefreshPopupHint();
    }

    private void OnSortSelected(long index)
    {
        _sortMode = (SortMode)(int)index;
        RebuildDiscipleList();
        RefreshPopupHint();
    }

    private void OnRosterTreeItemSelected()
    {
        var selectedItem = _rosterTree.GetSelected();
        if (selectedItem == null)
        {
            return;
        }

        var metadata = selectedItem.GetMetadata(0);
        if (metadata.VariantType != Variant.Type.Int)
        {
            return;
        }

        var discipleId = metadata.AsInt32();
        var profile = _visibleProfiles.FirstOrDefault(candidate => candidate.Id == discipleId);
        if (profile == null)
        {
            return;
        }

        _selectedDiscipleId = discipleId;
        RefreshDetail(profile);
    }

    private bool MatchesFilter(DiscipleProfile profile)
    {
        return _filterMode switch
        {
            FilterMode.All => true,
            FilterMode.Elite => profile.IsElite,
            FilterMode.Farmer => profile.JobType == JobType.Farmer,
            FilterMode.Worker => profile.JobType == JobType.Worker,
            FilterMode.Merchant => profile.JobType == JobType.Merchant,
            FilterMode.Scholar => profile.JobType == JobType.Scholar,
            FilterMode.Reserve => profile.JobType is null,
            _ => true
        };
    }

    private void SetFilterMode(FilterMode filterMode)
    {
        _filterMode = filterMode;
        _filterOption?.Select((int)filterMode);
    }

    private static FilterMode ResolveFilterMode(JobType? preferredJobType)
    {
        return preferredJobType switch
        {
            JobType.Farmer => FilterMode.Farmer,
            JobType.Worker => FilterMode.Worker,
            JobType.Merchant => FilterMode.Merchant,
            JobType.Scholar => FilterMode.Scholar,
            _ => FilterMode.All
        };
    }

    private void SortProfiles(List<DiscipleProfile> profiles)
    {
        profiles.Sort((left, right) => _sortMode switch
        {
            SortMode.Realm => CompareDescending(left.RealmTier, right.RealmTier, left.Id, right.Id),
            SortMode.Potential => CompareDescending(left.Potential, right.Potential, left.Id, right.Id),
            SortMode.Mood => CompareDescending(left.Mood, right.Mood, left.Id, right.Id),
            SortMode.Contribution => CompareDescending(left.Contribution, right.Contribution, left.Id, right.Id),
            _ => left.Id.CompareTo(right.Id)
        });
    }

    private static int CompareDescending(int leftValue, int rightValue, int leftId, int rightId)
    {
        var compare = rightValue.CompareTo(leftValue);
        return compare != 0 ? compare : leftId.CompareTo(rightId);
    }

    private string BuildListText(DiscipleProfile profile)
    {
        var entrySuffix = profile.IsElite ? "真传" : profile.RankName;
        return $"{profile.Name} [{entrySuffix}]";
    }

    private void AddMetricTile(GridContainer parent, string title, string key)
    {
        var tile = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        tile.AddThemeStyleboxOverride(
            "panel",
            CreateInsetPaperStyle(
                new Color(0f, 0f, 0f, 0.03f),
                new Color(InkGrayColor, 0.26f)));
        parent.AddChild(tile);

        var margin = CreateMarginContainer(12, 10, 12, 10);
        tile.AddChild(margin);

        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        margin.AddChild(row);

        var titleLabel = new Label
        {
            Text = title,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 12);
        titleLabel.AddThemeColorOverride("font_color", InkGrayColor);
        row.AddChild(titleLabel);

        var valueLabel = new Label
        {
            Text = "0",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        valueLabel.AddThemeFontSizeOverride("font_size", 20);
        valueLabel.AddThemeColorOverride("font_color", InkBlackColor);
        row.AddChild(valueLabel);

        _metrics[key] = new MetricBinding(valueLabel);
    }

    private void SetMetric(string key, int value)
    {
        if (!_metrics.TryGetValue(key, out var binding))
        {
            return;
        }

        var clamped = Math.Clamp(value, 0, 100);
        binding.ValueLabel.Text = clamped.ToString();

        var color = clamped switch
        {
            >= 85 => CinnabarColor,
            >= 65 => InkBlackColor,
            >= 45 => CeladonColor,
            _ => InkGrayColor
        };
        binding.ValueLabel.AddThemeColorOverride("font_color", color);
    }

    private static MarginContainer CreateMarginContainer(int left, int top, int right, int bottom)
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", left);
        margin.AddThemeConstantOverride("margin_top", top);
        margin.AddThemeConstantOverride("margin_right", right);
        margin.AddThemeConstantOverride("margin_bottom", bottom);
        return margin;
    }

    private static Label CreateSectionLabel(string text)
    {
        var label = new Label
        {
            Text = text
        };
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", InkBlackColor);
        return label;
    }

    private static Label CreateInfoLabel(Color? color = null, int fontSize = 12)
    {
        var label = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color ?? InkGrayColor);
        return label;
    }

    private static Button CreateCloseButton(string text)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(40, 34),
            Alignment = HorizontalAlignment.Center
        };
        button.AddThemeFontSizeOverride("font_size", 22);
        button.AddThemeStyleboxOverride("normal", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("hover", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("pressed", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("focus", CreateTransparentStyle());
        button.AddThemeColorOverride("font_color", InkBlackColor);
        button.AddThemeColorOverride("font_hover_color", CinnabarColor);
        button.AddThemeColorOverride("font_pressed_color", CinnabarColor);
        return button;
    }

    private static void StylePaperOptionButton(OptionButton optionButton)
    {
        optionButton.CustomMinimumSize = new Vector2(180, 0);
        optionButton.AddThemeFontSizeOverride("font_size", 12);
        optionButton.AddThemeStyleboxOverride("normal", CreatePaperButtonStyle());
        optionButton.AddThemeStyleboxOverride("hover", CreatePaperButtonStyle(new Color(CinnabarColor, 0.08f)));
        optionButton.AddThemeStyleboxOverride("pressed", CreatePaperButtonStyle(new Color(CinnabarColor, 0.14f)));
        optionButton.AddThemeStyleboxOverride("focus", CreatePaperButtonStyle(new Color(CinnabarColor, 0.08f)));
        optionButton.AddThemeColorOverride("font_color", InkBlackColor);
    }

    private IEnumerable<IGrouping<string, DiscipleProfile>> BuildPeakSections(IEnumerable<DiscipleProfile> profiles)
    {
        return profiles
            .GroupBy(ResolveRosterPeakTitle)
            .OrderBy(group => ResolveRosterPeakOrder(group.Key))
            .ThenBy(group => group.Key);
    }

    private void SelectRosterTreeItem(int discipleId)
    {
        if (!_rosterItems.TryGetValue(discipleId, out var item))
        {
            return;
        }

        item.Select(0);
        _rosterTree.EnsureCursorIsVisible();
    }

    private static int ResolveRosterPeakOrder(string peakTitle)
    {
        return peakTitle switch
        {
            "天衍峰" => 0,
            "青云峰" => 1,
            "庶务殿" => 2,
            "启蒙院" => 3,
            _ => 9
        };
    }

    private static string ResolveRosterPeakTitle(DiscipleProfile profile)
    {
        if (profile.AgeBand == DiscipleAgeBand.Seedling)
        {
            return "启蒙院";
        }

        if (profile.IsElite)
        {
            return profile.JobType switch
            {
                JobType.Worker => "天衍峰",
                JobType.Merchant => "青云峰",
                JobType.Scholar => "青云峰",
                JobType.Farmer => "天元峰",
                _ => "天衍峰"
            };
        }

        return profile.JobType switch
        {
            JobType.Farmer => "天元峰",
            JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal) ? "天权峰" : "天工峰",
            JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal) ? "天枢峰" : "青云峰",
            JobType.Scholar => "天机峰",
            _ => "庶务殿"
        };
    }

    private static string ResolveRosterHallTitle(DiscipleProfile profile)
    {
        if (profile.AgeBand == DiscipleAgeBand.Seedling)
        {
            return "传功总院";
        }

        if (profile.IsElite)
        {
            return profile.JobType switch
            {
                JobType.Worker => "总枢殿",
                JobType.Merchant => "外事总殿",
                JobType.Scholar => "传功总殿",
                JobType.Farmer => "济世堂",
                _ => "总枢殿"
            };
        }

        return profile.JobType switch
        {
            JobType.Farmer => "济世堂",
            JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal) ? "承山堂" : "铸机阁",
            JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal) ? "鸿胪司" : "外事总殿",
            JobType.Scholar => profile.CurrentAssignment.Contains("讲法", StringComparison.Ordinal) ? "传功总院" : "衍法阁",
            _ => "外门轮值司"
        };
    }

    private static int ResolveRosterHallOrder(string hallTitle)
    {
        return hallTitle switch
        {
            "总枢殿" => 0,
            "外事总殿" => 1,
            "传功总殿" => 2,
            "传功总院" => 3,
            "衍法阁" => 4,
            "铸机阁" => 5,
            "承山堂" => 6,
            "鸿胪司" => 7,
            "济世堂" => 8,
            "外门轮值司" => 9,
            _ => 9
        };
    }

    private static string ResolveRosterRankTitle(DiscipleProfile profile)
    {
        return profile.RankName;
    }

    private static string ResolveRosterBranchTitle(DiscipleProfile profile)
    {
        if (profile.AgeBand == DiscipleAgeBand.Seedling)
        {
            return "启蒙课业线";
        }

        if (profile.IsElite)
        {
            return profile.JobType switch
            {
                JobType.Worker => "总枢亲传线",
                JobType.Merchant => "外务真传线",
                JobType.Scholar => "真传研修线",
                JobType.Farmer => "灵植亲传线",
                _ => "真传嫡录线"
            };
        }

        return profile.JobType switch
        {
            JobType.Farmer => profile.CurrentAssignment.Contains("巡视", StringComparison.Ordinal) ? "药圃巡看线" : "阵材轮值线",
            JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal) ? "护山检修线" : "阵枢营造线",
            JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal) ? "商路采办线" : "总坊对牌线",
            JobType.Scholar => profile.CurrentAssignment.Contains("讲法", StringComparison.Ordinal) ? "讲法校勘线" : "推演研修线",
            _ => profile.CurrentAssignment.Contains("巡舍", StringComparison.Ordinal) ? "巡舍备勤线" : "待命补位线"
        };
    }

    private static int ResolveRosterBranchOrder(string branchTitle)
    {
        return branchTitle switch
        {
            "总枢亲传线" => 0,
            "真传研修线" => 1,
            "外务真传线" => 2,
            "灵植亲传线" => 3,
            "启蒙课业线" => 4,
            "推演研修线" => 5,
            "讲法校勘线" => 6,
            "阵枢营造线" => 7,
            "护山检修线" => 8,
            "总坊对牌线" => 9,
            "商路采办线" => 10,
            "药圃巡看线" => 11,
            "阵材轮值线" => 12,
            "巡舍备勤线" => 13,
            "待命补位线" => 14,
            _ => 20
        };
    }

    private static int ResolveRosterRankOrder(string rankTitle)
    {
        return rankTitle switch
        {
            "真传" => 0,
            "守峰前辈" => 1,
            "内门" => 2,
            "外门" => 3,
            "新苗" => 4,
            "候值" => 5,
            _ => 9
        };
    }

    private static void StyleRosterTree(Tree tree)
    {
        tree.AddThemeColorOverride("font_color", InkBlackColor);
        tree.AddThemeColorOverride("font_selected_color", CinnabarColor);
        tree.AddThemeColorOverride("guide_color", new Color(InkGrayColor, 0.45f));
        tree.AddThemeColorOverride("relationship_line_color", new Color(InkGrayColor, 0.35f));
        tree.AddThemeStyleboxOverride(
            "selected",
            CreateInsetPaperStyle(
                new Color(CinnabarColor, 0.08f),
                new Color(CinnabarColor, 0.45f)));
        tree.AddThemeStyleboxOverride(
            "selected_focus",
            CreateInsetPaperStyle(
                new Color(CinnabarColor, 0.12f),
                new Color(CinnabarColor, 0.65f)));
        tree.AddThemeStyleboxOverride(
            "cursor",
            CreateInsetPaperStyle(
                new Color(CinnabarColor, 0.08f),
                new Color(CinnabarColor, 0.45f)));
        tree.AddThemeStyleboxOverride(
            "cursor_unfocused",
            CreateInsetPaperStyle(
                new Color(CinnabarColor, 0.05f),
                new Color(CinnabarColor, 0.25f)));
        tree.AddThemeStyleboxOverride(
            "panel",
            CreateInsetPaperStyle(
                new Color(PaperBackgroundColor, 0.08f),
                new Color(InkGrayColor, 0.18f)));
    }

    private static Control CreateSidebarSectionTitle(string text)
    {
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 4);

        var label = new Label
        {
            Text = $"◈ {text}"
        };
        label.AddThemeFontSizeOverride("font_size", 15);
        label.AddThemeColorOverride("font_color", CinnabarColor);
        column.AddChild(label);

        var line = new ColorRect
        {
            Color = new Color(InkGrayColor, 0.55f),
            CustomMinimumSize = new Vector2(0, 1)
        };
        column.AddChild(line);

        return column;
    }

    private static Label CreateSidebarControlTitle(string text)
    {
        var label = new Label
        {
            Text = text
        };
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", InkGrayColor);
        return label;
    }

    private static PanelContainer CreateTraitTag(string text)
    {
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride(
            "panel",
            CreateInsetPaperStyle(
                new Color(CinnabarColor, 0.03f),
                new Color(CinnabarColor, 0.75f)));

        var margin = CreateMarginContainer(8, 4, 8, 4);
        panel.AddChild(margin);

        var label = new Label
        {
            Text = text
        };
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", CinnabarColor);
        margin.AddChild(label);

        return panel;
    }

    private static StyleBoxFlat CreateSidebarButtonStyle(bool selected, bool hover)
    {
        var background = selected
            ? new Color(CinnabarColor, hover ? 0.12f : 0.08f)
            : new Color(1f, 1f, 1f, hover ? 0.10f : 0.01f);
        var border = selected
            ? new Color(CinnabarColor, 0.55f)
            : new Color(1f, 1f, 1f, 0f);

        return new StyleBoxFlat
        {
            BgColor = background,
            BorderWidthBottom = selected ? 1 : 0,
            BorderColor = border,
            ContentMarginLeft = 8,
            ContentMarginTop = 6,
            ContentMarginRight = 8,
            ContentMarginBottom = 6
        };
    }

    private static StyleBoxFlat CreateInsetPaperStyle(Color color, Color borderColor, int borderWidth = 1)
    {
        return new StyleBoxFlat
        {
            BgColor = color,
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            BorderColor = borderColor,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0
        };
    }

    private static StyleBoxFlat CreatePaperStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = PaperBackgroundColor,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.48f, 0.42f, 0.35f, 0.45f),
            ShadowColor = new Color(0f, 0f, 0f, 0.35f),
            ShadowSize = 10
        };
    }

    private static StyleBoxFlat CreateRollerStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.29f, 0.19f, 0.13f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.14f, 0.09f, 0.05f, 1f)
        };
    }

    private static StyleBoxFlat CreateCircleStyle(Color background, Color borderColor)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            BorderColor = borderColor,
            CornerRadiusTopLeft = 999,
            CornerRadiusTopRight = 999,
            CornerRadiusBottomRight = 999,
            CornerRadiusBottomLeft = 999
        };
    }

    private static StyleBoxFlat CreatePaperButtonStyle(Color? color = null)
    {
        return new StyleBoxFlat
        {
            BgColor = color ?? new Color(1f, 1f, 1f, 0.03f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(InkGrayColor, 0.45f),
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 12,
            ContentMarginTop = 6,
            ContentMarginRight = 12,
            ContentMarginBottom = 6
        };
    }

    private static StyleBoxFlat CreateTransparentStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(1f, 1f, 1f, 0f),
            BorderWidthLeft = 0,
            BorderWidthTop = 0,
            BorderWidthRight = 0,
            BorderWidthBottom = 0
        };
    }

    private static void StyleInkProgressBar(ProgressBar progressBar, Color fillColor, Color backgroundColor)
    {
        progressBar.AddThemeStyleboxOverride("fill", CreateProgressFillStyle(fillColor));
        progressBar.AddThemeStyleboxOverride("background", CreateProgressBarStyle(backgroundColor));
        progressBar.CustomMinimumSize = new Vector2(0, 14);
    }

    private static StyleBoxFlat CreateProgressBarStyle(Color backgroundColor)
    {
        return new StyleBoxFlat
        {
            BgColor = backgroundColor,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = InkGrayColor
        };
    }

    private static StyleBoxFlat CreateProgressFillStyle(Color fillColor)
    {
        return new StyleBoxFlat
        {
            BgColor = fillColor
        };
    }

    private static int ResolveHeartState(DiscipleProfile profile)
    {
        return Math.Clamp((profile.Mood + profile.Contribution + profile.Execution) / 3, 0, 100);
    }

    private static string ResolveRootSummary(DiscipleProfile profile)
    {
        var elementText = profile.JobType switch
        {
            JobType.Farmer => "木土水",
            JobType.Worker => "土金火",
            JobType.Merchant => "金水木",
            JobType.Scholar => "木水金",
            _ => "五行未明"
        };

        var rootText = profile.Potential switch
        {
            >= 86 => "双灵根",
            >= 68 => "三灵根",
            >= 52 => "四灵根",
            _ => "杂灵根"
        };

        return $"{elementText}\n{rootText}";
    }

    private static int ResolveRealmProgress(DiscipleProfile profile)
    {
        var progress = ((profile.Potential * 3) + (profile.Insight * 2) + profile.Execution + (profile.RealmTier * 14)) / 7;
        return Math.Clamp(progress, 8, 99);
    }

    private static string ResolveRealmProgressText(DiscipleProfile profile)
    {
        return ToChineseProgressText(ResolveRealmProgress(profile));
    }

    private static string ResolveCombatSeal(DiscipleProfile profile)
    {
        var score = ((profile.Combat * 4) + (profile.Execution * 2) + profile.Contribution + (profile.RealmTier * 12)) / 8;
        return score switch
        {
            >= 88 => "真传可期",
            >= 72 => "堪镇锋列",
            >= 58 => "可担护峰",
            >= 44 => "待砺其锋",
            _ => "尚需温养"
        };
    }

    private static string ResolveCombatSealHint(DiscipleProfile profile)
    {
        return profile.IsElite
            ? "符种蕴锋已显"
            : profile.Combat >= 65
                ? "可入轮值锋册"
                : "待符种蕴养中";
    }

    private static int ResolveQiSeaProgress(DiscipleProfile profile)
    {
        var progress = ((profile.Health * 2) + profile.Mood + profile.Potential + (profile.RealmTier * 10)) / 5;
        return Math.Clamp(progress, 10, 100);
    }

    private static string ResolveQiSeaText(DiscipleProfile profile)
    {
        return ToChineseProgressText(ResolveQiSeaProgress(profile));
    }

    private static string BuildAnnotation(DiscipleProfile profile)
    {
        var primaryTrait = profile.TraitSummary
            .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "气机平和";

        return $"观其气机，{primaryTrait}，骨相与心识相济。现下以“{profile.CurrentAssignment}”为主线，{profile.Note} 若能继续借 {ResolveRosterPeakTitle(profile)} {ResolveRosterHallTitle(profile)} 之务磨砺，则在 {profile.RealmName} 上尚可再进一步。";
    }

    private static string ToChineseProgressText(int percent)
    {
        var numerals = new[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        var clamped = Math.Clamp(percent, 0, 99);
        var tens = clamped / 10;
        var ones = clamped % 10;
        return ones == 0 ? $"{numerals[tens]}成" : $"{numerals[tens]}成{numerals[ones]}分";
    }

    private sealed partial class DiscipleRadarChart : Control
    {
        private readonly List<(string Label, int Value)> _stats = new();
        private readonly List<Label> _axisLabels = new();

        public DiscipleRadarChart()
        {
            CustomMinimumSize = new Vector2(300, 300);
            MouseFilter = Control.MouseFilterEnum.Ignore;
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        }

        public void SetStats(params (string Label, int Value)[] stats)
        {
            _stats.Clear();
            _stats.AddRange(stats);
            EnsureLabels();
            LayoutLabels();
            QueueRedraw();
        }

        public override void _Notification(int what)
        {
            if (what == NotificationResized)
            {
                LayoutLabels();
                QueueRedraw();
            }
        }

        public override void _Draw()
        {
            if (_stats.Count < 3)
            {
                return;
            }

            var center = Size / 2f;
            var radius = Math.Min(Size.X, Size.Y) * 0.30f;
            var directions = BuildDirections(_stats.Count);

            DrawCircle(center, radius * 1.04f, new Color(0.12f, 0.10f, 0.08f, 0.02f));

            for (var ring = 1; ring <= 5; ring++)
            {
                var ringFactor = ring / 5f;
                var ringPoints = directions
                    .Select(direction => center + (direction * radius * ringFactor))
                    .ToArray();
                DrawPolyline(ToClosedLoop(ringPoints), new Color(0.48f, 0.48f, 0.45f, 0.35f), 1.2f, true);
            }

            foreach (var direction in directions)
            {
                DrawLine(center, center + (direction * radius), new Color(0.46f, 0.45f, 0.42f, 0.42f), 1f, true);
            }

            var dataPoints = directions
                .Select((direction, index) =>
                    center + (direction * radius * (Math.Clamp(_stats[index].Value, 0, 100) / 100f)))
                .ToArray();

            DrawColoredPolygon(dataPoints, new Color(0.12f, 0.11f, 0.10f, 0.05f));
            DrawPolyline(ToClosedLoop(dataPoints), new Color(0.18f, 0.18f, 0.18f, 0.92f), 2f, true);

            foreach (var point in dataPoints)
            {
                DrawCircle(point, 3.2f, new Color(0.18f, 0.18f, 0.18f, 0.88f));
            }
        }

        private void EnsureLabels()
        {
            while (_axisLabels.Count < _stats.Count)
            {
                var label = new Label
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                label.AddThemeFontSizeOverride("font_size", 12);
                label.AddThemeColorOverride("font_color", InkGrayColor);
                AddChild(label);
                _axisLabels.Add(label);
            }

            for (var index = 0; index < _axisLabels.Count; index++)
            {
                var label = _axisLabels[index];
                if (index < _stats.Count)
                {
                    label.Text = _stats[index].Label;
                    label.Visible = true;
                }
                else
                {
                    label.Visible = false;
                }
            }
        }

        private void LayoutLabels()
        {
            if (_stats.Count == 0)
            {
                return;
            }

            var center = Size / 2f;
            var radius = Math.Min(Size.X, Size.Y) * 0.38f;
            var directions = BuildDirections(_stats.Count);

            for (var index = 0; index < _stats.Count; index++)
            {
                var label = _axisLabels[index];
                label.Size = label.GetCombinedMinimumSize();
                var position = center + (directions[index] * radius) - (label.Size / 2f);
                label.Position = position;
            }
        }

        private static Vector2[] BuildDirections(int count)
        {
            var directions = new Vector2[count];
            for (var index = 0; index < count; index++)
            {
                var angle = (-Mathf.Pi / 2f) + (Mathf.Tau * index / count);
                directions[index] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            return directions;
        }

        private static Vector2[] ToClosedLoop(IReadOnlyList<Vector2> points)
        {
            var result = new Vector2[points.Count + 1];
            for (var index = 0; index < points.Count; index++)
            {
                result[index] = points[index];
            }

            result[^1] = points[0];
            return result;
        }
    }
}
