using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

// 弟子行囊规则
public static class DiscipleBackpackRules
{
    // 行囊中金币键
    public static readonly string GoldKey = nameof(GameState.Gold);
    // 行囊中稀有材料键
    public static readonly string RareMaterialKey = nameof(GameState.RareMaterial);

    // 需要跟踪的默认键
    private static readonly string[] TrackedKeys =
    {
        GoldKey,
        RareMaterialKey
    };

    // 确保行囊字典初始化并包含默认键
    public static void EnsureDefaults(GameState state)
    {
        state.DiscipleBackpackInventory ??= new Dictionary<string, int>();
        foreach (var key in TrackedKeys)
        {
            if (!state.DiscipleBackpackInventory.ContainsKey(key))
            {
                state.DiscipleBackpackInventory[key] = 0;
            }
        }
    }

    // 获取行囊某项数量
    public static int GetAmount(GameState state, string key)
    {
        EnsureDefaults(state);
        return state.DiscipleBackpackInventory.TryGetValue(key, out var amount) ? amount : 0;
    }

    // 应用数量增减（不允许小于 0）
    public static int ApplyDelta(GameState state, string key, int delta)
    {
        EnsureDefaults(state);
        var current = GetAmount(state, key);
        var next = current + delta;
        if (next < 0)
        {
            next = 0;
        }

        state.DiscipleBackpackInventory[key] = next;
        return next - current;
    }

    // 获取行囊汇总
    public static (int Gold, int RareMaterial) GetSummary(GameState state)
    {
        EnsureDefaults(state);
        return (GetAmount(state, GoldKey), GetAmount(state, RareMaterialKey));
    }

    // 判断行囊是否有战利品
    public static bool HasAnyLoot(GameState state)
    {
        var (gold, rare) = GetSummary(state);
        return gold > 0 || rare > 0;
    }

    // 尝试将行囊物资转入仓库
    public static bool TryTransferToWarehouse(GameState state, out string? log)
    {
        EnsureDefaults(state);
        var (gold, rare) = GetSummary(state);
        if (gold <= 0 && rare <= 0)
        {
            log = null;
            return false;
        }

        var entries = new List<string>();
        if (gold > 0)
        {
            InventoryRules.ApplyDelta(state, GoldKey, gold);
            entries.Add(MaterialSemanticRules.FormatDelta(GoldKey, gold));
            state.DiscipleBackpackInventory[GoldKey] = 0;
        }

        if (rare > 0)
        {
            InventoryRules.ApplyDelta(state, RareMaterialKey, rare);
            entries.Add(MaterialSemanticRules.FormatDelta(RareMaterialKey, rare));
            state.DiscipleBackpackInventory[RareMaterialKey] = 0;
        }

        log = $"行囊入库：{string.Join("、", entries)}。";
        return true;
    }
}
