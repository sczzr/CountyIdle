using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.Core;

/// <summary>
/// 主循环：推进时间、触发小时结算，并协调各系统的 Tick 与日志广播。
/// </summary>
public partial class GameLoop : Node
{
    // 时间口径：1 秒现实时间 = 1 游戏分钟（再由倍率缩放）
    private const double BaseRealSecondsPerGameMinute = 1.0;
    // 倍速边界（与 UI 口径保持一致）
    private const double MinTimeScale = 1.0;
    private const double MaxTimeScale = 4.0;
    // 每 60 游戏分钟结算一次小时
    private const int MinutesPerSettlement = 60;

    // 玩法系统：按小时或条件结算
    private readonly GameCalendarSystem _gameCalendarSystem = new();
    private readonly PopulationSystem _populationSystem = new();
    private readonly IndustrySystem _industrySystem = new();
    private readonly ResourceSystem _resourceSystem = new();
    private readonly EconomySystem _economySystem = new();
    private readonly SectTaskSystem _sectTaskSystem = new();
    private readonly SectGovernanceSystem _sectGovernanceSystem = new();
    private readonly SectRuleTreeSystem _sectRuleTreeSystem = new();
    private readonly SectPeakSupportSystem _sectPeakSupportSystem = new();
    private readonly DiscipleDirectiveSystem _discipleDirectiveSystem = new();
    private readonly MapOperationalLinkSystem _mapOperationalLinkSystem = new();
    private readonly ResearchSystem _researchSystem = new();
    private readonly BreedingSystem _breedingSystem = new();
    private readonly CombatSystem _combatSystem = new();
    private readonly CountyEventSystem _countyEventSystem = new();
    private readonly EventBus _eventBus = new();

    // 累计器：将真实时间换算为游戏分钟
    private double _secondAccumulator;
    private int _minuteAccumulator;
    private double _timeScale = MinTimeScale;
    private GameState _state = new();

    // 对外只读状态与事件总线
    public GameState State => _state;
    public EventBus Events => _eventBus;

    /// <summary>
    /// Godot 每帧调用：按真实时间推进游戏分钟。
    /// </summary>
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

    /// <summary>
    /// 设置时间倍率（会被夹在最小/最大范围）。
    /// </summary>
    public void SetTimeScale(double scale)
    {
        _timeScale = Math.Clamp(scale, MinTimeScale, MaxTimeScale);
    }

    /// <summary>
    /// 载入存档状态，并补齐缺省字段与规则。
    /// </summary>
    public void LoadState(GameState state)
    {
        _state = state ?? new GameState();
        InventoryRules.EndTransaction(_state);
        DiscipleBackpackRules.EnsureDefaults(_state);
        WorkshopCraftedInventoryRules.EnsureDefaults(_state);
        SectNamingRules.EnsureDefaults(_state);
        IndustryRules.EnsureDefaults(_state);
        PopulationRules.EnsureDefaults(_state);
        MaterialRules.EnsureDefaults(_state);
        _sectGovernanceSystem.EnsureDefaults(_state);
        _sectRuleTreeSystem.EnsureDefaults(_state);
        _sectPeakSupportSystem.EnsureDefaults(_state);
        _discipleDirectiveSystem.EnsureDefaults(_state);
        ValidateQuarterDecree(false);
        _minuteAccumulator = Math.Max(_state.GameMinutes % MinutesPerSettlement, 0);
        _secondAccumulator = 0;
        SyncTaskOrders();
        _eventBus.PublishState(_state.Clone());
    }

    /// <summary>
    /// 彻底重置为新档案状态。
    /// </summary>
    public void ResetState()
    {
        _state = new GameState();
        InventoryRules.EndTransaction(_state);
        DiscipleBackpackRules.EnsureDefaults(_state);
        WorkshopCraftedInventoryRules.EnsureDefaults(_state);
        SectNamingRules.EnsureDefaults(_state);
        PopulationRules.EnsureDefaults(_state);
        MaterialRules.EnsureDefaults(_state);
        _sectGovernanceSystem.EnsureDefaults(_state);
        _sectRuleTreeSystem.EnsureDefaults(_state);
        _sectPeakSupportSystem.EnsureDefaults(_state);
        _discipleDirectiveSystem.EnsureDefaults(_state);
        ValidateQuarterDecree(false);
        _minuteAccumulator = 0;
        _secondAccumulator = 0;
        SyncTaskOrders();
        _eventBus.PublishLog("已重置到初始状态。");
        _eventBus.PublishState(_state.Clone());
    }

    /// <summary>
    /// 探险开关（仅切换标志位）。
    /// </summary>
    public void ToggleExploration()
    {
        _state.ExplorationEnabled = !_state.ExplorationEnabled;
        _eventBus.PublishLog(_state.ExplorationEnabled ? "已开启森林探险。" : "已暂停森林探险。");
        _eventBus.PublishState(_state.Clone());
    }

    /// <summary>
    /// 建造产业建筑。
    /// </summary>
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

    /// <summary>
    /// 工具打造（产业链产出）。
    /// </summary>
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

    /// <summary>
    /// 矿场与仓库升级。
    /// </summary>
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

