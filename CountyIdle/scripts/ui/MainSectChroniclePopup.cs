using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string SectChroniclePanelScenePath = "res://scenes/ui/SectChroniclePanel.tscn";

    private SectChroniclePanel? _sectChroniclePanel;
    private Button? _openSectChronicleButton;

    private void CreateSectChroniclePanel()
    {
        var panelScene = GD.Load<PackedScene>(SectChroniclePanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _sectChroniclePanel = panelScene.Instantiate<SectChroniclePanel>();
        _sectChroniclePanel.Opened += OnSectChroniclePanelOpened;
        _sectChroniclePanel.Closed += OnSectChroniclePanelClosed;
        AddChild(_sectChroniclePanel);
        MoveChild(_sectChroniclePanel, GetChildCount() - 1);
    }

    private void BindSectChronicleEntry()
    {
        _openSectChronicleButton = GetNodeOrNull<Button>($"{RightPanelPath}/PanelContent/MainVBox/TitleRow/OpenChronicleButton");
        if (_openSectChronicleButton == null)
        {
            return;
        }

        _openSectChronicleButton.ToggleMode = true;
        _openSectChronicleButton.Pressed += OpenSectChroniclePanel;
    }

    private void OpenSectChroniclePanel()
    {
        if (_sectChroniclePanel == null)
        {
            return;
        }

        CloseBlockingOverlayPopups(_sectChroniclePanel);
        _sectChroniclePanel.Open(_gameLoop.State.Clone(), GetRecentChronicleLogs(), GetSectChronicleSnapshots());
    }

    private void RefreshSectChroniclePanelPopup(GameState state)
    {
        _sectChroniclePanel?.RefreshState(state, GetRecentChronicleLogs(), GetSectChronicleSnapshots());
    }

    private List<string> GetRecentChronicleLogs()
    {
        return new List<string>(_logs);
    }

    private void OnSectChroniclePanelOpened()
    {
        SetSectChronicleButtonState(true);
    }

    private void OnSectChroniclePanelClosed()
    {
        SetSectChronicleButtonState(false);
    }

    private void SetSectChronicleButtonState(bool pressed)
    {
        if (_openSectChronicleButton == null)
        {
            return;
        }

        _openSectChronicleButton.ButtonPressed = pressed;
    }

    private void UnbindSectChroniclePanelEvents()
    {
        if (_openSectChronicleButton != null)
        {
            _openSectChronicleButton.Pressed -= OpenSectChroniclePanel;
        }

        if (_sectChroniclePanel == null)
        {
            return;
        }

        _sectChroniclePanel.Opened -= OnSectChroniclePanelOpened;
        _sectChroniclePanel.Closed -= OnSectChroniclePanelClosed;
    }
}
