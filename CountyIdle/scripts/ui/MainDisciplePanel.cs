using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;
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
        _disciplePanel.DirectiveRequested += OnDiscipleDirectiveRequested;
        _disciplePanel.CultivationRequested += OnDiscipleCultivationRequested;
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
        CloseBlockingOverlayPopups(_disciplePanel);
        DiscipleEquipmentRules.EnsureRosterEquipmentProfiles(_gameLoop.State);
        _disciplePanel?.Open(_gameLoop.State.Clone());
    }

    private void OpenDisciplePanelForMapSelection(int discipleId, JobType? preferredJobType)
    {
        CloseBlockingOverlayPopups(_disciplePanel);
        DiscipleEquipmentRules.EnsureRosterEquipmentProfiles(_gameLoop.State);
        _disciplePanel?.Open(_gameLoop.State.Clone(), discipleId, preferredJobType);
    }

    private void RefreshDisciplePanelPopup(GameState state)
    {
        DiscipleEquipmentRules.EnsureRosterEquipmentProfiles(state);
        _disciplePanel?.RefreshState(state);
    }

    private void OnDiscipleDirectiveRequested(int discipleId, DiscipleDirectiveType directiveType)
    {
        _gameLoop.SetDiscipleDirective(discipleId, directiveType);
    }

    private void OnDiscipleCultivationRequested(int discipleId)
    {
        OpenCultivationPanelForDisciple(discipleId);
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

        _disciplePanel.DirectiveRequested -= OnDiscipleDirectiveRequested;
        _disciplePanel.CultivationRequested -= OnDiscipleCultivationRequested;
        _disciplePanel.Opened -= OnDisciplePanelOpened;
        _disciplePanel.Closed -= OnDisciplePanelClosed;
    }

    private BaseButton? GetDisciplePanelButton()
    {
        var bottomQuickButton = GetNodeOrNull<BaseButton>($"{BottomBarPath}/BarPadding/MainRow/QuickActionRow/DiscipleQuickButton");
        if (bottomQuickButton != null)
        {
            return bottomQuickButton;
        }

        return GetNodeOrNull<BaseButton>($"{CenterTopTabRowPath}/DisciplePanelButton");
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

