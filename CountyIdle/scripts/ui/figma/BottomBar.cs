using Godot;

namespace CountyIdle.UI.Figma;

public partial class BottomBar : PanelContainer
{
    private Label _cardOneLabel = null!;
    private Label _cardTwoLabel = null!;
    private Label _cardThreeLabel = null!;
    private Button _exploreButton = null!;
    private Button _speedX1Button = null!;
    private Button _speedX2Button = null!;
    private Button _speedX4Button = null!;
    private Button _saveButton = null!;
    private Button _loadButton = null!;
    private Button _settingsButton = null!;
    private Button _alertButton = null!;

    public override void _Ready()
    {
        _cardOneLabel = GetNode<Label>("PanelPadding/MainColumn/EquipmentRow/CardOnePanel/CardOneLabel");
        _cardTwoLabel = GetNode<Label>("PanelPadding/MainColumn/EquipmentRow/CardTwoPanel/CardTwoLabel");
        _cardThreeLabel = GetNode<Label>("PanelPadding/MainColumn/EquipmentRow/CardThreePanel/CardThreeLabel");

        _exploreButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/LeftActions/ExploreButton");
        _speedX1Button = GetNode<Button>("PanelPadding/MainColumn/ActionRow/LeftActions/SpeedX1Button");
        _speedX2Button = GetNode<Button>("PanelPadding/MainColumn/ActionRow/LeftActions/SpeedX2Button");
        _speedX4Button = GetNode<Button>("PanelPadding/MainColumn/ActionRow/LeftActions/SpeedX4Button");
        _saveButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/RightActions/SaveButton");
        _loadButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/RightActions/LoadButton");
        _settingsButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/RightActions/SettingsButton");
        _alertButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/RightActions/AlertButton");

        SetEquipmentCards("钢铁剑 · Lv.2", "伏木弓部族 · Lv.5", "初始砸星 · Lv.5");
        SetActionText("▶ 探险", "x1", "x2", "x4", "封存档整理", "读档", "设置", "资为立场");
    }

    public void SetEquipmentCards(string cardOne, string cardTwo, string cardThree)
    {
        _cardOneLabel.Text = cardOne;
        _cardTwoLabel.Text = cardTwo;
        _cardThreeLabel.Text = cardThree;
    }

    public void SetActionText(string explore, string speedX1, string speedX2, string speedX4, string save, string load, string settings, string alert)
    {
        _exploreButton.Text = explore;
        _speedX1Button.Text = speedX1;
        _speedX2Button.Text = speedX2;
        _speedX4Button.Text = speedX4;
        _saveButton.Text = save;
        _loadButton.Text = load;
        _settingsButton.Text = settings;
        _alertButton.Text = alert;
    }
}
