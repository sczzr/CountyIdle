using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem
{
    public bool TryApplySelectedCompoundPlan(TownCompoundPlanStyle planStyle, out string logText)
    {
        logText = string.Empty;

        if (_mapData == null || _selectedCell == null || _selectedActivityAnchor != null)
        {
            return false;
        }

        var currentCompound = _mapData.GetCellCompound(_selectedCell.Value);
        if (currentCompound == null)
        {
            return false;
        }

        var updatedCompound = _generator.ReplanCompound(currentCompound, planStyle);
        _mapData.SetCellCompound(updatedCompound);

        var buildingSummary = updatedCompound.SubBuildings.Length == 0
            ? "待规划"
            : string.Join(" / ", updatedCompound.SubBuildings.Select(static building => building.DisplayName));
        logText =
            $"已将【{updatedCompound.RegionName}·{GetContentKindTitle(updatedCompound.ContentKind)}】切换为{GetPlanStyleText(planStyle)}，当前坊局为：{buildingSummary}。";

        UpdateMapHint();
        QueueRedraw();
        return true;
    }
}
