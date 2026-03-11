using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class MapOperationalLinkSystem
{
    private const int CourierRoadWoodCost = 18;
    private const int CourierRoadStoneCost = 8;
    private const int ReliefFoodCost = 70;
    private const int ReliefGoldCost = 6;
    private const int StreetRepairWoodCost = 24;
    private const int StreetRepairGoldCost = 6;
    private const int StreetRepairContributionCost = 8;
    private const int NightWatchGoldCost = 10;
    private const int NightWatchContributionCost = 6;

    public MapOperationalSnapshot BuildSnapshot(GameState state, MapRegionScope activeScope)
    {
        InventoryRules.EndTransaction(state);
        var worldStyle = BuildWorldStyle(state);
        var prefectureStyle = BuildPrefectureStyle(state);
        var countyTownStyle = BuildCountyTownStyle(state);
        var snapshot = new MapOperationalSnapshot
        {
            WorldStyle = worldStyle,
            PrefectureStyle = prefectureStyle,
            CountyTownStyle = countyTownStyle,
            ShowDirectiveRow = activeScope is MapRegionScope.World or MapRegionScope.Prefecture or MapRegionScope.CountyTown
        };

        switch (activeScope)
        {
            case MapRegionScope.World:
                snapshot.ActiveStatusText = $"{SectMapSemanticRules.GetWorldMapTitle()}：{worldStyle.HintText}";
                snapshot.ActiveStatusColor = worldStyle.AccentColor;
                snapshot.PrimaryChoice = CreateDisabledChoice("世界总览", "切换到天衍峰山门图后可执行峰内调度。");
                snapshot.SecondaryChoice = CreateDisabledChoice("暂无调度", "世界图当前只展示全局态势。");
                break;
            case MapRegionScope.Prefecture:
                snapshot.ActiveStatusText = $"{SectMapSemanticRules.GetLegacyPrefectureMapTitle()}：{prefectureStyle.HintText}";
                snapshot.ActiveStatusColor = prefectureStyle.AccentColor;
                snapshot.PrimaryChoice = BuildCourierRoadChoice(state);
                snapshot.SecondaryChoice = BuildReliefChoice(state);
                break;
            case MapRegionScope.CountyTown:
                snapshot.ActiveStatusText = $"天衍峰：{countyTownStyle.HintText}";
                snapshot.ActiveStatusColor = countyTownStyle.AccentColor;
                snapshot.PrimaryChoice = BuildStreetRepairChoice(state);
                snapshot.SecondaryChoice = BuildNightWatchChoice(state);
                break;
            default:
                snapshot.ActiveStatusText = string.Empty;
                snapshot.ActiveStatusColor = new Color(0.93f, 0.90f, 0.80f, 1f);
                snapshot.PrimaryChoice = CreateDisabledChoice(string.Empty, string.Empty);
                snapshot.SecondaryChoice = CreateDisabledChoice(string.Empty, string.Empty);
                snapshot.ShowDirectiveRow = false;
                break;
        }

        return snapshot;
    }

    public bool TryExecuteDirective(GameState state, MapDirectiveAction action, out string log)
    {
        InventoryRules.EndTransaction(state);
        switch (action)
        {
            case MapDirectiveAction.RepairCourierRoad:
                if (state.Wood < CourierRoadWoodCost || state.Stone < CourierRoadStoneCost)
                {
                    log = $"整修{SectMapSemanticRules.GetOuterRegionRoadName()}失败：木材或石料不足。";
                    return false;
                }

                var courierWoodDelta = InventoryRules.ApplyDelta(state, nameof(GameState.Wood), -CourierRoadWoodCost);
                var courierStoneDelta = InventoryRules.ApplyDelta(state, nameof(GameState.Stone), -CourierRoadStoneCost);
                state.Happiness = Math.Min(state.Happiness + 1.0, 100.0);
                state.Threat = Math.Max(state.Threat - 1.5, 0.0);
                log =
                    $"整修{SectMapSemanticRules.GetOuterRegionRoadName()}：木 {courierWoodDelta:+#;-#;0}、石 {courierStoneDelta:+#;-#;0}，外域秩序稍稳，威胁降至 {state.Threat:0.#}%。";
                return true;

            case MapDirectiveAction.ReliefVillages:
                if (state.Food < ReliefFoodCost || state.Gold < ReliefGoldCost)
                {
                    log = $"{SectMapSemanticRules.GetOuterRegionReliefActionName()}失败：粮食或金钱不足。";
                    return false;
                }

                var reliefFoodDelta = InventoryRules.ApplyDelta(state, nameof(GameState.Food), -ReliefFoodCost);
                var reliefGoldDelta = InventoryRules.ApplyDelta(state, nameof(GameState.Gold), -ReliefGoldCost);
                state.Happiness = Math.Min(state.Happiness + 4.0, 100.0);
                state.Threat = Math.Max(state.Threat - 3.0, 0.0);
                log =
                    $"{SectMapSemanticRules.GetOuterRegionReliefActionName()}：粮 {reliefFoodDelta:+#;-#;0}、金 {reliefGoldDelta:+#;-#;0}，民心回升至 {state.Happiness:0.#}，威胁降至 {state.Threat:0.#}%。";
                return true;

            case MapDirectiveAction.RepairStreets:
                if (state.Wood < StreetRepairWoodCost ||
                    state.Gold < StreetRepairGoldCost ||
                    state.ContributionPoints < StreetRepairContributionCost)
                {
                    log = "修整街坊失败：木材、灵石或贡献点不足。";
                    return false;
                }

                var streetWoodDelta = InventoryRules.ApplyDelta(state, nameof(GameState.Wood), -StreetRepairWoodCost);
                var streetGoldDelta = InventoryRules.ApplyDelta(state, nameof(GameState.Gold), -StreetRepairGoldCost);
                var streetContributionDelta = InventoryRules.ApplyDelta(state, nameof(GameState.ContributionPoints), -StreetRepairContributionCost);
                state.Happiness = Math.Min(state.Happiness + 2.0, 100.0);
                state.Threat = Math.Max(state.Threat - 1.0, 0.0);
                log =
                    $"修整坊路：木 {streetWoodDelta:+#;-#;0}、灵石 {streetGoldDelta:+#;-#;0}、贡献 {streetContributionDelta:+#;-#;0}，宗门民心提升至 {state.Happiness:0.#}，威胁降至 {state.Threat:0.#}%。";
                return true;

            case MapDirectiveAction.NightWatch:
                if (state.Gold < NightWatchGoldCost ||
                    state.ContributionPoints < NightWatchContributionCost)
                {
                    log = "夜巡清巷失败：灵石或贡献点不足。";
                    return false;
                }

                var nightWatchGoldDelta = InventoryRules.ApplyDelta(state, nameof(GameState.Gold), -NightWatchGoldCost);
                var nightWatchContributionDelta = InventoryRules.ApplyDelta(state, nameof(GameState.ContributionPoints), -NightWatchContributionCost);
                state.Threat = Math.Max(state.Threat - 4.0, 0.0);
                state.Happiness = Math.Min(state.Happiness + 1.0, 100.0);
                log =
                    $"夜巡清巷：灵石 {nightWatchGoldDelta:+#;-#;0}、贡献 {nightWatchContributionDelta:+#;-#;0}，宗门威胁降至 {state.Threat:0.#}% ，民心回升至 {state.Happiness:0.#}。";
                return true;

            default:
                log = "当前地图没有可执行的调度。";
                return false;
        }
    }

    private MapViewStyle BuildWorldStyle(GameState state)
    {
        var happinessScore = NormalizePercent(state.Happiness);
        var securityScore = InvertPercent(state.Threat);
        var foodScore = NormalizeReserve(state.Food, Math.Max(state.Population * 4.5, 240.0));
        var goldScore = NormalizeReserve(state.Gold, 140.0);
        var score = (happinessScore * 0.32) + (securityScore * 0.28) + (foodScore * 0.22) + (goldScore * 0.18);
        var level = ResolveLevel(score);

        return CreateStyle(
            level,
            score,
            level switch
            {
                MapConditionLevel.Flourishing => $"世界商路畅通，仓廪充盈。粮 {state.Food:0} · 金 {state.Gold:0} · 威胁 {state.Threat:0.#}%。",
                MapConditionLevel.Stable => $"世界大势平稳，可继续扩产蓄势。粮 {state.Food:0} · 金 {state.Gold:0} · 威胁 {state.Threat:0.#}%。",
                MapConditionLevel.Strained => $"世界边路与供给开始承压，宜关注宗门供给与巡防。粮 {state.Food:0} · 金 {state.Gold:0} · 威胁 {state.Threat:0.#}%。",
                _ => $"世界警讯上升，需优先安抚与巡防。粮 {state.Food:0} · 金 {state.Gold:0} · 威胁 {state.Threat:0.#}%。"
            });
    }

    private MapViewStyle BuildPrefectureStyle(GameState state)
    {
        var happinessScore = NormalizePercent(state.Happiness);
        var securityScore = InvertPercent(state.Threat);
        var foodScore = NormalizeReserve(state.Food, Math.Max(state.Population * 3.8, 180.0));
        var reserveScore = NormalizeReserve(state.Gold, Math.Max(state.Population * 0.9, 90.0));
        var score = (happinessScore * 0.30) + (securityScore * 0.32) + (foodScore * 0.20) + (reserveScore * 0.18);
        var level = ResolveLevel(score);

        return CreateStyle(
            level,
            score,
            level switch
            {
                MapConditionLevel.Flourishing => $"{SectMapSemanticRules.GetOuterRegionRoadName()}与{SectMapSemanticRules.GetOuterRegionSettlementName()}都较安定，可继续稳步采办与巡护。",
                MapConditionLevel.Stable => $"外域{SectMapSemanticRules.GetOuterRegionRoadName()}平稳，可通过整修{SectMapSemanticRules.GetOuterRegionRoadName()}或{SectMapSemanticRules.GetOuterRegionReliefActionName()}继续压低风险。",
                MapConditionLevel.Strained => $"外域{SectMapSemanticRules.GetOuterRegionRoadName()}或{SectMapSemanticRules.GetOuterRegionSettlementName()}承压，建议优先整修与抚恤。",
                _ => $"外域{SectMapSemanticRules.GetOuterRegionRoadName()}吃紧且威胁攀升，须尽快安抚{SectMapSemanticRules.GetOuterRegionSettlementName()}。威胁 {state.Threat:0.#}%。"
            });
    }

    private MapViewStyle BuildCountyTownStyle(GameState state)
    {
        var happinessScore = NormalizePercent(state.Happiness);
        var securityScore = InvertPercent(state.Threat);
        var housingScore = NormalizeHousing(state);
        var reserveScore = NormalizeReserve(state.ContributionPoints, 120.0);
        var score = (happinessScore * 0.30) + (securityScore * 0.30) + (housingScore * 0.22) + (reserveScore * 0.18);
        var level = ResolveLevel(score);

        return CreateStyle(
            level,
            score,
            level switch
            {
                MapConditionLevel.Flourishing => $"坊区有序，住房 {state.HousingCapacity}/{state.Population}，威胁 {state.Threat:0.#}%。",
                MapConditionLevel.Stable => $"运转平稳，住房 {state.HousingCapacity}/{state.Population}，可修坊路或夜巡。",
                MapConditionLevel.Strained => $"坊区承压，住房 {state.HousingCapacity}/{state.Population}，宜修坊路并夜巡。",
                _ => $"山门吃紧，住房 {state.HousingCapacity}/{state.Population}，威胁 {state.Threat:0.#}%。"
            });
    }

    private MapDirectiveChoice BuildCourierRoadChoice(GameState state)
    {
        var enabled =
            state.Wood >= CourierRoadWoodCost &&
            state.Stone >= CourierRoadStoneCost;

        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.RepairCourierRoad,
            Label = $"整修{SectMapSemanticRules.GetOuterRegionRoadName()} (-18木 -8石)",
            HintText = enabled
                ? "稳住外域道路秩序并压低威胁。"
                : "木材或石料不足，暂时无法整修。",
            Enabled = enabled
        };
    }

    private MapDirectiveChoice BuildReliefChoice(GameState state)
    {
        var enabled = state.Food >= ReliefFoodCost && state.Gold >= ReliefGoldCost;
        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.ReliefVillages,
            Label = $"{SectMapSemanticRules.GetOuterRegionReliefActionName()} (-70粮 -6金)",
            HintText = enabled
                ? "快速拉回民心并压低威胁。"
                : $"粮食或金钱不足，暂时无法展开{SectMapSemanticRules.GetOuterRegionReliefActionName()}。",
            Enabled = enabled
        };
    }

    private MapDirectiveChoice BuildStreetRepairChoice(GameState state)
    {
        var enabled =
            state.Wood >= StreetRepairWoodCost &&
            state.Gold >= StreetRepairGoldCost &&
            state.ContributionPoints >= StreetRepairContributionCost;

        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.RepairStreets,
            Label = $"修整坊路 (-24木 -{StreetRepairGoldCost}灵石 -{StreetRepairContributionCost}贡献)",
            HintText = enabled
                ? "疏理天衍峰坊路并提升门人安稳度。"
                : "木材、灵石或贡献不足，暂时无法修整坊路。",
            Enabled = enabled
        };
    }

    private MapDirectiveChoice BuildNightWatchChoice(GameState state)
    {
        var enabled = state.Gold >= NightWatchGoldCost && state.ContributionPoints >= NightWatchContributionCost;
        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.NightWatch,
            Label = $"夜巡清巷 (-{NightWatchGoldCost}灵石 -{NightWatchContributionCost}贡献)",
            HintText = enabled
                ? "压低天衍峰山门戒备压力。"
                : "灵石或贡献不足，无法组织巡山夜值。",
            Enabled = enabled
        };
    }

    private static MapDirectiveChoice CreateDisabledChoice(string label, string hintText)
    {
        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.None,
            Label = label,
            HintText = hintText,
            Enabled = false
        };
    }

    private static double NormalizePercent(double value)
    {
        return Math.Clamp(value / 100.0, 0.0, 1.0);
    }

    private static double InvertPercent(double value)
    {
        return 1.0 - NormalizePercent(value);
    }

    private static double NormalizeReserve(double currentValue, double targetValue)
    {
        return Math.Clamp(currentValue / Math.Max(targetValue, 1.0), 0.0, 1.0);
    }

    private static double NormalizeHousing(GameState state)
    {
        var ratio = state.HousingCapacity / Math.Max(state.Population, 1.0);
        return Math.Clamp((ratio - 0.70) / 0.55, 0.0, 1.0);
    }

    private static MapConditionLevel ResolveLevel(double score)
    {
        if (score >= 0.75)
        {
            return MapConditionLevel.Flourishing;
        }

        if (score >= 0.55)
        {
            return MapConditionLevel.Stable;
        }

        if (score >= 0.35)
        {
            return MapConditionLevel.Strained;
        }

        return MapConditionLevel.Critical;
    }

    private static MapViewStyle CreateStyle(MapConditionLevel level, double score, string hintText)
    {
        var intensity = (float)Math.Clamp(score, 0.0, 1.0);
        return level switch
        {
            MapConditionLevel.Flourishing => new MapViewStyle
            {
                Condition = level,
                TitleSuffix = "繁荣",
                HintText = hintText,
                AccentColor = new Color(0.58f, 0.90f, 0.72f, 1f),
                BackdropColor = new Color(0.08f, 0.16f, 0.14f, 0.94f),
                GridColor = new Color(0.20f, 0.32f, 0.27f, 0.48f),
                OutlineColor = new Color(0.74f, 0.95f, 0.82f, 0.42f),
                RouteColor = new Color(0.98f, 0.84f, 0.50f, 0.88f),
                RiverColor = new Color(0.44f, 0.75f, 0.92f, 0.88f),
                NodeColor = new Color(0.96f, 0.93f, 0.80f, 1f),
                LabelColor = new Color(0.92f, 0.98f, 0.90f, 0.98f),
                TerrainTint = new Color(0.96f, 1.06f, 0.98f, 1f),
                BuildingTint = new Color(1.02f, 1.02f, 0.96f, 1f)
            },
            MapConditionLevel.Stable => new MapViewStyle
            {
                Condition = level,
                TitleSuffix = "平稳",
                HintText = hintText,
                AccentColor = new Color(0.90f, 0.89f, 0.76f, 1f),
                BackdropColor = new Color(0.09f, 0.11f, 0.16f, 0.92f),
                GridColor = new Color(0.16f, 0.20f, 0.28f, 0.55f),
                OutlineColor = new Color(0.82f, 0.86f, 0.96f, 0.35f),
                RouteColor = new Color(0.95f, 0.79f, 0.42f, 0.82f),
                RiverColor = new Color(0.38f, 0.62f, 0.88f, 0.82f),
                NodeColor = new Color(0.94f, 0.88f, 0.73f, 1f),
                LabelColor = new Color(0.93f, 0.90f, 0.80f, 0.96f),
                TerrainTint = new Color(1f, 1f, 1f, 1f),
                BuildingTint = new Color(1f, 1f, 1f, 1f)
            },
            MapConditionLevel.Strained => new MapViewStyle
            {
                Condition = level,
                TitleSuffix = "吃紧",
                HintText = hintText,
                AccentColor = new Color(0.95f, 0.76f, 0.46f, 1f),
                BackdropColor = new Color(0.15f, 0.11f, 0.09f, 0.94f),
                GridColor = new Color(0.30f, 0.23f, 0.18f, 0.48f),
                OutlineColor = new Color(0.96f, 0.84f, 0.64f, 0.40f),
                RouteColor = new Color(0.98f, 0.68f, 0.34f, 0.88f),
                RiverColor = new Color(0.44f, 0.68f, 0.84f, 0.76f),
                NodeColor = new Color(0.96f, 0.80f, 0.58f, 1f),
                LabelColor = new Color(0.97f, 0.88f, 0.72f, 0.96f),
                TerrainTint = new Color(1.06f, 0.95f, 0.88f, 1f),
                BuildingTint = new Color(1.08f, 0.94f, 0.88f, 1f)
            },
            _ => new MapViewStyle
            {
                Condition = level,
                TitleSuffix = "紧张",
                HintText = hintText,
                AccentColor = new Color(0.94f, 0.54f, 0.56f, 1f),
                BackdropColor = new Color(0.17f, 0.09f, 0.11f, 0.95f),
                GridColor = new Color(0.34f, 0.18f, 0.20f, 0.52f),
                OutlineColor = new Color(0.96f, 0.66f, 0.70f, 0.42f),
                RouteColor = new Color(0.92f, 0.48f, 0.42f, 0.90f),
                RiverColor = new Color(0.36f, 0.56f, 0.78f, 0.70f),
                NodeColor = new Color(0.95f, 0.72f, 0.70f, 1f),
                LabelColor = new Color(0.97f, 0.82f, 0.82f, 0.98f),
                TerrainTint = new Color(1.08f + (intensity * 0.02f), 0.88f, 0.88f, 1f),
                BuildingTint = new Color(1.10f, 0.86f, 0.86f, 1f)
            }
        };
    }
}
