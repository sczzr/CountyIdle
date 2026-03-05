using CountyIdle.Models;

namespace CountyIdle.Systems;

public class EconomySystem
{
    public void TickHour(GameState state)
    {
        state.Food += state.Farmers * 2.8;
        state.Wood += state.Workers * 0.9;
        state.Stone += state.Workers * 0.55;
        state.Gold += state.Merchants * 0.85;
        state.Research += state.Scholars * 0.4;

        var citizenWageCost = state.Population * 0.05;
        state.Gold -= citizenWageCost;

        if (state.Gold < 0)
        {
            state.Happiness -= 1.2;
            state.Gold = 0;
        }
    }
}
