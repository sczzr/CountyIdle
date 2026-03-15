using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string TaskPanelScenePath = "res://scenes/ui/TaskPanel.tscn";

    private TaskPanel? _taskPanel;

    private void CreateTaskPanel()
    {
        var panelScene = GD.Load<PackedScene>(TaskPanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _taskPanel = panelScene.Instantiate<TaskPanel>();
        _taskPanel.OrderAdjustmentRequested += OnTaskOrderAdjustmentRequested;
        _taskPanel.DevelopmentDirectionShiftRequested += OnDevelopmentDirectionShiftRequested;
        _taskPanel.SectLawShiftRequested += OnSectLawShiftRequested;
        _taskPanel.TalentPlanShiftRequested += OnTalentPlanShiftRequested;
        _taskPanel.QuarterDecreeShiftRequested += OnQuarterDecreeShiftRequested;
        _taskPanel.AffairsRuleShiftRequested += OnAffairsRuleShiftRequested;
        _taskPanel.DoctrineRuleShiftRequested += OnDoctrineRuleShiftRequested;
        _taskPanel.DisciplineRuleShiftRequested += OnDisciplineRuleShiftRequested;
        _taskPanel.ResetRequested += OnTaskOrdersResetRequested;
        _taskPanel.Opened += OnTaskPanelOpened;
        _taskPanel.Closed += OnTaskPanelClosed;
        AddChild(_taskPanel);
        MoveChild(_taskPanel, GetChildCount() - 1);
    }

    private void BindTaskButtonEvent()
    {
        var taskPanelButton = GetTaskPanelButton();
        if (taskPanelButton == null)
        {
            return;
        }

        taskPanelButton.Pressed += OpenTaskPanel;
    }

    private void OpenTaskPanel()
    {
        if (_taskPanel == null)
        {
            return;
        }

        CloseBlockingOverlayPopups(_taskPanel);
        _taskPanel.Open(_gameLoop.State.Clone());
    }

    private void OpenTaskPanelForJob(JobType jobType)
    {
        if (_taskPanel == null)
        {
            return;
        }

        CloseBlockingOverlayPopups(_taskPanel);
        _taskPanel.Open(_gameLoop.State.Clone(), SectTaskRules.GetPrimaryTaskForJob(jobType));
    }

    private void RefreshTaskPanelPopup(GameState state)
    {
        _taskPanel?.RefreshState(state);
    }

    private void OnTaskOrderAdjustmentRequested(SectTaskType taskType, int delta)
    {
        _gameLoop.AdjustTaskOrder(taskType, delta);
    }

    private void OnTaskOrdersResetRequested()
    {
        _gameLoop.ResetGovernance();
    }

    private void OnDevelopmentDirectionShiftRequested(int delta)
    {
        _gameLoop.ShiftDevelopmentDirection(delta);
    }

    private void OnSectLawShiftRequested(int delta)
    {
        _gameLoop.ShiftSectLaw(delta);
    }

    private void OnTalentPlanShiftRequested(int delta)
    {
        _gameLoop.ShiftTalentPlan(delta);
    }

    private void OnQuarterDecreeShiftRequested(int delta)
    {
        _gameLoop.ShiftQuarterDecree(delta);
    }

    private void OnAffairsRuleShiftRequested(int delta)
    {
        _gameLoop.ShiftAffairsRule(delta);
    }

    private void OnDoctrineRuleShiftRequested(int delta)
    {
        _gameLoop.ShiftDoctrineRule(delta);
    }

    private void OnDisciplineRuleShiftRequested(int delta)
    {
        _gameLoop.ShiftDisciplineRule(delta);
    }

    private void OnTaskPanelOpened()
    {
        SetTaskQuickButtonState(true);
    }

    private void OnTaskPanelClosed()
    {
        SetTaskQuickButtonState(false);
    }

    private void UnbindTaskPanelEvents()
    {
        var taskPanelButton = GetTaskPanelButton();
        if (taskPanelButton != null)
        {
            taskPanelButton.Pressed -= OpenTaskPanel;
        }

        if (_taskPanel == null)
        {
            return;
        }

        _taskPanel.OrderAdjustmentRequested -= OnTaskOrderAdjustmentRequested;
        _taskPanel.DevelopmentDirectionShiftRequested -= OnDevelopmentDirectionShiftRequested;
        _taskPanel.SectLawShiftRequested -= OnSectLawShiftRequested;
        _taskPanel.TalentPlanShiftRequested -= OnTalentPlanShiftRequested;
        _taskPanel.QuarterDecreeShiftRequested -= OnQuarterDecreeShiftRequested;
        _taskPanel.AffairsRuleShiftRequested -= OnAffairsRuleShiftRequested;
        _taskPanel.DoctrineRuleShiftRequested -= OnDoctrineRuleShiftRequested;
        _taskPanel.DisciplineRuleShiftRequested -= OnDisciplineRuleShiftRequested;
        _taskPanel.ResetRequested -= OnTaskOrdersResetRequested;
        _taskPanel.Opened -= OnTaskPanelOpened;
        _taskPanel.Closed -= OnTaskPanelClosed;
    }

    private Button? GetTaskPanelButton()
    {
        if (_useFigmaLayout)
        {
            return null;
        }

        var bottomQuickButton = GetNodeOrNull<Button>($"{BottomBarPath}/BarPadding/MainRow/QuickActionRow/TaskQuickButton");
        if (bottomQuickButton != null)
        {
            return bottomQuickButton;
        }

        return GetNodeOrNull<Button>($"{CenterTopTabRowPath}/TaskPanelButton");
    }

    private void SetTaskQuickButtonState(bool pressed)
    {
        var bottomQuickButton = GetTaskPanelButton();
        if (bottomQuickButton == null)
        {
            return;
        }

        bottomQuickButton.ToggleMode = true;
        bottomQuickButton.ButtonPressed = pressed;
    }
}
