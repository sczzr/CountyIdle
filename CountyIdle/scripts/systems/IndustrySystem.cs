using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class IndustrySystem
{
    private const double AgricultureBuildWoodCost = 24;
    private const double AgricultureBuildStoneCost = 12;
    private const double AgricultureBuildGoldCost = 10;

    private const double WorkshopBuildWoodCost = 26;
    private const double WorkshopBuildStoneCost = 18;
    private const double WorkshopBuildGoldCost = 12;

    private const double ResearchBuildWoodCost = 16;
    private const double ResearchBuildStoneCost = 22;
    private const double ResearchBuildGoldCost = 22;

    private const double TradeBuildWoodCost = 18;
    private const double TradeBuildStoneCost = 14;
    private const double TradeBuildGoldCost = 24;

    private const double AdminBuildWoodCost = 20;
    private const double AdminBuildStoneCost = 20;
    private const double AdminBuildGoldCost = 20;

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
        IndustryRules.EnsureDefaults(state);

        if (state.Workers <= 0)
        {
            log = "缺少管理人员，无法组织制工具。";
            return false;
        }

        if (state.WorkshopBuildings <= 0)
        {
            log = "缺少工坊，无法制造工具。";
            return false;
        }

        var woodCost = 8 + (state.WorkshopBuildings * 2);
        var stoneCost = 6 + (state.WorkshopBuildings * 1.5);
        var goldCost = 4 + (state.Workers * 0.25);
        if (!CanAfford(state, woodCost, stoneCost, goldCost))
        {
            log = "制工具失败：木石金不足。";
            return false;
        }

        ConsumeBuildCost(state, woodCost, stoneCost, goldCost);
        var toolGain = (state.WorkshopBuildings * 18) + (state.Workers * 1.8);
        state.IndustryTools += toolGain;
        log = $"工坊开炉：消耗木{woodCost:0}/石{stoneCost:0}/金{goldCost:0}，工具+{toolGain:0}。";
        return true;
    }

    private static void EnsureJobCap(GameState state, List<string> logs)
    {
        ClampJob(state, JobType.Worker, IndustryRules.GetManagementCapacity(state), "管理人员", logs);
        ClampJob(state, JobType.Scholar, IndustryRules.GetResearchCapacity(state), "研发人员", logs);
        ClampJob(state, JobType.Merchant, IndustryRules.GetCommerceCapacity(state), "商业人员", logs);
        ClampJob(state, JobType.Farmer, IndustryRules.GetProductionCapacity(state), "产业工人", logs);
    }

    private static void ClampJob(GameState state, JobType jobType, int capacity, string jobName, List<string> logs)
    {
        var assigned = IndustryRules.GetAssigned(state, jobType);
        if (assigned <= capacity)
        {
            return;
        }

        IndustryRules.SetAssigned(state, jobType, capacity);
        logs.Add($"{jobName}超编，已按产业容量回退至 {capacity}。");
    }

    private static bool TryBuildByType(GameState state, IndustryBuildingType buildingType, out string log)
    {
        var (wood, stone, gold, title) = buildingType switch
        {
            IndustryBuildingType.Agriculture => (AgricultureBuildWoodCost, AgricultureBuildStoneCost, AgricultureBuildGoldCost, "农坊"),
            IndustryBuildingType.Workshop => (WorkshopBuildWoodCost, WorkshopBuildStoneCost, WorkshopBuildGoldCost, "工坊"),
            IndustryBuildingType.Research => (ResearchBuildWoodCost, ResearchBuildStoneCost, ResearchBuildGoldCost, "学宫"),
            IndustryBuildingType.Trade => (TradeBuildWoodCost, TradeBuildStoneCost, TradeBuildGoldCost, "市集"),
            IndustryBuildingType.Administration => (AdminBuildWoodCost, AdminBuildStoneCost, AdminBuildGoldCost, "官署"),
            _ => (0, 0, 0, "建筑")
        };

        if (!CanAfford(state, wood, stone, gold))
        {
            log = $"{title}建造失败：木{wood:0}/石{stone:0}/金{gold:0} 不足。";
            return false;
        }

        ConsumeBuildCost(state, wood, stone, gold);
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

        log = $"产业扩建：新建{title} 1 座（木{wood:0}/石{stone:0}/金{gold:0}）。";
        return true;
    }

    private static void ResolveToolConsumption(GameState state, List<string> logs)
    {
        var toolCost = (state.Farmers * 0.12) + (state.Scholars * 0.18) + (state.Merchants * 0.12);
        state.IndustryTools = Math.Max(state.IndustryTools - toolCost, 0);

        var coverage = IndustryRules.GetToolCoverage(state);
        if (coverage < 0.55)
        {
            logs.Add($"工具紧缺：当前工具覆盖率 {coverage * 100:0}% 。");
        }
    }

    private static bool CanAfford(GameState state, double wood, double stone, double gold)
    {
        return state.Wood >= wood && state.Stone >= stone && state.Gold >= gold;
    }

    private static void ConsumeBuildCost(GameState state, double wood, double stone, double gold)
    {
        state.Wood -= wood;
        state.Stone -= stone;
        state.Gold -= gold;
    }
}
