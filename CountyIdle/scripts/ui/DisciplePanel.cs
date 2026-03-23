using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class DisciplePanel : PopupPanelBase
{
	private static readonly Color InkBlackColor = new(0.173f, 0.173f, 0.173f, 1f);
	private static readonly Color InkGrayColor = new(0.478f, 0.478f, 0.478f, 1f);
	private static readonly Color CinnabarColor = new(0.651f, 0.192f, 0.165f, 1f);
	private static readonly Color CeladonColor = new(0.439f, 0.553f, 0.506f, 1f);
	private static readonly Color PaperColor = new(0.922f, 0.906f, 0.867f, 1f);
	private static readonly Color PaperHighlightColor = new(0.992f, 0.992f, 0.988f, 1f);
	private static readonly Color JadeMistColor = new(0.831f, 0.898f, 0.851f, 1f);
	private static readonly Color DividerColor = new(0.812f, 0.792f, 0.753f, 1f);
	private const string MetricGridPath =
		"ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/MiddlePanel/MiddleMargin/MiddleColumn/MetricGrid";
	private const int RandomRosterMinCount = 12;
	private const int RandomRosterMaxCount = 48;
	private const string RandomRosterButtonIdleText = "调试：随机名册";
	private const string RandomRosterButtonActiveText = "调试：恢复宗门名册";
	private const float SidebarWidth = 308f;
	private const float TreeNodeWidth = 64f;
	private const float TreeNodeHeight = 180f;
	private const float TreeNodeGap = 140f;
	private const float TreeNodeRowGap = 238f;
	private const float TreeCanvasMinWidth = 1120f;
	private const float TreeCanvasBaseHeight = 760f;
	private const float TreeLabelWidth = 184f;

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
	private Label _treeCountLabel = null!;
	private Label _sectRootTitleLabel = null!;
	private Label _sectRootMetaLabel = null!;
	private OptionButton _filterOption = null!;
	private OptionButton _sortOption = null!;
	private Control? _debugPanel;
	private Button _randomRosterButton = null!;
	private ScrollContainer _rosterScroll = null!;
	private HBoxContainer _peakRow = null!;
	private VBoxContainer _peakColumnTemplate = null!;
	private VBoxContainer _hallGroupTemplate = null!;
	private PanelContainer _discipleCardTemplate = null!;
	private Control _treePage = null!;
	private Control _profilePage = null!;
	private Button _backButton = null!;
	private Label _profileNameLabel = null!;
	private Label _profileMetaLabel = null!;
	private Label _profileStatusLabel = null!;
	private Label _directiveStatusLabel = null!;
	private Label _directiveEffectLabel = null!;
	private Label _annotationLabel = null!;
	private RichTextLabel _fullInfoLabel = null!;
	private Button _directiveNoneButton = null!;
	private Button _directiveOuterButton = null!;
	private Button _directiveStewardButton = null!;
	private Button _cultivationJumpButton = null!;
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
	private Control _radarChart = null!;
	private readonly Dictionary<string, MetricBinding> _metrics = new();
	private readonly List<DiscipleProfile> _allProfiles = new();
	private readonly List<DiscipleProfile> _visibleProfiles = new();
	private readonly List<DiscipleProfile> _randomRosterProfiles = new();
	private readonly Dictionary<int, PanelContainer> _rosterCardButtons = new();
	private readonly Dictionary<string, Button> _branchTabButtons = new();
	private readonly Dictionary<int, Control> _lineageNodeHosts = new();
	private Node? _visualFx;
	private bool _uiBound;
	private bool _randomRosterPreviewActive;
	private bool _minimalistTreeLayoutBuilt;
	private int? _randomRosterSeed;
	private string? _selectedPeakKey;

	private GameState _state = new();
	private int _selectedDiscipleId = 1;
	private FilterMode _filterMode;
	private SortMode _sortMode;
	private Control _minimalistHost = null!;
	private VBoxContainer _sidebarBranchList = null!;
	private ScrollContainer _treeCanvasScroll = null!;
	private Control _treeCanvas = null!;
	private Label _treeAreaTitleLabel = null!;
	private Label _treeAreaSubtitleLabel = null!;
	private Label _watermarkLabel = null!;

	public event Action<int, DiscipleDirectiveType>? DirectiveRequested;
	public event Action<int>? CultivationRequested;

	public override void _Ready()
	{
		BindUiNodes();
		InitializeFilterControls();
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
		CallVisualFx("play_open");
	}

	public void ClosePanel()
	{
		ClosePopup();
	}

	public void RefreshState(GameState state, int? preferredDiscipleId = null, JobType? preferredJobType = null)
	{
		_state = state.Clone();
		PopulationRules.EnsureDefaults(_state);
		SectGovernanceRules.EnsureDefaults(_state);
		DiscipleDirectiveRules.EnsureDefaults(_state);
		DiscipleCultivationRules.EnsureDefaults(_state);

		_allProfiles.Clear();
		if (_randomRosterPreviewActive)
		{
			preferredDiscipleId = null;
			preferredJobType = null;
			EnsureRandomRosterPreview();
			_allProfiles.AddRange(_randomRosterProfiles);
		}
		else
		{
			_randomRosterProfiles.Clear();
			_randomRosterSeed = null;
			_allProfiles.AddRange(DiscipleRosterSystem.BuildRoster(_state));
		}
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

		RebuildDiscipleList();
		RefreshSummary();
		UpdateRandomRosterButton();
		RefreshPopupHint();
		// 地图点选弟子时直达命谱详情，其余情况保持宗门大谱视角。
		if (_visibleProfiles.Count == 0)
		{
			ShowTreePage();
			return;
		}
		if (preferredDiscipleId.HasValue)
		{
			ShowProfilePage();
		}
		else
		{
			ShowTreePage();
		}
	}

	protected override string GetPopupHintText()
	{
		if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
		{
			return PopupStatusMessage!;
		}

		if (_randomRosterPreviewActive)
		{
			return "当前为随机名册预览（调试），批注不会写入宗门名册。按 Esc 可收卷。";
		}

		return "弟子谱会按当前经营态势派生生成名册，用于查看门人属性、培养方向与当前差事；现可将个体纳入外务候补或执事培养，并直接反馈到历练与内务回流。按 Esc 可收卷。";
	}

	private void BindUiNodes()
	{
		if (_uiBound)
		{
			return;
		}

		_summaryLabel = GetNode<Label>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/SummaryPanel/SummaryMargin/SummaryColumn/SummaryLabel");
		_governanceLabel = GetNode<Label>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/SummaryPanel/SummaryMargin/SummaryColumn/GovernanceLabel");
		_treeCountLabel = GetNode<Label>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TreeCountBadge/TreeCountLabel");
		_sectRootTitleLabel = GetNode<Label>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/SectRootCenter/SectRootCard/SectRootMargin/SectRootColumn/SectTitleLabel");
		_sectRootMetaLabel = GetNode<Label>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/SectRootCenter/SectRootCard/SectRootMargin/SectRootColumn/SectMetaLabel");
		_filterOption = GetNode<OptionButton>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/FilterPanel/FilterMargin/FilterColumn/FilterOption");
		_sortOption = GetNode<OptionButton>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/FilterPanel/FilterMargin/FilterColumn/SortOption");
		_debugPanel = GetNodeOrNull<Control>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/DebugPanel");
		_randomRosterButton = GetNode<Button>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/DebugPanel/DebugMargin/DebugRow/RandomRosterButton");
		_rosterScroll = GetNode<ScrollContainer>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll");
		_peakRow = GetNode<HBoxContainer>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/BranchWrap/PeakRow");
		_peakColumnTemplate = GetNode<VBoxContainer>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/PeakColumnTemplate");
		_hallGroupTemplate = GetNode<VBoxContainer>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/HallGroupTemplate");
		_discipleCardTemplate = GetNode<PanelContainer>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/RosterPanel/RosterMargin/RosterScroll/ChartRoot/Templates/DiscipleCardTemplate");
		_treePage = GetNode<Control>("ScreenMargin/ScreenRoot/TreePage");
		_profilePage = GetNode<Control>("ScreenMargin/ScreenRoot/ProfilePage");
		_backButton = GetNode<Button>("TopOverlay/BackButton");
		_profileNameLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/ProfileName");
		_profileMetaLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/ProfileMeta");
		_profileStatusLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/ProfileStatus");
		_directiveStatusLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveHeader/DirectiveStatus");
		_directiveEffectLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveHeader/DirectiveEffect");
		_rootCircleLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/LeftPanel/LeftMargin/LeftColumn/RootCircleWrap/RootCircle/RootCircleLabel");
		_radarChart = GetNode<Control>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/MiddlePanel/MiddleMargin/MiddleColumn/RadarWrap/RadarChart");
		_realmStatusLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/RealmBox/RealmMargin/RealmColumn/RealmStatus");
		_realmProgressBar = GetNode<ProgressBar>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/RealmBox/RealmMargin/RealmColumn/RealmProgress");
		_realmProgressHintLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/RealmBox/RealmMargin/RealmColumn/RealmHint");
		_qiSeaProgressBar = GetNode<ProgressBar>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/QiSeaBox/QiSeaMargin/QiSeaColumn/QiSeaProgress");
		_qiSeaHintLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/QiSeaBox/QiSeaMargin/QiSeaColumn/QiSeaHint");
		_combatSealLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/CombatTag/CombatMargin/CombatColumn/CombatMain");
		_combatSealHintLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/CombatTag/CombatMargin/CombatColumn/CombatHint");
		_traitFlow = GetNode<FlowContainer>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/TraitPanel/TraitMargin/TraitColumn/TraitFlow");
		_traitTagTemplate = GetNode<PanelContainer>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/TraitPanel/TraitMargin/TraitColumn/TraitTagTemplate");
		_annotationLabel = GetNode<Label>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/LogPanel/LogMargin/LogColumn/LogSummary");
		_fullInfoLabel = GetNode<RichTextLabel>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/LogPanel/LogMargin/LogColumn/LogScroll/LogText");
		_directiveNoneButton = GetNode<Button>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionGrid/DirectiveNoneButton");
		_directiveOuterButton = GetNode<Button>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionGrid/DirectiveOuterButton");
		_directiveStewardButton = GetNode<Button>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionGrid/DirectiveStewardButton");
		_cultivationJumpButton = GetNode<Button>("ScreenMargin/ScreenRoot/ProfilePage/ProfileColumn/ProfileRow/RightPanel/RightMargin/RightColumn/DirectiveActions/ActionMargin/ActionColumn/ActionGrid/CultivationJumpButton");
		_hintLabel = GetNode<Label>("ScreenMargin/ScreenRoot/HintLabel");
		_closeButton = GetNode<Button>("TopOverlay/CloseButton");
		_visualFx = GetNodeOrNull<Node>("VisualFx");

		_metrics.Clear();
		BindMetric("Insight");
		BindMetric("Potential");
		BindMetric("Health");
		BindMetric("Craft");
		BindMetric("Mood");
		BindMetric("HeartState");
		BindMetric("Combat");
		BindMetric("Execution");
		BindMetric("Contribution");

		_filterOption.ItemSelected += OnFilterSelected;
		_sortOption.ItemSelected += OnSortSelected;
		_randomRosterButton.Pressed += ToggleRandomRosterPreview;
		_backButton.Pressed += ShowTreePage;
		_directiveNoneButton.Pressed += () => RequestDirectiveChange(DiscipleDirectiveType.None);
		_directiveOuterButton.Pressed += () => RequestDirectiveChange(DiscipleDirectiveType.OuterMissionCandidate);
		_directiveStewardButton.Pressed += () => RequestDirectiveChange(DiscipleDirectiveType.StewardCandidate);
		_cultivationJumpButton.Pressed += RequestCultivationOpen;
		_closeButton.Pressed += ClosePopup;

		_traitTagTemplate.Visible = false;
		_peakColumnTemplate.Visible = false;
		_hallGroupTemplate.Visible = false;
		_discipleCardTemplate.Visible = false;

		if (_debugPanel != null)
		{
			_debugPanel.Visible = OS.IsDebugBuild();
		}

		BuildMinimalistTreeLayout();
		UpdateRandomRosterButton();
		ShowTreePage();

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

	private void ToggleRandomRosterPreview()
	{
		if (!OS.IsDebugBuild())
		{
			return;
		}

		if (_randomRosterPreviewActive)
		{
			_randomRosterPreviewActive = false;
			_randomRosterProfiles.Clear();
			_randomRosterSeed = null;
			RefreshState(_state);
			UpdateRandomRosterButton();
			ShowPopupStatusMessage("已恢复宗门名册。");
			return;
		}

		_randomRosterPreviewActive = true;
		_randomRosterProfiles.Clear();
		_randomRosterSeed = null;
		RefreshState(_state);
		UpdateRandomRosterButton();
		ShowPopupStatusMessage("已生成随机名册用于调试预览。");
	}

	private void EnsureRandomRosterPreview()
	{
		if (_randomRosterProfiles.Count > 0)
		{
			return;
		}

		var count = ResolveRandomRosterCount();
		_randomRosterSeed ??= BuildRandomSeed();
		_randomRosterProfiles.AddRange(DiscipleRosterSystem.BuildRandomRoster(_state, count, _randomRosterSeed));
	}

	private int ResolveRandomRosterCount()
	{
		var target = Math.Max(_state.Population, RandomRosterMinCount);
		return Math.Clamp(target, RandomRosterMinCount, RandomRosterMaxCount);
	}

	private static int BuildRandomSeed()
	{
		return Random.Shared.Next();
	}

	private void UpdateRandomRosterButton()
	{
		if (!OS.IsDebugBuild())
		{
			if (_debugPanel != null)
			{
				_debugPanel.Visible = false;
			}

			return;
		}

		if (_debugPanel != null)
		{
			_debugPanel.Visible = true;
		}

		if (_randomRosterPreviewActive)
		{
			var seedText = _randomRosterSeed.HasValue ? $"（seed {_randomRosterSeed.Value}）" : string.Empty;
			_randomRosterButton.Text = RandomRosterButtonActiveText;
			_randomRosterButton.TooltipText = $"当前为随机名册预览{seedText}，点击恢复宗门名册。";
			return;
		}

		_randomRosterButton.Text = RandomRosterButtonIdleText;
		_randomRosterButton.TooltipText = "生成一批随机弟子名册，仅用于调试预览。";
	}

	private void RefreshSummary()
	{
		var talentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(_state);
		var law = SectGovernanceRules.GetActiveLawDefinition(_state);
		var direction = SectGovernanceRules.GetActiveDevelopmentDefinition(_state);
		_treeCountLabel.Text = $"总录门人：{_allProfiles.Count} 人";
		_sectRootTitleLabel.Text = SectNamingRules.GetName(_state, SectNamingRules.SectNameKey);
		_sectRootMetaLabel.Text = $"谱系收录 {_allProfiles.Count} 人 · 当前筛读 {_visibleProfiles.Count} 人";

		if (_randomRosterPreviewActive)
		{
			_summaryLabel.Text =
				$"卷册总录：调试预览 {_allProfiles.Count} 人 · 当前批注 {DiscipleDirectiveRules.BuildDirectiveSummary(_state)}";
		}
		else
		{
			_summaryLabel.Text =
				$"卷册总录：门人 {_state.Population} · 真传 {_state.ElitePopulation} · 现役 {_state.GetAssignedPopulation()} · 待命 {_state.GetUnassignedPopulation()} · {DiscipleDirectiveRules.BuildDirectiveSummary(_state)}";
		}
		_governanceLabel.Text =
			$"当前治宗：{direction.DisplayName} / {law.DisplayName} / {talentPlan.DisplayName}";
		RefreshMinimalistTreeHeader();
	}

	private void RebuildDiscipleList()
	{
		_visibleProfiles.Clear();
		_visibleProfiles.AddRange(_allProfiles.Where(MatchesFilter));
		SortProfiles(_visibleProfiles);

		if (_minimalistTreeLayoutBuilt)
		{
			RebuildMinimalistTree();
			return;
		}

		_rosterCardButtons.Clear();
		ClearPeakColumns();

		if (_visibleProfiles.Count == 0)
		{
			_peakRow.AddChild(CreateEmptyPeakColumn());
			ClearDetail();
			return;
		}

		foreach (var peakGroup in BuildPeakSections(_visibleProfiles))
		{
			_peakRow.AddChild(CreatePeakColumn(peakGroup.Key, peakGroup.ToList()));
		}

		var selectedIndex = _visibleProfiles.FindIndex(profile => profile.Id == _selectedDiscipleId);
		if (selectedIndex < 0)
		{
			selectedIndex = 0;
			_selectedDiscipleId = _visibleProfiles[0].Id;
		}

		SelectRosterCard(_selectedDiscipleId);
		RefreshDetail(_visibleProfiles[selectedIndex]);
	}

	private void RefreshDetail(DiscipleProfile profile)
	{
		var identityTag = ResolveIdentityTag(profile);
		var techniqueTag = ResolveTechniqueTag(profile);
		var skillTag = ResolveSkillTag(profile);
		var directiveText = DiscipleDirectiveRules.GetDirectiveDisplayName(profile.DirectiveType);
		// 依据长期火候提炼当前弟子的专精路数，让详情页能直接读出培养方向差异。
		var specializationSummary = DiscipleCultivationRules.BuildSpecializationSummary(_state, profile.Id);
		var specializationSummaryText = specializationSummary == "路数未成" ? "尚在积累" : specializationSummary;
		var specializationNarrative = DiscipleCultivationRules.BuildSpecializationNarrative(_state, profile.Id);
		var branchSummary = DiscipleCultivationRules.BuildSpecializationBranchSummary(_state, profile.Id);
		var branchNarrative = DiscipleCultivationRules.BuildSpecializationBranchNarrative(_state, profile.Id);
		var dutyAffinitySummary = DiscipleCultivationRules.BuildDutyAffinitySummary(_state, profile);
		var techniqueCraftSpecializationSummary = DiscipleCultivationRules.BuildTechniqueCraftSpecializationSummary(_state, profile.Id);
		var specializationEffectSummary = DiscipleCultivationRules.BuildSpecializationEffectSummary(_state, profile.Id);
		var latestInsightSummary = DiscipleCultivationRules.BuildLatestInsightSummary(_state, profile.Id);

		_profileNameLabel.Text = profile.Name;
		_profileMetaLabel.Text =
			$"身份：{identityTag}  |  修为：{profile.RealmName}  |  功法：{techniqueTag}  |  技艺：{skillTag}\n骨龄：{profile.AgeText}  |  籍录：{ResolveRosterPeakTitle(profile)} / {ResolveRosterHallTitle(profile)}  |  职：{profile.DutyDisplayName}  |  谱位：{profile.RankName}\n交互：{directiveText}";
		_rootCircleLabel.Text = ResolveRootSummary(profile);
		_realmStatusLabel.Text = $"修为境界：{profile.RealmName}";
		_realmProgressBar.Value = ResolveRealmProgress(profile);
		var techniqueStage = DiscipleCultivationRules.GetTrackStageLabel(_state, profile.Id, DiscipleCultivationAssignmentType.TechniquePolish);
		_realmProgressHintLabel.Text = string.IsNullOrWhiteSpace(techniqueStage)
			? $"进度：{ResolveRealmProgressText(profile)}"
			: $"进度：{ResolveRealmProgressText(profile)} · 功法{techniqueStage}";
		_combatSealLabel.Text = ResolveCombatSeal(profile);
		_combatSealHintLabel.Text = $"（{ResolveCombatSealHint(profile)}；修炼反哺：{DiscipleCultivationRules.BuildPerformanceFeedbackSummary(_state, profile.Id)}）";
		_qiSeaProgressBar.Value = ResolveQiSeaProgress(profile);
		var meditationStage = DiscipleCultivationRules.GetTrackStageLabel(_state, profile.Id, DiscipleCultivationAssignmentType.Meditation);
		_qiSeaHintLabel.Text = string.IsNullOrWhiteSpace(meditationStage)
			? $"蓄量：{ResolveQiSeaText(profile)}"
			: $"蓄量：{ResolveQiSeaText(profile)} · 静修{meditationStage}";
		_profileStatusLabel.Text =
			$"当前差事：{profile.CurrentAssignment}\n居所：{profile.ResidenceName}\n关联峰脉：{profile.LinkedPeakSummary}\n修炼积累：{DiscipleCultivationRules.BuildLongTermProgressSummary(_state, profile.Id)}\n培养路数：{specializationSummaryText}\n专修分支：{branchSummary}\n专精映照：{techniqueCraftSpecializationSummary}\n差事相性：{dutyAffinitySummary}\n专精效用：{specializationEffectSummary}\n最近修炼：{DiscipleCultivationRules.BuildLatestHistorySummary(_state, profile.Id)}\n最近感悟：{latestInsightSummary}";
		_directiveStatusLabel.Text = $"当前批注：{directiveText}";
		_directiveEffectLabel.Text = $"{BuildDirectiveEffectText(profile)}\n修炼反哺：{DiscipleCultivationRules.BuildPerformanceFeedbackSummary(_state, profile.Id)}\n路数批注：{specializationNarrative}\n分支批注：{branchNarrative}";
		_annotationLabel.Text = BuildAnnotation(profile);
		_fullInfoLabel.Text = BuildFullInfoText(profile, identityTag, techniqueTag, skillTag);
		RefreshTraits(profile);
		UpdateDirectiveButtons(profile);
		UpdateCultivationJumpButton(profile);

		SetMetric("Insight", profile.Insight);
		SetMetric("Potential", profile.Potential);
		SetMetric("Health", profile.Health);
		SetMetric("Craft", profile.Craft);
		SetMetric("Mood", profile.Mood);
		SetMetric("HeartState", ResolveHeartState(profile));
		SetMetric("Combat", profile.Combat);
		SetMetric("Execution", profile.Execution);
		SetMetric("Contribution", profile.Contribution);

		UpdateRadarChart(
			("悟性", profile.Insight),
			("潜力", profile.Potential),
			("根骨", profile.Health),
			("匠艺", profile.Craft),
			("神魂", profile.Mood),
			("心境", ResolveHeartState(profile)));
		CallVisualFx("transition_profile_card");
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
		_directiveStatusLabel.Text = "当前批注：未录";
		_directiveEffectLabel.Text = "未选中弟子时，无法下达外务候补或执事培养批注。";
		_annotationLabel.Text = string.Empty;
		_fullInfoLabel.Text = "[color=#5b4d42]卷中详录暂未展开，请先选中一名弟子。[/color]";
		RefreshTraits(null);
		UpdateDirectiveButtons(null);
		UpdateCultivationJumpButton(null);

		SetMetric("Insight", 0);
		SetMetric("Potential", 0);
		SetMetric("Health", 0);
		SetMetric("Craft", 0);
		SetMetric("Mood", 0);
		SetMetric("HeartState", 0);
		SetMetric("Combat", 0);
		SetMetric("Execution", 0);
		SetMetric("Contribution", 0);
		UpdateRadarChart(
			("悟性", 0),
			("潜力", 0),
			("根骨", 0),
			("匠艺", 0),
			("神魂", 0),
			("心境", 0));
		CallVisualFx("transition_profile_card");
	}

	private void ShowTreePage()
	{
		// 宗门大谱为宏观视角，收起返回按钮并保留名册选中。
		_treePage.Visible = true;
		_profilePage.Visible = false;
		_backButton.Visible = false;
		CallVisualFx("switch_to_tree");
	}

	private void ShowProfilePage()
	{
		// 命谱详情用于聚焦单人信息，开启返回按钮并触发过场动效。
		_treePage.Visible = false;
		_profilePage.Visible = true;
		_backButton.Visible = true;
		CallVisualFx("switch_to_profile");
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
		CallVisualFx("pulse_roster_refresh");
	}

	private void OnSortSelected(long index)
	{
		_sortMode = (SortMode)(int)index;
		RebuildDiscipleList();
		RefreshPopupHint();
		CallVisualFx("pulse_roster_refresh");
	}

	private void OnRosterCardPressed(int discipleId)
	{
		var profile = _visibleProfiles.FirstOrDefault(candidate => candidate.Id == discipleId);
		if (profile == null)
		{
			return;
		}

		_selectedDiscipleId = discipleId;
		_selectedPeakKey = ResolveRosterPeakKey(profile);
		SelectRosterCard(discipleId);
		SelectLineageNode(discipleId);
		RefreshDetail(profile);
		ShowProfilePage();
	}

	private void RequestDirectiveChange(DiscipleDirectiveType directiveType)
	{
		if (_randomRosterPreviewActive)
		{
			ShowPopupStatusMessage("随机名册仅用于调试预览，暂不可批注。");
			return;
		}

		var profile = _visibleProfiles.FirstOrDefault(candidate => candidate.Id == _selectedDiscipleId);
		if (profile == null)
		{
			ShowPopupStatusMessage("当前未选中弟子，无法入卷批注。");
			return;
		}

		DirectiveRequested?.Invoke(profile.Id, directiveType);
		ShowPopupStatusMessage($"已将“{profile.Name}”的卷中批注提请执事层更新。");
	}

	private void RequestCultivationOpen()
	{
		if (_randomRosterPreviewActive)
		{
			ShowPopupStatusMessage("随机名册仅用于调试预览，暂不可转入修炼卷。");
			return;
		}

		var profile = _visibleProfiles.FirstOrDefault(candidate => candidate.Id == _selectedDiscipleId);
		if (profile == null)
		{
			ShowPopupStatusMessage("当前未选中弟子，无法转入修炼卷。");
			return;
		}

		CultivationRequested?.Invoke(profile.Id);
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

	private void BuildMinimalistTreeLayout()
	{
		if (_minimalistTreeLayoutBuilt)
		{
			return;
		}

		// “灵鉴录·清简留白”重构：树页改为左侧支脉导航、右侧血脉族谱。
		var treePage = GetNode<Control>("ScreenMargin/ScreenRoot/TreePage");
		var treeColumn = GetNode<Control>("ScreenMargin/ScreenRoot/TreePage/TreeColumn");
		var headerRow = GetNode<HBoxContainer>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow");
		var titleGroup = GetNode<Control>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TitleGroup");
		var treeCountBadge = GetNode<Control>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/HeaderRow/TreeCountBadge");
		var summaryPanel = GetNode<PanelContainer>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/SummaryPanel");
		var filterPanel = GetNode<PanelContainer>("ScreenMargin/ScreenRoot/TreePage/TreeColumn/FilterPanel");

		_minimalistHost = new Control
		{
			Name = "MinimalistJadeRosterHost",
			LayoutMode = 1
		};
		_minimalistHost.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_minimalistHost.GrowHorizontal = Control.GrowDirection.Both;
		_minimalistHost.GrowVertical = Control.GrowDirection.Both;
		treePage.AddChild(_minimalistHost);

		var mainRow = new HBoxContainer
		{
			Name = "MainRow",
			LayoutMode = 1
		};
		mainRow.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		mainRow.GrowHorizontal = Control.GrowDirection.Both;
		mainRow.GrowVertical = Control.GrowDirection.Both;
		mainRow.AddThemeConstantOverride("separation", 30);
		_minimalistHost.AddChild(mainRow);

		var sidebar = new VBoxContainer
		{
			Name = "Sidebar",
			CustomMinimumSize = new Vector2(SidebarWidth, 0f),
			SizeFlagsHorizontal = Control.SizeFlags.Fill,
			SizeFlagsVertical = Control.SizeFlags.Fill
		};
		sidebar.AddThemeConstantOverride("separation", 14);
		mainRow.AddChild(sidebar);

		var divider = new ColorRect
		{
			Name = "Divider",
			CustomMinimumSize = new Vector2(1f, 0f),
			Color = new Color(DividerColor.R, DividerColor.G, DividerColor.B, 0.55f)
		};
		divider.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		mainRow.AddChild(divider);

		var treeArea = new Control
		{
			Name = "TreeArea",
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			ClipContents = true
		};
		mainRow.AddChild(treeArea);

		var treeOverlay = new VBoxContainer
		{
			Name = "TreeOverlay",
			LayoutMode = 1
		};
		treeOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		treeOverlay.GrowHorizontal = Control.GrowDirection.Both;
		treeOverlay.GrowVertical = Control.GrowDirection.Both;
		treeOverlay.AddThemeConstantOverride("separation", 10);
		treeArea.AddChild(treeOverlay);

		var treeHeader = new VBoxContainer
		{
			Name = "TreeHeader",
			LayoutMode = 2
		};
		treeHeader.AddThemeConstantOverride("separation", 4);
		treeOverlay.AddChild(treeHeader);

		_treeAreaTitleLabel = new Label
		{
			Name = "TreeAreaTitleLabel",
			Text = "血脉族谱",
			HorizontalAlignment = HorizontalAlignment.Left
		};
		_treeAreaTitleLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_treeAreaTitleLabel.AddThemeFontSizeOverride("font_size", 24);
		treeHeader.AddChild(_treeAreaTitleLabel);

		_treeAreaSubtitleLabel = new Label
		{
			Name = "TreeAreaSubtitleLabel",
			Text = "请先选定左侧支脉。",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		_treeAreaSubtitleLabel.AddThemeColorOverride("font_color", InkGrayColor);
		_treeAreaSubtitleLabel.AddThemeFontSizeOverride("font_size", 11);
		treeHeader.AddChild(_treeAreaSubtitleLabel);

		_treeCanvasScroll = new ScrollContainer
		{
			Name = "TreeCanvasScroll",
			LayoutMode = 2,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			VerticalScrollMode = ScrollContainer.ScrollMode.Auto
		};
		treeOverlay.AddChild(_treeCanvasScroll);

		_treeCanvas = new Control
		{
			Name = "TreeCanvas",
			CustomMinimumSize = new Vector2(TreeCanvasMinWidth, TreeCanvasBaseHeight),
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		_treeCanvasScroll.AddChild(_treeCanvas);

		_watermarkLabel = new Label
		{
			Name = "WatermarkLabel",
			Text = "浮云宗大谱",
			LayoutMode = 0,
			RotationDegrees = 90f,
			Modulate = new Color(0f, 0f, 0f, 0.04f)
		};
		_watermarkLabel.AddThemeFontSizeOverride("font_size", 108);
		_watermarkLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_treeCanvas.AddChild(_watermarkLabel);
		_watermarkLabel.Position = new Vector2(250f, 460f);

		ReparentTreeControl(sidebar, titleGroup);
		ReparentTreeControl(sidebar, treeCountBadge);
		ReparentTreeControl(sidebar, summaryPanel);
		ReparentTreeControl(sidebar, filterPanel);
		if (_debugPanel != null)
		{
			ReparentTreeControl(sidebar, _debugPanel);
		}

		var branchTitle = new Label
		{
			Name = "BranchTitle",
			Text = "支脉导航"
		};
		branchTitle.AddThemeColorOverride("font_color", CinnabarColor);
		branchTitle.AddThemeFontSizeOverride("font_size", 14);
		sidebar.AddChild(branchTitle);

		var branchHint = new Label
		{
			Name = "BranchHint",
			Text = "切换左侧支脉后，右侧只展开该脉的师承与门人。",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		branchHint.AddThemeColorOverride("font_color", InkGrayColor);
		branchHint.AddThemeFontSizeOverride("font_size", 11);
		sidebar.AddChild(branchHint);

		var branchScroll = new ScrollContainer
		{
			Name = "BranchScroll",
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			VerticalScrollMode = ScrollContainer.ScrollMode.Auto
		};
		sidebar.AddChild(branchScroll);

		_sidebarBranchList = new VBoxContainer
		{
			Name = "BranchList"
		};
		_sidebarBranchList.AddThemeConstantOverride("separation", 14);
		branchScroll.AddChild(_sidebarBranchList);

		treeColumn.Visible = false;
		headerRow.Visible = false;
		StyleMinimalistStaticPanels(titleGroup, treeCountBadge, summaryPanel, filterPanel, _debugPanel);
		_minimalistTreeLayoutBuilt = true;
	}

	private void ReparentTreeControl(Control newParent, Control child)
	{
		var oldParent = child.GetParent();
		oldParent?.RemoveChild(child);
		newParent.AddChild(child);
	}

	private void StyleMinimalistStaticPanels(
		Control titleGroup,
		Control treeCountBadge,
		PanelContainer summaryPanel,
		PanelContainer filterPanel,
		Control? debugPanel)
	{
		titleGroup.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		treeCountBadge.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		ApplyPaperCardStyle(summaryPanel, 24);
		ApplyPaperCardStyle(filterPanel, 24);
		if (debugPanel is PanelContainer debugCard)
		{
			ApplyPaperCardStyle(debugCard, 24);
		}

		if (treeCountBadge is PanelContainer badgePanel)
		{
			var badgeStyle = CreatePaperCardStyle(26);
			badgeStyle.BgColor = new Color(0.971f, 0.969f, 0.956f, 0.92f);
			badgeStyle.BorderColor = new Color(CinnabarColor.R, CinnabarColor.G, CinnabarColor.B, 0.25f);
			badgePanel.AddThemeStyleboxOverride("panel", badgeStyle);
		}

		foreach (var panel in new[] { summaryPanel, filterPanel })
		{
			panel.AddThemeConstantOverride("minimum_character_width", 0);
		}

		if (titleGroup.GetNodeOrNull<PanelContainer>("TitleSeal") is { } titleSeal)
		{
			var sealStyle = CreatePaperCardStyle(26);
			sealStyle.BgColor = new Color(CinnabarColor.R, CinnabarColor.G, CinnabarColor.B, 0.95f);
			sealStyle.BorderColor = new Color(CinnabarColor.R, CinnabarColor.G, CinnabarColor.B, 0.95f);
			titleSeal.AddThemeStyleboxOverride("panel", sealStyle);
			if (titleSeal.GetNodeOrNull<Label>("SealLabel") is { } sealLabel)
			{
				sealLabel.AddThemeColorOverride("font_color", PaperHighlightColor);
				sealLabel.AddThemeFontSizeOverride("font_size", 22);
			}
		}

		if (titleGroup.GetNodeOrNull<Label>("TitleColumn/TitleLabel") is { } titleLabel)
		{
			titleLabel.AddThemeColorOverride("font_color", InkBlackColor);
			titleLabel.AddThemeFontSizeOverride("font_size", 26);
		}

		if (titleGroup.GetNodeOrNull<Label>("TitleColumn/SubtitleLabel") is { } subtitleLabel)
		{
			subtitleLabel.AddThemeColorOverride("font_color", InkGrayColor);
			subtitleLabel.AddThemeFontSizeOverride("font_size", 11);
		}

		_treeCountLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_treeCountLabel.AddThemeFontSizeOverride("font_size", 12);
		_summaryLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_summaryLabel.AddThemeFontSizeOverride("font_size", 12);
		_governanceLabel.AddThemeColorOverride("font_color", InkGrayColor);
		_governanceLabel.AddThemeFontSizeOverride("font_size", 11);

		_filterOption.AddThemeFontSizeOverride("font_size", 12);
		_sortOption.AddThemeFontSizeOverride("font_size", 12);
		ApplySoftSelectStyle(_filterOption);
		ApplySoftSelectStyle(_sortOption);

		if (_randomRosterButton != null)
		{
			ApplyGhostButtonStyle(_randomRosterButton);
		}
	}

	private void RebuildMinimalistTree()
	{
		_rosterCardButtons.Clear();
		_lineageNodeHosts.Clear();
		ClearControlChildren(_sidebarBranchList);
		ClearTreeCanvasChildren();

		var peakGroups = BuildPeakSections(_visibleProfiles).ToList();
		if (peakGroups.Count == 0)
		{
			_selectedPeakKey = null;
			RefreshMinimalistTreeHeader();
			BuildEmptyTreeState();
			ClearDetail();
			return;
		}

		EnsureSelectedPeak(peakGroups);
		BuildBranchTabs(peakGroups);
		RefreshMinimalistTreeHeader();

		var selectedGroup = peakGroups.First(group => group.Key == _selectedPeakKey);
		var selectedProfiles = selectedGroup.ToList();
		if (selectedProfiles.All(profile => profile.Id != _selectedDiscipleId))
		{
			_selectedDiscipleId = selectedProfiles[0].Id;
		}

		BuildLineageTree(selectedGroup.Key, selectedProfiles);
		SelectLineageNode(_selectedDiscipleId);

		var selectedProfile = selectedProfiles.First(profile => profile.Id == _selectedDiscipleId);
		RefreshDetail(selectedProfile);
	}

	private void EnsureSelectedPeak(IReadOnlyList<IGrouping<string, DiscipleProfile>> peakGroups)
	{
		string? preferredPeakKey = null;
		var preferredProfile = _visibleProfiles.FirstOrDefault(profile => profile.Id == _selectedDiscipleId);
		if (preferredProfile != null)
		{
			preferredPeakKey = ResolveRosterPeakKey(preferredProfile);
		}

		if (!string.IsNullOrWhiteSpace(preferredPeakKey) &&
			peakGroups.Any(group => group.Key == preferredPeakKey))
		{
			_selectedPeakKey = preferredPeakKey;
			return;
		}

		if (!string.IsNullOrWhiteSpace(_selectedPeakKey) &&
			peakGroups.Any(group => group.Key == _selectedPeakKey))
		{
			return;
		}

		_selectedPeakKey = peakGroups[0].Key;
	}

	private void RefreshMinimalistTreeHeader()
	{
		if (!_minimalistTreeLayoutBuilt)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(_selectedPeakKey))
		{
			_treeAreaTitleLabel.Text = "血脉族谱";
			_treeAreaSubtitleLabel.Text = "当前筛选下暂无可展开支脉。";
			return;
		}

		var selectedProfiles = _visibleProfiles
			.Where(profile => ResolveRosterPeakKey(profile) == _selectedPeakKey)
			.ToList();
		var branchTitle = ResolveRosterPeakTitle(_selectedPeakKey!);
		_treeAreaTitleLabel.Text = $"{branchTitle} · 血脉族谱";
		_treeAreaSubtitleLabel.Text =
			$"本脉收录 {selectedProfiles.Count} 人，当前以胶囊玉简展开师承与门人。选中弟子后可转入命谱详情。";
	}

	private void BuildEmptyTreeState()
	{
		var emptyLabel = new Label
		{
			Text = "当前筛读下暂无弟子收录。",
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			LayoutMode = 0,
			Position = new Vector2(120f, 180f),
			Size = new Vector2(560f, 40f)
		};
		emptyLabel.AddThemeColorOverride("font_color", InkGrayColor);
		emptyLabel.AddThemeFontSizeOverride("font_size", 18);
		_treeCanvas.AddChild(emptyLabel);
	}

	private void BuildBranchTabs(IReadOnlyList<IGrouping<string, DiscipleProfile>> peakGroups)
	{
		_branchTabButtons.Clear();
		foreach (var peakGroup in peakGroups)
		{
			var button = CreateBranchTabButton(peakGroup.Key, peakGroup.ToList());
			_sidebarBranchList.AddChild(button);
			_branchTabButtons[peakGroup.Key] = button;
		}
	}

	private Button CreateBranchTabButton(string peakKey, IReadOnlyList<DiscipleProfile> profiles)
	{
		var button = new Button
		{
			Name = $"{peakKey}BranchTab",
			CustomMinimumSize = new Vector2(72f, 188f),
			SizeFlagsHorizontal = Control.SizeFlags.Fill,
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
			Flat = true,
			Text = BuildVerticalText(ResolveRosterPeakTitle(peakKey)),
			TooltipText = $"{ResolveRosterPeakTitle(peakKey)} · 收录 {profiles.Count} 人"
		};
		button.AddThemeFontSizeOverride("font_size", 18);
		button.AddThemeConstantOverride("line_separation", 3);
		button.Alignment = HorizontalAlignment.Center;
		button.Pressed += () => OnBranchTabPressed(peakKey);
		button.MouseEntered += () => button.Scale = new Vector2(1.02f, 1.02f);
		button.MouseExited += () => button.Scale = Vector2.One;

		var topDot = CreatePillDot("TopDot");
		topDot.Position = new Vector2(32f, 12f);
		button.AddChild(topDot);

		var bottomDot = CreatePillDot("BottomDot");
		bottomDot.Position = new Vector2(32f, 170f);
		button.AddChild(bottomDot);

		var countLabel = new Label
		{
			Name = "CountLabel",
			Text = $"{profiles.Count}",
			LayoutMode = 0,
			Position = new Vector2(19f, 150f),
			Size = new Vector2(34f, 18f),
			HorizontalAlignment = HorizontalAlignment.Center
		};
		countLabel.AddThemeFontSizeOverride("font_size", 10);
		button.AddChild(countLabel);
		button.SetMeta("count_label", countLabel);
		ApplyBranchTabStyle(button, peakKey == _selectedPeakKey);

		return button;
	}

	private void OnBranchTabPressed(string peakKey)
	{
		_selectedPeakKey = peakKey;
		var selectedProfiles = _visibleProfiles
			.Where(profile => ResolveRosterPeakKey(profile) == peakKey)
			.ToList();
		if (selectedProfiles.Count == 0)
		{
			return;
		}

		if (selectedProfiles.All(profile => profile.Id != _selectedDiscipleId))
		{
			_selectedDiscipleId = selectedProfiles[0].Id;
		}

		RebuildMinimalistTree();
		ShowTreePage();
	}

	private void BuildLineageTree(string peakKey, IReadOnlyList<DiscipleProfile> profiles)
	{
		var orderedProfiles = profiles
			.OrderBy(profile => ResolveRosterRankOrder(profile.RankName))
			.ThenByDescending(profile => profile.RealmTier)
			.ThenBy(profile => profile.Id)
			.ToList();
		var branchTitle = ResolveRosterPeakTitle(peakKey);
		var leader = ResolveBranchLeader(orderedProfiles);
		var discipleRows = Math.Max(1, (int)Math.Ceiling(orderedProfiles.Count / 4f));
		var canvasWidth = Math.Max(TreeCanvasMinWidth, 360f + Math.Min(4, orderedProfiles.Count) * TreeNodeGap);
		var canvasHeight = Math.Max(TreeCanvasBaseHeight, 460f + discipleRows * TreeNodeRowGap);
		_treeCanvas.CustomMinimumSize = new Vector2(canvasWidth, canvasHeight);
		_watermarkLabel.Position = new Vector2(canvasWidth - 220f, canvasHeight * 0.66f);

		var masterHost = CreateLineageNodeHost(
			branchTitle,
			"主脉执录",
			$"{leader.RealmName} / {leader.CurrentAssignment}",
			0,
			canvasWidth * 0.5f - TreeNodeWidth * 0.5f,
			84f,
			false,
			true);
		_treeCanvas.AddChild(masterHost);

		var rootLineX = masterHost.Position.X + TreeNodeWidth * 0.5f;
		var branchLineY = 336f;
		AddTreeLine(rootLineX, masterHost.Position.Y + TreeNodeHeight - 8f, rootLineX, branchLineY);

		var columns = Math.Min(4, orderedProfiles.Count);
		var columnSpacing = columns <= 1 ? 0f : (canvasWidth - 300f) / (columns - 1);
		var firstColumnX = columns <= 1 ? canvasWidth * 0.5f : 150f;
		var centers = new List<float>();
		for (var column = 0; column < columns; column++)
		{
			centers.Add(columns <= 1 ? firstColumnX : firstColumnX + columnSpacing * column);
		}

		AddTreeLine(centers.First(), branchLineY, centers.Last(), branchLineY);

		for (var index = 0; index < orderedProfiles.Count; index++)
		{
			var profile = orderedProfiles[index];
			var column = index % columns;
			var row = index / columns;
			var centerX = centers[column];
			var nodeX = centerX - TreeNodeWidth * 0.5f;
			var nodeY = 422f + row * TreeNodeRowGap;
			AddTreeLine(centerX, branchLineY, centerX, nodeY - 18f);

			var host = CreateLineageNodeHost(
				profile.Name,
				ResolveRosterRankTitle(profile),
				$"{profile.RealmName} · {ResolveRosterHallTitle(profile)} · {profile.CurrentAssignment}",
				profile.Id,
				nodeX,
				nodeY,
				profile.Id == _selectedDiscipleId,
				false);
			_treeCanvas.AddChild(host);
			_lineageNodeHosts[profile.Id] = host;
		}
	}

	private DiscipleProfile ResolveBranchLeader(IReadOnlyList<DiscipleProfile> profiles)
	{
		return profiles
			.OrderByDescending(profile => profile.IsElite)
			.ThenByDescending(profile => profile.RealmTier)
			.ThenByDescending(profile => profile.Contribution)
			.ThenBy(profile => profile.Id)
			.First();
	}

	private Control CreateLineageNodeHost(
		string title,
		string roleText,
		string metaText,
		int discipleId,
		float x,
		float y,
		bool selected,
		bool forceLabelVisible)
	{
		var host = new Control
		{
			Name = discipleId == 0 ? "MasterNode" : $"DiscipleNode{discipleId}",
			LayoutMode = 0,
			Position = new Vector2(x, y),
			Size = new Vector2(TreeNodeWidth + TreeLabelWidth + 18f, TreeNodeHeight)
		};

		var pill = new PanelContainer
		{
			Name = "Pill",
			CustomMinimumSize = new Vector2(TreeNodeWidth, TreeNodeHeight),
			LayoutMode = 0,
			MouseFilter = Control.MouseFilterEnum.Stop
		};
		pill.Size = new Vector2(TreeNodeWidth, TreeNodeHeight);
		ApplyPillStyle(pill, selected);
		host.AddChild(pill);

		var topDot = CreatePillDot("TopDot");
		topDot.Position = new Vector2(TreeNodeWidth * 0.5f - 3f, 12f);
		pill.AddChild(topDot);

		var bottomDot = CreatePillDot("BottomDot");
		bottomDot.Position = new Vector2(TreeNodeWidth * 0.5f - 3f, TreeNodeHeight - 18f);
		pill.AddChild(bottomDot);

		var nameLabel = new Label
		{
			Name = "NameLabel",
			Text = BuildVerticalText(title),
			LayoutMode = 0,
			Position = new Vector2(16f, 30f),
			Size = new Vector2(TreeNodeWidth - 32f, TreeNodeHeight - 60f),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		nameLabel.AddThemeColorOverride("font_color", selected ? PaperHighlightColor : InkBlackColor);
		nameLabel.AddThemeFontSizeOverride("font_size", discipleId == 0 ? 20 : 16);
		pill.AddChild(nameLabel);

		var museumLabel = new VBoxContainer
		{
			Name = "MuseumLabel",
			LayoutMode = 0,
			Position = new Vector2(TreeNodeWidth + 20f, 40f),
			SizeFlagsHorizontal = Control.SizeFlags.Fill,
			MouseFilter = Control.MouseFilterEnum.Stop
		};
		museumLabel.AddThemeConstantOverride("separation", 2);
		museumLabel.MouseDefaultCursorShape = CursorShape.PointingHand;
		host.AddChild(museumLabel);

		var roleLabel = new Label
		{
			Text = roleText,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		roleLabel.AddThemeColorOverride("font_color", selected ? CinnabarColor : InkBlackColor);
		roleLabel.AddThemeFontSizeOverride("font_size", 12);
		museumLabel.AddChild(roleLabel);

		var metaLabel = new Label
		{
			Text = metaText,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			CustomMinimumSize = new Vector2(TreeLabelWidth, 0f),
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		metaLabel.AddThemeColorOverride("font_color", InkGrayColor);
		metaLabel.AddThemeFontSizeOverride("font_size", 10);
		museumLabel.AddChild(metaLabel);
		museumLabel.Modulate = new Color(1f, 1f, 1f, forceLabelVisible || selected ? 1f : 0f);

		if (discipleId != 0)
		{
			pill.GuiInput += @event => OnLineageNodeGuiInput(@event, discipleId);
			pill.MouseEntered += () => SetLineageNodeHover(host, true);
			pill.MouseExited += () => SetLineageNodeHover(host, false);
			museumLabel.GuiInput += @event => OnLineageNodeGuiInput(@event, discipleId);
			museumLabel.MouseEntered += () => SetLineageNodeHover(host, true);
			museumLabel.MouseExited += () => SetLineageNodeHover(host, false);
		}

		host.SetMeta("pill", pill);
		host.SetMeta("museum_label", museumLabel);
		host.SetMeta("base_position", host.Position);
		host.SetMeta("selected", selected);
		return host;
	}

	private void OnLineageNodeGuiInput(InputEvent @event, int discipleId)
	{
		if (@event is not InputEventMouseButton mouseEvent ||
			!mouseEvent.Pressed ||
			mouseEvent.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		OnRosterCardPressed(discipleId);
	}

	private void SetLineageNodeHover(Control host, bool hovered)
	{
		if (!host.HasMeta("base_position"))
		{
			return;
		}
		var basePosition = (Vector2)host.GetMeta("base_position");

		var museumLabel = host.GetMeta("museum_label").AsGodotObject() as Control;
		var selected = host.GetMeta("selected").AsBool();
		var tween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(host, "position", hovered ? basePosition + new Vector2(0f, -6f) : basePosition, hovered ? 0.18 : 0.24);
		if (museumLabel != null)
		{
			var targetAlpha = hovered || selected ? 1f : 0f;
			tween.TweenProperty(museumLabel, "modulate:a", targetAlpha, hovered ? 0.18 : 0.24);
		}
	}

	private void SelectLineageNode(int discipleId)
	{
		foreach (var (entryId, host) in _lineageNodeHosts)
		{
			var selected = entryId == discipleId;
			host.SetMeta("selected", selected);
			if (host.GetMeta("pill").AsGodotObject() is PanelContainer pill)
			{
				ApplyPillStyle(pill, selected);
				if (pill.GetNodeOrNull<Label>("NameLabel") is { } nameLabel)
				{
					nameLabel.AddThemeColorOverride("font_color", selected ? PaperHighlightColor : InkBlackColor);
				}
			}

			if (host.GetMeta("museum_label").AsGodotObject() is Control museumLabel)
			{
				museumLabel.Modulate = new Color(1f, 1f, 1f, selected ? 1f : 0f);
			}

			if (selected)
			{
				_treeCanvasScroll.EnsureControlVisible(host);
			}
		}

		foreach (var (peakKey, button) in _branchTabButtons)
		{
			ApplyBranchTabStyle(button, peakKey == _selectedPeakKey);
		}
	}

	private void AddTreeLine(float x1, float y1, float x2, float y2)
	{
		if (Math.Abs(x1 - x2) <= 0.1f)
		{
			var line = new ColorRect
			{
				Name = $"VLine_{x1}_{y1}",
				LayoutMode = 0,
				Position = new Vector2(x1 - 0.5f, Math.Min(y1, y2)),
				Size = new Vector2(1f, Math.Abs(y2 - y1)),
				Color = new Color(DividerColor.R, DividerColor.G, DividerColor.B, 0.88f)
			};
			_treeCanvas.AddChild(line);
			return;
		}

		var horizontalLine = new ColorRect
		{
			Name = $"HLine_{x1}_{y1}",
			LayoutMode = 0,
			Position = new Vector2(Math.Min(x1, x2), y1 - 0.5f),
			Size = new Vector2(Math.Abs(x2 - x1), 1f),
			Color = new Color(DividerColor.R, DividerColor.G, DividerColor.B, 0.88f)
		};
		_treeCanvas.AddChild(horizontalLine);
	}

	private static void ClearControlChildren(Control control)
	{
		foreach (var child in control.GetChildren())
		{
			control.RemoveChild(child);
			child.QueueFree();
		}
	}

	private void ClearTreeCanvasChildren()
	{
		foreach (var child in _treeCanvas.GetChildren())
		{
			if (ReferenceEquals(child, _watermarkLabel))
			{
				continue;
			}

			_treeCanvas.RemoveChild(child);
			child.QueueFree();
		}
	}

	private void ApplyBranchTabStyle(Button button, bool active)
	{
		var style = CreatePillStyle(active);
		button.AddThemeStyleboxOverride("normal", style);
		button.AddThemeStyleboxOverride("hover", style);
		button.AddThemeStyleboxOverride("pressed", style);
		button.AddThemeStyleboxOverride("focus", style);
		button.AddThemeColorOverride("font_color", active ? PaperHighlightColor : InkBlackColor);
		button.AddThemeColorOverride("font_hover_color", active ? PaperHighlightColor : InkBlackColor);
		button.AddThemeColorOverride("font_pressed_color", active ? PaperHighlightColor : InkBlackColor);
		if (button.HasMeta("count_label") && button.GetMeta("count_label").AsGodotObject() is Label countLabel)
		{
			countLabel.AddThemeColorOverride("font_color", active ? PaperHighlightColor : InkGrayColor);
		}
	}

	private void ApplyPillStyle(PanelContainer panel, bool active)
	{
		panel.AddThemeStyleboxOverride("panel", CreatePillStyle(active));
	}

	private StyleBoxFlat CreatePillStyle(bool active)
	{
		var style = new StyleBoxFlat
		{
			BgColor = active ? CinnabarColor : new Color(JadeMistColor.R, JadeMistColor.G, JadeMistColor.B, 0.98f),
			BorderColor = active
				? new Color(CinnabarColor.R, CinnabarColor.G, CinnabarColor.B, 0.96f)
				: new Color(PaperHighlightColor.R, PaperHighlightColor.G, PaperHighlightColor.B, 0.76f),
			DrawCenter = true,
			CornerRadiusTopLeft = 32,
			CornerRadiusTopRight = 32,
			CornerRadiusBottomLeft = 32,
			CornerRadiusBottomRight = 32,
			ShadowColor = active
				? new Color(CinnabarColor.R, CinnabarColor.G, CinnabarColor.B, 0.22f)
				: new Color(0f, 0f, 0f, 0.08f),
			ShadowSize = active ? 16 : 12,
			ShadowOffset = new Vector2(0f, 6f)
		};
		style.SetBorderWidthAll(1);
		style.ContentMarginTop = 10;
		style.ContentMarginBottom = 10;
		style.ContentMarginLeft = 8;
		style.ContentMarginRight = 8;
		return style;
	}

	private ColorRect CreatePillDot(string name)
	{
		return new ColorRect
		{
			Name = name,
			LayoutMode = 0,
			Color = new Color(InkBlackColor.R, InkBlackColor.G, InkBlackColor.B, 0.72f),
			Size = new Vector2(6f, 6f)
		};
	}

	private StyleBoxFlat CreatePaperCardStyle(int cornerRadius)
	{
		var style = new StyleBoxFlat
		{
			BgColor = new Color(PaperColor.R, PaperColor.G, PaperColor.B, 0.94f),
			BorderColor = new Color(PaperHighlightColor.R, PaperHighlightColor.G, PaperHighlightColor.B, 0.72f),
			CornerRadiusTopLeft = cornerRadius,
			CornerRadiusTopRight = cornerRadius,
			CornerRadiusBottomLeft = cornerRadius,
			CornerRadiusBottomRight = cornerRadius,
			ShadowColor = new Color(0f, 0f, 0f, 0.04f),
			ShadowSize = 10,
			ShadowOffset = new Vector2(0f, 4f)
		};
		style.SetBorderWidthAll(1);
		style.ContentMarginTop = 10;
		style.ContentMarginBottom = 10;
		style.ContentMarginLeft = 10;
		style.ContentMarginRight = 10;
		return style;
	}

	private void ApplyPaperCardStyle(PanelContainer panel, int cornerRadius)
	{
		panel.AddThemeStyleboxOverride("panel", CreatePaperCardStyle(cornerRadius));
	}

	private void ApplySoftSelectStyle(BaseButton button)
	{
		var normal = CreatePaperCardStyle(18);
		var hover = CreatePaperCardStyle(18);
		hover.BorderColor = new Color(CinnabarColor.R, CinnabarColor.G, CinnabarColor.B, 0.22f);
		button.AddThemeStyleboxOverride("normal", normal);
		button.AddThemeStyleboxOverride("hover", hover);
		button.AddThemeStyleboxOverride("pressed", hover);
		button.AddThemeStyleboxOverride("focus", hover);
		button.AddThemeColorOverride("font_color", InkBlackColor);
	}

	private void ApplyGhostButtonStyle(Button button)
	{
		var normal = CreatePaperCardStyle(18);
		normal.BgColor = new Color(PaperHighlightColor.R, PaperHighlightColor.G, PaperHighlightColor.B, 0.90f);
		var hover = CreatePaperCardStyle(18);
		hover.BgColor = new Color(CinnabarColor.R, CinnabarColor.G, CinnabarColor.B, 0.12f);
		hover.BorderColor = new Color(CinnabarColor.R, CinnabarColor.G, CinnabarColor.B, 0.20f);
		button.AddThemeStyleboxOverride("normal", normal);
		button.AddThemeStyleboxOverride("hover", hover);
		button.AddThemeStyleboxOverride("pressed", hover);
		button.AddThemeStyleboxOverride("focus", hover);
		button.AddThemeColorOverride("font_color", InkBlackColor);
		button.AddThemeColorOverride("font_hover_color", InkBlackColor);
		button.AddThemeColorOverride("font_pressed_color", InkBlackColor);
	}

	private static string BuildVerticalText(string text)
	{
		return string.Join('\n', text.Where(character => !char.IsWhiteSpace(character)));
	}

	private void CallVisualFx(string methodName, params Variant[] args)
	{
		_visualFx?.Call(methodName, args);
	}

	private void UpdateRadarChart(params (string Label, int Value)[] stats)
	{
		var payload = new Godot.Collections.Array();
		foreach (var (label, value) in stats)
		{
			payload.Add(new Godot.Collections.Dictionary
			{
				{ "label", label },
				{ "value", value }
			});
		}

		_radarChart.Call("set_stats", payload);
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
		CallVisualFx("apply_metric_value_tone", binding.ValueLabel, clamped);
	}

	private IEnumerable<IGrouping<string, DiscipleProfile>> BuildPeakSections(IEnumerable<DiscipleProfile> profiles)
	{
		return profiles
			.GroupBy(ResolveRosterPeakKey)
			.OrderBy(group => ResolveRosterPeakOrder(group.Key))
			.ThenBy(group => group.Key);
	}

	private IEnumerable<IGrouping<string, DiscipleProfile>> BuildHallSections(IEnumerable<DiscipleProfile> profiles)
	{
		return profiles
			.GroupBy(ResolveRosterHallKey)
			.OrderBy(group => ResolveRosterHallOrder(group.Key))
			.ThenBy(group => group.Key);
	}

	private void ClearPeakColumns()
	{
		foreach (var child in _peakRow.GetChildren())
		{
			_peakRow.RemoveChild(child);
			child.QueueFree();
		}
	}

	private Control CreateEmptyPeakColumn()
	{
		var container = new VBoxContainer();
		container.CustomMinimumSize = new Vector2(260f, 0f);
		var label = new Label
		{
			Text = "当前筛选下暂无弟子收录。",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		container.AddChild(label);
		return container;
	}

	private Control CreatePeakColumn(string peakKey, IReadOnlyList<DiscipleProfile> profiles)
	{
		var peakColumn = (VBoxContainer)_peakColumnTemplate.Duplicate();
		peakColumn.Visible = true;
		var peakTitleLabel = peakColumn.GetNode<Label>("PeakCard/PeakMargin/PeakColumn/PeakTitleLabel");
		var peakCountLabel = peakColumn.GetNode<Label>("PeakCard/PeakMargin/PeakColumn/PeakCountLabel");
		var hallStack = peakColumn.GetNode<VBoxContainer>("HallStack");

		peakTitleLabel.Text = ResolveRosterPeakTitle(peakKey);
		peakCountLabel.Text = $"{profiles.Count} 人";
		CallVisualFx("style_peak_card", peakColumn.GetNode<PanelContainer>("PeakCard"));

		foreach (var hallGroup in BuildHallSections(profiles))
		{
			hallStack.AddChild(CreateHallGroup(hallGroup.Key, hallGroup.ToList()));
		}

		return peakColumn;
	}

	private Control CreateHallGroup(string hallKey, IReadOnlyList<DiscipleProfile> profiles)
	{
		var hallGroup = (VBoxContainer)_hallGroupTemplate.Duplicate();
		hallGroup.Visible = true;
		var hallTitleLabel = hallGroup.GetNode<Label>("HallCard/HallMargin/HallColumn/HallTitleLabel");
		var hallMetaLabel = hallGroup.GetNode<Label>("HallCard/HallMargin/HallColumn/HallMetaLabel");
		var discipleColumn = hallGroup.GetNode<VBoxContainer>("DiscipleColumn");

		hallTitleLabel.Text = SectNamingRules.GetName(_state, hallKey);
		hallMetaLabel.Text = $"{profiles.Count} 人";
		CallVisualFx("style_hall_card", hallGroup.GetNode<PanelContainer>("HallCard"));

		foreach (var profile in profiles
					 .OrderBy(profile => ResolveRosterRankOrder(profile.RankName))
					 .ThenBy(profile => profile.Name))
		{
			discipleColumn.AddChild(CreateDiscipleCard(profile));
		}

		return hallGroup;
	}

	private PanelContainer CreateDiscipleCard(DiscipleProfile profile)
	{
		var card = (PanelContainer)_discipleCardTemplate.Duplicate();
		card.Visible = true;
		card.MouseFilter = Control.MouseFilterEnum.Stop;
		card.CustomMinimumSize = new Vector2(0f, 82f);
		card.TooltipText = $"{profile.DutyDisplayName} · {profile.RealmName} · {profile.LinkedPeakSummary}";
		SetMouseIgnoreRecursive(card);
		card.GetNode<Label>("CardMargin/CardColumn/DiscipleBadgeLabel").Text = profile.IsElite ? "真传" : profile.RankName;
		card.GetNode<Label>("CardMargin/CardColumn/DiscipleNameLabel").Text = profile.Name;
		card.GetNode<Label>("CardMargin/CardColumn/DiscipleRealmLabel").Text = profile.RealmName;
		card.GetNode<Label>("CardMargin/CardColumn/DiscipleDutyLabel").Text = profile.CurrentAssignment;
		card.GuiInput += @event => OnRosterCardGuiInput(@event, profile.Id);
		_rosterCardButtons[profile.Id] = card;
		CallVisualFx("style_roster_card", card, profile.Id == _selectedDiscipleId);
		return card;
	}

	private static void SetMouseIgnoreRecursive(Node node)
	{
		foreach (var child in node.GetChildren())
		{
			if (child is Control control)
			{
				control.MouseFilter = Control.MouseFilterEnum.Ignore;
			}

			SetMouseIgnoreRecursive(child);
		}
	}

	private void SelectRosterCard(int discipleId)
	{
		foreach (var entry in _rosterCardButtons)
		{
			CallVisualFx("style_roster_card", entry.Value, entry.Key == discipleId);
		}

		if (_rosterCardButtons.TryGetValue(discipleId, out var card))
		{
			_rosterScroll.EnsureControlVisible(card);
		}
	}

	private void OnRosterCardGuiInput(InputEvent @event, int discipleId)
	{
		// 族谱卡片改为 PanelContainer 后，改用点击输入桥接详情跳转。
		if (@event is not InputEventMouseButton mouseEvent ||
			!mouseEvent.Pressed ||
			mouseEvent.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		OnRosterCardPressed(discipleId);
	}

	private static int ResolveRosterPeakOrder(string peakKey)
	{
		return peakKey switch
		{
			SectNamingRules.PeakTianyanKey => 0,
			SectNamingRules.PeakQingyunKey => 1,
			SectNamingRules.HallAffairsKey => 2,
			SectNamingRules.HallSeedlingKey => 3,
			_ => 9
		};
	}

	private string ResolveRosterPeakTitle(string peakKey)
	{
		return SectNamingRules.GetName(_state, peakKey);
	}

	private string ResolveRosterPeakTitle(DiscipleProfile profile)
	{
		return ResolveRosterPeakTitle(ResolveRosterPeakKey(profile));
	}

	private string ResolveRosterPeakKey(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return SectNamingRules.HallSeedlingKey;
		}

		if (profile.IsElite)
		{
			return profile.JobType switch
			{
				JobType.Worker => SectNamingRules.PeakTianyanKey,
				JobType.Merchant => SectNamingRules.PeakQingyunKey,
				JobType.Scholar => SectNamingRules.PeakQingyunKey,
				JobType.Farmer => SectNamingRules.PeakTianyuanKey,
				_ => SectNamingRules.PeakTianyanKey
			};
		}

		return profile.JobType switch
		{
			JobType.Farmer => SectNamingRules.PeakTianyuanKey,
			JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal)
				? SectNamingRules.PeakTianquanKey
				: SectNamingRules.PeakTiangongKey,
			JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal)
				? SectNamingRules.PeakTianshuKey
				: SectNamingRules.PeakQingyunKey,
			JobType.Scholar => SectNamingRules.PeakTianjiKey,
			_ => SectNamingRules.HallAffairsKey
		};
	}

	private string ResolveRosterHallTitle(DiscipleProfile profile)
	{
		return SectNamingRules.GetName(_state, ResolveRosterHallKey(profile));
	}

	private string ResolveRosterHallKey(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return SectNamingRules.HallTeachingInstituteKey;
		}

		if (profile.IsElite)
		{
			return profile.JobType switch
			{
				JobType.Worker => SectNamingRules.HallGeneralKey,
				JobType.Merchant => SectNamingRules.HallExternalAffairsKey,
				JobType.Scholar => SectNamingRules.HallTransmissionKey,
				JobType.Farmer => SectNamingRules.HallReliefKey,
				_ => SectNamingRules.HallGeneralKey
			};
		}

		return profile.JobType switch
		{
			JobType.Farmer => SectNamingRules.HallReliefKey,
			JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal)
				? SectNamingRules.HallBearMountainKey
				: SectNamingRules.HallForgeKey,
			JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal)
				? SectNamingRules.HallDiplomacyKey
				: SectNamingRules.HallExternalAffairsKey,
			JobType.Scholar => profile.CurrentAssignment.Contains("讲法", StringComparison.Ordinal)
				? SectNamingRules.HallTeachingInstituteKey
				: SectNamingRules.HallResearchKey,
			_ => SectNamingRules.HallOuterDutyKey
		};
	}

	private static int ResolveRosterHallOrder(string hallKey)
	{
		return hallKey switch
		{
			SectNamingRules.HallGeneralKey => 0,
			SectNamingRules.HallExternalAffairsKey => 1,
			SectNamingRules.HallTransmissionKey => 2,
			SectNamingRules.HallTeachingInstituteKey => 3,
			SectNamingRules.HallResearchKey => 4,
			SectNamingRules.HallForgeKey => 5,
			SectNamingRules.HallBearMountainKey => 6,
			SectNamingRules.HallDiplomacyKey => 7,
			SectNamingRules.HallReliefKey => 8,
			SectNamingRules.HallOuterDutyKey => 9,
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

	private PanelContainer CreateTraitTag(string text)
	{
		var panel = (PanelContainer)_traitTagTemplate.Duplicate();
		panel.Visible = true;

		var label = panel.GetNode<Label>("TagMargin/TagLabel");
		label.Text = text;
		CallVisualFx("style_trait_tag", panel, label);

		return panel;
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

	private static string ResolveEquipmentSummary(DiscipleProfile profile)
	{
		var equipment = profile.EquipmentProfile;
		if (equipment == null)
		{
			return "暂无记载";
		}

		var summary = $"{equipment.WeaponName} / {equipment.ArmorName} · {equipment.RelicName} · {equipment.TalismanName}";
		return $"{summary}（{equipment.QualityTag} · 战备 {equipment.GearScore}）";
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

	private static string ResolvePotentialSeal(DiscipleProfile profile)
	{
		return profile.Potential switch
		{
			>= 88 => "根骨上乘",
			>= 72 => "根骨中上",
			>= 56 => "根骨可塑",
			>= 40 => "根骨尚稳",
			_ => "根骨薄弱"
		};
	}

	private static string ResolveBodySeal(DiscipleProfile profile)
	{
		return profile.Health switch
		{
			>= 82 => "气血充盈",
			>= 64 => "气血稳实",
			>= 46 => "气血稍浮",
			_ => "气血失衡"
		};
	}

	private static string ResolveHeartSeal(DiscipleProfile profile)
	{
		var heartState = ResolveHeartState(profile);
		return heartState switch
		{
			>= 82 => "心境澄明",
			>= 64 => "心境安定",
			>= 46 => "心境浮动",
			_ => "心境躁乱"
		};
	}

	private static string ResolveTrainingFocus(DiscipleProfile profile)
	{
		var focuses = new (string Label, int Value)[]
		{
			("悟性", profile.Insight),
			("潜力", profile.Potential),
			("匠艺", profile.Craft),
			("战修", profile.Combat),
			("执行", profile.Execution),
			("贡献", profile.Contribution)
		};

		var topFocuses = focuses
			.OrderByDescending(item => item.Value)
			.ThenBy(item => item.Label, StringComparer.Ordinal)
			.Take(2)
			.Select(item => item.Label)
			.ToArray();

		return topFocuses.Length == 0 ? "暂无判定" : string.Join(" · ", topFocuses);
	}

	private string ResolveCultivationPlan(DiscipleProfile profile)
	{
		var cultivationAssignment = DiscipleCultivationRules.GetAssignment(_state, profile.Id);
		if (cultivationAssignment != DiscipleCultivationAssignmentType.None)
		{
			return
				$"{DiscipleCultivationRules.GetAssignmentDisplayName(cultivationAssignment)}为主，" +
				$"{DiscipleCultivationRules.GetAssignmentShortEffect(cultivationAssignment)} " +
				$"当前火候：{DiscipleCultivationRules.BuildTrackProgressSummary(_state, profile.Id, cultivationAssignment)}";
		}

		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return "启蒙课业为主，稳固根基。";
		}

		var focus = ResolveTrainingFocus(profile);
		var focusText = focus == "暂无判定" ? "主线稳修" : $"兼修 {focus}";
		return profile.DirectiveType switch
		{
			DiscipleDirectiveType.OuterMissionCandidate => $"外务历练为主，{focusText}。",
			DiscipleDirectiveType.StewardCandidate => $"内务磨砺为主，{focusText}。",
			_ => $"常制稳修，{focusText}。"
		};
	}

	private static string ResolveRealmStageTag(DiscipleProfile profile)
	{
		var realmName = profile.RealmName;
		if (realmName.Contains("炼气", StringComparison.Ordinal))
		{
			return "初阶·炼气";
		}

		if (realmName.Contains("筑基", StringComparison.Ordinal))
		{
			return "初阶·筑基";
		}

		if (realmName.Contains("金丹", StringComparison.Ordinal))
		{
			return "中阶·金丹";
		}

		if (realmName.Contains("元婴", StringComparison.Ordinal))
		{
			return "中阶·元婴";
		}

		if (realmName.Contains("化神", StringComparison.Ordinal))
		{
			return "高阶·化神";
		}

		return realmName.Contains("凡俗", StringComparison.Ordinal) ? "未入门" : "境界未明";
	}

	private static string BuildResumeDigest(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return $"启蒙新苗，随“{profile.CurrentAssignment}”研习，居 {profile.ResidenceName}。";
		}

		var peakSummary = string.IsNullOrWhiteSpace(profile.LinkedPeakSummary)
			? "峰脉未定"
			: profile.LinkedPeakSummary;

		return $"现任{profile.DutyDisplayName}，主线“{profile.CurrentAssignment}”，居 {profile.ResidenceName}，归 {peakSummary}。";
	}

	private string BuildAnnotation(DiscipleProfile profile)
	{
		var primaryTrait = profile.TraitSummary
			.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
			.FirstOrDefault() ?? "气机平和";
		var specializationSummary = DiscipleCultivationRules.BuildSpecializationSummary(_state, profile.Id);
		var specializationNarrative = DiscipleCultivationRules.BuildSpecializationNarrative(_state, profile.Id);
		var branchSummary = DiscipleCultivationRules.BuildSpecializationBranchSummary(_state, profile.Id);
		var branchNarrative = DiscipleCultivationRules.BuildSpecializationBranchNarrative(_state, profile.Id);
		var specializationSentence = specializationSummary == "路数未成"
			? "专精路数尚浅，当前仍以常制修行为主。"
			: $"其路数已归作“{specializationSummary}”，并牵出“{branchSummary}”，{specializationNarrative} {branchNarrative}";

		return
			$"观其气机，{primaryTrait}，骨相与心识相济。现下以“{profile.CurrentAssignment}”为主线，" +
			$"修炼卷记为“{ResolveCultivationPlan(profile)}”，其长期积累为“{DiscipleCultivationRules.BuildLongTermProgressSummary(_state, profile.Id)}”。" +
			$"{specializationSentence} 近时又记“{DiscipleCultivationRules.BuildLatestHistorySummary(_state, profile.Id)}”。" +
			$"{profile.Note} 若能继续借 {ResolveRosterPeakTitle(profile)} {ResolveRosterHallTitle(profile)} 之务磨砺，则在 {profile.RealmName} 上尚可再进一步。";
	}

	private string BuildFullInfoText(DiscipleProfile profile, string identityTag, string techniqueTag, string skillTag)
	{
		var secondarySkillTag = ResolveSecondarySkillTag(profile);
		var eliteText = profile.IsElite ? "已入真传卷" : "未入真传卷";
		var directiveText = DiscipleDirectiveRules.GetDirectiveDisplayName(profile.DirectiveType);
		var directiveEffect = DiscipleDirectiveRules.GetDirectiveShortEffect(profile.DirectiveType);
		var cultivationAssignment = DiscipleCultivationRules.GetAssignment(_state, profile.Id);
		var cultivationEffect = DiscipleCultivationRules.GetAssignmentShortEffect(cultivationAssignment);
		var cultivationProgressSummary = DiscipleCultivationRules.BuildLongTermProgressSummary(_state, profile.Id);
		var cultivationActiveProgress = DiscipleCultivationRules.BuildActiveTrackProgressSummary(_state, profile.Id);
		var cultivationHistoryText = DiscipleCultivationRules.BuildHistoryMultilineText(_state, profile.Id);
		var cultivationInsightSummary = DiscipleCultivationRules.BuildLatestInsightSummary(_state, profile.Id);
		var cultivationFeedbackSummary = DiscipleCultivationRules.BuildPerformanceFeedbackSummary(_state, profile.Id);
		var cultivationSpecializationSummary = DiscipleCultivationRules.BuildSpecializationSummary(_state, profile.Id);
		var cultivationSpecializationSummaryText = cultivationSpecializationSummary == "路数未成" ? "尚在积累" : cultivationSpecializationSummary;
		var cultivationSpecializationNarrative = DiscipleCultivationRules.BuildSpecializationNarrative(_state, profile.Id);
		var cultivationBranchSummary = DiscipleCultivationRules.BuildSpecializationBranchSummary(_state, profile.Id);
		var cultivationBranchNarrative = DiscipleCultivationRules.BuildSpecializationBranchNarrative(_state, profile.Id);
		var cultivationTechniqueSpecialization = DiscipleCultivationRules.BuildTechniqueSpecializationLabel(_state, profile.Id);
		var cultivationPrimarySkillSpecialization = DiscipleCultivationRules.BuildPrimaryCraftSpecializationLabel(_state, profile.Id);
		var cultivationSecondarySkillSpecialization = DiscipleCultivationRules.BuildSecondaryCraftSpecializationLabel(_state, profile.Id);
		var cultivationDutyAffinitySummary = DiscipleCultivationRules.BuildDutyAffinitySummary(_state, profile);
		var cultivationSpecializationEffectSummary = DiscipleCultivationRules.BuildSpecializationEffectSummary(_state, profile.Id);
		var overviewLine = $"{identityTag} · {profile.RealmName} · {techniqueTag} · {skillTag}";
		var rootSummaryLine = ResolveRootSummary(profile).Replace("\n", " · ");
		var detailLines = new[]
		{
			$"[b]概览[/b]：{overviewLine}",
			$"[b]卷册编号[/b]：第 {profile.Id} 号",
			string.Empty,
			"[b]身份与籍录[/b]",
			$"[b]姓名[/b]：{profile.Name}",
			$"[b]谱位[/b]：{profile.RankName} · {eliteText}",
			$"[b]骨龄[/b]：{profile.AgeText}",
			$"[b]职司[/b]：{profile.DutyDisplayName}",
			$"[b]当前差事[/b]：{profile.CurrentAssignment}",
			$"[b]居所[/b]：{profile.ResidenceName}",
			$"[b]关联峰脉[/b]：{profile.LinkedPeakSummary}",
			$"[b]履历侧记[/b]：{BuildResumeDigest(profile)}",
			string.Empty,
			"[b]修为与功法[/b]",
			$"[b]修为[/b]：{profile.RealmName}（层级 {profile.RealmTier}）",
			$"[b]修行阶段[/b]：{ResolveRealmStageTag(profile)}",
			$"[b]修为进度[/b]：{ResolveRealmProgressText(profile)}",
			$"[b]气海蓄量[/b]：{ResolveQiSeaText(profile)}",
			$"[b]战力评定[/b]：{ResolveCombatSeal(profile)}（{ResolveCombatSealHint(profile)}）",
			$"[b]装备/法器[/b]：{ResolveEquipmentSummary(profile)}",
			$"[b]灵根摘要[/b]：{rootSummaryLine}",
			$"[b]根骨评语[/b]：{ResolvePotentialSeal(profile)}",
			$"[b]功法[/b]：{techniqueTag}",
			$"[b]主修技艺[/b]：{skillTag}",
			$"[b]辅修技艺[/b]：{secondarySkillTag}",
			$"[b]功法偏锋[/b]：{cultivationTechniqueSpecialization}",
			$"[b]主艺偏锋[/b]：{cultivationPrimarySkillSpecialization}",
			$"[b]辅艺映照[/b]：{cultivationSecondarySkillSpecialization}",
			$"[b]修行安排[/b]：{ResolveCultivationPlan(profile)}",
			$"[b]修炼卷批注[/b]：{cultivationEffect}",
			$"[b]长期积累[/b]：{cultivationProgressSummary}",
			$"[b]当前火候[/b]：{cultivationActiveProgress}",
			$"[b]培养路数[/b]：{cultivationSpecializationSummaryText}",
			$"[b]专修分支[/b]：{cultivationBranchSummary}",
			$"[b]差事相性[/b]：{cultivationDutyAffinitySummary}",
			$"[b]路数批注[/b]：{cultivationSpecializationNarrative}",
			$"[b]分支批注[/b]：{cultivationBranchNarrative}",
			$"[b]专精效用[/b]：{cultivationSpecializationEffectSummary}",
			$"[b]火候反哺[/b]：{cultivationFeedbackSummary}",
			$"[b]最近感悟[/b]：{cultivationInsightSummary}",
			$"[b]修炼履历[/b]：\n{cultivationHistoryText}",
			string.Empty,
			"[b]性情与指标[/b]",
			$"[b]交互指令[/b]：{directiveText}（{directiveEffect}）",
			$"[b]性情印记[/b]：{profile.TraitSummary}",
			$"[b]心境评语[/b]：{ResolveHeartSeal(profile)}",
			$"[b]体魄评语[/b]：{ResolveBodySeal(profile)}",
			$"[b]培养侧重[/b]：{ResolveTrainingFocus(profile)}",
			$"[b]气血 / 心境 / 潜力 / 战力[/b]：{profile.Health} / {profile.Mood} / {profile.Potential} / {profile.Combat}",
			$"[b]匠艺 / 悟性 / 执行 / 贡献[/b]：{profile.Craft} / {profile.Insight} / {profile.Execution} / {profile.Contribution}",
			$"[b]培养批注[/b]：{profile.Note}"
		};

		return string.Join("\n", detailLines);
	}

	private string BuildDirectiveEffectText(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return "启蒙新苗暂只记录成长，不纳入外务候补或执事培养重点名册。";
		}

		var directiveSummary = DiscipleDirectiveRules.BuildDiscipleDirectiveEffectSummary(_state, profile);
		var dutyAffinitySummary = DiscipleCultivationRules.BuildDutyAffinitySummary(_state, profile);
		return $"{directiveSummary}\n当前差事相性：{dutyAffinitySummary}";
	}

	private static string ResolveIdentityTag(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return "新苗弟子";
		}

		if (profile.IsElite)
		{
			return "真传弟子";
		}

		if (profile.AgeBand == DiscipleAgeBand.Elder)
		{
			return "守峰执事";
		}

		if (profile.Potential >= 74)
		{
			return "内门弟子";
		}

		return profile.JobType.HasValue ? "外门弟子" : "候值门人";
	}

	private string ResolveTechniqueTag(DiscipleProfile profile)
	{
		string baseTechnique;
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			baseTechnique = "启蒙·养气篇";
			return baseTechnique;
		}

		baseTechnique = profile.JobType switch
		{
			JobType.Farmer => profile.IsElite ? "灵植·归元真诀" : "灵植·归元诀",
			JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal) ? "阵堂·承山诀" : "天工·锻机诀",
			JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal) ? "外域·行远诀" : "青云·通路诀",
			JobType.Scholar => profile.CurrentAssignment.Contains("讲法", StringComparison.Ordinal) ? "青云·真诀" : "天机·明衍诀",
			_ => "待定功法"
		};
		baseTechnique = DiscipleCultivationRules.DecorateTechniqueDisplayName(_state, profile.Id, baseTechnique);

		var stageLabel = DiscipleCultivationRules.GetTrackStageLabel(_state, profile.Id, DiscipleCultivationAssignmentType.TechniquePolish);
		return string.IsNullOrWhiteSpace(stageLabel) ? baseTechnique : $"{baseTechnique}（{stageLabel}）";
	}

	private string ResolveSkillTag(DiscipleProfile profile)
	{
		string baseSkill;
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			baseSkill = "待定技艺";
			return baseSkill;
		}

		baseSkill = profile.JobType switch
		{
			JobType.Farmer => "灵植",
			JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal) ? "阵法" : "炼器",
			JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal) ? "御兽" : "傀儡",
			JobType.Scholar => profile.CurrentAssignment.Contains("讲法", StringComparison.Ordinal) ? "符箓" : "天机",
			_ => "待定技艺"
		};
		baseSkill = DiscipleCultivationRules.DecoratePrimarySkillDisplayName(_state, profile.Id, baseSkill);

		var stageLabel = DiscipleCultivationRules.GetTrackStageLabel(_state, profile.Id, DiscipleCultivationAssignmentType.CraftPractice);
		return string.IsNullOrWhiteSpace(stageLabel) ? baseSkill : $"{baseSkill}（{stageLabel}）";
	}

	private string ResolveSecondarySkillTag(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return "基础课业";
		}

		var fallbackSkill = profile.JobType switch
		{
			JobType.Farmer => "医道",
			JobType.Worker => "阵法",
			JobType.Merchant => "卜算",
			JobType.Scholar => "符箓",
			_ => "基础庶务"
		};
		return DiscipleCultivationRules.DecorateSecondarySkillDisplayName(_state, profile.Id, fallbackSkill);
	}

	private void UpdateDirectiveButtons(DiscipleProfile? profile)
	{
		if (_randomRosterPreviewActive)
		{
			_directiveNoneButton.ToggleMode = true;
			_directiveOuterButton.ToggleMode = true;
			_directiveStewardButton.ToggleMode = true;

			_directiveNoneButton.ButtonPressed = false;
			_directiveOuterButton.ButtonPressed = false;
			_directiveStewardButton.ButtonPressed = false;

			_directiveNoneButton.Disabled = true;
			_directiveOuterButton.Disabled = true;
			_directiveStewardButton.Disabled = true;
			return;
		}

		var directiveType = profile?.DirectiveType ?? DiscipleDirectiveType.None;
		var allowSpecialDirective = profile != null && profile.AgeBand != DiscipleAgeBand.Seedling;

		_directiveNoneButton.ToggleMode = true;
		_directiveOuterButton.ToggleMode = true;
		_directiveStewardButton.ToggleMode = true;

		_directiveNoneButton.ButtonPressed = directiveType == DiscipleDirectiveType.None;
		_directiveOuterButton.ButtonPressed = directiveType == DiscipleDirectiveType.OuterMissionCandidate;
		_directiveStewardButton.ButtonPressed = directiveType == DiscipleDirectiveType.StewardCandidate;

		_directiveNoneButton.Disabled = profile == null;
		_directiveOuterButton.Disabled = !allowSpecialDirective;
		_directiveStewardButton.Disabled = !allowSpecialDirective;
	}

	private void UpdateCultivationJumpButton(DiscipleProfile? profile)
	{
		if (_randomRosterPreviewActive)
		{
			_cultivationJumpButton.Disabled = true;
			_cultivationJumpButton.Text = "修炼卷预览禁用";
			return;
		}

		if (profile == null)
		{
			_cultivationJumpButton.Disabled = true;
			_cultivationJumpButton.Text = "送入修炼卷";
			return;
		}

		var cultivationAssignment = DiscipleCultivationRules.GetAssignment(_state, profile.Id);
		var assignmentText = DiscipleCultivationRules.GetAssignmentDisplayName(cultivationAssignment);
		_cultivationJumpButton.Disabled = false;
		_cultivationJumpButton.Text = cultivationAssignment == DiscipleCultivationAssignmentType.None
			? "送入修炼卷"
			: $"修炼卷：{assignmentText}";
		_cultivationJumpButton.TooltipText = $"打开修炼卷并定位到“{profile.Name}”。";
	}

	private static string ToChineseProgressText(int percent)
	{
		var numerals = new[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
		var clamped = Math.Clamp(percent, 0, 99);
		var tens = clamped / 10;
		var ones = clamped % 10;
		return ones == 0 ? $"{numerals[tens]}成" : $"{numerals[tens]}成{numerals[ones]}分";
	}

}
