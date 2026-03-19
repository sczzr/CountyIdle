using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class CultivationPanel : PopupPanelBase
{
	/// <summary>
	/// 单张玉简卡片的节点绑定。
	/// </summary>
	private sealed class ActionCard
	{
		public ActionCard(
			DiscipleCultivationAssignmentType assignmentType,
			Button button,
			Label statusLabel,
			Control sealBadge,
			PanelContainer primaryEffect,
			PanelContainer secondaryEffect,
			PanelContainer iconBadge,
			Label iconLabel,
			string actionLabel)
		{
			AssignmentType = assignmentType;
			Button = button;
			StatusLabel = statusLabel;
			SealBadge = sealBadge;
			PrimaryEffect = primaryEffect;
			SecondaryEffect = secondaryEffect;
			IconBadge = iconBadge;
			IconLabel = iconLabel;
			ActionLabel = actionLabel;
		}

		public DiscipleCultivationAssignmentType AssignmentType { get; }
		public Button Button { get; }
		public Label StatusLabel { get; }
		public Control SealBadge { get; }
		public PanelContainer PrimaryEffect { get; }
		public PanelContainer SecondaryEffect { get; }
		public PanelContainer IconBadge { get; }
		public Label IconLabel { get; }
		public string ActionLabel { get; }
	}

	/// <summary>
	/// 左侧火候进度条的节点绑定。
	/// </summary>
	private sealed class ProgressTrackWidget
	{
		public ProgressTrackWidget(
			DiscipleCultivationAssignmentType assignmentType,
			ProgressBar progressBar,
			Label tagLabel,
			Control diamondContainer,
			List<PanelContainer> diamonds)
		{
			AssignmentType = assignmentType;
			ProgressBar = progressBar;
			TagLabel = tagLabel;
			DiamondContainer = diamondContainer;
			Diamonds = diamonds;
		}

		public DiscipleCultivationAssignmentType AssignmentType { get; }
		public ProgressBar ProgressBar { get; }
		public Label TagLabel { get; }
		/// <summary>
		/// 菱形刻度容器，用于承载可视化火候段。
		/// </summary>
		public Control DiamondContainer { get; }
		/// <summary>
		/// 菱形刻度节点列表（从左到右）。
		/// </summary>
		public IReadOnlyList<PanelContainer> Diamonds { get; }
	}

	/// <summary>
	/// 左侧点名册单条卡牌节点绑定。
	/// </summary>
	private sealed class RosterEntryWidget
	{
		public RosterEntryWidget(
			Button button,
			ColorRect selectionMarker,
			Label glyphLabel,
			Label nameLabel,
			Label metaLabel,
			PanelContainer assignmentTag,
			Label assignmentTagLabel,
			PanelContainer branchTag,
			Label branchTagLabel)
		{
			Button = button;
			SelectionMarker = selectionMarker;
			GlyphLabel = glyphLabel;
			NameLabel = nameLabel;
			MetaLabel = metaLabel;
			AssignmentTag = assignmentTag;
			AssignmentTagLabel = assignmentTagLabel;
			BranchTag = branchTag;
			BranchTagLabel = branchTagLabel;
		}

		public Button Button { get; }
		public ColorRect SelectionMarker { get; }
		public Label GlyphLabel { get; }
		public Label NameLabel { get; }
		public Label MetaLabel { get; }
		public PanelContainer AssignmentTag { get; }
		public Label AssignmentTagLabel { get; }
		public PanelContainer BranchTag { get; }
		public Label BranchTagLabel { get; }
	}

	private static readonly string[] ChineseTierDigits = ["零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖"];
	private const float RosterMutedAlpha = 0.55f;
	/// <summary>
	/// 火候菱形刻度的常规/填充样式，避免每次刷新都新建样式对象。
	/// </summary>
	private static readonly StyleBoxFlat DiamondEmptyStyle = BuildDiamondStyle(
		new Color(0f, 0f, 0f, 0.45f),
		new Color(0.42f, 0.38f, 0.27f, 0.9f),
		false);
	private static readonly StyleBoxFlat DiamondFilledStyle = BuildDiamondStyle(
		new Color(0.894f, 0.753f, 0.310f, 1f),
		new Color(1f, 1f, 1f, 0.9f),
		true);

	private Label _populationValueLabel = null!;
	private Label _techValueLabel = null!;
	private Label _resourceValueLabel = null!;
	private Label _registerValueLabel = null!;
	private Label _rosterTitleLabel = null!;
	private Label _rosterHintLabel = null!;
	private ScrollContainer _rosterScroll = null!;
	private Label _selectedDiscipleNameLabel = null!;
	private Label _selectedDiscipleMetaLabel = null!;
	private Label _selectedDiscipleRealmLabel = null!;
	private Label _selectedDiscipleAgeLabel = null!;
	private Label _selectedDiscipleResidenceLabel = null!;
	private Label _selectedDiscipleDutyLabel = null!;
	private Label _coreLetterLabel = null!;
	private Label _footerIconLabel = null!;
	private Label _footerTitleLabel = null!;
	private Label _selectedDiscipleStatusLabel = null!;
	private Label _selectedDiscipleStatusHighlightLabel = null!;
	private Label _selectedDiscipleInsightLabel = null!;
	private Button _previousDiscipleButton = null!;
	private Button _nextDiscipleButton = null!;
	private Button? _closeButton;
	private VBoxContainer _discipleRosterList = null!;
	private Button _discipleRosterButtonTemplate = null!;
	private Node? _visualFx;

	private readonly Dictionary<DiscipleCultivationAssignmentType, ActionCard> _actionCards = new();
	private readonly Dictionary<DiscipleCultivationAssignmentType, ProgressTrackWidget> _progressTrackWidgets = new();
	private readonly Dictionary<int, RosterEntryWidget> _rosterButtons = new();
	private readonly List<DiscipleProfile> _profiles = new();

	private GameState _state = new();
	private int _selectedDiscipleId = 1;

	public event Action<int, DiscipleCultivationAssignmentType>? AssignmentRequested;

	public override void _Ready()
	{
		BindUiNodes();
		InitializePopupHint("ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/HintPanel/HintMargin/HintRow/HintLabel");
		BuildProgressTrackWidgets();
		BuildActionCards();
		BindEvents();
		Hide();
	}

	public void Open(GameState state, int? preferredDiscipleId = null)
	{
		RefreshState(state, preferredDiscipleId);
		OpenPopup();
		CallVisualFx("play_open");
	}

	public void ClosePanel()
	{
		ClosePopup();
	}

	public void RefreshState(GameState state, int? preferredDiscipleId = null)
	{
		_state = state.Clone();
		PopulationRules.EnsureDefaults(_state);
		SectGovernanceRules.EnsureDefaults(_state);
		DiscipleDirectiveRules.EnsureDefaults(_state);
		DiscipleCultivationRules.EnsureDefaults(_state);

		_profiles.Clear();
		_profiles.AddRange(DiscipleRosterSystem.BuildRoster(_state));

		if (_profiles.Count <= 0)
		{
			_selectedDiscipleId = 0;
			RefreshSummary();
			RebuildRosterButtons();
			RefreshRosterButtons();
			RefreshSelectedDisciple();
			RefreshProgressTracks();
			RefreshActionCards();
			RefreshPopupHint();
			return;
		}

		if (preferredDiscipleId.HasValue && _profiles.Any(profile => profile.Id == preferredDiscipleId.Value))
		{
			_selectedDiscipleId = preferredDiscipleId.Value;
		}
		else if (_profiles.All(profile => profile.Id != _selectedDiscipleId))
		{
			_selectedDiscipleId = _profiles[0].Id;
		}

		RefreshSummary();
		RebuildRosterButtons();
		RefreshRosterButtons();
		RefreshSelectedDisciple();
		RefreshProgressTracks();
		RefreshActionCards();
		RefreshPopupHint();
		EnsureSelectedRosterEntryVisible();
	}

	public override void _Process(double delta)
	{
		TickPopupStatus(delta);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible)
		{
			return;
		}

		if (!TryHandlePopupClose(@event))
		{
			return;
		}

		GetViewport().SetInputAsHandled();
	}

	protected override string GetPopupHintText()
	{
		if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
		{
			return PopupStatusMessage!;
		}

		var profile = GetSelectedProfile();
		if (profile == null)
		{
			return "名册未成，暂无法敕令。";
		}

		return "结算时将产出对应资源与修为。";
	}

	private void BindUiNodes()
	{
		_populationValueLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/PopulationCard/CardMargin/CardColumn/ValueLabel");
		_techValueLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/TechCard/CardMargin/CardColumn/ValueLabel");
		_resourceValueLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/ResourceCard/CardMargin/CardColumn/ValueLabel");
		_registerValueLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/RegisterCard/CardMargin/CardColumn/ValueLabel");
		_rosterTitleLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterTitleLabel");
		_rosterHintLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterHintLabel");
		_rosterScroll = GetNode<ScrollContainer>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll");
		_discipleRosterList = GetNode<VBoxContainer>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList");
		_discipleRosterButtonTemplate = GetNode<Button>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate");
		_selectedDiscipleNameLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/SelectedDiscipleNameLabel");
		_selectedDiscipleMetaLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/SelectedDiscipleMetaLabel");
		_selectedDiscipleRealmLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/RealmBadge/RealmLabel");
		_selectedDiscipleAgeLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/AgeTag/AgeTagLabel");
		_selectedDiscipleResidenceLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/ResidenceTag/ResidenceTagLabel");
		_selectedDiscipleDutyLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/DutyTag/DutyLabel");
		_coreLetterLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/CoreLetterLabel");
		_footerIconLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/FooterIcon/FooterIconLabel");
		_footerTitleLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/FooterTextColumn/FooterTitleLabel");
		_selectedDiscipleStatusLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/FooterTextColumn/SelectedDiscipleStatusLabel");
		_selectedDiscipleStatusHighlightLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/SelectedDiscipleStatusHighlightLabel");
		_selectedDiscipleInsightLabel = GetNode<Label>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/SelectedDiscipleInsightLabel");
		_previousDiscipleButton = GetNode<Button>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/DiscipleNavRow/PreviousDiscipleButton");
		_nextDiscipleButton = GetNode<Button>("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/DiscipleNavRow/NextDiscipleButton");
		// 响应式改版期间顶部返回层可能尚未接回，关闭按钮允许缺席并由其他入口兜底关闭。
		_closeButton = GetNodeOrNull<Button>("TopOverlay/TopMargin/TopRow/CloseButton");
		_visualFx = GetNodeOrNull<Node>("VisualFx");
	}

	private void BindEvents()
	{
		// 顶部关闭按钮缺席时不再抛空引用，卷页至少保留左右切换与外部关闭能力。
		if (_closeButton is not null)
		{
			_closeButton.Pressed += ClosePanel;
		}
		_previousDiscipleButton.Pressed += () => StepSelectedDisciple(-1);
		_nextDiscipleButton.Pressed += () => StepSelectedDisciple(1);
	}

	private void BuildProgressTrackWidgets()
	{
		_progressTrackWidgets.Clear();
		RegisterProgressTrack(
			DiscipleCultivationAssignmentType.SkillTraining,
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/SkillTrackProgress",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/SkillTrackTag/SkillTrackTagLabel",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/SkillTrackDiamonds");
		RegisterProgressTrack(
			DiscipleCultivationAssignmentType.TechniquePolish,
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TechniqueTrackProgress",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TechniqueTrackTag/TechniqueTrackTagLabel",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TechniqueTrackDiamonds");
		RegisterProgressTrack(
			DiscipleCultivationAssignmentType.CraftPractice,
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/CraftTrackProgress",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/CraftTrackTag/CraftTrackTagLabel",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/CraftTrackDiamonds");
		RegisterProgressTrack(
			DiscipleCultivationAssignmentType.Meditation,
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/MeditationTrackProgress",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/MeditationTrackTag/MeditationTrackTagLabel",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/MeditationTrackDiamonds");
	}

	private void RegisterProgressTrack(
		DiscipleCultivationAssignmentType assignmentType,
		string progressBarPath,
		string tagLabelPath,
		string diamondContainerPath)
	{
		// 菱形刻度用于替代进度条展示火候段位。
		var diamondContainer = GetNode<Control>(diamondContainerPath);
		var diamonds = new List<PanelContainer>();
		foreach (var child in diamondContainer.GetChildren())
		{
			if (child is PanelContainer panel)
			{
				diamonds.Add(panel);
			}
		}
		_progressTrackWidgets[assignmentType] = new ProgressTrackWidget(
			assignmentType,
			GetNode<ProgressBar>(progressBarPath),
			GetNode<Label>(tagLabelPath),
			diamondContainer,
			diamonds);
	}

	private void BuildActionCards()
	{
		_actionCards.Clear();
		RegisterAction(
			DiscipleCultivationAssignmentType.SkillTraining,
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/StatusLabel",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/PrimaryEffect",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/SecondaryEffect",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/IconBadge",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel",
			"技能修炼");
		RegisterAction(
			DiscipleCultivationAssignmentType.TechniquePolish,
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/StatusLabel",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/PrimaryEffect",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/SecondaryEffect",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/IconBadge",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel",
			"功法打磨");
		RegisterAction(
			DiscipleCultivationAssignmentType.CraftPractice,
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/StatusLabel",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/PrimaryEffect",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/SecondaryEffect",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/IconBadge",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel",
			"技艺练习");
		RegisterAction(
			DiscipleCultivationAssignmentType.Meditation,
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/StatusLabel",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/PrimaryEffect",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/SecondaryEffect",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/IconBadge",
			"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel",
			"打坐修炼");

		foreach (var card in _actionCards.Values)
		{
			card.Button.Pressed += () => ToggleAssignment(card.AssignmentType);
		}
	}

	private void RegisterAction(
		DiscipleCultivationAssignmentType assignmentType,
		string buttonPath,
		string statusLabelPath,
		string sealBadgePath,
		string primaryEffectPath,
		string secondaryEffectPath,
		string iconBadgePath,
		string iconLabelPath,
		string actionLabel)
	{
		var button = GetNode<Button>(buttonPath);
		button.ToggleMode = true;
		_actionCards[assignmentType] = new ActionCard(
			assignmentType,
			button,
			GetNode<Label>(statusLabelPath),
			GetNode<Control>(sealBadgePath),
			GetNode<PanelContainer>(primaryEffectPath),
			GetNode<PanelContainer>(secondaryEffectPath),
			GetNode<PanelContainer>(iconBadgePath),
			GetNode<Label>(iconLabelPath),
			actionLabel);
	}

	private void RefreshSummary()
	{
		_populationValueLabel.Text = $"{_state.Population:0} 人";
		_techValueLabel.Text = FormatTechLevelDisplay(Math.Max(_state.TechLevel + 1, 1));
		_resourceValueLabel.Text = $"灵石 {_state.Gold:0}\n贡献 {_state.ContributionPoints:0}";
		_registerValueLabel.Text = BuildCompactAssignmentSummary(_state);
	}

	/// <summary>
	/// 重建左侧名册按钮，保证从弟子谱跳转或状态刷新后仍能逐人点名。
	/// </summary>
	private void RebuildRosterButtons()
	{
		foreach (var entry in _rosterButtons.Values)
		{
			if (IsInstanceValid(entry.Button))
			{
				entry.Button.QueueFree();
			}
		}

		_rosterButtons.Clear();
		_discipleRosterButtonTemplate.Visible = false;

		foreach (var profile in _profiles)
		{
			var discipleId = profile.Id;
			var rosterButton = (Button)_discipleRosterButtonTemplate.Duplicate();
			rosterButton.Visible = true;
			rosterButton.Name = $"DiscipleRosterButton{discipleId}";
			rosterButton.ButtonPressed = false;
			rosterButton.Pressed += () => SelectDisciple(discipleId);
			_discipleRosterList.AddChild(rosterButton);
			_discipleRosterList.MoveChild(rosterButton, _discipleRosterList.GetChildCount() - 1);
			_rosterButtons[discipleId] = new RosterEntryWidget(
				rosterButton,
				rosterButton.GetNode<ColorRect>("ButtonMargin/ButtonRow/SelectionMarker"),
				rosterButton.GetNode<Label>("ButtonMargin/ButtonRow/GlyphBadge/GlyphLabel"),
				rosterButton.GetNode<Label>("ButtonMargin/ButtonRow/BodyColumn/NameLabel"),
				rosterButton.GetNode<Label>("ButtonMargin/ButtonRow/BodyColumn/MetaLabel"),
				rosterButton.GetNode<PanelContainer>("ButtonMargin/ButtonRow/TagColumn/AssignmentTag"),
				rosterButton.GetNode<Label>("ButtonMargin/ButtonRow/TagColumn/AssignmentTag/AssignmentTagLabel"),
				rosterButton.GetNode<PanelContainer>("ButtonMargin/ButtonRow/TagColumn/BranchTag"),
				rosterButton.GetNode<Label>("ButtonMargin/ButtonRow/TagColumn/BranchTag/BranchTagLabel"));
		}
	}

	/// <summary>
	/// 刷新名册文案与选中态，让左侧上半层成为快速切人入口。
	/// </summary>
	private void RefreshRosterButtons()
	{
		if (_profiles.Count <= 0)
		{
			_rosterTitleLabel.Text = "门下点名册";
			_rosterHintLabel.Text = "当前暂无可登记弟子。";
			return;
		}

		_rosterTitleLabel.Text = $"门下点名册 · {_profiles.Count:0} 人";
		_rosterHintLabel.Text = "点名任一弟子，即在下方展开阵眼详情与当前火候。";

		foreach (var profile in _profiles)
		{
			if (!_rosterButtons.TryGetValue(profile.Id, out var rosterEntry))
			{
				continue;
			}

			var assignment = DiscipleCultivationRules.GetAssignment(_state, profile.Id);
			var branchSummary = DiscipleCultivationRules.BuildSpecializationBranchSummary(_state, profile.Id);
			var specializationSummary = DiscipleCultivationRules.BuildSpecializationSummary(_state, profile.Id);

			rosterEntry.GlyphLabel.Text = ResolveCoreGlyph(profile.Name);
			rosterEntry.NameLabel.Text = profile.Name;
			rosterEntry.MetaLabel.Text = BuildRosterMetaText(profile);
			rosterEntry.AssignmentTagLabel.Text = BuildRosterAssignmentTagText(assignment);
			var branchTagText = BuildRosterBranchTagText(branchSummary, specializationSummary);
			rosterEntry.BranchTagLabel.Text = branchTagText;
			rosterEntry.Button.TooltipText =
				$"{profile.Name} · {profile.RealmName}\n" +
				$"当前安排：{DiscipleCultivationRules.GetAssignmentDisplayName(assignment)}\n" +
				$"培养路数：{specializationSummary}\n" +
				$"专修分支：{branchSummary}\n" +
				$"当前火候：{DiscipleCultivationRules.BuildActiveTrackProgressSummary(_state, profile.Id)}";
			var isSelected = profile.Id == _selectedDiscipleId;
			rosterEntry.Button.ButtonPressed = isSelected;
			ApplyRosterEntryVisualState(rosterEntry, isSelected, assignment != DiscipleCultivationAssignmentType.None, branchTagText != "未成");
		}
	}

	private void RefreshSelectedDisciple()
	{
		var profile = GetSelectedProfile();
		if (profile == null)
		{
			_selectedDiscipleNameLabel.Text = "暂无可登记弟子";
			_selectedDiscipleMetaLabel.Text = "请等待名册生成，或从弟子谱中点名一位弟子。";
			_selectedDiscipleRealmLabel.Text = "待点名";
			_selectedDiscipleAgeLabel.Text = "骨龄 --";
			_selectedDiscipleResidenceLabel.Text = "居所未知";
			_selectedDiscipleDutyLabel.Text = "◈ 差事：待命";
			_coreLetterLabel.Text = "修";
			_footerIconLabel.Text = "修";
			_footerTitleLabel.Text = "当前运转周天";
			_selectedDiscipleStatusLabel.Text = "待命修炼";
			_selectedDiscipleStatusHighlightLabel.Text = "未运转";
			_selectedDiscipleStatusHighlightLabel.Visible = true;
			_selectedDiscipleInsightLabel.Visible = false;
			_previousDiscipleButton.Disabled = true;
			_nextDiscipleButton.Disabled = true;
			return;
		}

		var assignment = DiscipleCultivationRules.GetAssignment(_state, profile.Id);
		var assignmentText = DiscipleCultivationRules.GetAssignmentDisplayName(assignment);
		var latestInsight = DiscipleCultivationRules.BuildLatestInsightSummary(_state, profile.Id);

		_selectedDiscipleNameLabel.Text = profile.Name;
		_selectedDiscipleMetaLabel.Text =
			$"{profile.RankName} · {profile.AgeText}\n" +
			$"当前差事：{profile.CurrentAssignment} · 居所：{profile.ResidenceName}";
		_selectedDiscipleRealmLabel.Text = profile.RealmName;
		_selectedDiscipleAgeLabel.Text = $"骨龄 {profile.Age}";
		_selectedDiscipleResidenceLabel.Text = profile.ResidenceName;
		_selectedDiscipleDutyLabel.Text = $"◈ 差事：{profile.CurrentAssignment}";
		_coreLetterLabel.Text = ResolveCoreGlyph(profile.Name);
		_footerIconLabel.Text = ResolveAssignmentGlyph(assignment);
		// 状态槽仅保留短文案，避免长段说明挤压卷面。
		_footerTitleLabel.Text = "当前运转周天";
		if (assignment == DiscipleCultivationAssignmentType.None)
		{
			_selectedDiscipleStatusLabel.Text = "待命修炼";
			_selectedDiscipleStatusHighlightLabel.Text = "待命";
			_selectedDiscipleStatusHighlightLabel.Visible = true;
		}
		else
		{
			_selectedDiscipleStatusLabel.Text = assignmentText;
			_selectedDiscipleStatusHighlightLabel.Text = "运转中";
			_selectedDiscipleStatusHighlightLabel.Visible = true;
		}
		_selectedDiscipleInsightLabel.Text = $"最近感悟：{latestInsight}";
		_selectedDiscipleInsightLabel.Visible = false;

		var canStep = _profiles.Count > 1;
		_previousDiscipleButton.Disabled = !canStep;
		_nextDiscipleButton.Disabled = !canStep;
	}

	private void RefreshProgressTracks()
	{
		var profile = GetSelectedProfile();
		var currentAssignment = profile == null
			? DiscipleCultivationAssignmentType.None
			: DiscipleCultivationRules.GetAssignment(_state, profile.Id);

		foreach (var widget in _progressTrackWidgets.Values)
		{
			if (profile == null)
			{
				widget.ProgressBar.Value = 0;
				widget.ProgressBar.TooltipText = "当前暂无修炼积累。";
				widget.TagLabel.Text = "未起";
				UpdateDiamondTrack(widget, 0f, "当前暂无修炼积累。");
				continue;
			}

			var progressRatio = DiscipleCultivationRules.GetTrackProgressRatio(_state, profile.Id, widget.AssignmentType);
			widget.ProgressBar.Value = progressRatio * 100.0;
			var progressSummary = DiscipleCultivationRules.BuildTrackProgressSummary(_state, profile.Id, widget.AssignmentType);
			widget.ProgressBar.TooltipText = progressSummary;
			widget.TagLabel.Text = ResolveTrackTagText(_state, profile.Id, widget.AssignmentType, currentAssignment == widget.AssignmentType);
			UpdateDiamondTrack(widget, (float)progressRatio, progressSummary);
		}
	}

	/// <summary>
	/// 用菱形刻度同步火候进度，避免长条进度条破坏仪式感。
	/// </summary>
	private static void UpdateDiamondTrack(ProgressTrackWidget widget, float progressRatio, string progressSummary)
	{
		widget.DiamondContainer.TooltipText = progressSummary;
		if (widget.Diamonds.Count <= 0)
		{
			return;
		}

		var filledCount = Math.Clamp((int)Math.Ceiling(progressRatio * widget.Diamonds.Count), 0, widget.Diamonds.Count);
		for (var i = 0; i < widget.Diamonds.Count; i++)
		{
			widget.Diamonds[i].AddThemeStyleboxOverride("panel", i < filledCount ? DiamondFilledStyle : DiamondEmptyStyle);
		}
	}

	private void RefreshActionCards()
	{
		var profile = GetSelectedProfile();
		var currentAssignment = profile == null
			? DiscipleCultivationAssignmentType.None
			: DiscipleCultivationRules.GetAssignment(_state, profile.Id);

		foreach (var card in _actionCards.Values)
		{
			var isActive = profile != null && currentAssignment == card.AssignmentType;
			card.Button.Disabled = profile == null;
			card.Button.ButtonPressed = isActive;
			card.SealBadge.Visible = isActive;
			ApplyActionEffectStyle(card.PrimaryEffect, isActive);
			ApplyActionEffectStyle(card.SecondaryEffect, isActive);
			ApplyActionIconStyle(card.IconBadge, card.IconLabel, isActive);

			if (profile == null)
			{
				card.StatusLabel.Text = "当前无可点名弟子，待名册成形后方可敕令。";
				continue;
			}

			var progressSummary = DiscipleCultivationRules.BuildTrackProgressSummary(_state, profile.Id, card.AssignmentType);
			if (isActive)
			{
				card.StatusLabel.Text = $"已敕令“{profile.Name}”主修{card.ActionLabel}。\n当前火候：{progressSummary}";
				continue;
			}

			card.StatusLabel.Text = currentAssignment == DiscipleCultivationAssignmentType.None
				? $"尚未敕令主修，启用后：{DiscipleCultivationRules.GetAssignmentShortEffect(card.AssignmentType)}\n此脉火候：{progressSummary}"
				: $"当前主修为「{DiscipleCultivationRules.GetAssignmentDisplayName(currentAssignment)}」，改敕后将转入此门。\n此脉火候：{progressSummary}";
		}
	}

	private void StepSelectedDisciple(int delta)
	{
		if (_profiles.Count <= 0)
		{
			return;
		}

		var currentIndex = _profiles.FindIndex(profile => profile.Id == _selectedDiscipleId);
		if (currentIndex < 0)
		{
			currentIndex = 0;
		}

		var nextIndex = (currentIndex + delta) % _profiles.Count;
		if (nextIndex < 0)
		{
			nextIndex += _profiles.Count;
		}

		_selectedDiscipleId = _profiles[nextIndex].Id;
		RefreshRosterButtons();
		RefreshSelectedDisciple();
		RefreshProgressTracks();
		RefreshActionCards();
		RefreshPopupHint();
		EnsureSelectedRosterEntryVisible();
		CallVisualFx("pulse_soul_core");
	}

	private void ToggleAssignment(DiscipleCultivationAssignmentType assignmentType)
	{
		var profile = GetSelectedProfile();
		if (profile == null)
		{
			ShowPopupStatusMessage("当前未选中弟子，无法登记修炼安排。");
			return;
		}

		var currentAssignment = DiscipleCultivationRules.GetAssignment(_state, profile.Id);
		var nextAssignment = currentAssignment == assignmentType
			? DiscipleCultivationAssignmentType.None
			: assignmentType;

		DiscipleCultivationRules.SetAssignment(_state, profile.Id, nextAssignment);
		RefreshSummary();
		RefreshRosterButtons();
		RefreshSelectedDisciple();
		RefreshProgressTracks();
		RefreshActionCards();
		RefreshPopupHint();

		AssignmentRequested?.Invoke(profile.Id, nextAssignment);
		ShowPopupStatusMessage(nextAssignment == DiscipleCultivationAssignmentType.None
			? $"已撤去“{profile.Name}”的修炼敕令。"
			: $"已敕令“{profile.Name}”主修「{DiscipleCultivationRules.GetAssignmentDisplayName(nextAssignment)}」。");
	}

	private DiscipleProfile? GetSelectedProfile()
	{
		return _profiles.FirstOrDefault(profile => profile.Id == _selectedDiscipleId);
	}

	/// <summary>
	/// 名册点名会直接切换左侧下半层的阵眼详情。
	/// </summary>
	private void SelectDisciple(int discipleId)
	{
		if (_profiles.All(profile => profile.Id != discipleId))
		{
			return;
		}

		_selectedDiscipleId = discipleId;
		RefreshRosterButtons();
		RefreshSelectedDisciple();
		RefreshProgressTracks();
		RefreshActionCards();
		RefreshPopupHint();
		EnsureSelectedRosterEntryVisible();
		CallVisualFx("pulse_soul_core");
	}

	private void EnsureSelectedRosterEntryVisible()
	{
		if (_profiles.Count <= 0)
		{
			return;
		}

		if (!_rosterButtons.TryGetValue(_selectedDiscipleId, out var rosterEntry))
		{
			return;
		}

		_rosterScroll.CallDeferred("ensure_control_visible", rosterEntry.Button);
	}

	private static void ApplyRosterEntryVisualState(
		RosterEntryWidget rosterEntry,
		bool isSelected,
		bool hasAssignment,
		bool hasBranchTag)
	{
		rosterEntry.SelectionMarker.SelfModulate = new Color(1f, 1f, 1f, isSelected ? 1f : 0f);
		rosterEntry.AssignmentTag.Modulate = new Color(1f, 1f, 1f, hasAssignment ? 1f : RosterMutedAlpha);
		rosterEntry.BranchTag.Modulate = new Color(1f, 1f, 1f, hasBranchTag ? 1f : RosterMutedAlpha);
	}

	private static void ApplyActionEffectStyle(PanelContainer effectPanel, bool isActive)
	{
		effectPanel.AddThemeStyleboxOverride("panel", BuildActionEffectStyle(isActive));
	}

	private static void ApplyActionIconStyle(PanelContainer iconBadge, Label iconLabel, bool isActive)
	{
		iconBadge.AddThemeStyleboxOverride("panel", BuildActionIconStyle(isActive));
		iconLabel.AddThemeColorOverride("font_color", isActive ? new Color(0.07f, 0.08f, 0.07f) : new Color(0.42f, 0.38f, 0.27f));
	}

	private static StyleBoxFlat BuildActionEffectStyle(bool isActive)
	{
		var style = new StyleBoxFlat();
		style.CornerRadiusTopLeft = 4;
		style.CornerRadiusTopRight = 4;
		style.CornerRadiusBottomRight = 4;
		style.CornerRadiusBottomLeft = 4;

		if (isActive)
		{
			style.BgColor = new Color(0.894f, 0.753f, 0.310f, 0.03f);
			style.BorderWidthLeft = 1;
			style.BorderWidthTop = 1;
			style.BorderWidthRight = 1;
			style.BorderWidthBottom = 1;
			style.BorderColor = new Color(0.894f, 0.753f, 0.310f, 0.3f);
			return style;
		}

		style.BgColor = new Color(0f, 0f, 0f, 0.3f);
		return style;
	}

	private static StyleBoxFlat BuildActionIconStyle(bool isActive)
	{
		var style = new StyleBoxFlat();
		style.CornerRadiusTopLeft = 4;
		style.CornerRadiusTopRight = 4;
		style.CornerRadiusBottomRight = 4;
		style.CornerRadiusBottomLeft = 4;

		if (isActive)
		{
			style.BgColor = new Color(0.894f, 0.753f, 0.310f, 1f);
			style.ShadowColor = new Color(0.894f, 0.753f, 0.310f, 0.3f);
			style.ShadowSize = 8;
			return style;
		}

		style.BgColor = new Color(0.894f, 0.753f, 0.310f, 0.03f);
		style.BorderWidthLeft = 1;
		style.BorderWidthTop = 1;
		style.BorderWidthRight = 1;
		style.BorderWidthBottom = 1;
		style.BorderColor = new Color(0.42f, 0.38f, 0.27f, 0.9f);
		return style;
	}

	/// <summary>
	/// 构建菱形刻度的样式，填充态会附加轻微光晕。
	/// </summary>
	private static StyleBoxFlat BuildDiamondStyle(Color fillColor, Color borderColor, bool withGlow)
	{
		var style = new StyleBoxFlat();
		style.BgColor = fillColor;
		style.BorderWidthLeft = 1;
		style.BorderWidthTop = 1;
		style.BorderWidthRight = 1;
		style.BorderWidthBottom = 1;
		style.BorderColor = borderColor;
		style.CornerRadiusTopLeft = 2;
		style.CornerRadiusTopRight = 2;
		style.CornerRadiusBottomRight = 2;
		style.CornerRadiusBottomLeft = 2;
		if (withGlow)
		{
			style.ShadowColor = new Color(0.894f, 0.753f, 0.310f, 0.6f);
			style.ShadowSize = 6;
		}
		return style;
	}

	private void CallVisualFx(string methodName, params Variant[] args)
	{
		_visualFx?.Call(methodName, args);
	}

	/// <summary>
	/// 顶栏将登记数压缩成两行短记，避免全屏 HUD 被说明文案挤压。
	/// </summary>
	private static string BuildCompactAssignmentSummary(GameState state)
	{
		return
			$"技 {DiscipleCultivationRules.GetAssignmentCount(state, DiscipleCultivationAssignmentType.SkillTraining)} · " +
			$"法 {DiscipleCultivationRules.GetAssignmentCount(state, DiscipleCultivationAssignmentType.TechniquePolish)}\n" +
			$"艺 {DiscipleCultivationRules.GetAssignmentCount(state, DiscipleCultivationAssignmentType.CraftPractice)} · " +
			$"坐 {DiscipleCultivationRules.GetAssignmentCount(state, DiscipleCultivationAssignmentType.Meditation)}";
	}

	/// <summary>
	/// 点名册第二行保留境界与当前职司，便于玩家快速筛人。
	/// </summary>
	private static string BuildRosterMetaText(DiscipleProfile profile)
	{
		return $"{profile.RealmName} · {profile.DutyDisplayName}";
	}

	/// <summary>
	/// 将完整修炼安排名压缩为短签口径，方便名册卡牌右侧显示。
	/// </summary>
	private static string BuildRosterAssignmentTagText(DiscipleCultivationAssignmentType assignmentType)
	{
		return assignmentType switch
		{
			DiscipleCultivationAssignmentType.SkillTraining => "主修 技",
			DiscipleCultivationAssignmentType.TechniquePolish => "主修 法",
			DiscipleCultivationAssignmentType.CraftPractice => "主修 艺",
			DiscipleCultivationAssignmentType.Meditation => "主修 坐",
			_ => "常制"
		};
	}

	/// <summary>
	/// 将较长的路数/分支摘要压缩成点名册可扫视的短签。
	/// </summary>
	private static string BuildRosterBranchTagText(string branchSummary, string specializationSummary)
	{
		if (!string.IsNullOrWhiteSpace(branchSummary) && branchSummary != "分支未定")
		{
			var compactBranch = branchSummary.Split('（')[0].Trim();
			return compactBranch.Length > 4 ? compactBranch[..4] : compactBranch;
		}

		if (string.IsNullOrWhiteSpace(specializationSummary) || specializationSummary == "路数未成")
		{
			return "未成";
		}

		var compactRoute = specializationSummary.Split('·')[0].Trim();
		return compactRoute.Length > 4 ? compactRoute[..4] : compactRoute;
	}

	/// <summary>
	/// 将研修层级转换成更像修仙卷册的显示口径。
	/// </summary>
	private static string FormatTechLevelDisplay(int tier)
	{
		var safeTier = Math.Clamp(tier, 1, ChineseTierDigits.Length - 1);
		return $"{ChineseTierDigits[safeTier]}层  T{safeTier}";
	}

	/// <summary>
	/// 以姓名首字为阵眼主字，强化“弟子位于阵心”的视觉锚点。
	/// </summary>
	private static string ResolveCoreGlyph(string name)
	{
		return string.IsNullOrWhiteSpace(name)
			? "修"
			: name.Trim()[0].ToString();
	}

	/// <summary>
	/// 根据主修类型返回状态槽的字章，用于视觉聚焦。
	/// </summary>
	private static string ResolveAssignmentGlyph(DiscipleCultivationAssignmentType assignmentType)
	{
		return assignmentType switch
		{
			DiscipleCultivationAssignmentType.SkillTraining => "技",
			DiscipleCultivationAssignmentType.TechniquePolish => "法",
			DiscipleCultivationAssignmentType.CraftPractice => "艺",
			DiscipleCultivationAssignmentType.Meditation => "坐",
			_ => "修"
		};
	}

	/// <summary>
	/// 进度条右侧的小签用于快速判断火候是否已经起势，以及是否处于当前主修。
	/// </summary>
	private static string ResolveTrackTagText(
		GameState state,
		int discipleId,
		DiscipleCultivationAssignmentType assignmentType,
		bool isActive)
	{
		var stageLabel = DiscipleCultivationRules.GetTrackStageLabel(state, discipleId, assignmentType);
		if (string.IsNullOrWhiteSpace(stageLabel))
		{
			return isActive ? "主修" : "未起";
		}

		return isActive ? $"{stageLabel} · 主修" : stageLabel;
	}
}
