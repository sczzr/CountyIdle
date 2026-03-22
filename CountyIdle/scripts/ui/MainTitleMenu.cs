using Godot;
using CountyIdle.UI.Title;

namespace CountyIdle;

/// <summary>
/// 主界面题屏接线：在正式进入宗门经营前，先展示国风玉简主菜单。
/// </summary>
public partial class Main
{
    private const string TitleMenuOverlayPath = "TitleMenuOverlay";

    private TitleMenuOverlay? _titleMenuOverlay;

    /// <summary>
    /// 题屏在启动时接管主界面可见性，并暂停主循环推进。
    /// </summary>
    private void InitializeTitleMenuOverlay()
    {
        _titleMenuOverlay = GetNodeOrNull<TitleMenuOverlay>(TitleMenuOverlayPath);
        if (_titleMenuOverlay == null)
        {
            return;
        }

        _titleMenuOverlay.StartRequested += OnTitleMenuStartRequested;
        _titleMenuOverlay.LoadRequested += OnTitleMenuLoadRequested;
        _titleMenuOverlay.SettingsRequested += OnTitleMenuSettingsRequested;
        _titleMenuOverlay.ModsRequested += OnTitleMenuModsRequested;
        _titleMenuOverlay.ExitRequested += OnTitleMenuExitRequested;

        SetMainShellVisible(false);
        SuspendGameplayForTitleMenu();
        _titleMenuOverlay.Open();
    }

    private void OnTitleMenuStartRequested()
    {
        EnterGameplayFromTitleMenu();
        AppendLog("已踏入仙途，宗门运转启封。");
    }

    private void OnTitleMenuLoadRequested()
    {
        OpenSaveSlotsPanelForLoad();
    }

    private void OnTitleMenuSettingsRequested()
    {
        OpenSettingsPanel();
    }

    private void OnTitleMenuModsRequested()
    {
        _titleMenuOverlay?.ShowStatusMessage("奇門遁甲卷暂未开放，后续会对接正式 MOD 入口。");
    }

    private void OnTitleMenuExitRequested()
    {
        GetTree().Quit();
    }

    /// <summary>
    /// 真正进入宗门经营时，恢复主界面与主循环。
    /// </summary>
    private void EnterGameplayFromTitleMenu()
    {
        if (!IsTitleMenuVisible())
        {
            return;
        }

        _titleMenuOverlay?.CloseOverlay();
        SetMainShellVisible(true);
        ResumeGameplayFromTitleMenu();
    }

    private bool IsTitleMenuVisible()
    {
        return _titleMenuOverlay != null && _titleMenuOverlay.Visible;
    }

    private void SuspendGameplayForTitleMenu()
    {
        if (_gameLoop != null)
        {
            _gameLoop.ProcessMode = Node.ProcessModeEnum.Disabled;
        }
    }

    private void ResumeGameplayFromTitleMenu()
    {
        if (_gameLoop != null)
        {
            _gameLoop.ProcessMode = Node.ProcessModeEnum.Inherit;
        }
    }

    /// <summary>
    /// 题屏开启时隐藏正式主界面壳层；进入游戏后再恢复。
    /// </summary>
    private void SetMainShellVisible(bool visible)
    {
        _legacyLayoutRoot.Visible = visible;
        if (_backgroundTextureRect != null)
        {
            _backgroundTextureRect.Visible = visible;
        }

        if (_mainHudLayer != null)
        {
            _mainHudLayer.Visible = visible;
        }
    }

    private void UnbindTitleMenuOverlayEvents()
    {
        if (_titleMenuOverlay == null)
        {
            return;
        }

        _titleMenuOverlay.StartRequested -= OnTitleMenuStartRequested;
        _titleMenuOverlay.LoadRequested -= OnTitleMenuLoadRequested;
        _titleMenuOverlay.SettingsRequested -= OnTitleMenuSettingsRequested;
        _titleMenuOverlay.ModsRequested -= OnTitleMenuModsRequested;
        _titleMenuOverlay.ExitRequested -= OnTitleMenuExitRequested;
    }
}
