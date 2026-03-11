using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class PopulationSystem
{
    private const int MinimumPopulation = 20;

    public bool TickHour(GameState state, out string? log)
    {
        InventoryRules.EndTransaction(state);
        PopulationRules.EnsureDefaults(state);
        SectGovernanceRules.EnsureDefaults(state);
        SectRuleTreeRules.EnsureDefaults(state);
        SectPeakSupportRules.EnsureDefaults(state);
        var logs = new List<string>();

        var foodRequired = state.Population * 0.65;
        InventoryRules.ApplyDelta(state, nameof(GameState.Food), -foodRequired);
        var previousPopulation = state.Population;

        if (state.Food < 0)
        {
            var foodDeficit = Math.Abs(state.Food);
            var starvationLoss = (int)Math.Ceiling(foodDeficit * 0.08);
            starvationLoss = Math.Min(starvationLoss, Math.Max(state.Population - MinimumPopulation, 0));
            var actualStarvationLoss = ApplyPopulationLoss(state, starvationLoss);

            InventoryRules.SetVisibleAmount(state, nameof(GameState.Food), 0);
            state.Happiness -= 4.5;

            if (actualStarvationLoss > 0)
            {
                logs.Add($"饥荒减员 {actualStarvationLoss}。");
            }
        }

        var housingFactor = Math.Clamp((double)state.HousingCapacity / Math.Max(state.Population, 1), 0.5, 1.15);
        var foodReserveFactor = Math.Clamp(state.Food / Math.Max(state.Population * 2.0, 1.0), 0.4, 1.2);
        var happinessFactor = Math.Clamp(state.Happiness / 100.0, 0.3, 1.3);
        var growthMultiplier =
            Math.Max(state.PopulationGrowthMultiplier, 1.0) *
            SectGovernanceRules.GetPopulationGrowthModifier(state) *
            SectGovernanceRules.GetQuarterPopulationGrowthModifier(state) *
            SectRuleTreeRules.GetPopulationGrowthModifier(state) *
            SectPeakSupportRules.GetPopulationGrowthModifier(state);
        var growthRate = 0.006 * housingFactor * foodReserveFactor * happinessFactor * growthMultiplier;
        var newCitizens = Math.Max((int)Math.Floor(state.Population * growthRate), 0);
        if (newCitizens > 0)
        {
            state.Population += newCitizens;
            state.AdultPopulation += newCitizens;
        }

        PopulationRules.EnsureDefaults(state);

        var foodMood = state.Food > state.Population * 4 ? 0.8 : -0.3;
        var housingMood = state.HousingCapacity >= state.Population ? 0.5 : -1.0;
        var threatMood = -(state.Threat * 0.06);
        var prosperityMood = state.Gold > state.Population ? 0.35 : -0.25;

        state.Happiness = Math.Clamp(
            state.Happiness + foodMood + housingMood + threatMood + prosperityMood,
            5,
            100);

        state.Happiness = Math.Clamp(
            state.Happiness +
            SectGovernanceRules.GetHourlyHappinessDelta(state) +
            SectGovernanceRules.GetQuarterHappinessDelta(state) +
            SectRuleTreeRules.GetHourlyHappinessDelta(state) +
            SectPeakSupportRules.GetHourlyHappinessDelta(state),
            5,
            100);
        state.Threat = Math.Clamp(
            state.Threat +
            SectGovernanceRules.GetHourlyThreatDelta(state) +
            SectGovernanceRules.GetQuarterThreatDelta(state) +
            SectRuleTreeRules.GetHourlyThreatDelta(state) +
            SectPeakSupportRules.GetHourlyThreatDelta(state),
            0,
            100);

        var populationDelta = state.Population - previousPopulation;
        if (populationDelta > 0)
        {
            logs.Add($"门人增长 {populationDelta}。");
        }

        var maxAssigned = Math.Min(state.GetAssignedPopulation(), state.Population);
        if (maxAssigned != state.GetAssignedPopulation())
        {
            var overflow = state.GetAssignedPopulation() - maxAssigned;
            RemoveOverflowJobs(state, overflow);
        }

        if (logs.Count == 0)
        {
            log = null;
            return false;
        }

        log = string.Join(" | ", logs);
        return true;
    }

    private static int ApplyPopulationLoss(GameState state, int losses)
    {
        var remaining = Math.Max(losses, 0);
        if (remaining <= 0)
        {
            return 0;
        }

        var deadFromElder = Math.Min(state.ElderPopulation, remaining);
        state.ElderPopulation -= deadFromElder;
        remaining -= deadFromElder;

        var deadFromAdults = Math.Min(state.AdultPopulation, remaining);
        state.AdultPopulation -= deadFromAdults;
        remaining -= deadFromAdults;

        var deadFromChild = Math.Min(state.ChildPopulation, remaining);
        state.ChildPopulation -= deadFromChild;
        remaining -= deadFromChild;

        state.Population = Math.Max(state.ChildPopulation + state.AdultPopulation + state.ElderPopulation, MinimumPopulation);
        state.SickPopulation = Math.Clamp(state.SickPopulation, 0, state.AdultPopulation);
        return losses - remaining;
    }

    private static void RemoveOverflowJobs(GameState state, int overflow)
    {
        while (overflow > 0)
        {
            if (state.Scholars > 0)
            {
                state.Scholars--;
                overflow--;
                continue;
            }

            if (state.Merchants > 0)
            {
                state.Merchants--;
                overflow--;
                continue;
            }

            if (state.Workers > 0)
            {
                state.Workers--;
                overflow--;
                continue;
            }

            if (state.Farmers > 0)
            {
                state.Farmers--;
                overflow--;
                continue;
            }

            break;
        }
    }
}
