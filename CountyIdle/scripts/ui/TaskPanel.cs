using System;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class TaskPanel : PopupPanelBase
{
    private static readonly Color PaperMainColor = new(0.95f, 0.92f, 0.84f, 1f);
    private static readonly Color PaperDarkColor = new(0.89f, 0.85f, 0.76f, 1f);
    private static readonly Color InkMainColor = new(0.17f, 0.15f, 0.13f, 1f);
    private static readonly Color InkMutedColor = new(0.42f, 0.37f, 0.33f, 1f);
    private static readonly Color SealRedColor = new(0.65f, 0.16f, 0.16f, 1f);
    private static readonly Color BorderInkColor = new(0.29f, 0.25f, 0.21f, 1f);
    private static readonly Color AccentGoldColor = new(0.72f, 0.53f, 0.04f, 1f);

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
            ? "宗主只定治宗法旨，执事层会依卷落实人手与资源。"
            : "宗主只定外务倾向，对外往来仍只认灵石结算。";
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
            CustomMinimumSize = new Vector2(1000, 690)
        };
        center.AddChild(wrapper);

        var topTrim = new ColorRect
        {
            Color = new Color(0.83f, 0.69f, 0.21f, 1f)
        };
        topTrim.SetAnchorsPreset(LayoutPreset.TopWide);
        topTrim.OffsetLeft = 12;
        topTrim.OffsetTop = 12;
        topTrim.OffsetRight = -12;
        topTrim.OffsetBottom = 22;
        wrapper.AddChild(topTrim);

        var bottomTrim = new ColorRect
        {
            Color = new Color(0.83f, 0.69f, 0.21f, 1f)
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

        var paperPanel = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        paperPanel.AddThemeStyleboxOverride("panel", CreatePaperStyle());
        frameRow.AddChild(paperPanel);

        var rightRoller = new PanelContainer
        {
            CustomMinimumSize = new Vector2(24, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        rightRoller.AddThemeStyleboxOverride("panel", CreateRollerStyle());
        frameRow.AddChild(rightRoller);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 24);
        margin.AddThemeConstantOverride("margin_top", 24);
        margin.AddThemeConstantOverride("margin_right", 24);
        margin.AddThemeConstantOverride("margin_bottom", 22);
        paperPanel.AddChild(margin);

        var rootColumn = new VBoxContainer();
        rootColumn.AddThemeConstantOverride("separation", 14);
        margin.AddChild(rootColumn);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", 10);
        rootColumn.AddChild(headerRow);

        var titleLabel = new Label
        {
            Text = "浮云宗·治宗册"
        };
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleLabel.AddThemeFontSizeOverride("font_size", 26);
        titleLabel.AddThemeColorOverride("font_color", InkMainColor);
        headerRow.AddChild(titleLabel);

        _closeButton = CreateActionButton("✖", compact: true, inkOnly: true);
        _closeButton.Pressed += ClosePopup;
        headerRow.AddChild(_closeButton);

        var divider = new ColorRect
        {
            CustomMinimumSize = new Vector2(0, 1),
            Color = BorderInkColor
        };
        rootColumn.AddChild(divider);

        var summaryPanel = new PanelContainer();
        summaryPanel.AddThemeStyleboxOverride("panel", CreateNoteStyle(PaperDarkColor));
        rootColumn.AddChild(summaryPanel);

        var summaryMargin = new MarginContainer();
        summaryMargin.AddThemeConstantOverride("margin_left", 12);
        summaryMargin.AddThemeConstantOverride("margin_top", 10);
        summaryMargin.AddThemeConstantOverride("margin_right", 12);
        summaryMargin.AddThemeConstantOverride("margin_bottom", 10);
        summaryPanel.AddChild(summaryMargin);

        var summaryColumn = new VBoxContainer();
        summaryColumn.AddThemeConstantOverride("separation", 6);
        summaryMargin.AddChild(summaryColumn);

        var summaryTitle = CreateSummaryLabel("卷首批注");
        summaryTitle.AddThemeFontSizeOverride("font_size", 15);
        summaryTitle.AddThemeColorOverride("font_color", InkMainColor);
        summaryColumn.AddChild(summaryTitle);

        _spiritStoneLabel = CreateSummaryLabel();
        _contributionLabel = CreateSummaryLabel();
        _assignmentLabel = CreateSummaryLabel();
        _tradeRuleLabel = CreateSummaryLabel();
        summaryColumn.AddChild(_spiritStoneLabel);
        summaryColumn.AddChild(_contributionLabel);
        summaryColumn.AddChild(_assignmentLabel);
        summaryColumn.AddChild(_tradeRuleLabel);

        var governancePanel = new PanelContainer();
        governancePanel.AddThemeStyleboxOverride("panel", CreateNoteStyle(new Color(PaperMainColor.R, PaperMainColor.G, PaperMainColor.B, 0.92f)));
        rootColumn.AddChild(governancePanel);

        var governanceMargin = new MarginContainer();
        governanceMargin.AddThemeConstantOverride("margin_left", 12);
        governanceMargin.AddThemeConstantOverride("margin_top", 10);
        governanceMargin.AddThemeConstantOverride("margin_right", 12);
        governanceMargin.AddThemeConstantOverride("margin_bottom", 10);
        governancePanel.AddChild(governanceMargin);

        var governanceColumn = new VBoxContainer();
        governanceColumn.AddThemeConstantOverride("separation", 10);
        governanceMargin.AddChild(governanceColumn);

        var governanceTitle = CreateSummaryLabel("治宗法旨");
        governanceTitle.AddThemeFontSizeOverride("font_size", 15);
        governanceTitle.AddThemeColorOverride("font_color", InkMainColor);
        governanceColumn.AddChild(governanceTitle);
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
        contentSplit.SplitOffsets = new[] { 338 };
        rootColumn.AddChild(contentSplit);

        var listPanel = new PanelContainer();
        listPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        listPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        listPanel.AddThemeStyleboxOverride("panel", CreateNoteStyle());
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

        var listTitle = CreateSummaryLabel("治务条目");
        listTitle.AddThemeFontSizeOverride("font_size", 14);
        listColumn.AddChild(listTitle);

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
        listColumn.AddChild(_taskList);

        var detailPanel = new PanelContainer();
        detailPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        detailPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detailPanel.AddThemeStyleboxOverride("panel", CreateNoteStyle());
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
        _detailLabel.AddThemeColorOverride("font_color", InkMainColor);
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

        _resetButton = CreateActionButton("复归常制");
        _resetButton.Pressed += OnResetPressed;
        actionRow.AddChild(_resetButton);

        var footerCloseButton = CreateActionButton("收卷");
        footerCloseButton.Pressed += ClosePopup;
        actionRow.AddChild(footerCloseButton);

        _hintLabel = CreateSummaryLabel();
        _hintLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _hintLabel.AddThemeColorOverride("font_color", InkMutedColor);
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

        _spiritStoneLabel.Text = $"灵石：{spiritStone:N0}";
        _contributionLabel.Text = $"宗门贡献：{contribution:N0}";
        _assignmentLabel.Text = $"当前治宗重心：{SectTaskRules.BuildGovernanceHeadline(_state)}";
        _tradeRuleLabel.Text = $"执事落实：{SectTaskRules.BuildGovernanceExecutionSummary(_state)} · 当前{quarterLabel}法令：{quarterDecree.DisplayName} · {SectRuleTreeRules.BuildActiveRuleSummary(_state)} · 峰内内务走贡献点与灵石双轨，峰外往来只认灵石。";
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

    private static Label CreateSummaryLabel(string text = "")
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", 13);
        label.AddThemeColorOverride("font_color", InkMainColor);
        return label;
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
        rowPanel.AddThemeStyleboxOverride("panel", CreateNoteStyle(new Color(PaperDarkColor.R, PaperDarkColor.G, PaperDarkColor.B, 0.82f)));
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
        titleLabel.AddThemeColorOverride("font_color", InkMainColor);
        titleRow.AddChild(titleLabel);

        var previousButton = CreateActionButton("前令", compact: true);
        previousButton.CustomMinimumSize = new Vector2(58, 30);
        previousButton.Pressed += onPrevious;
        titleRow.AddChild(previousButton);

        var nextButton = CreateActionButton("后令", compact: true);
        nextButton.CustomMinimumSize = new Vector2(58, 30);
        nextButton.Pressed += onNext;
        titleRow.AddChild(nextButton);

        valueLabel = CreateSummaryLabel();
        valueLabel.AddThemeFontSizeOverride("font_size", 14);
        valueLabel.AddThemeColorOverride("font_color", SealRedColor);
        rowColumn.AddChild(valueLabel);

        hintLabel = CreateSummaryLabel();
        hintLabel.AddThemeColorOverride("font_color", InkMutedColor);
        rowColumn.AddChild(hintLabel);
    }

    private static Button CreateActionButton(string text, bool compact = false, bool inkOnly = false)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = compact ? new Vector2(64, 32) : new Vector2(96, 36)
        };
        button.Flat = true;
        button.Alignment = HorizontalAlignment.Left;
        button.AddThemeFontSizeOverride("font_size", compact ? 12 : 14);
        if (inkOnly)
        {
            button.Alignment = HorizontalAlignment.Center;
            button.AddThemeStyleboxOverride("normal", CreateTransparentStyle());
            button.AddThemeStyleboxOverride("hover", CreateTransparentStyle());
            button.AddThemeStyleboxOverride("pressed", CreateTransparentStyle());
            button.AddThemeStyleboxOverride("focus", CreateTransparentStyle());
            button.AddThemeColorOverride("font_color", InkMainColor);
            button.AddThemeColorOverride("font_hover_color", SealRedColor);
            button.AddThemeColorOverride("font_pressed_color", SealRedColor);
            return button;
        }

        button.AddThemeStyleboxOverride("normal", CreateOrderButtonStyle(false));
        button.AddThemeStyleboxOverride("hover", CreateOrderButtonHoverStyle(false));
        button.AddThemeStyleboxOverride("pressed", CreateOrderButtonHoverStyle(false));
        button.AddThemeStyleboxOverride("focus", CreateOrderButtonHoverStyle(false));
        button.AddThemeStyleboxOverride("disabled", CreateOrderButtonStyle(true));
        button.AddThemeColorOverride("font_color", InkMainColor);
        button.AddThemeColorOverride("font_hover_color", PaperMainColor);
        button.AddThemeColorOverride("font_pressed_color", PaperMainColor);
        button.AddThemeColorOverride("font_disabled_color", InkMutedColor);
        return button;
    }

    private static StyleBoxFlat CreatePaperStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = PaperMainColor,
            BorderColor = new Color(0.48f, 0.42f, 0.35f, 0.45f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ShadowColor = new Color(0f, 0f, 0f, 0.35f),
            ShadowSize = 10
        };
    }

    private static StyleBoxFlat CreateRollerStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.29f, 0.19f, 0.13f, 1f),
            BorderColor = new Color(0.14f, 0.09f, 0.05f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1
        };
    }

    private static StyleBoxFlat CreateNoteStyle(Color? backgroundColor = null)
    {
        return new StyleBoxFlat
        {
            BgColor = backgroundColor ?? PaperDarkColor,
            BorderColor = new Color(0.64f, 0.58f, 0.50f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1
        };
    }

    private static StyleBoxFlat CreateOrderButtonStyle(bool disabled)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(PaperMainColor.R, PaperMainColor.G, PaperMainColor.B, 0f),
            BorderColor = disabled ? InkMutedColor : BorderInkColor,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ContentMarginLeft = 12,
            ContentMarginTop = 10,
            ContentMarginRight = 12,
            ContentMarginBottom = 10
        };
    }

    private static StyleBoxFlat CreateOrderButtonHoverStyle(bool disabled)
    {
        if (disabled)
        {
            return CreateOrderButtonStyle(true);
        }

        return new StyleBoxFlat
        {
            BgColor = InkMainColor,
            BorderColor = InkMainColor,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ContentMarginLeft = 12,
            ContentMarginTop = 10,
            ContentMarginRight = 12,
            ContentMarginBottom = 10
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
        return new StyleBoxEmpty
        {
        };
    }
}
