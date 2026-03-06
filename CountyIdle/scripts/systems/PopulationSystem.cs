using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class PopulationSystem
{
    private const int MinimumPopulation = 20;

    public bool TickHour(GameState state, out string? log)
    {
        PopulationRules.EnsureDefaults(state);
        var logs = new List<string>();
        var previousCommuteMinutes = PopulationRules.GetCommuteMinutes(state);

        PopulationRules.RefreshDynamicCommute(state);

        var foodRequired = state.Population * 0.65;
        state.Food -= foodRequired;
        var foodDeficit = 0.0;

        if (state.Food < 0)
        {
            foodDeficit = Math.Abs(state.Food);
            var starvationLoss = (int)Math.Ceiling(foodDeficit * 0.08);
            starvationLoss = Math.Min(starvationLoss, Math.Max(state.Population - MinimumPopulation, 0));
            var actualStarvationLoss = ApplyPopulationDeaths(state, starvationLoss);

            state.Food = 0;
            state.Happiness -= 4.5;

            if (actualStarvationLoss > 0)
            {
                logs.Add($"饥荒减员 {actualStarvationLoss}。");
            }
        }

        state.Population = Math.Max(state.ChildPopulation + state.AdultPopulation + state.ElderPopulation, MinimumPopulation);
        PopulationRules.EnsureDefaults(state);

        var sleepFactor = PopulationRules.GetSleepFactor(state);
        var housingPressure = PopulationRules.GetHousingPressure(state);
        var clothingCoverage = PopulationRules.GetClothingCoverage(state);
        var commuteMinutes = PopulationRules.GetCommuteMinutes(state);

        var clothingWear = state.Population * 0.012;
        state.ClothingStock = Math.Max(state.ClothingStock - clothingWear, 0);

        var birthHappinessFactor = Math.Clamp(state.Happiness / 100.0, 0.5, 1.2);
        var birthHousingFactor = Math.Clamp((double)state.HousingCapacity / Math.Max(state.Population, 1), 0.6, 1.1);
        var growthMultiplier = Math.Max(state.PopulationGrowthMultiplier, 1.0);
        var births = (int)Math.Floor(state.AdultPopulation * 0.0032 * birthHappinessFactor * birthHousingFactor * growthMultiplier);
        births = Math.Max(births, 0);
        state.ChildPopulation += births;

        var childToAdult = Math.Min((int)Math.Floor(state.ChildPopulation * 0.0018), state.ChildPopulation);
        state.ChildPopulation -= childToAdult;
        state.AdultPopulation += childToAdult;

        var adultToElder = Math.Min((int)Math.Floor(state.AdultPopulation * 0.0009), state.AdultPopulation);
        state.AdultPopulation -= adultToElder;
        state.ElderPopulation += adultToElder;

        var sicknessRate = 0.002 +
                           (housingPressure * 0.012) +
                           (Math.Max(0, 0.75 - sleepFactor) * 0.010) +
                           ((1 - clothingCoverage) * 0.006);
        var healthyAdults = Math.Max(state.AdultPopulation - state.SickPopulation, 0);
        var newSick = Math.Min((int)Math.Floor(state.AdultPopulation * sicknessRate), healthyAdults);
        var recover = Math.Min((int)Math.Floor(state.SickPopulation * (0.11 + clothingCoverage * 0.06)), state.SickPopulation);
        state.SickPopulation = Math.Clamp(state.SickPopulation + newSick - recover, 0, state.AdultPopulation);

        var foodPressure = foodDeficit / Math.Max(state.Population, 1.0);
        var deathRate = 0.0004 +
                        ((double)state.SickPopulation / Math.Max(state.Population, 1)) * 0.0015 +
                        (foodPressure * 0.004);
        var naturalDeaths = (int)Math.Ceiling(state.Population * deathRate);
        naturalDeaths = Math.Min(naturalDeaths, Math.Max(state.Population - MinimumPopulation, 0));
        var actualNaturalDeaths = ApplyPopulationDeaths(state, naturalDeaths);

        state.Population = Math.Max(state.ChildPopulation + state.AdultPopulation + state.ElderPopulation, MinimumPopulation);
        PopulationRules.EnsureDefaults(state);

        var foodMood = state.Food > state.Population * 4 ? 0.8 : -0.3;
        var housingMood = state.HousingCapacity >= state.Population ? 0.5 : -1.0;
        var threatMood = -(state.Threat * 0.06);
        var prosperityMood = state.Gold > state.Population ? 0.35 : -0.25;
        var sleepMood = sleepFactor >= 0.95 ? 0.4 : (sleepFactor < 0.8 ? -0.9 : 0);
        var clothingMood = clothingCoverage >= 0.9 ? 0.3 : (clothingCoverage < 0.6 ? -0.7 : 0);
        var commuteMood = -(commuteMinutes / 60.0) * 0.8;
        var sicknessMood = -((double)state.SickPopulation / Math.Max(state.Population, 1)) * 1.6;
        var crowdingMood = -(housingPressure * 2.4);

        state.Happiness = Math.Clamp(
            state.Happiness + foodMood + housingMood + threatMood + prosperityMood + sleepMood + clothingMood + commuteMood + sicknessMood + crowdingMood,
            5,
            100);

        if (births > 0 || childToAdult > 0 || adultToElder > 0 || newSick > 0 || recover > 0 || actualNaturalDeaths > 0)
        {
            logs.Add($"人口循环：新生{births} 成长{childToAdult} 衰老{adultToElder} 患病+{newSick}/康复{recover} 死亡{actualNaturalDeaths}。");
        }

        if (sleepFactor < 0.8)
        {
            logs.Add($"睡眠不足：休整系数 {sleepFactor:0.00}。");
        }

        if (commuteMinutes >= 35)
        {
            logs.Add($"远距通勤：单程 {commuteMinutes:0} 分钟，岗位到岗率下降。");
        }

        if (Math.Abs(commuteMinutes - previousCommuteMinutes) >= 4)
        {
            logs.Add($"通勤更新：当前单程约 {commuteMinutes:0} 分钟。 ");
        }

        if (clothingCoverage < 0.6)
        {
            logs.Add($"衣物紧缺：覆盖率 {clothingCoverage * 100:0}% 。");
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

    private static int ApplyPopulationDeaths(GameState state, int deaths)
    {
        var remaining = Math.Max(deaths, 0);
        if (remaining <= 0)
        {
            return 0;
        }

        var deadFromSick = Math.Min(state.SickPopulation, remaining);
        state.SickPopulation -= deadFromSick;
        state.AdultPopulation -= deadFromSick;
        remaining -= deadFromSick;

        var deadFromElder = Math.Min(state.ElderPopulation, remaining);
        state.ElderPopulation -= deadFromElder;
        remaining -= deadFromElder;

        var healthyAdult = Math.Max(state.AdultPopulation - state.SickPopulation, 0);
        var deadFromHealthyAdult = Math.Min(healthyAdult, remaining);
        state.AdultPopulation -= deadFromHealthyAdult;
        remaining -= deadFromHealthyAdult;

        var deadFromChild = Math.Min(state.ChildPopulation, remaining);
        state.ChildPopulation -= deadFromChild;
        remaining -= deadFromChild;

        return deaths - remaining;
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
