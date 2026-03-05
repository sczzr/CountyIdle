using System.Collections.Generic;
using Godot;
using CountyIdle.Core;
using CountyIdle.Models;

namespace CountyIdle;

public partial class Main : Control
{
    private const string MainLayoutPath = "RootMargin/MainLayout";
    private const string TopBarPath = $"{MainLayoutPath}/TopBar";
    private const string BodyRowPath = $"{MainLayoutPath}/BodyRow";
    private const string LeftPanelPath = $"{BodyRowPath}/LeftPanel";
    private const string RightPanelPath = $"{BodyRowPath}/RightPanel";
    private const string BottomBarPath = $"{MainLayoutPath}/BottomBar";
    private const double LanternPulseDurationSeconds = 0.42;
    private const double LanternResetDurationSeconds = 0.14;

    private static readonly Color BaseButtonModulate = Colors.White;
    private static readonly Color LanternGlowModulate = new(1.0f, 0.86f, 0.64f, 1.0f);

    private readonly Queue<string> _logs = new();
    private readonly SaveSystem _saveSystem = new();
    private readonly Dictionary<JobType, Label> _jobLabels = new();
    private readonly Dictionary<Button, Tween> _buttonPulseTweens = new();

    private GameLoop _gameLoop = null!;
    private Label _summaryLabel = null!;
    private Label _resourceLabel = null!;
    private Label _combatLabel = null!;
    private RichTextLabel _logLabel = null!;
    private Button _exploreButton = null!;

    public override void _Ready()
    {
        BindUiNodes();
        BindUiEvents();
        BindLanternHoverEffects();
        SetupGameLoop();
        LoadInitialState();
    }

    public override void _ExitTree()
    {
        foreach (var tween in _buttonPulseTweens.Values)
        {
            tween.Kill();
        }

        _buttonPulseTweens.Clear();
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
            $"人口 {state.Population}（精英 {state.ElitePopulation}） | 幸福 {state.Happiness:0.0} | 科技阶 T{state.TechLevel} | 空闲 {state.GetUnassignedPopulation()} | 小时结算 {state.HourSettlements}";

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

    private void BindUiNodes()
    {
        _summaryLabel = GetNode<Label>($"{TopBarPath}/BarContent/SummaryLabel");
        _resourceLabel = GetNode<Label>($"{TopBarPath}/BarContent/ResourceLabel");
        _combatLabel = GetNode<Label>($"{TopBarPath}/BarContent/CombatLabel");
        _logLabel = GetNode<RichTextLabel>($"{RightPanelPath}/PanelContent/LogLabel");
        _exploreButton = GetNode<Button>($"{BottomBarPath}/ButtonsRow/ExploreButton");

        _jobLabels[JobType.Farmer] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/FarmerRow/JobLabel");
        _jobLabels[JobType.Worker] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/WorkerRow/JobLabel");
        _jobLabels[JobType.Merchant] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/MerchantRow/JobLabel");
        _jobLabels[JobType.Scholar] = GetNode<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/ScholarRow/JobLabel");
    }

    private void BindUiEvents()
    {
        _exploreButton.Pressed += () => _gameLoop.ToggleExploration();

        GetNode<Button>($"{BottomBarPath}/ButtonsRow/SaveButton").Pressed += () =>
        {
            _saveSystem.Save(_gameLoop.State, out var msg);
            AppendLog(msg);
        };

        GetNode<Button>($"{BottomBarPath}/ButtonsRow/LoadButton").Pressed += () =>
        {
            _saveSystem.TryLoad(out var state, out var msg);
            _gameLoop.LoadState(state);
            AppendLog(msg);
        };

        GetNode<Button>($"{BottomBarPath}/ButtonsRow/ResetButton").Pressed += () => _gameLoop.ResetState();

        BindJobButtons(
            JobType.Farmer,
            $"{LeftPanelPath}/PanelContent/JobsVBox/FarmerRow/MinusButton",
            $"{LeftPanelPath}/PanelContent/JobsVBox/FarmerRow/PlusButton");
        BindJobButtons(
            JobType.Worker,
            $"{LeftPanelPath}/PanelContent/JobsVBox/WorkerRow/MinusButton",
            $"{LeftPanelPath}/PanelContent/JobsVBox/WorkerRow/PlusButton");
        BindJobButtons(
            JobType.Merchant,
            $"{LeftPanelPath}/PanelContent/JobsVBox/MerchantRow/MinusButton",
            $"{LeftPanelPath}/PanelContent/JobsVBox/MerchantRow/PlusButton");
        BindJobButtons(
            JobType.Scholar,
            $"{LeftPanelPath}/PanelContent/JobsVBox/ScholarRow/MinusButton",
            $"{LeftPanelPath}/PanelContent/JobsVBox/ScholarRow/PlusButton");
    }

    private void BindJobButtons(JobType jobType, string minusButtonPath, string plusButtonPath)
    {
        GetNode<Button>(minusButtonPath).Pressed += () => _gameLoop.AdjustJob(jobType, -5);
        GetNode<Button>(plusButtonPath).Pressed += () => _gameLoop.AdjustJob(jobType, 5);
    }

    private void BindLanternHoverEffects()
    {
        var buttons = new List<Button>();
        CollectButtons(this, buttons);

        foreach (var button in buttons)
        {
            button.SelfModulate = BaseButtonModulate;
            button.MouseEntered += () => StartLanternPulse(button);
            button.MouseExited += () => StopLanternPulse(button);
            button.FocusEntered += () => StartLanternPulse(button);
            button.FocusExited += () => StopLanternPulse(button);
        }
    }

    private static void CollectButtons(Node node, List<Button> result)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Button button)
            {
                result.Add(button);
            }

            CollectButtons(child, result);
        }
    }

    private void StartLanternPulse(Button button)
    {
        StopLanternPulse(button, smoothReset: false);

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);
        tween.SetLoops();
        tween.TweenProperty(button, "self_modulate", LanternGlowModulate, LanternPulseDurationSeconds);
        tween.TweenProperty(button, "self_modulate", BaseButtonModulate, LanternPulseDurationSeconds);

        _buttonPulseTweens[button] = tween;
    }

    private void StopLanternPulse(Button button, bool smoothReset = true)
    {
        if (_buttonPulseTweens.TryGetValue(button, out var tween))
        {
            tween.Kill();
            _buttonPulseTweens.Remove(button);
        }

        if (!smoothReset)
        {
            button.SelfModulate = BaseButtonModulate;
            return;
        }

        var resetTween = CreateTween();
        resetTween.SetTrans(Tween.TransitionType.Sine);
        resetTween.SetEase(Tween.EaseType.Out);
        resetTween.TweenProperty(button, "self_modulate", BaseButtonModulate, LanternResetDurationSeconds);
    }
}
