using System;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle;

public partial class Main
{
	private Label? _tileInspectorTitleLabel;
	private Label? _tileInspectorSubtitleLabel;
	private Label? _tileInspectorBadgeLabel;
	private Label? _tileInspectorStatusLabel;
	private Label? _tileInspectorStatusValueLabel;
	private Label? _tileInspectorLocationLabel;
	private Label? _tileInspectorLocationValueLabel;
	private Label? _tileInspectorBuildingLabel;
	private Label? _tileInspectorBuildingValueLabel;
	private Label? _tileInspectorBuildingSlotLabel;
	private Label? _tileInspectorDescriptionLabel;
	private Label? _tileInspectorActionHintLabel;
	private Button? _tileInspectorConstructionButton;
	private Node? _tileInspectorVisualFx;

	private void BindSectTileInspectorNodes()
	{
		const string InspectorRootPath = $"{LeftPanelPath}/PanelContent/ContentScroll/JobsVBox/IndustryEfficiency/InspectorVBox";
		_tileInspectorVisualFx = GetNodeOrNull<Node>("TileInspectorVisualFx");
		_tileInspectorTitleLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/InspectorHeader/TileTitle");
		_tileInspectorSubtitleLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/InspectorHeader/TileSubtitle");
		_tileInspectorBadgeLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/InspectorHeader/TileBadgeBox/TileBadgeLabel");
		_tileInspectorStatusLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/AttrGrid/StatusBox/AttrVBox/AttrLabel");
		_tileInspectorStatusValueLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/AttrGrid/StatusBox/AttrVBox/AttrValue");
		_tileInspectorLocationLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/AttrGrid/LocationBox/AttrVBox/AttrLabel");
		_tileInspectorLocationValueLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/AttrGrid/LocationBox/AttrVBox/AttrValue");
		_tileInspectorBuildingLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/BuildingListBox/BuildingListVBox/BuildingListLabel");
		_tileInspectorBuildingValueLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/BuildingListBox/BuildingListVBox/BuildingListValue");
		_tileInspectorBuildingSlotLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/BuildingListBox/BuildingListVBox/BuildingSlotLabel");
		_tileInspectorConstructionButton = GetNodeOrNull<Button>($"{InspectorRootPath}/BuildingListBox/BuildingListVBox/OpenConstructionButton");
		_tileInspectorDescriptionLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/InspectorDescription");
		_tileInspectorActionHintLabel = GetNodeOrNull<Label>($"{InspectorRootPath}/ActionHintPanel/ActionHintLabel");

		ApplySectTileInspectorSummary(TownMapSelectionSummary.CreateDefault());
	}

	private void ClearSectTileInspectorNodes()
	{
		_tileInspectorVisualFx = null;
		_tileInspectorTitleLabel = null;
		_tileInspectorSubtitleLabel = null;
		_tileInspectorBadgeLabel = null;
		_tileInspectorStatusLabel = null;
		_tileInspectorStatusValueLabel = null;
		_tileInspectorLocationLabel = null;
		_tileInspectorLocationValueLabel = null;
		_tileInspectorBuildingLabel = null;
		_tileInspectorBuildingValueLabel = null;
		_tileInspectorBuildingSlotLabel = null;
		_tileInspectorConstructionButton = null;
		_tileInspectorDescriptionLabel = null;
		_tileInspectorActionHintLabel = null;
	}

	private void BindSectTileInspectorEvents()
	{
		if (_sectMapRenderer != null)
		{
			_sectMapRenderer.SelectionSummaryChanged += OnSectMapSelectionSummaryChanged;
		}

		if (_worldMapRenderer != null)
		{
			_worldMapRenderer.WorldSiteSelectionChanged += OnWorldSiteSelectionChanged;
		}

		if (_tileInspectorConstructionButton != null)
		{
			_tileInspectorConstructionButton.Pressed += OpenConstructionPanel;
		}
	}

