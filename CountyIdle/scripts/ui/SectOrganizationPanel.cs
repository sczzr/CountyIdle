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

	private sealed class PeakNavBinding
	{
		public PeakNavBinding(int peakIndex, PanelContainer root, Label titleLabel, Label summaryLabel)
		{
			PeakIndex = peakIndex;
			Root = root;
			TitleLabel = titleLabel;
			SummaryLabel = summaryLabel;
		}

		public int PeakIndex { get; }

		public PanelContainer Root { get; }

		public Label TitleLabel { get; }

		public Label SummaryLabel { get; }
	}

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
	private readonly List<PeakNavBinding> _peakNavItems = new();

	private Label _headerStatusLabel = null!;
	private Label _hintLabel = null!;
	private Label _peakTitleLabel = null!;
	private Label _peakCounterLabel = null!;
	private Label _peakPositionLabel = null!;
	private Label _peakFocusLabel = null!;
	private Label _peakCoreUnitsLabel = null!;
	private Label _peakSupportActiveLabel = null!;
	private Label _peakSupportCandidateLabel = null!;
	private GridContainer _departmentGrid = null!;
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
		return $"当前浏览【{peakTitle}】。左侧可切换峰脉，右侧职司导览可定位关联峰脉并下发协同峰令。当前协同：{supportStatus}。按 Esc 可收卷。";
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

		var outerMargin = new MarginContainer();
		outerMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		outerMargin.AddThemeConstantOverride("margin_left", 36);
		outerMargin.AddThemeConstantOverride("margin_top", 28);
		outerMargin.AddThemeConstantOverride("margin_right", 36);
		outerMargin.AddThemeConstantOverride("margin_bottom", 28);
		overlay.AddChild(outerMargin);

		var wrapper = new Control
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		wrapper.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		outerMargin.AddChild(wrapper);

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

		var paperMargin = CreateMarginContainer(26, 24, 26, 22);
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

		var statusColumn = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(280, 0)
		};
		statusColumn.AddThemeConstantOverride("separation", 4);
		headerRow.AddChild(statusColumn);

		var statusTitle = new Label
		{
			Text = "当前全局协同：",
			HorizontalAlignment = HorizontalAlignment.Right
		};
		statusTitle.AddThemeColorOverride("font_color", InkGrayColor);
		statusTitle.AddThemeFontSizeOverride("font_size", 12);
		statusColumn.AddChild(statusTitle);

		_headerStatusLabel = new Label
		{
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center
		};
		_headerStatusLabel.AddThemeColorOverride("font_color", CinnabarColor);
		_headerStatusLabel.AddThemeFontSizeOverride("font_size", 13);
		statusColumn.AddChild(_headerStatusLabel);

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
		bodyRow.AddThemeConstantOverride("separation", 20);
		rootColumn.AddChild(bodyRow);

		var leftColumn = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(250, 0),
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		leftColumn.AddThemeConstantOverride("separation", 12);
		bodyRow.AddChild(leftColumn);

		leftColumn.AddChild(CreateSectionLabel("九峰总览"));

		var peakListScroll = new ScrollContainer
		{
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		peakListScroll.AddThemeStyleboxOverride("panel", CreateTransparentStyle());
		leftColumn.AddChild(peakListScroll);

		var peakListColumn = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		peakListColumn.AddThemeConstantOverride("separation", 8);
		peakListScroll.AddChild(peakListColumn);

		var peakCount = SectOrganizationRules.GetPeakCount();
		for (var index = 0; index < peakCount; index++)
		{
			var binding = CreatePeakNavItem(index);
			_peakNavItems.Add(binding);
			peakListColumn.AddChild(binding.Root);
		}

		var middleColumn = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		middleColumn.AddThemeConstantOverride("separation", 12);
		bodyRow.AddChild(middleColumn);

		var middleScroll = new ScrollContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		middleScroll.AddThemeStyleboxOverride("panel", CreateTransparentStyle());
		middleColumn.AddChild(middleScroll);

		var middleContent = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		middleContent.AddThemeConstantOverride("separation", 16);
		middleScroll.AddChild(middleContent);

		var detailHeaderRow = new HBoxContainer();
		detailHeaderRow.AddThemeConstantOverride("separation", 10);
		middleContent.AddChild(detailHeaderRow);

		_peakTitleLabel = new Label
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		_peakTitleLabel.AddThemeColorOverride("font_color", InkBlackColor);
		_peakTitleLabel.AddThemeFontSizeOverride("font_size", 36);
		detailHeaderRow.AddChild(_peakTitleLabel);

		_peakCounterLabel = new Label
		{
			CustomMinimumSize = new Vector2(64, 0),
			HorizontalAlignment = HorizontalAlignment.Right
		};
		_peakCounterLabel.AddThemeColorOverride("font_color", CinnabarColor);
		_peakCounterLabel.AddThemeFontSizeOverride("font_size", 13);
		detailHeaderRow.AddChild(_peakCounterLabel);

		var middleDivider = new ColorRect
		{
			CustomMinimumSize = new Vector2(0, 1),
			Color = new Color(0.45f, 0.40f, 0.33f, 0.55f)
		};
		middleContent.AddChild(middleDivider);

		var summaryPanel = new PanelContainer();
		summaryPanel.AddThemeStyleboxOverride("panel", CreateTonePanelStyle(new Color(0.96f, 0.93f, 0.86f, 0.55f)));
		middleContent.AddChild(summaryPanel);

		var summaryMargin = CreateMarginContainer(16, 14, 16, 14);
		summaryPanel.AddChild(summaryMargin);

		var summaryColumn = new VBoxContainer();
		summaryColumn.AddThemeConstantOverride("separation", 8);
		summaryMargin.AddChild(summaryColumn);

		summaryColumn.AddChild(CreateKeyValueRow("峰脉定位", out _peakPositionLabel));
		summaryColumn.AddChild(CreateKeyValueRow("经营焦点", out _peakFocusLabel, highlightValue: true));
		summaryColumn.AddChild(CreateKeyValueRow("核心机构", out _peakCoreUnitsLabel));
		summaryColumn.AddChild(CreateKeyValueRow("法旨效力", out _peakSupportActiveLabel));
		summaryColumn.AddChild(CreateKeyValueRow("候补加成", out _peakSupportCandidateLabel));

		middleContent.AddChild(CreateSectionLabel("附属部门与处室"));

		_departmentGrid = new GridContainer
		{
			Columns = 2,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		_departmentGrid.AddThemeConstantOverride("h_separation", 12);
		_departmentGrid.AddThemeConstantOverride("v_separation", 12);
		middleContent.AddChild(_departmentGrid);

		var rightColumn = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(320, 0),
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		rightColumn.AddThemeConstantOverride("separation", 12);
		bodyRow.AddChild(rightColumn);

		rightColumn.AddChild(CreateSectionLabel("职司导览"));

		var jobCardsContainer = new VBoxContainer
		{
			SizeFlagsVertical = Control.SizeFlags.ExpandFill
		};
		jobCardsContainer.AddThemeConstantOverride("separation", 10);
		rightColumn.AddChild(jobCardsContainer);

		foreach (var jobType in JobOrder)
		{
			var binding = CreateJobCard(jobType);
			_jobCards[jobType] = binding;
			jobCardsContainer.AddChild(binding.Root);
		}

		var actionDivider = new ColorRect
		{
			CustomMinimumSize = new Vector2(0, 1),
			Color = new Color(0.70f, 0.65f, 0.56f, 0.48f)
		};
		rightColumn.AddChild(actionDivider);

		var actionColumn = new VBoxContainer();
		actionColumn.AddThemeConstantOverride("separation", 8);
		rightColumn.AddChild(actionColumn);

		_setSupportButton = CreateActionButton("立 协同峰令", InkBlackColor, emphasize: true);
		_setSupportButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_setSupportButton.Pressed += OnSetSupportPressed;
		actionColumn.AddChild(_setSupportButton);

		var actionRow = new HBoxContainer();
		actionRow.AddThemeConstantOverride("separation", 8);
		actionColumn.AddChild(actionRow);

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
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			HorizontalAlignment = HorizontalAlignment.Center
		};
		_hintLabel.AddThemeColorOverride("font_color", InkGrayColor);
		_hintLabel.AddThemeFontSizeOverride("font_size", 12);
		rightColumn.AddChild(_hintLabel);
	}

	private PeakNavBinding CreatePeakNavItem(int peakIndex)
	{
		var card = new PanelContainer
		{
			MouseFilter = MouseFilterEnum.Stop,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		card.AddThemeStyleboxOverride("panel", CreatePeakNavStyle(selected: false));
		card.MouseDefaultCursorShape = CursorShape.PointingHand;
		card.GuiInput += @event => OnPeakNavInput(peakIndex, @event);

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

		return new PeakNavBinding(peakIndex, card, titleLabel, summaryLabel);
	}

	private static HBoxContainer CreateKeyValueRow(string labelText, out Label valueLabel, bool highlightValue = false)
	{
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 8);

		var label = new Label
		{
			Text = labelText,
			CustomMinimumSize = new Vector2(88, 0)
		};
		label.AddThemeColorOverride("font_color", InkGrayColor);
		label.AddThemeFontSizeOverride("font_size", 12);
		row.AddChild(label);

		valueLabel = new Label
		{
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		valueLabel.AddThemeColorOverride("font_color", highlightValue ? CinnabarColor : InkBlackColor);
		valueLabel.AddThemeFontSizeOverride("font_size", 13);
		row.AddChild(valueLabel);

		return row;
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

	private void OnPeakNavInput(int peakIndex, InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseButton ||
			!mouseButton.Pressed ||
			mouseButton.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		_selectedPeakIndex = peakIndex;
		RefreshPeakDetail();
		RefreshPopupHint();
		GetViewport().SetInputAsHandled();
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

	private void RefreshOverview()
	{
		var activeSupport = SectPeakSupportRules.GetActiveDefinition(_state);
		_headerStatusLabel.Text = $"当前协同：{activeSupport.DisplayName}｜{activeSupport.ShortEffect}";
		_headerStatusLabel.TooltipText = activeSupport.Description;
	}

	private void RefreshPeakNav()
	{
		foreach (var binding in _peakNavItems)
		{
			var profile = SectOrganizationRules.GetPeakProfile(binding.PeakIndex);
			binding.TitleLabel.Text = profile.IsCurrentPlayableFocus ? $"{profile.Name}（当前）" : profile.Name;
			binding.SummaryLabel.Text = profile.CoreUnits;
			binding.Root.TooltipText = SectOrganizationRules.BuildPeakDetailText(binding.PeakIndex);

			var selected = binding.PeakIndex == _selectedPeakIndex;
			binding.Root.AddThemeStyleboxOverride("panel", CreatePeakNavStyle(selected));
			binding.TitleLabel.AddThemeColorOverride("font_color", selected ? CinnabarColor : InkBlackColor);
		}
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

	private void RefreshDepartmentCards(string departmentDetails)
	{
		ClearChildren(_departmentGrid);

		foreach (var entry in ParseDepartmentDetails(departmentDetails))
		{
			_departmentGrid.AddChild(CreateDepartmentCard(entry.Name, entry.Detail));
		}
	}

	private void RefreshPeakDetail()
	{
		var peakCount = SectOrganizationRules.GetPeakCount();
		_selectedPeakIndex = SectOrganizationRules.NormalizePeakIndex(_selectedPeakIndex);

		var profile = SectOrganizationRules.GetPeakProfile(_selectedPeakIndex);
		var selectedSupportType = SectOrganizationRules.GetSupportTypeForPeakIndex(_selectedPeakIndex);
		var selectedSupportDefinition = SectPeakSupportRules.GetDefinition(selectedSupportType);
		var activeSupport = SectPeakSupportRules.GetActiveSupport(_state);
		var activeSupportDefinition = SectPeakSupportRules.GetActiveDefinition(_state);

		_peakTitleLabel.Text = profile.Name;
		_peakCounterLabel.Text = $"{_selectedPeakIndex + 1}/{peakCount}";
		_peakPositionLabel.Text = profile.IsCurrentPlayableFocus ? $"{profile.Positioning}（当前）" : profile.Positioning;
		_peakFocusLabel.Text = profile.Responsibility;
		_peakCoreUnitsLabel.Text = profile.CoreUnits;
		_peakSupportActiveLabel.Text = $"{activeSupportDefinition.DisplayName}｜{activeSupportDefinition.ModifierSummary}";
		_peakSupportCandidateLabel.Text = $"{selectedSupportDefinition.DisplayName}｜{selectedSupportDefinition.ModifierSummary}";
		RefreshDepartmentCards(profile.DepartmentDetails);

		var isCurrentSupport = activeSupport == selectedSupportType;
		_setSupportButton.Text = isCurrentSupport ? "当前已协同" : $"立 {selectedSupportDefinition.DisplayName} 协同";
		_setSupportButton.Disabled = isCurrentSupport;
		_setSupportButton.TooltipText = selectedSupportDefinition.Description;

		var isBalanced = activeSupport == SectPeakSupportType.Balanced;
		_resetSupportButton.Disabled = isBalanced;
		_resetSupportButton.TooltipText = "恢复诸峰均衡轮转，不再额外偏置单峰支援。";

		_openGovernanceButton.Text = $"转 {SectTaskRules.GetJobButtonText(_selectedJobType)}";
		_openGovernanceButton.TooltipText = SectTaskRules.GetJobPanelInfo(_state, _selectedJobType).DetailText;
		RefreshPeakNav();
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

	private readonly record struct DepartmentEntry(string Name, string Detail);

	private static IEnumerable<DepartmentEntry> ParseDepartmentDetails(string departmentDetails)
	{
		if (string.IsNullOrWhiteSpace(departmentDetails))
		{
			yield break;
		}

		var lines = departmentDetails.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		foreach (var rawLine in lines)
		{
			var line = rawLine.Trim();
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			var parts = line.Split(new[] { '：' }, 2, StringSplitOptions.None);
			if (parts.Length == 1)
			{
				parts = line.Split(new[] { ':' }, 2, StringSplitOptions.None);
			}

			var name = parts[0].Trim();
			var detail = parts.Length > 1 ? parts[1].Trim() : string.Empty;
			if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(detail))
			{
				continue;
			}

			yield return new DepartmentEntry(name, detail);
		}
	}

	private static PanelContainer CreateDepartmentCard(string title, string detail)
	{
		var card = new PanelContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		card.AddThemeStyleboxOverride("panel", CreateDepartmentCardStyle());

		var margin = CreateMarginContainer(12, 10, 12, 10);
		card.AddChild(margin);

		var column = new VBoxContainer();
		column.AddThemeConstantOverride("separation", 4);
		margin.AddChild(column);

		var titleLabel = new Label
		{
			Text = title
		};
		titleLabel.AddThemeColorOverride("font_color", InkBlackColor);
		titleLabel.AddThemeFontSizeOverride("font_size", 14);
		column.AddChild(titleLabel);

		var detailLabel = new Label
		{
			Text = detail,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		detailLabel.AddThemeColorOverride("font_color", InkGrayColor);
		detailLabel.AddThemeFontSizeOverride("font_size", 12);
		column.AddChild(detailLabel);

		return card;
	}

	private static void ClearChildren(Node parent)
	{
		foreach (var child in parent.GetChildren())
		{
			parent.RemoveChild(child);
			child.QueueFree();
		}
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
		button.AddThemeFontSizeOverride("font_size", emphasize ? 15 : 13);

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

	private static StyleBoxFlat CreatePeakNavStyle(bool selected)
	{
		return new StyleBoxFlat
		{
			BgColor = selected ? new Color(0.93f, 0.88f, 0.80f, 0.70f) : new Color(1f, 1f, 1f, 0f),
			BorderColor = selected ? CinnabarColor : new Color(0.70f, 0.65f, 0.56f, 0.35f),
			BorderWidthLeft = selected ? 3 : 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0
		};
	}

	private static StyleBoxFlat CreateDepartmentCardStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(0.98f, 0.96f, 0.90f, 0.55f),
			BorderColor = new Color(0.70f, 0.65f, 0.56f, 0.45f),
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
			BgColor = selected ? new Color(0.95f, 0.90f, 0.82f, 0.85f) : new Color(0.98f, 0.96f, 0.90f, 0.45f),
			BorderColor = selected ? new Color(0.62f, 0.16f, 0.13f, 0.82f) : new Color(0.70f, 0.65f, 0.56f, 0.45f),
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
		var style = new StyleBoxFlat
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

		if (emphasize)
		{
			style.ShadowSize = filled ? 0 : 3;
			style.ShadowOffset = filled ? Vector2.Zero : new Vector2(2, 2);
			style.ShadowColor = new Color(0f, 0f, 0f, 0.25f);
		}

		return style;
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
