using System;
using Godot;

namespace CountyIdle.UI.Title;

/// <summary>
/// 启动画面的题屏总控：负责题字、菜单文案、响应式布局与右下角落款。
/// </summary>
public partial class TitleMenuOverlay : Control
{
    private const string CalligraphyFontPath = "res://assets/ui/fonts/MaShanZheng-Regular.ttf";
    private const string SerifFontPath = "res://assets/ui/fonts/NotoSerifSC[wght].ttf";
    private static readonly FontFile CalligraphyFont = GD.Load<FontFile>(CalligraphyFontPath)!;
    private static readonly FontFile SerifFont = GD.Load<FontFile>(SerifFontPath)!;
    private static readonly Color InkColor = new(0.1725f, 0.1725f, 0.1725f, 1.0f);
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Label _statusLabel = null!;
    private HBoxContainer _menuRow = null!;
    private VBoxContainer _contentVBox = null!;
    private VBoxContainer _headerBox = null!;
    private JadeMenuItem _startItem = null!;
    private JadeMenuItem _loadItem = null!;
    private JadeMenuItem _settingsItem = null!;
    private JadeMenuItem _modsItem = null!;
    private JadeMenuItem _exitItem = null!;
    private PanelContainer _sealBox = null!;
    private GridContainer _sealGrid = null!;
    private PoemWaterfallManager _poemWaterfall = null!;
    private TextureRect? _sealTexture;

    public event Action? StartRequested;
    public event Action? LoadRequested;
    public event Action? SettingsRequested;
    public event Action? ModsRequested;
    public event Action? ExitRequested;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        _titleLabel = GetNode<Label>("CenterWrap/ContentVBox/HeaderBox/TitleLabel");
        _subtitleLabel = GetNode<Label>("CenterWrap/ContentVBox/HeaderBox/SubtitleLabel");
        _statusLabel = GetNode<Label>("CenterWrap/ContentVBox/StatusLabel");
        _poemWaterfall = GetNode<PoemWaterfallManager>("PoemWaterfall");
        _menuRow = GetNode<HBoxContainer>("CenterWrap/ContentVBox/MenuRow");
        _contentVBox = GetNode<VBoxContainer>("CenterWrap/ContentVBox");
        _headerBox = GetNode<VBoxContainer>("CenterWrap/ContentVBox/HeaderBox");
        _startItem = GetNode<JadeMenuItem>("CenterWrap/ContentVBox/MenuRow/StartItem");
        _loadItem = GetNode<JadeMenuItem>("CenterWrap/ContentVBox/MenuRow/LoadItem");
        _settingsItem = GetNode<JadeMenuItem>("CenterWrap/ContentVBox/MenuRow/SettingsItem");
        _modsItem = GetNode<JadeMenuItem>("CenterWrap/ContentVBox/MenuRow/ModsItem");
        _exitItem = GetNode<JadeMenuItem>("CenterWrap/ContentVBox/MenuRow/ExitItem");
        _sealBox = GetNode<PanelContainer>("SealBox");
        _sealGrid = GetNode<GridContainer>("SealBox/SealGrid");
        _sealTexture = GetNodeOrNull<TextureRect>("SealTexture");

