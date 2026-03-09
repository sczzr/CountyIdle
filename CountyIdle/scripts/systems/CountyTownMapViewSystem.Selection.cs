using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem
{
    private void NotifySelectionSummaryChanged()
    {
        SelectionSummaryChanged?.Invoke(BuildSelectionSummary());
    }

    private TownMapSelectionSummary BuildSelectionSummary()
    {
        if (_selectedActivityAnchor == null)
        {
            return TownMapSelectionSummary.CreateDefault();
        }

        var anchor = _selectedActivityAnchor;
        var anchorTypeText = SectMapSemanticRules.GetAnchorTypeText(anchor.AnchorType);
        var assignedResidents = GetAssignedResidentCount(anchor);
        var presentResidents = GetPresentResidentCount(anchor);
        var inboundResidents = GetInboundResidentCount(anchor);
        var statusText = GetSelectedAnchorStatusText(anchor);
        var selectedWalker = GetSelectedResidentWalker();

        return new TownMapSelectionSummary(
            true,
            anchor.AnchorType,
            anchor.Label,
            $"{anchorTypeText} · 归属：{SectMapSemanticRules.GetSettlementName()}",
            statusText,
            $"{presentResidents}/{assignedResidents} 驻守",
            $"{inboundResidents} 名前往中",
            $"Hex [{anchor.LotCell.X}, {anchor.LotCell.Y}] · 临路 [{anchor.RoadCell.X}, {anchor.RoadCell.Y}]",
            BuildSelectionDescription(anchor, statusText, selectedWalker));
    }

    private string BuildSelectionDescription(TownActivityAnchorData anchor, string statusText, object? selectedWalker)
    {
        var anchorDescription = anchor.AnchorType switch
        {
            TownActivityAnchorType.Farmstead => "此处承担阵材培植与基础供养，是宗门稳定产出的前排地块。",
            TownActivityAnchorType.Workshop => "此处承担傀儡工坊与阵务营造，会直接反哺工器与建设链路。",
            TownActivityAnchorType.Market => "此处承担总坊流转与内外调度，是仓储与流通的重要接口。",
            TownActivityAnchorType.Academy => "此处承担传法院研修与推演，是科技与突破的前线节点。",
            TownActivityAnchorType.Administration => "此处承担庶务殿核账与宗门内务，是治理指令的总控节点。",
            TownActivityAnchorType.Leisure => "此处承担晚间论道与静悟休憩，会反馈门人的生活节奏与氛围。",
            _ => "此处为天衍峰当前可交互场所。"
        };

        if (selectedWalker == null)
        {
            return $"{anchorDescription} 当前状态：{statusText}。未定位到可视代表门人。";
        }

        dynamic walker = selectedWalker;
        return $"{anchorDescription} 当前状态：{statusText}。代表门人：{walker.Profile.Name} · {walker.Profile.DutyDisplayName} · {walker.Profile.RealmName}。";
    }
}
