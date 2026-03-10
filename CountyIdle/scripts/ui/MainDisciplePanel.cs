using Godot;
using CountyIdle.Models;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string DisciplePanelScenePath = "res://scenes/ui/DisciplePanel.tscn";

    private DisciplePanel? _disciplePanel;

    private void CreateDisciplePanel()
    {
        var panelScene = GD.Load<PackedScene>(DisciplePanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _disciplePanel = panelScene.Instantiate<DisciplePanel>();
        _disciplePanel.Opened += OnDisciplePanelOpened;
        _disciplePanel.Closed += OnDisciplePanelClosed;
        AddChild(_disciplePanel);
        MoveChild(_disciplePanel, GetChildCount() - 1);
    }

    private void BindDiscipleButtonEvent()
    {
        var disciplePanelButton = GetDisciplePanelButton();
        if (disciplePanelButton == null)
        {
            return;
        }

        disciplePanelButton.Pressed += OpenDisciplePanel;
    }

    private void BindDiscipleMapInspectionEvent()
    {
        if (_sectMapRenderer == null)
        {
            return;
        }

        _sectMapRenderer.DiscipleInspectionRequested += OpenDisciplePanelForMapSelection;
    }

    private void OpenDisciplePanel()
    {
        _disciplePanel?.Open(_gameLoop.State.Clone());
    }

    private void OpenDisciplePanelForMapSelection(int discipleId, JobType? preferredJobType)
    {
        _disciplePanel?.Open(_gameLoop.State.Clone(), discipleId, preferredJobType);
    }

    private void RefreshDisciplePanelPopup(GameState state)
    {
        _disciplePanel?.RefreshState(state);
    }

    private void OnDisciplePanelOpened()
    {
        SetDiscipleQuickButtonState(true);
    }

    private void OnDisciplePanelClosed()
    {
        SetDiscipleQuickButtonState(false);
    }

    private void UnbindDisciplePanelEvents()
    {
        var disciplePanelButton = GetDisciplePanelButton();
        if (disciplePanelButton != null)
        {
            disciplePanelButton.Pressed -= OpenDisciplePanel;
        }

        if (_sectMapRenderer != null)
        {
            _sectMapRenderer.DiscipleInspectionRequested -= OpenDisciplePanelForMapSelection;
        }

        if (_disciplePanel == null)
        {
            return;
        }

        _disciplePanel.Opened -= OnDisciplePanelOpened;
        _disciplePanel.Closed -= OnDisciplePanelClosed;
    }

    private Button? GetDisciplePanelButton()
    {
        if (_useFigmaLayout)
        {
            return null;
        }

        var bottomQuickButton = GetNodeOrNull<Button>($"{BottomBarPath}/BarPadding/MainRow/QuickActionRow/DiscipleQuickButton");
        if (bottomQuickButton != null)
        {
            return bottomQuickButton;
        }

        return GetNodeOrNull<Button>($"{CenterTopTabRowPath}/DisciplePanelButton");
    }

    private void SetDiscipleQuickButtonState(bool pressed)
    {
        var bottomQuickButton = GetDisciplePanelButton();
        if (bottomQuickButton == null)
        {
            return;
        }

        bottomQuickButton.ToggleMode = true;
        bottomQuickButton.ButtonPressed = pressed;
    }
}
