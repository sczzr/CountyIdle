using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class ResourceSystem
{
    private const double MinLaborFactor = 0.35;
    private const double MaxLaborFactor = 1.25;

    public bool TickHour(GameState state, out string? log)
    {
        log = null;
        InventoryRules.EndTransaction(state);
        IndustryRules.EnsureDefaults(state);
        PopulationRules.EnsureDefaults(state);
        MaterialRules.EnsureDefaults(state);

        var logs = new List<string>();

        ResolveTieredGathering(state, logs);
        ResolvePrimaryProcessing(state, logs);
        ResolveMining(state, logs);
        ResolveSmelting(state, logs);
        ResolveCivilMaterialProcessing(state, logs);
        ResolveMaterialResearch(state, logs);
        ResolveManufacturing(state, logs);
        ResolveWarehouse(state, logs);

        if (logs.Count == 0)
        {
            return false;
        }

        log = string.Join(" | ", logs);
        return true;
    }

    private static void ResolveTieredGathering(GameState state, List<string> logs)
    {
        var gatheringEstimate = MaterialRules.EstimateGathering(state);
        if (gatheringEstimate.EffectiveGatherers <= 0)
        {
            return;
        }

        var timberGain = InventoryRules.ApplyDelta(state, nameof(GameState.Timber), gatheringEstimate.TimberGain);
        var rawStoneGain = InventoryRules.ApplyDelta(state, nameof(GameState.RawStone), gatheringEstimate.RawStoneGain);
        var clayGain = InventoryRules.ApplyDelta(state, nameof(GameState.Clay), gatheringEstimate.ClayGain);
        var brineGain = InventoryRules.ApplyDelta(state, nameof(GameState.Brine), gatheringEstimate.BrineGain);
        var herbsGain = InventoryRules.ApplyDelta(state, nameof(GameState.Herbs), gatheringEstimate.HerbsGain);
        var hempGain = InventoryRules.ApplyDelta(state, nameof(GameState.HempFiber), gatheringEstimate.HempGain);
        var reedsGain = InventoryRules.ApplyDelta(state, nameof(GameState.Reeds), gatheringEstimate.ReedsGain);
        var hidesGain = InventoryRules.ApplyDelta(state, nameof(GameState.Hides), gatheringEstimate.HidesGain);

        if (timberGain > 0 || rawStoneGain > 0 || clayGain > 0 || brineGain > 0 || herbsGain > 0 || hempGain > 0 || reedsGain > 0 || hidesGain > 0)
        {
            logs.Add(
                $"{SectMapSemanticRules.GetWildernessGatheringLabel()}：林木+{timberGain}、原石+{rawStoneGain}、黏土+{clayGain}、卤水+{brineGain}、药材+{herbsGain}、麻料+{hempGain}、芦苇+{reedsGain}、皮毛+{hidesGain}。");
        }
    }

    private static void ResolvePrimaryProcessing(GameState state, List<string> logs)
    {
        var processingEstimate = MaterialRules.EstimatePrimaryProcessing(state);
        var woodBatch = processingEstimate.WoodGain;
        var stoneBatch = processingEstimate.StoneGain;

        var producedWood = 0.0;
        var producedStone = 0.0;

        if (woodBatch >= 0.25)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.Timber), -(woodBatch * MaterialRules.GetTimberToWoodInput()));
            producedWood = InventoryRules.ApplyDelta(state, nameof(GameState.Wood), woodBatch);
        }

        if (stoneBatch >= 0.25)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.RawStone), -(stoneBatch * MaterialRules.GetRawStoneToStoneInput()));
            producedStone = InventoryRules.ApplyDelta(state, nameof(GameState.Stone), stoneBatch);
        }

        if (producedWood > 0 || producedStone > 0)
        {
            logs.Add($"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}初加工：木料+{producedWood:0}、石料+{producedStone:0}。");
        }
    }

    private static void ResolveMining(GameState state, List<string> logs)
    {
        var laborFactor = ResolveMiningLaborFactor(state);
        var techFactor = 1.0 + (state.TechLevel * 0.08);
        var levelFactor = 1.0 + ((state.MiningLevel - 1) * 0.10);

        var ironGain = state.MiningLevel * 3.6 * laborFactor * techFactor * levelFactor;
        var copperGain = state.MiningLevel * 2.3 * laborFactor * techFactor * levelFactor;
        var coalGain = state.MiningLevel * 2.9 * laborFactor * techFactor * levelFactor;

        var actualIronGain = InventoryRules.ApplyDelta(state, nameof(GameState.IronOre), ironGain);
        var actualCopperGain = InventoryRules.ApplyDelta(state, nameof(GameState.CopperOre), copperGain);
        var actualCoalGain = InventoryRules.ApplyDelta(state, nameof(GameState.Coal), coalGain);

        if (actualIronGain > 0 || actualCopperGain > 0 || actualCoalGain > 0)
        {
            logs.Add($"矿坑开采：铁矿+{actualIronGain}、铜矿+{actualCopperGain}、煤矿+{actualCoalGain}。");
        }
    }

    private static double ResolveMiningLaborFactor(GameState state)
    {
        var laborScore = state.Workers + (state.Farmers * 0.35);
        var expectedLabor = Math.Max(state.MiningLevel * 12.0, 1.0);
        return Math.Clamp(laborScore / expectedLabor, MinLaborFactor, MaxLaborFactor);
    }

    private static void ResolveSmelting(GameState state, List<string> logs)
    {
        if (state.WorkshopBuildings <= 0)
        {
            return;
        }

        var smeltQuota = state.WorkshopBuildings * (0.7 + (state.TechLevel * 0.25));
        var copperBatch = Math.Min(
            smeltQuota * 0.45,
            Math.Min(state.CopperOre / 1.3, state.Coal / 0.45));
        var wroughtBatch = Math.Min(
            smeltQuota * 0.70,
            Math.Min(state.IronOre / 1.9, state.Coal / 1.15));

        var copperGain = 0.0;
        var wroughtGain = 0.0;

        if (copperBatch >= 0.2)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.CopperOre), -(copperBatch * 1.3));
            InventoryRules.ApplyDelta(state, nameof(GameState.Coal), -(copperBatch * 0.45));
            copperGain = InventoryRules.ApplyDelta(state, nameof(GameState.CopperIngot), copperBatch * 0.90);
        }

        if (wroughtBatch >= 0.2)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.IronOre), -(wroughtBatch * 1.9));
            InventoryRules.ApplyDelta(state, nameof(GameState.Coal), -(wroughtBatch * 1.15));
            wroughtGain = InventoryRules.ApplyDelta(state, nameof(GameState.WroughtIron), wroughtBatch * 0.95);
        }

        if (copperGain > 0 || wroughtGain > 0)
        {
            logs.Add($"矿石冶炼：铜锭+{copperGain:0}、熟铁+{wroughtGain:0}。");
        }
    }

    private static void ResolveCivilMaterialProcessing(GameState state, List<string> logs)
    {
        var saltBatch = Math.Min(state.Brine / 1.6, (state.WorkshopBuildings * 0.40) + (state.TradeBuildings * 0.18));
        var medicineBatch = Math.Min(state.Herbs / 1.4, (state.ResearchBuildings * 0.28) + (state.Scholars * 0.06));
        var clothBatch = Math.Min(state.HempFiber / 1.5, (state.WorkshopBuildings * 0.35) + (state.TradeBuildings * 0.16));
        var leatherBatch = Math.Min(state.Hides / 1.35, (state.Workers * 0.10) + (state.WorkshopBuildings * 0.18));

        var saltGain = 0.0;
        var medicineGain = 0.0;
        var clothGain = 0.0;
        var leatherGain = 0.0;

        if (saltBatch >= 0.2)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.Brine), -(saltBatch * 1.6));
            saltGain = InventoryRules.ApplyDelta(state, nameof(GameState.FineSalt), saltBatch);
        }

        if (medicineBatch >= 0.2)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.Herbs), -(medicineBatch * 1.4));
            medicineGain = InventoryRules.ApplyDelta(state, nameof(GameState.HerbalMedicine), medicineBatch);
        }

        if (clothBatch >= 0.2)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.HempFiber), -(clothBatch * 1.5));
            clothGain = InventoryRules.ApplyDelta(state, nameof(GameState.HempCloth), clothBatch);
        }

        if (leatherBatch >= 0.2)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.Hides), -(leatherBatch * 1.35));
            leatherGain = InventoryRules.ApplyDelta(state, nameof(GameState.Leather), leatherBatch);
        }

        if (saltGain > 0 || medicineGain > 0 || clothGain > 0 || leatherGain > 0)
        {
            logs.Add($"民生产线：精盐+{saltGain:0}、药剂+{medicineGain:0}、麻布+{clothGain:0}、皮革+{leatherGain:0}。");
        }
    }

    private static void ResolveMaterialResearch(GameState state, List<string> logs)
    {
        if (state.TechLevel < 2 || state.Scholars <= 0)
        {
            return;
        }

        var synthesisBatch = Math.Min(
            state.Scholars * 0.25,
            Math.Min(state.WroughtIron / 0.9, Math.Min(state.CopperIngot / 0.35, state.Research / 5.5)));

        if (synthesisBatch < 0.35)
        {
            return;
        }

        InventoryRules.ApplyDelta(state, nameof(GameState.WroughtIron), -(synthesisBatch * 0.9));
        InventoryRules.ApplyDelta(state, nameof(GameState.CopperIngot), -(synthesisBatch * 0.35));
        state.Research = Math.Max(state.Research - (synthesisBatch * 5.5), 0);

        var compositeGain = InventoryRules.ApplyDelta(state, nameof(GameState.CompositeMaterial), synthesisBatch * 0.9);
        if (compositeGain > 0)
        {
            logs.Add($"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)}研发：复合材料+{compositeGain:0}。");
        }
    }

    private static void ResolveManufacturing(GameState state, List<string> logs)
    {
        var partBatch = Math.Min(
            state.WorkshopBuildings * 0.55,
            Math.Min(state.WroughtIron / 0.55, state.CompositeMaterial / 0.45));

        if (partBatch >= 0.3)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.WroughtIron), -(partBatch * 0.55));
            InventoryRules.ApplyDelta(state, nameof(GameState.CompositeMaterial), -(partBatch * 0.45));

            var partGain = InventoryRules.ApplyDelta(state, nameof(GameState.IndustrialParts), partBatch * 1.1);
            var toolGain = InventoryRules.ApplyDelta(state, nameof(GameState.IndustryTools), partBatch * 1.1 * 0.55);
            if (partGain > 0 || toolGain > 0)
            {
                logs.Add($"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}制造：工业部件+{partGain:0}、工具+{toolGain:0}。");
            }
        }

        var constructionBatch = Math.Min(
            state.Workers * 0.30,
            Math.Min(state.Stone / 2.1, Math.Min(state.Wood / 1.6, Math.Min(state.Clay / 0.9, state.Reeds / 0.65))));

        if (constructionBatch < 0.4)
        {
            return;
        }

        InventoryRules.ApplyDelta(state, nameof(GameState.Stone), -(constructionBatch * 2.1));
        InventoryRules.ApplyDelta(state, nameof(GameState.Wood), -(constructionBatch * 1.6));
        InventoryRules.ApplyDelta(state, nameof(GameState.Clay), -(constructionBatch * 0.9));
        InventoryRules.ApplyDelta(state, nameof(GameState.Reeds), -(constructionBatch * 0.65));

        var constructionGain = constructionBatch;
        if (state.WroughtIron >= constructionBatch * 0.18)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.WroughtIron), -(constructionBatch * 0.18));
            constructionGain *= 1.12;
        }

        var actualConstructionGain = InventoryRules.ApplyDelta(state, nameof(GameState.ConstructionMaterials), constructionGain);
        if (actualConstructionGain > 0)
        {
            logs.Add($"营造司加工：建造构件+{actualConstructionGain:0}。");
        }
    }

    private static void ResolveWarehouse(GameState state, List<string> logs)
    {
        InventoryRules.EndTransaction(state);
        state.WarehouseCapacity = IndustryRules.CalculateWarehouseCapacity(state);
        var usedBefore = state.GetWarehouseUsed();
        if (usedBefore <= state.WarehouseCapacity)
        {
            return;
        }

        var overflow = usedBefore - state.WarehouseCapacity;
        var totalOverflow = overflow;

        state.Reeds = DrainResource(state.Reeds, ref overflow);
        state.Hides = DrainResource(state.Hides, ref overflow);
        state.HempFiber = DrainResource(state.HempFiber, ref overflow);
        state.Herbs = DrainResource(state.Herbs, ref overflow);
        state.Brine = DrainResource(state.Brine, ref overflow);
        state.Clay = DrainResource(state.Clay, ref overflow);
        state.RawStone = DrainResource(state.RawStone, ref overflow);
        state.Timber = DrainResource(state.Timber, ref overflow);
        state.Coal = DrainResource(state.Coal, ref overflow);
        state.CopperOre = DrainResource(state.CopperOre, ref overflow);
        state.IronOre = DrainResource(state.IronOre, ref overflow);
        state.IndustrialParts = DrainResource(state.IndustrialParts, ref overflow);
        state.ConstructionMaterials = DrainResource(state.ConstructionMaterials, ref overflow);
        state.CopperIngot = DrainResource(state.CopperIngot, ref overflow);
        state.WroughtIron = DrainResource(state.WroughtIron, ref overflow);
        state.MetalIngot = DrainResource(state.MetalIngot, ref overflow);
        state.CompositeMaterial = DrainResource(state.CompositeMaterial, ref overflow);
        state.Wood = DrainResource(state.Wood, ref overflow);
        state.Stone = DrainResource(state.Stone, ref overflow);
        state.FineSalt = DrainResource(state.FineSalt, ref overflow);
        state.HerbalMedicine = DrainResource(state.HerbalMedicine, ref overflow);
        state.HempCloth = DrainResource(state.HempCloth, ref overflow);
        state.Leather = DrainResource(state.Leather, ref overflow);
        state.Food = DrainResource(state.Food, ref overflow);
        state.IndustryTools = DrainResource(state.IndustryTools, ref overflow);
        state.RareMaterial = DrainResource(state.RareMaterial, ref overflow);

        if (totalOverflow > 0.01)
        {
            logs.Add($"仓储超容：挤压损耗资源 {(int)Math.Round(totalOverflow - overflow, MidpointRounding.AwayFromZero)}。");
        }
    }

    private static double DrainResource(double stock, ref double overflow)
    {
        if (overflow <= 0 || stock <= 0)
        {
            return stock;
        }

        var drain = Math.Min(stock, overflow);
        var remaining = stock - drain;
        overflow -= drain;
        return remaining;
    }
}
