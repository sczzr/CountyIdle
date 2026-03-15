using CountyIdle.Models;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string DemoPanelPath = $"{RightPanelPath}/PanelContent/MainVBox/DemoBox";

    private DemoPanel? _demoPanel;

    private void BindDemoPanelNodes()
    {
        _demoPanel = GetNodeOrNull<DemoPanel>(DemoPanelPath);
    }

    private void RefreshDemoPanel(GameState state)
    {
        _demoPanel?.Refresh(state);
    }
}
