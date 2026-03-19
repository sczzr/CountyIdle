using Godot;

namespace CountyIdle;

public partial class Main
{
    private void ConfigureLegacyFocusNavigation()
    {

        SetLegacyMapSurfaceFocusMode(Control.FocusModeEnum.None);

        var warehouseQuickButton = GetWarehousePanelButton();
        var taskQuickButton = GetTaskPanelButton();
        var organizationQuickButton = GetSectOrganizationPanelButton();
        var discipleQuickButton = GetDisciplePanelButton();
        var cultivationQuickButton = GetCultivationPanelButton();
        Control? mapZoomSliderControl = _mapZoomSlider;
        Control? mapZoomResetButtonControl = _mapZoomResetButton;
        Control? worldMapButtonControl = _worldMapButton;

        LinkFocusNeighbors(_worldMapButton,
            left: taskQuickButton,
            down: organizationQuickButton ?? taskQuickButton);
        LinkFocusNeighbors(_mapZoomSlider,
            left: _worldMapButton,
            right: _mapZoomResetButton,
            down: organizationQuickButton ?? taskQuickButton);
        LinkFocusNeighbors(_mapZoomResetButton,
            left: _mapZoomSlider,
            down: cultivationQuickButton ?? discipleQuickButton ?? organizationQuickButton ?? taskQuickButton);

        LinkFocusNeighbors(_mapPrimaryActionButton,
            left: taskQuickButton,
            top: _worldMapButton,
            right: _mapSecondaryActionButton,
            down: taskQuickButton);
        LinkFocusNeighbors(_mapSecondaryActionButton,
            left: organizationQuickButton ?? taskQuickButton,
            top: _mapZoomResetButton,
            down: cultivationQuickButton ?? discipleQuickButton ?? organizationQuickButton ?? taskQuickButton);

        LinkFocusNeighbors(warehouseQuickButton,
            top: _worldMapButton);
        LinkFocusNeighbors(taskQuickButton,
            top: _worldMapButton);
        LinkFocusNeighbors(organizationQuickButton,
            top: mapZoomSliderControl ?? worldMapButtonControl);
        LinkFocusNeighbors(discipleQuickButton,
            top: mapZoomResetButtonControl ?? mapZoomSliderControl ?? worldMapButtonControl);
        LinkFocusNeighbors(cultivationQuickButton,
            top: mapZoomResetButtonControl ?? mapZoomSliderControl ?? worldMapButtonControl);
    }

    private void SetLegacyMapSurfaceFocusMode(Control.FocusModeEnum focusMode)
    {
        if (_worldMapView != null)
        {
            _worldMapView.FocusMode = focusMode;
        }

        if (_prefectureMapView != null)
        {
            _prefectureMapView.FocusMode = focusMode;
        }

        if (_countyTownMapView != null)
        {
            _countyTownMapView.FocusMode = focusMode;
        }
    }

    private static void LinkFocusNeighbors(
        Control? source,
        Control? left = null,
        Control? top = null,
        Control? right = null,
        Control? down = null)
    {
        if (source == null)
        {
            return;
        }

        if (left != null)
        {
            source.FocusNeighborLeft = source.GetPathTo(left);
        }

        if (top != null)
        {
            source.FocusNeighborTop = source.GetPathTo(top);
        }

        if (right != null)
        {
            source.FocusNeighborRight = source.GetPathTo(right);
        }

        if (down != null)
        {
            source.FocusNeighborBottom = source.GetPathTo(down);
        }
    }
}
