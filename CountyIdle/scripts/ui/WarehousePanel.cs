using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.UI;

public partial class WarehousePanel : PopupPanelBase
{
    private Label _warehouseStatusValue = null!;
    private ProgressBar _warehouseLoadBar = null!;
    private Label _baseResourcesValue = null!;
    private Label _oreResourcesValue = null!;
    private Label _materialResourcesValue = null!;
    private Button _upgradeButton = null!;
    private Button _craftToolsButton = null!;
    private Button _buildWorkshopButton = null!;
    private Button _buildAdministrationButton = null!;
    private Button _closeButton = null!;
    private Button _cancelButton = null!;
    private double _warehouseLoadRate;

    public event Action? UpgradeMineWarehouseRequested;
    public event Action? CraftToolsRequested;
    public event Action? BuildWorkshopRequested;
    public event Action? BuildAdministrationRequested;

    public override void _Ready()
    {
        _warehouseStatusValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/StatusSection/WarehouseStatusValue");
        _warehouseLoadBar = GetNode<ProgressBar>("CenterLayer/Dialog/Margin/MainColumn/StatusSection/WarehouseLoadBar");
        _baseResourcesValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/CategorySection/BaseResourcesValue");
        _oreResourcesValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/CategorySection/OreResourcesValue");
        _materialResourcesValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/CategorySection/MaterialResourcesValue");
        _upgradeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/PrimaryActionRow/UpgradeButton");
        _craftToolsButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/PrimaryActionRow/CraftToolsButton");
        _buildWorkshopButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/BuildActionRow/BuildWorkshopButton");
        _buildAdministrationButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/BuildActionRow/BuildAdministrationButton");
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
            $"粮 {state.Food:0} · 木 {state.Wood:0} · 石 {state.Stone:0} · 金 {state.Gold:0}";
        _oreResourcesValue.Text =
            $"铁矿 {state.IronOre:0} · 铜矿 {state.CopperOre:0} · 煤矿 {state.Coal:0} · 稀材 {state.RareMaterial:0}";
        _materialResourcesValue.Text =
            $"金属锭 {state.MetalIngot:0.0} · 复合材料 {state.CompositeMaterial:0.0} · 工业部件 {state.IndustrialParts:0.0} · 建造构件 {state.ConstructionMaterials:0.0}";

        _upgradeButton.Text = state.TechLevel >= 1
            ? $"矿仓联建（矿Lv.{state.MiningLevel}/仓Lv.{state.WarehouseLevel}）"
            : "🔒 需科技 锻造术(T1)";
        _craftToolsButton.Text = $"制工具（库存 {state.IndustryTools:0}）";
        _buildWorkshopButton.Text = $"扩建工坊（{state.WorkshopBuildings}）";
        _buildAdministrationButton.Text = $"扩建官署（{state.AdministrationBuildings}）";
        RefreshPopupHint();
    }

    private void BindEvents()
    {
        _closeButton.Pressed += ClosePopup;
        _cancelButton.Pressed += ClosePopup;
        _upgradeButton.Pressed += () => HandleWarehouseAction("已发送矿仓联建请求，请查看仓储容量与资源变化。", UpgradeMineWarehouseRequested);
        _craftToolsButton.Pressed += () => HandleWarehouseAction("已发送制工具请求，请查看工具库存与日志。", CraftToolsRequested);
        _buildWorkshopButton.Pressed += () => HandleWarehouseAction("已发送扩建工坊请求，请查看建筑数量与资源变化。", BuildWorkshopRequested);
        _buildAdministrationButton.Pressed += () => HandleWarehouseAction("已发送扩建官署请求，请查看建筑数量与资源变化。", BuildAdministrationRequested);
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return _warehouseLoadRate >= 90.0
            ? "仓储接近满载，建议优先矿仓联建或尽快消耗材料。按 Esc 可快速关闭。"
            : "分类查看仓储库存，并直接执行矿仓联建、工具制造与产能扩建。按 Esc 可快速关闭。";
    }

    private void HandleWarehouseAction(string statusMessage, Action? requestedAction)
    {
        ShowPopupStatusMessage(statusMessage);
        requestedAction?.Invoke();
    }
}
