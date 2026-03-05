using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class PopulationSystem
{
    public void TickHour(GameState state)
    {
        var foodRequired = state.Population * 0.65;
        state.Food -= foodRequired;

        if (state.Food < 0)
        {
            var starvationLoss = (int)Math.Ceiling(Math.Abs(state.Food) * 0.08);
            state.Population = Math.Max(state.Population - starvationLoss, 20);
            state.Food = 0;
            state.Happiness -= 4.5;
        }

        var housingFactor = Math.Clamp((double)state.HousingCapacity / Math.Max(state.Population, 1), 0.5, 1.15);
        var foodReserveFactor = Math.Clamp(state.Food / Math.Max(state.Population * 2.0, 1.0), 0.4, 1.2);
        var happinessFactor = Math.Clamp(state.Happiness / 100.0, 0.3, 1.3);

        var growthRate = 0.006 * housingFactor * foodReserveFactor * happinessFactor;
        var newCitizens = (int)Math.Floor(state.Population * growthRate);
        state.Population += Math.Max(newCitizens, 0);

        var foodMood = state.Food > state.Population * 4 ? 0.8 : -0.3;
        var housingMood = state.HousingCapacity >= state.Population ? 0.5 : -1.0;
        var threatMood = -(state.Threat * 0.06);
        var prosperityMood = state.Gold > state.Population ? 0.35 : -0.25;

        state.Happiness = Math.Clamp(
            state.Happiness + foodMood + housingMood + threatMood + prosperityMood,
            5,
            100);

        var maxAssigned = Math.Min(state.GetAssignedPopulation(), state.Population);
        if (maxAssigned != state.GetAssignedPopulation())
        {
            var overflow = state.GetAssignedPopulation() - maxAssigned;
            RemoveOverflowJobs(state, overflow);
        }
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
