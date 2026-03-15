using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string SectOrganizationPanelScenePath = "res://scenes/ui/SectOrganizationPanel.tscn";

    private SectOrganizationPanel? _sectOrganizationPanel;

    private void CreateSectOrganizationPanel()
    {
        var panelScene = GD.Load<PackedScene>(SectOrganizationPanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _sectOrganizationPanel = panelScene.Instantiate<SectOrganizationPanel>();
        _sectOrganizationPanel.SupportRequested += OnSectOrganizationSupportRequested;
        _sectOrganizationPanel.SupportResetRequested += OnSectOrganizationSupportResetRequested;
        _sectOrganizationPanel.GovernanceRequested += OnSectOrganizationGovernanceRequested;
        _sectOrganizationPanel.Opened += OnSectOrganizationPanelOpened;
        _sectOrganizationPanel.Closed += OnSectOrganizationPanelClosed;
        AddChild(_sectOrganizationPanel);
        MoveChild(_sectOrganizationPanel, GetChildCount() - 1);
    }

    private void BindSectOrganizationButtonEvent()
    {
        var organizationPanelButton = GetSectOrganizationPanelButton();
        if (organizationPanelButton == null)
        {
            return;
        }

        organizationPanelButton.Pressed += OpenSectOrganizationPanel;
    }

    private void OpenSectOrganizationPanel()
    {
        CloseBlockingOverlayPopups(_sectOrganizationPanel);
        _sectOrganizationPanel?.Open(_gameLoop.State.Clone());
    }

    private void OpenSectOrganizationPanelForJob(JobType jobType)
    {
        CloseBlockingOverlayPopups(_sectOrganizationPanel);
        _sectOrganizationPanel?.Open(_gameLoop.State.Clone(), jobType);
    }

    private void RefreshSectOrganizationPanelPopup(GameState state)
    {
        _sectOrganizationPanel?.RefreshState(state);
    }

    private void OnSectOrganizationSupportRequested(SectPeakSupportType supportType)
    {
        _gameLoop.SetPeakSupport(supportType);
    }

    private void OnSectOrganizationSupportResetRequested()
    {
        _gameLoop.ResetPeakSupport();
    }

    private void OnSectOrganizationGovernanceRequested(JobType jobType)
    {
        OpenTaskPanelForJob(jobType);
        AppendLog($"已从宗门组织谱系转入【{SectTaskRules.GetJobButtonText(jobType)}】。");
    }

    private void OnSectOrganizationPanelOpened()
    {
        SetSectOrganizationQuickButtonState(true);
    }

    private void OnSectOrganizationPanelClosed()
    {
        SetSectOrganizationQuickButtonState(false);
    }

    private void UnbindSectOrganizationPanelEvents()
    {
        var organizationPanelButton = GetSectOrganizationPanelButton();
        if (organizationPanelButton != null)
        {
            organizationPanelButton.Pressed -= OpenSectOrganizationPanel;
        }

        if (_sectOrganizationPanel == null)
        {
            return;
        }

        _sectOrganizationPanel.SupportRequested -= OnSectOrganizationSupportRequested;
        _sectOrganizationPanel.SupportResetRequested -= OnSectOrganizationSupportResetRequested;
        _sectOrganizationPanel.GovernanceRequested -= OnSectOrganizationGovernanceRequested;
        _sectOrganizationPanel.Opened -= OnSectOrganizationPanelOpened;
        _sectOrganizationPanel.Closed -= OnSectOrganizationPanelClosed;
    }

    private Button? GetSectOrganizationPanelButton()
    {

        return GetNodeOrNull<Button>($"{BottomBarPath}/BarPadding/MainRow/QuickActionRow/OrganizationQuickButton");
    }

    private void SetSectOrganizationQuickButtonState(bool pressed)
    {
        var bottomQuickButton = GetSectOrganizationPanelButton();
        if (bottomQuickButton == null)
        {
            return;
        }

        bottomQuickButton.ToggleMode = true;
        bottomQuickButton.ButtonPressed = pressed;
    }
}

