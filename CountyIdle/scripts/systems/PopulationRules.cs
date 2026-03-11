using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class PopulationRules
{
    private const int MinimumPopulation = 20;

    public static void EnsureDefaults(GameState state)
    {
        state.Population = Math.Max(state.Population, MinimumPopulation);
        state.ChildPopulation = Math.Max(state.ChildPopulation, 0);
        state.AdultPopulation = Math.Max(state.AdultPopulation, 0);
        state.ElderPopulation = Math.Max(state.ElderPopulation, 0);

        var segmentedPopulation = state.ChildPopulation + state.AdultPopulation + state.ElderPopulation;
        if (segmentedPopulation <= 0)
        {
            state.ChildPopulation = (int)Math.Round(state.Population * 0.18);
            state.ElderPopulation = (int)Math.Round(state.Population * 0.12);
            state.AdultPopulation = Math.Max(state.Population - state.ChildPopulation - state.ElderPopulation, 0);
        }
        else if (segmentedPopulation != state.Population)
        {
            if (segmentedPopulation > state.Population)
            {
                var scale = (double)state.Population / segmentedPopulation;
                state.ChildPopulation = (int)Math.Round(state.ChildPopulation * scale);
                state.ElderPopulation = (int)Math.Round(state.ElderPopulation * scale);
                state.AdultPopulation = Math.Max(state.Population - state.ChildPopulation - state.ElderPopulation, 0);
            }
            else
            {
                state.AdultPopulation += state.Population - segmentedPopulation;
            }
        }

        state.SickPopulation = Math.Clamp(state.SickPopulation, 0, state.AdultPopulation);
        state.ClothingStock = Math.Max(state.ClothingStock, 0);
    }
}
