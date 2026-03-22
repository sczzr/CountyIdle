using Godot;

namespace CountyIdle.UI.Title;

/// <summary>
/// 标题页背景诗文瀑布：用动态 Label + Tween 模拟低透明度竖排诗句缓慢下落。
/// </summary>
public partial class PoemWaterfallManager : Control
{
    private const string SerifFontPath = "res://assets/ui/fonts/NotoSerifSC[wght].ttf";
    private static readonly FontFile SerifFont = GD.Load<FontFile>(SerifFontPath)!;
    private static readonly string[] Poems =
    {
        "北冥有鱼，其名为鲲。鲲之大，不知其几千里也。",
        "天地与我并生，而万物与我为一。",
        "道可道，非常道；名可名，非常名。",
        "上善若水。水善利万物而不争。",
        "飘飘乎如遗世独立，羽化而登仙。",
        "御剑乘风来，除魔天地间。",
        "山不在高，有仙则名；水不在深，有龙则灵。",
        "天之道，损有余而补不足。",
        "夫英雄者，胸怀大志，腹有良谋。",
        "所谓太极，无极而生，动静之机也。"
    };

    private readonly RandomNumberGenerator _rng = new();
    private int _spawnGeneration;
    private bool _isActive;

    [Export]
    public int LineCount { get; set; } = 8;

    [Export]
    public float MinDurationSeconds { get; set; } = 24.0f;

    [Export]
    public float MaxDurationSeconds { get; set; } = 42.0f;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _rng.Randomize();
        GetViewport().SizeChanged += OnViewportSizeChanged;
    }

    public override void _ExitTree()
    {
        if (GetViewport() != null)
        {
            GetViewport().SizeChanged -= OnViewportSizeChanged;
        }
    }

    private void OnViewportSizeChanged()
    {
        if (!_isActive)
        {
            return;
        }

        ClearLines();
        SpawnInitialLines();
    }

    /// <summary>
    /// 题屏显示时再启用瀑布流，避免隐藏状态仍持续生成 Tween 与 Label 占用性能。
    /// </summary>
    public void SetActive(bool active)
    {
        if (_isActive == active)
        {
            return;
        }

        _isActive = active;
        _spawnGeneration++;

        if (!_isActive)
        {
            ClearLines();
            return;
        }

        SpawnInitialLines();
    }

    private void ClearLines()
    {
        foreach (var child in GetChildren())
        {
            if (child is Label label)
            {
                label.QueueFree();
            }
        }
    }

    private void SpawnInitialLines()
    {
        if (!_isActive)
        {
            return;
        }

        var viewportSize = GetViewportRect().Size;
        if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f)
        {
            return;
        }

        for (var index = 0; index < LineCount; index++)
        {
            SpawnLine(randomizeProgress: true);
        }
    }

    private void SpawnLine(bool randomizeProgress)
    {
        if (!_isActive)
        {
            return;
        }

        var viewportSize = GetViewportRect().Size;
        if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f)
        {
            return;
        }

        var generation = _spawnGeneration;
        var label = new Label
        {
            MouseFilter = MouseFilterEnum.Ignore,
            ClipText = false,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.Off
        };
        AddChild(label);

        var fontSize = _rng.RandfRange(viewportSize.Y * 0.013f, viewportSize.Y * 0.024f);
        label.AddThemeFontOverride("font", SerifFont);
        label.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(fontSize));
        label.AddThemeColorOverride("font_color", new Color(0.1725f, 0.1725f, 0.1725f, 1.0f));
        label.Text = ToVerticalText(Poems[_rng.RandiRange(0, Poems.Length - 1)]);
        label.Size = label.GetMinimumSize();

        var startY = -viewportSize.Y * 0.2f - label.Size.Y;
        var endY = viewportSize.Y * 1.5f;
        var duration = _rng.RandfRange(MinDurationSeconds, MaxDurationSeconds);
        var maxX = Mathf.Max(viewportSize.X - label.Size.X, 0.0f);
        var x = _rng.RandfRange(0.0f, maxX);
        var startPosition = new Vector2(x, startY);
        var targetPosition = new Vector2(x, endY);
        var progress = randomizeProgress ? _rng.Randf() : 0.0f;
        var currentPosition = startPosition.Lerp(targetPosition, progress);
        var remainingDuration = Mathf.Max(2.0f, duration * (1.0f - progress));

        label.Position = currentPosition;
        label.Modulate = new Color(0.1725f, 0.1725f, 0.1725f, 0.06f);

        var tween = CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(label, "position:y", endY, remainingDuration);
        tween.TweenInterval(0.01f);
        tween.Finished += () =>
        {
            if (IsInstanceValid(label))
            {
                label.QueueFree();
            }

            if (_isActive && IsInsideTree() && generation == _spawnGeneration)
            {
                SpawnLine(randomizeProgress: false);
            }
        };
    }

    private static string ToVerticalText(string source)
    {
        return string.Join("\n", source.Trim().ToCharArray());
    }
}
