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
    }

    private Button? GetWarehousePanelButton()
    {
        if (_useFigmaLayout)
        {
            return null;
        }

        return GetNodeOrNull<Button>($"{CenterTopTabRowPath}/WarehousePanelButton");
    }
}
