using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle;

public partial class Main
{
    private Label? _sectChroniclePrimaryAlertLabel;
    private Label? _sectChronicleSecondaryAlertLabel;

    private void BindSectChronicleNodes()
    {
        _sectChroniclePrimaryAlertLabel = GetNodeOrNull<Label>($"{RightPanelPath}/PanelContent/MainVBox/AlertsBox/AlertsVBox/AlertsPadding/AlertsList/AlertItem1/AlertLabel");
        _sectChronicleSecondaryAlertLabel = GetNodeOrNull<Label>($"{RightPanelPath}/PanelContent/MainVBox/AlertsBox/AlertsVBox/AlertsPadding/AlertsList/AlertItem2/AlertLabel");
    }

    private void ClearSectChronicleNodes()
    {
        _sectChroniclePrimaryAlertLabel = null;
        _sectChronicleSecondaryAlertLabel = null;
    }

    private void RefreshSectChroniclePanel(GameState state)
    {
        if (_sectChroniclePrimaryAlertLabel == null || _sectChronicleSecondaryAlertLabel == null)
        {
            return;
        }

        var calendarInfo = _gameCalendarSystem.Describe(state.GameMinutes);
        var summary = SectChronicleRules.BuildSummary(state, calendarInfo);

        _sectChroniclePrimaryAlertLabel.Text = summary.PrimaryAlertText;
        _sectChroniclePrimaryAlertLabel.TooltipText = summary.PrimaryAlertText;
        _sectChronicleSecondaryAlertLabel.Text = summary.SecondaryAlertText;
        _sectChronicleSecondaryAlertLabel.TooltipText = summary.SecondaryAlertText;
    }
}