	private void UnbindSectTileInspectorEvents()
	{
		if (_sectMapRenderer != null)
		{
			_sectMapRenderer.SelectionSummaryChanged -= OnSectMapSelectionSummaryChanged;
		}

		if (_worldMapRenderer != null)
		{
			_worldMapRenderer.WorldSiteSelectionChanged -= OnWorldSiteSelectionChanged;
		}

		if (_tileInspectorConstructionButton != null)
		{
			_tileInspectorConstructionButton.Pressed -= OpenConstructionPanel;
		}
	}

	private void OnSectMapSelectionSummaryChanged(TownMapSelectionSummary summary)
	{
		ApplySectTileInspectorSummary(summary);
		HandleConstructionSelectionSummaryChanged(summary);
	}

	private void OnWorldSiteSelectionChanged(XianxiaSiteData? site)
	{
		if (site != null)
		{
			_lastSelectedWorldSite = site;
		}

		if (_currentMapTab != MapTab.World)
		{
			return;
		}

		ApplyWorldSiteInspectorSummary(site);
	}

	private void ApplySectTileInspectorSummary(TownMapSelectionSummary summary)
	{
		UpdateJobPanelVisibilityForSectSelection(summary);

		if (_tileInspectorTitleLabel == null ||
			_tileInspectorSubtitleLabel == null ||
			_tileInspectorBadgeLabel == null ||
			_tileInspectorStatusLabel == null ||
			_tileInspectorStatusValueLabel == null ||
			_tileInspectorLocationLabel == null ||
			_tileInspectorLocationValueLabel == null ||
			_tileInspectorBuildingLabel == null ||
			_tileInspectorBuildingValueLabel == null ||
			_tileInspectorBuildingSlotLabel == null ||
			_tileInspectorDescriptionLabel == null ||
			_tileInspectorActionHintLabel == null)
		{
			return;
		}

		var nameMap = _gameLoop != null ? _gameLoop.State.SectNameMap : null;
		string Resolve(string text) => SectNamingRules.ReplaceKnownNames(nameMap, text);

		_tileInspectorTitleLabel.Text = Resolve(summary.Title);
		_tileInspectorSubtitleLabel.Text = Resolve(summary.Subtitle);
		_tileInspectorStatusLabel.Text = summary.StatusLabel;
		_tileInspectorStatusValueLabel.Text = Resolve(summary.StatusText);
		_tileInspectorLocationLabel.Text = summary.LocationLabel;
		_tileInspectorLocationValueLabel.Text = Resolve(summary.LocationText);
		_tileInspectorBuildingLabel.Text = summary.BuildingLabel;
		_tileInspectorBuildingValueLabel.Text = Resolve(BuildBuildingListText(summary));
		_tileInspectorBuildingSlotLabel.Text = BuildBuildingSlotText(summary);
		_tileInspectorDescriptionLabel.Text = Resolve(summary.DescriptionText);

		UpdateConstructionEntryHint(summary);
		ConfigureBuildOnlyInspector(summary);
		ApplyTileInspectorVisualTone(summary);
	}

