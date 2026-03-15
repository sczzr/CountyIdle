using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class IndustrySystem
{
    private const double AgricultureBuildWoodCost = 24;
    private const double AgricultureBuildStoneCost = 12;
    private const double AgricultureBuildGoldCost = 10;
    private const double AgricultureBuildContributionCost = 10;
    private const double AgricultureBuildConstructionCost = 0;

    private const double WorkshopBuildWoodCost = 26;
    private const double WorkshopBuildStoneCost = 18;
    private const double WorkshopBuildGoldCost = 12;
    private const double WorkshopBuildContributionCost = 12;
    private const double WorkshopBuildConstructionCost = 1;

    private const double ResearchBuildWoodCost = 16;
    private const double ResearchBuildStoneCost = 22;
    private const double ResearchBuildGoldCost = 22;
    private const double ResearchBuildContributionCost = 18;
    private const double ResearchBuildConstructionCost = 2;

    private const double TradeBuildWoodCost = 18;
    private const double TradeBuildStoneCost = 14;
    private const double TradeBuildGoldCost = 24;
    private const double TradeBuildContributionCost = 16;
    private const double TradeBuildConstructionCost = 2;

    private const double AdminBuildWoodCost = 20;
    private const double AdminBuildStoneCost = 20;
    private const double AdminBuildGoldCost = 20;
    private const double AdminBuildContributionCost = 14;
    private const double AdminBuildConstructionCost = 2;
    private const double ForestryChainWoodCost = 12;
    private const double ForestryChainStoneCost = 10;
    private const double ForestryChainGoldCost = 8;
    private const double ForestryChainContributionCost = 6;
    private const double ForestryChainConstructionCost = 0.5;
    private const double MasonryChainWoodCost = 10;
    private const double MasonryChainStoneCost = 12;
    private const double MasonryChainGoldCost = 8;
    private const double MasonryChainContributionCost = 7;
    private const double MasonryChainConstructionCost = 0.6;
    private const double MedicinalChainWoodCost = 9;
    private const double MedicinalChainStoneCost = 8;
    private const double MedicinalChainGoldCost = 9;
    private const double MedicinalChainContributionCost = 6;
    private const double MedicinalChainConstructionCost = 0.4;
    private const double FiberChainWoodCost = 11;
    private const double FiberChainStoneCost = 7;
    private const double FiberChainGoldCost = 8;
    private const double FiberChainContributionCost = 6;
    private const double FiberChainConstructionCost = 0.4;

    public bool TickHour(GameState state, out string? log)
    {
        log = null;
        IndustryRules.EnsureDefaults(state);

        var logs = new List<string>();
        EnsureJobCap(state, logs);
        ResolveToolConsumption(state, logs);

        if (logs.Count == 0)
        {
            return false;
        }

        log = string.Join(" | ", logs);
        return true;
    }

    public bool TryConstructBuilding(GameState state, IndustryBuildingType buildingType, out string log)
    {
        InventoryRules.EndTransaction(state);
        IndustryRules.EnsureDefaults(state);

        if (state.Workers <= 0)
        {
            log = "缺少管理人员，无法组织建造。";
            return false;
        }

        if (!TryBuildByType(state, buildingType, out log))
        {
            return false;
        }

        return true;
    }

    public bool TryCraftTools(GameState state, out string log)
    {
        InventoryRules.EndTransaction(state);
        IndustryRules.EnsureDefaults(state);
        MaterialRules.EnsureDefaults(state);
        SectRuleTreeRules.EnsureDefaults(state);
        SectPeakSupportRules.EnsureDefaults(state);

        var ruleTreeToolModifier = SectRuleTreeRules.GetToolCraftModifier(state);
        var supportToolModifier = SectPeakSupportRules.GetToolCraftModifier(state);
        var quarterToolModifier = SectGovernanceRules.GetQuarterToolCraftModifier(state);
        var supportDefinition = SectPeakSupportRules.GetActiveDefinition(state);

        if (state.Workers <= 0)
        {
            log = "缺少管理人员，无法组织锻制工器。";
            return false;
        }

        if (state.WorkshopBuildings <= 0)
        {
            log = $"缺少{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}，无法锻制工器。";
            return false;
        }

        var woodCost = 8 + (state.WorkshopBuildings * 2);
        var stoneCost = 6 + (state.WorkshopBuildings * 1.5);
        var goldCost = 4 + (state.Workers * 0.25);
        var contributionCost = 6 + (state.WorkshopBuildings * 0.5);
        var partCost = state.TechLevel >= 1 ? 1 + (state.WorkshopBuildings * 0.2) : 0;

        if (MaterialRules.HasTieredMetals(state))
        {
            var wroughtIronCost = 2.8 + (state.WorkshopBuildings * 0.85);
            var copperIngotCost = (state.TechLevel >= 1 ? 0.8 : 0.35) + (state.WorkshopBuildings * 0.20);
            if (!CanAfford(
                    state,
                    woodCost,
                    stoneCost,
                    goldCost,
                    contributionCost,
                    industrialParts: partCost,
                    wroughtIronCost: wroughtIronCost,
                    copperIngotCost: copperIngotCost))
            {
                log = $"锻制工器失败：{MaterialSemanticRules.GetDisplayName(nameof(GameState.Wood))}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.Stone))}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold))}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.ContributionPoints))}或矿材不足。";
                return false;
            }

            ConsumeBuildCost(
                state,
                woodCost,
                stoneCost,
                goldCost,
                contributionCost,
                industrialParts: partCost,
                wroughtIronCost: wroughtIronCost,
                copperIngotCost: copperIngotCost);
            var tieredAdvancedFactor = 1.03 + (partCost * 0.04);
            var tieredToolGain = ((state.WorkshopBuildings * 18) + (state.Workers * 1.8)) * tieredAdvancedFactor * ruleTreeToolModifier * supportToolModifier * quarterToolModifier;
            var actualToolGain = InventoryRules.ApplyDelta(state, nameof(GameState.IndustryTools), tieredToolGain);
            log =
                $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}锻器：消耗{MaterialSemanticRules.GetDisplayName(nameof(GameState.Wood))}{InventoryRules.QuantizeCost(woodCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.Stone))}{InventoryRules.QuantizeCost(stoneCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold))}{InventoryRules.QuantizeCost(goldCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.ContributionPoints))}{InventoryRules.QuantizeCost(contributionCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.WroughtIron))}{InventoryRules.QuantizeCost(wroughtIronCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.CopperIngot))}{InventoryRules.QuantizeCost(copperIngotCost)}，{MaterialSemanticRules.FormatDelta(nameof(GameState.IndustryTools), actualToolGain)}。";
            if (supportToolModifier > 1.0)
            {
                log += $" {supportDefinition.DisplayName}协同生效，工器产出提升 {(supportToolModifier - 1.0) * 100:0}% 。";
            }
            if (ruleTreeToolModifier > 1.0)
            {
                log += $" 门规验收加持，工器产出提升 {(ruleTreeToolModifier - 1.0) * 100:0}% 。";
            }
            if (quarterToolModifier > 1.0)
            {
                log += $" 季度法令加持，工器产出提升 {(quarterToolModifier - 1.0) * 100:0}% 。";
            }
            return true;
        }

        var ironOreCost = 4 + (state.WorkshopBuildings * 1.2);
        if (!CanAfford(state, woodCost, stoneCost, goldCost, contributionCost, ironOreCost: ironOreCost, industrialParts: partCost))
        {
            log = $"锻制工器失败：{MaterialSemanticRules.GetDisplayName(nameof(GameState.Wood))}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.Stone))}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold))}、{MaterialSemanticRules.GetDisplayName(nameof(GameState.ContributionPoints))}或矿材不足。";
            return false;
        }

        ConsumeBuildCost(state, woodCost, stoneCost, goldCost, contributionCost, ironOreCost: ironOreCost, industrialParts: partCost);
        var legacyAdvancedFactor = state.TechLevel >= 1 ? 1.0 + (partCost * 0.04) : 1.0;
        var legacyToolGain = ((state.WorkshopBuildings * 18) + (state.Workers * 1.8)) * legacyAdvancedFactor * ruleTreeToolModifier * supportToolModifier * quarterToolModifier;
        var actualLegacyToolGain = InventoryRules.ApplyDelta(state, nameof(GameState.IndustryTools), legacyToolGain);
        log =
            $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}开炉：消耗{MaterialSemanticRules.GetDisplayName(nameof(GameState.Wood))}{InventoryRules.QuantizeCost(woodCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.Stone))}{InventoryRules.QuantizeCost(stoneCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold))}{InventoryRules.QuantizeCost(goldCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.ContributionPoints))}{InventoryRules.QuantizeCost(contributionCost)}/{MaterialSemanticRules.GetDisplayName(nameof(GameState.IronOre))}{InventoryRules.QuantizeCost(ironOreCost)}，{MaterialSemanticRules.FormatDelta(nameof(GameState.IndustryTools), actualLegacyToolGain)}（旧链路兼容）。";
        if (supportToolModifier > 1.0)
        {
            log += $" {supportDefinition.DisplayName}协同生效，工器产出提升 {(supportToolModifier - 1.0) * 100:0}% 。";
        }
        if (ruleTreeToolModifier > 1.0)
        {
            log += $" 门规验收加持，工器产出提升 {(ruleTreeToolModifier - 1.0) * 100:0}% 。";
        }
        if (quarterToolModifier > 1.0)
        {
            log += $" 季度法令加持，工器产出提升 {(quarterToolModifier - 1.0) * 100:0}% 。";
        }
        return true;
    }

    public bool TryBuildTierZeroChain(GameState state, TierZeroMaterialChainType chainType, out string log)
    {
        InventoryRules.EndTransaction(state);
        IndustryRules.EnsureDefaults(state);
        MaterialRules.EnsureDefaults(state);

        if (state.Workers <= 0)
        {
            log = "缺少管理人员，无法组织 T0 链路扩建。";
            return false;
        }

        var (wood, stone, gold, contribution, construction, displayName, nextLevel) = chainType switch
        {
            TierZeroMaterialChainType.Forestry => (
                ForestryChainWoodCost + (state.ForestryChainLevel * 3),
                ForestryChainStoneCost + (state.ForestryChainLevel * 2.5),
                ForestryChainGoldCost + (state.ForestryChainLevel * 2.2),
                ForestryChainContributionCost + (state.ForestryChainLevel * 1.5),
                ForestryChainConstructionCost + (state.ForestryChainLevel * 0.25),
                MaterialRules.GetTierZeroChainDisplayName(chainType),
                state.ForestryChainLevel + 1),
            TierZeroMaterialChainType.Masonry => (
                MasonryChainWoodCost + (state.MasonryChainLevel * 2.6),
                MasonryChainStoneCost + (state.MasonryChainLevel * 3),
                MasonryChainGoldCost + (state.MasonryChainLevel * 2.1),
                MasonryChainContributionCost + (state.MasonryChainLevel * 1.6),
                MasonryChainConstructionCost + (state.MasonryChainLevel * 0.28),
                MaterialRules.GetTierZeroChainDisplayName(chainType),
                state.MasonryChainLevel + 1),
            TierZeroMaterialChainType.Medicinal => (
                MedicinalChainWoodCost + (state.MedicinalChainLevel * 2.4),
                MedicinalChainStoneCost + (state.MedicinalChainLevel * 2.0),
                MedicinalChainGoldCost + (state.MedicinalChainLevel * 2.4),
                MedicinalChainContributionCost + (state.MedicinalChainLevel * 1.5),
                MedicinalChainConstructionCost + (state.MedicinalChainLevel * 0.18),
                MaterialRules.GetTierZeroChainDisplayName(chainType),
                state.MedicinalChainLevel + 1),
            TierZeroMaterialChainType.Fiber => (
                FiberChainWoodCost + (state.FiberChainLevel * 2.8),
                FiberChainStoneCost + (state.FiberChainLevel * 1.8),
                FiberChainGoldCost + (state.FiberChainLevel * 2.2),
                FiberChainContributionCost + (state.FiberChainLevel * 1.5),
                FiberChainConstructionCost + (state.FiberChainLevel * 0.18),
                MaterialRules.GetTierZeroChainDisplayName(chainType),
                state.FiberChainLevel + 1),
            _ => (0.0, 0.0, 0.0, 0.0, 0.0, "T0 链路", 0)
        };

        if (!CanAfford(state, wood, stone, gold, contribution, constructionMaterials: construction))
        {
            log =
                $"{displayName}扩建失败：需木{InventoryRules.QuantizeCost(wood)}/石{InventoryRules.QuantizeCost(stone)}/灵石{InventoryRules.QuantizeCost(gold)}/贡献{InventoryRules.QuantizeCost(contribution)}/建材{InventoryRules.QuantizeCost(construction)}。";
            return false;
        }

        ConsumeBuildCost(state, wood, stone, gold, contribution, constructionMaterials: construction);
        switch (chainType)
        {
            case TierZeroMaterialChainType.Forestry:
                state.ForestryChainLevel = nextLevel;
                break;
            case TierZeroMaterialChainType.Masonry:
                state.MasonryChainLevel = nextLevel;
                break;
            case TierZeroMaterialChainType.Medicinal:
                state.MedicinalChainLevel = nextLevel;
                break;
            case TierZeroMaterialChainType.Fiber:
                state.FiberChainLevel = nextLevel;
                break;
        }

        log =
            $"T0 链扩建：{displayName} 升至 Lv.{nextLevel}（木{InventoryRules.QuantizeCost(wood)}/石{InventoryRules.QuantizeCost(stone)}/灵石{InventoryRules.QuantizeCost(gold)}/贡献{InventoryRules.QuantizeCost(contribution)}/建材{InventoryRules.QuantizeCost(construction)}）。";
        return true;
    }

    public bool TryUpgradeMineAndWarehouse(GameState state, out string log)
    {
        InventoryRules.EndTransaction(state);
        IndustryRules.EnsureDefaults(state);

        if (state.Workers <= 0)
        {
            log = "缺少管理人员，无法组织矿仓联建。";
            return false;
        }

        var woodCost = 18 + (state.MiningLevel * 4) + (state.WarehouseLevel * 5);
        var stoneCost = 22 + (state.MiningLevel * 6) + (state.WarehouseLevel * 6);
        var goldCost = 14 + (state.MiningLevel * 3) + (state.WarehouseLevel * 4);
        var contributionCost = 18 + (state.MiningLevel * 2) + (state.WarehouseLevel * 2);
        var constructionCost = 3 + (state.MiningLevel * 1.5);

        if (!CanAfford(state, woodCost, stoneCost, goldCost, contributionCost, constructionMaterials: constructionCost))
        {
            log =
                $"矿仓联建失败：需木{InventoryRules.QuantizeCost(woodCost)}/石{InventoryRules.QuantizeCost(stoneCost)}/灵石{InventoryRules.QuantizeCost(goldCost)}/贡献{InventoryRules.QuantizeCost(contributionCost)}/建材{InventoryRules.QuantizeCost(constructionCost)}。";
            return false;
        }

        ConsumeBuildCost(state, woodCost, stoneCost, goldCost, contributionCost, constructionMaterials: constructionCost);
        state.MiningLevel += 1;
        state.WarehouseLevel += 1;
        state.WarehouseCapacity = IndustryRules.CalculateWarehouseCapacity(state);

        log = $"矿仓联建：矿坑 Lv.{state.MiningLevel}，仓储 Lv.{state.WarehouseLevel}，容量 {state.WarehouseCapacity:0}（消耗灵石{InventoryRules.QuantizeCost(goldCost)}/贡献{InventoryRules.QuantizeCost(contributionCost)}）。";
        return true;
    }

    private static void EnsureJobCap(GameState state, List<string> logs)
    {
        ClampSkill(state, CraftSkillType.SpiritPlant, IndustryRules.GetSpiritPlantCapacity(state), logs);
        ClampSkill(state, CraftSkillType.SpiritBeast, IndustryRules.GetSpiritBeastCapacity(state), logs);
        ClampSkill(state, CraftSkillType.Alchemy, IndustryRules.GetAlchemyCapacity(state), logs);
        ClampSkill(state, CraftSkillType.Forging, IndustryRules.GetForgingCapacity(state), logs);
        ClampSkill(state, CraftSkillType.Talisman, IndustryRules.GetTalismanCapacity(state), logs);
        ClampSkill(state, CraftSkillType.Formation, IndustryRules.GetFormationCapacity(state), logs);
        ClampSkill(state, CraftSkillType.Golem, IndustryRules.GetGolemCapacity(state), logs);
        ClampSkill(state, CraftSkillType.Arcane, IndustryRules.GetArcaneCapacity(state), logs);
    }

    private static void ClampSkill(GameState state, CraftSkillType skillType, int capacity, List<string> logs)
    {
        var assigned = IndustryRules.GetAssigned(state, skillType);
        if (assigned <= capacity)
        {
            return;
        }

        IndustryRules.SetAssigned(state, skillType, capacity);
        logs.Add($"{SkillProgressionRules.GetActiveSkillName(state, skillType)}道额超编，已按道额容量回退至 {capacity}。");
    }

    private static bool TryBuildByType(GameState state, IndustryBuildingType buildingType, out string log)
    {
        var (wood, stone, gold, contribution, construction, title) = buildingType switch
        {
            IndustryBuildingType.Agriculture => (AgricultureBuildWoodCost, AgricultureBuildStoneCost, AgricultureBuildGoldCost, AgricultureBuildContributionCost, AgricultureBuildConstructionCost, SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)),
            IndustryBuildingType.Workshop => (WorkshopBuildWoodCost, WorkshopBuildStoneCost, WorkshopBuildGoldCost, WorkshopBuildContributionCost, WorkshopBuildConstructionCost, SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)),
            IndustryBuildingType.Research => (ResearchBuildWoodCost, ResearchBuildStoneCost, ResearchBuildGoldCost, ResearchBuildContributionCost, ResearchBuildConstructionCost, SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)),
            IndustryBuildingType.Trade => (TradeBuildWoodCost, TradeBuildStoneCost, TradeBuildGoldCost, TradeBuildContributionCost, TradeBuildConstructionCost, SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)),
            IndustryBuildingType.Administration => (AdminBuildWoodCost, AdminBuildStoneCost, AdminBuildGoldCost, AdminBuildContributionCost, AdminBuildConstructionCost, SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)),
            _ => (0, 0, 0, 0, 0, "建筑")
        };

        if (!CanAfford(state, wood, stone, gold, contribution, constructionMaterials: construction))
        {
            log =
                $"{title}建造失败：木{InventoryRules.QuantizeCost(wood)}/石{InventoryRules.QuantizeCost(stone)}/灵石{InventoryRules.QuantizeCost(gold)}/贡献{InventoryRules.QuantizeCost(contribution)}/建材{InventoryRules.QuantizeCost(construction)} 不足。";
            return false;
        }

        ConsumeBuildCost(state, wood, stone, gold, contribution, constructionMaterials: construction);
        switch (buildingType)
        {
            case IndustryBuildingType.Agriculture:
                state.AgricultureBuildings += 1;
                break;
            case IndustryBuildingType.Workshop:
                state.WorkshopBuildings += 1;
                break;
            case IndustryBuildingType.Research:
                state.ResearchBuildings += 1;
                break;
            case IndustryBuildingType.Trade:
                state.TradeBuildings += 1;
                break;
            case IndustryBuildingType.Administration:
                state.AdministrationBuildings += 1;
                break;
        }

        log =
            $"产业扩建：新建{title} 1 座（木{InventoryRules.QuantizeCost(wood)}/石{InventoryRules.QuantizeCost(stone)}/灵石{InventoryRules.QuantizeCost(gold)}/贡献{InventoryRules.QuantizeCost(contribution)}/建材{InventoryRules.QuantizeCost(construction)}）。";
        return true;
    }

    private static void ResolveToolConsumption(GameState state, List<string> logs)
    {
        var toolCost = 
            (state.FollowersSpiritPlant * 0.10) + 
            (state.FollowersForging * 0.24) + 
            (state.FollowersAlchemy * 0.18) +
            (state.FollowersTalisman * 0.16) + 
            (state.FollowersArcane * 0.26) + 
            (state.FollowersGolem * 0.21);
        if (state.IndustryTools > 0)
        {
            InventoryRules.ApplyDelta(state, nameof(GameState.IndustryTools), -toolCost);
            if (state.IndustryTools < 0)
            {
                InventoryRules.SetVisibleAmount(state, nameof(GameState.IndustryTools), 0);
            }
        }

        var coverage = IndustryRules.GetToolCoverage(state);
        if (coverage < 0.55)
        {
            logs.Add($"工器紧缺：当前工器覆盖率 {coverage * 100:0}% 。");
        }
    }

    private static bool CanAfford(
        GameState state,
        double wood,
        double stone,
        double gold,
        double contributionCost = 0,
        double constructionMaterials = 0,
        double ironOreCost = 0,
        double industrialParts = 0,
        double wroughtIronCost = 0,
        double copperIngotCost = 0)
    {
        InventoryRules.EndTransaction(state);
        return state.Wood >= InventoryRules.QuantizeCost(wood) &&
               state.Stone >= InventoryRules.QuantizeCost(stone) &&
               state.Gold >= InventoryRules.QuantizeCost(gold) &&
               state.ContributionPoints >= InventoryRules.QuantizeCost(contributionCost) &&
               state.ConstructionMaterials >= InventoryRules.QuantizeCost(constructionMaterials) &&
               state.IronOre >= InventoryRules.QuantizeCost(ironOreCost) &&
               state.IndustrialParts >= InventoryRules.QuantizeCost(industrialParts) &&
               state.WroughtIron >= InventoryRules.QuantizeCost(wroughtIronCost) &&
               state.CopperIngot >= InventoryRules.QuantizeCost(copperIngotCost);
    }

    private static void ConsumeBuildCost(
        GameState state,
        double wood,
        double stone,
        double gold,
        double contributionCost = 0,
        double constructionMaterials = 0,
        double ironOreCost = 0,
        double industrialParts = 0,
        double wroughtIronCost = 0,
        double copperIngotCost = 0)
    {
        state.Wood = Math.Max(state.Wood - InventoryRules.QuantizeCost(wood), 0);
        state.Stone = Math.Max(state.Stone - InventoryRules.QuantizeCost(stone), 0);
        state.Gold = Math.Max(state.Gold - InventoryRules.QuantizeCost(gold), 0);
        state.ContributionPoints = Math.Max(state.ContributionPoints - InventoryRules.QuantizeCost(contributionCost), 0);
        state.ConstructionMaterials = Math.Max(state.ConstructionMaterials - InventoryRules.QuantizeCost(constructionMaterials), 0);
        state.IronOre = Math.Max(state.IronOre - InventoryRules.QuantizeCost(ironOreCost), 0);
        state.IndustrialParts = Math.Max(state.IndustrialParts - InventoryRules.QuantizeCost(industrialParts), 0);
        state.WroughtIron = Math.Max(state.WroughtIron - InventoryRules.QuantizeCost(wroughtIronCost), 0);
        state.CopperIngot = Math.Max(state.CopperIngot - InventoryRules.QuantizeCost(copperIngotCost), 0);
    }
}
