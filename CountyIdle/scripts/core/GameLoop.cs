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

    private readonly GameCalendarSystem _gameCalendarSystem = new();
    private readonly PopulationSystem _populationSystem = new();
    private readonly IndustrySystem _industrySystem = new();
    private readonly ResourceSystem _resourceSystem = new();
    private readonly EconomySystem _economySystem = new();
    private readonly SectTaskSystem _sectTaskSystem = new();
    private readonly SectGovernanceSystem _sectGovernanceSystem = new();
    private readonly SectRuleTreeSystem _sectRuleTreeSystem = new();
    private readonly SectPeakSupportSystem _sectPeakSupportSystem = new();
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
        _sectGovernanceSystem.EnsureDefaults(_state);
        _sectRuleTreeSystem.EnsureDefaults(_state);
        _sectPeakSupportSystem.EnsureDefaults(_state);
        ValidateQuarterDecree(false);
        _minuteAccumulator = Math.Max(_state.GameMinutes % MinutesPerSettlement, 0);
        _secondAccumulator = 0;
        SyncTaskOrders();
        _eventBus.PublishState(_state.Clone());
    }

    public void ResetState()
    {
        _state = new GameState();
        InventoryRules.EndTransaction(_state);
        PopulationRules.EnsureDefaults(_state);
        MaterialRules.EnsureDefaults(_state);
        _sectGovernanceSystem.EnsureDefaults(_state);
        _sectRuleTreeSystem.EnsureDefaults(_state);
        _sectPeakSupportSystem.EnsureDefaults(_state);
        ValidateQuarterDecree(false);
        _minuteAccumulator = 0;
        _secondAccumulator = 0;
        SyncTaskOrders();
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
            SyncTaskOrders();
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
            SyncTaskOrders();
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
            SyncTaskOrders();
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
            SyncTaskOrders();
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
            SyncTaskOrders();
            _eventBus.PublishLog(log);
            _eventBus.PublishState(_state.Clone());
            return;
        }

        _eventBus.PublishLog(log);
    }

    public void AdjustJob(JobType jobType, int delta)
    {
        AdjustTaskOrder(SectTaskRules.GetPrimaryTaskForJob(jobType), delta);
    }

    public void AdjustTaskOrder(SectTaskType taskType, int delta)
    {
        if (!_sectTaskSystem.AdjustOrder(_state, taskType, delta, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        SyncTaskOrders();
        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ResetTaskOrders()
    {
        if (!_sectTaskSystem.ResetOrders(_state, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        SyncTaskOrders();
        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ShiftDevelopmentDirection(int delta)
    {
        if (!_sectGovernanceSystem.ShiftDevelopmentDirection(_state, delta, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        SyncTaskOrders();
        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ShiftSectLaw(int delta)
    {
        if (!_sectGovernanceSystem.ShiftLaw(_state, delta, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        SyncTaskOrders();
        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ShiftTalentPlan(int delta)
    {
        if (!_sectGovernanceSystem.ShiftTalentPlan(_state, delta, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        SyncTaskOrders();
        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ResetGovernance()
    {
        if (!_sectGovernanceSystem.ResetGovernance(_state, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        _sectRuleTreeSystem.ResetRules(_state);
        SyncTaskOrders();
        _eventBus.PublishLog($"{log} 门规纲目已恢复常制。");
        _eventBus.PublishState(_state.Clone());
    }

    public void ShiftQuarterDecree(int delta)
    {
        var currentQuarterIndex = _gameCalendarSystem.GetQuarterIndex(_state.GameMinutes);
        if (!_sectGovernanceSystem.ShiftQuarterDecree(_state, currentQuarterIndex, out var log, delta))
        {
            _eventBus.PublishLog(log);
            return;
        }

        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ShiftAffairsRule(int delta)
    {
        if (!_sectRuleTreeSystem.ShiftAffairsRule(_state, delta, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ShiftDoctrineRule(int delta)
    {
        if (!_sectRuleTreeSystem.ShiftDoctrineRule(_state, delta, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ShiftDisciplineRule(int delta)
    {
        if (!_sectRuleTreeSystem.ShiftDisciplineRule(_state, delta, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void SetPeakSupport(SectPeakSupportType supportType)
    {
        if (!_sectPeakSupportSystem.SetPeakSupport(_state, supportType, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    public void ResetPeakSupport()
    {
        if (!_sectPeakSupportSystem.ResetPeakSupport(_state, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        _eventBus.PublishLog(log);
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
        var previousQuarterIndex = _gameCalendarSystem.GetQuarterIndex(_state.GameMinutes);
        _state.GameMinutes += 1;
        var currentQuarterIndex = _gameCalendarSystem.GetQuarterIndex(_state.GameMinutes);
        if (currentQuarterIndex != previousQuarterIndex)
        {
            ValidateQuarterDecree(true);
        }

        _minuteAccumulator += 1;

        if (_minuteAccumulator < MinutesPerSettlement)
        {
            return;
        }

        _minuteAccumulator = 0;
        _state.HourSettlements += 1;
        SyncTaskOrders();

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

        SyncTaskOrders();
        _eventBus.PublishState(_state.Clone());
    }

    private void SyncTaskOrders()
    {
        _sectGovernanceSystem.EnsureDefaults(_state);
        _sectRuleTreeSystem.EnsureDefaults(_state);
        _sectTaskSystem.EnsureDefaults(_state);
    }

    private void ValidateQuarterDecree(bool publishLog)
    {
        var currentQuarterIndex = _gameCalendarSystem.GetQuarterIndex(_state.GameMinutes);
        if (!_sectGovernanceSystem.HandleQuarterTransition(_state, currentQuarterIndex, out var log))
        {
            return;
        }

        if (publishLog && !string.IsNullOrWhiteSpace(log))
        {
            _eventBus.PublishLog(log);
        }
    }
}
