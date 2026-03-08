using System;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.Core;

public partial class GameLoop : Node
{
    private const double BaseRealSecondsPerGameMinute = 1.0;
    private const double MinTimeScale = 1.0;
    private const double MaxTimeScale = 4.0;
    private const int MinutesPerSettlement = 60;

    private readonly PopulationSystem _populationSystem = new();
    private readonly IndustrySystem _industrySystem = new();
    private readonly ResourceSystem _resourceSystem = new();
    private readonly EconomySystem _economySystem = new();
    private readonly MapOperationalLinkSystem _mapOperationalLinkSystem = new();
    private readonly ResearchSystem _researchSystem = new();
    private readonly BreedingSystem _breedingSystem = new();
    private readonly CombatSystem _combatSystem = new();
    private readonly CountyEventSystem _countyEventSystem = new();
    private readonly EventBus _eventBus = new();

    private double _secondAccumulator;
    private int _minuteAccumulator;
    private double _timeScale = MinTimeScale;
    private GameState _state = new();

    public GameState State => _state;
    public EventBus Events => _eventBus;

    public override void _Process(double delta)
    {
        _secondAccumulator += delta;
        var secondsPerGameMinute = BaseRealSecondsPerGameMinute / _timeScale;

        while (_secondAccumulator >= secondsPerGameMinute)
        {
            _secondAccumulator -= secondsPerGameMinute;
            AdvanceOneGameMinute();
        }
    }

    public void SetTimeScale(double scale)
    {
        _timeScale = Math.Clamp(scale, MinTimeScale, MaxTimeScale);
    }

    public void LoadState(GameState state)
    {
        _state = state ?? new GameState();
        InventoryRules.EndTransaction(_state);
        IndustryRules.EnsureDefaults(_state);
        PopulationRules.EnsureDefaults(_state);
        MaterialRules.EnsureDefaults(_state);
        _minuteAccumulator = Math.Max(_state.GameMinutes % MinutesPerSettlement, 0);
        _secondAccumulator = 0;
        ClampJobsToIndustryCapacity(publishLogs: false);
        _eventBus.PublishState(_state.Clone());
    }

