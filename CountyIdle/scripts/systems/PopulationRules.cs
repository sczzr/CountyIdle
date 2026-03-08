using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class PopulationRules
{
    private const int MinimumPopulation = 20;
    private const double MinimumCommuteDistanceKm = 0.2;
    private const double MaximumCommuteDistanceKm = 12.0;
    private const double MinimumRoadMobility = 0.7;
    private const double MaximumRoadMobility = 1.3;
    private const double MinimumDynamicCommuteDistanceKm = 0.4;
    private const double MaximumDynamicCommuteDistanceKm = 6.5;

    public static void EnsureDefaults(GameState state)
    {
        MaterialRules.EnsureDefaults(state);
        state.Population = Math.Max(state.Population, MinimumPopulation);
        state.ChildPopulation = Math.Max(state.ChildPopulation, 0);
        state.AdultPopulation = Math.Max(state.AdultPopulation, 0);
        state.ElderPopulation = Math.Max(state.ElderPopulation, 0);

        var segmentedPopulation = state.ChildPopulation + state.AdultPopulation + state.ElderPopulation;
        if (segmentedPopulation <= 0)
        {
            state.ChildPopulation = (int)Math.Round(state.Population * 0.18);
            state.ElderPopulation = (int)Math.Round(state.Population * 0.12);
            state.AdultPopulation = Math.Max(state.Population - state.ChildPopulation - state.ElderPopulation, 0);
        }
        else if (segmentedPopulation != state.Population)
        {
            if (segmentedPopulation > state.Population)
            {
                var scale = (double)state.Population / segmentedPopulation;
                state.ChildPopulation = (int)Math.Round(state.ChildPopulation * scale);
                state.ElderPopulation = (int)Math.Round(state.ElderPopulation * scale);
                state.AdultPopulation = Math.Max(state.Population - state.ChildPopulation - state.ElderPopulation, 0);
            }
            else
            {
                state.AdultPopulation += state.Population - segmentedPopulation;
            }
        }

        state.SickPopulation = Math.Clamp(state.SickPopulation, 0, state.AdultPopulation);
        state.ClothingStock = Math.Max(state.ClothingStock, 0);
        state.AverageCommuteDistanceKm = Math.Clamp(state.AverageCommuteDistanceKm, MinimumCommuteDistanceKm, MaximumCommuteDistanceKm);
        state.RoadMobilityMultiplier = Math.Clamp(state.RoadMobilityMultiplier, MinimumRoadMobility, MaximumRoadMobility);
        state.MapCommuteReductionBonusKm = Math.Clamp(state.MapCommuteReductionBonusKm, 0.0, 0.80);
        state.MapRoadMobilityBonus = Math.Clamp(state.MapRoadMobilityBonus, 0.0, 0.18);
    }

    public static void RefreshDynamicCommute(GameState state)
    {
        var workplaceBuildings = Math.Max(
            state.AgricultureBuildings +
            state.WorkshopBuildings +
            state.ResearchBuildings +
            state.TradeBuildings +
            state.AdministrationBuildings,
            1);

        var housingLoad = Math.Clamp((double)state.Population / Math.Max(state.HousingCapacity, 1), 0.75, 1.35);
        var countyScale = Math.Clamp(Math.Sqrt(Math.Max(state.Population, MinimumPopulation)) / 12.0, 0.85, 2.20);
        var residentialSpread = Math.Clamp(state.HousingCapacity / (workplaceBuildings * 24.0), 0.80, 2.20);
        var workplaceCoverage = Math.Clamp((state.GetAssignedPopulation() + 12.0) / (workplaceBuildings * 14.0), 0.80, 1.35);

        var commuteDistance = 0.68 * countyScale * housingLoad * residentialSpread * workplaceCoverage;
        commuteDistance -= state.MapCommuteReductionBonusKm;
        state.AverageCommuteDistanceKm = Math.Clamp(commuteDistance, MinimumDynamicCommuteDistanceKm, MaximumDynamicCommuteDistanceKm);

        var mobilityFromBuildings = 0.92 + (state.AdministrationBuildings * 0.02) + (state.WorkshopBuildings * 0.01) + state.MapRoadMobilityBonus;
        var mobilityPenaltyFromThreat = state.Threat * 0.0025;
        state.RoadMobilityMultiplier = Math.Clamp(mobilityFromBuildings - mobilityPenaltyFromThreat, MinimumRoadMobility, MaximumRoadMobility);
    }

    public static double GetSleepFactor(GameState state)
    {
        var sleepNeed = state.Population * 0.33;
        var sleepCapacity = state.HousingCapacity * 0.40;
        return Math.Clamp(sleepCapacity / Math.Max(sleepNeed, 1.0), 0.55, 1.05);
    }

    public static double GetHousingPressure(GameState state)
    {
        return Math.Clamp((state.Population - state.HousingCapacity) / Math.Max(state.Population, 1.0), 0, 0.45);
    }

    public static double GetClothingCoverage(GameState state)
    {
        return Math.Clamp(MaterialRules.GetClothingEquivalent(state) / Math.Max(state.Population, 1.0), 0.30, 1.0);
    }

    public static double GetSaltCoverage(GameState state)
    {
        var saltNeed = Math.Max(state.Population * 0.018, 1.0);
        return Math.Clamp(state.FineSalt / saltNeed, 0.0, 1.0);
    }

    public static double GetMedicineCoverage(GameState state)
    {
        var medicineNeed = Math.Max((state.SickPopulation * 0.14) + (state.Population * 0.004), 1.0);
        return Math.Clamp(state.HerbalMedicine / medicineNeed, 0.0, 1.0);
    }

    public static double GetCommuteMinutes(GameState state)
    {
        var commuteMinutes = (state.AverageCommuteDistanceKm / (4.2 * state.RoadMobilityMultiplier)) * 60;
        return Math.Clamp(commuteMinutes, 3, 45);
    }

    public static double GetOnDutyFactor(GameState state)
    {
        var commuteMinutes = GetCommuteMinutes(state);
        return Math.Clamp((60 - commuteMinutes) / 60, 0.25, 0.95);
    }

    public static double GetHealthLaborFactor(GameState state)
    {
        return Math.Clamp(1 - (state.SickPopulation / Math.Max(state.AdultPopulation, 1.0)), 0.45, 1.0);
    }

    public static int GetEffectiveAssignedPopulation(int assignedPopulation, GameState state)
    {
        var validAssigned = Math.Max(assignedPopulation, 0);
        var laborFactor = GetOnDutyFactor(state) * GetSleepFactor(state) * GetHealthLaborFactor(state);
        return (int)Math.Floor(validAssigned * laborFactor);
    }
}
