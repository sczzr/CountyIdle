using System;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class WarehousePanel : PopupPanelBase
{
    private Label _warehouseStatusValue = null!;
    private ProgressBar _warehouseLoadBar = null!;
    private Label _baseResourcesValue = null!;
    private Label _oreResourcesValue = null!;
    private Label _materialResourcesValue = null!;
    private Label _tierZeroChainStatusValue = null!;
    private Button _upgradeButton = null!;
    private Button _craftToolsButton = null!;
    private Button _buildWorkshopButton = null!;
    private Button _buildAdministrationButton = null!;
    private Button _buildForestryChainButton = null!;
    private Button _buildMasonryChainButton = null!;
    private Button _buildMedicinalChainButton = null!;
    private Button _buildFiberChainButton = null!;
    private Button _closeButton = null!;
    private Button _cancelButton = null!;
    private double _warehouseLoadRate;

    public event Action? UpgradeMineWarehouseRequested;
    public event Action? CraftToolsRequested;
    public event Action? BuildWorkshopRequested;
    public event Action? BuildAdministrationRequested;
    public event Action? BuildForestryChainRequested;
    public event Action? BuildMasonryChainRequested;
    public event Action? BuildMedicinalChainRequested;
    public event Action? BuildFiberChainRequested;

    public override void _Ready()
    {
        _warehouseStatusValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/StatusSection/WarehouseStatusValue");
        _warehouseLoadBar = GetNode<ProgressBar>("CenterLayer/Dialog/Margin/MainColumn/StatusSection/WarehouseLoadBar");
        _baseResourcesValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/CategorySection/BaseResourcesValue");
        _oreResourcesValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/CategorySection/OreResourcesValue");
        _materialResourcesValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/CategorySection/MaterialResourcesValue");
        _tierZeroChainStatusValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroStatusValue");
        _upgradeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/PrimaryActionRow/UpgradeButton");
        _craftToolsButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/PrimaryActionRow/CraftToolsButton");
        _buildWorkshopButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/BuildActionRow/BuildWorkshopButton");
        _buildAdministrationButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/BuildActionRow/BuildAdministrationButton");
        _buildForestryChainButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroActionRowTop/BuildForestryChainButton");
        _buildMasonryChainButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroActionRowTop/BuildMasonryChainButton");
        _buildMedicinalChainButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroActionRowBottom/BuildMedicinalChainButton");
        _buildFiberChainButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroActionRowBottom/BuildFiberChainButton");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _cancelButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CloseFooterButton");

        InitializePopupHint("CenterLayer/Dialog/Margin/MainColumn/HintLabel");
        BindEvents();
        Hide();
    }

    public void Open(GameState state)
    {
        RefreshState(state);
        OpenPopup();
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

    public void RefreshState(GameState state)
    {
        var used = state.GetWarehouseUsed();
        var capacity = Math.Max(state.WarehouseCapacity, 1.0);
        var usedRate = (used / capacity) * 100.0;
        _warehouseLoadRate = usedRate;

        _warehouseStatusValue.Text =
            $"仓储Lv.{state.WarehouseLevel} · 矿坑Lv.{state.MiningLevel} · 占用 {used:0}/{capacity:0} ({usedRate:0}%)";
        _warehouseLoadBar.MaxValue = capacity;
        _warehouseLoadBar.Value = Math.Clamp(used, 0, capacity);

        _baseResourcesValue.Text =
            $"基础库存：粮 {state.Food:0} · 木料 {state.Wood:0} · 石料 {state.Stone:0} · 金 {state.Gold:0}\n" +
            $"自然原料：林木 {state.Timber:0} · 原石 {state.RawStone:0} · 黏土 {state.Clay:0} · 卤水 {state.Brine:0}\n" +
            $"民生原料：药材 {state.Herbs:0} · 麻料 {state.HempFiber:0} · 芦苇 {state.Reeds:0} · 皮毛 {state.Hides:0}";
        _oreResourcesValue.Text =
            $"矿产：铁矿 {state.IronOre:0} · 铜矿 {state.CopperOre:0} · 煤矿 {state.Coal:0} · 稀材 {state.RareMaterial:0}\n" +
            $"冶铸：铜锭 {state.CopperIngot:0} · 熟铁 {state.WroughtIron:0} · 复材 {state.CompositeMaterial:0}";
        _materialResourcesValue.Text =
            $"民生材料：精盐 {state.FineSalt:0} · 药剂 {state.HerbalMedicine:0} · 麻布 {state.HempCloth:0} · 皮革 {state.Leather:0}\n" +
            $"营造产物：工业部件 {state.IndustrialParts:0} · 建造构件 {state.ConstructionMaterials:0}\n" +
            $"衣物储备 {state.ClothingStock:0} · 工具库存 {state.IndustryTools:0}";
        _tierZeroChainStatusValue.Text = MaterialRules.DescribeTierZeroChains(state);

        _upgradeButton.Text = state.TechLevel >= 1
            ? $"矿仓联建（矿Lv.{state.MiningLevel}/仓Lv.{state.WarehouseLevel}）"
            : "🔒 需科技 锻造术(T1)";
        _craftToolsButton.Text = $"制工具（库存 {state.IndustryTools:0}）";
        _buildWorkshopButton.Text = $"扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}（{state.WorkshopBuildings}）";
        _buildAdministrationButton.Text = $"扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)}（{state.AdministrationBuildings}）";
        _buildForestryChainButton.Text = $"林木链 Lv.{state.ForestryChainLevel}";
        _buildMasonryChainButton.Text = $"石陶链 Lv.{state.MasonryChainLevel}";
        _buildMedicinalChainButton.Text = $"盐药链 Lv.{state.MedicinalChainLevel}";
        _buildFiberChainButton.Text = $"纤皮链 Lv.{state.FiberChainLevel}";
        RefreshPopupHint();
    }

    private void BindEvents()
    {
        _closeButton.Pressed += ClosePopup;
        _cancelButton.Pressed += ClosePopup;
        _upgradeButton.Pressed += () => HandleWarehouseAction("已发送矿仓联建请求，请查看仓储容量与资源变化。", UpgradeMineWarehouseRequested);
        _craftToolsButton.Pressed += () => HandleWarehouseAction("已发送制工具请求，请查看工具库存与日志。", CraftToolsRequested);
        _buildWorkshopButton.Pressed += () => HandleWarehouseAction($"已发送扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}请求，请查看建筑数量与资源变化。", BuildWorkshopRequested);
        _buildAdministrationButton.Pressed += () => HandleWarehouseAction($"已发送扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)}请求，请查看建筑数量与资源变化。", BuildAdministrationRequested);
        _buildForestryChainButton.Pressed += () => HandleWarehouseAction("已发送林木链扩建请求，请查看原木与木料变化。", BuildForestryChainRequested);
        _buildMasonryChainButton.Pressed += () => HandleWarehouseAction("已发送石陶链扩建请求，请查看原石、黏土与建材变化。", BuildMasonryChainRequested);
        _buildMedicinalChainButton.Pressed += () => HandleWarehouseAction("已发送盐药链扩建请求，请查看卤水、药材与民生材料变化。", BuildMedicinalChainRequested);
        _buildFiberChainButton.Pressed += () => HandleWarehouseAction("已发送纤皮链扩建请求，请查看麻料、芦苇、皮毛与衣料变化。", BuildFiberChainRequested);
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return _warehouseLoadRate >= 90.0
            ? "仓储接近满载，建议优先矿仓联建或尽快消耗材料。按 Esc 可快速关闭。"
            : "分类查看仓储库存，并直接扩建 T0 链路、矿仓与工具产能。按 Esc 可快速关闭。";
    }

    private void HandleWarehouseAction(string statusMessage, Action? requestedAction)
    {
        ShowPopupStatusMessage(statusMessage);
        requestedAction?.Invoke();
    }
}
