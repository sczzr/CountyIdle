using System;
using Godot;

namespace CountyIdle.UI;

/// <summary>
/// 留影录左侧单个玉简条目：负责显示卷名、落卷时刻、卷册类别与选中反馈。
/// </summary>
public partial class SaveJadeSlipItem : Control
{
    private static readonly Color InkColor = new(0.1725f, 0.1490f, 0.1255f, 1.0f);
    private static readonly Color InkMutedColor = new(0.4196f, 0.3686f, 0.3373f, 1.0f);
    private static readonly Color JadeLightColor = new(0.9059f, 0.9490f, 0.9176f, 1.0f);
    private static readonly Color JadeDarkColor = new(0.7725f, 0.8784f, 0.8235f, 1.0f);
    private static readonly Color SealColor = new(0.6980f, 0.1333f, 0.1333f, 1.0f);

    private PanelContainer _frame = null!;
    private ColorRect _glow = null!;
    private ColorRect _accent = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Label _badgeLabel = null!;
    private Label _stateLabel = null!;
    private Button _hitButton = null!;
    private Tween? _hoverTween;
    private bool _isHovered;
    private bool _isSelected;
    private string _titleText = "手卷 · 未命名";
    private string _subtitleText = string.Empty;
    private string _badgeText = string.Empty;
    private string _stateText = string.Empty;

    public string SlotKey { get; private set; } = string.Empty;

    public event Action<string>? Activated;

