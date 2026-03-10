using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class TaskPanel : PopupPanelBase
{
    private enum GovernanceTab
    {
        Policy,
        Season,
        Rules,
        Affairs
    }

    private static readonly Color PaperMainColor = new(0.95f, 0.92f, 0.84f, 1f);
    private static readonly Color PaperDarkColor = new(0.89f, 0.85f, 0.76f, 1f);
    private static readonly Color InkMainColor = new(0.17f, 0.15f, 0.13f, 1f);
    private static readonly Color InkMutedColor = new(0.42f, 0.37f, 0.33f, 1f);
    private static readonly Color SealRedColor = new(0.64f, 0.19f, 0.14f, 1f);
    private static readonly Color BorderInkColor = new(0.29f, 0.25f, 0.21f, 1f);
    private static readonly Color SidebarActiveColor = new(0.63f, 0.19f, 0.14f, 1f);

    private const string RootPath = "Overlay/Center/Frame/RootColumn";
    private const string HeaderPath = RootPath + "/HeaderPanel/HeaderMargin/HeaderRow";
    private const string BodyPath = RootPath + "/BodyRow";
    private const string ContentStackPath = BodyPath + "/ContentMargin/ContentScroll/ContentStack";
    private const string PolicyTabPath = ContentStackPath + "/PolicyTab";
    private const string SeasonTabPath = ContentStackPath + "/SeasonTab";
    private const string RulesTabPath = ContentStackPath + "/RulesTab";
    private const string AffairsTabPath = ContentStackPath + "/AffairsTab";

    private readonly GameCalendarSystem _calendarSystem = new();
    private readonly Dictionary<GovernanceTab, Button> _tabButtons = new();

    private Label _spiritStoneValueLabel = null!;
    private Label _contributionValueLabel = null!;
    private Label _policySummaryLabel = null!;
    private Label _seasonSummaryLabel = null!;
    private Label _rulesSummaryLabel = null!;
    private Label _affairsSummaryLabel = null!;
    private Label _developmentValueLabel = null!;
    private Label _developmentHintLabel = null!;
    private Label _lawValueLabel = null!;
    private Label _lawHintLabel = null!;
    private Label _talentValueLabel = null!;
    private Label _talentHintLabel = null!;
    private Label _quarterDecreeValueLabel = null!;
    private Label _quarterDecreeHintLabel = null!;
    private Label _affairsRuleValueLabel = null!;
    private Label _affairsRuleHintLabel = null!;
    private Label _doctrineRuleValueLabel = null!;
    private Label _doctrineRuleHintLabel = null!;
    private Label _disciplineRuleValueLabel = null!;
    private Label _disciplineRuleHintLabel = null!;
    private ItemList _taskList = null!;
    private Label _detailLabel = null!;
    private Button _minusOneButton = null!;
    private Button _plusOneButton = null!;
    private Button _plusFiveButton = null!;
    private Button _resetButton = null!;
    private Button _closeButton = null!;
    private Label _hintLabel = null!;
    private ScrollContainer _contentScroll = null!;
    private Control _policyTabContent = null!;
    private Control _seasonTabContent = null!;
    private Control _rulesTabContent = null!;
    private Control _affairsTabContent = null!;
    private Button _policyTabButton = null!;
    private Button _seasonTabButton = null!;
    private Button _rulesTabButton = null!;
    private Button _affairsTabButton = null!;
    private Button _developmentPrevButton = null!;
    private Button _developmentNextButton = null!;
    private Button _lawPrevButton = null!;
    private Button _lawNextButton = null!;
    private Button _talentPrevButton = null!;
    private Button _talentNextButton = null!;
    private Button _quarterDecreePrevButton = null!;
    private Button _quarterDecreeNextButton = null!;
    private Button _affairsRulePrevButton = null!;
    private Button _affairsRuleNextButton = null!;
    private Button _doctrineRulePrevButton = null!;
    private Button _doctrineRuleNextButton = null!;
    private Button _disciplineRulePrevButton = null!;
    private Button _disciplineRuleNextButton = null!;
    private HSplitContainer _affairsSplit = null!;

    private GameState _state = new();
    private GovernanceTab _activeTab = GovernanceTab.Policy;
    private SectTaskType _selectedTaskType = SectTaskType.FieldDuty;

    public event Action<SectTaskType, int>? OrderAdjustmentRequested;
    public event Action<int>? DevelopmentDirectionShiftRequested;
    public event Action<int>? SectLawShiftRequested;
    public event Action<int>? TalentPlanShiftRequested;
    public event Action<int>? QuarterDecreeShiftRequested;
    public event Action<int>? AffairsRuleShiftRequested;
    public event Action<int>? DoctrineRuleShiftRequested;
    public event Action<int>? DisciplineRuleShiftRequested;
    public event Action? ResetRequested;

    public override void _Ready()
    {
        BindUiNodes();
        ApplyUiStyles();
        BindEvents();
        SwitchTab(GovernanceTab.Policy);
        InitializePopupHint(_hintLabel);
        Hide();
    }

    private void BindUiNodes()
    {
        _spiritStoneValueLabel = GetNode<Label>($"{HeaderPath}/StatRow/SpiritStoneRow/SpiritStoneValueLabel");
        _contributionValueLabel = GetNode<Label>($"{HeaderPath}/StatRow/ContributionRow/ContributionValueLabel");
        _closeButton = GetNode<Button>($"{HeaderPath}/CloseButton");
        _hintLabel = GetNode<Label>($"{RootPath}/HintLabel");
        _contentScroll = GetNode<ScrollContainer>($"{BodyPath}/ContentMargin/ContentScroll");
        _policyTabContent = GetNode<Control>(PolicyTabPath);
        _seasonTabContent = GetNode<Control>(SeasonTabPath);
        _rulesTabContent = GetNode<Control>(RulesTabPath);
        _affairsTabContent = GetNode<Control>(AffairsTabPath);
        _policyTabButton = GetNode<Button>($"{BodyPath}/SidebarPanel/SidebarColumn/PolicyTabButton");
        _seasonTabButton = GetNode<Button>($"{BodyPath}/SidebarPanel/SidebarColumn/SeasonTabButton");
        _rulesTabButton = GetNode<Button>($"{BodyPath}/SidebarPanel/SidebarColumn/RulesTabButton");
        _affairsTabButton = GetNode<Button>($"{BodyPath}/SidebarPanel/SidebarColumn/AffairsTabButton");
        _policySummaryLabel = GetNode<Label>(
            $"{PolicyTabPath}/PolicySummaryPanel/PolicySummaryMargin/PolicySummaryColumn/PolicySummaryLabel");
        _seasonSummaryLabel = GetNode<Label>(
            $"{SeasonTabPath}/SeasonSummaryPanel/SeasonSummaryMargin/SeasonSummaryColumn/SeasonSummaryLabel");
        _rulesSummaryLabel = GetNode<Label>(
            $"{RulesTabPath}/RulesSummaryPanel/RulesSummaryMargin/RulesSummaryColumn/RulesSummaryLabel");
        _affairsSummaryLabel = GetNode<Label>(
            $"{AffairsTabPath}/AffairsSummaryPanel/AffairsSummaryMargin/AffairsSummaryColumn/AffairsSummaryLabel");
        _developmentValueLabel = GetNode<Label>(
            $"{PolicyTabPath}/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentCapsule/DevelopmentCapsuleRow/DevelopmentValueLabel");
        _developmentHintLabel = GetNode<Label>(
            $"{PolicyTabPath}/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentInfoColumn/DevelopmentHintLabel");
        _developmentPrevButton = GetNode<Button>(
            $"{PolicyTabPath}/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentCapsule/DevelopmentCapsuleRow/DevelopmentPrevButton");
        _developmentNextButton = GetNode<Button>(
            $"{PolicyTabPath}/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentCapsule/DevelopmentCapsuleRow/DevelopmentNextButton");
        _lawValueLabel = GetNode<Label>(
            $"{PolicyTabPath}/LawCard/LawCardMargin/LawCardRow/LawCapsule/LawCapsuleRow/LawValueLabel");
        _lawHintLabel = GetNode<Label>(
            $"{PolicyTabPath}/LawCard/LawCardMargin/LawCardRow/LawInfoColumn/LawHintLabel");
        _lawPrevButton = GetNode<Button>(
            $"{PolicyTabPath}/LawCard/LawCardMargin/LawCardRow/LawCapsule/LawCapsuleRow/LawPrevButton");
        _lawNextButton = GetNode<Button>(
            $"{PolicyTabPath}/LawCard/LawCardMargin/LawCardRow/LawCapsule/LawCapsuleRow/LawNextButton");
        _talentValueLabel = GetNode<Label>(
            $"{PolicyTabPath}/TalentCard/TalentCardMargin/TalentCardRow/TalentCapsule/TalentCapsuleRow/TalentValueLabel");
        _talentHintLabel = GetNode<Label>(
            $"{PolicyTabPath}/TalentCard/TalentCardMargin/TalentCardRow/TalentInfoColumn/TalentHintLabel");
        _talentPrevButton = GetNode<Button>(
            $"{PolicyTabPath}/TalentCard/TalentCardMargin/TalentCardRow/TalentCapsule/TalentCapsuleRow/TalentPrevButton");
        _talentNextButton = GetNode<Button>(
            $"{PolicyTabPath}/TalentCard/TalentCardMargin/TalentCardRow/TalentCapsule/TalentCapsuleRow/TalentNextButton");
        _quarterDecreeValueLabel = GetNode<Label>(
            $"{SeasonTabPath}/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeCapsule/QuarterDecreeCapsuleRow/QuarterDecreeValueLabel");
        _quarterDecreeHintLabel = GetNode<Label>(
            $"{SeasonTabPath}/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeInfoColumn/QuarterDecreeHintLabel");
        _quarterDecreePrevButton = GetNode<Button>(
            $"{SeasonTabPath}/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeCapsule/QuarterDecreeCapsuleRow/QuarterDecreePrevButton");
        _quarterDecreeNextButton = GetNode<Button>(
            $"{SeasonTabPath}/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeCapsule/QuarterDecreeCapsuleRow/QuarterDecreeNextButton");
        _affairsRuleValueLabel = GetNode<Label>(
            $"{RulesTabPath}/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleCapsule/AffairsRuleCapsuleRow/AffairsRuleValueLabel");
        _affairsRuleHintLabel = GetNode<Label>(
            $"{RulesTabPath}/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleInfoColumn/AffairsRuleHintLabel");
        _affairsRulePrevButton = GetNode<Button>(
            $"{RulesTabPath}/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleCapsule/AffairsRuleCapsuleRow/AffairsRulePrevButton");
        _affairsRuleNextButton = GetNode<Button>(
            $"{RulesTabPath}/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleCapsule/AffairsRuleCapsuleRow/AffairsRuleNextButton");
        _doctrineRuleValueLabel = GetNode<Label>(
            $"{RulesTabPath}/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleCapsule/DoctrineRuleCapsuleRow/DoctrineRuleValueLabel");
        _doctrineRuleHintLabel = GetNode<Label>(
            $"{RulesTabPath}/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleInfoColumn/DoctrineRuleHintLabel");
        _doctrineRulePrevButton = GetNode<Button>(
            $"{RulesTabPath}/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleCapsule/DoctrineRuleCapsuleRow/DoctrineRulePrevButton");
        _doctrineRuleNextButton = GetNode<Button>(
            $"{RulesTabPath}/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleCapsule/DoctrineRuleCapsuleRow/DoctrineRuleNextButton");
        _disciplineRuleValueLabel = GetNode<Label>(
            $"{RulesTabPath}/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleCapsule/DisciplineRuleCapsuleRow/DisciplineRuleValueLabel");
        _disciplineRuleHintLabel = GetNode<Label>(
            $"{RulesTabPath}/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleInfoColumn/DisciplineRuleHintLabel");
        _disciplineRulePrevButton = GetNode<Button>(
            $"{RulesTabPath}/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleCapsule/DisciplineRuleCapsuleRow/DisciplineRulePrevButton");
        _disciplineRuleNextButton = GetNode<Button>(
            $"{RulesTabPath}/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleCapsule/DisciplineRuleCapsuleRow/DisciplineRuleNextButton");
        _affairsSplit = GetNode<HSplitContainer>(
            $"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit");
        _taskList = GetNode<ItemList>(
            $"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsListPanel/AffairsListPanelMargin/TaskList");
        _detailLabel = GetNode<Label>(
            $"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsDetailPanel/AffairsDetailMargin/DetailLabel");
        _minusOneButton = GetNode<Button>(
            $"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsActionRow/MinusOneButton");
        _plusOneButton = GetNode<Button>(
            $"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsActionRow/PlusOneButton");
        _plusFiveButton = GetNode<Button>(
            $"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsActionRow/PlusFiveButton");
        _resetButton = GetNode<Button>(
            $"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsActionRow/ResetButton");

        _tabButtons.Clear();
        _tabButtons[GovernanceTab.Policy] = _policyTabButton;
        _tabButtons[GovernanceTab.Season] = _seasonTabButton;
        _tabButtons[GovernanceTab.Rules] = _rulesTabButton;
        _tabButtons[GovernanceTab.Affairs] = _affairsTabButton;
    }

    private void ApplyUiStyles()
    {
        ApplyPanelStyle(GetNode<PanelContainer>("Overlay/Center/Frame"), CreateFrameStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{RootPath}/HeaderPanel"), CreateHeaderStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{BodyPath}/SidebarPanel"), CreateSidebarPanelStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{PolicyTabPath}/PolicySummaryPanel"), CreateSummaryStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{SeasonTabPath}/SeasonSummaryPanel"), CreateSummaryStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{RulesTabPath}/RulesSummaryPanel"), CreateSummaryStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{AffairsTabPath}/AffairsSummaryPanel"), CreateSummaryStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{PolicyTabPath}/DevelopmentCard"), CreateCardStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{PolicyTabPath}/LawCard"), CreateCardStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{PolicyTabPath}/TalentCard"), CreateCardStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{PolicyTabPath}/LockedCard"), CreateCardStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{SeasonTabPath}/QuarterDecreeCard"), CreateCardStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{RulesTabPath}/AffairsRuleCard"), CreateCardStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{RulesTabPath}/DoctrineRuleCard"), CreateCardStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{RulesTabPath}/DisciplineRuleCard"), CreateCardStyle());
        ApplyPanelStyle(GetNode<PanelContainer>($"{AffairsTabPath}/AffairsListCard"), CreateCardStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>($"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsListPanel"),
            CreateInnerPaperStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>($"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsSplit/AffairsDetailPanel"),
            CreateInnerPaperStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>(
                $"{PolicyTabPath}/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentCapsule"),
            CreateControlCapsuleStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>($"{PolicyTabPath}/LawCard/LawCardMargin/LawCardRow/LawCapsule"),
            CreateControlCapsuleStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>($"{PolicyTabPath}/TalentCard/TalentCardMargin/TalentCardRow/TalentCapsule"),
            CreateControlCapsuleStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>($"{PolicyTabPath}/LockedCard/LockedCardMargin/LockedCardRow/LockedStateCapsule"),
            CreateControlCapsuleStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>(
                $"{SeasonTabPath}/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeCapsule"),
            CreateControlCapsuleStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>(
                $"{RulesTabPath}/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleCapsule"),
            CreateControlCapsuleStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>(
                $"{RulesTabPath}/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleCapsule"),
            CreateControlCapsuleStyle());
        ApplyPanelStyle(
            GetNode<PanelContainer>(
                $"{RulesTabPath}/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleCapsule"),
            CreateControlCapsuleStyle());

        ApplyLabelStyle(GetNode<Label>($"{HeaderPath}/TitleLabel"), 24, InkMainColor);
        ApplyLabelStyle(GetNode<Label>($"{HeaderPath}/StatRow/ContributionRow/ContributionTitle"), 15, InkMainColor);
        ApplyLabelStyle(GetNode<Label>($"{HeaderPath}/StatRow/SpiritStoneRow/SpiritStoneTitle"), 15, InkMainColor);
        ApplyLabelStyle(_contributionValueLabel, 16, SealRedColor);
        ApplyLabelStyle(_spiritStoneValueLabel, 16, SealRedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{PolicyTabPath}/PolicySummaryPanel/PolicySummaryMargin/PolicySummaryColumn/PolicySummaryTitle"),
            15,
            InkMainColor);
        ApplyLabelStyle(_policySummaryLabel, 13, InkMutedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{SeasonTabPath}/SeasonSummaryPanel/SeasonSummaryMargin/SeasonSummaryColumn/SeasonSummaryTitle"),
            15,
            InkMainColor);
        ApplyLabelStyle(_seasonSummaryLabel, 13, InkMutedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{RulesTabPath}/RulesSummaryPanel/RulesSummaryMargin/RulesSummaryColumn/RulesSummaryTitle"),
            15,
            InkMainColor);
        ApplyLabelStyle(_rulesSummaryLabel, 13, InkMutedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{AffairsTabPath}/AffairsSummaryPanel/AffairsSummaryMargin/AffairsSummaryColumn/AffairsSummaryTitle"),
            15,
            InkMainColor);
        ApplyLabelStyle(_affairsSummaryLabel, 13, InkMutedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{PolicyTabPath}/DevelopmentCard/DevelopmentCardMargin/DevelopmentCardRow/DevelopmentInfoColumn/DevelopmentTitle"),
            18,
            InkMainColor);
        ApplyLabelStyle(_developmentHintLabel, 13, InkMutedColor);
        ApplyLabelStyle(_developmentValueLabel, 16, SealRedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{PolicyTabPath}/LawCard/LawCardMargin/LawCardRow/LawInfoColumn/LawTitle"),
            18,
            InkMainColor);
        ApplyLabelStyle(_lawHintLabel, 13, InkMutedColor);
        ApplyLabelStyle(_lawValueLabel, 16, SealRedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{PolicyTabPath}/TalentCard/TalentCardMargin/TalentCardRow/TalentInfoColumn/TalentTitle"),
            18,
            InkMainColor);
        ApplyLabelStyle(_talentHintLabel, 13, InkMutedColor);
        ApplyLabelStyle(_talentValueLabel, 16, SealRedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{PolicyTabPath}/LockedCard/LockedCardMargin/LockedCardRow/LockedInfoColumn/LockedTitle"),
            18,
            InkMainColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{PolicyTabPath}/LockedCard/LockedCardMargin/LockedCardRow/LockedInfoColumn/LockedDescription"),
            13,
            InkMutedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{PolicyTabPath}/LockedCard/LockedCardMargin/LockedCardRow/LockedStateCapsule/LockedStateLabel"),
            14,
            InkMutedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{SeasonTabPath}/QuarterDecreeCard/QuarterDecreeCardMargin/QuarterDecreeCardRow/QuarterDecreeInfoColumn/QuarterDecreeTitle"),
            18,
            InkMainColor);
        ApplyLabelStyle(_quarterDecreeHintLabel, 13, InkMutedColor);
        ApplyLabelStyle(_quarterDecreeValueLabel, 16, SealRedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{RulesTabPath}/AffairsRuleCard/AffairsRuleCardMargin/AffairsRuleCardRow/AffairsRuleInfoColumn/AffairsRuleTitle"),
            18,
            InkMainColor);
        ApplyLabelStyle(_affairsRuleHintLabel, 13, InkMutedColor);
        ApplyLabelStyle(_affairsRuleValueLabel, 16, SealRedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{RulesTabPath}/DoctrineRuleCard/DoctrineRuleCardMargin/DoctrineRuleCardRow/DoctrineRuleInfoColumn/DoctrineRuleTitle"),
            18,
            InkMainColor);
        ApplyLabelStyle(_doctrineRuleHintLabel, 13, InkMutedColor);
        ApplyLabelStyle(_doctrineRuleValueLabel, 16, SealRedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{RulesTabPath}/DisciplineRuleCard/DisciplineRuleCardMargin/DisciplineRuleCardRow/DisciplineRuleInfoColumn/DisciplineRuleTitle"),
            18,
            InkMainColor);
        ApplyLabelStyle(_disciplineRuleHintLabel, 13, InkMutedColor);
        ApplyLabelStyle(_disciplineRuleValueLabel, 16, SealRedColor);
        ApplyLabelStyle(
            GetNode<Label>(
                $"{AffairsTabPath}/AffairsListCard/AffairsListMargin/AffairsListColumn/AffairsListTitle"),
            15,
            InkMainColor);
        ApplyLabelStyle(_detailLabel, 13, InkMainColor);
        ApplyLabelStyle(_hintLabel, 12, InkMutedColor);
        _hintLabel.AddThemeConstantOverride("line_spacing", 1);

        _closeButton.AddThemeFontSizeOverride("font_size", 24);
        _closeButton.AddThemeColorOverride("font_color", InkMutedColor);
        _closeButton.AddThemeColorOverride("font_hover_color", SealRedColor);
        _closeButton.AddThemeColorOverride("font_pressed_color", SealRedColor);
        _closeButton.AddThemeStyleboxOverride("normal", CreateTransparentStyle());
        _closeButton.AddThemeStyleboxOverride("hover", CreateTransparentStyle());
        _closeButton.AddThemeStyleboxOverride("pressed", CreateTransparentStyle());
        _closeButton.AddThemeStyleboxOverride("focus", CreateTransparentStyle());

        ApplySidebarButtonStyle(_policyTabButton);
        ApplySidebarButtonStyle(_seasonTabButton);
        ApplySidebarButtonStyle(_rulesTabButton);
        ApplySidebarButtonStyle(_affairsTabButton);

        ApplyArrowButtonStyle(_developmentPrevButton);
        ApplyArrowButtonStyle(_developmentNextButton);
        ApplyArrowButtonStyle(_lawPrevButton);
        ApplyArrowButtonStyle(_lawNextButton);
        ApplyArrowButtonStyle(_talentPrevButton);
        ApplyArrowButtonStyle(_talentNextButton);
        ApplyArrowButtonStyle(_quarterDecreePrevButton);
        ApplyArrowButtonStyle(_quarterDecreeNextButton);
        ApplyArrowButtonStyle(_affairsRulePrevButton);
        ApplyArrowButtonStyle(_affairsRuleNextButton);
        ApplyArrowButtonStyle(_doctrineRulePrevButton);
        ApplyArrowButtonStyle(_doctrineRuleNextButton);
        ApplyArrowButtonStyle(_disciplineRulePrevButton);
        ApplyArrowButtonStyle(_disciplineRuleNextButton);

        ApplyFooterActionButtonStyle(_minusOneButton, false);
        ApplyFooterActionButtonStyle(_plusOneButton, false);
        ApplyFooterActionButtonStyle(_plusFiveButton, false);
        ApplyFooterActionButtonStyle(_resetButton, true);

        _taskList.AddThemeStyleboxOverride("panel", CreateTransparentStyle());
        _taskList.AddThemeStyleboxOverride("cursor", CreateSelectionStyle());
        _taskList.AddThemeStyleboxOverride("cursor_unfocused", CreateSelectionStyle());
        _taskList.AddThemeColorOverride("font_color", InkMainColor);
        _taskList.AddThemeColorOverride("font_selected_color", PaperMainColor);
        _taskList.AddThemeConstantOverride("h_separation", 6);
        _taskList.AddThemeConstantOverride("v_separation", 5);

        _affairsSplit.SplitOffsets = new[] { 280 };
    }

    private void BindEvents()
    {
        _closeButton.Pressed += OnClosePressed;
        _policyTabButton.Pressed += () => SwitchTab(GovernanceTab.Policy);
        _seasonTabButton.Pressed += () => SwitchTab(GovernanceTab.Season);
        _rulesTabButton.Pressed += () => SwitchTab(GovernanceTab.Rules);
        _affairsTabButton.Pressed += () => SwitchTab(GovernanceTab.Affairs);
        _developmentPrevButton.Pressed += () => DevelopmentDirectionShiftRequested?.Invoke(-1);
        _developmentNextButton.Pressed += () => DevelopmentDirectionShiftRequested?.Invoke(1);
        _lawPrevButton.Pressed += () => SectLawShiftRequested?.Invoke(-1);
        _lawNextButton.Pressed += () => SectLawShiftRequested?.Invoke(1);
        _talentPrevButton.Pressed += () => TalentPlanShiftRequested?.Invoke(-1);
        _talentNextButton.Pressed += () => TalentPlanShiftRequested?.Invoke(1);
        _quarterDecreePrevButton.Pressed += () => QuarterDecreeShiftRequested?.Invoke(-1);
        _quarterDecreeNextButton.Pressed += () => QuarterDecreeShiftRequested?.Invoke(1);
        _affairsRulePrevButton.Pressed += () => AffairsRuleShiftRequested?.Invoke(-1);
        _affairsRuleNextButton.Pressed += () => AffairsRuleShiftRequested?.Invoke(1);
        _doctrineRulePrevButton.Pressed += () => DoctrineRuleShiftRequested?.Invoke(-1);
        _doctrineRuleNextButton.Pressed += () => DoctrineRuleShiftRequested?.Invoke(1);
        _disciplineRulePrevButton.Pressed += () => DisciplineRuleShiftRequested?.Invoke(-1);
        _disciplineRuleNextButton.Pressed += () => DisciplineRuleShiftRequested?.Invoke(1);
        _taskList.ItemSelected += index => OnTaskSelected((int)index);
        _minusOneButton.Pressed += () => AdjustSelectedTaskOrder(-1);
        _plusOneButton.Pressed += () => AdjustSelectedTaskOrder(1);
        _plusFiveButton.Pressed += () => AdjustSelectedTaskOrder(5);
        _resetButton.Pressed += OnResetPressed;
    }

    private static void ApplyPanelStyle(Control panel, StyleBox style)
    {
        panel.AddThemeStyleboxOverride("panel", style);
    }

    private static void ApplyLabelStyle(Label label, int fontSize, Color color)
    {
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
    }

    private void ApplySidebarButtonStyle(Button button)
    {
        button.AddThemeFontSizeOverride("font_size", 18);
        button.AddThemeColorOverride("font_color", InkMutedColor);
        button.AddThemeColorOverride("font_hover_color", InkMainColor);
        button.AddThemeColorOverride("font_pressed_color", InkMainColor);
        button.AddThemeStyleboxOverride("normal", CreateSidebarItemStyle(false, false));
        button.AddThemeStyleboxOverride("hover", CreateSidebarItemStyle(false, true));
        button.AddThemeStyleboxOverride("pressed", CreateSidebarItemStyle(true, false));
        button.AddThemeStyleboxOverride("focus", CreateSidebarItemStyle(false, true));
    }

    private void ApplyArrowButtonStyle(Button button)
    {
        button.AddThemeFontSizeOverride("font_size", 22);
        button.AddThemeColorOverride("font_color", InkMutedColor);
        button.AddThemeColorOverride("font_hover_color", SealRedColor);
        button.AddThemeColorOverride("font_pressed_color", SealRedColor);
        button.AddThemeStyleboxOverride("normal", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("hover", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("pressed", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("focus", CreateTransparentStyle());
    }

    private void ApplyFooterActionButtonStyle(Button button, bool accent)
    {
        button.AddThemeFontSizeOverride("font_size", 13);
        button.AddThemeColorOverride("font_color", accent ? SealRedColor : InkMainColor);
        button.AddThemeColorOverride("font_hover_color", PaperMainColor);
        button.AddThemeColorOverride("font_pressed_color", PaperMainColor);
        button.AddThemeStyleboxOverride("normal", CreateFooterButtonStyle(accent, false));
        button.AddThemeStyleboxOverride("hover", CreateFooterButtonStyle(accent, true));
        button.AddThemeStyleboxOverride("pressed", CreateFooterButtonStyle(accent, true));
        button.AddThemeStyleboxOverride("focus", CreateFooterButtonStyle(accent, true));
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

    public void Open(GameState state, SectTaskType? preferredTask = null)
    {
        RefreshState(state);
        SelectTask(preferredTask ?? _selectedTaskType);
        SwitchTab(GovernanceTab.Policy);
        OpenPopup();
    }

    public void RefreshState(GameState state)
    {
        _state = state.Clone();
        SectGovernanceRules.EnsureDefaults(_state);
        SectRuleTreeRules.EnsureDefaults(_state);
        SectTaskRules.EnsureDefaults(_state);
        RefreshSummary();
        RebuildTaskList();
        RefreshTaskDetail();
        RefreshPopupHint();
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return _activeTab switch
        {
            GovernanceTab.Policy => "大政方针用于确立宗门长期方向，执事层将据此自动落实。",
            GovernanceTab.Season => "节气法旨按季度生效，每季可择一令以顺应时势。",
            GovernanceTab.Rules => "门规戒律会持续影响庶务、传功与巡山三线执行表现。",
            GovernanceTab.Affairs => "庶务调度用于细化治务条目力度，峰外往来仍只认灵石。",
            _ => "宗主只定治宗法旨，执事层会依卷落实人手与资源。"
        };
    }

    private void BuildUi()
    {
        MouseFilter = Control.MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var overlay = new ColorRect
        {
            Color = new Color(0.06f, 0.06f, 0.06f, 0.68f),
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

        var frame = new PanelContainer
        {
            CustomMinimumSize = new Vector2(960, 680),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        frame.AddThemeStyleboxOverride("panel", CreateFrameStyle());
        center.AddChild(frame);

        var rootColumn = new VBoxContainer();
        rootColumn.AddThemeConstantOverride("separation", 0);
        frame.AddChild(rootColumn);

        var header = BuildHeader();
        rootColumn.AddChild(header);

        var headerDivider = new ColorRect
        {
            CustomMinimumSize = new Vector2(0, 1),
            Color = BorderInkColor
        };
        rootColumn.AddChild(headerDivider);

        var bodyRow = new HBoxContainer();
        bodyRow.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        bodyRow.AddThemeConstantOverride("separation", 0);
        rootColumn.AddChild(bodyRow);

        var sidebar = BuildSidebar();
        bodyRow.AddChild(sidebar);

        var bodyDivider = new ColorRect
        {
            CustomMinimumSize = new Vector2(1, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            Color = new Color(0.71f, 0.64f, 0.52f, 0.72f)
        };
        bodyRow.AddChild(bodyDivider);

        var contentMargin = new MarginContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        contentMargin.AddThemeConstantOverride("margin_left", 20);
        contentMargin.AddThemeConstantOverride("margin_top", 18);
        contentMargin.AddThemeConstantOverride("margin_right", 16);
        contentMargin.AddThemeConstantOverride("margin_bottom", 14);
        bodyRow.AddChild(contentMargin);

        _contentScroll = new ScrollContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        contentMargin.AddChild(_contentScroll);

        var contentStack = new VBoxContainer();
        contentStack.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentStack.AddThemeConstantOverride("separation", 12);
        _contentScroll.AddChild(contentStack);

        _policyTabContent = BuildPolicyTab();
        _seasonTabContent = BuildSeasonTab();
        _rulesTabContent = BuildRulesTab();
        _affairsTabContent = BuildAffairsTab();
        contentStack.AddChild(_policyTabContent);
        contentStack.AddChild(_seasonTabContent);
        contentStack.AddChild(_rulesTabContent);
        contentStack.AddChild(_affairsTabContent);

        var footerDivider = new ColorRect
        {
            CustomMinimumSize = new Vector2(0, 1),
            Color = new Color(0.71f, 0.64f, 0.52f, 0.66f)
        };
        rootColumn.AddChild(footerDivider);

        _hintLabel = new Label
        {
            CustomMinimumSize = new Vector2(0, 36),
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _hintLabel.AddThemeFontSizeOverride("font_size", 12);
        _hintLabel.AddThemeColorOverride("font_color", InkMutedColor);
        _hintLabel.AddThemeConstantOverride("line_spacing", 1);
        rootColumn.AddChild(_hintLabel);

        SwitchTab(GovernanceTab.Policy);
    }

    private PanelContainer BuildHeader()
    {
        var header = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 66)
        };
        header.AddThemeStyleboxOverride("panel", CreateHeaderStyle());

        var headerMargin = new MarginContainer();
        headerMargin.AddThemeConstantOverride("margin_left", 24);
        headerMargin.AddThemeConstantOverride("margin_top", 8);
        headerMargin.AddThemeConstantOverride("margin_right", 24);
        headerMargin.AddThemeConstantOverride("margin_bottom", 8);
        header.AddChild(headerMargin);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", 24);
        headerMargin.AddChild(headerRow);

        var titleLabel = new Label
        {
            Text = "浮云宗  ·  治宗册",
            VerticalAlignment = VerticalAlignment.Center
        };
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleLabel.AddThemeColorOverride("font_color", InkMainColor);
        headerRow.AddChild(titleLabel);

        var statRow = new HBoxContainer();
        statRow.AddThemeConstantOverride("separation", 16);
        headerRow.AddChild(statRow);

        statRow.AddChild(CreateStatPair("宗门贡献:", out _contributionValueLabel));
        statRow.AddChild(CreateStatPair("灵石结余:", out _spiritStoneValueLabel));

        _closeButton = new Button
        {
            Text = "×",
            CustomMinimumSize = new Vector2(40, 40),
            Flat = true,
            Alignment = HorizontalAlignment.Center
        };
        _closeButton.AddThemeFontSizeOverride("font_size", 24);
        _closeButton.AddThemeColorOverride("font_color", InkMutedColor);
        _closeButton.AddThemeColorOverride("font_hover_color", SealRedColor);
        _closeButton.AddThemeColorOverride("font_pressed_color", SealRedColor);
        _closeButton.AddThemeStyleboxOverride("normal", CreateTransparentStyle());
        _closeButton.AddThemeStyleboxOverride("hover", CreateTransparentStyle());
        _closeButton.AddThemeStyleboxOverride("pressed", CreateTransparentStyle());
        _closeButton.AddThemeStyleboxOverride("focus", CreateTransparentStyle());
        _closeButton.Pressed += OnClosePressed;
        headerRow.AddChild(_closeButton);

        return header;
    }

    private static HBoxContainer CreateStatPair(string title, out Label valueLabel)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 4);

        var titleLabel = new Label
        {
            Text = title
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 15);
        titleLabel.AddThemeColorOverride("font_color", InkMainColor);
        row.AddChild(titleLabel);

        valueLabel = new Label
        {
            Text = "0"
        };
        valueLabel.AddThemeFontSizeOverride("font_size", 16);
        valueLabel.AddThemeColorOverride("font_color", SealRedColor);
        row.AddChild(valueLabel);

        return row;
    }

    private PanelContainer BuildSidebar()
    {
        var sidebarPanel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(210, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        sidebarPanel.AddThemeStyleboxOverride("panel", CreateSidebarPanelStyle());

        var sidebarColumn = new VBoxContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        sidebarColumn.AddThemeConstantOverride("separation", 0);
        sidebarPanel.AddChild(sidebarColumn);

        AddSidebarTabButton(sidebarColumn, GovernanceTab.Policy, "大政方针");
        AddSidebarTabButton(sidebarColumn, GovernanceTab.Season, "节气法旨");
        AddSidebarTabButton(sidebarColumn, GovernanceTab.Rules, "门规戒律");
        AddSidebarTabButton(sidebarColumn, GovernanceTab.Affairs, "庶务调度");

        var spacer = new Control
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        sidebarColumn.AddChild(spacer);

        return sidebarPanel;
    }

    private void AddSidebarTabButton(VBoxContainer parent, GovernanceTab tab, string text)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 74),
            ToggleMode = true,
            Flat = true,
            Alignment = HorizontalAlignment.Left,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        button.AddThemeFontSizeOverride("font_size", 18);
        button.AddThemeColorOverride("font_color", InkMutedColor);
        button.AddThemeColorOverride("font_hover_color", InkMainColor);
        button.AddThemeColorOverride("font_pressed_color", InkMainColor);
        button.AddThemeStyleboxOverride("normal", CreateSidebarItemStyle(false, false));
        button.AddThemeStyleboxOverride("hover", CreateSidebarItemStyle(false, true));
        button.AddThemeStyleboxOverride("pressed", CreateSidebarItemStyle(true, false));
        button.AddThemeStyleboxOverride("focus", CreateSidebarItemStyle(false, true));
        button.Pressed += () => SwitchTab(tab);
        parent.AddChild(button);
        _tabButtons[tab] = button;
    }

    private Control BuildPolicyTab()
    {
        var tabContent = CreateTabContentRoot();

        var summaryPanel = CreateSummaryPanel("卷首批注", out _policySummaryLabel);
        tabContent.AddChild(summaryPanel);

        BuildGovernanceCard(
            tabContent,
            "发展方向",
            out _developmentValueLabel,
            out _developmentHintLabel,
            () => DevelopmentDirectionShiftRequested?.Invoke(-1),
            () => DevelopmentDirectionShiftRequested?.Invoke(1));
        BuildGovernanceCard(
            tabContent,
            "宗门法令",
            out _lawValueLabel,
            out _lawHintLabel,
            () => SectLawShiftRequested?.Invoke(-1),
            () => SectLawShiftRequested?.Invoke(1));
        BuildGovernanceCard(
            tabContent,
            "育才方略",
            out _talentValueLabel,
            out _talentHintLabel,
            () => TalentPlanShiftRequested?.Invoke(-1),
            () => TalentPlanShiftRequested?.Invoke(1));

        BuildLockedCard(tabContent, "外门劳役", "解锁后可调配外门弟子开荒。", "暂未参悟");

        return tabContent;
    }

    private Control BuildSeasonTab()
    {
        var tabContent = CreateTabContentRoot();

        var summaryPanel = CreateSummaryPanel("季令批注", out _seasonSummaryLabel);
        tabContent.AddChild(summaryPanel);

        BuildGovernanceCard(
            tabContent,
            "季度法令",
            out _quarterDecreeValueLabel,
            out _quarterDecreeHintLabel,
            () => QuarterDecreeShiftRequested?.Invoke(-1),
            () => QuarterDecreeShiftRequested?.Invoke(1));

        return tabContent;
    }

    private Control BuildRulesTab()
    {
        var tabContent = CreateTabContentRoot();

        var summaryPanel = CreateSummaryPanel("卷中批注", out _rulesSummaryLabel);
        tabContent.AddChild(summaryPanel);

        BuildGovernanceCard(
            tabContent,
            "庶务门规",
            out _affairsRuleValueLabel,
            out _affairsRuleHintLabel,
            () => AffairsRuleShiftRequested?.Invoke(-1),
            () => AffairsRuleShiftRequested?.Invoke(1));
        BuildGovernanceCard(
            tabContent,
            "传功门规",
            out _doctrineRuleValueLabel,
            out _doctrineRuleHintLabel,
            () => DoctrineRuleShiftRequested?.Invoke(-1),
            () => DoctrineRuleShiftRequested?.Invoke(1));
        BuildGovernanceCard(
            tabContent,
            "巡山门规",
            out _disciplineRuleValueLabel,
            out _disciplineRuleHintLabel,
            () => DisciplineRuleShiftRequested?.Invoke(-1),
            () => DisciplineRuleShiftRequested?.Invoke(1));

        return tabContent;
    }

    private Control BuildAffairsTab()
    {
        var tabContent = CreateTabContentRoot();

        var summaryPanel = CreateSummaryPanel("庶务总纲", out _affairsSummaryLabel);
        tabContent.AddChild(summaryPanel);

        var listDetailCard = new PanelContainer();
        listDetailCard.AddThemeStyleboxOverride("panel", CreateCardStyle());
        tabContent.AddChild(listDetailCard);

        var cardMargin = new MarginContainer();
        cardMargin.AddThemeConstantOverride("margin_left", 14);
        cardMargin.AddThemeConstantOverride("margin_top", 14);
        cardMargin.AddThemeConstantOverride("margin_right", 14);
        cardMargin.AddThemeConstantOverride("margin_bottom", 14);
        listDetailCard.AddChild(cardMargin);

        var cardColumn = new VBoxContainer();
        cardColumn.AddThemeConstantOverride("separation", 12);
        cardMargin.AddChild(cardColumn);

        var listTitle = new Label
        {
            Text = "治务条目",
            VerticalAlignment = VerticalAlignment.Center
        };
        listTitle.AddThemeFontSizeOverride("font_size", 15);
        listTitle.AddThemeColorOverride("font_color", InkMainColor);
        cardColumn.AddChild(listTitle);

        var split = new HSplitContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 320)
        };
        split.SplitOffsets = new[] { 280 };
        cardColumn.AddChild(split);

        var listPanel = new PanelContainer();
        listPanel.AddThemeStyleboxOverride("panel", CreateInnerPaperStyle());
        split.AddChild(listPanel);

        var listMargin = new MarginContainer();
        listMargin.AddThemeConstantOverride("margin_left", 10);
        listMargin.AddThemeConstantOverride("margin_top", 10);
        listMargin.AddThemeConstantOverride("margin_right", 10);
        listMargin.AddThemeConstantOverride("margin_bottom", 10);
        listPanel.AddChild(listMargin);

        _taskList = new ItemList
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            AllowReselect = true,
            SelectMode = ItemList.SelectModeEnum.Single
        };
        _taskList.AddThemeStyleboxOverride("panel", CreateTransparentStyle());
        _taskList.AddThemeStyleboxOverride("cursor", CreateSelectionStyle());
        _taskList.AddThemeStyleboxOverride("cursor_unfocused", CreateSelectionStyle());
        _taskList.AddThemeColorOverride("font_color", InkMainColor);
        _taskList.AddThemeColorOverride("font_selected_color", PaperMainColor);
        _taskList.AddThemeConstantOverride("h_separation", 6);
        _taskList.AddThemeConstantOverride("v_separation", 5);
        _taskList.ItemSelected += index => OnTaskSelected((int)index);
        listMargin.AddChild(_taskList);

        var detailPanel = new PanelContainer();
        detailPanel.AddThemeStyleboxOverride("panel", CreateInnerPaperStyle());
        split.AddChild(detailPanel);

        var detailMargin = new MarginContainer();
        detailMargin.AddThemeConstantOverride("margin_left", 12);
        detailMargin.AddThemeConstantOverride("margin_top", 12);
        detailMargin.AddThemeConstantOverride("margin_right", 12);
        detailMargin.AddThemeConstantOverride("margin_bottom", 12);
        detailPanel.AddChild(detailMargin);

        _detailLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        _detailLabel.AddThemeFontSizeOverride("font_size", 13);
        _detailLabel.AddThemeColorOverride("font_color", InkMainColor);
        detailMargin.AddChild(_detailLabel);

        var actionRow = new HBoxContainer();
        actionRow.AddThemeConstantOverride("separation", 8);
        cardColumn.AddChild(actionRow);

        _minusOneButton = CreateFooterActionButton("收敛");
        _minusOneButton.Pressed += () => AdjustSelectedTaskOrder(-1);
        actionRow.AddChild(_minusOneButton);

        _plusOneButton = CreateFooterActionButton("推进");
        _plusOneButton.Pressed += () => AdjustSelectedTaskOrder(1);
        actionRow.AddChild(_plusOneButton);

        _plusFiveButton = CreateFooterActionButton("鼎力推进");
        _plusFiveButton.Pressed += () => AdjustSelectedTaskOrder(5);
        actionRow.AddChild(_plusFiveButton);

        var spacer = new Control
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        actionRow.AddChild(spacer);

        _resetButton = CreateFooterActionButton("复归常制", accent: true);
        _resetButton.Pressed += OnResetPressed;
        actionRow.AddChild(_resetButton);

        return tabContent;
    }

    private static VBoxContainer CreateTabContentRoot()
    {
        var tabContent = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        tabContent.AddThemeConstantOverride("separation", 14);
        return tabContent;
    }

    private static PanelContainer CreateSummaryPanel(string title, out Label summaryLabel)
    {
        var summaryPanel = new PanelContainer();
        summaryPanel.AddThemeStyleboxOverride("panel", CreateSummaryStyle());

        var summaryMargin = new MarginContainer();
        summaryMargin.AddThemeConstantOverride("margin_left", 16);
        summaryMargin.AddThemeConstantOverride("margin_top", 12);
        summaryMargin.AddThemeConstantOverride("margin_right", 16);
        summaryMargin.AddThemeConstantOverride("margin_bottom", 12);
        summaryPanel.AddChild(summaryMargin);

        var summaryColumn = new VBoxContainer();
        summaryColumn.AddThemeConstantOverride("separation", 6);
        summaryMargin.AddChild(summaryColumn);

        var titleLabel = new Label
        {
            Text = title
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 15);
        titleLabel.AddThemeColorOverride("font_color", InkMainColor);
        summaryColumn.AddChild(titleLabel);

        summaryLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        summaryLabel.AddThemeFontSizeOverride("font_size", 13);
        summaryLabel.AddThemeColorOverride("font_color", InkMutedColor);
        summaryColumn.AddChild(summaryLabel);

        return summaryPanel;
    }

    private static void BuildGovernanceCard(
        VBoxContainer parent,
        string title,
        out Label valueLabel,
        out Label hintLabel,
        Action onPrevious,
        Action onNext)
    {
        var cardPanel = new PanelContainer();
        cardPanel.AddThemeStyleboxOverride("panel", CreateCardStyle());
        parent.AddChild(cardPanel);

        var cardMargin = new MarginContainer();
        cardMargin.AddThemeConstantOverride("margin_left", 16);
        cardMargin.AddThemeConstantOverride("margin_top", 14);
        cardMargin.AddThemeConstantOverride("margin_right", 16);
        cardMargin.AddThemeConstantOverride("margin_bottom", 14);
        cardPanel.AddChild(cardMargin);

        var cardRow = new HBoxContainer();
        cardRow.AddThemeConstantOverride("separation", 18);
        cardMargin.AddChild(cardRow);

        var infoColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        infoColumn.AddThemeConstantOverride("separation", 6);
        cardRow.AddChild(infoColumn);

        var titleLabel = new Label
        {
            Text = title
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        titleLabel.AddThemeColorOverride("font_color", InkMainColor);
        infoColumn.AddChild(titleLabel);

        hintLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        hintLabel.AddThemeFontSizeOverride("font_size", 13);
        hintLabel.AddThemeColorOverride("font_color", InkMutedColor);
        infoColumn.AddChild(hintLabel);

        var capsule = new PanelContainer
        {
            CustomMinimumSize = new Vector2(260, 58)
        };
        capsule.AddThemeStyleboxOverride("panel", CreateControlCapsuleStyle());
        cardRow.AddChild(capsule);

        var capsuleRow = new HBoxContainer();
        capsuleRow.Alignment = BoxContainer.AlignmentMode.Center;
        capsuleRow.AddThemeConstantOverride("separation", 10);
        capsule.AddChild(capsuleRow);

        var previousButton = CreateArrowButton("◀");
        previousButton.Pressed += onPrevious;
        capsuleRow.AddChild(previousButton);

        valueLabel = new Label
        {
            CustomMinimumSize = new Vector2(132, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        valueLabel.AddThemeFontSizeOverride("font_size", 16);
        valueLabel.AddThemeColorOverride("font_color", SealRedColor);
        capsuleRow.AddChild(valueLabel);

        var nextButton = CreateArrowButton("▶");
        nextButton.Pressed += onNext;
        capsuleRow.AddChild(nextButton);
    }

    private static void BuildLockedCard(VBoxContainer parent, string title, string description, string statusText)
    {
        var cardPanel = new PanelContainer();
        cardPanel.AddThemeStyleboxOverride("panel", CreateCardStyle());
        parent.AddChild(cardPanel);

        var cardMargin = new MarginContainer();
        cardMargin.AddThemeConstantOverride("margin_left", 16);
        cardMargin.AddThemeConstantOverride("margin_top", 14);
        cardMargin.AddThemeConstantOverride("margin_right", 16);
        cardMargin.AddThemeConstantOverride("margin_bottom", 14);
        cardPanel.AddChild(cardMargin);

        var cardRow = new HBoxContainer();
        cardRow.AddThemeConstantOverride("separation", 18);
        cardMargin.AddChild(cardRow);

        var infoColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        infoColumn.AddThemeConstantOverride("separation", 6);
        cardRow.AddChild(infoColumn);

        var titleLabel = new Label
        {
            Text = title
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        titleLabel.AddThemeColorOverride("font_color", InkMainColor);
        infoColumn.AddChild(titleLabel);

        var descLabel = new Label
        {
            Text = description
        };
        descLabel.AddThemeFontSizeOverride("font_size", 13);
        descLabel.AddThemeColorOverride("font_color", InkMutedColor);
        infoColumn.AddChild(descLabel);

        var stateCapsule = new PanelContainer
        {
            CustomMinimumSize = new Vector2(170, 54)
        };
        stateCapsule.AddThemeStyleboxOverride("panel", CreateControlCapsuleStyle());
        cardRow.AddChild(stateCapsule);

        var stateLabel = new Label
        {
            Text = statusText,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        stateLabel.AddThemeFontSizeOverride("font_size", 14);
        stateLabel.AddThemeColorOverride("font_color", InkMutedColor);
        stateCapsule.AddChild(stateLabel);
    }

    private static Button CreateArrowButton(string text)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(42, 38),
            Flat = true,
            Alignment = HorizontalAlignment.Center
        };
        button.AddThemeFontSizeOverride("font_size", 22);
        button.AddThemeColorOverride("font_color", InkMutedColor);
        button.AddThemeColorOverride("font_hover_color", SealRedColor);
        button.AddThemeColorOverride("font_pressed_color", SealRedColor);
        button.AddThemeStyleboxOverride("normal", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("hover", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("pressed", CreateTransparentStyle());
        button.AddThemeStyleboxOverride("focus", CreateTransparentStyle());
        return button;
    }

    private Button CreateFooterActionButton(string text, bool accent = false)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(136, 40),
            Flat = true,
            Alignment = HorizontalAlignment.Center
        };
        button.AddThemeFontSizeOverride("font_size", 13);
        button.AddThemeColorOverride("font_color", accent ? SealRedColor : InkMainColor);
        button.AddThemeColorOverride("font_hover_color", PaperMainColor);
        button.AddThemeColorOverride("font_pressed_color", PaperMainColor);
        button.AddThemeStyleboxOverride("normal", CreateFooterButtonStyle(accent, false));
        button.AddThemeStyleboxOverride("hover", CreateFooterButtonStyle(accent, true));
        button.AddThemeStyleboxOverride("pressed", CreateFooterButtonStyle(accent, true));
        button.AddThemeStyleboxOverride("focus", CreateFooterButtonStyle(accent, true));
        return button;
    }

    private void SwitchTab(GovernanceTab tab)
    {
        _activeTab = tab;

        foreach (var (tabKey, button) in _tabButtons)
        {
            button.ButtonPressed = tabKey == tab;
            button.AddThemeColorOverride("font_color", tabKey == tab ? InkMainColor : InkMutedColor);
        }

        _policyTabContent.Visible = tab == GovernanceTab.Policy;
        _seasonTabContent.Visible = tab == GovernanceTab.Season;
        _rulesTabContent.Visible = tab == GovernanceTab.Rules;
        _affairsTabContent.Visible = tab == GovernanceTab.Affairs;

        _contentScroll.ScrollVertical = 0;
        RefreshPopupHint();
    }

    private void RefreshSummary()
    {
        var spiritStone = InventoryRules.GetVisibleAmount(_state, nameof(GameState.Gold));
        var contribution = InventoryRules.GetVisibleAmount(_state, nameof(GameState.ContributionPoints));
        var development = SectGovernanceRules.GetActiveDevelopmentDefinition(_state);
        var law = SectGovernanceRules.GetActiveLawDefinition(_state);
        var talentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(_state);
        var quarterDecree = SectGovernanceRules.GetActiveQuarterDecreeDefinition(_state);
        var affairsRule = SectRuleTreeRules.GetActiveAffairsDefinition(_state);
        var doctrineRule = SectRuleTreeRules.GetActiveDoctrineDefinition(_state);
        var disciplineRule = SectRuleTreeRules.GetActiveDisciplineDefinition(_state);
        var quarterLabel = _calendarSystem.GetQuarterLabel(_state.GameMinutes);

        _contributionValueLabel.Text = contribution.ToString("N0");
        _spiritStoneValueLabel.Text = spiritStone.ToString("N0");

        _policySummaryLabel.Text =
            $"当前治宗重心为【{SectTaskRules.BuildGovernanceHeadline(_state)}】。{SectTaskRules.BuildGovernanceExecutionSummary(_state)}。";
        _seasonSummaryLabel.Text =
            $"当前时序：{quarterLabel}。本季法令为【{quarterDecree.DisplayName}】。";
        _rulesSummaryLabel.Text =
            $"现行门规：{SectRuleTreeRules.BuildActiveRuleSummary(_state)}。";
        _affairsSummaryLabel.Text =
            "此卷用于精细调度各条庶务力度；峰内内务走贡献点与灵石双轨，峰外往来只认灵石。";

        _developmentValueLabel.Text = development.DisplayName;
        _developmentHintLabel.Text = development.ShortEffect;
        _lawValueLabel.Text = law.DisplayName;
        _lawHintLabel.Text = law.ShortEffect;
        _talentValueLabel.Text = talentPlan.DisplayName;
        _talentHintLabel.Text = talentPlan.ShortEffect;
        _quarterDecreeValueLabel.Text = quarterDecree.DisplayName;
        _quarterDecreeHintLabel.Text = quarterDecree.DecreeType == SectQuarterDecreeType.None
            ? $"{quarterLabel}待颁法令"
            : $"{quarterLabel} · {quarterDecree.ShortEffect}";
        _affairsRuleValueLabel.Text = affairsRule.DisplayName;
        _affairsRuleHintLabel.Text = affairsRule.ShortEffect;
        _doctrineRuleValueLabel.Text = doctrineRule.DisplayName;
        _doctrineRuleHintLabel.Text = doctrineRule.ShortEffect;
        _disciplineRuleValueLabel.Text = disciplineRule.DisplayName;
        _disciplineRuleHintLabel.Text = disciplineRule.ShortEffect;
    }

    private void RebuildTaskList()
    {
        _taskList.Clear();
        foreach (var definition in SectTaskRules.GetOrderedDefinitions())
        {
            _taskList.AddItem(SectTaskRules.GetTaskListText(_state, definition.TaskType));
        }

        SelectTask(_selectedTaskType);
    }

    private void OnTaskSelected(int index)
    {
        var definitions = SectTaskRules.GetOrderedDefinitions();
        if (index < 0 || index >= definitions.Count)
        {
            return;
        }

        _selectedTaskType = definitions[index].TaskType;
        RefreshTaskDetail();
        RefreshPopupHint();
    }

    private void RefreshTaskDetail()
    {
        _detailLabel.Text = SectTaskRules.BuildTaskDetailText(_state, _selectedTaskType);
    }

    private void SelectTask(SectTaskType taskType)
    {
        _selectedTaskType = taskType;
        var definitions = SectTaskRules.GetOrderedDefinitions();
        for (var index = 0; index < definitions.Count; index++)
        {
            if (definitions[index].TaskType != taskType)
            {
                continue;
            }

            _taskList.Select(index);
            break;
        }

        RefreshTaskDetail();
    }

    private void AdjustSelectedTaskOrder(int delta)
    {
        OrderAdjustmentRequested?.Invoke(_selectedTaskType, delta);
        var definition = SectTaskRules.GetDefinition(_selectedTaskType);
        ShowPopupStatusMessage($"已向执事层批复“{definition.DisplayName}”的治宗调整。");
    }

    private void OnResetPressed()
    {
        ResetRequested?.Invoke();
        ShowPopupStatusMessage("已请执事层复归常制治宗方略。");
    }

    private static StyleBoxFlat CreateFrameStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = PaperMainColor,
            BorderColor = BorderInkColor,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            ShadowColor = new Color(0f, 0f, 0f, 0.45f),
            ShadowSize = 24
        };
    }

    private static StyleBoxFlat CreateHeaderStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(PaperDarkColor.R, PaperDarkColor.G, PaperDarkColor.B, 0.72f),
            BorderWidthLeft = 0,
            BorderWidthTop = 0,
            BorderWidthRight = 0,
            BorderWidthBottom = 0
        };
    }

    private static StyleBoxFlat CreateSidebarPanelStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(PaperMainColor.R, PaperMainColor.G, PaperMainColor.B, 0.38f),
            BorderWidthLeft = 0,
            BorderWidthTop = 0,
            BorderWidthRight = 0,
            BorderWidthBottom = 0
        };
    }

    private static StyleBoxFlat CreateSidebarItemStyle(bool active, bool hover)
    {
        return new StyleBoxFlat
        {
            BgColor = active
                ? new Color(PaperDarkColor.R, PaperDarkColor.G, PaperDarkColor.B, 0.52f)
                : hover
                    ? new Color(PaperDarkColor.R, PaperDarkColor.G, PaperDarkColor.B, 0.30f)
                    : new Color(0f, 0f, 0f, 0f),
            BorderWidthLeft = 4,
            BorderWidthTop = 0,
            BorderWidthRight = 0,
            BorderWidthBottom = 1,
            BorderColor = active
                ? SidebarActiveColor
                : new Color(0.82f, 0.76f, 0.67f, 0.82f),
            ContentMarginLeft = 24,
            ContentMarginRight = 16
        };
    }

    private static StyleBoxFlat CreateSummaryStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(1f, 1f, 1f, 0.40f),
            BorderColor = new Color(0.77f, 0.71f, 0.62f, 0.95f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1
        };
    }

    private static StyleBoxFlat CreateCardStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(PaperMainColor.R, PaperMainColor.G, PaperMainColor.B, 0.88f),
            BorderColor = new Color(0.71f, 0.64f, 0.54f, 0.96f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ShadowColor = new Color(0.58f, 0.50f, 0.41f, 0.25f),
            ShadowSize = 2
        };
    }

    private static StyleBoxFlat CreateControlCapsuleStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(PaperDarkColor.R, PaperDarkColor.G, PaperDarkColor.B, 0.70f),
            BorderColor = new Color(0.78f, 0.69f, 0.57f, 0.9f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 24,
            CornerRadiusTopRight = 24,
            CornerRadiusBottomRight = 24,
            CornerRadiusBottomLeft = 24
        };
    }

    private static StyleBoxFlat CreateInnerPaperStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.95f, 0.92f, 0.84f, 0.74f),
            BorderColor = new Color(0.78f, 0.71f, 0.61f, 0.92f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1
        };
    }

    private static StyleBoxFlat CreateFooterButtonStyle(bool accent, bool active)
    {
        var border = accent ? SealRedColor : BorderInkColor;
        return new StyleBoxFlat
        {
            BgColor = active ? border : new Color(0f, 0f, 0f, 0f),
            BorderColor = border,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ContentMarginLeft = 12,
            ContentMarginTop = 8,
            ContentMarginRight = 12,
            ContentMarginBottom = 8
        };
    }

    private static StyleBoxFlat CreateSelectionStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = InkMainColor,
            BorderWidthLeft = 0,
            BorderWidthTop = 0,
            BorderWidthRight = 0,
            BorderWidthBottom = 0
        };
    }

    private static StyleBoxEmpty CreateTransparentStyle()
    {
        return new StyleBoxEmpty();
    }

    private void OnClosePressed()
    {
        ClosePopup();
    }
}

