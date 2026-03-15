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
    private Node? _visualFx;

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
        _visualFx = GetNodeOrNull<Node>("VisualFx");
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
        CallVisualFx("play_open");
    }

    public void ClosePanel()
    {
        ClosePopup();
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

    private void SwitchTab(GovernanceTab tab)
    {
        _activeTab = tab;

        foreach (var (tabKey, button) in _tabButtons)
        {
            button.ButtonPressed = tabKey == tab;
        }

        _policyTabContent.Visible = tab == GovernanceTab.Policy;
        _seasonTabContent.Visible = tab == GovernanceTab.Season;
        _rulesTabContent.Visible = tab == GovernanceTab.Rules;
        _affairsTabContent.Visible = tab == GovernanceTab.Affairs;

        _contentScroll.ScrollVertical = 0;
        RefreshPopupHint();
        CallVisualFx("apply_tab_button_state", tab.ToString());
        CallVisualFx("play_tab_switch", tab.ToString());
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
        CallVisualFx("pulse_task_detail");
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

    private void CallVisualFx(string methodName, params Variant[] args)
    {
        _visualFx?.Call(methodName, args);
    }

    private void OnClosePressed()
    {
        ClosePopup();
    }
}
