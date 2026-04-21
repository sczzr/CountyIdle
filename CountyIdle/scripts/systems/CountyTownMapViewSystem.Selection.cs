using System;
using CountyIdle.Models;
using System.Linq;
using Godot;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem
{
	// 通知选中信息变化
	private void NotifySelectionSummaryChanged()
	{
		SelectionSummaryChanged?.Invoke(BuildSelectionSummary());
	}

	// 将名称替换为当前宗门命名
	private string ResolveNamedText(string text)
	{
		return SectNamingRules.ReplaceKnownNames(_nameMap, text);
	}

	// 构建选中摘要（地块或锚点）
	private TownMapSelectionSummary BuildSelectionSummary()
	{
		if (_selectedActivityAnchor != null)
		{
			return BuildAnchorSelectionSummary(_selectedActivityAnchor);
		}

		if (_selectedCell == null || _mapData == null)
		{
			return TownMapSelectionSummary.CreateDefault(
				SectNamingRules.GetName(_nameMap, SectNamingRules.SectNameKey),
				SectNamingRules.GetName(_nameMap, SectNamingRules.PeakTianyanKey));
		}

		var compound = _mapData.GetCellCompound(_selectedCell.Value);
		if (compound == null)
		{
			return TownMapSelectionSummary.CreateDefault(
				SectNamingRules.GetName(_nameMap, SectNamingRules.SectNameKey),
				SectNamingRules.GetName(_nameMap, SectNamingRules.PeakTianyanKey));
		}

		return BuildCellSelectionSummary(compound);
	}

	// 构建锚点选中摘要
	private TownMapSelectionSummary BuildAnchorSelectionSummary(TownActivityAnchorData anchor)
	{
		var anchorTypeText = SectMapSemanticRules.GetAnchorTypeText(anchor.AnchorType, _nameMap);
		var assignedResidents = GetAssignedResidentCount(anchor);
		var presentResidents = GetPresentResidentCount(anchor);
		var inboundResidents = GetInboundResidentCount(anchor);
		var statusText = GetSelectedAnchorStatusText(anchor);
		var selectedWalker = GetSelectedResidentWalker();
		var compound = _mapData?.GetCellCompound(anchor.LotCell);
		var (buildingListText, buildingList) = BuildPlacedBuildingSummary(anchor.LotCell);
		var buildSlotCount = compound?.BuildSlotCount ?? Math.Max(buildingList.Length, 1);
		var occupiedBuildSlotCount = GetLogicalStructureCountAtCell(anchor.LotCell);
		var anchorLabel = ResolveNamedText(anchor.Label);

		return new TownMapSelectionSummary(
			true,
			anchor.AnchorType,
			compound?.ContentKind ?? TownCellContentKind.Service,
			compound?.SuggestedBuildType,
			buildSlotCount,
			occupiedBuildSlotCount,
			TownActivityAnchorVisualRules.GetBadgeText(_nameMap, anchor.AnchorType, true),
			anchorLabel,
			$"{anchorTypeText} · 归属：{SectMapSemanticRules.GetSettlementName(_nameMap)}",
			"建筑列表",
			ResolveNamedText(buildingListText),
			buildingList,
			"当前态势",
			statusText,
			"驻守门人",
			$"{presentResidents}/{assignedResidents} 驻守",
			"前往中",
			$"{inboundResidents} 名前往中",
			"地气坐标",
			$"Hex [{anchor.LotCell.X}, {anchor.LotCell.Y}] · 临路 [{anchor.RoadCell.X}, {anchor.RoadCell.Y}]",
			ResolveNamedText(BuildSelectionDescription(anchor, statusText, selectedWalker)));
	}

	// 构建地块选中摘要
	private TownMapSelectionSummary BuildCellSelectionSummary(TownCellCompoundData compound)
	{
		var (buildingSummary, buildingList) = BuildPlacedBuildingSummary(compound.Cell);
		var occupiedBuildSlotCount = GetLogicalStructureCountAtCell(compound.Cell);
		var statusText = GetCompoundStatusText(compound);
		var qiText = $"{compound.BaseQiCapacity} 池 · 需求 {compound.TotalQiDemand:0.#} · 拥堵 {compound.QiCongestion:0.00}";
		var slotText = $"{occupiedBuildSlotCount}/{compound.BuildSlotCount} 已落成坊位 · 协同 {compound.SynergyScore:+0.00;-0.00;0.00}";
		var terrainText = _mapData?.GetTerrain(compound.Cell.X, compound.Cell.Y) ?? TownTerrainType.Ground;
		var featureSummary = compound.FeatureTexts.Length == 0
			? "暂无特征"
			: string.Join("、", compound.FeatureTexts);
		var regionName = ResolveNamedText(compound.RegionName);

		return new TownMapSelectionSummary(
			true,
			null,
			compound.ContentKind,
			compound.SuggestedBuildType,
			compound.BuildSlotCount,
			occupiedBuildSlotCount,
			GetContentKindBadgeText(compound.ContentKind),
			$"{regionName}·{GetContentKindTitle(compound.ContentKind)}",
			$"{compound.QiAffinityText} · {GetPlanStyleText(compound.PlanStyle)} · 坊局：{buildingSummary}",
			"建筑列表",
			buildingSummary,
			buildingList,
			"当前态势",
			statusText,
			"坊位格局",
			slotText,
			"地脉灵气",
			qiText,
			"地气坐标",
			$"Hex [{compound.Cell.X}, {compound.Cell.Y}] · {GetTerrainText(terrainText)}",
			ResolveNamedText(BuildCompoundDescription(compound, featureSummary, buildingSummary, statusText)));
	}

	// 汇总当前地块已落成建筑列表；若还未落成，则回退到待营建文案。
	private (string SummaryText, string[] BuildingList) BuildPlacedBuildingSummary(Vector2I cell)
	{
		if (_mapData == null)
		{
			return ("待营建", Array.Empty<string>());
		}

		var buildingList = _mapData.ActivityAnchors
			.Where(anchor => anchor.LotCell == cell)
			.Select(anchor => ResolveNamedText(anchor.Label))
			.Distinct()
			.ToArray();

		if (buildingList.Length > 0)
		{
			return (string.Join(" / ", buildingList), buildingList);
		}

		return ("待营建", Array.Empty<string>());
	}

	// 统计当前地块的实际已占用坊位数；带锚点建筑按 1 个槽位计，不重复把建筑壳体算两次。
	private int GetLogicalStructureCountAtCell(Vector2I cell)
	{
		if (_mapData == null)
		{
			return 0;
		}

		var anchorCount = _mapData.ActivityAnchors.Count(anchor => anchor.LotCell == cell);
		var buildingCount = _mapData.Buildings.Count(building => building.Cell == cell);
		return Math.Max(anchorCount, buildingCount);
	}

	// 生成锚点描述文本
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

	// 计算地块状态文本
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

	// 组合地块描述文本
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

	// 锚点建筑列表文案
	private static string BuildAnchorBuildingListText(TownActivityAnchorData anchor)
	{
		var floorText = anchor.Floors > 1 ? $"{anchor.Floors}层" : "1层";
		return $"{anchor.Label}（{floorText}）";
	}

	// 内容类型文本
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

	// 内容类型标题
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

	// 内容类型徽章文本
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

	// 坊局规划风格文本
	private static string GetPlanStyleText(TownCompoundPlanStyle planStyle)
	{
		return planStyle switch
		{
			TownCompoundPlanStyle.Specialized => "主修坊局",
			TownCompoundPlanStyle.Synergy => "协同坊局",
			TownCompoundPlanStyle.Balanced => "稳态坊局",
			_ => "天然坊局"
		};
	}

	// 地形文本
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
