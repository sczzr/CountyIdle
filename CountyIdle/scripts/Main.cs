using System.Collections.Generic;
using Godot;
using CountyIdle.Core;
using CountyIdle.Models;

namespace CountyIdle;

public partial class Main : Control
{
    private readonly Queue<string> _logs = new();
    private readonly SaveSystem _saveSystem = new();
    private readonly Dictionary<JobType, Label> _jobLabels = new();

    private GameLoop _gameLoop = null!;
    private Label _summaryLabel = null!;
    private Label _resourceLabel = null!;
    private Label _combatLabel = null!;
    private Label _statusLabel = null!;
    private RichTextLabel _logLabel = null!;
    private Button _exploreButton = null!;

    public override void _Ready()
    {
        BuildUi();
        SetupGameLoop();
        LoadInitialState();
    }

    private void SetupGameLoop()
    {
        _gameLoop = new GameLoop();
        AddChild(_gameLoop);
        _gameLoop.Events.StateChanged += OnStateChanged;
        _gameLoop.Events.LogAdded += AppendLog;
    }

    private void LoadInitialState()
    {
        if (_saveSystem.TryLoad(out var state, out var msg))
        {
            _gameLoop.LoadState(state);
            AppendLog(msg);
            return;
        }

        _gameLoop.ResetState();
        AppendLog(msg);
    }

    private void OnStateChanged(GameState state)
    {
        _summaryLabel.Text =
            $"人口 {state.Population}（精英 {state.ElitePopulation}） | 幸福 {state.Happiness:0.0} | 空闲 {state.GetUnassignedPopulation()} | 小时结算 {state.HourSettlements}";

        _resourceLabel.Text =
            $"粮食 {state.Food:0}  木材 {state.Wood:0}  石料 {state.Stone:0}  金币 {state.Gold:0}  科研 {state.Research:0}  稀有 {state.RareMaterial:0}";

        _combatLabel.Text =
            $"探险层数 {state.ExplorationDepth} | 威胁 {state.Threat:0.0} | 装备评分 {state.AvgGearScore:0.0}";

        _exploreButton.Text = state.ExplorationEnabled ? "暂停探险" : "恢复探险";

        RefreshJobLabels(state);
    }

    private void RefreshJobLabels(GameState state)
    {
        _jobLabels[JobType.Farmer].Text = $"农：{state.Farmers}";
        _jobLabels[JobType.Worker].Text = $"工：{state.Workers}";
        _jobLabels[JobType.Merchant].Text = $"商：{state.Merchants}";
        _jobLabels[JobType.Scholar].Text = $"士：{state.Scholars}";
    }

    private void AppendLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _logs.Enqueue($"[T{_gameLoop?.State.GameMinutes ?? 0:00000}] {message}");
        while (_logs.Count > 12)
        {
            _logs.Dequeue();
        }

        _logLabel.Text = string.Join("\n", _logs);
    }

    private void BuildUi()
    {
        var root = new MarginContainer
        {
            AnchorsPreset = (int)LayoutPreset.FullRect,
            OffsetLeft = 16,
            OffsetTop = 12,
            OffsetRight = -16,
            OffsetBottom = -12
        };
        AddChild(root);

        var column = new VBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        root.AddChild(column);

        var title = new Label { Text = "郡守挂机原型（Godot C#）", HorizontalAlignment = HorizontalAlignment.Center };
        column.AddChild(title);

        _summaryLabel = new Label();
        _resourceLabel = new Label();
        _combatLabel = new Label();
        column.AddChild(_summaryLabel);
        column.AddChild(_resourceLabel);
        column.AddChild(_combatLabel);

        var jobsTitle = new Label { Text = "职业分工（每次 ±5）" };
        column.AddChild(jobsTitle);

        AddJobRow(column, JobType.Farmer);
        AddJobRow(column, JobType.Worker);
        AddJobRow(column, JobType.Merchant);
        AddJobRow(column, JobType.Scholar);

        var actionRow = new HBoxContainer();
        column.AddChild(actionRow);

        _exploreButton = new Button { Text = "暂停探险", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _exploreButton.Pressed += () => _gameLoop.ToggleExploration();
        actionRow.AddChild(_exploreButton);

        var saveButton = new Button { Text = "存档", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        saveButton.Pressed += () =>
        {
            _saveSystem.Save(_gameLoop.State, out var msg);
            AppendLog(msg);
        };
        actionRow.AddChild(saveButton);

        var loadButton = new Button { Text = "读档", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        loadButton.Pressed += () =>
        {
            _saveSystem.TryLoad(out var state, out var msg);
            _gameLoop.LoadState(state);
            AppendLog(msg);
        };
        actionRow.AddChild(loadButton);

        var resetButton = new Button { Text = "重开", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        resetButton.Pressed += () => _gameLoop.ResetState();
        actionRow.AddChild(resetButton);

        _statusLabel = new Label
        {
            Text = "规则：1秒=1游戏分钟，每60分钟结算一次资源/人口/探险。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        column.AddChild(_statusLabel);

        _logLabel = new RichTextLabel
        {
            FitContent = true,
            ScrollActive = false,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        column.AddChild(_logLabel);
    }

    private void AddJobRow(VBoxContainer parent, JobType jobType)
    {
        var row = new HBoxContainer();
        parent.AddChild(row);

        var label = new Label
        {
            Text = $"{JobToShortName(jobType)}：0",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _jobLabels[jobType] = label;
        row.AddChild(label);

        var minus = new Button { Text = "-5" };
        minus.Pressed += () => _gameLoop.AdjustJob(jobType, -5);
        row.AddChild(minus);

        var plus = new Button { Text = "+5" };
        plus.Pressed += () => _gameLoop.AdjustJob(jobType, 5);
        row.AddChild(plus);
    }

    private static string JobToShortName(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "农",
            JobType.Worker => "工",
            JobType.Merchant => "商",
            JobType.Scholar => "士",
            _ => "?"
        };
    }
}
