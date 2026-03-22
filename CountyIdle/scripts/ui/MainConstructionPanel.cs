using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string ConstructionPanelScenePath = "res://scenes/ui/ConstructionPanel.tscn";
    private const string ConstructionPanelButtonPath = $"{RightPanelPath}/PanelContent/MainVBox/BuildingListBox/BuildingListVBox/BuildingHeader/OpenConstructionButton";

    private ConstructionPanel? _constructionPanel;
    private TownMapSelectionSummary _lastTownMapSelectionSummary = TownMapSelectionSummary.CreateDefault();

    private void CreateConstructionPanel()
    {
        var panelScene = GD.Load<PackedScene>(ConstructionPanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _constructionPanel = panelScene.Instantiate<ConstructionPanel>();
        _constructionPanel.BuildRequested += OnConstructionBuildRequested;
        _constructionPanel.CancelCurrentRequested += OnConstructionCancelCurrentRequested;
        _constructionPanel.CancelPendingRequested += OnConstructionCancelPendingRequested;
        _constructionPanel.Opened += OnConstructionPanelOpened;
        _constructionPanel.Closed += OnConstructionPanelClosed;
        AddChild(_constructionPanel);
        MoveChild(_constructionPanel, GetChildCount() - 1);
    }

    private void BindConstructionPanelButtonEvent()
    {
        var button = GetConstructionPanelButton();
        if (button == null)
        {
            return;
        }

        button.Pressed += OpenConstructionPanel;
    }

    private void OpenConstructionPanel()
    {
        if (_constructionPanel == null || _gameLoop == null)
        {
            return;
        }

        CloseBlockingOverlayPopups(_constructionPanel);
        _constructionPanel.Open(_gameLoop.State.Clone(), ResolveConstructionSelectionSummary(_gameLoop.State));
    }

    private void RefreshConstructionPanelPopup(GameState state)
    {
        _constructionPanel?.RefreshState(state, ResolveConstructionSelectionSummary(state));
    }

    private void HandleConstructionSelectionSummaryChanged(TownMapSelectionSummary summary)
    {
        _lastTownMapSelectionSummary = summary;
        if (_constructionPanel == null || _gameLoop == null)
        {
            return;
        }

        _constructionPanel.RefreshState(_gameLoop.State, ResolveConstructionSelectionSummary(_gameLoop.State));
    }

    private TownMapSelectionSummary ResolveConstructionSelectionSummary(GameState? state)
    {
        if (_lastTownMapSelectionSummary == null)
        {
            return BuildFallbackSelectionSummary(state);
        }

        if (!_lastTownMapSelectionSummary.HasSelection)
        {
            return BuildFallbackSelectionSummary(state) ?? _lastTownMapSelectionSummary;
        }

        return _lastTownMapSelectionSummary;
    }

    private static TownMapSelectionSummary? BuildFallbackSelectionSummary(GameState? state)
    {
        if (state == null)
        {
            return TownMapSelectionSummary.CreateDefault();
        }

        var sectName = SectNamingRules.GetName(state.SectNameMap, SectNamingRules.SectNameKey);
        var peakName = SectNamingRules.GetName(state.SectNameMap, SectNamingRules.PeakTianyanKey);
        return TownMapSelectionSummary.CreateDefault(sectName, peakName);
    }

    private void OnConstructionBuildRequested(IndustryBuildingType buildingType)
    {
        BuildIndustryBuildingWithPlacement(buildingType);
    }

    private void OnConstructionCancelCurrentRequested()
    {
        _gameLoop?.CancelCurrentConstruction();
    }

    private void OnConstructionCancelPendingRequested()
    {
        _gameLoop?.CancelPendingConstruction();
    }

    private void OnConstructionPanelOpened()
    {
        SetConstructionPanelButtonState(true);
    }

    private void OnConstructionPanelClosed()
    {
        SetConstructionPanelButtonState(false);
    }

    private void UnbindConstructionPanelEvents()
    {
        var button = GetConstructionPanelButton();
        if (button != null)
        {
            button.Pressed -= OpenConstructionPanel;
        }

        if (_constructionPanel == null)
        {
            return;
        }

        _constructionPanel.BuildRequested -= OnConstructionBuildRequested;
        _constructionPanel.CancelCurrentRequested -= OnConstructionCancelCurrentRequested;
        _constructionPanel.CancelPendingRequested -= OnConstructionCancelPendingRequested;
        _constructionPanel.Opened -= OnConstructionPanelOpened;
        _constructionPanel.Closed -= OnConstructionPanelClosed;
    }

    private BaseButton? GetConstructionPanelButton()
    {
        var bottomButton = GetNodeOrNull<BaseButton>($"{BottomBarPath}/BarPadding/MainRow/QuickActionRow/ConstructionQuickButton");
        if (bottomButton != null)
        {
            return bottomButton;
        }

        return GetNodeOrNull<BaseButton>(ConstructionPanelButtonPath);
    }

    private void SetConstructionPanelButtonState(bool pressed)
    {
        var button = GetConstructionPanelButton();
        if (button == null)
        {
            return;
        }

        button.ToggleMode = true;
        button.ButtonPressed = pressed;
    }
}
