using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class InventoryRules
{
    private sealed record Accessor(Func<GameState, double> Get, Action<GameState, double> Set);

    private static readonly IReadOnlyDictionary<string, Accessor> Accessors = new Dictionary<string, Accessor>
    {
        [nameof(GameState.ClothingStock)] = new(static state => state.ClothingStock, static (state, value) => state.ClothingStock = value),
        [nameof(GameState.Food)] = new(static state => state.Food, static (state, value) => state.Food = value),
        [nameof(GameState.Wood)] = new(static state => state.Wood, static (state, value) => state.Wood = value),
        [nameof(GameState.Stone)] = new(static state => state.Stone, static (state, value) => state.Stone = value),
        [nameof(GameState.Timber)] = new(static state => state.Timber, static (state, value) => state.Timber = value),
        [nameof(GameState.RawStone)] = new(static state => state.RawStone, static (state, value) => state.RawStone = value),
        [nameof(GameState.Clay)] = new(static state => state.Clay, static (state, value) => state.Clay = value),
        [nameof(GameState.Brine)] = new(static state => state.Brine, static (state, value) => state.Brine = value),
        [nameof(GameState.Herbs)] = new(static state => state.Herbs, static (state, value) => state.Herbs = value),
        [nameof(GameState.HempFiber)] = new(static state => state.HempFiber, static (state, value) => state.HempFiber = value),
        [nameof(GameState.Reeds)] = new(static state => state.Reeds, static (state, value) => state.Reeds = value),
        [nameof(GameState.Hides)] = new(static state => state.Hides, static (state, value) => state.Hides = value),
        [nameof(GameState.FineSalt)] = new(static state => state.FineSalt, static (state, value) => state.FineSalt = value),
        [nameof(GameState.HerbalMedicine)] = new(static state => state.HerbalMedicine, static (state, value) => state.HerbalMedicine = value),
        [nameof(GameState.HempCloth)] = new(static state => state.HempCloth, static (state, value) => state.HempCloth = value),
        [nameof(GameState.Leather)] = new(static state => state.Leather, static (state, value) => state.Leather = value),
        [nameof(GameState.Gold)] = new(static state => state.Gold, static (state, value) => state.Gold = value),
        [nameof(GameState.RareMaterial)] = new(static state => state.RareMaterial, static (state, value) => state.RareMaterial = value),
        [nameof(GameState.IronOre)] = new(static state => state.IronOre, static (state, value) => state.IronOre = value),
        [nameof(GameState.CopperOre)] = new(static state => state.CopperOre, static (state, value) => state.CopperOre = value),
        [nameof(GameState.Coal)] = new(static state => state.Coal, static (state, value) => state.Coal = value),
        [nameof(GameState.CopperIngot)] = new(static state => state.CopperIngot, static (state, value) => state.CopperIngot = value),
        [nameof(GameState.WroughtIron)] = new(static state => state.WroughtIron, static (state, value) => state.WroughtIron = value),
        [nameof(GameState.MetalIngot)] = new(static state => state.MetalIngot, static (state, value) => state.MetalIngot = value),
        [nameof(GameState.CompositeMaterial)] = new(static state => state.CompositeMaterial, static (state, value) => state.CompositeMaterial = value),
        [nameof(GameState.IndustrialParts)] = new(static state => state.IndustrialParts, static (state, value) => state.IndustrialParts = value),
        [nameof(GameState.ConstructionMaterials)] = new(static state => state.ConstructionMaterials, static (state, value) => state.ConstructionMaterials = value),
        [nameof(GameState.IndustryTools)] = new(static state => state.IndustryTools, static (state, value) => state.IndustryTools = value)
    };

    public static void EnsureDefaults(GameState state)
    {
        state.DiscreteInventoryProgress ??= new Dictionary<string, double>();
        foreach (var key in Accessors.Keys)
        {
            if (!state.DiscreteInventoryProgress.ContainsKey(key))
            {
                state.DiscreteInventoryProgress[key] = 0;
            }
        }
    }

    public static void BeginTransaction(GameState state)
    {
        EnsureDefaults(state);
        foreach (var (key, accessor) in Accessors)
        {
            var progress = GetProgress(state, key);
            if (Math.Abs(progress) <= 0.000001)
            {
                continue;
            }

            accessor.Set(state, accessor.Get(state) + progress);
            state.DiscreteInventoryProgress[key] = 0;
        }
    }

    public static void EndTransaction(GameState state)
    {
        EnsureDefaults(state);
        foreach (var (key, accessor) in Accessors)
        {
            var total = accessor.Get(state);
            if (double.IsNaN(total) || double.IsInfinity(total))
            {
                total = 0;
            }

            var visible = Math.Floor(total);
            var progress = GetProgress(state, key) + (total - visible);
            NormalizeVisibleAndProgress(ref visible, ref progress);
            accessor.Set(state, visible);
            state.DiscreteInventoryProgress[key] = progress;
        }
    }

    public static Dictionary<string, int> CaptureVisible(GameState state)
    {
        EndTransaction(state);
        var snapshot = new Dictionary<string, int>(Accessors.Count);
        foreach (var (key, accessor) in Accessors)
        {
            snapshot[key] = (int)accessor.Get(state);
        }

        return snapshot;
    }

    public static int ApplyDelta(GameState state, string key, double rawDelta)
    {
        EnsureDefaults(state);
        EndTransaction(state);

        if (!Accessors.TryGetValue(key, out var accessor))
        {
            return 0;
        }

        var visible = accessor.Get(state);
        var progress = GetProgress(state, key) + rawDelta;
        var actualDelta = 0;

        if (progress >= 1)
        {
            var gain = (int)Math.Floor(progress);
            visible += gain;
            progress -= gain;
            actualDelta += gain;
        }

        if (progress < 0)
        {
            var loss = (int)Math.Ceiling(-progress);
            visible -= loss;
            progress += loss;
            actualDelta -= loss;
        }

        if (progress >= 0.999999)
        {
            visible += 1;
            progress = 0;
            actualDelta += 1;
        }
        else if (progress <= 0.000001)
        {
            progress = 0;
        }

        accessor.Set(state, visible);
        state.DiscreteInventoryProgress[key] = progress;
        return actualDelta;
    }

    public static int GetDelta(IReadOnlyDictionary<string, int> snapshot, GameState state, string key)
    {
        EndTransaction(state);
        if (!Accessors.TryGetValue(key, out var accessor))
        {
            return 0;
        }

        var before = snapshot.TryGetValue(key, out var value) ? value : 0;
        return (int)accessor.Get(state) - before;
    }

    public static int PredictDelta(GameState state, string key, double rawDelta)
    {
        EndTransaction(state);
        if (!Accessors.TryGetValue(key, out var accessor))
        {
            return 0;
        }

        var visible = accessor.Get(state);
        var progress = GetProgress(state, key);
        var total = visible + progress + rawDelta;
        var afterVisible = Math.Floor(total);
        var afterProgress = total - afterVisible;
        NormalizeVisibleAndProgress(ref afterVisible, ref afterProgress);
        return (int)afterVisible - (int)visible;
    }

    public static int QuantizeCost(double amount)
    {
        return Math.Max((int)Math.Ceiling(amount - 0.000001), 0);
    }

    public static int GetVisibleAmount(GameState state, string key)
    {
        EndTransaction(state);
        return Accessors.TryGetValue(key, out var accessor)
            ? (int)accessor.Get(state)
            : 0;
    }

    public static void SetVisibleAmount(GameState state, string key, int amount)
    {
        EnsureDefaults(state);
        if (!Accessors.TryGetValue(key, out var accessor))
        {
            return;
        }

        accessor.Set(state, amount);
        state.DiscreteInventoryProgress[key] = 0;
    }

    public static int QuantizeOutput(double amount)
    {
        return Math.Max((int)Math.Floor(amount + 0.000001), 0);
    }

    private static double GetProgress(GameState state, string key)
    {
        return state.DiscreteInventoryProgress.TryGetValue(key, out var progress)
            ? progress
            : 0;
    }

    private static void NormalizeVisibleAndProgress(ref double visible, ref double progress)
    {
        if (progress >= 1 || progress < 0)
        {
            var carry = Math.Floor(progress);
            visible += carry;
            progress -= carry;
        }

        if (progress < 0)
        {
            visible -= 1;
            progress += 1;
        }

        if (progress >= 0.999999)
        {
            visible += 1;
            progress = 0;
        }
        else if (progress <= 0.000001)
        {
            progress = 0;
        }
    }
}
