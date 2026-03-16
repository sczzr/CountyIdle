using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class WorkshopCraftedInventoryRules
{
    public static readonly string IndustryToolsKey = nameof(GameState.IndustryTools);

    private static readonly string[] TrackedKeys =
    {
        IndustryToolsKey
    };

    private static readonly HashSet<string> TrackedKeySet = new(TrackedKeys);

    public static void EnsureDefaults(GameState state)
    {
        state.WorkshopCraftedInventory ??= new Dictionary<string, int>();
        state.WorkshopCraftedProgress ??= new Dictionary<string, double>();
        foreach (var key in TrackedKeys)
        {
            if (!state.WorkshopCraftedInventory.ContainsKey(key))
            {
                state.WorkshopCraftedInventory[key] = 0;
            }

            if (!state.WorkshopCraftedProgress.ContainsKey(key))
            {
                state.WorkshopCraftedProgress[key] = 0;
            }
        }
    }

    public static int GetAmount(GameState state, string key)
    {
        EnsureDefaults(state);
        return state.WorkshopCraftedInventory.TryGetValue(key, out var amount) ? amount : 0;
    }

    public static int ApplyDelta(GameState state, string key, double rawDelta)
    {
        EnsureDefaults(state);

        var visible = GetAmount(state, key);
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

        if (visible < 0)
        {
            actualDelta -= visible;
            visible = 0;
            if (progress < 0)
            {
                progress = 0;
            }
        }

        state.WorkshopCraftedInventory[key] = visible;
        state.WorkshopCraftedProgress[key] = progress;
        return actualDelta;
    }

    public static int GetSummary(GameState state)
    {
        EnsureDefaults(state);
        var total = 0;
        foreach (var amount in state.WorkshopCraftedInventory.Values)
        {
            if (amount > 0)
            {
                total += amount;
            }
        }

        return total;
    }

    public static IReadOnlyList<(string Key, int Amount)> GetSummaryEntries(GameState state)
    {
        EnsureDefaults(state);
        var entries = new List<(string Key, int Amount)>();
        foreach (var key in TrackedKeys)
        {
            var amount = GetAmount(state, key);
            if (amount > 0)
            {
                entries.Add((key, amount));
            }
        }

        if (state.WorkshopCraftedInventory.Count > TrackedKeys.Length)
        {
            var extraKeys = new List<string>();
            foreach (var entry in state.WorkshopCraftedInventory)
            {
                if (entry.Value <= 0 || TrackedKeySet.Contains(entry.Key))
                {
                    continue;
                }

                extraKeys.Add(entry.Key);
            }

            extraKeys.Sort(StringComparer.Ordinal);
            foreach (var key in extraKeys)
            {
                entries.Add((key, state.WorkshopCraftedInventory[key]));
            }
        }

        return entries;
    }

    public static bool HasAnyCrafted(GameState state)
    {
        return GetSummary(state) > 0;
    }

    public static bool TryTransferToWarehouse(GameState state, out string? log)
    {
        EnsureDefaults(state);
        var entries = GetSummaryEntries(state);
        if (entries.Count == 0)
        {
            log = null;
            return false;
        }

        var logEntries = new List<string>();
        foreach (var entry in entries)
        {
            InventoryRules.ApplyDelta(state, entry.Key, entry.Amount);
            logEntries.Add(MaterialSemanticRules.FormatDelta(entry.Key, entry.Amount));
            state.WorkshopCraftedInventory[entry.Key] = 0;
        }

        log = $"工坊成品入库：{string.Join("、", logEntries)}。";
        return true;
    }

    private static double GetProgress(GameState state, string key)
    {
        return state.WorkshopCraftedProgress.TryGetValue(key, out var progress) ? progress : 0;
    }
}
