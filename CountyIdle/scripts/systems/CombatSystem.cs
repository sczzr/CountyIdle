using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

// 战斗/探险系统：按小时结算探险进度与战斗结果
public class CombatSystem
{
    // 随机数生成器
    private readonly RandomNumberGenerator _rng = new();
    // 装备掉落结算
    private readonly EquipmentSystem _equipmentSystem = new();

    // 初始化并随机化种子
    public CombatSystem()
    {
        _rng.Randomize();
    }

    // 每小时执行一次战斗/探险结算
    public bool TickHour(GameState state, out string? log)
    {
        log = null;
        // 每小时先结束背包事务，避免累积未提交变更
        InventoryRules.EndTransaction(state);

        // 未开启探险或没有精英人口时，仅累积威胁
        if (!state.ExplorationEnabled || state.ElitePopulation <= 0)
        {
            state.Threat = Math.Clamp(state.Threat + 0.2, 0, 100);
            return false;
        }

        // 探险进度每 3 小时结算一次
        state.ExplorationProgressHours += 1;
        if (state.ExplorationProgressHours < 3)
        {
            return false;
        }

        state.ExplorationProgressHours = 0;
        // 计算敌方战力与我方战力
        var enemyPower = 9 + state.ExplorationDepth * 1.6;
        var outerCandidateCount = DiscipleDirectiveRules.GetDirectiveCount(state, DiscipleDirectiveType.OuterMissionCandidate);
        var outerMissionTeamPowerBonus = DiscipleDirectiveRules.GetOuterMissionTeamPowerBonus(state);
        var outerMissionLootModifier = DiscipleDirectiveRules.GetOuterMissionLootModifier(state);
        var teamPower = state.ElitePopulation * 0.95 + state.AvgGearScore * 1.1 + outerMissionTeamPowerBonus;
        var winChance = Math.Clamp(0.2 + ((teamPower - enemyPower) / 28.0), 0.12, 0.9);

        // 胜利结算
        if (_rng.Randf() <= winChance)
        {
            var goldGain = (int)Math.Round((18 + state.ExplorationDepth * 3) * outerMissionLootModifier);
            var rareGain = 1 + (_rng.Randf() < 0.35 ? 1 : 0);
            var visibleGoldGain = DiscipleBackpackRules.ApplyDelta(state, DiscipleBackpackRules.GoldKey, goldGain);
            var visibleRareGain = DiscipleBackpackRules.ApplyDelta(state, DiscipleBackpackRules.RareMaterialKey, rareGain);
            state.Threat = Math.Clamp(state.Threat - 2.2, 0, 100);

            // 小概率推进更深层数
            if (_rng.Randf() < 0.38)
            {
                state.ExplorationDepth += 1;
            }

            var lootText =
                $"行囊收获：{MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold))}+{visibleGoldGain}，{MaterialSemanticRules.GetDisplayName(nameof(GameState.RareMaterial))}+{visibleRareGain}";
            var combatLog = outerCandidateCount > 0
                ? $"探险胜利：外务候补 {outerCandidateCount} 人协力，队伍战力额外 +{outerMissionTeamPowerBonus:0.0}，{lootText}，当前层数 {state.ExplorationDepth}。"
                : $"探险胜利：{lootText}，当前层数 {state.ExplorationDepth}。";
            if (_equipmentSystem.TryResolveExplorationDrop(state, out var gearLog) && !string.IsNullOrWhiteSpace(gearLog))
            {
                log = $"{combatLog} {gearLog}";
                return true;
            }

            log = combatLog;
            return true;
        }

        // 失败结算：威胁上升，可能损失精英
        state.Threat = Math.Clamp(state.Threat + 2.4, 0, 100);
        if (_rng.Randf() < 0.22 && state.ElitePopulation > 1)
        {
            state.ElitePopulation -= 1;
        }

        log = outerCandidateCount > 0
            ? "探险受挫：虽有外务候补随行，但此次未能得手，队伍负伤撤退，郡内威胁上升。"
            : "探险受挫：队伍负伤撤退，郡内威胁上升。";
        return true;
    }
}