    /// <summary>
    /// T0 产业链扩建。
    /// </summary>
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

    /// <summary>
    /// 外务行囊交回宗库。
    /// </summary>
    public void StashDiscipleBackpack()
    {
        if (!DiscipleBackpackRules.TryTransferToWarehouse(_state, out var log))
        {
            _eventBus.PublishLog("外务行囊暂无可交回物资。");
            return;
        }

        _eventBus.PublishLog(log!);
        _eventBus.PublishState(_state.Clone());
    }

    /// <summary>
    /// 工坊成品入库。
    /// </summary>
    public void StashWorkshopCrafted()
    {
        if (!WorkshopCraftedInventoryRules.TryTransferToWarehouse(_state, out var log))
        {
            _eventBus.PublishLog("工坊成品暂无可入库物资。");
            return;
        }

        _eventBus.PublishLog(log!);
        _eventBus.PublishState(_state.Clone());
    }

    /// <summary>
    /// 执行地图调度指令（世界/外域/山门地图）。
    /// </summary>
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

    /// <summary>
    /// 旧岗位接口入口：映射为宗主中枢任务调度。
    /// </summary>
    public void AdjustJob(JobType jobType, int delta)
    {
        AdjustTaskOrder(SectTaskRules.GetPrimaryTaskForJob(jobType), delta);
    }

    /// <summary>
    /// 调整宗主中枢任务权重。
    /// </summary>
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

    /// <summary>
    /// 重置所有任务权重为默认。
    /// </summary>
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

    /// <summary>
    /// 调整发展方向（治理中枢）。
    /// </summary>
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

    /// <summary>
    /// 调整法令权重。
    /// </summary>
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

    /// <summary>
    /// 调整育才方向。
    /// </summary>
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

    /// <summary>
    /// 重置治理配置并恢复门规为常制。
    /// </summary>
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

    /// <summary>
    /// 调整季度法令。
    /// </summary>
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

    /// <summary>
    /// 调整庶务门规。
    /// </summary>
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

    /// <summary>
    /// 调整传功门规。
    /// </summary>
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

    /// <summary>
    /// 调整巡山门规。
    /// </summary>
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

    /// <summary>
    /// 设置峰脉协同支持。
    /// </summary>
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

    /// <summary>
    /// 重置峰脉协同。
    /// </summary>
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

    /// <summary>
    /// 更新宗门命名（显示名映射）。
    /// </summary>
    public void UpdateSectNames(IReadOnlyDictionary<string, string> nameMap)
    {
        if (nameMap == null)
        {
            return;
        }

        SectNamingRules.ApplyNames(_state, nameMap);
        _eventBus.PublishLog("宗门命名已更新。");
        _eventBus.PublishState(_state.Clone());
    }

    /// <summary>
    /// 设置弟子指令（执事/候补/观察）。
    /// </summary>
    public void SetDiscipleDirective(int discipleId, DiscipleDirectiveType directiveType)
    {
        if (!_discipleDirectiveSystem.SetDirective(_state, discipleId, directiveType, out var log))
        {
            _eventBus.PublishLog(log);
            return;
        }

        SyncTaskOrders();
        _eventBus.PublishLog(log);
        _eventBus.PublishState(_state.Clone());
    }

    /// <summary>
    /// 将岗位人数回退指定数量（保护不为负）。
    /// </summary>
    private void RemoveFromJob(JobType jobType, int amount)
    {
        var currentAssigned = IndustryRules.GetAssigned(_state, jobType);
        IndustryRules.SetAssigned(_state, jobType, Math.Max(currentAssigned - amount, 0));
    }

    /// <summary>
    /// 将所有岗位人数限制在产业容量内。
    /// </summary>
    private void ClampJobsToIndustryCapacity(bool publishLogs)
    {
        ClampJobToIndustryCapacity(JobType.Worker, publishLogs);
        ClampJobToIndustryCapacity(JobType.Scholar, publishLogs);
        ClampJobToIndustryCapacity(JobType.Merchant, publishLogs);
        ClampJobToIndustryCapacity(JobType.Farmer, publishLogs);
    }

    /// <summary>
    /// 单一岗位的容量回退逻辑。
    /// </summary>
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

    /// <summary>
    /// 获取岗位显示名（受当前语义规则影响）。
    /// </summary>
    private string GetJobDisplayName(JobType jobType)
    {
        return JobProgressionRules.GetActiveRoleName(_state, jobType);
    }

    /// <summary>
    /// 推进 1 游戏分钟，并在满 60 分钟时触发小时结算。
    /// </summary>
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

        // 小时结算顺序：产业 → 资源 → 经济 → 研修 → 人口 → 繁育 → 战斗 → 事件
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

    /// <summary>
    /// 确保治理/门规/任务/指令系统的默认值齐备。
    /// </summary>
    private void SyncTaskOrders()
    {
        _sectGovernanceSystem.EnsureDefaults(_state);
        _sectRuleTreeSystem.EnsureDefaults(_state);
        _sectTaskSystem.EnsureDefaults(_state);
        _discipleDirectiveSystem.EnsureDefaults(_state);
    }

    /// <summary>
    /// 季度切换时校验并刷新法令。
    /// </summary>
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
