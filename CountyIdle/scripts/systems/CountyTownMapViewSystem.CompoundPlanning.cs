using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem
{
    // 对选中地块应用坊局规划方案
    public bool TryApplySelectedCompoundPlan(TownCompoundPlanStyle planStyle, out string logText)
    {
        logText = string.Empty;

        // 必须选中地块且不能选中建筑锚点
        if (_mapData == null || _selectedCell == null || _selectedActivityAnchor != null)
        {
            return false;
        }

        // 获取当前地块坊局配置
        var currentCompound = _mapData.GetCellCompound(_selectedCell.Value);
        if (currentCompound == null)
        {
            return false;
        }

        // 重新规划并写回
        var updatedCompound = _generator.ReplanCompound(currentCompound, planStyle);
        _mapData.SetCellCompound(updatedCompound);

        // 输出规划结果文本
        var buildingSummary = updatedCompound.SubBuildings.Length == 0
            ? "待规划"
            : string.Join(" / ", updatedCompound.SubBuildings.Select(static building => building.DisplayName));
        logText =
            $"已将【{updatedCompound.RegionName}·{GetContentKindTitle(updatedCompound.ContentKind)}】切换为{GetPlanStyleText(planStyle)}，当前坊局为：{buildingSummary}。";

        // 刷新提示与重绘
        UpdateMapHint();
        QueueRedraw();
        return true;
    }
}
