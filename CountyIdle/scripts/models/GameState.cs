using System;

namespace CountyIdle.Models;

public class GameState
{
    public int Population { get; set; } = 120;
    public int HousingCapacity { get; set; } = 180;
    public int ElitePopulation { get; set; } = 8;
    public int ChildPopulation { get; set; } = 18;
    public int AdultPopulation { get; set; } = 92;
    public int ElderPopulation { get; set; } = 10;
    public int SickPopulation { get; set; } = 4;
    public double ClothingStock { get; set; } = 140;
    public double AverageCommuteDistanceKm { get; set; } = 1.2;
    public double RoadMobilityMultiplier { get; set; } = 1.0;
    public double MapCommuteReductionBonusKm { get; set; } = 0.0;
    public double MapRoadMobilityBonus { get; set; } = 0.0;

    public int Farmers { get; set; } = 70;
    public int Workers { get; set; } = 25;
    public int Merchants { get; set; } = 12;
    public int Scholars { get; set; } = 8;

    public double Happiness { get; set; } = 72.0;
    public double Threat { get; set; } = 10.0;

    public double Food { get; set; } = 680;
    public double Wood { get; set; } = 220;
    public double Stone { get; set; } = 140;
    public double Gold { get; set; } = 90;
    public double Research { get; set; } = 0;
    public double RareMaterial { get; set; } = 0;
    public double IronOre { get; set; } = 65;
    public double CopperOre { get; set; } = 42;
    public double Coal { get; set; } = 58;
    public double MetalIngot { get; set; } = 12;
    public double CompositeMaterial { get; set; } = 0;
    public double IndustrialParts { get; set; } = 0;
    public double ConstructionMaterials { get; set; } = 6;

    public int TechLevel { get; set; } = 0;
    public double FoodProductionMultiplier { get; set; } = 1.0;
    public double IndustryProductionMultiplier { get; set; } = 1.0;
    public double TradeProductionMultiplier { get; set; } = 1.0;
    public double PopulationGrowthMultiplier { get; set; } = 1.0;

    public int ExplorationDepth { get; set; } = 1;
    public bool ExplorationEnabled { get; set; } = true;
    public int ExplorationProgressHours { get; set; } = 0;
    public double AvgGearScore { get; set; } = 12;
    public int CommonGearCount { get; set; } = 0;
    public int RareGearCount { get; set; } = 0;
    public int EpicGearCount { get; set; } = 0;
    public int LegendaryGearCount { get; set; } = 0;
    public int EventCooldownHours { get; set; } = 0;

    public int AgricultureBuildings { get; set; } = 3;
    public int WorkshopBuildings { get; set; } = 2;
    public int ResearchBuildings { get; set; } = 1;
    public int TradeBuildings { get; set; } = 1;
    public int AdministrationBuildings { get; set; } = 4;
    public double IndustryTools { get; set; } = 120;
    public int MiningLevel { get; set; } = 1;
    public int WarehouseLevel { get; set; } = 1;
    public double WarehouseCapacity { get; set; } = 1200;

    public int GameMinutes { get; set; } = 0;
    public int HourSettlements { get; set; } = 0;

    public int GetAssignedPopulation()
    {
        return Farmers + Workers + Merchants + Scholars;
    }

    public int GetUnassignedPopulation()
    {
        return Math.Max(Population - GetAssignedPopulation(), 0);
    }

    public double GetWarehouseUsed()
    {
        return Math.Max(Food, 0) +
               Math.Max(Wood, 0) +
               Math.Max(Stone, 0) +
               Math.Max(IndustryTools, 0) +
               Math.Max(RareMaterial, 0) +
               Math.Max(IronOre, 0) +
               Math.Max(CopperOre, 0) +
               Math.Max(Coal, 0) +
               Math.Max(MetalIngot, 0) +
               Math.Max(CompositeMaterial, 0) +
               Math.Max(IndustrialParts, 0) +
               Math.Max(ConstructionMaterials, 0);
    }

    public GameState Clone()
    {
        return (GameState)MemberwiseClone();
    }
}
