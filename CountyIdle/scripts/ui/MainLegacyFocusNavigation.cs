using Godot;

namespace CountyIdle;

public partial class Main
{
    private void ConfigureLegacyFocusNavigation()
    {
        if (_useFigmaLayout)
        {
            return;
        }

        SetLegacyMapSurfaceFocusMode(Control.FocusModeEnum.None);

        var warehouseQuickButton = GetWarehousePanelButton();
        var taskQuickButton = GetTaskPanelButton();
        var organizationQuickButton = GetSectOrganizationPanelButton();
        var discipleQuickButton = GetDisciplePanelButton();
        Control? mapZoomSliderControl = _mapZoomSlider;
        Control? mapZoomResetButtonControl = _mapZoomResetButton;
        Control? worldMapButtonControl = _worldMapButton;
        Control? countyTownMapButtonControl = _countyTownMapButton;

        LinkFocusNeighbors(_countyTownMapButton,
            left: _tileInspectorPrimaryButton ?? taskQuickButton,
            down: taskQuickButton);
        LinkFocusNeighbors(_worldMapButton,
            left: _countyTownMapButton,
            down: organizationQuickButton ?? taskQuickButton);
        LinkFocusNeighbors(_mapZoomSlider,
            left: _worldMapButton,
            right: _mapZoomResetButton,
            down: organizationQuickButton ?? taskQuickButton);
        LinkFocusNeighbors(_mapZoomResetButton,
            left: _mapZoomSlider,
            down: discipleQuickButton ?? organizationQuickButton ?? taskQuickButton);

        LinkFocusNeighbors(_mapPrimaryActionButton,
            left: _tileInspectorSecondaryButton ?? taskQuickButton,
            top: _countyTownMapButton,
            right: _mapSecondaryActionButton,
            down: taskQuickButton);
        LinkFocusNeighbors(_mapSecondaryActionButton,
            left: _tileInspectorTertiaryButton ?? organizationQuickButton ?? taskQuickButton,
            top: _mapZoomResetButton,
            down: discipleQuickButton ?? organizationQuickButton ?? taskQuickButton);

        LinkFocusNeighbors(_tileInspectorPrimaryButton,
            right: _countyTownMapButton);
        LinkFocusNeighbors(_tileInspectorSecondaryButton,
            right: _mapPrimaryActionButton ?? taskQuickButton);
        LinkFocusNeighbors(_tileInspectorTertiaryButton,
            right: _mapSecondaryActionButton ?? organizationQuickButton ?? taskQuickButton);

        LinkFocusNeighbors(warehouseQuickButton,
            top: _countyTownMapButton);
        LinkFocusNeighbors(taskQuickButton,
            top: _countyTownMapButton);
        LinkFocusNeighbors(organizationQuickButton,
            top: mapZoomSliderControl ?? worldMapButtonControl ?? countyTownMapButtonControl);
        LinkFocusNeighbors(discipleQuickButton,
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
