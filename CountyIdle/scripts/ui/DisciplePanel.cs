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
	private Control _radarChart = null!;
	private readonly Dictionary<string, MetricBinding> _metrics = new();
	private readonly List<DiscipleProfile> _allProfiles = new();
	private readonly List<DiscipleProfile> _visibleProfiles = new();
	private readonly Dictionary<int, TreeItem> _rosterItems = new();
	private Node? _visualFx;
	private bool _uiBound;

	private GameState _state = new();
	private int _selectedDiscipleId = 1;
	private FilterMode _filterMode;
	private SortMode _sortMode;

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
		_annotationLabel = GetNode<Label>("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/AnnotationPanel/AnnotationMargin/AnnotationColumn/AnnotationText");
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

	private void RefreshSummary()
	{
		var talentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(_state);
		var law = SectGovernanceRules.GetActiveLawDefinition(_state);
		var direction = SectGovernanceRules.GetActiveDevelopmentDefinition(_state);

		_summaryLabel.Text =
			$"卷册总录：门人 {_state.Population} · 真传 {_state.ElitePopulation} · 现役 {_state.GetAssignedPopulation()} · 待命 {_state.GetUnassignedPopulation()}";
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
		_annotationLabel.Text = string.Empty;
		RefreshTraits(null);

		SetMetric("Insight", 0);
		SetMetric("Potential", 0);
		SetMetric("Health", 0);
		SetMetric("Craft", 0);
		SetMetric("Mood", 0);
		SetMetric("HeartState", 0);
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

}
