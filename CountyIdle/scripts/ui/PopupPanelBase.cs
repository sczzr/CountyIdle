using System;
using Godot;

namespace CountyIdle.UI;

public abstract partial class PopupPanelBase : Control
{
    private const double DefaultStatusMessageDurationSeconds = 2.4;

    private Label? _popupHintLabel;
    private string? _popupStatusMessage;
    private double _popupStatusMessageRemainingSeconds;

    protected string? PopupStatusMessage => _popupStatusMessage;

    protected void InitializePopupHint(string hintLabelPath)
    {
        _popupHintLabel = GetNodeOrNull<Label>(hintLabelPath);
        RefreshPopupHint();
    }

    protected void InitializePopupHint(Label hintLabel)
    {
        _popupHintLabel = hintLabel;
        RefreshPopupHint();
    }

    protected void OpenPopup()
    {
        ClearPopupStatusMessage(refreshHint: false);
        Show();
        RefreshPopupHint();
    }

    protected void ClosePopup()
    {
        OnPopupClosing();
        ClearPopupStatusMessage(refreshHint: false);
        Hide();
        RefreshPopupHint();
    }

    protected virtual void OnPopupClosing()
    {
    }

    protected bool TryHandlePopupClose(InputEvent @event)
    {
        if (!Visible || @event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return false;
        }

        return TryHandlePopupClose(keyEvent);
    }

    protected bool TryHandlePopupClose(InputEventKey keyEvent)
    {
        if (keyEvent.Keycode != Key.Escape)
        {
            return false;
        }

        ClosePopup();
        return true;
    }

    protected void TickPopupStatus(double delta)
    {
        if (_popupStatusMessageRemainingSeconds <= 0.0)
        {
            return;
        }

        _popupStatusMessageRemainingSeconds = Math.Max(0.0, _popupStatusMessageRemainingSeconds - delta);
        if (_popupStatusMessageRemainingSeconds > 0.0)
        {
            return;
        }

        _popupStatusMessage = null;
        RefreshPopupHint();
    }

    protected void ShowPopupStatusMessage(string statusMessage, double durationSeconds = DefaultStatusMessageDurationSeconds)
    {
        _popupStatusMessage = statusMessage;
        _popupStatusMessageRemainingSeconds = Math.Max(0.0, durationSeconds);
        RefreshPopupHint();
    }

    protected void ClearPopupStatusMessage(bool refreshHint = true)
    {
        _popupStatusMessage = null;
        _popupStatusMessageRemainingSeconds = 0.0;
        if (refreshHint)
        {
            RefreshPopupHint();
        }
    }

    protected void RefreshPopupHint()
    {
        if (_popupHintLabel == null)
        {
            return;
        }

        _popupHintLabel.Text = GetPopupHintText();
    }

    protected virtual string GetPopupHintText()
    {
        return string.IsNullOrWhiteSpace(_popupStatusMessage) ? string.Empty : _popupStatusMessage;
    }
}
