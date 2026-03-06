using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class MapOperationalLinkSystem
{
    private const double CourierRoadWoodCost = 18.0;
    private const double CourierRoadStoneCost = 8.0;
    private const double ReliefFoodCost = 70.0;
    private const double ReliefGoldCost = 6.0;
    private const double StreetRepairWoodCost = 24.0;
    private const double NightWatchGoldCost = 10.0;
    private const double MaximumCommuteReductionBonusKm = 0.80;
    private const double MaximumRoadMobilityBonus = 0.18;

    public MapOperationalSnapshot BuildSnapshot(GameState state, MapRegionScope activeScope)
    {
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
                snapshot.ActiveStatusText = $"天下州域：{worldStyle.HintText}";
                snapshot.ActiveStatusColor = worldStyle.AccentColor;
                snapshot.PrimaryChoice = CreateDisabledChoice("州域总览", "切换到郡图或县城后可执行地图调度。");
                snapshot.SecondaryChoice = CreateDisabledChoice("暂无调度", "世界图当前只展示全局态势。");
                break;
            case MapRegionScope.Prefecture:
                snapshot.ActiveStatusText = $"周边郡图：{prefectureStyle.HintText}";
                snapshot.ActiveStatusColor = prefectureStyle.AccentColor;
                snapshot.PrimaryChoice = BuildCourierRoadChoice(state);
                snapshot.SecondaryChoice = BuildReliefChoice(state);
                break;
            case MapRegionScope.CountyTown:
                snapshot.ActiveStatusText = $"县城俯瞰：{countyTownStyle.HintText}";
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
        switch (action)
        {
            case MapDirectiveAction.RepairCourierRoad:
                if (state.Wood < CourierRoadWoodCost || state.Stone < CourierRoadStoneCost)
                {
                    log = "整修驿路失败：木材或石料不足。";
                    return false;
                }

                if (state.MapCommuteReductionBonusKm >= MaximumCommuteReductionBonusKm &&
                    state.MapRoadMobilityBonus >= MaximumRoadMobilityBonus)
                {
                    log = "整修驿路已达当前上限，可先把资源用于其他地图调度。";
                    return false;
                }

                state.Wood -= CourierRoadWoodCost;
                state.Stone -= CourierRoadStoneCost;
                state.MapCommuteReductionBonusKm = Math.Min(state.MapCommuteReductionBonusKm + 0.12, MaximumCommuteReductionBonusKm);
                state.MapRoadMobilityBonus = Math.Min(state.MapRoadMobilityBonus + 0.03, MaximumRoadMobilityBonus);
                state.Happiness = Math.Min(state.Happiness + 1.0, 100.0);
                PopulationRules.RefreshDynamicCommute(state);
                log =
                    $"整修驿路：木 -{CourierRoadWoodCost:0}、石 -{CourierRoadStoneCost:0}，通勤压缩到 {state.AverageCommuteDistanceKm:0.00}km，道路机动提升至 x{state.RoadMobilityMultiplier:0.00}。";
                return true;

            case MapDirectiveAction.ReliefVillages:
                if (state.Food < ReliefFoodCost || state.Gold < ReliefGoldCost)
                {
                    log = "赈济乡里失败：粮食或金钱不足。";
                    return false;
                }

                state.Food -= ReliefFoodCost;
                state.Gold -= ReliefGoldCost;
                state.Happiness = Math.Min(state.Happiness + 4.0, 100.0);
                state.Threat = Math.Max(state.Threat - 3.0, 0.0);
                if (state.SickPopulation > 0)
                {
                    state.SickPopulation = Math.Max(state.SickPopulation - 1, 0);
                }

                PopulationRules.RefreshDynamicCommute(state);
                log =
                    $"赈济乡里：粮 -{ReliefFoodCost:0}、金 -{ReliefGoldCost:0}，民心回升至 {state.Happiness:0.#}，威胁降至 {state.Threat:0.#}%。";
                return true;

            case MapDirectiveAction.RepairStreets:
                if (state.Wood < StreetRepairWoodCost)
                {
                    log = "修整街坊失败：木材不足。";
                    return false;
                }

                if (state.MapCommuteReductionBonusKm >= MaximumCommuteReductionBonusKm &&
                    state.MapRoadMobilityBonus >= MaximumRoadMobilityBonus)
                {
                    log = "街坊道路已较为顺畅，当前无需继续堆叠修整。";
                    return false;
                }

                state.Wood -= StreetRepairWoodCost;
                state.MapCommuteReductionBonusKm = Math.Min(state.MapCommuteReductionBonusKm + 0.08, MaximumCommuteReductionBonusKm);
                state.MapRoadMobilityBonus = Math.Min(state.MapRoadMobilityBonus + 0.02, MaximumRoadMobilityBonus);
                state.Happiness = Math.Min(state.Happiness + 2.0, 100.0);
                PopulationRules.RefreshDynamicCommute(state);
                log =
                    $"修整街坊：木 -{StreetRepairWoodCost:0}，县城通勤压缩到 {state.AverageCommuteDistanceKm:0.00}km，民心提升至 {state.Happiness:0.#}。";
                return true;

            case MapDirectiveAction.NightWatch:
                if (state.Gold < NightWatchGoldCost)
                {
                    log = "夜巡清巷失败：金钱不足。";
                    return false;
                }

                state.Gold -= NightWatchGoldCost;
                state.Threat = Math.Max(state.Threat - 4.0, 0.0);
                state.Happiness = Math.Min(state.Happiness + 1.0, 100.0);
                PopulationRules.RefreshDynamicCommute(state);
                log =
                    $"夜巡清巷：金 -{NightWatchGoldCost:0}，县城威胁降至 {state.Threat:0.#}% ，民心回升至 {state.Happiness:0.#}。";
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
                MapConditionLevel.Flourishing => $"州域商路畅通，仓廪充盈。粮 {state.Food:0} · 金 {state.Gold:0} · 威胁 {state.Threat:0.#}%。",
                MapConditionLevel.Stable => $"州域大势平稳，可继续扩产蓄势。粮 {state.Food:0} · 金 {state.Gold:0} · 威胁 {state.Threat:0.#}%。",
                MapConditionLevel.Strained => $"州域边路与供给开始承压，宜关注郡图与县城调度。粮 {state.Food:0} · 金 {state.Gold:0} · 威胁 {state.Threat:0.#}%。",
                _ => $"州域警讯上升，需优先安抚与巡防。粮 {state.Food:0} · 金 {state.Gold:0} · 威胁 {state.Threat:0.#}%。"
            });
    }

    private MapViewStyle BuildPrefectureStyle(GameState state)
    {
        var happinessScore = NormalizePercent(state.Happiness);
        var securityScore = InvertPercent(state.Threat);
        var logisticsScore = GetLogisticsScore(state);
        var foodScore = NormalizeReserve(state.Food, Math.Max(state.Population * 3.8, 180.0));
        var score = (happinessScore * 0.28) + (securityScore * 0.28) + (logisticsScore * 0.26) + (foodScore * 0.18);
        var level = ResolveLevel(score);
        var commuteMinutes = PopulationRules.GetCommuteMinutes(state);

        return CreateStyle(
            level,
            score,
            level switch
            {
                MapConditionLevel.Flourishing => $"驿路与乡里都较安定，通勤约 {commuteMinutes:0} 分钟，道路机动 x{state.RoadMobilityMultiplier:0.00}。",
                MapConditionLevel.Stable => $"郡路平稳，可通过整修驿路或赈济继续压低风险。通勤约 {commuteMinutes:0} 分钟。",
                MapConditionLevel.Strained => $"郡路拥挤或乡里承压，建议优先整修驿路与赈济乡里。通勤约 {commuteMinutes:0} 分钟。",
                _ => $"郡路吃紧且威胁攀升，须尽快安抚乡里。通勤约 {commuteMinutes:0} 分钟，威胁 {state.Threat:0.#}%。"
            });
    }

    private MapViewStyle BuildCountyTownStyle(GameState state)
    {
        var happinessScore = NormalizePercent(state.Happiness);
        var securityScore = InvertPercent(state.Threat);
        var logisticsScore = GetLogisticsScore(state);
        var housingScore = NormalizeHousing(state);
        var score = (happinessScore * 0.28) + (securityScore * 0.24) + (logisticsScore * 0.24) + (housingScore * 0.24);
        var level = ResolveLevel(score);

        return CreateStyle(
            level,
            score,
            level switch
            {
                MapConditionLevel.Flourishing => $"街坊有序，住房与道路都较宽裕。住房 {state.HousingCapacity}/{state.Population}，威胁 {state.Threat:0.#}%。",
                MapConditionLevel.Stable => $"县城运转平稳，可视需要修整街坊或安排夜巡。住房 {state.HousingCapacity}/{state.Population}。",
                MapConditionLevel.Strained => $"县城开始拥挤或治安承压，建议修整街坊并加强夜巡。住房 {state.HousingCapacity}/{state.Population}。",
                _ => $"县城拥挤且治安偏紧，需优先夜巡清巷。住房 {state.HousingCapacity}/{state.Population}，威胁 {state.Threat:0.#}%。"
            });
    }

    private MapDirectiveChoice BuildCourierRoadChoice(GameState state)
    {
        var enabled =
            state.Wood >= CourierRoadWoodCost &&
            state.Stone >= CourierRoadStoneCost &&
            (state.MapCommuteReductionBonusKm < MaximumCommuteReductionBonusKm || state.MapRoadMobilityBonus < MaximumRoadMobilityBonus);

        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.RepairCourierRoad,
            Label = "整修驿路 (-18木 -8石)",
            HintText = enabled
                ? "压缩通勤并提升道路机动。"
                : "木材/石料不足，或当前驿路整修已接近上限。",
            Enabled = enabled
        };
    }

    private MapDirectiveChoice BuildReliefChoice(GameState state)
    {
        var enabled = state.Food >= ReliefFoodCost && state.Gold >= ReliefGoldCost;
        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.ReliefVillages,
            Label = "赈济乡里 (-70粮 -6金)",
            HintText = enabled
                ? "快速拉回民心并压低威胁。"
                : "粮食或金钱不足，暂时无法展开赈济。",
            Enabled = enabled
        };
    }

    private MapDirectiveChoice BuildStreetRepairChoice(GameState state)
    {
        var enabled =
            state.Wood >= StreetRepairWoodCost &&
            (state.MapCommuteReductionBonusKm < MaximumCommuteReductionBonusKm || state.MapRoadMobilityBonus < MaximumRoadMobilityBonus);

        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.RepairStreets,
            Label = "修整街坊 (-24木)",
            HintText = enabled
                ? "改善县城道路并提升民心。"
                : "木材不足，或街坊道路已接近当前整修上限。",
            Enabled = enabled
        };
    }

    private MapDirectiveChoice BuildNightWatchChoice(GameState state)
    {
        var enabled = state.Gold >= NightWatchGoldCost;
        return new MapDirectiveChoice
        {
            Action = MapDirectiveAction.NightWatch,
            Label = "夜巡清巷 (-10金)",
            HintText = enabled
                ? "压低县城治安压力。"
                : "金钱不足，无法组织夜巡。",
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

    private static double GetLogisticsScore(GameState state)
    {
        var commuteScore = Math.Clamp(1.0 - (state.AverageCommuteDistanceKm / 6.5), 0.0, 1.0);
        var mobilityScore = Math.Clamp((state.RoadMobilityMultiplier - 0.7) / 0.6, 0.0, 1.0);
        return (commuteScore * 0.55) + (mobilityScore * 0.45);
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
