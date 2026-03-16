using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

// 繁育系统：按小时推动人口繁衍事件
public class BreedingSystem
{
    // 随机数生成器
    private readonly RandomNumberGenerator _rng = new();

    // 初始化并随机化种子
    public BreedingSystem()
    {
        _rng.Randomize();
    }

    // 每小时执行一次繁育判定
    public bool TickHour(GameState state, out string? log)
    {
        log = null;

        // 基础人口门槛
        if (state.Population < 100)
        {
            return false;
        }

        // 繁育基础概率：幸福度越高越容易
        var baseChance = 0.12 + (state.Happiness / 400.0);
        if (_rng.Randf() > baseChance)
        {
            return false;
        }

        // 1-2 名精英后代
        var born = _rng.Randf() < 0.08 ? 2 : 1;
        state.ElitePopulation += born;

        // 小概率触发“血脉突变”加成
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