	private void ApplyWorldSiteInspectorSummary(XianxiaSiteData? site)
	{
		UpdateJobPanelVisibilityForWorldSiteSelection(site);

		if (_tileInspectorTitleLabel == null ||
			_tileInspectorSubtitleLabel == null ||
			_tileInspectorBadgeLabel == null ||
			_tileInspectorStatusLabel == null ||
			_tileInspectorStatusValueLabel == null ||
			_tileInspectorLocationLabel == null ||
			_tileInspectorLocationValueLabel == null ||
			_tileInspectorBuildingLabel == null ||
			_tileInspectorBuildingValueLabel == null ||
			_tileInspectorBuildingSlotLabel == null ||
			_tileInspectorDescriptionLabel == null ||
			_tileInspectorActionHintLabel == null)
		{
			return;
		}

		UpdateConstructionEntryHint(TownMapSelectionSummary.CreateDefault());

		if (site == null)
		{
			_tileInspectorTitleLabel.Text = "世界地图";
			_tileInspectorSubtitleLabel.Text = "尚未选中外域点位";
			_tileInspectorBadgeLabel.Text = "世界层";
			_tileInspectorStatusLabel.Text = "点位态势";
			_tileInspectorStatusValueLabel.Text = "等待点选";
			_tileInspectorLocationLabel.Text = "所属区块";
			_tileInspectorLocationValueLabel.Text = "待识别";
			_tileInspectorBuildingLabel.Text = "建筑概况";
			_tileInspectorBuildingValueLabel.Text = "世界层无院域建筑";
			_tileInspectorBuildingSlotLabel.Text = "坊位：世界层不可直接营建";
			_tileInspectorDescriptionLabel.Text = "左键点选世界地图中的宗门、凡俗据点、坊市、世家、仙城或遗迹节点后，这里会显示对应点位的情报摘要。";
			ApplyWorldInspectorVisualTone("world", false);
			ConfigureBuildOnlyInspector(TownMapSelectionSummary.CreateDefault());
			return;
		}

		var primaryTypeText = ResolveWorldPrimaryTypeText(site.PrimaryType);
		var rarityText = ResolveWorldRarityText(site.RarityTier);
		_tileInspectorTitleLabel.Text = site.Label;
		_tileInspectorSubtitleLabel.Text = $"{primaryTypeText} · {ResolveWorldSecondaryTagText(site.SecondaryTag)}";
		_tileInspectorBadgeLabel.Text = $"{primaryTypeText}点";
		_tileInspectorStatusLabel.Text = "稀有度";
		_tileInspectorStatusValueLabel.Text = rarityText;
		_tileInspectorLocationLabel.Text = "所属区块";
		_tileInspectorLocationValueLabel.Text = ResolveWorldRegionText(site.RegionId);
		_tileInspectorBuildingLabel.Text = "建筑概况";
		_tileInspectorBuildingValueLabel.Text = "世界层无院域建筑";
		_tileInspectorBuildingSlotLabel.Text = "坊位：世界层不可直接营建";
		_tileInspectorDescriptionLabel.Text = BuildWorldSiteDescription(site, primaryTypeText, rarityText);
		ConfigureBuildOnlyInspector(TownMapSelectionSummary.CreateDefault());
		ApplyWorldInspectorVisualTone(site.PrimaryType, true);
	}

	private void ConfigureBuildOnlyInspector(TownMapSelectionSummary summary)
	{
		if (_tileInspectorActionHintLabel == null)
		{
			return;
		}

		// 当前机宜卷已经收口为建造专用摘要，因此只维护建造提示，不再暴露旧的调度/仓储/弟子联动动作。
		if (_currentMapTab is MapTab.World or MapTab.WorldSite)
		{
			ApplyTileInspectorActionHint("当前仅展示地块与点位情报；若要直接建造建筑，请先返回山门图并点选具体院域。");
			return;
		}

		if (!summary.HasSelection)
		{
			ApplyTileInspectorActionHint("机宜卷已常驻展开。请先点选一块山门院域，再通过“对该地块营建”为该地块挑选建筑。");
			return;
		}

		if (!summary.HasBuildCapacity)
		{
			ApplyTileInspectorActionHint($"当前院域坊位已满（{summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount}），请改选其他地块继续营建。");
			return;
		}

		if (summary.AnchorType != null)
		{
			ApplyTileInspectorActionHint($"当前点中的是此地已落成建筑；当前坊位 {summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount}，仍可继续对这块地追加营建。");
			return;
		}

		ApplyTileInspectorActionHint("当前已锁定具体院域，可直接通过“对该地块营建”为这块地挑选建筑并发起营建排队。");
	}

	private static string BuildBuildingListText(TownMapSelectionSummary summary)
	{
		if (summary.BuildingList.Length == 0)
		{
			return summary.BuildingText;
		}

		var lines = new string[summary.BuildingList.Length];
		for (var i = 0; i < summary.BuildingList.Length; i++)
		{
			var entry = summary.BuildingList[i];
			lines[i] = string.IsNullOrWhiteSpace(entry) ? "-" : $"- {entry}";
		}

		return string.Join("\n", lines);
	}

