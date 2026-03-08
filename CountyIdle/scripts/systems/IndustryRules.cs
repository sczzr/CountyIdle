using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class IndustryRules
{
    public const int ProductionPerAgricultureBuilding = 16;
    public const int ProductionPerWorkshopBuilding = 12;
    public const int ResearchPerBuilding = 10;
    public const int CommercePerBuilding = 14;
    public const int ManagementPerBuilding = 8;
    private const int BaseWarehouseCapacity = 900;
    private const int WarehouseCapacityPerLevel = 260;
    private const int WarehouseCapacityPerAdministration = 45;

    public static void EnsureDefaults(GameState state)
    {
        state.AgricultureBuildings = Math.Max(state.AgricultureBuildings, 1);
        state.WorkshopBuildings = Math.Max(state.WorkshopBuildings, 1);
        state.ResearchBuildings = Math.Max(state.ResearchBuildings, 1);
        state.TradeBuildings = Math.Max(state.TradeBuildings, 1);
        state.AdministrationBuildings = Math.Max(state.AdministrationBuildings, 1);
        state.IndustryTools = Math.Max(state.IndustryTools, 0);
        state.MiningLevel = Math.Max(state.MiningLevel, 1);
        state.WarehouseLevel = Math.Max(state.WarehouseLevel, 1);
        state.WarehouseCapacity = Math.Max(state.WarehouseCapacity, CalculateWarehouseCapacity(state));
    }

    public static int GetCapacity(GameState state, JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => GetProductionCapacity(state),
            JobType.Worker => GetManagementCapacity(state),
            JobType.Merchant => GetCommerceCapacity(state),
            JobType.Scholar => GetResearchCapacity(state),
            _ => 0
        };
    }

    public static int GetAssigned(GameState state, JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => state.Farmers,
            JobType.Worker => state.Workers,
            JobType.Merchant => state.Merchants,
            JobType.Scholar => state.Scholars,
            _ => 0
        };
    }

    public static void SetAssigned(GameState state, JobType jobType, int value)
    {
        var assigned = Math.Max(value, 0);
        switch (jobType)
        {
            case JobType.Farmer:
                state.Farmers = assigned;
                break;
            case JobType.Worker:
                state.Workers = assigned;
                break;
            case JobType.Merchant:
                state.Merchants = assigned;
                break;
            case JobType.Scholar:
                state.Scholars = assigned;
                break;
        }
    }

    public static int GetProductionCapacity(GameState state)
    {
        return (state.AgricultureBuildings * ProductionPerAgricultureBuilding) +
               (state.WorkshopBuildings * ProductionPerWorkshopBuilding);
    }

    public static int GetResearchCapacity(GameState state)
    {
        return state.ResearchBuildings * ResearchPerBuilding;
    }

    public static int GetCommerceCapacity(GameState state)
    {
        return state.TradeBuildings * CommercePerBuilding;
    }

    public static int GetManagementCapacity(GameState state)
    {
        return state.AdministrationBuildings * ManagementPerBuilding;
    }

    public static double GetRequiredTools(GameState state)
    {
        return (state.Farmers * 0.34) + (state.Scholars * 0.62) + (state.Merchants * 0.42);
    }

    public static double GetToolCoverage(GameState state)
    {
        var requiredTools = GetRequiredTools(state);
        if (requiredTools <= 0.01)
        {
            return 1.0;
        }

        return Math.Clamp(state.IndustryTools / requiredTools, 0.25, 1.0);
    }

    public static double GetManagementBoost(GameState state)
    {
        var ratio = state.Population <= 0 ? 0 : (double)state.Workers / state.Population;
        return 1.0 + Math.Clamp(ratio, 0, 0.28);
    }

    public static double CalculateWarehouseCapacity(GameState state)
    {
        return BaseWarehouseCapacity +
               (state.WarehouseLevel * WarehouseCapacityPerLevel) +
               (state.AdministrationBuildings * WarehouseCapacityPerAdministration);
    }
}
