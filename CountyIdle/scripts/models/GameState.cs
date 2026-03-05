using System;

namespace CountyIdle.Models;

public class GameState
{
    public int Population { get; set; } = 120;
    public int HousingCapacity { get; set; } = 180;
    public int ElitePopulation { get; set; } = 8;

    public int Farmers { get; set; } = 70;
    public int Workers { get; set; } = 25;
    public int Merchants { get; set; } = 12;
    public int Scholars { get; set; } = 8;

    public double Happiness { get; set; } = 72.0;
    public double Threat { get; set; } = 10.0;

    public double Food { get; set; } = 680;
    public double Wood { get; set; } = 220;
    public double Stone { get; set; } = 140;
    public double Gold { get; set; } = 90;
    public double Research { get; set; } = 0;
    public double RareMaterial { get; set; } = 0;

    public int ExplorationDepth { get; set; } = 1;
    public bool ExplorationEnabled { get; set; } = true;
    public int ExplorationProgressHours { get; set; } = 0;
    public double AvgGearScore { get; set; } = 12;

    public int GameMinutes { get; set; } = 0;
    public int HourSettlements { get; set; } = 0;

    public int GetAssignedPopulation()
    {
        return Farmers + Workers + Merchants + Scholars;
    }

    public int GetUnassignedPopulation()
    {
        return Math.Max(Population - GetAssignedPopulation(), 0);
    }

    public GameState Clone()
    {
        return (GameState)MemberwiseClone();
    }
}