	// 将剩余坊位折成一行，保证左侧营建卷不开营建面板也能看到当前地块还能再塞几座建筑。
	private static string BuildBuildingSlotText(TownMapSelectionSummary summary)
	{
		if (!summary.HasSelection)
		{
			return "坊位：待选中";
		}

		if (summary.BuildSlotCount <= 0)
		{
			return "坊位：当前地块未定义可建坊位";
		}

		var remaining = Math.Max(summary.BuildSlotCount - summary.OccupiedBuildSlotCount, 0);
		return remaining > 0
			? $"坊位：{summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount} 已占用，尚余 {remaining} 位"
			: $"坊位：{summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount} 已满";
	}

	private void ApplyTileInspectorActionHint(string hintText)
	{
		if (_tileInspectorActionHintLabel == null)
		{
			return;
		}

		_tileInspectorActionHintLabel.Text = hintText;
		_tileInspectorActionHintLabel.TooltipText = hintText;
	}

	private void UpdateConstructionEntryHint(TownMapSelectionSummary summary)
	{
		if (_tileInspectorConstructionButton == null)
		{
			return;
		}

		_tileInspectorConstructionButton.Text = "对该地块营建";

		if (_currentMapTab is MapTab.World or MapTab.WorldSite)
		{
			_tileInspectorConstructionButton.Disabled = true;
			_tileInspectorConstructionButton.TooltipText = "当前处于世界层视图，仅山门地块支持直接发起地块营建。";
			return;
		}

		if (!summary.HasSelection)
		{
			_tileInspectorConstructionButton.Disabled = false;
			_tileInspectorConstructionButton.TooltipText = "尚未选中院域地块，请先点选具体地块后再对该地块发起营建。";
			return;
		}

		if (!summary.HasBuildCapacity)
		{
			_tileInspectorConstructionButton.Disabled = true;
			_tileInspectorConstructionButton.TooltipText = $"当前院域坊位已满（{summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount}），请改选其他地块继续营建。";
			return;
		}

		if (summary.AnchorType != null)
		{
			_tileInspectorConstructionButton.Disabled = false;
			_tileInspectorConstructionButton.TooltipText = $"当前点中的是此地已落成建筑；当前坊位 {summary.OccupiedBuildSlotCount}/{summary.BuildSlotCount}，仍可继续对该地块追加营建。";
			return;
		}

		_tileInspectorConstructionButton.Disabled = false;
		_tileInspectorConstructionButton.TooltipText = "已锁定当前院域地块，可直接打开营建卷并对该地块发起营建排队。";
	}

	private void ApplyWorldInspectorVisualTone(string primaryType, bool hasSelection)
	{
		CallTileInspectorVisualFx("apply_world_inspector_tone", primaryType, hasSelection);
	}

	private void ApplyTileInspectorVisualTone(TownMapSelectionSummary summary)
	{
		var badgeText = string.IsNullOrWhiteSpace(summary.BadgeText)
			? TownActivityAnchorVisualRules.GetBadgeText(
				_gameLoop != null ? _gameLoop.State.SectNameMap : null,
				summary.AnchorType,
				summary.HasSelection)
			: summary.BadgeText;
		var accentColor = summary.AnchorType != null
			? TownActivityAnchorVisualRules.GetAccentColor(summary.AnchorType, summary.HasSelection)
			: TownActivityAnchorVisualRules.GetAccentColor(summary.ContentKind, summary.HasSelection);
		var statusColor = summary.AnchorType != null
			? TownActivityAnchorVisualRules.GetInspectorStatusColor(summary.AnchorType, summary.HasSelection)
			: TownActivityAnchorVisualRules.GetInspectorStatusColor(summary.ContentKind, summary.HasSelection);
		CallTileInspectorVisualFx("apply_local_inspector_tone", badgeText, accentColor, statusColor);
	}

