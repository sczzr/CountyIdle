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
	private const string MetricGridPath =
		"Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel/FoundationMargin/FoundationColumn/StatsCenter/MetricGrid";
	private const int RandomRosterMinCount = 12;
	private const int RandomRosterMaxCount = 48;
	private const string RandomRosterButtonIdleText = "调试：随机名册";
	private const string RandomRosterButtonActiveText = "调试：恢复宗门名册";

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
	private Control? _debugPanel;
	private Button _randomRosterButton = null!;
	private Tree _rosterTree = null!;
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
	private readonly Dictionary<int, TreeItem> _rosterItems = new();
	private Node? _visualFx;
	private bool _uiBound;
	private bool _randomRosterPreviewActive;
	private int? _randomRosterSeed;

	private GameState _state = new();
	private int _selectedDiscipleId = 1;
	private FilterMode _filterMode;
	private SortMode _sortMode;

	public event Action<int, DiscipleDirectiveType>? DirectiveRequested;

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

		RefreshSummary();
		RebuildDiscipleList();
		UpdateRandomRosterButton();
		RefreshPopupHint();
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

		_summaryLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/SummaryPanel/SummaryMargin/SummaryColumn/SummaryLabel");
		_governanceLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/SummaryPanel/SummaryMargin/SummaryColumn/GovernanceLabel");
		_filterOption = GetNode<OptionButton>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/FilterPanel/FilterMargin/FilterColumn/FilterOption");
		_sortOption = GetNode<OptionButton>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/FilterPanel/FilterMargin/FilterColumn/SortOption");
		_debugPanel = GetNodeOrNull<Control>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/DebugPanel");
		_randomRosterButton = GetNode<Button>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/DebugPanel/DebugMargin/DebugRow/RandomRosterButton");
		_rosterTree = GetNode<Tree>("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/RosterFrame/RosterMargin/RosterTree");
		_profileNameLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/ProfileHeader/ProfileTextColumn/ProfileName");
		_profileMetaLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/ProfileHeader/ProfileTextColumn/ProfileMeta");
		_profileStatusLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/ProfileHeader/ProfileTextColumn/ProfileStatus");
		_directiveStatusLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/DirectivePanel/DirectiveMargin/DirectiveColumn/DirectiveStatus");
		_directiveEffectLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/DirectivePanel/DirectiveMargin/DirectiveColumn/DirectiveEffect");
		_rootCircleLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/ProfileHeader/RootCircle/RootCircleLabel");
		_radarChart = GetNode<Control>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel/FoundationMargin/FoundationColumn/RadarCenter/RadarChart");
		_realmStatusLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmStatus");
		_realmProgressBar = GetNode<ProgressBar>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmProgress");
		_realmProgressHintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmHint");
		_qiSeaProgressBar = GetNode<ProgressBar>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/QiSeaBox/QiSeaProgress");
		_qiSeaHintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/QiSeaBox/QiSeaHint");
		_combatSealLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag/CombatMargin/CombatColumn/CombatMain");
		_combatSealHintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag/CombatMargin/CombatColumn/CombatHint");
		_traitFlow = GetNode<FlowContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/TraitPanel/TraitMargin/TraitColumn/TraitFlow");
		_traitTagTemplate = GetNode<PanelContainer>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/TraitPanel/TraitTagTemplate");
		_annotationLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/FullInfoPanel/FullInfoMargin/FullInfoColumn/AnnotationPanel/AnnotationMargin/AnnotationColumn/AnnotationText");
		_fullInfoLabel = GetNode<RichTextLabel>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/FullInfoPanel/FullInfoMargin/FullInfoColumn/FullInfoScroll/FullInfoLabel");
		_directiveNoneButton = GetNode<Button>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/DirectivePanel/DirectiveMargin/DirectiveColumn/DirectiveButtonRow/DirectiveNoneButton");
		_directiveOuterButton = GetNode<Button>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/DirectivePanel/DirectiveMargin/DirectiveColumn/DirectiveButtonRow/DirectiveOuterButton");
		_directiveStewardButton = GetNode<Button>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileRow/DirectivePanel/DirectiveMargin/DirectiveColumn/DirectiveButtonRow/DirectiveStewardButton");
		_hintLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/HintLabel");
		_closeButton = GetNode<Button>("Overlay/Wrapper/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/CloseButton");
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
		_rosterTree.ItemSelected += OnRosterTreeItemSelected;
		_directiveNoneButton.Pressed += () => RequestDirectiveChange(DiscipleDirectiveType.None);
		_directiveOuterButton.Pressed += () => RequestDirectiveChange(DiscipleDirectiveType.OuterMissionCandidate);
		_directiveStewardButton.Pressed += () => RequestDirectiveChange(DiscipleDirectiveType.StewardCandidate);
		_closeButton.Pressed += ClosePopup;

		_traitTagTemplate.Visible = false;

		if (_debugPanel != null)
		{
			_debugPanel.Visible = OS.IsDebugBuild();
		}

		UpdateRandomRosterButton();

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
			peakItem.SetText(0, $"◈ {ResolveRosterPeakTitle(peakGroup.Key)}");
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
		var identityTag = ResolveIdentityTag(profile);
		var techniqueTag = ResolveTechniqueTag(profile);
		var skillTag = ResolveSkillTag(profile);
		var directiveText = DiscipleDirectiveRules.GetDirectiveDisplayName(profile.DirectiveType);

		_profileNameLabel.Text = profile.Name;
		_profileMetaLabel.Text =
			$"身份：{identityTag}  |  修为：{profile.RealmName}  |  功法：{techniqueTag}  |  技艺：{skillTag}\n骨龄：{profile.AgeText}  |  籍录：{ResolveRosterPeakTitle(profile)} / {ResolveRosterHallTitle(profile)}  |  职：{profile.DutyDisplayName}  |  谱位：{profile.RankName}\n交互：{directiveText}";
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
		_directiveStatusLabel.Text = $"当前批注：{directiveText}";
		_directiveEffectLabel.Text = BuildDirectiveEffectText(profile);
		_annotationLabel.Text = BuildAnnotation(profile);
		_fullInfoLabel.Text = BuildFullInfoText(profile, identityTag, techniqueTag, skillTag);
		RefreshTraits(profile);
		UpdateDirectiveButtons(profile);

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

	private void SelectRosterTreeItem(int discipleId)
	{
		if (!_rosterItems.TryGetValue(discipleId, out var item))
		{
			return;
		}

		item.Select(0);
		_rosterTree.EnsureCursorIsVisible();
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

	private static string ResolveCultivationPlan(DiscipleProfile profile)
	{
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

		return $"观其气机，{primaryTrait}，骨相与心识相济。现下以“{profile.CurrentAssignment}”为主线，{profile.Note} 若能继续借 {ResolveRosterPeakTitle(profile)} {ResolveRosterHallTitle(profile)} 之务磨砺，则在 {profile.RealmName} 上尚可再进一步。";
	}

	private string BuildFullInfoText(DiscipleProfile profile, string identityTag, string techniqueTag, string skillTag)
	{
		var secondarySkillTag = ResolveSecondarySkillTag(profile);
		var eliteText = profile.IsElite ? "已入真传卷" : "未入真传卷";
		var directiveText = DiscipleDirectiveRules.GetDirectiveDisplayName(profile.DirectiveType);
		var directiveEffect = DiscipleDirectiveRules.GetDirectiveShortEffect(profile.DirectiveType);
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
			$"[b]修行安排[/b]：{ResolveCultivationPlan(profile)}",
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

		return DiscipleDirectiveRules.BuildDiscipleDirectiveEffectSummary(_state, profile);
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

	private static string ResolveTechniqueTag(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return "启蒙·养气篇";
		}

		return profile.JobType switch
		{
			JobType.Farmer => profile.IsElite ? "灵植·归元真诀" : "灵植·归元诀",
			JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal) ? "阵堂·承山诀" : "天工·锻机诀",
			JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal) ? "外域·行远诀" : "青云·通路诀",
			JobType.Scholar => profile.CurrentAssignment.Contains("讲法", StringComparison.Ordinal) ? "青云·真诀" : "天机·明衍诀",
			_ => "待定功法"
		};
	}

	private static string ResolveSkillTag(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return "待定技艺";
		}

		return profile.JobType switch
		{
			JobType.Farmer => "灵植",
			JobType.Worker => profile.CurrentAssignment.Contains("检修", StringComparison.Ordinal) ? "阵法" : "炼器",
			JobType.Merchant => profile.CurrentAssignment.Contains("商路", StringComparison.Ordinal) ? "御兽" : "傀儡",
			JobType.Scholar => profile.CurrentAssignment.Contains("讲法", StringComparison.Ordinal) ? "符箓" : "天机",
			_ => "待定技艺"
		};
	}

	private static string ResolveSecondarySkillTag(DiscipleProfile profile)
	{
		if (profile.AgeBand == DiscipleAgeBand.Seedling)
		{
			return "基础课业";
		}

		return profile.JobType switch
		{
			JobType.Farmer => "医道",
			JobType.Worker => "阵法",
			JobType.Merchant => "卜算",
			JobType.Scholar => "符箓",
			_ => "基础庶务"
		};
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

	private static string ToChineseProgressText(int percent)
	{
		var numerals = new[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
		var clamped = Math.Clamp(percent, 0, 99);
		var tens = clamped / 10;
		var ones = clamped % 10;
		return ones == 0 ? $"{numerals[tens]}成" : $"{numerals[tens]}成{numerals[ones]}分";
	}

}
