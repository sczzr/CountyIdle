using Godot;

namespace CountyIdle.UI.Figma;

public partial class CenterView : PanelContainer
{
    private Label _titleLabel = null!;
    private Label _descriptionLabel = null!;
    private Button _leftActionButton = null!;
    private Button _middleActionButton = null!;
    private Button _rightActionButton = null!;

    public override void _Ready()
    {
        _leftActionButton = GetNode<Button>("PanelPadding/MainColumn/TopActionRow/LeftActionButton");
        _middleActionButton = GetNode<Button>("PanelPadding/MainColumn/TopActionRow/MiddleActionButton");
        _rightActionButton = GetNode<Button>("PanelPadding/MainColumn/TopActionRow/RightActionButton");
        _titleLabel = GetNode<Label>("PanelPadding/MainColumn/CenterInfoBox/CenterInfoColumn/CenterTitleLabel");
        _descriptionLabel = GetNode<Label>("PanelPadding/MainColumn/CenterInfoBox/CenterInfoColumn/CenterDescriptionLabel");

        SetTopActions("钻轴销购 (2.50)", "租品明细", "关于国师");
        SetCenterInfo("安度提界", "[楼下入口]等级提升 2.50 中的楼陆性能Q");
    }

    public void SetTopActions(string leftAction, string middleAction, string rightAction)
    {
        _leftActionButton.Text = leftAction;
        _middleActionButton.Text = middleAction;
        _rightActionButton.Text = rightAction;
    }

    public void SetCenterInfo(string title, string description)
    {
        _titleLabel.Text = title;
        _descriptionLabel.Text = description;
    }
}