	private void CallTileInspectorVisualFx(string methodName, params Variant[] args)
	{
		_tileInspectorVisualFx?.Call(methodName, args);
	}

	private static string ResolveWorldPrimaryTypeText(string primaryType)
	{
		return primaryType switch
		{
			"Sect" => "宗门",
			"Wilderness" => "野外",
			"MortalRealm" => "凡俗国度",
			"CultivatorClan" => "修仙世家",
			"ImmortalCity" => "仙城",
			"Market" => "坊市",
			"Ruin" => "遗迹",
			_ => "外域点位"
		};
	}

	private static string ResolveWorldSecondaryTagText(string secondaryTag)
	{
		return secondaryTag switch
		{
			"MountainGate" => "山门本宗",
			"BranchPeak" => "分峰别院",
			"OuterCourtyard" => "外门院",
			"SectMarket" => "宗门坊",
			"LooseCultivatorBazaar" => "散修集",
			"RoadsideMarket" => "路市",
			"CountySeat" => "府县治所",
			"FarmVillage" => "农庄乡里",
			"RiverTown" => "水镇",
			"AncestralEstate" => "祖庭本家",
			"GuestHall" => "客卿别馆",
			"SpiritFieldManor" => "灵田庄园",
			"ForgeLineage" => "铸器世家",
			"MedicineLineage" => "丹药世家",
			"GrandCity" => "大城",
			"TransitHub" => "驿城",
			"HarborCity" => "河港仙城",
			"FrontierCity" => "边陲仙城",
			"ImperialCultCity" => "王朝修士都城",
			"AncientCave" => "古修洞府",
			"BattlefieldRemnant" => "古战场遗址",
			"SealedDungeon" => "封印地宫",
			"TrialRealm" => "试炼秘境",
			"SpiritMountainWilds" => "灵脉山野",
			"ForestWilds" => "古木荒野",
			"SwampWilds" => "灵沼野地",
			"CrystalWilds" => "晶砂荒原",
			"SkyWilds" => "浮天野境",
			"DesertWilds" => "流沙野境",
			"RiverWilds" => "河谷野径",
			"OpenWilds" => "外野地界",
			_ => string.IsNullOrWhiteSpace(secondaryTag) ? "未定子类" : secondaryTag
		};
	}

	private static string ResolveWorldRegionText(string regionId)
	{
		return regionId switch
		{
			"SpiritMountain" => "灵脉山域",
			"MortalHeartland" => "凡俗腹地",
			"TradeCorridor" => "商路走廊",
			"FrontierWilds" => "边疆险地",
			"BrokenVeinRuins" => "古迹断脉区",
			_ => "未定区块"
		};
	}

	private static string ResolveWorldRarityText(string rarityTier)
	{
		return rarityTier switch
		{
			"Legendary" => "传说",
			"Rare" => "稀有",
			"Uncommon" => "少见",
			_ => "常见"
		};
	}

	private static string BuildWorldSiteDescription(XianxiaSiteData site, string primaryTypeText, string rarityText)
	{
		var focus = site.PrimaryType switch
		{
			"Sect" => "此地更偏向宗门交涉、传承往来与势力关系。",
			"MortalRealm" => "此地更偏向供养、人口、安民与附庸护持。",
			"Market" => "此地更偏向交易、传闻、物资流转与短期机会。",
			"Wilderness" => "此地更偏向探路、采集、遭遇事件与野外历练推进。",
			"CultivatorClan" => "此地更偏向血脉、人脉、客卿合作与家族委托。",
			"ImmortalCity" => "此地更偏向大宗交易、驻点经营与跨域枢纽往来。",
			"Ruin" => "此地更偏向探索、试炼、机缘与高风险回报。",
			_ => "此地承接外域层的分层玩法。"
		};

		return $"【{site.Label}】属于{primaryTypeText}层，子类为“{ResolveWorldSecondaryTagText(site.SecondaryTag)}”，当前稀有度为{rarityText}，所在区块为{ResolveWorldRegionText(site.RegionId)}。{focus}";
	}
}