    public override void _Ready()
    {
        _frame = GetNode<PanelContainer>("Frame");
        _glow = GetNode<ColorRect>("Glow");
        _accent = GetNode<ColorRect>("Frame/Margin/ContentRow/Accent");
        _titleLabel = GetNode<Label>("Frame/Margin/ContentRow/TextColumn/TitleLabel");
        _subtitleLabel = GetNode<Label>("Frame/Margin/ContentRow/TextColumn/SubtitleLabel");
        _badgeLabel = GetNode<Label>("Frame/Margin/ContentRow/StatusColumn/BadgeLabel");
        _stateLabel = GetNode<Label>("Frame/Margin/ContentRow/StatusColumn/StateLabel");
        _hitButton = GetNode<Button>("HitButton");

        BindEvents();
        ApplyTheme();
        ApplyCurrentTextState();
        RefreshVisualState(true);
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
    /// 由留影录主面板统一注入展示数据，避免条目脚本直接依赖存档模型。
    /// </summary>
    public void SetDisplay(string slotKey, string title, string subtitle, string badge, string stateText, bool isSelected)
    {
        SlotKey = slotKey;
        _titleText = title;
        _subtitleText = subtitle;
        _badgeText = badge;
        _stateText = stateText;
        _isSelected = isSelected;

        if (!IsNodeReady())
        {
            return;
        }

        ApplyCurrentTextState();
        RefreshVisualState(true);
    }

    /// <summary>
    /// 列表重选时只切换视觉态，不重复重建节点树。
    /// </summary>
    public void SetSelectedState(bool isSelected)
    {
        _isSelected = isSelected;

        if (!IsNodeReady())
        {
            return;
        }

        RefreshVisualState(false);
    }

    /// <summary>
    /// 把外部提前写入的展示文本回填到已就绪的节点，避免先设值后进树时丢失内容。
    /// </summary>
    private void ApplyCurrentTextState()
    {
        _titleLabel.Text = _titleText;
        _subtitleLabel.Text = _subtitleText;
        _badgeLabel.Text = _badgeText;
    }

    private void BindEvents()
    {
        _hitButton.Flat = true;
        _hitButton.FocusMode = FocusModeEnum.All;
        _hitButton.MouseEntered += OnHoverEntered;
        _hitButton.MouseExited += OnHoverExited;
        _hitButton.FocusEntered += OnHoverEntered;
        _hitButton.FocusExited += OnHoverExited;
        _hitButton.Pressed += OnPressed;
    }

    private void ApplyTheme()
    {
        _titleLabel.AddThemeFontSizeOverride("font_size", 18);
        _subtitleLabel.AddThemeFontSizeOverride("font_size", 12);
        _badgeLabel.AddThemeFontSizeOverride("font_size", 13);
        _stateLabel.AddThemeFontSizeOverride("font_size", 12);

        _titleLabel.AddThemeColorOverride("font_color", InkColor);
        _subtitleLabel.AddThemeColorOverride("font_color", InkMutedColor);
        _badgeLabel.AddThemeColorOverride("font_color", InkColor);
        _stateLabel.AddThemeColorOverride("font_color", InkMutedColor);
    }

    private void OnHoverEntered()
    {
        _isHovered = true;
        RefreshVisualState(false);
    }

    private void OnHoverExited()
    {
        _isHovered = false;
        RefreshVisualState(false);
    }

    private void OnPressed()
    {
        Activated?.Invoke(SlotKey);
    }

    private void RefreshVisualState(bool instant)
    {
        var isAutoArchive = _badgeText.Contains("天道刻印", StringComparison.Ordinal);
        var isPrimaryArchive = _badgeText.Contains("本命主卷", StringComparison.Ordinal);
        var baseColor = isAutoArchive
            ? new Color(0.925f, 0.905f, 0.812f, 1.0f)
            : (isPrimaryArchive ? new Color(0.958f, 0.932f, 0.872f, 1.0f) : JadeLightColor);
        var accentBase = isAutoArchive
            ? new Color(0.72f, 0.58f, 0.23f, 1.0f)
            : (isPrimaryArchive ? SealColor : JadeDarkColor);
        var background = _isSelected
            ? accentBase.Lerp(Colors.White, 0.68f)
            : (_isHovered ? baseColor.Lerp(Colors.White, 0.22f) : baseColor.Lerp(Colors.White, 0.40f));
        var border = _isSelected ? SealColor : (_isHovered ? accentBase : new Color(accentBase.R, accentBase.G, accentBase.B, 0.62f));
        var accentColor = _isSelected ? SealColor : (_isHovered ? accentBase : new Color(accentBase.R, accentBase.G, accentBase.B, 0.88f));
        var targetOffsetX = _isSelected ? 12.0f : (_isHovered ? 6.0f : 0.0f);
        var targetScale = _isSelected ? new Vector2(1.01f, 1.01f) : (_isHovered ? new Vector2(1.005f, 1.005f) : Vector2.One);
        var glowColor = isAutoArchive
            ? new Color(0.90f, 0.80f, 0.42f, 1.0f)
            : (isPrimaryArchive ? new Color(0.92f, 0.68f, 0.68f, 1.0f) : new Color(0.75f, 0.90f, 0.84f, 1.0f));
        var glowAlpha = _isSelected ? 0.22f : (_isHovered ? 0.10f : 0.0f);

        _frame.AddThemeStyleboxOverride("panel", CreateFrameStyle(background, border));
        _accent.Color = accentColor;
        _badgeLabel.Modulate = _isSelected
            ? new Color(SealColor.R, SealColor.G, SealColor.B, 1.0f)
            : new Color(accentBase.R, accentBase.G, accentBase.B, 0.96f);
        _stateLabel.Text = _isSelected ? "已选" : _stateText;
        _stateLabel.Modulate = _isSelected ? new Color(SealColor.R, SealColor.G, SealColor.B, 0.92f) : new Color(InkMutedColor.R, InkMutedColor.G, InkMutedColor.B, 1.0f);
        _glow.Color = new Color(glowColor.R, glowColor.G, glowColor.B, glowAlpha);

        if (instant)
        {
            _hoverTween?.Kill();
            _frame.Position = new Vector2(targetOffsetX, 0.0f);
            _frame.Scale = targetScale;
            return;
        }

        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
        _hoverTween.TweenProperty(_frame, "position", new Vector2(targetOffsetX, 0.0f), 0.22f);
        _hoverTween.TweenProperty(_frame, "scale", targetScale, 0.22f);
    }

    private static StyleBoxFlat CreateFrameStyle(Color background, Color border)
    {
        var style = new StyleBoxFlat();
        style.BgColor = background;
        style.BorderWidthLeft = 1;
        style.BorderWidthTop = 1;
        style.BorderWidthRight = 1;
        style.BorderWidthBottom = 1;
        style.BorderColor = border;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.ShadowColor = new Color(0.07f, 0.06f, 0.05f, 0.14f);
        style.ShadowSize = 10;
        style.ContentMarginLeft = 0;
        style.ContentMarginTop = 0;
        style.ContentMarginRight = 0;
        style.ContentMarginBottom = 0;
        return style;
    }
}


