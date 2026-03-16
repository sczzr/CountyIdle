using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

// 郡县事件系统：随机触发正/负事件
public class CountyEventSystem
{
    // 正面事件冷却（小时）
    private const int PositiveEventCooldownHours = 2;
    // 负面事件冷却（小时）
    private const int NegativeEventCooldownHours = 3;

    // 触发概率上下限
    private const double MinTriggerChance = 0.12;
    private const double MaxTriggerChance = 0.48;
    // 候选事件数量带来的概率增益
    private const double CandidateChanceBonus = 0.08;
    // 威胁值对触发概率的加成系数
    private const double ThreatChanceScale = 0.0015;

    // 幸福度边界
    private const int MinHappiness = 5;
    private const int MaxHappiness = 100;

    // 随机数生成器
    private readonly RandomNumberGenerator _rng = new();

    // 初始化并随机化种子
    public CountyEventSystem()
    {
        _rng.Randomize();
    }

    // 每小时执行一次事件判定
    public bool TickHour(GameState state, out string? log)
    {
        log = null;
        // 结束库存事务，保证可见值一致
        InventoryRules.EndTransaction(state);
        // 事件冷却不允许为负
        state.EventCooldownHours = Math.Max(state.EventCooldownHours, 0);

        // 冷却期内不触发事件
        if (state.EventCooldownHours > 0)
        {
            state.EventCooldownHours -= 1;
            return false;
        }

        // 生成可触发事件候选
        var candidates = BuildCandidates(state);
        if (candidates.Count == 0)
        {
            return false;
        }

        // 根据候选数与威胁值计算触发概率
        var triggerChance = Math.Clamp(
            MinTriggerChance + (candidates.Count * CandidateChanceBonus) + (state.Threat * ThreatChanceScale),
            MinTriggerChance,
            MaxTriggerChance);

        if (_rng.Randf() > triggerChance)
        {
            return false;
        }

        // 按权重抽取事件
        var selected = PickWeightedCandidate(candidates);
        log = selected.Resolve(state);
        state.EventCooldownHours = selected.CooldownHours;
        return true;
    }

    // 构建事件候选列表
    private static List<EventCandidate> BuildCandidates(GameState state)
    {
        var candidates = new List<EventCandidate>();

        // 商路集市：商贾与幸福度门槛
        if (state.Merchants >= 10 && state.Happiness >= 55)
        {
            candidates.Add(new EventCandidate(
                weight: 1.1 + (state.Merchants * 0.04),
                cooldownHours: PositiveEventCooldownHours,
                resolve: ResolveMarketFair));
        }

        // 传法院讲习：学者门槛
        if (state.Scholars >= 8)
        {
            candidates.Add(new EventCandidate(
                weight: 1.0 + (state.Scholars * 0.05),
                cooldownHours: PositiveEventCooldownHours,
                resolve: ResolveAcademyLecture));
        }

        // 边境袭扰：高威胁触发
        if (state.Threat >= 42)
        {
            candidates.Add(new EventCandidate(
                weight: 0.95 + ((state.Threat - 42) * 0.03),
                cooldownHours: NegativeEventCooldownHours,
                resolve: ResolveBorderRaid));
        }

        return candidates;
    }

    // 按权重随机挑选候选事件
    private EventCandidate PickWeightedCandidate(List<EventCandidate> candidates)
    {
        var totalWeight = 0.0;
        foreach (var candidate in candidates)
        {
            totalWeight += Math.Max(candidate.Weight, 0.01);
        }

        if (totalWeight <= 0)
        {
            return candidates[0];
        }

        var roll = _rng.RandfRange(0f, (float)totalWeight);
        var cumulative = 0.0;
        foreach (var candidate in candidates)
        {
            cumulative += Math.Max(candidate.Weight, 0.01);
            if (roll <= cumulative)
            {
                return candidate;
            }
        }

        return candidates[^1];
    }

    // 商路集市结算
    private static string ResolveMarketFair(GameState state)
    {
        var goldGain = 16 + (state.Merchants * 0.9);
        var foodGain = 10 + (state.Merchants * 0.35);

        var actualGoldGain = InventoryRules.ApplyDelta(state, nameof(GameState.Gold), goldGain);
        var actualFoodGain = InventoryRules.ApplyDelta(state, nameof(GameState.Food), foodGain);
        state.Happiness = Math.Clamp(state.Happiness + 0.9, MinHappiness, MaxHappiness);

        return $"商路集市：商贾来往，获得金币+{actualGoldGain}、粮食+{actualFoodGain}。";
    }

    // 传法院讲习结算
    private static string ResolveAcademyLecture(GameState state)
    {
        var researchGain = 8 + (state.Scholars * 0.75);
        var happinessGain = state.TechLevel >= 1 ? 1.1 : 0.7;

        state.Research += researchGain;
        state.Happiness = Math.Clamp(state.Happiness + happinessGain, MinHappiness, MaxHappiness);

        return SectNamingRules.ReplaceKnownNames(state,
            $"传法院讲习：弟子推演阵图，获得科研+{researchGain:0}，民心提升。");
    }

    // 边境袭扰结算
    private static string ResolveBorderRaid(GameState state)
    {
        var mitigation = state.ElitePopulation >= 10 ? 0.55 : state.ElitePopulation >= 6 ? 0.75 : 1.0;
        var goldLoss = (12 + (state.Threat * 0.42)) * mitigation;
        var foodLoss = (18 + (state.Threat * 0.50)) * mitigation;
        var happinessLoss = 2.4 * mitigation;

        var actualGoldLoss = -InventoryRules.ApplyDelta(state, nameof(GameState.Gold), -goldLoss);
        var actualFoodLoss = -InventoryRules.ApplyDelta(state, nameof(GameState.Food), -foodLoss);
        if (state.Gold < 0)
        {
            InventoryRules.SetVisibleAmount(state, nameof(GameState.Gold), 0);
        }

        if (state.Food < 0)
        {
            InventoryRules.SetVisibleAmount(state, nameof(GameState.Food), 0);
        }

        state.Happiness = Math.Clamp(state.Happiness - happinessLoss, MinHappiness, MaxHappiness);
        state.Threat = Math.Clamp(state.Threat - (state.ElitePopulation >= 6 ? 1.5 : 0.5), 0, 100);

        return mitigation < 1
            ? $"警告：边境袭扰被精英队压制，仍损失金币-{actualGoldLoss}、粮食-{actualFoodLoss}。"
            : $"警告：边境袭扰冲击郡县，损失金币-{actualGoldLoss}、粮食-{actualFoodLoss}。";
    }

    // 事件候选配置
    private sealed class EventCandidate
    {
        // 构建候选事件
        public EventCandidate(double weight, int cooldownHours, Func<GameState, string> resolve)
        {
            Weight = weight;
            CooldownHours = cooldownHours;
            Resolve = resolve;
        }

        // 权重
        public double Weight { get; }
        // 冷却小时数
        public int CooldownHours { get; }
        // 结算逻辑
        public Func<GameState, string> Resolve { get; }
    }
}
