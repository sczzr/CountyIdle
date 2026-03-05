using Godot;

namespace CountyIdle.UI.Figma;

public partial class EquipmentPanel : PanelContainer
{
    private Label _titleLabel = null!;
    private Label _summaryLabel = null!;
    private Label _slotOneLabel = null!;
    private Label _slotTwoLabel = null!;
    private Label _slotThreeLabel = null!;
    private Label _slotFourLabel = null!;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("PanelPadding/MainColumn/TitleLabel");
        _summaryLabel = GetNode<Label>("PanelPadding/MainColumn/SummaryPanel/SummaryLabel");
        _slotOneLabel = GetNode<Label>("PanelPadding/MainColumn/SlotsColumn/SlotOnePanel/SlotOneLabel");
        _slotTwoLabel = GetNode<Label>("PanelPadding/MainColumn/SlotsColumn/SlotTwoPanel/SlotTwoLabel");
        _slotThreeLabel = GetNode<Label>("PanelPadding/MainColumn/SlotsColumn/SlotThreePanel/SlotThreeLabel");
        _slotFourLabel = GetNode<Label>("PanelPadding/MainColumn/SlotsColumn/SlotFourPanel/SlotFourLabel");

        SetHeader("装备 · ID 1国翼", "护盾：升级银币 100%");
        SetSlotSummary(
            "武士刀 · 血 贤圣 试练",
            "盔甲统 · 真 红叶轮",
            "精炼件 · 真 红战神怒",
            "学士抱 · ★ 星云");
    }

    public void SetHeader(string title, string summary)
    {
        _titleLabel.Text = title;
        _summaryLabel.Text = summary;
    }

    public void SetSlotSummary(string slotOne, string slotTwo, string slotThree, string slotFour)
    {
        _slotOneLabel.Text = slotOne;
        _slotTwoLabel.Text = slotTwo;
        _slotThreeLabel.Text = slotThree;
        _slotFourLabel.Text = slotFour;
    }
}