        ConfigureMenuTexts();
        BindMenuEvents();
        GetViewport().SizeChanged += OnViewportSizeChanged;
        UpdateResponsiveLayout();
        ClearStatusMessage();
        _poemWaterfall.SetActive(false);
        Hide();
    }

    public override void _ExitTree()
    {
        if (GetViewport() != null)
        {
            GetViewport().SizeChanged -= OnViewportSizeChanged;
        }

        _startItem.Activated -= OnStartRequested;
        _loadItem.Activated -= OnLoadRequested;
        _settingsItem.Activated -= OnSettingsRequested;
        _modsItem.Activated -= OnModsRequested;
        _exitItem.Activated -= OnExitRequested;
    }

    public void Open()
    {
        ClearStatusMessage();
        _poemWaterfall.SetActive(true);
        Show();
        MoveToFront();
        UpdateResponsiveLayout();
    }

    public void CloseOverlay()
    {
        _poemWaterfall.SetActive(false);
        Hide();
    }

    public void ShowStatusMessage(string message)
    {
        _statusLabel.Text = message;
        _statusLabel.Visible = !string.IsNullOrWhiteSpace(message);
    }

    public void ClearStatusMessage()
    {
        _statusLabel.Text = string.Empty;
        _statusLabel.Visible = false;
    }

    private void ConfigureMenuTexts()
    {
        _titleLabel.Text = "山海棋局";
        _subtitleLabel.Text = "S H A N H A I   S T R A T E G Y";
        _startItem.SetTexts("踏入仙途", "云深不知处", "山海觅长生");
        _loadItem.SetTexts("往昔因果", "梦回旧山河", "缘起落花时");
        _settingsItem.SetTexts("万象方寸", "乾坤归造化", "静思理玄机");
        _modsItem.SetTexts("奇门遁甲", "旁门参造化", "幻影变乾坤");
        _exitItem.SetTexts("离线归真", "挥袖远红尘", "拂衣入大荒");
    }

    private void BindMenuEvents()
    {
        _startItem.Activated += OnStartRequested;
        _loadItem.Activated += OnLoadRequested;
        _settingsItem.Activated += OnSettingsRequested;
        _modsItem.Activated += OnModsRequested;
        _exitItem.Activated += OnExitRequested;
    }

    private void OnViewportSizeChanged()
    {
        UpdateResponsiveLayout();
    }

    private void UpdateResponsiveLayout()
    {
        var viewportSize = GetViewportRect().Size;
        if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f)
        {
            return;
        }

        var unit = Mathf.Clamp(viewportSize.Y * 0.0085f, 5.4f, 13.6f);
        _contentVBox.AddThemeConstantOverride("separation", Mathf.RoundToInt(4.5f * unit));
        _headerBox.AddThemeConstantOverride("separation", Mathf.RoundToInt(0.9f * unit));
        _menuRow.AddThemeConstantOverride("separation", Mathf.RoundToInt(4.0f * unit));
        _titleLabel.AddThemeFontOverride("font", CalligraphyFont);
        _titleLabel.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(12.0f * unit));
        _titleLabel.AddThemeColorOverride("font_color", InkColor);
        _subtitleLabel.AddThemeFontOverride("font", SerifFont);
        _subtitleLabel.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(Mathf.Max(10.0f, 1.35f * unit)));
        _subtitleLabel.AddThemeColorOverride("font_color", new Color(0.35f, 0.35f, 0.35f, 0.55f));
        _statusLabel.AddThemeFontOverride("font", SerifFont);
        _statusLabel.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(Mathf.Max(11.0f, 1.4f * unit)));
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.42f, 0.33f, 0.24f, 0.86f));

        _startItem.ApplyVisualUnit(unit);
        _loadItem.ApplyVisualUnit(unit);
        _settingsItem.ApplyVisualUnit(unit);
        _modsItem.ApplyVisualUnit(unit);
        _exitItem.ApplyVisualUnit(unit);

        var sealSize = new Vector2(10.0f * unit, 10.0f * unit);
        _sealBox.CustomMinimumSize = sealSize;
        _sealBox.Size = sealSize;
        _sealBox.Position = viewportSize - sealSize - new Vector2(6.0f * unit, 6.0f * unit);
        _sealGrid.AddThemeConstantOverride("h_separation", Mathf.RoundToInt(0.15f * unit));
        _sealGrid.AddThemeConstantOverride("v_separation", Mathf.RoundToInt(0.15f * unit));

        foreach (var child in _sealGrid.GetChildren())
        {
            if (child is not Label label)
            {
                continue;
            }

            label.AddThemeFontOverride("font", CalligraphyFont);
            label.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(3.45f * unit));
            label.AddThemeColorOverride("font_color", new Color(0.6980f, 0.1333f, 0.1333f, 1.0f));
        }

        if (_sealTexture != null)
        {
            _sealTexture.Position = _sealBox.Position;
            _sealTexture.Size = sealSize;
            _sealTexture.Visible = true;
            _sealBox.Visible = false;
        }
        else
        {
            _sealBox.Visible = true;
        }
    }

    private void OnStartRequested()
    {
        ClearStatusMessage();
        StartRequested?.Invoke();
    }

    private void OnLoadRequested()
    {
        ClearStatusMessage();
        LoadRequested?.Invoke();
    }

    private void OnSettingsRequested()
    {
        ClearStatusMessage();
        SettingsRequested?.Invoke();
    }

    private void OnModsRequested()
    {
        ModsRequested?.Invoke();
    }

    private void OnExitRequested()
    {
        ExitRequested?.Invoke();
    }
}
