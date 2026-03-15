using Godot;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string SectNamePanelScenePath = "res://scenes/ui/SectNamePanel.tscn";

    private SectNamePanel? _sectNamePanel;

    private void CreateSectNamePanel()
    {
        var panelScene = GD.Load<PackedScene>(SectNamePanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _sectNamePanel = panelScene.Instantiate<SectNamePanel>();
        _sectNamePanel.ApplyRequested += OnSectNameApplyRequested;
        AddChild(_sectNamePanel);
        MoveChild(_sectNamePanel, GetChildCount() - 1);
    }

    private void OpenSectNamePanel()
    {
        CloseBlockingOverlayPopups(_sectNamePanel);
        _sectNamePanel?.Open(_gameLoop.State.Clone());
    }

    private void OnSectNameApplyRequested(System.Collections.Generic.IReadOnlyDictionary<string, string> nameMap)
    {
        _gameLoop.UpdateSectNames(nameMap);
    }
}
