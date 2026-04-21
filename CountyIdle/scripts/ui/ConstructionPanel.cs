using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class ConstructionPanel : PopupPanelBase
{
    private sealed record BuildingEntry(
        IndustryBuildingType BuildingType,
        PanelContainer Root,
        Label NameLabel,
        Label CountLabel,
        Label TagLabel,
        Label EffectLabel,
        Label RequirementLabel,
        Label CostLabel,
        Button DetailButton,
        Button BuildButton);

    private enum ConstructionCategory
    {
        All,
        Production,
        ResearchTrade,
        Governance
    }

    private Label _titleLabel = null!;
    private Button _closeButton = null!;
    private Label _hintLabel = null!;
    private Label _tileTitleLabel = null!;
    private Label _tileSubtitleLabel = null!;
    private Label _tileStatusLabel = null!;
    private Label _tileLocationLabel = null!;
    private Label _tileDescriptionLabel = null!;
    private Label _buildingSummaryLabel = null!;
    private Label _buildingSummaryValueLabel = null!;
    private Label _buildingRecommendationLabel = null!;
    private Label _buildingSlotStatusLabel = null!;
    private Label _detailTitleLabel = null!;
    private Label _detailEffectLabel = null!;
    private Label _detailCostLabel = null!;
    private Label _detailHintLabel = null!;
    private Label _queueHintLabel = null!;
    private Label _queueStatusLabel = null!;
    private Label _queuePendingLabel = null!;
    private ProgressBar _queueProgressBar = null!;
    private Button _queueCancelCurrentButton = null!;
    private Button _queueCancelPendingButton = null!;
    private Button _categoryAllButton = null!;
    private Button _categoryProductionButton = null!;
    private Button _categoryResearchTradeButton = null!;
    private Button _categoryGovernanceButton = null!;
    private Label _groupHeaderProduction = null!;
    private Label _groupHeaderResearchTrade = null!;
    private Label _groupHeaderGovernance = null!;

    private readonly Dictionary<IndustryBuildingType, BuildingEntry> _entries = new();
    private IndustryBuildingType _selectedBuildingType = IndustryBuildingType.Agriculture;
    private ConstructionCategory _activeCategory = ConstructionCategory.All;
    private GameState? _currentState;
    private TownMapSelectionSummary? _currentSummary;

    public event Action<IndustryBuildingType>? BuildRequested;
    public event Action? CancelCurrentRequested;
    public event Action? CancelPendingRequested;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/TitleLabel");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _hintLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/HintLabel");
        _tileTitleLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TileCard/CardMargin/CardColumn/TileTitle");
        _tileSubtitleLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TileCard/CardMargin/CardColumn/TileSubtitle");
        _tileStatusLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TileCard/CardMargin/CardColumn/TileStatus");
        _tileLocationLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TileCard/CardMargin/CardColumn/TileLocation");
        _tileDescriptionLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TileCard/CardMargin/CardColumn/TileDescription");
        _buildingSummaryLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/BuildingCard/CardMargin/CardColumn/BuildingLabel");
        _buildingSummaryValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/BuildingCard/CardMargin/CardColumn/BuildingValue");
        _buildingRecommendationLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/BuildingCard/CardMargin/CardColumn/RecommendationLabel");
        _buildingSlotStatusLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/BuildingCard/CardMargin/CardColumn/SlotStatusLabel");
        _detailTitleLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailCard/CardMargin/DetailColumn/DetailTitle");
        _detailEffectLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailCard/CardMargin/DetailColumn/DetailEffect");
        _detailCostLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailCard/CardMargin/DetailColumn/DetailCost");
        _detailHintLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailCard/CardMargin/DetailColumn/DetailHint");
        _queueHintLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/QueueCard/CardMargin/QueueColumn/QueueHint");
        _queueStatusLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/QueueCard/CardMargin/QueueColumn/QueueStatusLabel");
        _queueProgressBar = GetNode<ProgressBar>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/QueueCard/CardMargin/QueueColumn/QueueProgressBar");
        _queuePendingLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/QueueCard/CardMargin/QueueColumn/QueuePendingLabel");
        _queueCancelCurrentButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/QueueCard/CardMargin/QueueColumn/QueueActionRow/CancelCurrentButton");
        _queueCancelPendingButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/QueueCard/CardMargin/QueueColumn/QueueActionRow/CancelPendingButton");

        _titleLabel.Text = "文明式营建卷";
        _hintLabel.Text = "挑选宗门营建并与院域坊局联动，营建将进入队列并在时辰结算后落点。";

        InitializePopupHint(_hintLabel);
        BindEntries();
        BindCategoryButtons();
        BindGroupHeaders();
        BindEvents();
        Hide();
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

    public void Open(GameState state, TownMapSelectionSummary summary)
    {
        RefreshState(state, summary);
        OpenPopup();
    }

    public void ClosePanel()
    {
        ClosePopup();
    }

    public void RefreshState(GameState state, TownMapSelectionSummary summary)
    {
        _currentState = state;
        _currentSummary = summary;
        RefreshSummary(state, summary);
        RefreshEntries(state, summary);
        RefreshQueue(state);
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return "营建卷用于对比建筑收益与建造消耗，营建将进入队列并在时辰结算后落点。";
    }

    private void BindEvents()
    {
        _closeButton.Pressed += ClosePanel;
        _queueCancelCurrentButton.Pressed += () => CancelCurrentRequested?.Invoke();
        _queueCancelPendingButton.Pressed += () => CancelPendingRequested?.Invoke();
    }

    private void BindEntries()
    {
        _entries.Clear();
        RegisterEntry(IndustryBuildingType.Agriculture, "CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/BuildingListScroll/BuildingRows/RowAgriculture");
        RegisterEntry(IndustryBuildingType.Workshop, "CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/BuildingListScroll/BuildingRows/RowWorkshop");
        RegisterEntry(IndustryBuildingType.Research, "CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/BuildingListScroll/BuildingRows/RowResearch");
        RegisterEntry(IndustryBuildingType.Trade, "CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/BuildingListScroll/BuildingRows/RowTrade");
        RegisterEntry(IndustryBuildingType.Administration, "CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/BuildingListScroll/BuildingRows/RowAdministration");
    }

    private void RegisterEntry(IndustryBuildingType buildingType, string rowPath)
    {
        var root = GetNode<PanelContainer>(rowPath);
        var nameLabel = GetNode<Label>($"{rowPath}/RowMargin/RowColumn/HeaderRow/NameLabel");
        var countLabel = GetNode<Label>($"{rowPath}/RowMargin/RowColumn/HeaderRow/CountLabel");
        var tagLabel = GetNode<Label>($"{rowPath}/RowMargin/RowColumn/HeaderRow/TagLabel");
        var effectLabel = GetNode<Label>($"{rowPath}/RowMargin/RowColumn/EffectLabel");
        var requirementLabel = GetNode<Label>($"{rowPath}/RowMargin/RowColumn/RequirementLabel");
        var costLabel = GetNode<Label>($"{rowPath}/RowMargin/RowColumn/ActionRow/CostLabel");
        var detailButton = GetNode<Button>($"{rowPath}/RowMargin/RowColumn/ActionRow/DetailButton");
        var buildButton = GetNode<Button>($"{rowPath}/RowMargin/RowColumn/ActionRow/BuildButton");

        detailButton.Text = "详情";
        buildButton.Text = "建造";

        detailButton.Pressed += () => SelectBuilding(buildingType);
        buildButton.Pressed += () =>
        {
            SelectBuilding(buildingType);
            BuildRequested?.Invoke(buildingType);
        };

        _entries[buildingType] = new BuildingEntry(
            buildingType,
            root,
            nameLabel,
            countLabel,
            tagLabel,
            effectLabel,
            requirementLabel,
            costLabel,
            detailButton,
            buildButton);
    }

    private void RefreshSummary(GameState state, TownMapSelectionSummary summary)
    {
        var nameMap = state.SectNameMap;
        string Resolve(string text) => SectNamingRules.ReplaceKnownNames(nameMap, text);

        _tileTitleLabel.Text = Resolve(summary.Title);
        _tileSubtitleLabel.Text = Resolve(summary.Subtitle);
        _tileStatusLabel.Text = $"{summary.StatusLabel}：{Resolve(summary.StatusText)}";
        _tileLocationLabel.Text = $"{summary.LocationLabel}：{Resolve(summary.LocationText)}";
        _tileDescriptionLabel.Text = Resolve(summary.DescriptionText);

        _buildingSummaryLabel.Text = summary.BuildingLabel;
        _buildingSummaryValueLabel.Text = Resolve(BuildBuildingListText(summary));
        _buildingSlotStatusLabel.Text = BuildSlotStatusText(summary);
        if (summary.SuggestedBuildType.HasValue)
        {
            _buildingRecommendationLabel.Text =
                $"推荐：{SectMapSemanticRules.GetBuildingDisplayName(summary.SuggestedBuildType.Value)}";
        }
        else
        {
            _buildingRecommendationLabel.Text = "推荐：待评估";
        }

        // 在营建卷顶部直接说明剩余坊位，减少玩家来回切换后才发现当前格已满的挫败感。
        _hintLabel.Text = BuildConstructionHeaderHint(summary);
    }

    private void RefreshEntries(GameState state, TownMapSelectionSummary summary)
    {
        var recommended = summary.SuggestedBuildType;
        var hasPlacementSelection = summary.HasBuildCapacity;
        foreach (var entry in _entries.Values)
        {
            var preview = IndustrySystem.GetBuildCostPreview(entry.BuildingType);
            var count = GetBuildingCount(state, entry.BuildingType);
            var costText = BuildCostText(preview);

            entry.NameLabel.Text = preview.DisplayName;
            entry.CountLabel.Text = $"×{count}";
            entry.CostLabel.Text = costText;
            entry.EffectLabel.Text = BuildEffectText(state, entry.BuildingType);
            entry.RequirementLabel.Text = BuildRequirementText(state, summary, entry.BuildingType);
            entry.TagLabel.Text = recommended == entry.BuildingType ? "推荐" : string.Empty;
            entry.TagLabel.Visible = recommended == entry.BuildingType;

            var hasManagers = state.Workers > 0;
            var canAfford = IndustrySystem.CanAffordBuildCost(state, preview);
            entry.BuildButton.Disabled = !(hasPlacementSelection && hasManagers && canAfford);
            if (!hasPlacementSelection)
            {
                entry.BuildButton.TooltipText = "请先在山门图选中院域地块，再执行建造。";
            }
            else
            {
                entry.BuildButton.TooltipText = hasManagers
                    ? (canAfford ? $"消耗：{costText}" : "资源不足，暂不可建造。")
                    : "缺少管理人员，无法组织建造。";
            }
            entry.Root.Visible = IsEntryVisible(entry.BuildingType);
        }

        ApplyGroupHeaderVisibility();

        var fallback = recommended ?? _selectedBuildingType;
        if (!_entries.ContainsKey(fallback))
        {
            fallback = IndustryBuildingType.Agriculture;
        }

        SelectBuilding(fallback);
    }

    private void RefreshQueue(GameState state)
    {
        // 刷新营建队列展示。
        var queue = state.ConstructionQueue ?? new List<ConstructionQueueItem>();
        if (queue.Count == 0)
        {
            _queueStatusLabel.Text = "队列空置：暂无营建排队。";
            _queuePendingLabel.Text = "排队：--";
            _queueProgressBar.Visible = false;
            _queueProgressBar.Value = 0;
            _queueCancelCurrentButton.Disabled = true;
            _queueCancelPendingButton.Disabled = true;
            _queueCancelCurrentButton.Text = "停工";
            _queueCancelCurrentButton.TooltipText = "当前无在建项目。";
            _queueCancelPendingButton.TooltipText = "暂无排队项目可撤销。";
            return;
        }

        var current = queue[0];
        var total = Math.Max(current.TotalHours, 1);
        var remaining = Math.Max(current.RemainingHours, 0);
        var finished = Math.Clamp(total - remaining, 0, total);
        var displayName = SectMapSemanticRules.GetBuildingDisplayName(current.BuildingType);
        _queueStatusLabel.Text = $"当前施工：{displayName} · 进度 {finished}/{total} 时辰";
        _queueProgressBar.Visible = true;
        _queueProgressBar.MaxValue = total;
        _queueProgressBar.Value = finished;

        if (queue.Count > 1)
        {
            var pendingNames = new List<string>();
            for (var index = 1; index < queue.Count && pendingNames.Count < 3; index++)
            {
                pendingNames.Add(SectMapSemanticRules.GetBuildingDisplayName(queue[index].BuildingType));
            }

            var suffix = queue.Count - 1 > pendingNames.Count
                ? $" 等{queue.Count - 1}项"
                : string.Empty;
            _queuePendingLabel.Text = $"排队：{string.Join(" / ", pendingNames)}{suffix}";
        }
        else
        {
            _queuePendingLabel.Text = "排队：无";
        }

        var canRefundCurrent = remaining >= total;
        _queueCancelCurrentButton.Disabled = false;
        _queueCancelCurrentButton.Text = canRefundCurrent ? "撤销当前" : "停工";
        _queueCancelCurrentButton.TooltipText = canRefundCurrent
            ? "撤销当前营建（未开工可退回消耗）。"
            : "停工不退费，仅终止当前建造。";

        _queueCancelPendingButton.Disabled = queue.Count <= 1;
        _queueCancelPendingButton.TooltipText = queue.Count > 1
            ? "撤销排队中的营建，退回未开工消耗。"
            : "暂无排队项目可撤销。";
    }

    private void SelectBuilding(IndustryBuildingType buildingType)
    {
        _selectedBuildingType = buildingType;
        if (_currentState == null)
        {
            return;
        }

        var preview = IndustrySystem.GetBuildCostPreview(buildingType);
        _detailTitleLabel.Text = $"{preview.DisplayName} · 详览";
        _detailEffectLabel.Text = BuildEffectText(_currentState, buildingType);
        _detailCostLabel.Text = $"建造消耗：{BuildCostText(preview)}";
        _detailHintLabel.Text = BuildDetailHint(_currentSummary, buildingType);

        foreach (var entry in _entries.Values)
        {
            entry.Root.SelfModulate = entry.BuildingType == buildingType
                ? new Color(1f, 0.97f, 0.9f, 1f)
                : Colors.White;
        }
    }

    private static string BuildBuildingListText(TownMapSelectionSummary summary)
    {
        if (summary.BuildingList.Length > 0)
        {
            return string.Join(" / ", summary.BuildingList);
        }

        return summary.BuildingText;
    }

    private static string BuildEffectText(GameState state, IndustryBuildingType buildingType)
    {
        return buildingType switch
        {
            IndustryBuildingType.Agriculture =>
                $"灵植容量 +{IndustryRules.CapacityPerSpiritPlantBuilding}",
            IndustryBuildingType.Workshop =>
                $"炼器容量 +{IndustryRules.CapacityPerForgingBuilding}\n灵植容量 +{IndustryRules.CapacityPerForgingBuilding / 2}",
            IndustryBuildingType.Research =>
                $"符箓容量 +{IndustryRules.CapacityPerTalismanBuilding}\n天机容量 +{IndustryRules.CapacityPerArcaneBuilding}",
            IndustryBuildingType.Trade =>
                $"灵兽容量 +{IndustryRules.CapacityPerBeastBuilding}\n傀儡容量 +{IndustryRules.CapacityPerGolemBuilding}",
            IndustryBuildingType.Administration =>
                $"炼丹容量 +{IndustryRules.CapacityPerAlchemyBuilding}\n阵法容量 +{IndustryRules.CapacityPerFormationBuilding}\n仓储上限 +{ResolveWarehouseCapacityDelta(state):0}",
            _ => "暂无效果"
        };
    }

    private static double ResolveWarehouseCapacityDelta(GameState state)
    {
        if (state == null)
        {
            return 0;
        }

        var before = IndustryRules.CalculateWarehouseCapacity(state);
        var clone = state.Clone();
        clone.AdministrationBuildings += 1;
        var after = IndustryRules.CalculateWarehouseCapacity(clone);
        return Math.Max(0, after - before);
    }

    private static string BuildDetailHint(TownMapSelectionSummary? summary, IndustryBuildingType buildingType)
    {
        if (summary == null)
        {
            return "建造将优先落点到山门图可用院域。";
        }

        if (!summary.HasSelection)
        {
            return "尚未选中院域地块，营建卷仅显示总览与推荐提示。请先点选院域后再执行建造。";
        }

        if (!summary.HasBuildCapacity)
        {
            return $"当前院域坊位已满（{summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount}），请改选其他地块或等待后续扩展机制。";
        }

        if (summary.AnchorType != null)
        {
            return "当前点中的是此地已落成建筑；若坊位未满，仍可继续对这块地追加营建。";
        }

        var placementHint = "当前院域已锁定，营建入队后会优先落点到选中地块。";

        if (summary.SuggestedBuildType == buildingType)
        {
            return $"{placementHint}\n该建筑与当前院域定位匹配，易形成稳定协同。";
        }

        return $"{placementHint}\n可结合坊位格局与灵气状况再作取舍。";
    }

    // 将槽位状态压缩成一眼能读懂的卷面提示。
    private static string BuildSlotStatusText(TownMapSelectionSummary summary)
    {
        if (!summary.HasSelection)
        {
            return "坊位：待选中";
        }

        if (summary.BuildSlotCount <= 0)
        {
            return "坊位：当前地块未定义可建坊位";
        }

        var remaining = Math.Max(summary.BuildSlotCount - summary.OccupiedBuildSlotCount, 0);
        return remaining > 0
            ? $"坊位：{summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount} 已占用，尚余 {remaining} 位"
            : $"坊位：{summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount} 已满";
    }

    // 让卷首提示跟着地块槽位状态变化，优先强调“还能不能继续在这格建”。
    private static string BuildConstructionHeaderHint(TownMapSelectionSummary summary)
    {
        if (!summary.HasSelection)
        {
            return "挑选宗门营建并与院域坊局联动，建造会优先落点到当前选中院域。";
        }

        if (summary.BuildSlotCount <= 0)
        {
            return "当前选中地块尚未定义可建坊位，请改选其他院域。";
        }

        var remaining = Math.Max(summary.BuildSlotCount - summary.OccupiedBuildSlotCount, 0);
        if (remaining <= 0)
        {
            return $"当前院域坊位已满（{summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount}），请改选其他地块继续营建。";
        }

        return $"当前院域尚余 {remaining} 个坊位，可继续像文明城市那样往同一地块追加不同建筑。";
    }

    private static int GetBuildingCount(GameState state, IndustryBuildingType buildingType)
    {
        return buildingType switch
        {
            IndustryBuildingType.Agriculture => state.AgricultureBuildings,
            IndustryBuildingType.Workshop => state.WorkshopBuildings,
            IndustryBuildingType.Research => state.ResearchBuildings,
            IndustryBuildingType.Trade => state.TradeBuildings,
            IndustryBuildingType.Administration => state.AdministrationBuildings,
            _ => 0
        };
    }

    private static string BuildCostText(IndustrySystem.BuildingCostPreview preview)
    {
        var parts = new List<string>(5);
        AppendCost(parts, nameof(GameState.Wood), preview.Wood);
        AppendCost(parts, nameof(GameState.Stone), preview.Stone);
        AppendCost(parts, nameof(GameState.Gold), preview.Gold);
        AppendCost(parts, nameof(GameState.ContributionPoints), preview.Contribution);
        AppendCost(parts, nameof(GameState.ConstructionMaterials), preview.Construction);
        return parts.Count > 0 ? string.Join(" / ", parts) : "无消耗";
    }

    private static void AppendCost(List<string> parts, string fieldName, double value)
    {
        if (value <= 0)
        {
            return;
        }

        var displayName = MaterialSemanticRules.GetDisplayName(fieldName);
        var amount = InventoryRules.QuantizeCost(value);
        parts.Add($"{displayName}{amount}");
    }

    private void BindCategoryButtons()
    {
        _categoryAllButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/CategoryRow/CategoryAll");
        _categoryProductionButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/CategoryRow/CategoryProduction");
        _categoryResearchTradeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/CategoryRow/CategoryResearchTrade");
        _categoryGovernanceButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/CategoryRow/CategoryGovernance");

        BindCategoryButton(_categoryAllButton, ConstructionCategory.All);
        BindCategoryButton(_categoryProductionButton, ConstructionCategory.Production);
        BindCategoryButton(_categoryResearchTradeButton, ConstructionCategory.ResearchTrade);
        BindCategoryButton(_categoryGovernanceButton, ConstructionCategory.Governance);

        SetActiveCategory(ConstructionCategory.All, false);
    }

    private void BindCategoryButton(Button button, ConstructionCategory category)
    {
        button.ToggleMode = true;
        button.Pressed += () => SetActiveCategory(category, true);
    }

    private void SetActiveCategory(ConstructionCategory category, bool refreshEntries)
    {
        _activeCategory = category;
        UpdateCategoryButtonState();
        if (!refreshEntries || _currentState == null || _currentSummary == null)
        {
            return;
        }

        RefreshEntries(_currentState, _currentSummary);
    }

    private void UpdateCategoryButtonState()
    {
        _categoryAllButton.ButtonPressed = _activeCategory == ConstructionCategory.All;
        _categoryProductionButton.ButtonPressed = _activeCategory == ConstructionCategory.Production;
        _categoryResearchTradeButton.ButtonPressed = _activeCategory == ConstructionCategory.ResearchTrade;
        _categoryGovernanceButton.ButtonPressed = _activeCategory == ConstructionCategory.Governance;
    }

    private void BindGroupHeaders()
    {
        _groupHeaderProduction = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/BuildingListScroll/BuildingRows/GroupProduction");
        _groupHeaderResearchTrade = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/BuildingListScroll/BuildingRows/GroupResearchTrade");
        _groupHeaderGovernance = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/BuildingListCard/CardMargin/ListColumn/BuildingListScroll/BuildingRows/GroupGovernance");
    }

    private bool IsEntryVisible(IndustryBuildingType buildingType)
    {
        return _activeCategory switch
        {
            ConstructionCategory.All => true,
            ConstructionCategory.Production => buildingType is IndustryBuildingType.Agriculture or IndustryBuildingType.Workshop,
            ConstructionCategory.ResearchTrade => buildingType is IndustryBuildingType.Research or IndustryBuildingType.Trade,
            ConstructionCategory.Governance => buildingType == IndustryBuildingType.Administration,
            _ => true
        };
    }

    private void ApplyGroupHeaderVisibility()
    {
        if (_groupHeaderProduction != null)
        {
            _groupHeaderProduction.Visible = _activeCategory is ConstructionCategory.All or ConstructionCategory.Production;
        }

        if (_groupHeaderResearchTrade != null)
        {
            _groupHeaderResearchTrade.Visible = _activeCategory is ConstructionCategory.All or ConstructionCategory.ResearchTrade;
        }

        if (_groupHeaderGovernance != null)
        {
            _groupHeaderGovernance.Visible = _activeCategory is ConstructionCategory.All or ConstructionCategory.Governance;
        }
    }

    private static string BuildRequirementText(GameState state, TownMapSelectionSummary summary, IndustryBuildingType buildingType)
    {
        var hasPlacementSelection = summary.HasBuildCapacity;
        var slotText = hasPlacementSelection ? summary.ResidentText : "需先选中院域";
        var qiText = hasPlacementSelection ? summary.TransitText : "需先选中院域";
        var workforceText = $"人口 {state.Population} · 管理 {state.Workers}";
        var prereqBuilding = ResolvePrerequisiteBuilding(buildingType);
        return $"前置：坊位 {slotText} · 灵气 {qiText} · {workforceText} · 前置建筑 {prereqBuilding}";
    }

    private static string ResolvePrerequisiteBuilding(IndustryBuildingType buildingType)
    {
        return buildingType switch
        {
            IndustryBuildingType.Agriculture => "无（基础营建默认解锁）",
            IndustryBuildingType.Workshop => "无（基础营建默认解锁）",
            IndustryBuildingType.Research => "无（基础营建默认解锁）",
            IndustryBuildingType.Trade => "无（基础营建默认解锁）",
            IndustryBuildingType.Administration => "无（基础营建默认解锁）",
            _ => "无"
        };
    }
}
