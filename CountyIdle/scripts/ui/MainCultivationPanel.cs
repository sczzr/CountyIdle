using Godot;
using CountyIdle.Models;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string CultivationPanelScenePath = "res://scenes/ui/CultivationPanel.tscn";

    private CultivationPanel? _cultivationPanel;

    private void CreateCultivationPanel()
    {
        var panelScene = GD.Load<PackedScene>(CultivationPanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _cultivationPanel = panelScene.Instantiate<CultivationPanel>();
        _cultivationPanel.AssignmentRequested += OnCultivationAssignmentRequested;
        _cultivationPanel.Opened += OnCultivationPanelOpened;
        _cultivationPanel.Closed += OnCultivationPanelClosed;
        AddChild(_cultivationPanel);
        MoveChild(_cultivationPanel, GetChildCount() - 1);
    }

    private void BindCultivationButtonEvent()
    {
        var cultivationPanelButton = GetCultivationPanelButton();
        if (cultivationPanelButton == null)
        {
            return;
        }

        cultivationPanelButton.Pressed += OpenCultivationPanel;
    }

    private void OpenCultivationPanel()
    {
        CloseBlockingOverlayPopups(_cultivationPanel);
        _cultivationPanel?.Open(_gameLoop.State.Clone());
    }

    private void OpenCultivationPanelForDisciple(int discipleId)
    {
        CloseBlockingOverlayPopups(_cultivationPanel);
        _cultivationPanel?.Open(_gameLoop.State.Clone(), discipleId);
    }

    private void RefreshCultivationPanelPopup(GameState state)
    {
        _cultivationPanel?.RefreshState(state);
    }

    private void OnCultivationAssignmentRequested(int discipleId, DiscipleCultivationAssignmentType assignmentType)
    {
        _gameLoop.SetDiscipleCultivationAssignment(discipleId, assignmentType);
    }

    private void OnCultivationPanelOpened()
    {
        SetCultivationQuickButtonState(true);
    }

    private void OnCultivationPanelClosed()
    {
        SetCultivationQuickButtonState(false);
    }

    private void UnbindCultivationPanelEvents()
    {
        var cultivationPanelButton = GetCultivationPanelButton();
        if (cultivationPanelButton != null)
        {
            cultivationPanelButton.Pressed -= OpenCultivationPanel;
        }

        if (_cultivationPanel == null)
        {
            return;
        }

        _cultivationPanel.AssignmentRequested -= OnCultivationAssignmentRequested;
        _cultivationPanel.Opened -= OnCultivationPanelOpened;
        _cultivationPanel.Closed -= OnCultivationPanelClosed;
    }

    private BaseButton? GetCultivationPanelButton()
    {
        return GetNodeOrNull<BaseButton>($"{BottomBarPath}/BarPadding/MainRow/QuickActionRow/CultivationQuickButton");
    }

    private void SetCultivationQuickButtonState(bool pressed)
    {
        var cultivationPanelButton = GetCultivationPanelButton();
        if (cultivationPanelButton == null)
        {
            return;
        }

        cultivationPanelButton.ToggleMode = true;
        cultivationPanelButton.ButtonPressed = pressed;
    }
}
