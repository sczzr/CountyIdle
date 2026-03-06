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
        IndustryRules.EnsureDefaults(state);

        var logs = new List<string>();

        ResolveMining(state, logs);
        ResolveSmelting(state, logs);
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

    private static void ResolveMining(GameState state, List<string> logs)
    {
        var laborFactor = ResolveMiningLaborFactor(state);
        var techFactor = 1.0 + (state.TechLevel * 0.08);
        var levelFactor = 1.0 + ((state.MiningLevel - 1) * 0.10);

        var ironGain = state.MiningLevel * 3.6 * laborFactor * techFactor * levelFactor;
        var copperGain = state.MiningLevel * 2.3 * laborFactor * techFactor * levelFactor;
        var coalGain = state.MiningLevel * 2.9 * laborFactor * techFactor * levelFactor;

        state.IronOre += ironGain;
        state.CopperOre += copperGain;
        state.Coal += coalGain;

        logs.Add($"矿坑开采：铁矿+{ironGain:0.0}、铜矿+{copperGain:0.0}、煤矿+{coalGain:0.0}。");
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
        var smeltBatch = Math.Min(
            smeltQuota,
            Math.Min(state.IronOre / 2.2, Math.Min(state.CopperOre / 1.2, state.Coal / 1.6)));

        if (smeltBatch < 0.5)
        {
            return;
        }

        state.IronOre = Math.Max(state.IronOre - (smeltBatch * 2.2), 0);
        state.CopperOre = Math.Max(state.CopperOre - (smeltBatch * 1.2), 0);
        state.Coal = Math.Max(state.Coal - (smeltBatch * 1.6), 0);

        var ingotGain = smeltBatch * 0.95;
        state.MetalIngot += ingotGain;
        logs.Add($"矿石冶炼：金属锭+{ingotGain:0.0}。");
    }

    private static void ResolveMaterialResearch(GameState state, List<string> logs)
    {
        if (state.TechLevel < 2 || state.Scholars <= 0)
        {
            return;
        }

        var synthesisBatch = Math.Min(
            state.Scholars * 0.25,
            Math.Min(state.MetalIngot / 1.4, state.Research / 5.5));

        if (synthesisBatch < 0.35)
        {
            return;
        }

        state.MetalIngot = Math.Max(state.MetalIngot - (synthesisBatch * 1.4), 0);
        state.Research = Math.Max(state.Research - (synthesisBatch * 5.5), 0);

        var compositeGain = synthesisBatch * 0.9;
        state.CompositeMaterial += compositeGain;
        logs.Add($"学宫研发：复合材料+{compositeGain:0.0}。");
    }

    private static void ResolveManufacturing(GameState state, List<string> logs)
    {
        var partBatch = Math.Min(
            state.WorkshopBuildings * 0.55,
            Math.Min(state.MetalIngot / 0.8, state.CompositeMaterial / 0.45));

        if (partBatch >= 0.3)
        {
            state.MetalIngot = Math.Max(state.MetalIngot - (partBatch * 0.8), 0);
            state.CompositeMaterial = Math.Max(state.CompositeMaterial - (partBatch * 0.45), 0);

            var partGain = partBatch * 1.1;
            state.IndustrialParts += partGain;
            state.IndustryTools += partGain * 0.55;
            logs.Add($"工坊制造：工业部件+{partGain:0.0}。");
        }

        var constructionBatch = Math.Min(
            state.Workers * 0.35,
            Math.Min(state.Stone / 2.6, Math.Min(state.Wood / 1.9, state.MetalIngot / 0.45)));

        if (constructionBatch < 0.4)
        {
            return;
        }

        state.Stone = Math.Max(state.Stone - (constructionBatch * 2.6), 0);
        state.Wood = Math.Max(state.Wood - (constructionBatch * 1.9), 0);
        state.MetalIngot = Math.Max(state.MetalIngot - (constructionBatch * 0.45), 0);
        state.ConstructionMaterials += constructionBatch;
        logs.Add($"营造司加工：建造构件+{constructionBatch:0.0}。");
    }

    private static void ResolveWarehouse(GameState state, List<string> logs)
    {
        state.WarehouseCapacity = IndustryRules.CalculateWarehouseCapacity(state);
        var usedBefore = state.GetWarehouseUsed();
        if (usedBefore <= state.WarehouseCapacity)
        {
            return;
        }

        var overflow = usedBefore - state.WarehouseCapacity;
        var totalOverflow = overflow;

        state.Coal = DrainResource(state.Coal, ref overflow);
        state.CopperOre = DrainResource(state.CopperOre, ref overflow);
        state.IronOre = DrainResource(state.IronOre, ref overflow);
        state.IndustrialParts = DrainResource(state.IndustrialParts, ref overflow);
        state.ConstructionMaterials = DrainResource(state.ConstructionMaterials, ref overflow);
        state.MetalIngot = DrainResource(state.MetalIngot, ref overflow);
        state.CompositeMaterial = DrainResource(state.CompositeMaterial, ref overflow);
        state.Wood = DrainResource(state.Wood, ref overflow);
        state.Stone = DrainResource(state.Stone, ref overflow);
        state.Food = DrainResource(state.Food, ref overflow);
        state.IndustryTools = DrainResource(state.IndustryTools, ref overflow);
        state.RareMaterial = DrainResource(state.RareMaterial, ref overflow);

        if (totalOverflow > 0.01)
        {
            logs.Add($"仓储超容：挤压损耗资源 {totalOverflow - overflow:0.0}。");
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
