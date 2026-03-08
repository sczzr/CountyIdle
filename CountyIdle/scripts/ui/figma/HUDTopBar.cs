using Godot;

namespace CountyIdle.UI.Figma;

public partial class HUDTopBar : PanelContainer
{
    private Label _populationLabel = null!;
    private Label _moraleLabel = null!;
    private Label _foodLabel = null!;
    private Label _stoneLabel = null!;
    private Label _fireLabel = null!;
    private Label _moneyLabel = null!;
    private Label _qiLabel = null!;
    private Label _eraLabel = null!;

    public override void _Ready()
    {
        _populationLabel = GetNode<Label>("BarPadding/MainRow/ResourceRow/PopulationLabel");
        _moraleLabel = GetNode<Label>("BarPadding/MainRow/ResourceRow/MoraleLabel");
        _foodLabel = GetNode<Label>("BarPadding/MainRow/ResourceRow/FoodLabel");
        _stoneLabel = GetNode<Label>("BarPadding/MainRow/ResourceRow/StoneLabel");
        _fireLabel = GetNode<Label>("BarPadding/MainRow/ResourceRow/FireLabel");
        _moneyLabel = GetNode<Label>("BarPadding/MainRow/ResourceRow/MoneyLabel");
        _qiLabel = GetNode<Label>("BarPadding/MainRow/ResourceRow/QiLabel");
        _eraLabel = GetNode<Label>("BarPadding/MainRow/CalendarBox/EraLabel");

        SetResourceText("人口 220", "军心 72", "粮 688", "石 220", "火 98", "钱 325", "真气 11", "景禾元年 正月 初一 · 立春");
    }

    public void SetResourceText(
        string population,
        string morale,
        string food,
        string stone,
        string fire,
        string money,
        string qi,
        string era)
    {
        _populationLabel.Text = population;
        _moraleLabel.Text = morale;
        _foodLabel.Text = food;
        _stoneLabel.Text = stone;
        _fireLabel.Text = fire;
        _moneyLabel.Text = money;
        _qiLabel.Text = qi;
        _eraLabel.Text = era;
    }
}
