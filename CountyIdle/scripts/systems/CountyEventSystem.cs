using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class CountyEventSystem
{
    private const int PositiveEventCooldownHours = 2;
    private const int NegativeEventCooldownHours = 3;

    private const double MinTriggerChance = 0.12;
    private const double MaxTriggerChance = 0.48;
    private const double CandidateChanceBonus = 0.08;
    private const double ThreatChanceScale = 0.0015;

    private const int MinHappiness = 5;
    private const int MaxHappiness = 100;

    private readonly RandomNumberGenerator _rng = new();

    public CountyEventSystem()
    {
        _rng.Randomize();
    }

    public bool TickHour(GameState state, out string? log)
    {
        log = null;
        InventoryRules.EndTransaction(state);
        state.EventCooldownHours = Math.Max(state.EventCooldownHours, 0);

        if (state.EventCooldownHours > 0)
        {
            state.EventCooldownHours -= 1;
            return false;
        }

        var candidates = BuildCandidates(state);
        if (candidates.Count == 0)
        {
            return false;
        }

        var triggerChance = Math.Clamp(
            MinTriggerChance + (candidates.Count * CandidateChanceBonus) + (state.Threat * ThreatChanceScale),
            MinTriggerChance,
            MaxTriggerChance);

        if (_rng.Randf() > triggerChance)
        {
            return false;
        }

        var selected = PickWeightedCandidate(candidates);
        log = selected.Resolve(state);
        state.EventCooldownHours = selected.CooldownHours;
        return true;
    }

    private static List<EventCandidate> BuildCandidates(GameState state)
    {
        var candidates = new List<EventCandidate>();

        if (state.Merchants >= 10 && state.Happiness >= 55)
        {
            candidates.Add(new EventCandidate(
                weight: 1.1 + (state.Merchants * 0.04),
                cooldownHours: PositiveEventCooldownHours,
                resolve: ResolveMarketFair));
        }

        if (state.Scholars >= 8)
        {
            candidates.Add(new EventCandidate(
                weight: 1.0 + (state.Scholars * 0.05),
                cooldownHours: PositiveEventCooldownHours,
                resolve: ResolveAcademyLecture));
        }

        if (state.Threat >= 42)
        {
            candidates.Add(new EventCandidate(
                weight: 0.95 + ((state.Threat - 42) * 0.03),
                cooldownHours: NegativeEventCooldownHours,
                resolve: ResolveBorderRaid));
        }

        return candidates;
    }

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

    private static string ResolveMarketFair(GameState state)
    {
        var goldGain = 16 + (state.Merchants * 0.9);
        var foodGain = 10 + (state.Merchants * 0.35);

        var actualGoldGain = InventoryRules.ApplyDelta(state, nameof(GameState.Gold), goldGain);
        var actualFoodGain = InventoryRules.ApplyDelta(state, nameof(GameState.Food), foodGain);
        state.Happiness = Math.Clamp(state.Happiness + 0.9, MinHappiness, MaxHappiness);

        return $"商路集市：商贾来往，获得金币+{actualGoldGain}、粮食+{actualFoodGain}。";
    }

    private static string ResolveAcademyLecture(GameState state)
    {
        var researchGain = 8 + (state.Scholars * 0.75);
        var happinessGain = state.TechLevel >= 1 ? 1.1 : 0.7;

        state.Research += researchGain;
        state.Happiness = Math.Clamp(state.Happiness + happinessGain, MinHappiness, MaxHappiness);

                    return $"传法院讲习：弟子推演阵图，获得科研+{researchGain:0}，民心提升。";
    }

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

    private sealed class EventCandidate
    {
        public EventCandidate(double weight, int cooldownHours, Func<GameState, string> resolve)
        {
            Weight = weight;
            CooldownHours = cooldownHours;
            Resolve = resolve;
        }

        public double Weight { get; }
        public int CooldownHours { get; }
        public Func<GameState, string> Resolve { get; }
    }
}
