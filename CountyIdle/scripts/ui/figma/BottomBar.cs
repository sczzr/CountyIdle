using Godot;

namespace CountyIdle.UI.Figma;

public partial class BottomBar : PanelContainer
{
    private Label _cardOneLabel = null!;
    private Label _cardTwoLabel = null!;
    private Label _cardThreeLabel = null!;
    private Button _speedButton = null!;
    private Button _burstButton = null!;
    private Button _saveButton = null!;
    private Button _loadButton = null!;
    private Button _settingsButton = null!;
    private Button _alertButton = null!;

    public override void _Ready()
    {
        _cardOneLabel = GetNode<Label>("PanelPadding/MainColumn/EquipmentRow/CardOnePanel/CardOneLabel");
        _cardTwoLabel = GetNode<Label>("PanelPadding/MainColumn/EquipmentRow/CardTwoPanel/CardTwoLabel");
        _cardThreeLabel = GetNode<Label>("PanelPadding/MainColumn/EquipmentRow/CardThreePanel/CardThreeLabel");

        _speedButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/LeftActions/SpeedButton");
        _burstButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/LeftActions/BurstButton");
        _saveButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/RightActions/SaveButton");
        _loadButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/RightActions/LoadButton");
        _settingsButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/RightActions/SettingsButton");
        _alertButton = GetNode<Button>("PanelPadding/MainColumn/ActionRow/RightActions/AlertButton");

        SetEquipmentCards("钢铁剑 · Lv.2", "伏木弓部族 · Lv.5", "初始砸星 · Lv.5");
        SetActionText("▶ 51", "■ ×2", "封存档整理", "读档", "设置", "资为立场");
    }

    public void SetEquipmentCards(string cardOne, string cardTwo, string cardThree)
    {
        _cardOneLabel.Text = cardOne;
        _cardTwoLabel.Text = cardTwo;
        _cardThreeLabel.Text = cardThree;
    }

    public void SetActionText(string speed, string burst, string save, string load, string settings, string alert)
    {
        _speedButton.Text = speed;
        _burstButton.Text = burst;
        _saveButton.Text = save;
        _loadButton.Text = load;
        _settingsButton.Text = settings;
        _alertButton.Text = alert;
    }
}
