using System;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.Core;

public partial class GameLoop : Node
{
    private const double RealSecondsPerGameMinute = 1.0;
    private const int MinutesPerSettlement = 60;

    private readonly PopulationSystem _populationSystem = new();
    private readonly EconomySystem _economySystem = new();
    private readonly BreedingSystem _breedingSystem = new();
    private readonly CombatSystem _combatSystem = new();
    private readonly EventBus _eventBus = new();

    private double _secondAccumulator;
    private int _minuteAccumulator;
    private GameState _state = new();

    public GameState State => _state;
    public EventBus Events => _eventBus;

    public override void _Process(double delta)
    {
        _secondAccumulator += delta;

        while (_secondAccumulator >= RealSecondsPerGameMinute)
        {
            _secondAccumulator -= RealSecondsPerGameMinute;
            AdvanceOneGameMinute();
        }
    }

    public void LoadState(GameState state)
    {
        _state = state ?? new GameState();
        _eventBus.PublishState(_state.Clone());
    }

    public void ResetState()
    {
        _state = new GameState();
        _minuteAccumulator = 0;
        _secondAccumulator = 0;
        _eventBus.PublishLog("已重置到初始状态。");
        _eventBus.PublishState(_state.Clone());
    }

    public void ToggleExploration()
    {
        _state.ExplorationEnabled = !_state.ExplorationEnabled;
        _eventBus.PublishLog(_state.ExplorationEnabled ? "已开启森林探险。" : "已暂停森林探险。");
        _eventBus.PublishState(_state.Clone());
    }

    public void AdjustJob(JobType jobType, int delta)
    {
        if (delta == 0)
        {
            return;
        }

        if (delta > 0 && _state.GetUnassignedPopulation() < delta)
        {
            _eventBus.PublishLog("空闲人口不足，无法继续分配。");
            return;
        }

        switch (jobType)
        {
            case JobType.Farmer:
                _state.Farmers = Math.Max(_state.Farmers + delta, 0);
                break;
            case JobType.Worker:
                _state.Workers = Math.Max(_state.Workers + delta, 0);
                break;
            case JobType.Merchant:
                _state.Merchants = Math.Max(_state.Merchants + delta, 0);
                break;
            case JobType.Scholar:
                _state.Scholars = Math.Max(_state.Scholars + delta, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(jobType), jobType, null);
        }

        var overAssigned = _state.GetAssignedPopulation() - _state.Population;
        if (overAssigned > 0)
        {
            RemoveFromJob(jobType, overAssigned);
        }

        _eventBus.PublishState(_state.Clone());
    }

    private void RemoveFromJob(JobType jobType, int amount)
    {
        switch (jobType)
        {
            case JobType.Farmer:
                _state.Farmers = Math.Max(_state.Farmers - amount, 0);
                break;
            case JobType.Worker:
                _state.Workers = Math.Max(_state.Workers - amount, 0);
                break;
            case JobType.Merchant:
                _state.Merchants = Math.Max(_state.Merchants - amount, 0);
                break;
            case JobType.Scholar:
                _state.Scholars = Math.Max(_state.Scholars - amount, 0);
                break;
        }
    }

    private void AdvanceOneGameMinute()
    {
        _state.GameMinutes += 1;
        _minuteAccumulator += 1;

        if (_minuteAccumulator < MinutesPerSettlement)
        {
            return;
        }

        _minuteAccumulator = 0;
        _state.HourSettlements += 1;

        _economySystem.TickHour(_state);
        _populationSystem.TickHour(_state);

        if (_breedingSystem.TickHour(_state, out var breedingLog) && !string.IsNullOrWhiteSpace(breedingLog))
        {
            _eventBus.PublishLog(breedingLog);
        }

        if (_combatSystem.TickHour(_state, out var combatLog) && !string.IsNullOrWhiteSpace(combatLog))
        {
            _eventBus.PublishLog(combatLog);
        }

        _eventBus.PublishState(_state.Clone());
    }
}
