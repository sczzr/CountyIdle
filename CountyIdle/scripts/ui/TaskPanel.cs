using System;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class TaskPanel : PopupPanelBase
{
    private readonly GameCalendarSystem _calendarSystem = new();

    private Label _spiritStoneLabel = null!;
    private Label _contributionLabel = null!;
    private Label _assignmentLabel = null!;
    private Label _tradeRuleLabel = null!;
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

    private GameState _state = new();
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

    public void Open(GameState state, SectTaskType? preferredTask = null)
    {
        RefreshState(state);
        SelectTask(preferredTask ?? _selectedTaskType);
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

        var definition = SectTaskRules.GetDefinition(_selectedTaskType);
        return definition.IsInternalTask
            ? "宗主只定峰内方向与法令力度，执事层会自动协调人手与资源。"
            : "宗主只定外务倾向，对外贸易仍只使用灵石结算。";
    }

    private void BuildUi()
    {
        MouseFilter = Control.MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var overlay = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0.74f),
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

        var dialog = new PanelContainer
        {
            CustomMinimumSize = new Vector2(920, 560),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
        };
        dialog.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.10f, 0.11f, 0.14f, 0.98f)));
        center.AddChild(dialog);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_top", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_bottom", 18);
        dialog.AddChild(margin);

        var rootColumn = new VBoxContainer();
        rootColumn.AddThemeConstantOverride("separation", 12);
        margin.AddChild(rootColumn);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", 10);
        rootColumn.AddChild(headerRow);

        var titleLabel = new Label
        {
            Text = "浮云宗·宗主中枢"
        };
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleLabel.AddThemeFontSizeOverride("font_size", 20);
        titleLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.91f, 0.79f));
        headerRow.AddChild(titleLabel);

        _closeButton = CreateActionButton("关闭");
        _closeButton.Pressed += ClosePopup;
        headerRow.AddChild(_closeButton);

        var summaryPanel = new PanelContainer();
        summaryPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.06f, 0.07f, 0.09f, 0.92f)));
        rootColumn.AddChild(summaryPanel);

        var summaryMargin = new MarginContainer();
        summaryMargin.AddThemeConstantOverride("margin_left", 12);
        summaryMargin.AddThemeConstantOverride("margin_top", 12);
        summaryMargin.AddThemeConstantOverride("margin_right", 12);
        summaryMargin.AddThemeConstantOverride("margin_bottom", 12);
        summaryPanel.AddChild(summaryMargin);

        var summaryColumn = new VBoxContainer();
        summaryColumn.AddThemeConstantOverride("separation", 6);
        summaryMargin.AddChild(summaryColumn);

        _spiritStoneLabel = CreateSummaryLabel();
        _contributionLabel = CreateSummaryLabel();
        _assignmentLabel = CreateSummaryLabel();
        _tradeRuleLabel = CreateSummaryLabel();
        summaryColumn.AddChild(_spiritStoneLabel);
        summaryColumn.AddChild(_contributionLabel);
        summaryColumn.AddChild(_assignmentLabel);
        summaryColumn.AddChild(_tradeRuleLabel);

        var governancePanel = new PanelContainer();
        governancePanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.07f, 0.08f, 0.10f, 0.94f)));
        rootColumn.AddChild(governancePanel);

        var governanceMargin = new MarginContainer();
        governanceMargin.AddThemeConstantOverride("margin_left", 12);
        governanceMargin.AddThemeConstantOverride("margin_top", 12);
        governanceMargin.AddThemeConstantOverride("margin_right", 12);
        governanceMargin.AddThemeConstantOverride("margin_bottom", 12);
        governancePanel.AddChild(governanceMargin);

        var governanceColumn = new VBoxContainer();
        governanceColumn.AddThemeConstantOverride("separation", 10);
        governanceMargin.AddChild(governanceColumn);

        governanceColumn.AddChild(CreateSummaryLabel("宗主决策"));
        BuildGovernanceRow(
            governanceColumn,
            "发展方向",
            out _developmentValueLabel,
            out _developmentHintLabel,
            () => DevelopmentDirectionShiftRequested?.Invoke(-1),
            () => DevelopmentDirectionShiftRequested?.Invoke(1));
        BuildGovernanceRow(
            governanceColumn,
            "宗门法令",
            out _lawValueLabel,
            out _lawHintLabel,
            () => SectLawShiftRequested?.Invoke(-1),
            () => SectLawShiftRequested?.Invoke(1));
        BuildGovernanceRow(
            governanceColumn,
            "育才方略",
            out _talentValueLabel,
            out _talentHintLabel,
            () => TalentPlanShiftRequested?.Invoke(-1),
            () => TalentPlanShiftRequested?.Invoke(1));
        BuildGovernanceRow(
            governanceColumn,
            "季度法令",
            out _quarterDecreeValueLabel,
            out _quarterDecreeHintLabel,
            () => QuarterDecreeShiftRequested?.Invoke(-1),
            () => QuarterDecreeShiftRequested?.Invoke(1));
        BuildGovernanceRow(
            governanceColumn,
            "庶务门规",
            out _affairsRuleValueLabel,
            out _affairsRuleHintLabel,
            () => AffairsRuleShiftRequested?.Invoke(-1),
            () => AffairsRuleShiftRequested?.Invoke(1));
        BuildGovernanceRow(
            governanceColumn,
            "传功门规",
            out _doctrineRuleValueLabel,
            out _doctrineRuleHintLabel,
            () => DoctrineRuleShiftRequested?.Invoke(-1),
            () => DoctrineRuleShiftRequested?.Invoke(1));
        BuildGovernanceRow(
            governanceColumn,
            "巡山门规",
            out _disciplineRuleValueLabel,
            out _disciplineRuleHintLabel,
            () => DisciplineRuleShiftRequested?.Invoke(-1),
            () => DisciplineRuleShiftRequested?.Invoke(1));

        var contentSplit = new HSplitContainer();
        contentSplit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentSplit.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        contentSplit.SplitOffsets = new[] { 320 };
        rootColumn.AddChild(contentSplit);

        var listPanel = new PanelContainer();
        listPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        listPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        listPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.08f, 0.09f, 0.11f, 0.94f)));
        contentSplit.AddChild(listPanel);

        var listMargin = new MarginContainer();
        listMargin.AddThemeConstantOverride("margin_left", 10);
        listMargin.AddThemeConstantOverride("margin_top", 10);
        listMargin.AddThemeConstantOverride("margin_right", 10);
        listMargin.AddThemeConstantOverride("margin_bottom", 10);
        listPanel.AddChild(listMargin);

        var listColumn = new VBoxContainer();
        listColumn.AddThemeConstantOverride("separation", 8);
        listMargin.AddChild(listColumn);

        var listTitle = CreateSummaryLabel("治理条目");
        listTitle.AddThemeFontSizeOverride("font_size", 14);
        listColumn.AddChild(listTitle);

        _taskList = new ItemList
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            AllowReselect = true,
            SelectMode = ItemList.SelectModeEnum.Single
        };
        _taskList.ItemSelected += index => OnTaskSelected((int)index);
        listColumn.AddChild(_taskList);

        var detailPanel = new PanelContainer();
        detailPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        detailPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detailPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.08f, 0.09f, 0.11f, 0.94f)));
        contentSplit.AddChild(detailPanel);

        var detailMargin = new MarginContainer();
        detailMargin.AddThemeConstantOverride("margin_left", 14);
        detailMargin.AddThemeConstantOverride("margin_top", 14);
        detailMargin.AddThemeConstantOverride("margin_right", 14);
        detailMargin.AddThemeConstantOverride("margin_bottom", 14);
        detailPanel.AddChild(detailMargin);

        _detailLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        _detailLabel.AddThemeFontSizeOverride("font_size", 13);
        _detailLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.88f, 0.92f));
        detailMargin.AddChild(_detailLabel);

        var actionRow = new HBoxContainer();
        actionRow.AddThemeConstantOverride("separation", 10);
        rootColumn.AddChild(actionRow);

        _minusOneButton = CreateActionButton("收敛");
        _minusOneButton.Pressed += () => AdjustSelectedTaskOrder(-1);
        actionRow.AddChild(_minusOneButton);

        _plusOneButton = CreateActionButton("推进");
        _plusOneButton.Pressed += () => AdjustSelectedTaskOrder(1);
        actionRow.AddChild(_plusOneButton);

        _plusFiveButton = CreateActionButton("鼎力推进");
        _plusFiveButton.Pressed += () => AdjustSelectedTaskOrder(5);
        actionRow.AddChild(_plusFiveButton);

        var spacer = new Control
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        actionRow.AddChild(spacer);

        _resetButton = CreateActionButton("恢复均衡");
        _resetButton.Pressed += OnResetPressed;
        actionRow.AddChild(_resetButton);

        var footerCloseButton = CreateActionButton("返回");
        footerCloseButton.Pressed += ClosePopup;
        actionRow.AddChild(footerCloseButton);

        _hintLabel = CreateSummaryLabel();
        _hintLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _hintLabel.AddThemeColorOverride("font_color", new Color(0.71f, 0.76f, 0.86f));
        rootColumn.AddChild(_hintLabel);
    }

    private void RefreshSummary()
    {
        var snapshot = SectTaskRules.BuildResolutionSnapshot(_state);
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

        _spiritStoneLabel.Text = $"灵石：{spiritStone}";
        _contributionLabel.Text = $"贡献点：{contribution}";
        _assignmentLabel.Text = $"治宗重心：{SectTaskRules.BuildGovernanceHeadline(_state)}";
        _tradeRuleLabel.Text = $"执行态势：{SectTaskRules.BuildGovernanceExecutionSummary(_state)} · 当前{quarterLabel}法令：{quarterDecree.DisplayName} · {SectRuleTreeRules.BuildActiveRuleSummary(_state)} · 内务走贡献点 + 灵石，外事只认灵石。";
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
        ShowPopupStatusMessage($"已向执事层下达“{definition.DisplayName}”的治理调整。");
    }

    private void OnResetPressed()
    {
        ResetRequested?.Invoke();
        ShowPopupStatusMessage("已请求恢复均衡治宗方略。");
    }

    private static Label CreateSummaryLabel(string text = "")
    {
        return new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
    }

    private static void BuildGovernanceRow(
        VBoxContainer parent,
        string title,
        out Label valueLabel,
        out Label hintLabel,
        Action onPrevious,
        Action onNext)
    {
        var rowPanel = new PanelContainer();
        rowPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.09f, 0.10f, 0.13f, 0.92f)));
        parent.AddChild(rowPanel);

        var rowMargin = new MarginContainer();
        rowMargin.AddThemeConstantOverride("margin_left", 10);
        rowMargin.AddThemeConstantOverride("margin_top", 8);
        rowMargin.AddThemeConstantOverride("margin_right", 10);
        rowMargin.AddThemeConstantOverride("margin_bottom", 8);
        rowPanel.AddChild(rowMargin);

        var rowColumn = new VBoxContainer();
        rowColumn.AddThemeConstantOverride("separation", 6);
        rowMargin.AddChild(rowColumn);

        var titleRow = new HBoxContainer();
        titleRow.AddThemeConstantOverride("separation", 8);
        rowColumn.AddChild(titleRow);

        var titleLabel = CreateSummaryLabel(title);
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleRow.AddChild(titleLabel);

        var previousButton = CreateActionButton("◀");
        previousButton.CustomMinimumSize = new Vector2(48, 30);
        previousButton.Pressed += onPrevious;
        titleRow.AddChild(previousButton);

        var nextButton = CreateActionButton("▶");
        nextButton.CustomMinimumSize = new Vector2(48, 30);
        nextButton.Pressed += onNext;
        titleRow.AddChild(nextButton);

        valueLabel = CreateSummaryLabel();
        valueLabel.AddThemeFontSizeOverride("font_size", 14);
        valueLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.91f, 0.79f));
        rowColumn.AddChild(valueLabel);

        hintLabel = CreateSummaryLabel();
        hintLabel.AddThemeColorOverride("font_color", new Color(0.71f, 0.76f, 0.86f));
        rowColumn.AddChild(hintLabel);
    }

    private static Button CreateActionButton(string text)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(88, 36)
        };
        button.AddThemeStyleboxOverride("normal", CreateButtonStyle());
        button.AddThemeStyleboxOverride("hover", CreateButtonStyle(new Color(0.15f, 0.17f, 0.22f, 1f)));
        button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(new Color(0.12f, 0.14f, 0.18f, 1f)));
        button.AddThemeStyleboxOverride("focus", CreateButtonStyle(new Color(0.15f, 0.17f, 0.22f, 1f)));
        button.AddThemeStyleboxOverride("disabled", CreateButtonStyle(new Color(0.09f, 0.10f, 0.13f, 0.85f)));
        return button;
    }

    private static StyleBoxFlat CreatePanelStyle(Color? backgroundColor = null)
    {
        return new StyleBoxFlat
        {
            BgColor = backgroundColor ?? new Color(0.10f, 0.11f, 0.14f, 1f),
            BorderColor = new Color(0.20f, 0.22f, 0.28f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusBottomLeft = 6
        };
    }

    private static StyleBoxFlat CreateButtonStyle(Color? backgroundColor = null)
    {
        return new StyleBoxFlat
        {
            BgColor = backgroundColor ?? new Color(0.11f, 0.12f, 0.16f, 1f),
            BorderColor = new Color(0.20f, 0.22f, 0.28f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4
        };
    }
}