    public void ResetState()
    {
        _state = new GameState();
        InventoryRules.EndTransaction(_state);
        PopulationRules.EnsureDefaults(_state);
        MaterialRules.EnsureDefaults(_state);
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

    public void BuildIndustryBuilding(IndustryBuildingType buildingType)
    {
        if (_industrySystem.TryConstructBuilding(_state, buildingType, out var log))
        {
            ClampJobsToIndustryCapacity(publishLogs: false);
            _eventBus.PublishLog(log);
            _eventBus.PublishState(_state.Clone());
            return;
        }

        _eventBus.PublishLog(log);
    }

    public void CraftIndustryTools()
    {
        if (_industrySystem.TryCraftTools(_state, out var log))
        {
            _eventBus.PublishLog(log);
            _eventBus.PublishState(_state.Clone());
            return;
        }

        _eventBus.PublishLog(log);
    }

    public void UpgradeMineAndWarehouse()
    {
        if (_industrySystem.TryUpgradeMineAndWarehouse(_state, out var log))
        {
            _eventBus.PublishLog(log);
            _eventBus.PublishState(_state.Clone());
            return;
        }

        _eventBus.PublishLog(log);
    }

    public void BuildTierZeroChain(TierZeroMaterialChainType chainType)
    {
        if (_industrySystem.TryBuildTierZeroChain(_state, chainType, out var log))
        {
            _eventBus.PublishLog(log);
            _eventBus.PublishState(_state.Clone());
            return;
        }

        _eventBus.PublishLog(log);
    }

    public void ExecuteMapDirective(MapDirectiveAction directiveAction)
    {
        if (_mapOperationalLinkSystem.TryExecuteDirective(_state, directiveAction, out var log))
        {
            PopulationRules.EnsureDefaults(_state);
            _eventBus.PublishLog(log);
            _eventBus.PublishState(_state.Clone());
            return;
        }

        _eventBus.PublishLog(log);
    }

    public void AdjustJob(JobType jobType, int delta)
    {
        if (delta == 0)
        {
            return;
        }

        IndustryRules.EnsureDefaults(_state);

        var currentAssigned = IndustryRules.GetAssigned(_state, jobType);
        var targetAssigned = currentAssigned;

        if (delta > 0)
        {
            var availablePopulation = _state.GetUnassignedPopulation();
            if (availablePopulation <= 0)
            {
                _eventBus.PublishLog("空闲人口不足，无法继续分配。");
                return;
            }

            var capacity = IndustryRules.GetCapacity(_state, jobType);
            var allowedByCapacity = Math.Max(capacity - currentAssigned, 0);
            var actualIncrease = Math.Min(delta, Math.Min(availablePopulation, allowedByCapacity));

            if (actualIncrease <= 0)
            {
                _eventBus.PublishLog($"{GetJobDisplayName(jobType)}受岗位容量限制，请先扩建相关建筑或补充工具。");
                return;
            }

            targetAssigned += actualIncrease;
        }
        else
        {
            targetAssigned = Math.Max(currentAssigned + delta, 0);
        }

        IndustryRules.SetAssigned(_state, jobType, targetAssigned);

        var overAssigned = _state.GetAssignedPopulation() - _state.Population;
        if (overAssigned > 0)
        {
            RemoveFromJob(jobType, overAssigned);
        }

        _eventBus.PublishState(_state.Clone());
    }

    private void RemoveFromJob(JobType jobType, int amount)
    {
        var currentAssigned = IndustryRules.GetAssigned(_state, jobType);
        IndustryRules.SetAssigned(_state, jobType, Math.Max(currentAssigned - amount, 0));
    }

    private void ClampJobsToIndustryCapacity(bool publishLogs)
    {
        ClampJobToIndustryCapacity(JobType.Worker, publishLogs);
        ClampJobToIndustryCapacity(JobType.Scholar, publishLogs);
        ClampJobToIndustryCapacity(JobType.Merchant, publishLogs);
        ClampJobToIndustryCapacity(JobType.Farmer, publishLogs);
    }

    private void ClampJobToIndustryCapacity(JobType jobType, bool publishLogs)
    {
        var capacity = IndustryRules.GetCapacity(_state, jobType);
        var assigned = IndustryRules.GetAssigned(_state, jobType);
        if (assigned <= capacity)
        {
            return;
        }

        IndustryRules.SetAssigned(_state, jobType, capacity);
        if (publishLogs)
        {
            _eventBus.PublishLog($"{GetJobDisplayName(jobType)}已回退至岗位容量 {capacity}。");
        }
    }

    private string GetJobDisplayName(JobType jobType)
    {
        return JobProgressionRules.GetActiveRoleName(_state, jobType);
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

        if (_industrySystem.TickHour(_state, out var industryLog) && !string.IsNullOrWhiteSpace(industryLog))
        {
            _eventBus.PublishLog(industryLog);
        }

        if (_resourceSystem.TickHour(_state, out var resourceLog) && !string.IsNullOrWhiteSpace(resourceLog))
        {
            _eventBus.PublishLog(resourceLog);
        }

        _economySystem.TickHour(_state);

        if (_researchSystem.TickHour(_state, out var researchLog) && !string.IsNullOrWhiteSpace(researchLog))
        {
            _eventBus.PublishLog(researchLog);
        }

        if (_populationSystem.TickHour(_state, out var populationLog) && !string.IsNullOrWhiteSpace(populationLog))
        {
            _eventBus.PublishLog(populationLog);
        }

        if (_breedingSystem.TickHour(_state, out var breedingLog) && !string.IsNullOrWhiteSpace(breedingLog))
        {
            _eventBus.PublishLog(breedingLog);
        }

        if (_combatSystem.TickHour(_state, out var combatLog) && !string.IsNullOrWhiteSpace(combatLog))
        {
            _eventBus.PublishLog(combatLog);
        }

        if (_countyEventSystem.TickHour(_state, out var eventLog) && !string.IsNullOrWhiteSpace(eventLog))
        {
            _eventBus.PublishLog(eventLog);
        }

        _eventBus.PublishState(_state.Clone());
    }
}
