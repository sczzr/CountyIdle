using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class CombatSystem
{
    private readonly RandomNumberGenerator _rng = new();

    public CombatSystem()
    {
        _rng.Randomize();
    }

    public bool TickHour(GameState state, out string? log)
    {
        log = null;

        if (!state.ExplorationEnabled || state.ElitePopulation <= 0)
        {
            state.Threat = Math.Clamp(state.Threat + 0.2, 0, 100);
            return false;
        }

        state.ExplorationProgressHours += 1;
        if (state.ExplorationProgressHours < 3)
        {
            return false;
        }

        state.ExplorationProgressHours = 0;
        var enemyPower = 9 + state.ExplorationDepth * 1.6;
        var teamPower = state.ElitePopulation * 0.95 + state.AvgGearScore * 1.1;
        var winChance = Math.Clamp(0.2 + ((teamPower - enemyPower) / 28.0), 0.12, 0.9);

        if (_rng.Randf() <= winChance)
        {
            var goldGain = 18 + state.ExplorationDepth * 3;
            var rareGain = 1 + (_rng.Randf() < 0.35 ? 1 : 0);
            state.Gold += goldGain;
            state.RareMaterial += rareGain;
            state.Threat = Math.Clamp(state.Threat - 2.2, 0, 100);

            if (_rng.Randf() < 0.38)
            {
                state.ExplorationDepth += 1;
            }

            log = $"探险胜利：获得金币+{goldGain}，稀有素材+{rareGain}，当前层数 {state.ExplorationDepth}。";
            return true;
        }

        state.Threat = Math.Clamp(state.Threat + 2.4, 0, 100);
        if (_rng.Randf() < 0.22 && state.ElitePopulation > 1)
        {
            state.ElitePopulation -= 1;
        }

        log = "探险受挫：队伍负伤撤退，郡内威胁上升。";
        return true;
    }
}
