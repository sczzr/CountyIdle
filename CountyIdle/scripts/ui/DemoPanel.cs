using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.UI;

public partial class DemoPanel : PanelContainer
{
    private const int TargetHourSettlements = 3;
    private const int TargetExplorationCompletions = 1;
    private const int FirstBreakthroughTier = 1;
    private const double FirstBreakthroughResearch = 30.0; // 对齐 ResearchSystem 的首次突破门槛
    private const int ExplorationCycleHours = 3; // 对齐 CombatSystem 的历练结算周期
    private const int TotalGoals = 3;

    private const string RootPath = "DemoVBox";
    private const string HeaderPath = RootPath + "/DemoHeader";
    private const string TitlePath = HeaderPath + "/DemoTitle";
    private const string ProgressPath = HeaderPath + "/DemoProgress";
    private const string GoalsPath = RootPath + "/DemoGoals";

    private Label _titleLabel = null!;
    private Label _progressLabel = null!;
    private RichTextLabel _goalsLabel = null!;

    private int _lastGameMinutes = -1;
    private int _lastHourSettlements = -1;
    private int _lastExplorationProgressHours = -1;
    private int _explorationCompletions;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>(TitlePath);
        _progressLabel = GetNode<Label>(ProgressPath);
        _goalsLabel = GetNode<RichTextLabel>(GoalsPath);
        UpdateHeader(0);
    }

    public void Refresh(GameState state)
    {
        if (state == null)
        {
            return;
        }

        ResetIfNewRun(state);
        TrackExplorationCompletion(state);

        var hourGoalDone = state.HourSettlements >= TargetHourSettlements;
        var researchGoalDone = state.TechLevel >= FirstBreakthroughTier;
        var explorationGoalDone = _explorationCompletions >= TargetExplorationCompletions;

        var completed = (hourGoalDone ? 1 : 0) + (researchGoalDone ? 1 : 0) + (explorationGoalDone ? 1 : 0);
        UpdateHeader(completed);

        var hourProgress = Math.Min(state.HourSettlements, TargetHourSettlements);
        var researchProgress = Math.Clamp(state.Research, 0.0, FirstBreakthroughResearch);
        var explorationProgress = Math.Clamp(state.ExplorationProgressHours, 0, ExplorationCycleHours);

        var hourText = $"完成 {TargetHourSettlements} 次时辰结算（{hourProgress}/{TargetHourSettlements}）";
        var researchText = researchGoalDone
            ? "完成一次研修突破"
            : $"完成一次研修突破（进度 {researchProgress:0.#}/{FirstBreakthroughResearch:0}）";
        var explorationText = state.ExplorationEnabled
            ? $"完成 {TargetExplorationCompletions} 次历练结算（{Math.Min(_explorationCompletions, TargetExplorationCompletions)}/{TargetExplorationCompletions}，进度 {explorationProgress}/{ExplorationCycleHours}）"
            : $"完成 {TargetExplorationCompletions} 次历练结算（历练已暂停）";

        _goalsLabel.Text = string.Join("\n", new[]
        {
            BuildGoalLine(hourGoalDone, hourText),
            BuildGoalLine(researchGoalDone, researchText),
            BuildGoalLine(explorationGoalDone, explorationText)
        });
    }

    private void UpdateHeader(int completed)
    {
        _titleLabel.Text = "试玩目标";
        _progressLabel.Text = $"{completed}/{TotalGoals}";
    }

    private void ResetIfNewRun(GameState state)
    {
        if ((_lastGameMinutes >= 0 && state.GameMinutes < _lastGameMinutes) ||
            (_lastHourSettlements >= 0 && state.HourSettlements < _lastHourSettlements))
        {
            _explorationCompletions = 0;
            _lastExplorationProgressHours = -1;
        }

        _lastGameMinutes = state.GameMinutes;
        _lastHourSettlements = state.HourSettlements;
    }

    private void TrackExplorationCompletion(GameState state)
    {
        if (_lastExplorationProgressHours > 0 &&
            state.ExplorationProgressHours == 0 &&
            state.ExplorationEnabled)
        {
            _explorationCompletions += 1;
        }

        _lastExplorationProgressHours = state.ExplorationProgressHours;
    }

    private static string BuildGoalLine(bool done, string text)
    {
        var marker = done ? "●" : "○";
        var color = done ? "#8a6a3b" : "#6b5f54";
        return $"[color={color}]{marker} {text}[/color]";
    }
}
