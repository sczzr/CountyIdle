using System;
using Godot;

namespace CountyIdle.UI.Title;

/// <summary>
/// 单个玉简菜单项：负责三片玉简的排布、竖排题字与 hover/点击动效。
/// </summary>
public partial class JadeMenuItem : Control
{
    private const string CalligraphyFontPath = "res://assets/ui/fonts/MaShanZheng-Regular.ttf";
    private static readonly FontFile CalligraphyFont = GD.Load<FontFile>(CalligraphyFontPath)!;
    private static readonly Color InkColor = new(0.1725f, 0.1725f, 0.1725f, 1.0f);
    private static readonly Color SealColor = new(0.6980f, 0.1333f, 0.1333f, 1.0f);
    private static readonly Color SideInkColor = new(0.1725f, 0.1725f, 0.1725f, 0.42f);

    private const float LeafWidthUnits = 9.0f;
    private const float LeafHeightUnits = 42.0f;
    private const float LeafSpreadUnits = 10.2f;
    private const float HoverRiseUnits = 2.0f;
    private const float GroupPaddingUnits = 2.0f;
    private const float MainFontUnits = 3.2f;
    private const float SideFontUnits = 1.9f;
    private const float HoleDiameterUnits = 0.95f;
    private const float HoleMarginUnits = 2.2f;

    private Control _leftLeaf = null!;
    private Control _rightLeaf = null!;
    private Control _mainLeaf = null!;
    private Panel? _leftLeafFrame;
    private Panel? _rightLeafFrame;
    private Panel? _mainLeafFrame;
    private TextureRect? _leftLeafTexture;
    private TextureRect? _rightLeafTexture;
    private TextureRect? _mainLeafTexture;
    private Label _leftLabel = null!;
    private Label _rightLabel = null!;
    private Label _mainLabel = null!;
    private Button _hitButton = null!;
    private Tween? _hoverTween;
    private float _visualUnit = 6.12f;
    private Vector2 _collapsedLeafPosition = Vector2.Zero;
    private Vector2 _leftExpandedPosition = Vector2.Zero;
    private Vector2 _rightExpandedPosition = Vector2.Zero;
    private Vector2 _mainCollapsedPosition = Vector2.Zero;
    private Vector2 _mainExpandedPosition = Vector2.Zero;
    private bool _isExpanded;

    public event Action? Activated;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        _leftLeaf = GetNode<Control>("LeftLeaf");
        _rightLeaf = GetNode<Control>("RightLeaf");
        _mainLeaf = GetNode<Control>("MainLeaf");
        _leftLeafFrame = GetNodeOrNull<Panel>("LeftLeaf/LeafFrame");
        _rightLeafFrame = GetNodeOrNull<Panel>("RightLeaf/LeafFrame");
        _mainLeafFrame = GetNodeOrNull<Panel>("MainLeaf/LeafFrame");
        _leftLeafTexture = GetNodeOrNull<TextureRect>("LeftLeaf/LeafTexture");
        _rightLeafTexture = GetNodeOrNull<TextureRect>("RightLeaf/LeafTexture");
        _mainLeafTexture = GetNodeOrNull<TextureRect>("MainLeaf/LeafTexture");
        _leftLabel = GetNode<Label>("LeftLeaf/TextLabel");
        _rightLabel = GetNode<Label>("RightLeaf/TextLabel");
        _mainLabel = GetNode<Label>("MainLeaf/TextLabel");
        _hitButton = GetNode<Button>("HitButton");

