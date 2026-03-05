using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class ResearchSystem
{
    private const int Tier1ResearchThreshold = 30;
    private const int Tier2ResearchThreshold = 90;
    private const int Tier3ResearchThreshold = 180;

    private const double BaseMultiplier = 1.0;
    private const double Tier1FoodMultiplier = 1.15;
    private const double Tier2IndustryMultiplier = 1.15;
    private const double Tier2TradeMultiplier = 1.10;
    private const double Tier3PopulationGrowthMultiplier = 1.20;

    public bool TickHour(GameState state, out string? log)
    {
        log = null;
        EnsureValidMultipliers(state);

        var targetTier = ResolveTargetTier(state.Research);
        if (targetTier <= state.TechLevel)
        {
            return false;
        }

        var breakthroughs = new List<string>();
        for (var tier = state.TechLevel + 1; tier <= targetTier; tier++)
        {
            ApplyTierBonuses(state, tier);
            breakthroughs.Add(GetTierLog(tier));
        }

        log = string.Join(" | ", breakthroughs);
        return breakthroughs.Count > 0;
    }

    private static int ResolveTargetTier(double research)
    {
        if (research >= Tier3ResearchThreshold)
        {
            return 3;
        }

        if (research >= Tier2ResearchThreshold)
        {
            return 2;
        }

        if (research >= Tier1ResearchThreshold)
        {
            return 1;
        }

        return 0;
    }

    private static void ApplyTierBonuses(GameState state, int tier)
    {
        state.TechLevel = tier;

        state.FoodProductionMultiplier = tier >= 1 ? Tier1FoodMultiplier : BaseMultiplier;
        state.IndustryProductionMultiplier = tier >= 2 ? Tier2IndustryMultiplier : BaseMultiplier;
        state.TradeProductionMultiplier = tier >= 2 ? Tier2TradeMultiplier : BaseMultiplier;
        state.PopulationGrowthMultiplier = tier >= 3 ? Tier3PopulationGrowthMultiplier : BaseMultiplier;
    }

    private static string GetTierLog(int tier)
    {
        return tier switch
        {
            1 => "郡学突破 T1：农桑改良生效，粮食产量提升 15%。",
            2 => "郡学突破 T2：工坊账册优化，木石产量 +15%，贸易产金 +10%。",
            3 => "郡学突破 T3：户籍与医坊协同，人口增长效率提升 20%。",
            _ => "郡学完成未知突破。"
        };
    }

    private static void EnsureValidMultipliers(GameState state)
    {
        state.FoodProductionMultiplier = Math.Max(state.FoodProductionMultiplier, BaseMultiplier);
        state.IndustryProductionMultiplier = Math.Max(state.IndustryProductionMultiplier, BaseMultiplier);
        state.TradeProductionMultiplier = Math.Max(state.TradeProductionMultiplier, BaseMultiplier);
        state.PopulationGrowthMultiplier = Math.Max(state.PopulationGrowthMultiplier, BaseMultiplier);
    }
}
