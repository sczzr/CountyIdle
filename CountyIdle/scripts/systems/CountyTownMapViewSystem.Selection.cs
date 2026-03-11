using CountyIdle.Models;
using System.Linq;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem
{
    private void NotifySelectionSummaryChanged()
    {
        SelectionSummaryChanged?.Invoke(BuildSelectionSummary());
    }

    private TownMapSelectionSummary BuildSelectionSummary()
    {
        if (_selectedActivityAnchor != null)
        {
            return BuildAnchorSelectionSummary(_selectedActivityAnchor);
        }

        if (_selectedCell == null || _mapData == null)
        {
            return TownMapSelectionSummary.CreateDefault();
        }

        var compound = _mapData.GetCellCompound(_selectedCell.Value);
        if (compound == null)
        {
            return TownMapSelectionSummary.CreateDefault();
        }

        return BuildCellSelectionSummary(compound);
    }

    private TownMapSelectionSummary BuildAnchorSelectionSummary(TownActivityAnchorData anchor)
    {
        var anchorTypeText = SectMapSemanticRules.GetAnchorTypeText(anchor.AnchorType);
        var assignedResidents = GetAssignedResidentCount(anchor);
        var presentResidents = GetPresentResidentCount(anchor);
        var inboundResidents = GetInboundResidentCount(anchor);
        var statusText = GetSelectedAnchorStatusText(anchor);
        var selectedWalker = GetSelectedResidentWalker();

        return new TownMapSelectionSummary(
            true,
            anchor.AnchorType,
            TownCellContentKind.Service,
            null,
            TownActivityAnchorVisualRules.GetBadgeText(anchor.AnchorType, true),
            anchor.Label,
            $"{anchorTypeText} · 归属：{SectMapSemanticRules.GetSettlementName()}",
            "当前态势",
            statusText,
            "驻守门人",
            $"{presentResidents}/{assignedResidents} 驻守",
            "前往中",
            $"{inboundResidents} 名前往中",
            "地气坐标",
            $"Hex [{anchor.LotCell.X}, {anchor.LotCell.Y}] · 临路 [{anchor.RoadCell.X}, {anchor.RoadCell.Y}]",
            BuildSelectionDescription(anchor, statusText, selectedWalker));
    }

    private TownMapSelectionSummary BuildCellSelectionSummary(TownCellCompoundData compound)
    {
        var buildingSummary = compound.SubBuildings.Length == 0
            ? "待规划"
            : string.Join(" / ", compound.SubBuildings.Select(static building => building.DisplayName));
        var statusText = GetCompoundStatusText(compound);
        var qiText = $"{compound.BaseQiCapacity} 池 · 需求 {compound.TotalQiDemand:0.#} · 拥堵 {compound.QiCongestion:0.00}";
        var slotText = $"{compound.SubBuildings.Length}/{compound.BuildSlotCount} 坊位 · 协同 {compound.SynergyScore:+0.00;-0.00;0.00}";
        var terrainText = _mapData?.GetTerrain(compound.Cell.X, compound.Cell.Y) ?? TownTerrainType.Ground;
        var featureSummary = compound.FeatureTexts.Length == 0
            ? "暂无特征"
            : string.Join("、", compound.FeatureTexts);

        return new TownMapSelectionSummary(
            true,
            null,
            compound.ContentKind,
            compound.SuggestedBuildType,
            GetContentKindBadgeText(compound.ContentKind),
            $"{compound.RegionName}·{GetContentKindTitle(compound.ContentKind)}",
            $"{compound.QiAffinityText} · 建议坊局：{buildingSummary}",
            "当前态势",
            statusText,
            "坊位格局",
            slotText,
            "地脉灵气",
            qiText,
            "地气坐标",
            $"Hex [{compound.Cell.X}, {compound.Cell.Y}] · {GetTerrainText(terrainText)}",
            BuildCompoundDescription(compound, featureSummary, buildingSummary, statusText));
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

    private static string GetCompoundStatusText(TownCellCompoundData compound)
    {
        if (compound.BuildSlotCount <= 1 || compound.SubBuildings.Length <= 1)
        {
            return "坊位受限";
        }

        if (compound.QiCongestion >= 0.35f)
        {
            return "灵池过载";
        }

        if (compound.QiCongestion >= 0.20f)
        {
            return "灵池分流";
        }

        if (compound.SynergyScore <= -0.05f || compound.Stability < 0.72f)
        {
            return "坊局互扰";
        }

        if (compound.SynergyScore >= 0.20f && compound.Stability >= 1.08f)
        {
            return "稳态成局";
        }

        if (compound.SynergyScore >= 0.20f && compound.Stability >= 1.0f)
        {
            return "坊局协同";
        }

        if (compound.BaseQiCapacity >= 140)
        {
            return "灵脉丰沛";
        }

        if (compound.ContentKind == TownCellContentKind.Empty)
        {
            return "待立坊局";
        }

        return compound.QiRecoveryPerHour >= 8 ? "回灵顺畅" : "可稳步经营";
    }

    private static string BuildCompoundDescription(
        TownCellCompoundData compound,
        string featureSummary,
        string buildingSummary,
        string statusText)
    {
        var efficiencyHint = compound.QiCongestion switch
        {
            >= 0.35f => "当前灵池已接近过载，继续扩位前更适合先腾挪高耗坊位或补强回灵。",
            >= 0.20f => "当前坊局已有明显灵气分流，继续塞入新坊位前建议先补灵气或精简组合。",
            _ when compound.SynergyScore > 0.15f => "当前坊局协同已初步成形，适合围绕现有组合继续强化。",
            _ => "当前坊局仍以打底为主，适合继续补齐支撑位或稳定位。"
        };
        var stabilityHint = compound.Stability switch
        {
            < 0.72f => "院域气机偏躁，较容易被随机事件或季节波动放大短板。",
            > 1.08f => "院域稳态较强，适合承担连续生产或长线研修。",
            _ => "院域稳定度处在可经营区间，适合继续观察最佳组合。"
        };
        return $"{compound.RegionName}以{compound.QiAffinityText}为主，天然特征为：{featureSummary}。当前院域态势：{statusText}，坊局为【{buildingSummary}】，稳定度 {compound.Stability:0.00}。{efficiencyHint}{stabilityHint}";
    }

    private static string GetContentKindText(TownCellContentKind contentKind)
    {
        return contentKind switch
        {
            TownCellContentKind.Infrastructure => "基础设施",
            TownCellContentKind.Production => "生产坊局",
            TownCellContentKind.Service => "服务坊局",
            TownCellContentKind.Residence => "居住坊局",
            TownCellContentKind.Special => "特殊院域",
            _ => "待规划院域"
        };
    }

    private static string GetContentKindTitle(TownCellContentKind contentKind)
    {
        return contentKind switch
        {
            TownCellContentKind.Infrastructure => "坊路院域",
            TownCellContentKind.Production => "产务院域",
            TownCellContentKind.Service => "治务院域",
            TownCellContentKind.Residence => "居舍院域",
            TownCellContentKind.Special => "巡山院域",
            _ => "预留院域"
        };
    }

    private static string GetContentKindBadgeText(TownCellContentKind contentKind)
    {
        return contentKind switch
        {
            TownCellContentKind.Infrastructure => "院域 / 坊路",
            TownCellContentKind.Production => "院域 / 生产",
            TownCellContentKind.Service => "院域 / 治务",
            TownCellContentKind.Residence => "院域 / 居舍",
            TownCellContentKind.Special => "院域 / 巡山",
            _ => "院域 / 预留"
        };
    }

    private static string GetTerrainText(TownTerrainType terrainType)
    {
        return terrainType switch
        {
            TownTerrainType.Road => "坊路地势",
            TownTerrainType.Courtyard => "院坪地势",
            TownTerrainType.Water => "临水地势",
            _ => "平地地势"
        };
    }
}
