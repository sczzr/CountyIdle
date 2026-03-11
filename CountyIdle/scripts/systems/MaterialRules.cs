using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class MaterialRules
{
    private const double TimberGatherRatePerWorker = 0.72;
    private const double RawStoneGatherRatePerWorker = 0.52;
    private const double ClayGatherRatePerWorker = 0.28;
    private const double BrineGatherRatePerWorker = 0.22;
    private const double HerbsGatherRatePerWorker = 0.18;
    private const double HempGatherRatePerWorker = 0.26;
    private const double ReedsGatherRatePerWorker = 0.20;
    private const double HidesGatherRatePerWorker = 0.12;
    private const double HidesGatherRatePerWorkerFromWorkers = 0.08;

    private const double TimberToWoodInput = 1.08;
    private const double RawStoneToStoneInput = 1.12;

    public readonly record struct TieredGatheringEstimate(
        int EffectiveGatherers,
        double ProductionFactor,
        double TimberGain,
        double RawStoneGain,
        double ClayGain,
        double BrineGain,
        double HerbsGain,
        double HempGain,
        double ReedsGain,
        double HidesGain);

    public readonly record struct PrimaryProcessingEstimate(
        double WoodGain,
        double StoneGain);

    public static void EnsureDefaults(GameState state)
    {
        InventoryRules.EnsureDefaults(state);
        state.Timber = Math.Max(state.Timber, 0);
        state.RawStone = Math.Max(state.RawStone, 0);
        state.Clay = Math.Max(state.Clay, 0);
        state.Brine = Math.Max(state.Brine, 0);
        state.Herbs = Math.Max(state.Herbs, 0);
        state.HempFiber = Math.Max(state.HempFiber, 0);
        state.Reeds = Math.Max(state.Reeds, 0);
        state.Hides = Math.Max(state.Hides, 0);
        state.FineSalt = Math.Max(state.FineSalt, 0);
        state.HerbalMedicine = Math.Max(state.HerbalMedicine, 0);
        state.HempCloth = Math.Max(state.HempCloth, 0);
        state.Leather = Math.Max(state.Leather, 0);
        state.CopperIngot = Math.Max(state.CopperIngot, 0);
        state.WroughtIron = Math.Max(state.WroughtIron, 0);
        state.MetalIngot = Math.Max(state.MetalIngot, 0);
        state.ForestryChainLevel = Math.Max(state.ForestryChainLevel, 0);
        state.MasonryChainLevel = Math.Max(state.MasonryChainLevel, 0);
        state.MedicinalChainLevel = Math.Max(state.MedicinalChainLevel, 0);
        state.FiberChainLevel = Math.Max(state.FiberChainLevel, 0);

        if (state.ForestryChainLevel + state.MasonryChainLevel + state.MedicinalChainLevel + state.FiberChainLevel <= 0)
        {
            state.ForestryChainLevel = Math.Max((state.AgricultureBuildings + 1) / 2, 1);
            state.MasonryChainLevel = Math.Max((state.WorkshopBuildings + 1) / 2, 1);
            state.MedicinalChainLevel = Math.Max(Math.Max(state.TradeBuildings, state.ResearchBuildings), 1);
            state.FiberChainLevel = Math.Max((state.AgricultureBuildings + state.TradeBuildings + 1) / 3, 1);
        }

        if (!HasTieredMetals(state) && state.MetalIngot > 0.01)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.CopperIngot), state.MetalIngot * 0.42);
            InventoryRules.ApplyDelta(state, nameof(GameState.WroughtIron), state.MetalIngot * 0.58);
            InventoryRules.SetVisibleAmount(state, nameof(GameState.MetalIngot), 0);
        }
    }

    public static bool HasTieredMetals(GameState state)
    {
        return state.CopperIngot > 0.01 || state.WroughtIron > 0.01;
    }

    public static int GetEffectiveGatherers(GameState state)
    {
        var productionAssigned = Math.Min(state.Farmers, IndustryRules.GetProductionCapacity(state));
        return Math.Max(productionAssigned, 0);
    }

    public static double GetGatheringProductionFactor(GameState state)
    {
        return Math.Max(state.IndustryProductionMultiplier, 1.0) *
               IndustryRules.GetManagementBoost(state) *
               IndustryRules.GetToolCoverage(state);
    }

    public static TieredGatheringEstimate EstimateGathering(GameState state)
    {
        var effectiveGatherers = GetEffectiveGatherers(state);
        if (effectiveGatherers <= 0)
        {
            return new TieredGatheringEstimate(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        var productionFactor = GetGatheringProductionFactor(state);
        var forestryFactor = ResolveChainFactor(state.ForestryChainLevel, 0.64, 0.18);
        var masonryFactor = ResolveChainFactor(state.MasonryChainLevel, 0.62, 0.18);
        var medicinalFactor = ResolveChainFactor(state.MedicinalChainLevel, 0.60, 0.17);
        var fiberFactor = ResolveChainFactor(state.FiberChainLevel, 0.60, 0.17);

        var timberGain = effectiveGatherers * TimberGatherRatePerWorker * productionFactor * forestryFactor;
        var rawStoneGain = effectiveGatherers * RawStoneGatherRatePerWorker * productionFactor * masonryFactor;
        var clayGain = effectiveGatherers * ClayGatherRatePerWorker * productionFactor * (masonryFactor * 0.88);
        var brineGain = effectiveGatherers * BrineGatherRatePerWorker * productionFactor * medicinalFactor;
        var herbsGain = effectiveGatherers * HerbsGatherRatePerWorker * productionFactor * (medicinalFactor * 0.92);
        var hempGain = effectiveGatherers * HempGatherRatePerWorker * productionFactor * fiberFactor;
        var reedsGain = effectiveGatherers * ReedsGatherRatePerWorker * productionFactor * (fiberFactor * 0.94);
        var hidesGain =
            ((effectiveGatherers * HidesGatherRatePerWorker) + (state.Workers * HidesGatherRatePerWorkerFromWorkers)) *
            productionFactor *
            (fiberFactor * 0.90);

        return new TieredGatheringEstimate(
            effectiveGatherers,
            productionFactor,
            timberGain,
            rawStoneGain,
            clayGain,
            brineGain,
            herbsGain,
            hempGain,
            reedsGain,
            hidesGain);
    }

    public static PrimaryProcessingEstimate EstimatePrimaryProcessing(
        GameState state,
        double projectedTimberGain = 0,
        double projectedRawStoneGain = 0)
    {
        var woodCapacity = (state.WorkshopBuildings * 7.5) + (state.Workers * 0.9);
        var stoneCapacity = (state.WorkshopBuildings * 5.5) + (state.Workers * 0.72);

        var woodGain = Math.Min(Math.Max(state.Timber + projectedTimberGain, 0) / TimberToWoodInput, woodCapacity);
        var stoneGain = Math.Min(Math.Max(state.RawStone + projectedRawStoneGain, 0) / RawStoneToStoneInput, stoneCapacity);

        return new PrimaryProcessingEstimate(
            Math.Max(woodGain, 0),
            Math.Max(stoneGain, 0));
    }

    public static double GetTimberToWoodInput()
    {
        return TimberToWoodInput;
    }

    public static double GetRawStoneToStoneInput()
    {
        return RawStoneToStoneInput;
    }

    public static string GetTierZeroChainDisplayName(TierZeroMaterialChainType chainType)
    {
        return chainType switch
        {
            TierZeroMaterialChainType.Forestry => "灵木链（灵植园/伐木坊）",
            TierZeroMaterialChainType.Masonry => "石陶链（采罡场/赤陶窑/石作坊）",
            TierZeroMaterialChainType.Medicinal => "盐丹链（盐泉/采药圃/丹房）",
            TierZeroMaterialChainType.Fiber => "织裘链（青麻圃/青芦泽/灵兽围/制裘坊）",
            _ => "T0 链路"
        };
    }

    public static string DescribeTierZeroChains(GameState state)
    {
        return
            $"T0 灵材链：灵木链 Lv.{state.ForestryChainLevel} · 石陶链 Lv.{state.MasonryChainLevel}\n" +
            $"盐丹链 Lv.{state.MedicinalChainLevel} · 织裘链 Lv.{state.FiberChainLevel}";
    }

    private static double ResolveChainFactor(int chainLevel, double baseFactor, double perLevelFactor)
    {
        return Math.Clamp(baseFactor + (Math.Max(chainLevel, 0) * perLevelFactor), 0.45, 1.85);
    }
}
