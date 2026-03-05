using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class BreedingSystem
{
    private readonly RandomNumberGenerator _rng = new();

    public BreedingSystem()
    {
        _rng.Randomize();
    }

    public bool TickHour(GameState state, out string? log)
    {
        log = null;

        if (state.Population < 100)
        {
            return false;
        }

        var baseChance = 0.12 + (state.Happiness / 400.0);
        if (_rng.Randf() > baseChance)
        {
            return false;
        }

        var born = _rng.Randf() < 0.08 ? 2 : 1;
        state.ElitePopulation += born;

        if (_rng.Randf() < 0.16)
        {
            state.AvgGearScore += 0.6;
            log = born > 1
                ? $"血脉突变：出现{born}名稀有后代，工匠潜力提升。"
                : "血脉突变：新生精英携带优秀天赋。";
            return true;
        }

        log = born > 1 ? $"姻缘祠繁育成功：新增{born}名精英。"
            : "姻缘祠繁育成功：新增1名精英。";
        return true;
    }
}
