using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class EconomySystem
{
    public void TickHour(GameState state)
    {
        IndustryRules.EnsureDefaults(state);
        PopulationRules.EnsureDefaults(state);

        var foodMultiplier = Math.Max(state.FoodProductionMultiplier, 1.0);
        var industryMultiplier = Math.Max(state.IndustryProductionMultiplier, 1.0);
        var tradeMultiplier = Math.Max(state.TradeProductionMultiplier, 1.0);
        var managementBoost = IndustryRules.GetManagementBoost(state);
        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var onDutyFactor = PopulationRules.GetOnDutyFactor(state);
        var sleepFactor = PopulationRules.GetSleepFactor(state);
        var healthLaborFactor = PopulationRules.GetHealthLaborFactor(state);
        var laborAvailabilityFactor = onDutyFactor * sleepFactor * healthLaborFactor;

        var productionAssigned = Math.Min(state.Farmers, IndustryRules.GetProductionCapacity(state));
        var researchAssigned = Math.Min(state.Scholars, IndustryRules.GetResearchCapacity(state));
        var commerceAssigned = Math.Min(state.Merchants, IndustryRules.GetCommerceCapacity(state));

        var effectiveProductionWorkers = (int)Math.Floor(productionAssigned * laborAvailabilityFactor);
        var effectiveResearchers = (int)Math.Floor(researchAssigned * laborAvailabilityFactor);
        var effectiveMerchants = (int)Math.Floor(commerceAssigned * laborAvailabilityFactor);

        var productionFactor = managementBoost * toolCoverage;

        state.Food += effectiveProductionWorkers * 2.4 * foodMultiplier * productionFactor;
        state.Wood += effectiveProductionWorkers * 0.62 * industryMultiplier * productionFactor;
        state.Stone += effectiveProductionWorkers * 0.48 * industryMultiplier * productionFactor;
        state.Gold += effectiveMerchants * 0.86 * tradeMultiplier * productionFactor;
        state.Research += effectiveResearchers * 0.46 * productionFactor;

        var citizenWageCost = state.Population * 0.05;
        var managerWageCost = state.Workers * 0.11;
        state.Gold -= citizenWageCost + managerWageCost;

        if (state.Gold < 0)
        {
            state.Happiness -= 1.2;
            state.Gold = 0;
        }
    }
}
