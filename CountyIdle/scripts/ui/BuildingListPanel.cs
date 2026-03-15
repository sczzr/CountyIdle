using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class BuildingListPanel : PanelContainer
{
    private sealed record RowBinding(
        IndustryBuildingType BuildingType,
        Label NameLabel,
        Label CountLabel,
        Label CostLabel,
        Button BuildButton);

    private const string RootPath = "BuildingListVBox";
    private const string HeaderPath = RootPath + "/BuildingHeader";
    private const string RowsPath = RootPath + "/BuildingRows";

    private Label _titleLabel = null!;
    private Label _hintLabel = null!;
    private readonly Dictionary<IndustryBuildingType, RowBinding> _rows = new();

    public event Action<IndustryBuildingType>? BuildRequested;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>($"{HeaderPath}/BuildingTitle");
        _hintLabel = GetNode<Label>($"{HeaderPath}/BuildingHint");
        _titleLabel.Text = "营建清单";
        _hintLabel.Text = "可批量建造";

        BindRow(IndustryBuildingType.Agriculture, $"{RowsPath}/RowAgriculture");
        BindRow(IndustryBuildingType.Workshop, $"{RowsPath}/RowWorkshop");
        BindRow(IndustryBuildingType.Research, $"{RowsPath}/RowResearch");
        BindRow(IndustryBuildingType.Trade, $"{RowsPath}/RowTrade");
        BindRow(IndustryBuildingType.Administration, $"{RowsPath}/RowAdministration");
    }

    public void Refresh(GameState state)
    {
        if (state == null)
        {
            return;
        }

        foreach (var binding in _rows.Values)
        {
            var preview = IndustrySystem.GetBuildCostPreview(binding.BuildingType);
            var count = GetBuildingCount(state, binding.BuildingType);
            var costText = BuildCostText(preview);

            binding.NameLabel.Text = preview.DisplayName;
            binding.CountLabel.Text = $"×{count}";
            binding.CostLabel.Text = costText;

            var hasManagers = state.Workers > 0;
            var canAfford = IndustrySystem.CanAffordBuildCost(state, preview);
            binding.BuildButton.Disabled = !(hasManagers && canAfford);
            binding.BuildButton.TooltipText = hasManagers
                ? (canAfford ? $"消耗：{costText}" : "资源不足，暂不可建造。")
                : "缺少管理人员，无法组织建造。";
        }
    }

    private void BindRow(IndustryBuildingType buildingType, string rowPath)
    {
        var nameLabel = GetNode<Label>($"{rowPath}/HeaderRow/NameLabel");
        var countLabel = GetNode<Label>($"{rowPath}/HeaderRow/CountLabel");
        var costLabel = GetNode<Label>($"{rowPath}/ActionRow/CostLabel");
        var buildButton = GetNode<Button>($"{rowPath}/ActionRow/BuildButton");

        buildButton.Text = "建造";
        buildButton.Pressed += () => BuildRequested?.Invoke(buildingType);
        _rows[buildingType] = new RowBinding(buildingType, nameLabel, countLabel, costLabel, buildButton);
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
}
