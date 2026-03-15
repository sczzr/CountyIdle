using Godot;
using CountyIdle.Models;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string WarehousePanelScenePath = "res://scenes/ui/WarehousePanel.tscn";

    private WarehousePanel? _warehousePanel;

    private void CreateWarehousePanel()
    {
        var panelScene = GD.Load<PackedScene>(WarehousePanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _warehousePanel = panelScene.Instantiate<WarehousePanel>();
        _warehousePanel.UpgradeMineWarehouseRequested += OnWarehouseUpgradeRequested;
        _warehousePanel.CraftToolsRequested += OnWarehouseCraftToolsRequested;
        _warehousePanel.BuildWorkshopRequested += OnWarehouseBuildWorkshopRequested;
        _warehousePanel.BuildAdministrationRequested += OnWarehouseBuildAdministrationRequested;
        _warehousePanel.BuildForestryChainRequested += OnWarehouseBuildForestryChainRequested;
        _warehousePanel.BuildMasonryChainRequested += OnWarehouseBuildMasonryChainRequested;
        _warehousePanel.BuildMedicinalChainRequested += OnWarehouseBuildMedicinalChainRequested;
        _warehousePanel.BuildFiberChainRequested += OnWarehouseBuildFiberChainRequested;
        _warehousePanel.Opened += OnWarehousePanelOpened;
        _warehousePanel.Closed += OnWarehousePanelClosed;
        AddChild(_warehousePanel);
        MoveChild(_warehousePanel, GetChildCount() - 1);
    }

    private void BindWarehouseButtonEvent()
    {
        var warehousePanelButton = GetWarehousePanelButton();
        if (warehousePanelButton == null)
        {
            return;
        }

        warehousePanelButton.Pressed += OpenWarehousePanel;
    }

    private void OpenWarehousePanel()
    {
        if (_warehousePanel == null)
        {
            return;
        }

        CloseBlockingOverlayPopups(_warehousePanel);
        _warehousePanel.Open(_gameLoop.State.Clone());
    }

    private void RefreshWarehousePanelPopup(GameState state)
    {
        _warehousePanel?.RefreshState(state);
    }

    private void OnWarehouseUpgradeRequested()
    {
        OnMineUpgradePressed();
    }

    private void OnWarehouseCraftToolsRequested()
    {
        _gameLoop.CraftIndustryTools();
    }

    private void OnWarehouseBuildWorkshopRequested()
    {
        _gameLoop.BuildIndustryBuilding(IndustryBuildingType.Workshop);
    }

    private void OnWarehouseBuildAdministrationRequested()
    {
        _gameLoop.BuildIndustryBuilding(IndustryBuildingType.Administration);
    }

    private void OnWarehouseBuildForestryChainRequested()
    {
        _gameLoop.BuildTierZeroChain(TierZeroMaterialChainType.Forestry);
    }

    private void OnWarehouseBuildMasonryChainRequested()
    {
        _gameLoop.BuildTierZeroChain(TierZeroMaterialChainType.Masonry);
    }

    private void OnWarehouseBuildMedicinalChainRequested()
    {
        _gameLoop.BuildTierZeroChain(TierZeroMaterialChainType.Medicinal);
    }

    private void OnWarehouseBuildFiberChainRequested()
    {
        _gameLoop.BuildTierZeroChain(TierZeroMaterialChainType.Fiber);
    }

    private void OnWarehousePanelOpened()
    {
        SetWarehouseQuickButtonState(true);
    }

    private void OnWarehousePanelClosed()
    {
        SetWarehouseQuickButtonState(false);
    }

    private void UnbindWarehousePanelEvents()
    {
        var warehousePanelButton = GetWarehousePanelButton();
        if (warehousePanelButton != null)
        {
            warehousePanelButton.Pressed -= OpenWarehousePanel;
        }

        if (_warehousePanel == null)
        {
            return;
        }

        _warehousePanel.UpgradeMineWarehouseRequested -= OnWarehouseUpgradeRequested;
        _warehousePanel.CraftToolsRequested -= OnWarehouseCraftToolsRequested;
        _warehousePanel.BuildWorkshopRequested -= OnWarehouseBuildWorkshopRequested;
        _warehousePanel.BuildAdministrationRequested -= OnWarehouseBuildAdministrationRequested;
        _warehousePanel.BuildForestryChainRequested -= OnWarehouseBuildForestryChainRequested;
        _warehousePanel.BuildMasonryChainRequested -= OnWarehouseBuildMasonryChainRequested;
        _warehousePanel.BuildMedicinalChainRequested -= OnWarehouseBuildMedicinalChainRequested;
        _warehousePanel.BuildFiberChainRequested -= OnWarehouseBuildFiberChainRequested;
        _warehousePanel.Opened -= OnWarehousePanelOpened;
        _warehousePanel.Closed -= OnWarehousePanelClosed;
    }

    private Button? GetWarehousePanelButton()
    {

        var bottomQuickButton = GetNodeOrNull<Button>($"{BottomBarPath}/BarPadding/MainRow/QuickActionRow/WarehouseQuickButton");
        if (bottomQuickButton != null)
        {
            return bottomQuickButton;
        }

        return GetNodeOrNull<Button>($"{CenterTopTabRowPath}/WarehousePanelButton");
    }

    private void SetWarehouseQuickButtonState(bool pressed)
    {
        var bottomQuickButton = GetWarehousePanelButton();
        if (bottomQuickButton == null)
        {
            return;
        }

        bottomQuickButton.ToggleMode = true;
        bottomQuickButton.ButtonPressed = pressed;
    }
}

