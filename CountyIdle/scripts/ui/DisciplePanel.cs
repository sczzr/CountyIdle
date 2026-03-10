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
	private const string MetricGridPath =
		"Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel/FoundationMargin/FoundationColumn/StatsCenter/MetricGrid";

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
	private PanelContainer _traitTagTemplate = null!;
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
		_traitTagTemplate = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/TraitPanel/TraitTagTemplate");
		_annotationLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/AnnotationPanel/AnnotationMargin/AnnotationColumn/AnnotationText");
		_hintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/HintLabel");
		_closeButton = GetNode<Button>("Overlay/Wrapper/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/CloseButton");

		_metrics.Clear();
		BindMetric("Insight");
		BindMetric("Potential");
		BindMetric("Health");
		BindMetric("Craft");
		BindMetric("Mood");
		BindMetric("HeartState");

		_filterOption.ItemSelected += OnFilterSelected;
		_sortOption.ItemSelected += OnSortSelected;
		_rosterTree.ItemSelected += OnRosterTreeItemSelected;
		_closeButton.Pressed += ClosePopup;

		_traitTagTemplate.Visible = false;

		_uiBound = true;
	}

	private void BindMetric(string key)
	{
		var valueLabel = GetNode<Label>($"{MetricGridPath}/{key}Tile/{key}Margin/{key}Column/{key}Value");
		_metrics[key] = new MetricBinding(valueLabel);
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

		ApplyMetricTileStyle("Insight");
		ApplyMetricTileStyle("Potential");
		ApplyMetricTileStyle("Health");
		ApplyMetricTileStyle("Craft");
		ApplyMetricTileStyle("Mood");
		ApplyMetricTileStyle("HeartState");

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

		_closeButton.AddThemeFontSizeOverride("font_size", 22);
		_closeButton.AddThemeStyleboxOverride("normal", CreateTransparentStyle());
		_closeButton.AddThemeStyleboxOverride("hover", CreateTransparentStyle());
		_closeButton.AddThemeStyleboxOverride("pressed", CreateTransparentStyle());
		_closeButton.AddThemeStyleboxOverride("focus", CreateTransparentStyle());
		_closeButton.AddThemeColorOverride("font_color", InkBlackColor);
		_closeButton.AddThemeColorOverride("font_hover_color", CinnabarColor);
		_closeButton.AddThemeColorOverride("font_pressed_color", CinnabarColor);

		StylePaperOptionButton(_filterOption);
		StylePaperOptionButton(_sortOption);
		StyleRosterTree(_rosterTree);
		StyleInkProgressBar(_realmProgressBar, InkBlackColor, new Color(0.91f, 0.89f, 0.84f, 1f));
		StyleInkProgressBar(_qiSeaProgressBar, CeladonColor, new Color(0.91f, 0.89f, 0.84f, 1f));
	}

	private void ApplyMetricTileStyle(string key)
	{
		var tilePath = $"{MetricGridPath}/{key}Tile";
		var tile = GetNode<PanelContainer>(tilePath);
		tile.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(0f, 0f, 0f, 0.03f),
				new Color(InkGrayColor, 0.26f)));

		var titleLabel = GetNode<Label>($"{tilePath}/{key}Margin/{key}Column/{key}Title");
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 12);
		titleLabel.AddThemeColorOverride("font_color", InkGrayColor);

		var valueLabel = GetNode<Label>($"{tilePath}/{key}Margin/{key}Column/{key}Value");
		valueLabel.HorizontalAlignment = HorizontalAlignment.Center;
		valueLabel.AddThemeFontSizeOverride("font_size", 20);
		valueLabel.AddThemeColorOverride("font_color", InkBlackColor);
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

	private PanelContainer CreateTraitTag(string text)
	{
		var panel = (PanelContainer)_traitTagTemplate.Duplicate();
		panel.Visible = true;
		panel.AddThemeStyleboxOverride(
			"panel",
			CreateInsetPaperStyle(
				new Color(CinnabarColor, 0.03f),
				new Color(CinnabarColor, 0.75f)));

		var label = panel.GetNode<Label>("TagMargin/TagLabel");
		label.Text = text;
		label.AddThemeFontSizeOverride("font_size", 12);
		label.AddThemeColorOverride("font_color", CinnabarColor);

		return panel;
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
