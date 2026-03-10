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
	private bool _uiBound;

	private GameState _state = new();
	private int _selectedDiscipleId = 1;
	private FilterMode _filterMode;
	private SortMode _sortMode;

	public override void _Ready()
	{
		BindUiNodes();
		InitializeFilterControls();
		ApplyUiStyles();
		EnsureDynamicWidgets();
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
			Color = new Color(0.08f, 0.08f, 0.08f, 0.68f),
			MouseFilter = Control.MouseFilterEnum.Stop
		};
		overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(overlay);

		var wrapper = new PanelContainer
		{
			MouseFilter = Control.MouseFilterEnum.Stop
		};
		wrapper.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		wrapper.OffsetLeft = 18;
		wrapper.OffsetTop = 18;
		wrapper.OffsetRight = -18;
		wrapper.OffsetBottom = -18;
		wrapper.AddThemeStyleboxOverride("panel", CreatePaperStyle());
		overlay.AddChild(wrapper);

		var rootColumn = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		rootColumn.AddThemeConstantOverride("separation", 0);
		wrapper.AddChild(rootColumn);

		rootColumn.AddChild(BuildHeader());

		var divider = new ColorRect
		{
			Color = new Color(InkGrayColor, 0.55f),
			CustomMinimumSize = new Vector2(0, 1)
		};
		rootColumn.AddChild(divider);

		var bodyRow = new HBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		bodyRow.AddThemeConstantOverride("separation", 0);
		rootColumn.AddChild(bodyRow);

		var leftPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(360, 0),
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		leftPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor.R, PaperBackgroundColor.G, PaperBackgroundColor.B, 0.32f),
				new Color(InkGrayColor, 0.25f)));
		bodyRow.AddChild(leftPanel);

		var leftMargin = CreateMarginContainer(16, 16, 16, 16);
		leftPanel.AddChild(leftMargin);
		leftMargin.AddChild(BuildRosterTab());

		var bodyDivider = new ColorRect
		{
			Color = new Color(InkGrayColor, 0.32f),
			CustomMinimumSize = new Vector2(1, 0),
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		bodyRow.AddChild(bodyDivider);

		var rightMargin = CreateMarginContainer(20, 16, 20, 16);
		rightMargin.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		rightMargin.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		bodyRow.AddChild(rightMargin);

		var rightColumn = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		rightColumn.AddThemeConstantOverride("separation", 14);
		rightMargin.AddChild(rightColumn);

		rightColumn.AddChild(BuildProfileTab());
		rightColumn.AddChild(BuildFoundationTab());

		var footerDivider = new ColorRect
		{
			Color = new Color(InkGrayColor, 0.35f),
			CustomMinimumSize = new Vector2(0, 1)
		};
		rootColumn.AddChild(footerDivider);

		_hintLabel = CreateInfoLabel(InkGrayColor, 11);
		_hintLabel.CustomMinimumSize = new Vector2(0, 30);
		_hintLabel.VerticalAlignment = VerticalAlignment.Center;
		rootColumn.AddChild(_hintLabel);

		_filterOption.Select(0);
		_sortOption.Select(0);
	}

	private PanelContainer BuildHeader()
	{
		var headerPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(0, 64)
		};
		headerPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor.R, PaperBackgroundColor.G, PaperBackgroundColor.B, 0.78f),
				new Color(InkGrayColor, 0.45f)));

		var headerMargin = CreateMarginContainer(24, 12, 24, 10);
		headerPanel.AddChild(headerMargin);

		var headerRow = new HBoxContainer();
		headerRow.AddThemeConstantOverride("separation", 12);
		headerMargin.AddChild(headerRow);

		var titleColumn = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		titleColumn.AddThemeConstantOverride("separation", 2);
		headerRow.AddChild(titleColumn);

		var titleLabel = new Label
		{
			Text = "浮云宗  ·  弟子谱",
			VerticalAlignment = VerticalAlignment.Center
		};
		titleLabel.AddThemeFontSizeOverride("font_size", 22);
		titleLabel.AddThemeColorOverride("font_color", InkBlackColor);
		titleColumn.AddChild(titleLabel);

		var subtitleLabel = new Label
		{
			Text = "卷中分峰录名，可按峰脉、职司和修为检索门人根基。",
			VerticalAlignment = VerticalAlignment.Center
		};
		subtitleLabel.AddThemeFontSizeOverride("font_size", 12);
		subtitleLabel.AddThemeColorOverride("font_color", InkGrayColor);
		titleColumn.AddChild(subtitleLabel);

		_closeButton = CreateCloseButton("×");
		_closeButton.Pressed += ClosePopup;
		headerRow.AddChild(_closeButton);

		return headerPanel;
	}

	private Control BuildRosterTab()
	{
		var tab = CreateTabRoot();

		var summaryPanel = new PanelContainer();
		summaryPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor, 0.58f),
				new Color(InkGrayColor, 0.35f)));
		tab.AddChild(summaryPanel);

		var summaryMargin = CreateMarginContainer(12, 10, 12, 10);
		summaryPanel.AddChild(summaryMargin);

		var summaryColumn = new VBoxContainer();
		summaryColumn.AddThemeConstantOverride("separation", 6);
		summaryMargin.AddChild(summaryColumn);

		var summaryTitle = new Label
		{
			Text = "卷首批注"
		};
		summaryTitle.AddThemeFontSizeOverride("font_size", 14);
		summaryTitle.AddThemeColorOverride("font_color", InkBlackColor);
		summaryColumn.AddChild(summaryTitle);

		_summaryLabel = CreateInfoLabel(InkBlackColor, 12);
		summaryColumn.AddChild(_summaryLabel);

		_governanceLabel = CreateInfoLabel(InkGrayColor, 11);
		summaryColumn.AddChild(_governanceLabel);

		var filterPanel = new PanelContainer();
		filterPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor, 0.46f),
				new Color(InkGrayColor, 0.32f)));
		tab.AddChild(filterPanel);

		var filterMargin = CreateMarginContainer(10, 8, 10, 8);
		filterPanel.AddChild(filterMargin);

		var filterRow = new VBoxContainer();
		filterRow.AddThemeConstantOverride("separation", 8);
		filterMargin.AddChild(filterRow);

		_filterOption = new OptionButton();
		_filterOption.AddItem("全部弟子");
		_filterOption.AddItem("真传名册");
		_filterOption.AddItem("阵材职司");
		_filterOption.AddItem("阵务职司");
		_filterOption.AddItem("外事职司");
		_filterOption.AddItem("推演职司");
		_filterOption.AddItem("待命轮值");
		_filterOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		StylePaperOptionButton(_filterOption);
		_filterOption.ItemSelected += OnFilterSelected;
		filterRow.AddChild(_filterOption);

		_sortOption = new OptionButton();
		_sortOption.AddItem("名册顺序");
		_sortOption.AddItem("修为优先");
		_sortOption.AddItem("潜力优先");
		_sortOption.AddItem("心境优先");
		_sortOption.AddItem("贡献优先");
		_sortOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		StylePaperOptionButton(_sortOption);
		_sortOption.ItemSelected += OnSortSelected;
		filterRow.AddChild(_sortOption);

		var rosterFrame = new PanelContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		rosterFrame.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor, 0.62f),
				new Color(InkGrayColor, 0.30f)));
		tab.AddChild(rosterFrame);

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

		return tab;
	}

	private static VBoxContainer CreateTabRoot()
	{
		var tab = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		tab.AddThemeConstantOverride("separation", 12);
		return tab;
	}

	private Control BuildProfileTab()
	{
		var tab = CreateTabRoot();

		var profilePanel = new PanelContainer();
		profilePanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor, 0.62f),
				new Color(InkGrayColor, 0.35f)));
		tab.AddChild(profilePanel);

		var profileMargin = CreateMarginContainer(16, 14, 16, 14);
		profilePanel.AddChild(profileMargin);

		var headerRow = new HBoxContainer();
		headerRow.AddThemeConstantOverride("separation", 14);
		profileMargin.AddChild(headerRow);

		var nameColumn = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		nameColumn.AddThemeConstantOverride("separation", 4);
		headerRow.AddChild(nameColumn);

		_profileNameLabel = new Label();
		_profileNameLabel.AddThemeFontSizeOverride("font_size", 26);
		_profileNameLabel.AddThemeColorOverride("font_color", InkBlackColor);
		nameColumn.AddChild(_profileNameLabel);

		_profileMetaLabel = CreateInfoLabel(InkGrayColor, 12);
		nameColumn.AddChild(_profileMetaLabel);

		_profileStatusLabel = CreateInfoLabel(InkBlackColor, 12);
		nameColumn.AddChild(_profileStatusLabel);

		var rootCircleFrame = new PanelContainer
		{
			CustomMinimumSize = new Vector2(96, 96)
		};
		rootCircleFrame.AddThemeStyleboxOverride(
			"panel",
			CreateCircleStyle(
				new Color(1f, 1f, 1f, 0f),
				CinnabarColor));
		headerRow.AddChild(rootCircleFrame);

		_rootCircleLabel = new Label
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		_rootCircleLabel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_rootCircleLabel.AddThemeFontSizeOverride("font_size", 12);
		_rootCircleLabel.AddThemeColorOverride("font_color", CinnabarColor);
		rootCircleFrame.AddChild(_rootCircleLabel);

		return tab;
	}

	private Control BuildFoundationTab()
	{
		var tab = CreateTabRoot();

		var dashboardRow = new HBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		dashboardRow.AddThemeConstantOverride("separation", 24);
		tab.AddChild(dashboardRow);

		var foundationPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(420, 0),
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		foundationPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0f, 0f, 0f, 0.03f),
				new Color(0.74f, 0.68f, 0.60f, 1f)));
		dashboardRow.AddChild(foundationPanel);

		var foundationMargin = CreateMarginContainer(20, 16, 20, 16);
		foundationPanel.AddChild(foundationMargin);

		var foundationColumn = new VBoxContainer();
		foundationColumn.AddThemeConstantOverride("separation", 12);
		foundationMargin.AddChild(foundationColumn);

		var foundationTitle = new Label
		{
			Text = "先天根基评估"
		};
		foundationTitle.AddThemeFontSizeOverride("font_size", 14);
		foundationTitle.AddThemeColorOverride("font_color", InkBlackColor);
		foundationColumn.AddChild(foundationTitle);

		var radarCenter = new CenterContainer
		{
			CustomMinimumSize = new Vector2(0, 220)
		};
		foundationColumn.AddChild(radarCenter);

		_radarChart = new DiscipleRadarChart();
		radarCenter.AddChild(_radarChart);

		var statsCenter = new CenterContainer();
		foundationColumn.AddChild(statsCenter);

		var metricGrid = new GridContainer
		{
			Columns = 3,
			CustomMinimumSize = new Vector2(330, 0),
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
		};
		metricGrid.AddThemeConstantOverride("h_separation", 12);
		metricGrid.AddThemeConstantOverride("v_separation", 12);
		statsCenter.AddChild(metricGrid);

		AddMetricTile(metricGrid, "悟性", "Insight");
		AddMetricTile(metricGrid, "潜力", "Potential");
		AddMetricTile(metricGrid, "根骨", "Health");
		AddMetricTile(metricGrid, "匠艺", "Craft");
		AddMetricTile(metricGrid, "神魂", "Mood");
		AddMetricTile(metricGrid, "心境", "HeartState");

		var cultivationPanel = new PanelContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		cultivationPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0f, 0f, 0f, 0.02f),
				new Color(0.78f, 0.71f, 0.61f, 1f)));
		dashboardRow.AddChild(cultivationPanel);

		var cultivationMargin = CreateMarginContainer(20, 16, 20, 16);
		cultivationPanel.AddChild(cultivationMargin);

		var cultivationColumn = new VBoxContainer();
		cultivationColumn.AddThemeConstantOverride("separation", 16);
		cultivationMargin.AddChild(cultivationColumn);

		var cultivationTitle = new Label
		{
			Text = "修为与造化"
		};
		cultivationTitle.AddThemeFontSizeOverride("font_size", 14);
		cultivationTitle.AddThemeColorOverride("font_color", InkBlackColor);
		cultivationColumn.AddChild(cultivationTitle);

		var realmBox = new VBoxContainer();
		realmBox.AddThemeConstantOverride("separation", 6);
		cultivationColumn.AddChild(realmBox);

		var realmTitle = new Label
		{
			Text = "修为境界"
		};
		realmTitle.AddThemeFontSizeOverride("font_size", 13);
		realmTitle.AddThemeColorOverride("font_color", InkBlackColor);
		realmBox.AddChild(realmTitle);

		_realmStatusLabel = new Label();
		_realmStatusLabel.AddThemeFontSizeOverride("font_size", 13);
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

		_realmProgressHintLabel = CreateInfoLabel(InkGrayColor, 11);
		_realmProgressHintLabel.HorizontalAlignment = HorizontalAlignment.Right;
		realmBox.AddChild(_realmProgressHintLabel);

		var qiSeaBox = new VBoxContainer();
		qiSeaBox.AddThemeConstantOverride("separation", 6);
		cultivationColumn.AddChild(qiSeaBox);

		var qiSeaTitle = new Label
		{
			Text = "灵力储备（气海）"
		};
		qiSeaTitle.AddThemeFontSizeOverride("font_size", 13);
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

		_qiSeaHintLabel = CreateInfoLabel(InkGrayColor, 11);
		_qiSeaHintLabel.HorizontalAlignment = HorizontalAlignment.Right;
		qiSeaBox.AddChild(_qiSeaHintLabel);

		var combatPanel = new PanelContainer();
		combatPanel.AddThemeStyleboxOverride("panel", CreateCombatTagStyle());
		cultivationColumn.AddChild(combatPanel);

		var combatMargin = CreateMarginContainer(12, 10, 12, 10);
		combatPanel.AddChild(combatMargin);

		var combatColumn = new VBoxContainer();
		combatColumn.AddThemeConstantOverride("separation", 4);
		combatMargin.AddChild(combatColumn);

		var combatTitle = new Label
		{
			Text = "综合战力评定",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		combatTitle.AddThemeFontSizeOverride("font_size", 12);
		combatTitle.AddThemeColorOverride("font_color", CinnabarColor);
		combatColumn.AddChild(combatTitle);

		_combatSealLabel = new Label
		{
			HorizontalAlignment = HorizontalAlignment.Center
		};
		_combatSealLabel.AddThemeFontSizeOverride("font_size", 22);
		_combatSealLabel.AddThemeColorOverride("font_color", CinnabarColor);
		combatColumn.AddChild(_combatSealLabel);

		_combatSealHintLabel = CreateInfoLabel(InkGrayColor, 12);
		_combatSealHintLabel.HorizontalAlignment = HorizontalAlignment.Center;
		combatColumn.AddChild(_combatSealHintLabel);

		var traitPanel = new PanelContainer();
		traitPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0f, 0f, 0f, 0.02f),
				new Color(0.78f, 0.71f, 0.61f, 1f)));
		cultivationColumn.AddChild(traitPanel);

		var traitMargin = CreateMarginContainer(14, 12, 14, 12);
		traitPanel.AddChild(traitMargin);

		var traitColumn = new VBoxContainer();
		traitColumn.AddThemeConstantOverride("separation", 8);
		traitMargin.AddChild(traitColumn);

		var traitTitle = new Label
		{
			Text = "性情印记"
		};
		traitTitle.AddThemeFontSizeOverride("font_size", 13);
		traitTitle.AddThemeColorOverride("font_color", InkBlackColor);
		traitColumn.AddChild(traitTitle);

		_traitFlow = new FlowContainer();
		_traitFlow.AddThemeConstantOverride("h_separation", 8);
		_traitFlow.AddThemeConstantOverride("v_separation", 8);
		traitColumn.AddChild(_traitFlow);

		var annotationPanel = new PanelContainer();
		annotationPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0.97f, 0.96f, 0.94f, 1f),
				new Color(0.56f, 0.48f, 0.37f, 0.95f),
				2));
		cultivationColumn.AddChild(annotationPanel);

		var annotationMargin = CreateMarginContainer(18, 14, 18, 14);
		annotationPanel.AddChild(annotationMargin);

		var annotationColumn = new VBoxContainer();
		annotationColumn.AddThemeConstantOverride("separation", 8);
		annotationMargin.AddChild(annotationColumn);

		var annotationHeader = new Label
		{
			Text = "【衍天批注】"
		};
		annotationHeader.AddThemeFontSizeOverride("font_size", 13);
		annotationHeader.AddThemeColorOverride("font_color", InkBlackColor);
		annotationColumn.AddChild(annotationHeader);

		_annotationLabel = CreateInfoLabel(new Color(0.25f, 0.25f, 0.25f, 1f), 13);
		annotationColumn.AddChild(_annotationLabel);

		return tab;
	}

	
	private void BindUiNodes()
	{
		if (_uiBound)
		{
			return;
		}

		_summaryLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/SummaryPanel/SummaryMargin/SummaryColumn/SummaryLabel");
		_governanceLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/SummaryPanel/SummaryMargin/SummaryColumn/GovernanceLabel");
		_filterOption = GetNode<OptionButton>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/FilterPanel/FilterMargin/FilterColumn/FilterOption");
		_sortOption = GetNode<OptionButton>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/FilterPanel/FilterMargin/FilterColumn/SortOption");
		_rosterTree = GetNode<Tree>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/RosterFrame/RosterMargin/RosterTree");
		_profileNameLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/ProfileName");
		_profileMetaLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/ProfileMeta");
		_profileStatusLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/ProfileStatus");
		_rootCircleLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/RootCircle/RootCircleLabel");
		_radarChart = GetNodeOrNull<DiscipleRadarChart>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel/FoundationMargin/FoundationColumn/RadarCenter/RadarChart") ?? new DiscipleRadarChart();
		_realmStatusLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmStatus");
		_realmProgressBar = GetNode<ProgressBar>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmProgress");
		_realmProgressHintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmHint");
		_qiSeaProgressBar = GetNode<ProgressBar>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/QiSeaBox/QiSeaProgress");
		_qiSeaHintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/QiSeaBox/QiSeaHint");
		_combatSealLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag/CombatMargin/CombatColumn/CombatMain");
		_combatSealHintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag/CombatMargin/CombatColumn/CombatHint");
		_traitFlow = GetNode<FlowContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/TraitPanel/TraitMargin/TraitColumn/TraitFlow");
		_annotationLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/AnnotationPanel/AnnotationMargin/AnnotationColumn/AnnotationText");
		_hintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/HintLabel");
		_closeButton = GetNode<Button>("Overlay/Wrapper/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/CloseButton");

		_filterOption.ItemSelected += OnFilterSelected;
		_sortOption.ItemSelected += OnSortSelected;
		_rosterTree.ItemSelected += OnRosterTreeItemSelected;
		_closeButton.Pressed += ClosePopup;

		_uiBound = true;
	}

	private void InitializeFilterControls()
	{
		if (_filterOption.ItemCount == 0)
		{
			_filterOption.AddItem("全部弟子");
			_filterOption.AddItem("真传名册");
			_filterOption.AddItem("阵材职司");
			_filterOption.AddItem("阵务职司");
			_filterOption.AddItem("外事职司");
			_filterOption.AddItem("推演职司");
			_filterOption.AddItem("待命轮值");
		}

		if (_sortOption.ItemCount == 0)
		{
			_sortOption.AddItem("名册顺序");
			_sortOption.AddItem("修为优先");
			_sortOption.AddItem("潜力优先");
			_sortOption.AddItem("心境优先");
			_sortOption.AddItem("贡献优先");
		}

		_filterOption.Select((int)_filterMode);
		_sortOption.Select((int)_sortMode);
	}

	private void EnsureDynamicWidgets()
	{
		var radarParent = GetNode<Control>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel/FoundationMargin/FoundationColumn/RadarCenter");
		if (_radarChart.GetParent() == null)
		{
			radarParent.AddChild(_radarChart);
		}

		var metricGrid = GetNode<GridContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel/FoundationMargin/FoundationColumn/StatsCenter/MetricGrid");
		_metrics.Clear();
		AddMetricTile(metricGrid, "悟性", "Insight");
		AddMetricTile(metricGrid, "潜力", "Potential");
		AddMetricTile(metricGrid, "根骨", "Health");
		AddMetricTile(metricGrid, "匠艺", "Craft");
		AddMetricTile(metricGrid, "神魂", "Mood");
		AddMetricTile(metricGrid, "心境", "HeartState");
	}

	private void ApplyUiStyles()
	{
		var wrapper = GetNode<PanelContainer>("Overlay/Wrapper");
		wrapper.AddThemeStyleboxOverride("panel", CreatePaperStyle());

		var headerPanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/HeaderPanel");
		headerPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor.R, PaperBackgroundColor.G, PaperBackgroundColor.B, 0.78f),
				new Color(InkGrayColor, 0.45f)));

		var leftPanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel");
		leftPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor.R, PaperBackgroundColor.G, PaperBackgroundColor.B, 0.32f),
				new Color(InkGrayColor, 0.25f)));

		var summaryPanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/SummaryPanel");
		summaryPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor, 0.58f),
				new Color(InkGrayColor, 0.35f)));

		var filterPanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/FilterPanel");
		filterPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor, 0.46f),
				new Color(InkGrayColor, 0.32f)));

		var rosterFrame = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/RosterFrame");
		rosterFrame.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor, 0.62f),
				new Color(InkGrayColor, 0.30f)));

		var profilePanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel");
		profilePanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(PaperBackgroundColor, 0.62f),
				new Color(InkGrayColor, 0.35f)));

		var rootCircle = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/RootCircle");
		rootCircle.AddThemeStyleboxOverride(
			"panel",
			CreateCircleStyle(
				new Color(1f, 1f, 1f, 0f),
				CinnabarColor));

		_profileNameLabel.AddThemeFontSizeOverride("font_size", 26);
		_profileNameLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_profileMetaLabel.AddThemeFontSizeOverride("font_size", 12);
		_profileMetaLabel.AddThemeColorOverride("font_color", InkGrayColor);
		_profileStatusLabel.AddThemeFontSizeOverride("font_size", 12);
		_profileStatusLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_rootCircleLabel.AddThemeFontSizeOverride("font_size", 12);
		_rootCircleLabel.AddThemeColorOverride("font_color", CinnabarColor);

		var foundationPanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel");
		foundationPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0f, 0f, 0f, 0.03f),
				new Color(0.74f, 0.68f, 0.60f, 1f)));

		var cultivationPanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel");
		cultivationPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0f, 0f, 0f, 0.02f),
				new Color(0.78f, 0.71f, 0.61f, 1f)));

		var traitPanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/TraitPanel");
		traitPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0f, 0f, 0f, 0.02f),
				new Color(0.78f, 0.71f, 0.61f, 1f)));

		var annotationPanel = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/AnnotationPanel");
		annotationPanel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0.97f, 0.96f, 0.94f, 1f),
				new Color(0.56f, 0.48f, 0.37f, 0.95f),
				2));

		var combatTag = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag");
		combatTag.AddThemeStyleboxOverride("panel", CreateCombatTagStyle());

		_summaryLabel.AddThemeFontSizeOverride("font_size", 12);
		_summaryLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_governanceLabel.AddThemeFontSizeOverride("font_size", 11);
		_governanceLabel.AddThemeColorOverride("font_color", InkGrayColor);

		_realmStatusLabel.AddThemeFontSizeOverride("font_size", 13);
		_realmStatusLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_realmProgressHintLabel.AddThemeFontSizeOverride("font_size", 11);
		_realmProgressHintLabel.AddThemeColorOverride("font_color", InkGrayColor);
		_realmProgressHintLabel.HorizontalAlignment = HorizontalAlignment.Right;

		_qiSeaHintLabel.AddThemeFontSizeOverride("font_size", 11);
		_qiSeaHintLabel.AddThemeColorOverride("font_color", InkGrayColor);
		_qiSeaHintLabel.HorizontalAlignment = HorizontalAlignment.Right;

		_combatSealLabel.AddThemeFontSizeOverride("font_size", 22);
		_combatSealLabel.AddThemeColorOverride("font_color", CinnabarColor);
		_combatSealLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_combatSealHintLabel.AddThemeFontSizeOverride("font_size", 12);
		_combatSealHintLabel.AddThemeColorOverride("font_color", InkGrayColor);
		_combatSealHintLabel.HorizontalAlignment = HorizontalAlignment.Center;

		_annotationLabel.AddThemeFontSizeOverride("font_size", 13);
		_annotationLabel.AddThemeColorOverride("font_color", new Color(0.25f, 0.25f, 0.25f, 1f));

		_hintLabel.AddThemeFontSizeOverride("font_size", 11);
		_hintLabel.AddThemeColorOverride("font_color", InkGrayColor);

		StylePaperOptionButton(_filterOption);
		StylePaperOptionButton(_sortOption);
		StyleRosterTree(_rosterTree);
		StyleInkProgressBar(_realmProgressBar, InkBlackColor, new Color(0.91f, 0.89f, 0.84f, 1f));
		StyleInkProgressBar(_qiSeaProgressBar, CeladonColor, new Color(0.91f, 0.89f, 0.84f, 1f));
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

			foreach (var profile in peakGroup
						 .OrderBy(profile => ResolveRosterRankOrder(profile.RankName))
						 .ThenBy(profile => profile.Name))
			{
				var discipleItem = _rosterTree.CreateItem(peakItem);
				discipleItem.SetText(0, BuildListText(profile));
				discipleItem.SetMetadata(0, profile.Id);
				discipleItem.SetTooltipText(0, $"{profile.DutyDisplayName} · {profile.RealmName} · {profile.LinkedPeakSummary}");
				_rosterItems[profile.Id] = discipleItem;
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
			CustomMinimumSize = new Vector2(96, 78),
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
		};
		tile.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0f, 0f, 0f, 0.03f),
				new Color(InkGrayColor, 0.26f)));
		parent.AddChild(tile);

		var margin = CreateMarginContainer(8, 8, 8, 8);
		tile.AddChild(margin);

		var column = new VBoxContainer();
		column.AddThemeConstantOverride("separation", 6);
		margin.AddChild(column);

		var titleLabel = new Label
		{
			Text = title
		};
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 12);
		titleLabel.AddThemeColorOverride("font_color", InkGrayColor);
		column.AddChild(titleLabel);

		var valueLabel = new Label
		{
			Text = "0"
		};
		valueLabel.HorizontalAlignment = HorizontalAlignment.Center;
		valueLabel.AddThemeFontSizeOverride("font_size", 20);
		valueLabel.AddThemeColorOverride("font_color", InkBlackColor);
		column.AddChild(valueLabel);

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

	private static StyleBoxFlat CreateCombatTagStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(CinnabarColor, 0.03f),
			BorderWidthLeft = 2,
			BorderWidthTop = 2,
			BorderWidthRight = 2,
			BorderWidthBottom = 2,
			BorderColor = CinnabarColor
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
			CustomMinimumSize = new Vector2(320, 320);
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
