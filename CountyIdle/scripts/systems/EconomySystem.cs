using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed record EconomyHourPreview(
    double FoodDeltaRaw,
    double GoldDeltaRaw,
    double ContributionDeltaRaw,
    double ResearchDeltaRaw);

public class EconomySystem
{
    public static EconomyHourPreview BuildHourPreview(GameState state)
    {
        InventoryRules.EndTransaction(state);
        IndustryRules.EnsureDefaults(state);
        PopulationRules.EnsureDefaults(state);
        MaterialRules.EnsureDefaults(state);
        SectTaskRules.EnsureDefaults(state);
        SectGovernanceRules.EnsureDefaults(state);
        SectRuleTreeRules.EnsureDefaults(state);
        SectPeakSupportRules.EnsureDefaults(state);

        var foodMultiplier = Math.Max(state.FoodProductionMultiplier, 1.0);
        var tradeMultiplier = Math.Max(state.TradeProductionMultiplier, 1.0);
        var managementBoost = IndustryRules.GetManagementBoost(state);
        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var productionFactor = managementBoost * toolCoverage;
        var taskSnapshot = SectTaskRules.BuildResolutionSnapshot(state);
        var governanceFoodModifier = SectGovernanceRules.GetFoodYieldModifier(state);
        var governanceGoldModifier = SectGovernanceRules.GetGoldYieldModifier(state);
        var governanceContributionModifier = SectGovernanceRules.GetContributionYieldModifier(state);
        var governanceResearchModifier = SectGovernanceRules.GetResearchYieldModifier(state);
        var quarterFoodModifier = SectGovernanceRules.GetQuarterFoodYieldModifier(state);
        var quarterGoldModifier = SectGovernanceRules.GetQuarterGoldYieldModifier(state);
        var quarterContributionModifier = SectGovernanceRules.GetQuarterContributionYieldModifier(state);
        var quarterResearchModifier = SectGovernanceRules.GetQuarterResearchYieldModifier(state);
        var ruleTreeFoodModifier = SectRuleTreeRules.GetFoodYieldModifier(state);
        var ruleTreeContributionModifier = SectRuleTreeRules.GetContributionYieldModifier(state);
        var ruleTreeResearchModifier = SectRuleTreeRules.GetResearchYieldModifier(state);
        var supportFoodModifier = SectPeakSupportRules.GetFoodYieldModifier(state);
        var supportGoldModifier = SectPeakSupportRules.GetGoldYieldModifier(state);
        var supportContributionModifier = SectPeakSupportRules.GetContributionYieldModifier(state);
        var supportResearchModifier = SectPeakSupportRules.GetResearchYieldModifier(state);

        var foodDeltaRaw = 0.0;
        var goldDeltaRaw = 0.0;
        var contributionDeltaRaw = 0.0;
        var researchDeltaRaw = 0.0;

        foreach (var definition in SectTaskRules.GetOrderedDefinitions())
        {
            var assigned = taskSnapshot.ResolvedWorkersByTask[definition.TaskType];
            var effectiveWorkers = Math.Max(assigned, 0);
            if (effectiveWorkers <= 0)
            {
                continue;
            }

            if (definition.FoodYieldPerWorker > 0)
            {
                foodDeltaRaw += effectiveWorkers * definition.FoodYieldPerWorker * foodMultiplier * productionFactor * governanceFoodModifier * quarterFoodModifier * ruleTreeFoodModifier * supportFoodModifier;
            }

            if (definition.GoldYieldPerWorker > 0)
            {
                goldDeltaRaw += effectiveWorkers * definition.GoldYieldPerWorker * tradeMultiplier * productionFactor * governanceGoldModifier * quarterGoldModifier * supportGoldModifier;
            }

            if (definition.ContributionYieldPerWorker > 0)
            {
                contributionDeltaRaw += effectiveWorkers * definition.ContributionYieldPerWorker * productionFactor * governanceContributionModifier * quarterContributionModifier * ruleTreeContributionModifier * supportContributionModifier;
            }

            if (definition.ResearchYieldPerWorker > 0)
            {
                researchDeltaRaw += effectiveWorkers * definition.ResearchYieldPerWorker * productionFactor * governanceResearchModifier * quarterResearchModifier * ruleTreeResearchModifier * supportResearchModifier;
            }
        }

        var citizenWageCost = state.Population * 0.05;
        var managerWageCost = taskSnapshot.ResolvedByJob[JobType.Worker] * 0.11;
        goldDeltaRaw -= citizenWageCost + managerWageCost;

        return new EconomyHourPreview(foodDeltaRaw, goldDeltaRaw, contributionDeltaRaw, researchDeltaRaw);
    }

    public void TickHour(GameState state)
    {
        var preview = BuildHourPreview(state);

        InventoryRules.ApplyDelta(state, nameof(GameState.Food), preview.FoodDeltaRaw);
        InventoryRules.ApplyDelta(state, nameof(GameState.Gold), preview.GoldDeltaRaw);
        InventoryRules.ApplyDelta(state, nameof(GameState.ContributionPoints), preview.ContributionDeltaRaw);
        state.Research += preview.ResearchDeltaRaw;

        if (state.Gold < 0)
        {
            state.Happiness -= 1.2;
            InventoryRules.SetVisibleAmount(state, nameof(GameState.Gold), 0);
        }

        if (state.ContributionPoints < 0)
        {
            InventoryRules.SetVisibleAmount(state, nameof(GameState.ContributionPoints), 0);
        }
    }
}
