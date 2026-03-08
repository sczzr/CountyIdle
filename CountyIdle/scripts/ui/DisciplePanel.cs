using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class DisciplePanel : PopupPanelBase
{
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
        public MetricBinding(Label valueLabel, ProgressBar progressBar)
        {
            ValueLabel = valueLabel;
            ProgressBar = progressBar;
        }

        public Label ValueLabel { get; }
        public ProgressBar ProgressBar { get; }
    }

    private Label _summaryLabel = null!;
    private Label _governanceLabel = null!;
    private OptionButton _filterOption = null!;
    private OptionButton _sortOption = null!;
    private ItemList _discipleList = null!;
    private Label _profileNameLabel = null!;
    private Label _profileMetaLabel = null!;
    private Label _profileStatusLabel = null!;
    private Label _peakLabel = null!;
    private Label _traitLabel = null!;
    private Label _noteLabel = null!;
    private Button _closeButton = null!;
    private Label _hintLabel = null!;
    private readonly Dictionary<string, MetricBinding> _metrics = new();
    private readonly List<DiscipleProfile> _allProfiles = new();
    private readonly List<DiscipleProfile> _visibleProfiles = new();

    private GameState _state = new();
    private int _selectedDiscipleId = 1;
    private FilterMode _filterMode;
    private SortMode _sortMode;

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

    public void Open(GameState state, int? preferredDiscipleId = null, JobType? preferredJobType = null)
    {
        RefreshState(state, preferredDiscipleId, preferredJobType);
        OpenPopup();
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

        return "弟子谱会按当前经营态势派生生成名册，用于查看门人属性、培养方向与当前差事；不直接改写小时结算。按 Esc 可关闭。";
    }

    private void BuildUi()
    {
        MouseFilter = Control.MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var overlay = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0.76f),
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
            CustomMinimumSize = new Vector2(1020, 640),
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
            Text = "浮云宗·弟子谱"
        };
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleLabel.AddThemeFontSizeOverride("font_size", 20);
        titleLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.91f, 0.79f));
        headerRow.AddChild(titleLabel);

        _closeButton = CreateActionButton("关闭");
        _closeButton.Pressed += ClosePopup;
        headerRow.AddChild(_closeButton);

        var summaryPanel = new PanelContainer();
        summaryPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.06f, 0.07f, 0.09f, 0.94f)));
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

        _summaryLabel = CreateInfoLabel();
        _governanceLabel = CreateInfoLabel();
        summaryColumn.AddChild(_summaryLabel);
        summaryColumn.AddChild(_governanceLabel);

        var filterRow = new HBoxContainer();
        filterRow.AddThemeConstantOverride("separation", 10);
        rootColumn.AddChild(filterRow);

        filterRow.AddChild(CreateSectionLabel("筛选"));
        _filterOption = new OptionButton();
        _filterOption.AddItem("全部弟子");
        _filterOption.AddItem("真传名册");
        _filterOption.AddItem("阵材职司");
        _filterOption.AddItem("阵务职司");
        _filterOption.AddItem("外事职司");
        _filterOption.AddItem("推演职司");
        _filterOption.AddItem("待命轮值");
        _filterOption.ItemSelected += OnFilterSelected;
        filterRow.AddChild(_filterOption);

        filterRow.AddChild(CreateSectionLabel("排序"));
        _sortOption = new OptionButton();
        _sortOption.AddItem("名册顺序");
        _sortOption.AddItem("修为优先");
        _sortOption.AddItem("潜力优先");
        _sortOption.AddItem("心境优先");
        _sortOption.AddItem("贡献优先");
        _sortOption.ItemSelected += OnSortSelected;
        filterRow.AddChild(_sortOption);

        var filler = new Control
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        filterRow.AddChild(filler);

        var split = new HSplitContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            SplitOffsets = [340]
        };
        rootColumn.AddChild(split);

        var listPanel = new PanelContainer();
        listPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.08f, 0.09f, 0.11f, 0.94f)));
        split.AddChild(listPanel);

        var listMargin = new MarginContainer();
        listMargin.AddThemeConstantOverride("margin_left", 10);
        listMargin.AddThemeConstantOverride("margin_top", 10);
        listMargin.AddThemeConstantOverride("margin_right", 10);
        listMargin.AddThemeConstantOverride("margin_bottom", 10);
        listPanel.AddChild(listMargin);

        var listColumn = new VBoxContainer();
        listColumn.AddThemeConstantOverride("separation", 8);
        listMargin.AddChild(listColumn);

        var listTitle = CreateSectionLabel("当前名册");
        listTitle.AddThemeFontSizeOverride("font_size", 14);
        listColumn.AddChild(listTitle);

        _discipleList = new ItemList
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            AllowReselect = true,
            SelectMode = ItemList.SelectModeEnum.Single
        };
        _discipleList.ItemSelected += OnDiscipleSelected;
        listColumn.AddChild(_discipleList);

        var detailPanel = new PanelContainer();
        detailPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.08f, 0.09f, 0.11f, 0.94f)));
        split.AddChild(detailPanel);

        var detailMargin = new MarginContainer();
        detailMargin.AddThemeConstantOverride("margin_left", 14);
        detailMargin.AddThemeConstantOverride("margin_top", 14);
        detailMargin.AddThemeConstantOverride("margin_right", 14);
        detailMargin.AddThemeConstantOverride("margin_bottom", 14);
        detailPanel.AddChild(detailMargin);

        var detailColumn = new VBoxContainer();
        detailColumn.AddThemeConstantOverride("separation", 10);
        detailMargin.AddChild(detailColumn);

        _profileNameLabel = new Label();
        _profileNameLabel.AddThemeFontSizeOverride("font_size", 18);
        _profileNameLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.91f, 0.79f));
        detailColumn.AddChild(_profileNameLabel);

        _profileMetaLabel = CreateInfoLabel();
        _profileStatusLabel = CreateInfoLabel();
        detailColumn.AddChild(_profileMetaLabel);
        detailColumn.AddChild(_profileStatusLabel);

        var metricPanel = new PanelContainer();
        metricPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.05f, 0.06f, 0.08f, 0.92f)));
        detailColumn.AddChild(metricPanel);

        var metricMargin = new MarginContainer();
        metricMargin.AddThemeConstantOverride("margin_left", 12);
        metricMargin.AddThemeConstantOverride("margin_top", 12);
        metricMargin.AddThemeConstantOverride("margin_right", 12);
        metricMargin.AddThemeConstantOverride("margin_bottom", 12);
        metricPanel.AddChild(metricMargin);

        var metricColumn = new VBoxContainer();
        metricColumn.AddThemeConstantOverride("separation", 8);
        metricMargin.AddChild(metricColumn);

        AddMetricRow(metricColumn, "气血", "Health");
        AddMetricRow(metricColumn, "心境", "Mood");
        AddMetricRow(metricColumn, "潜力", "Potential");
        AddMetricRow(metricColumn, "战力", "Combat");
        AddMetricRow(metricColumn, "匠艺", "Craft");
        AddMetricRow(metricColumn, "悟性", "Insight");
        AddMetricRow(metricColumn, "执行", "Execution");
        AddMetricRow(metricColumn, "贡献", "Contribution");

        _peakLabel = CreateWrapLabel();
        _traitLabel = CreateWrapLabel();
        _noteLabel = CreateWrapLabel();
        detailColumn.AddChild(_peakLabel);
        detailColumn.AddChild(_traitLabel);
        detailColumn.AddChild(_noteLabel);

        _hintLabel = CreateInfoLabel();
        rootColumn.AddChild(_hintLabel);

        _filterOption.Select(0);
        _sortOption.Select(0);
    }

    private void RefreshSummary()
    {
        var talentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(_state);
        var law = SectGovernanceRules.GetActiveLawDefinition(_state);
        var direction = SectGovernanceRules.GetActiveDevelopmentDefinition(_state);
        var commuteMinutes = PopulationRules.GetCommuteMinutes(_state);

        _summaryLabel.Text =
            $"门人总册 {_state.Population} · 真传 {_state.ElitePopulation} · 现役职司 {_state.GetAssignedPopulation()} · 待命 {_state.GetUnassignedPopulation()} · 伤病 {_state.SickPopulation} · 平均通勤 {commuteMinutes:0} 分";
        _governanceLabel.Text =
            $"当前治宗：{direction.DisplayName} / {law.DisplayName} / {talentPlan.DisplayName} · 可用来观察培养倾向、门人状态与岗位结构。";
    }

    private void RebuildDiscipleList()
    {
        _visibleProfiles.Clear();
        _visibleProfiles.AddRange(_allProfiles.Where(MatchesFilter));
        SortProfiles(_visibleProfiles);

        _discipleList.Clear();
        foreach (var profile in _visibleProfiles)
        {
            _discipleList.AddItem(BuildListText(profile));
        }

        if (_visibleProfiles.Count == 0)
        {
            _profileNameLabel.Text = "当前无可显示弟子";
            _profileMetaLabel.Text = "请稍后再看，或切换筛选条件。";
            _profileStatusLabel.Text = string.Empty;
            _peakLabel.Text = string.Empty;
            _traitLabel.Text = string.Empty;
            _noteLabel.Text = string.Empty;
            foreach (var binding in _metrics.Values)
            {
                binding.ValueLabel.Text = "0";
                binding.ProgressBar.Value = 0;
            }

            return;
        }

        var selectedIndex = _visibleProfiles.FindIndex(profile => profile.Id == _selectedDiscipleId);
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
            _selectedDiscipleId = _visibleProfiles[0].Id;
        }

        _discipleList.Select(selectedIndex);
        RefreshDetail(_visibleProfiles[selectedIndex]);
    }

    private void RefreshDetail(DiscipleProfile profile)
    {
        _profileNameLabel.Text = $"{profile.Name} · {profile.RankName}";
        _profileMetaLabel.Text = $"{profile.DutyDisplayName} · {profile.RealmName} · {profile.AgeText}";
        _profileStatusLabel.Text = $"当前差事：{profile.CurrentAssignment} · 居所：{profile.ResidenceName}";
        _peakLabel.Text = $"关联峰脉：{profile.LinkedPeakSummary}";
        _traitLabel.Text = $"特征：{profile.TraitSummary}";
        _noteLabel.Text = $"培养建议：{profile.Note}";

        SetMetric("Health", profile.Health);
        SetMetric("Mood", profile.Mood);
        SetMetric("Potential", profile.Potential);
        SetMetric("Combat", profile.Combat);
        SetMetric("Craft", profile.Craft);
        SetMetric("Insight", profile.Insight);
        SetMetric("Execution", profile.Execution);
        SetMetric("Contribution", profile.Contribution);
    }

    private void OnFilterSelected(long index)
    {
        _filterMode = (FilterMode)(int)index;
        RebuildDiscipleList();
        RefreshPopupHint();
    }

    private void OnSortSelected(long index)
    {
        _sortMode = (SortMode)(int)index;
        RebuildDiscipleList();
        RefreshPopupHint();
    }

    private void OnDiscipleSelected(long index)
    {
        if (index < 0 || index >= _visibleProfiles.Count)
        {
            return;
        }

        var profile = _visibleProfiles[(int)index];
        _selectedDiscipleId = profile.Id;
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

    private string BuildListText(DiscipleProfile profile)
    {
        var eliteTag = profile.IsElite ? "真" : profile.RankName[..1];
        return $"[{eliteTag}] {profile.Name} · {profile.DutyDisplayName} · {profile.RealmName}";
    }

    private void AddMetricRow(VBoxContainer parent, string title, string key)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 10);
        parent.AddChild(row);

        var titleLabel = new Label
        {
            Text = title,
            CustomMinimumSize = new Vector2(56, 0)
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 12);
        row.AddChild(titleLabel);

        var progressBar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 100,
            Value = 0,
            ShowPercentage = false,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        row.AddChild(progressBar);

        var valueLabel = new Label
        {
            Text = "0",
            CustomMinimumSize = new Vector2(48, 0),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        valueLabel.AddThemeFontSizeOverride("font_size", 12);
        row.AddChild(valueLabel);

        _metrics[key] = new MetricBinding(valueLabel, progressBar);
    }

    private void SetMetric(string key, int value)
    {
        if (!_metrics.TryGetValue(key, out var binding))
        {
            return;
        }

        var clamped = Math.Clamp(value, 0, 100);
        binding.ValueLabel.Text = clamped.ToString();
        binding.ProgressBar.Value = clamped;
        binding.ProgressBar.TooltipText = $"{clamped}/100";
    }

    private static Label CreateSectionLabel(string text)
    {
        var label = new Label
        {
            Text = text
        };
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", new Color(0.95f, 0.91f, 0.79f));
        return label;
    }

    private static Label CreateInfoLabel()
    {
        var label = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", new Color(0.84f, 0.87f, 0.93f));
        return label;
    }

    private static Label CreateWrapLabel()
    {
        var label = CreateInfoLabel();
        label.SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;
        return label;
    }

    private static Button CreateActionButton(string text)
    {
        var button = new Button
        {
            Text = text
        };
        button.AddThemeFontSizeOverride("font_size", 12);
        button.AddThemeStyleboxOverride("normal", CreateButtonStyle());
        button.AddThemeStyleboxOverride("hover", CreateButtonStyle(new Color(0.24f, 0.28f, 0.34f, 1f)));
        button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(new Color(0.17f, 0.20f, 0.25f, 1f)));
        button.AddThemeStyleboxOverride("focus", CreateButtonStyle(new Color(0.20f, 0.24f, 0.29f, 1f)));
        return button;
    }

    private static StyleBoxFlat CreatePanelStyle(Color color)
    {
        return new StyleBoxFlat
        {
            BgColor = color,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.2f, 0.21f, 0.24f, 1f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4
        };
    }

    private static StyleBoxFlat CreateButtonStyle(Color? color = null)
    {
        return new StyleBoxFlat
        {
            BgColor = color ?? new Color(0.18f, 0.21f, 0.26f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.31f, 0.35f, 0.42f, 1f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ContentMarginLeft = 12,
            ContentMarginTop = 6,
            ContentMarginRight = 12,
            ContentMarginBottom = 6
        };
    }
}
