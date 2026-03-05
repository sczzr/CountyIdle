using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class EconomySystem
{
    public void TickHour(GameState state)
    {
        var foodMultiplier = Math.Max(state.FoodProductionMultiplier, 1.0);
        var industryMultiplier = Math.Max(state.IndustryProductionMultiplier, 1.0);
        var tradeMultiplier = Math.Max(state.TradeProductionMultiplier, 1.0);

        state.Food += state.Farmers * 2.8 * foodMultiplier;
        state.Wood += state.Workers * 0.9 * industryMultiplier;
        state.Stone += state.Workers * 0.55 * industryMultiplier;
        state.Gold += state.Merchants * 0.85 * tradeMultiplier;
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
