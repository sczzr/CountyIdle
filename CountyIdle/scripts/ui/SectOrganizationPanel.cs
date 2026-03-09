using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class SectOrganizationPanel : PopupPanelBase
{
    private static readonly Color PaperBackgroundColor = new(0.956f, 0.945f, 0.918f, 1f);
    private static readonly Color InkBlackColor = new(0.173f, 0.145f, 0.125f, 1f);
    private static readonly Color InkGrayColor = new(0.404f, 0.353f, 0.302f, 0.95f);
    private static readonly Color CinnabarColor = new(0.620f, 0.165f, 0.133f, 1f);
    private static readonly Color BorderGoldColor = new(0.773f, 0.627f, 0.349f, 1f);
    private static readonly Color CeladonColor = new(0.439f, 0.553f, 0.506f, 1f);

    private static readonly JobType[] JobOrder =
    [
        JobType.Farmer,
        JobType.Worker,
        JobType.Merchant,
        JobType.Scholar
    ];

    private sealed class JobCardBinding
    {
        public JobCardBinding(PanelContainer root, Label titleLabel, Label summaryLabel, Label detailLabel)
        {
            Root = root;
            TitleLabel = titleLabel;
            SummaryLabel = summaryLabel;
            DetailLabel = detailLabel;
        }

        public PanelContainer Root { get; }

        public Label TitleLabel { get; }

        public Label SummaryLabel { get; }

        public Label DetailLabel { get; }
    }

    public event Action<SectPeakSupportType>? SupportRequested;
    public event Action? SupportResetRequested;
    public event Action<JobType>? GovernanceRequested;

    private readonly Dictionary<JobType, JobCardBinding> _jobCards = new();

    private Label _headerStatusLabel = null!;
    private RichTextLabel _overviewLabel = null!;
    private Label _hintLabel = null!;
    private Label _peakTitleLabel = null!;
    private Label _peakCounterLabel = null!;
    private Label _peakSummaryLabel = null!;
    private RichTextLabel _peakDetailLabel = null!;
    private Label _peakSupportStatusLabel = null!;
    private Button _prevButton = null!;
    private Button _nextButton = null!;
    private Button _setSupportButton = null!;
    private Button _resetSupportButton = null!;
    private Button _openGovernanceButton = null!;
    private Button _closeButton = null!;

    private GameState _state = new();
    private JobType _selectedJobType = JobType.Scholar;
    private int _selectedPeakIndex = SectOrganizationRules.GetDefaultPeakIndex();

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

    public void Open(GameState state, JobType? preferredJobType = null, int? preferredPeakIndex = null)
    {
        RefreshState(state, preferredJobType, preferredPeakIndex);
        OpenPopup();
    }

    public void RefreshState(GameState state, JobType? preferredJobType = null, int? preferredPeakIndex = null)
    {
        _state = state.Clone();
        SectPeakSupportRules.EnsureDefaults(_state);

        if (preferredJobType.HasValue)
        {
            _selectedJobType = preferredJobType.Value;
            _selectedPeakIndex = SectOrganizationRules.GetRecommendedPeakIndex(preferredJobType.Value);
        }
        else if (preferredPeakIndex.HasValue)
        {
            _selectedPeakIndex = SectOrganizationRules.NormalizePeakIndex(preferredPeakIndex.Value);
        }
        else
        {
            _selectedPeakIndex = SectOrganizationRules.NormalizePeakIndex(_selectedPeakIndex);
        }

        RefreshOverview();
        RefreshJobCards();
        RefreshPeakDetail();
        RefreshPopupHint();
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        var peakTitle = SectOrganizationRules.GetPeakTitle(_selectedPeakIndex);
        var supportStatus = SectPeakSupportRules.BuildActiveSupportStatus(_state);
        return $"当前浏览【{peakTitle}】。可按四条职司定位关联峰脉，并直接下发协同峰令。当前协同：{supportStatus}。按 Esc 可收卷。";
    }

    private void BuildUi()
    {
        MouseFilter = MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var overlay = new ColorRect
        {
            Color = new Color(0.10f, 0.09f, 0.08f, 0.92f),
            MouseFilter = MouseFilterEnum.Stop
        };
        overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(overlay);

        var center = new CenterContainer
        {
            MouseFilter = MouseFilterEnum.Ignore
        };
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        overlay.AddChild(center);

        var wrapper = new Control
        {
            CustomMinimumSize = new Vector2(1180, 720)
        };
        center.AddChild(wrapper);

        var topTrim = new ColorRect
        {
            Color = BorderGoldColor
        };
        topTrim.SetAnchorsPreset(LayoutPreset.TopWide);
        topTrim.OffsetLeft = 12;
        topTrim.OffsetTop = 12;
        topTrim.OffsetRight = -12;
        topTrim.OffsetBottom = 22;
        wrapper.AddChild(topTrim);

        var bottomTrim = new ColorRect
        {
            Color = BorderGoldColor
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

        var paperFrame = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        paperFrame.AddThemeStyleboxOverride("panel", CreatePaperStyle());
        frameRow.AddChild(paperFrame);

        var rightRoller = new PanelContainer
        {
            CustomMinimumSize = new Vector2(24, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        rightRoller.AddThemeStyleboxOverride("panel", CreateRollerStyle());
        frameRow.AddChild(rightRoller);

        var paperMargin = CreateMarginContainer(24, 24, 24, 22);
        paperFrame.AddChild(paperMargin);

        var rootColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        rootColumn.AddThemeConstantOverride("separation", 16);
        paperMargin.AddChild(rootColumn);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", 14);
        rootColumn.AddChild(headerRow);

        var titleColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        titleColumn.AddThemeConstantOverride("separation", 4);
        headerRow.AddChild(titleColumn);

        var titleLabel = new Label
        {
            Text = "浮云宗·峰令谱",
            Modulate = Colors.White
        };
        titleLabel.AddThemeColorOverride("font_color", InkBlackColor);
        titleLabel.AddThemeFontSizeOverride("font_size", 28);
        titleColumn.AddChild(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "九峰、总殿与支柱峰职责尽录于卷，并可在此批复协同峰令。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        subtitleLabel.AddThemeColorOverride("font_color", InkGrayColor);
        subtitleLabel.AddThemeFontSizeOverride("font_size", 13);
        titleColumn.AddChild(subtitleLabel);

        _headerStatusLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(280, 0)
        };
        _headerStatusLabel.AddThemeColorOverride("font_color", CinnabarColor);
        _headerStatusLabel.AddThemeFontSizeOverride("font_size", 13);
        headerRow.AddChild(_headerStatusLabel);

        _closeButton = CreateCloseButton("✖");
        _closeButton.Pressed += ClosePopup;
        headerRow.AddChild(_closeButton);

        var divider = new ColorRect
        {
            CustomMinimumSize = new Vector2(0, 1),
            Color = new Color(0.70f, 0.65f, 0.56f, 0.48f)
        };
        rootColumn.AddChild(divider);

        var bodyRow = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        bodyRow.AddThemeConstantOverride("separation", 18);
        rootColumn.AddChild(bodyRow);

        var leftColumn = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(380, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        leftColumn.AddThemeConstantOverride("separation", 14);
        bodyRow.AddChild(leftColumn);

        var overviewPanel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 196)
        };
        overviewPanel.AddThemeStyleboxOverride("panel", CreateTonePanelStyle(new Color(0.91f, 0.87f, 0.79f, 0.66f)));
        leftColumn.AddChild(overviewPanel);

        var overviewMargin = CreateMarginContainer(14, 12, 14, 12);
        overviewPanel.AddChild(overviewMargin);

        var overviewColumn = new VBoxContainer();
        overviewColumn.AddThemeConstantOverride("separation", 8);
        overviewMargin.AddChild(overviewColumn);

        overviewColumn.AddChild(CreateSectionLabel("九峰总览"));

        _overviewLabel = new RichTextLabel
        {
            FitContent = false,
            ScrollActive = true,
            BbcodeEnabled = false,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        _overviewLabel.AddThemeColorOverride("default_color", InkGrayColor);
        overviewColumn.AddChild(_overviewLabel);

        leftColumn.AddChild(CreateSectionLabel("职司导览"));

        var jobCardsContainer = new VBoxContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        jobCardsContainer.AddThemeConstantOverride("separation", 10);
        leftColumn.AddChild(jobCardsContainer);

        foreach (var jobType in JobOrder)
        {
            var binding = CreateJobCard(jobType);
            _jobCards[jobType] = binding;
            jobCardsContainer.AddChild(binding.Root);
        }

        var rightColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        rightColumn.AddThemeConstantOverride("separation", 14);
        bodyRow.AddChild(rightColumn);

        var peakNavPanel = new PanelContainer();
        peakNavPanel.AddThemeStyleboxOverride("panel", CreateTonePanelStyle(new Color(0.92f, 0.89f, 0.82f, 0.70f)));
        rightColumn.AddChild(peakNavPanel);

        var peakNavMargin = CreateMarginContainer(16, 14, 16, 14);
        peakNavPanel.AddChild(peakNavMargin);

        var peakNavColumn = new VBoxContainer();
        peakNavColumn.AddThemeConstantOverride("separation", 10);
        peakNavMargin.AddChild(peakNavColumn);

        var navHeaderRow = new HBoxContainer();
        navHeaderRow.AddThemeConstantOverride("separation", 10);
        peakNavColumn.AddChild(navHeaderRow);

        _peakTitleLabel = new Label
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _peakTitleLabel.AddThemeColorOverride("font_color", InkBlackColor);
        _peakTitleLabel.AddThemeFontSizeOverride("font_size", 26);
        navHeaderRow.AddChild(_peakTitleLabel);

        _peakCounterLabel = new Label
        {
            CustomMinimumSize = new Vector2(52, 0),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        _peakCounterLabel.AddThemeColorOverride("font_color", CinnabarColor);
        _peakCounterLabel.AddThemeFontSizeOverride("font_size", 13);
        navHeaderRow.AddChild(_peakCounterLabel);

        var navButtonRow = new HBoxContainer();
        navButtonRow.AddThemeConstantOverride("separation", 8);
        peakNavColumn.AddChild(navButtonRow);

        _prevButton = CreateActionButton("◀ 上一峰");
        _prevButton.CustomMinimumSize = new Vector2(110, 30);
        _prevButton.Pressed += () => ShiftPeak(-1);
        navButtonRow.AddChild(_prevButton);

        _nextButton = CreateActionButton("下一峰 ▶");
        _nextButton.CustomMinimumSize = new Vector2(110, 30);
        _nextButton.Pressed += () => ShiftPeak(1);
        navButtonRow.AddChild(_nextButton);

        _peakSummaryLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _peakSummaryLabel.AddThemeColorOverride("font_color", InkGrayColor);
        _peakSummaryLabel.AddThemeFontSizeOverride("font_size", 13);
        peakNavColumn.AddChild(_peakSummaryLabel);

        _peakSupportStatusLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _peakSupportStatusLabel.AddThemeColorOverride("font_color", InkGrayColor);
        _peakSupportStatusLabel.AddThemeFontSizeOverride("font_size", 12);
        peakNavColumn.AddChild(_peakSupportStatusLabel);

        var detailPanel = new PanelContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        detailPanel.AddThemeStyleboxOverride("panel", CreateTonePanelStyle(new Color(0.93f, 0.90f, 0.84f, 0.72f)));
        rightColumn.AddChild(detailPanel);

        var detailMargin = CreateMarginContainer(16, 16, 16, 16);
        detailPanel.AddChild(detailMargin);

        _peakDetailLabel = new RichTextLabel
        {
            BbcodeEnabled = false,
            FitContent = false,
            ScrollActive = true,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        _peakDetailLabel.AddThemeColorOverride("default_color", InkGrayColor);
        detailMargin.AddChild(_peakDetailLabel);

        var actionRow = new HBoxContainer();
        actionRow.AddThemeConstantOverride("separation", 10);
        rightColumn.AddChild(actionRow);

        _setSupportButton = CreateActionButton("立 协同峰令");
        _setSupportButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _setSupportButton.Pressed += OnSetSupportPressed;
        actionRow.AddChild(_setSupportButton);

        _resetSupportButton = CreateActionButton("复 均衡轮转", CinnabarColor);
        _resetSupportButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _resetSupportButton.Pressed += OnResetSupportPressed;
        actionRow.AddChild(_resetSupportButton);

        _openGovernanceButton = CreateActionButton("转 宗主中枢", CeladonColor);
        _openGovernanceButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _openGovernanceButton.Pressed += OnOpenGovernancePressed;
        actionRow.AddChild(_openGovernanceButton);

        _hintLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _hintLabel.AddThemeColorOverride("font_color", InkGrayColor);
        _hintLabel.AddThemeFontSizeOverride("font_size", 12);
        rootColumn.AddChild(_hintLabel);
    }

    private JobCardBinding CreateJobCard(JobType jobType)
    {
        var card = new PanelContainer
        {
            MouseFilter = MouseFilterEnum.Stop
        };
        card.AddThemeStyleboxOverride("panel", CreateJobCardStyle(selected: false));
        card.MouseDefaultCursorShape = CursorShape.PointingHand;
        card.GuiInput += @event => OnJobCardInput(jobType, @event);

        var margin = CreateMarginContainer(12, 10, 12, 10);
        card.AddChild(margin);

        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 4);
        margin.AddChild(column);

        var titleLabel = new Label();
        titleLabel.AddThemeColorOverride("font_color", InkBlackColor);
        titleLabel.AddThemeFontSizeOverride("font_size", 16);
        column.AddChild(titleLabel);

        var summaryLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        summaryLabel.AddThemeColorOverride("font_color", InkGrayColor);
        summaryLabel.AddThemeFontSizeOverride("font_size", 12);
        column.AddChild(summaryLabel);

        var detailLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        detailLabel.AddThemeColorOverride("font_color", new Color(0.32f, 0.28f, 0.23f, 0.92f));
        detailLabel.AddThemeFontSizeOverride("font_size", 11);
        column.AddChild(detailLabel);

        return new JobCardBinding(card, titleLabel, summaryLabel, detailLabel);
    }

    private void OnJobCardInput(JobType jobType, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton ||
            !mouseButton.Pressed ||
            mouseButton.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        _selectedJobType = jobType;
        _selectedPeakIndex = SectOrganizationRules.GetRecommendedPeakIndex(jobType);
        RefreshJobCards();
        RefreshPeakDetail();
        RefreshPopupHint();
        GetViewport().SetInputAsHandled();
    }

    private void ShiftPeak(int delta)
    {
        var peakCount = SectOrganizationRules.GetPeakCount();
        if (peakCount <= 0)
        {
            return;
        }

        _selectedPeakIndex = ((_selectedPeakIndex + delta) % peakCount + peakCount) % peakCount;
        RefreshPeakDetail();
        RefreshPopupHint();
    }

    private void RefreshOverview()
    {
        var activeSupport = SectPeakSupportRules.GetActiveDefinition(_state);
        _headerStatusLabel.Text = $"当前协同：{activeSupport.DisplayName}｜{activeSupport.ShortEffect}";
        _headerStatusLabel.TooltipText = activeSupport.Description;
        _overviewLabel.Text = SectOrganizationRules.BuildPeakOverviewText();
    }

    private void RefreshJobCards()
    {
        foreach (var jobType in JobOrder)
        {
            if (!_jobCards.TryGetValue(jobType, out var binding))
            {
                continue;
            }

            var info = SectTaskRules.GetJobPanelInfo(_state, jobType);
            var selected = _selectedJobType == jobType;

            binding.TitleLabel.Text = info.TitleText;
            binding.SummaryLabel.Text = info.SummaryText;
            binding.DetailLabel.Text = $"关联峰脉：{SectOrganizationRules.GetPeakTitle(SectOrganizationRules.GetRecommendedPeakIndex(jobType))}";
            binding.Root.TooltipText = info.DetailText;
            binding.Root.AddThemeStyleboxOverride("panel", CreateJobCardStyle(selected));
        }
    }

    private void RefreshPeakDetail()
    {
        var peakCount = SectOrganizationRules.GetPeakCount();
        _selectedPeakIndex = SectOrganizationRules.NormalizePeakIndex(_selectedPeakIndex);

        var peakTitle = SectOrganizationRules.GetPeakTitle(_selectedPeakIndex);
        var peakSummary = SectOrganizationRules.GetPeakSummary(_selectedPeakIndex);
        var peakDetail = SectOrganizationRules.BuildPeakDetailText(_selectedPeakIndex);
        var selectedSupportType = SectOrganizationRules.GetSupportTypeForPeakIndex(_selectedPeakIndex);
        var selectedSupportDefinition = SectPeakSupportRules.GetDefinition(selectedSupportType);
        var activeSupport = SectPeakSupportRules.GetActiveSupport(_state);
        var activeSupportDefinition = SectPeakSupportRules.GetActiveDefinition(_state);

        _peakTitleLabel.Text = peakTitle;
        _peakCounterLabel.Text = $"{_selectedPeakIndex + 1}/{peakCount}";
        _peakSummaryLabel.Text = peakSummary;
        _peakSummaryLabel.TooltipText = peakDetail;
        _peakDetailLabel.Text = peakDetail;
        _peakSupportStatusLabel.Text =
            $"当前协同：{activeSupportDefinition.DisplayName}｜{activeSupportDefinition.ModifierSummary}\n候选峰令：{selectedSupportDefinition.DisplayName}｜{selectedSupportDefinition.ModifierSummary}";
        _peakSupportStatusLabel.TooltipText = selectedSupportDefinition.Description;

        var isCurrentSupport = activeSupport == selectedSupportType;
        _setSupportButton.Text = isCurrentSupport ? "当前已协同" : $"立 {selectedSupportDefinition.DisplayName} 协同";
        _setSupportButton.Disabled = isCurrentSupport;
        _setSupportButton.TooltipText = selectedSupportDefinition.Description;

        var isBalanced = activeSupport == SectPeakSupportType.Balanced;
        _resetSupportButton.Disabled = isBalanced;
        _resetSupportButton.TooltipText = "恢复诸峰均衡轮转，不再额外偏置单峰支援。";

        _prevButton.Disabled = peakCount <= 1;
        _nextButton.Disabled = peakCount <= 1;

        _openGovernanceButton.Text = $"转 {SectTaskRules.GetJobButtonText(_selectedJobType)}";
        _openGovernanceButton.TooltipText = SectTaskRules.GetJobPanelInfo(_state, _selectedJobType).DetailText;
    }

    private void OnSetSupportPressed()
    {
        var supportType = SectOrganizationRules.GetSupportTypeForPeakIndex(_selectedPeakIndex);
        SupportRequested?.Invoke(supportType);
        ShowPopupStatusMessage($"已请求将【{SectOrganizationRules.GetPeakTitle(_selectedPeakIndex)}】立为本季协同峰。");
    }

    private void OnResetSupportPressed()
    {
        SupportResetRequested?.Invoke();
        ShowPopupStatusMessage("已请求恢复诸峰均衡轮转。");
    }

    private void OnOpenGovernancePressed()
    {
        GovernanceRequested?.Invoke(_selectedJobType);
        ClosePopup();
    }

    private static Label CreateSectionLabel(string text)
    {
        var label = new Label
        {
            Text = text
        };
        label.AddThemeColorOverride("font_color", InkBlackColor);
        label.AddThemeFontSizeOverride("font_size", 16);
        return label;
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

    private static Button CreateActionButton(string text, Color? accentColor = null, bool emphasize = false)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 34)
        };

        var textColor = accentColor ?? InkBlackColor;
        button.AddThemeColorOverride("font_color", textColor);
        button.AddThemeColorOverride("font_hover_color", textColor);
        button.AddThemeColorOverride("font_pressed_color", textColor);
        button.AddThemeColorOverride("font_disabled_color", new Color(textColor.R, textColor.G, textColor.B, 0.50f));
        button.AddThemeFontSizeOverride("font_size", 13);

        var normal = CreateButtonStyle(accentColor ?? new Color(0.27f, 0.23f, 0.19f, 0.82f), emphasize);
        var active = CreateButtonStyle(accentColor ?? new Color(0.27f, 0.23f, 0.19f, 0.82f), emphasize, filled: true);
        var disabled = CreateDisabledButtonStyle(accentColor ?? new Color(0.27f, 0.23f, 0.19f, 0.82f), emphasize);
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", active);
        button.AddThemeStyleboxOverride("pressed", active);
        button.AddThemeStyleboxOverride("focus", active);
        button.AddThemeStyleboxOverride("disabled", disabled);
        return button;
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

    private static StyleBoxFlat CreateTonePanelStyle(Color backgroundColor)
    {
        return new StyleBoxFlat
        {
            BgColor = backgroundColor,
            BorderColor = new Color(0.70f, 0.65f, 0.56f, 0.58f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0
        };
    }

    private static StyleBoxFlat CreateJobCardStyle(bool selected)
    {
        return new StyleBoxFlat
        {
            BgColor = selected ? new Color(0.91f, 0.87f, 0.79f, 0.90f) : new Color(0.94f, 0.91f, 0.84f, 0.68f),
            BorderColor = selected ? new Color(0.62f, 0.16f, 0.13f, 0.82f) : new Color(0.70f, 0.65f, 0.56f, 0.58f),
            BorderWidthLeft = selected ? 2 : 1,
            BorderWidthTop = selected ? 2 : 1,
            BorderWidthRight = selected ? 2 : 1,
            BorderWidthBottom = selected ? 2 : 1,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0
        };
    }

    private static StyleBoxFlat CreateButtonStyle(Color borderColor, bool emphasize, bool filled = false)
    {
        return new StyleBoxFlat
        {
            BgColor = filled
                ? (emphasize ? new Color(0.91f, 0.84f, 0.74f, 0.82f) : new Color(PaperBackgroundColor.R, PaperBackgroundColor.G, PaperBackgroundColor.B, 0.58f))
                : new Color(PaperBackgroundColor.R, PaperBackgroundColor.G, PaperBackgroundColor.B, emphasize ? 0.08f : 0f),
            BorderColor = new Color(borderColor.R, borderColor.G, borderColor.B, emphasize ? 0.86f : 0.72f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 12,
            ContentMarginTop = 7,
            ContentMarginRight = 12,
            ContentMarginBottom = 7
        };
    }

    private static StyleBoxFlat CreateDisabledButtonStyle(Color borderColor, bool emphasize)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(PaperBackgroundColor.R, PaperBackgroundColor.G, PaperBackgroundColor.B, emphasize ? 0.16f : 0.08f),
            BorderColor = new Color(borderColor.R, borderColor.G, borderColor.B, 0.28f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 12,
            ContentMarginTop = 7,
            ContentMarginRight = 12,
            ContentMarginBottom = 7
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
}
