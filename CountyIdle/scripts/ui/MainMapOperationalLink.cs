using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle;

public partial class Main
{
    private readonly MapOperationalLinkSystem _mapOperationalLinkSystem = new();
    private Control? _mapDirectiveRow;
    private Label? _mapStatusLabel;
    private Button? _mapPrimaryActionButton;
    private Button? _mapSecondaryActionButton;
    private MapDirectiveAction _mapPrimaryDirectiveAction = MapDirectiveAction.None;
    private MapDirectiveAction _mapSecondaryDirectiveAction = MapDirectiveAction.None;

    private void BindMapOperationalLegacyNodes()
    {
        _mapDirectiveRow = GetNodeOrNull<Control>($"{CenterPanelContentPath}/MapDirectiveRow");
        _mapStatusLabel = GetNodeOrNull<Label>($"{CenterPanelContentPath}/MapDirectiveRow/MapStatusLabel");
        _mapPrimaryActionButton = GetNodeOrNull<Button>($"{CenterPanelContentPath}/MapDirectiveRow/MapPrimaryActionButton");
        _mapSecondaryActionButton = GetNodeOrNull<Button>($"{CenterPanelContentPath}/MapDirectiveRow/MapSecondaryActionButton");
    }

    private void ClearMapOperationalNodes()
    {
        _mapDirectiveRow = null;
        _mapStatusLabel = null;
        _mapPrimaryActionButton = null;
        _mapSecondaryActionButton = null;
        _mapPrimaryDirectiveAction = MapDirectiveAction.None;
        _mapSecondaryDirectiveAction = MapDirectiveAction.None;
    }

    private void BindMapOperationalEvents()
    {
        if (_mapPrimaryActionButton != null)
        {
            _mapPrimaryActionButton.Pressed += () => ExecuteMapDirective(_mapPrimaryDirectiveAction);
        }

        if (_mapSecondaryActionButton != null)
        {
            _mapSecondaryActionButton.Pressed += () => ExecuteMapDirective(_mapSecondaryDirectiveAction);
        }
    }

    private void RefreshMapOperationalLinkUi(GameState? state = null)
    {
        state ??= _gameLoop?.State;
        if (state == null)
        {
            if (_mapDirectiveRow != null)
            {
                _mapDirectiveRow.Visible = false;
            }

            return;
        }

        var snapshot = _mapOperationalLinkSystem.BuildSnapshot(state, ResolveMapRegionScope(_currentMapTab));
        (_worldMapView as StrategicMapViewSystem)?.RefreshOperationalState(snapshot.WorldStyle);
        (_prefectureMapView as StrategicMapViewSystem)?.RefreshOperationalState(snapshot.PrefectureStyle);
        _countyTownMapRenderer?.RefreshOperationalState(snapshot.CountyTownStyle);

        if (_mapDirectiveRow == null || _mapStatusLabel == null || _mapPrimaryActionButton == null || _mapSecondaryActionButton == null)
        {
            return;
        }

        _mapDirectiveRow.Visible = snapshot.ShowDirectiveRow;
        if (!_mapDirectiveRow.Visible)
        {
            return;
        }

        _mapStatusLabel.Text = snapshot.ActiveStatusText;
        _mapStatusLabel.Modulate = snapshot.ActiveStatusColor;

        _mapPrimaryDirectiveAction = snapshot.PrimaryChoice.Action;
        _mapSecondaryDirectiveAction = snapshot.SecondaryChoice.Action;

        ApplyDirectiveChoice(_mapPrimaryActionButton, snapshot.PrimaryChoice);
        ApplyDirectiveChoice(_mapSecondaryActionButton, snapshot.SecondaryChoice);
    }

    private void ExecuteMapDirective(MapDirectiveAction action)
    {
        if (_gameLoop == null || action == MapDirectiveAction.None)
        {
            return;
        }

        _gameLoop.ExecuteMapDirective(action);
    }

    private static void ApplyDirectiveChoice(Button button, MapDirectiveChoice choice)
    {
        button.Text = string.IsNullOrWhiteSpace(choice.Label) ? "暂无调度" : choice.Label;
        button.TooltipText = choice.HintText;
        button.Disabled = !choice.Enabled;
    }

    private static MapRegionScope ResolveMapRegionScope(MapTab mapTab)
    {
        return mapTab switch
        {
            MapTab.World => MapRegionScope.World,
            MapTab.Prefecture => MapRegionScope.Prefecture,
            MapTab.CountyTown => MapRegionScope.CountyTown,
            _ => MapRegionScope.None
        };
    }
}
