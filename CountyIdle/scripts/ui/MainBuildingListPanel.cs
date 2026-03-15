using CountyIdle.Models;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string BuildingListPanelPath = $"{RightPanelPath}/PanelContent/MainVBox/BuildingListBox";

    private BuildingListPanel? _buildingListPanel;

    private void BindBuildingListPanelNodes()
    {
        _buildingListPanel = GetNodeOrNull<BuildingListPanel>(BuildingListPanelPath);
        if (_buildingListPanel == null)
        {
            return;
        }

        _buildingListPanel.BuildRequested += OnBuildingListBuildRequested;
    }

    private void RefreshBuildingListPanel(GameState state)
    {
        _buildingListPanel?.Refresh(state);
    }

    private void OnBuildingListBuildRequested(IndustryBuildingType buildingType)
    {
        BuildIndustryBuildingWithPlacement(buildingType);
    }

    private void UnbindBuildingListPanelEvents()
    {
        if (_buildingListPanel == null)
        {
            return;
        }

        _buildingListPanel.BuildRequested -= OnBuildingListBuildRequested;
    }
}
