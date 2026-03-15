using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.UI;

public partial class CultivationPanel : PopupPanelBase
{
    private enum CultivationAction
    {
        SkillTraining,
        TechniquePolish,
        CraftPractice,
        Meditation
    }

    private sealed class ActionCard
    {
        public ActionCard(Button button, Label statusLabel, string actionLabel, string inactiveText, string activeText, string inactiveButtonText)
        {
            Button = button;
            StatusLabel = statusLabel;
            ActionLabel = actionLabel;
            InactiveText = inactiveText;
            ActiveText = activeText;
            InactiveButtonText = inactiveButtonText;
        }

        public Button Button { get; }
        public Label StatusLabel { get; }
        public string ActionLabel { get; }
        public string InactiveText { get; }
        public string ActiveText { get; }
        public string InactiveButtonText { get; }
    }

    private Label _populationValueLabel = null!;
    private Label _techValueLabel = null!;
    private Label _resourceValueLabel = null!;
    private Button _closeButton = null!;
    private Node? _visualFx;

    private readonly Dictionary<CultivationAction, ActionCard> _actionCards = new();
    private readonly HashSet<CultivationAction> _activeActions = new();

    public override void _Ready()
    {
        _populationValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/PopulationCard/CardMargin/CardColumn/ValueLabel");
        _techValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/TechCard/CardMargin/CardColumn/ValueLabel");
        _resourceValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/SummaryRow/ResourceCard/CardMargin/CardColumn/ValueLabel");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _visualFx = GetNodeOrNull<Node>("VisualFx");

        InitializePopupHint("CenterLayer/Dialog/Margin/MainColumn/HintLabel");
        BuildActionCards();
        BindEvents();
        Hide();
    }

    public void Open(GameState state)
    {
        RefreshState(state);
        OpenPopup();
        CallVisualFx("play_open");
    }

    public void ClosePanel()
    {
        ClosePopup();
    }

    public void RefreshState(GameState state)
    {
        _populationValueLabel.Text = state.Population.ToString("0");
        _techValueLabel.Text = $"T{Math.Max(state.TechLevel + 1, 1)}";
        _resourceValueLabel.Text = $"灵石 {state.Gold:0}\n贡献 {state.ContributionPoints:0}";
        RefreshActionCards();
    }

    public override void _Process(double delta)
    {
        TickPopupStatus(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible)
        {
            return;
        }

        if (!TryHandlePopupClose(@event))
        {
            return;
        }

        GetViewport().SetInputAsHandled();
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return "本卷用于登记弟子修炼安排，当前仅作记录，尚未接入时辰结算。";
    }

    private void BindEvents()
    {
        _closeButton.Pressed += ClosePanel;
    }

    private void BuildActionCards()
    {
        _actionCards.Clear();

        RegisterAction(
            CultivationAction.SkillTraining,
            "CenterLayer/Dialog/Margin/MainColumn/ActionSection/SkillTrainingCard/CardMargin/CardRow/ActionButton",
            "CenterLayer/Dialog/Margin/MainColumn/ActionSection/SkillTrainingCard/CardMargin/CardRow/InfoColumn/StatusLabel",
            "技能修炼",
            "当前：未安排",
            "当前：技能修炼中（待接入结算）",
            "安排修炼");

        RegisterAction(
            CultivationAction.TechniquePolish,
            "CenterLayer/Dialog/Margin/MainColumn/ActionSection/TechniquePolishCard/CardMargin/CardRow/ActionButton",
            "CenterLayer/Dialog/Margin/MainColumn/ActionSection/TechniquePolishCard/CardMargin/CardRow/InfoColumn/StatusLabel",
            "功法打磨",
            "当前：未安排",
            "当前：功法打磨中（待接入结算）",
            "安排打磨");

        RegisterAction(
            CultivationAction.CraftPractice,
            "CenterLayer/Dialog/Margin/MainColumn/ActionSection/CraftPracticeCard/CardMargin/CardRow/ActionButton",
            "CenterLayer/Dialog/Margin/MainColumn/ActionSection/CraftPracticeCard/CardMargin/CardRow/InfoColumn/StatusLabel",
            "技艺练习",
            "当前：未安排",
            "当前：技艺练习中（待接入结算）",
            "安排练习");

        RegisterAction(
            CultivationAction.Meditation,
            "CenterLayer/Dialog/Margin/MainColumn/ActionSection/MeditationCard/CardMargin/CardRow/ActionButton",
            "CenterLayer/Dialog/Margin/MainColumn/ActionSection/MeditationCard/CardMargin/CardRow/InfoColumn/StatusLabel",
            "打坐修炼",
            "当前：未安排",
            "当前：静修中（待接入结算）",
            "安排静修");

        foreach (var pair in _actionCards)
        {
            var action = pair.Key;
            pair.Value.Button.Pressed += () => ToggleAction(action);
        }
    }

    private void RegisterAction(
        CultivationAction action,
        string buttonPath,
        string statusLabelPath,
        string actionLabel,
        string inactiveText,
        string activeText,
        string inactiveButtonText)
    {
        var button = GetNode<Button>(buttonPath);
        var statusLabel = GetNode<Label>(statusLabelPath);
        _actionCards[action] = new ActionCard(button, statusLabel, actionLabel, inactiveText, activeText, inactiveButtonText);
    }

    private void ToggleAction(CultivationAction action)
    {
        if (_activeActions.Contains(action))
        {
            _activeActions.Remove(action);
            ShowPopupStatusMessage($"已撤销{_actionCards[action].ActionLabel}安排。");
        }
        else
        {
            _activeActions.Add(action);
            ShowPopupStatusMessage($"已登记{_actionCards[action].ActionLabel}安排（待接入结算）。");
        }

        UpdateActionCard(action);
    }

    private void RefreshActionCards()
    {
        foreach (var action in _actionCards.Keys)
        {
            UpdateActionCard(action);
        }
    }

    private void UpdateActionCard(CultivationAction action)
    {
        if (!_actionCards.TryGetValue(action, out var card))
        {
            return;
        }

        var isActive = _activeActions.Contains(action);
        card.StatusLabel.Text = isActive ? card.ActiveText : card.InactiveText;
        card.Button.Text = isActive ? "撤销安排" : card.InactiveButtonText;
    }

    private void CallVisualFx(string methodName, params Variant[] args)
    {
        _visualFx?.Call(methodName, args);
    }
}
