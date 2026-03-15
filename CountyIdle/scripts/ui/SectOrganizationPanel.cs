using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class SectOrganizationPanel : PopupPanelBase
{
	private const string RootPath = "Overlay/OuterMargin/Wrapper/FrameRow/PaperFrame/PaperMargin/RootColumn";
	private const string HeaderPath = RootPath + "/HeaderRow";
	private const string BodyPath = RootPath + "/BodyRow";
	private const string MiddleContentPath = BodyPath + "/MiddleColumn/MiddleScroll/MiddleContent";

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
	private VBoxContainer _peakListColumn = null!;
	private VBoxContainer _jobCardsContainer = null!;
	private Button _setSupportButton = null!;
	private Button _resetSupportButton = null!;
	private Button _openGovernanceButton = null!;
	private Button _closeButton = null!;
	private Node? _visualFx;

	private GameState _state = new();
	private JobType _selectedJobType = JobType.Scholar;
	private int _selectedPeakIndex = SectOrganizationRules.GetDefaultPeakIndex();

	public override void _Ready()
	{
		BindUiNodes();
		BuildDynamicEntries();
		BindEvents();
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
		CallVisualFx("play_open");
	}

	public void ClosePanel()
	{
		ClosePopup();
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

	private void BindUiNodes()
	{
		_headerStatusLabel = GetNode<Label>($"{HeaderPath}/StatusColumn/HeaderStatusLabel");
		_closeButton = GetNode<Button>($"{HeaderPath}/CloseButton");
		_peakTitleLabel = GetNode<Label>($"{MiddleContentPath}/DetailHeaderRow/PeakTitleLabel");
		_peakCounterLabel = GetNode<Label>($"{MiddleContentPath}/DetailHeaderRow/PeakCounterLabel");
		_peakPositionLabel = GetNode<Label>(
			$"{MiddleContentPath}/SummaryPanel/SummaryMargin/SummaryColumn/PeakPositionRow/PeakPositionValue");
		_peakFocusLabel = GetNode<Label>(
			$"{MiddleContentPath}/SummaryPanel/SummaryMargin/SummaryColumn/PeakFocusRow/PeakFocusValue");
		_peakCoreUnitsLabel = GetNode<Label>(
			$"{MiddleContentPath}/SummaryPanel/SummaryMargin/SummaryColumn/PeakCoreUnitsRow/PeakCoreUnitsValue");
		_peakSupportActiveLabel = GetNode<Label>(
			$"{MiddleContentPath}/SummaryPanel/SummaryMargin/SummaryColumn/PeakSupportActiveRow/PeakSupportActiveValue");
		_peakSupportCandidateLabel = GetNode<Label>(
			$"{MiddleContentPath}/SummaryPanel/SummaryMargin/SummaryColumn/PeakSupportCandidateRow/PeakSupportCandidateValue");
		_departmentGrid = GetNode<GridContainer>($"{MiddleContentPath}/DepartmentGrid");
		_peakListColumn = GetNode<VBoxContainer>($"{BodyPath}/LeftColumn/PeakListScroll/PeakListColumn");
		_jobCardsContainer = GetNode<VBoxContainer>($"{BodyPath}/RightColumn/JobCardsContainer");
		_setSupportButton = GetNode<Button>($"{BodyPath}/RightColumn/ActionColumn/SetSupportButton");
		_resetSupportButton = GetNode<Button>($"{BodyPath}/RightColumn/ActionColumn/ActionRow/ResetSupportButton");
		_openGovernanceButton = GetNode<Button>($"{BodyPath}/RightColumn/ActionColumn/ActionRow/OpenGovernanceButton");
		_hintLabel = GetNode<Label>($"{BodyPath}/RightColumn/HintLabel");
		_visualFx = GetNodeOrNull<Node>("VisualFx");
	}

	private void BuildDynamicEntries()
	{
		_peakNavItems.Clear();
		ClearChildren(_peakListColumn);
		var peakCount = SectOrganizationRules.GetPeakCount();
		for (var index = 0; index < peakCount; index++)
		{
			var binding = CreatePeakNavItem(index);
			_peakNavItems.Add(binding);
			_peakListColumn.AddChild(binding.Root);
		}

		_jobCards.Clear();
		ClearChildren(_jobCardsContainer);
		foreach (var jobType in JobOrder)
		{
			var binding = CreateJobCard(jobType);
			_jobCards[jobType] = binding;
			_jobCardsContainer.AddChild(binding.Root);
		}
	}

	private void BindEvents()
	{
		_closeButton.Pressed += ClosePopup;
		_setSupportButton.Pressed += OnSetSupportPressed;
		_resetSupportButton.Pressed += OnResetSupportPressed;
		_openGovernanceButton.Pressed += OnOpenGovernancePressed;
	}

	private PeakNavBinding CreatePeakNavItem(int peakIndex)
	{
		var card = new PanelContainer
		{
			MouseFilter = MouseFilterEnum.Stop,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		card.GuiInput += @event => OnPeakNavInput(peakIndex, @event);

		var margin = new MarginContainer();
		card.AddChild(margin);
		CallVisualFx("prepare_dynamic_card_margin", margin);

		var column = new VBoxContainer();
		margin.AddChild(column);
		CallVisualFx("prepare_peak_nav_shell", card, column);

		var titleLabel = new Label();
		column.AddChild(titleLabel);

		var summaryLabel = new Label
		{
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		column.AddChild(summaryLabel);

		CallVisualFx("style_peak_nav_item", card, titleLabel, summaryLabel);

		return new PeakNavBinding(peakIndex, card, titleLabel, summaryLabel);
	}

	private JobCardBinding CreateJobCard(JobType jobType)
	{
		var card = new PanelContainer
		{
			MouseFilter = MouseFilterEnum.Stop
		};
		card.GuiInput += @event => OnJobCardInput(jobType, @event);

		var margin = new MarginContainer();
		card.AddChild(margin);
		CallVisualFx("prepare_dynamic_card_margin", margin);

		var column = new VBoxContainer();
		margin.AddChild(column);
		CallVisualFx("prepare_job_card_shell", card, column);

		var titleLabel = new Label();
		column.AddChild(titleLabel);

		var summaryLabel = new Label
		{
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		column.AddChild(summaryLabel);

		var detailLabel = new Label
		{
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		column.AddChild(detailLabel);

		CallVisualFx("style_job_card", card, titleLabel, summaryLabel, detailLabel);

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
		CallVisualFx("pulse_peak_nav");
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
		CallVisualFx("pulse_job_cards");
		GetViewport().SetInputAsHandled();
	}

	private void CallVisualFx(string methodName, params Variant[] args)
	{
		_visualFx?.Call(methodName, args);
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
			CallVisualFx("apply_peak_nav_state", binding.Root, binding.TitleLabel, selected);
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
			CallVisualFx("apply_job_card_state", binding.Root, selected);
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

	private PanelContainer CreateDepartmentCard(string title, string detail)
	{
		var card = new PanelContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};

		var margin = new MarginContainer();
		card.AddChild(margin);
		CallVisualFx("prepare_dynamic_card_margin", margin);

		var column = new VBoxContainer();
		margin.AddChild(column);
		CallVisualFx("prepare_department_card_shell", card, column);

		var titleLabel = new Label
		{
			Text = title
		};
		column.AddChild(titleLabel);

		var detailLabel = new Label
		{
			Text = detail,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		column.AddChild(detailLabel);

		CallVisualFx("style_department_card", card, titleLabel, detailLabel);

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

}
