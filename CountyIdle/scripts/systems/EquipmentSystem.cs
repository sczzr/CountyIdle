using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class EquipmentSystem
{
    private const double MinDropChance = 0.35;
    private const double MaxDropChance = 0.80;
    private const double BaseDropChance = 0.35;
    private const double DepthDropChanceFactor = 0.03;

    private const double MinAffixChance = 0.22;
    private const double MaxAffixChance = 0.60;
    private const double BaseAffixChance = 0.22;
    private const double DepthAffixChanceFactor = 0.015;
    private const double AffixScoreMultiplier = 1.35;

    private readonly RandomNumberGenerator _rng = new();

    private enum GearRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public EquipmentSystem()
    {
        _rng.Randomize();
    }

    public bool TryResolveExplorationDrop(GameState state, out string? log)
    {
        log = null;

        var depth = Math.Max(state.ExplorationDepth, 1);
        var dropChance = Math.Clamp(BaseDropChance + (depth * DepthDropChanceFactor), MinDropChance, MaxDropChance);
        if (_rng.Randf() > dropChance)
        {
            return false;
        }

        var rarity = RollRarity(depth);
        var baseScoreGain = GetBaseScoreGain(rarity);
        var hasAffix = _rng.Randf() <= Math.Clamp(BaseAffixChance + (depth * DepthAffixChanceFactor), MinAffixChance, MaxAffixChance);
        var affixName = hasAffix ? RollAffixName() : null;
        var finalScoreGain = hasAffix ? baseScoreGain * AffixScoreMultiplier : baseScoreGain;

        state.AvgGearScore = Math.Max(state.AvgGearScore + finalScoreGain, 0);
        IncrementGearCount(state, rarity);

        log = hasAffix
            ? $"战利品：获得{GetRarityDisplayName(rarity)}装备【{affixName}】，战备评分 +{finalScoreGain:0.00}。"
            : $"战利品：获得{GetRarityDisplayName(rarity)}装备，战备评分 +{finalScoreGain:0.00}。";
        return true;
    }

    private GearRarity RollRarity(int depth)
    {
        var commonWeight = Math.Max(20.0, 62.0 - (depth * 2.0));
        var rareWeight = Math.Min(40.0, 26.0 + (depth * 1.5));
        var epicWeight = Math.Min(25.0, 10.0 + (depth * 0.6));
        var legendaryWeight = Math.Min(15.0, 2.0 + (depth * 0.2));

        var totalWeight = commonWeight + rareWeight + epicWeight + legendaryWeight;
        var roll = _rng.RandfRange(0.0f, (float)totalWeight);

        if (roll < commonWeight)
        {
            return GearRarity.Common;
        }

        roll -= (float)commonWeight;
        if (roll < rareWeight)
        {
            return GearRarity.Rare;
        }

        roll -= (float)rareWeight;
        if (roll < epicWeight)
        {
            return GearRarity.Epic;
        }

        return GearRarity.Legendary;
    }

    private static double GetBaseScoreGain(GearRarity rarity)
    {
        return rarity switch
        {
            GearRarity.Common => 0.25,
            GearRarity.Rare => 0.55,
            GearRarity.Epic => 0.95,
            GearRarity.Legendary => 1.60,
            _ => 0.25
        };
    }

    private static string GetRarityDisplayName(GearRarity rarity)
    {
        return rarity switch
        {
            GearRarity.Common => "普通",
            GearRarity.Rare => "精良",
            GearRarity.Epic => "史诗",
            GearRarity.Legendary => "传说",
            _ => "普通"
        };
    }

    private static void IncrementGearCount(GameState state, GearRarity rarity)
    {
        switch (rarity)
        {
            case GearRarity.Common:
                state.CommonGearCount += 1;
                break;
            case GearRarity.Rare:
                state.RareGearCount += 1;
                break;
            case GearRarity.Epic:
                state.EpicGearCount += 1;
                break;
            case GearRarity.Legendary:
                state.LegendaryGearCount += 1;
                break;
        }
    }

    private string RollAffixName()
    {
        return _rng.RandiRange(0, 3) switch
        {
            0 => "锋锐",
            1 => "迅捷",
            2 => "坚韧",
            _ => "福运"
        };
    }
}