        BindHitArea();
        ApplyVisualUnit(_visualUnit);
        ApplyCollapsedState();
    }

    public override void _ExitTree()
    {
        if (_hitButton == null)
        {
            return;
        }

        _hitButton.MouseEntered -= OnHoverEntered;
        _hitButton.MouseExited -= OnHoverExited;
        _hitButton.FocusEntered -= OnHoverEntered;
        _hitButton.FocusExited -= OnHoverExited;
        _hitButton.Pressed -= OnPressed;
    }

    /// <summary>
    /// 由标题页统一注入文案，避免在场景文件里散落内容常量。
    /// </summary>
    public void SetTexts(string mainText, string leftText, string rightText)
    {
        _mainLabel.Text = ToVerticalText(mainText);
        _leftLabel.Text = ToVerticalText(leftText);
        _rightLabel.Text = ToVerticalText(rightText);
    }

    /// <summary>
    /// 以 viewport 高度推导视觉单位，保持标题页在不同窗口下的尺寸节奏。
    /// </summary>
    public void ApplyVisualUnit(float unit)
    {
        _visualUnit = Mathf.Max(unit, 4.5f);
        var leafSize = new Vector2(LeafWidthUnits * _visualUnit, LeafHeightUnits * _visualUnit);
        var horizontalPadding = GroupPaddingUnits * _visualUnit;
        var groupWidth = leafSize.X + (LeafSpreadUnits * _visualUnit * 2.0f) + horizontalPadding;
        var groupHeight = leafSize.Y + (HoverRiseUnits * _visualUnit) + horizontalPadding;
        CustomMinimumSize = new Vector2(groupWidth, groupHeight);
        Size = CustomMinimumSize;

        var baseX = (groupWidth - leafSize.X) * 0.5f;
        var baseY = HoverRiseUnits * _visualUnit + (horizontalPadding * 0.5f);
        _collapsedLeafPosition = new Vector2(baseX, baseY);
        _leftExpandedPosition = new Vector2(baseX - (LeafSpreadUnits * _visualUnit), baseY);
        _rightExpandedPosition = new Vector2(baseX + (LeafSpreadUnits * _visualUnit), baseY);
        _mainCollapsedPosition = _collapsedLeafPosition;
        _mainExpandedPosition = new Vector2(baseX, baseY - (HoverRiseUnits * _visualUnit));

        ConfigureLeaf(_leftLeaf, _leftLeafFrame, _leftLeafTexture, _leftLabel, leafSize, false);
        ConfigureLeaf(_rightLeaf, _rightLeafFrame, _rightLeafTexture, _rightLabel, leafSize, false);
        ConfigureLeaf(_mainLeaf, _mainLeafFrame, _mainLeafTexture, _mainLabel, leafSize, true);

        _leftLeaf.Position = _isExpanded ? _leftExpandedPosition : _collapsedLeafPosition;
        _rightLeaf.Position = _isExpanded ? _rightExpandedPosition : _collapsedLeafPosition;
        _mainLeaf.Position = _isExpanded ? _mainExpandedPosition : _mainCollapsedPosition;

        _hitButton.CustomMinimumSize = CustomMinimumSize;
        _hitButton.Size = CustomMinimumSize;
    }

    private void BindHitArea()
    {
        _hitButton.Flat = true;
        _hitButton.FocusMode = FocusModeEnum.All;
        _hitButton.MouseEntered += OnHoverEntered;
        _hitButton.MouseExited += OnHoverExited;
        _hitButton.FocusEntered += OnHoverEntered;
        _hitButton.FocusExited += OnHoverExited;
        _hitButton.Pressed += OnPressed;
    }

    private void ConfigureLeaf(Control leaf, Panel? leafFrame, TextureRect? leafTexture, Label label, Vector2 leafSize, bool isMainLeaf)
    {
        leaf.CustomMinimumSize = leafSize;
        leaf.Size = leafSize;
        leaf.PivotOffset = leafSize * 0.5f;

        var body = leaf.GetNode<ColorRect>("Body");
        body.Size = leafSize;
        if (body.Material is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("corner_radius", 0.115f);
            shaderMaterial.SetShaderParameter("border_width", 0.018f);
        }

        var topHole = leaf.GetNode<Control>("TopHole");
        var bottomHole = leaf.GetNode<Control>("BottomHole");
        var holeSize = HoleDiameterUnits * _visualUnit;
        var holeX = (leafSize.X - holeSize) * 0.5f;
        var holeMargin = HoleMarginUnits * _visualUnit;
        topHole.Position = new Vector2(holeX, holeMargin);
        topHole.Size = new Vector2(holeSize, holeSize);
        bottomHole.Position = new Vector2(holeX, leafSize.Y - holeMargin - holeSize);
        bottomHole.Size = new Vector2(holeSize, holeSize);

        if (leafTexture != null)
        {
            if (leafFrame != null)
            {
                // 让描边与阴影交给 Godot 的 StyleBox 承担，避免 SVG 边缘与贴图阴影叠出灰边。
                leafFrame.Position = Vector2.Zero;
                leafFrame.Size = leafSize;
                leafFrame.Visible = true;
            }

            leafTexture.Position = Vector2.Zero;
            leafTexture.Size = leafSize;
            leafTexture.Visible = true;
            body.Visible = false;
            topHole.Visible = false;
            bottomHole.Visible = false;
        }
        else
        {
            if (leafFrame != null)
            {
                leafFrame.Visible = false;
            }

            body.Visible = true;
            topHole.Visible = true;
            bottomHole.Visible = true;
        }

        label.Size = leafSize;
        label.Position = Vector2.Zero;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.AutowrapMode = TextServer.AutowrapMode.Off;
        label.AddThemeFontOverride("font", CalligraphyFont);
        label.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt((isMainLeaf ? MainFontUnits : SideFontUnits) * _visualUnit));
        label.Modulate = isMainLeaf ? InkColor : SideInkColor;
    }

    private void ApplyCollapsedState()
    {
        _isExpanded = false;
        _hoverTween?.Kill();
        _mainLeaf.Position = _mainCollapsedPosition;
        _mainLeaf.RotationDegrees = 0.0f;
        _mainLeaf.Scale = Vector2.One;
        _mainLabel.Modulate = InkColor;
        _leftLeaf.Position = _collapsedLeafPosition;
        _leftLeaf.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        _leftLeaf.RotationDegrees = 0.0f;
        _leftLeaf.Scale = Vector2.One;
        _rightLeaf.Position = _collapsedLeafPosition;
        _rightLeaf.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        _rightLeaf.RotationDegrees = 0.0f;
        _rightLeaf.Scale = Vector2.One;
    }

    private void OnHoverEntered()
    {
        PlayHoverState(true);
    }

    private void OnHoverExited()
    {
        PlayHoverState(false);
    }

    private void OnPressed()
    {
        Activated?.Invoke();
    }

    private void PlayHoverState(bool expand)
    {
        _isExpanded = expand;
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);

        _hoverTween.TweenProperty(_mainLeaf, "position", expand ? _mainExpandedPosition : _mainCollapsedPosition, 0.6f);
        _hoverTween.TweenProperty(_mainLabel, "modulate", expand ? SealColor : InkColor, 0.4f);

        _hoverTween.TweenProperty(_leftLeaf, "position", expand ? _leftExpandedPosition : _collapsedLeafPosition, 0.7f);
        _hoverTween.TweenProperty(_leftLeaf, "modulate", expand ? Colors.White : new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.5f);
        _hoverTween.TweenProperty(_leftLeaf, "rotation_degrees", expand ? -4.0f : 0.0f, 0.7f);
        _hoverTween.TweenProperty(_leftLeaf, "scale", expand ? new Vector2(0.94f, 0.94f) : Vector2.One, 0.7f);

        _hoverTween.TweenProperty(_rightLeaf, "position", expand ? _rightExpandedPosition : _collapsedLeafPosition, 0.7f);
        _hoverTween.TweenProperty(_rightLeaf, "modulate", expand ? Colors.White : new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.5f);
        _hoverTween.TweenProperty(_rightLeaf, "rotation_degrees", expand ? 4.0f : 0.0f, 0.7f);
        _hoverTween.TweenProperty(_rightLeaf, "scale", expand ? new Vector2(0.94f, 0.94f) : Vector2.One, 0.7f);
    }

    private static string ToVerticalText(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        return string.Join("\n", source.Trim().ToCharArray());
    }
}
