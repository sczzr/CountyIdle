using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle;

public partial class Main
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (_gameLoop == null)
        {
            return;
        }

        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        if (_settingsPanel?.Visible == true || _warehousePanel?.Visible == true || _saveSlotsPanel?.Visible == true)
        {
            return;
        }

        var pressedKey = keyEvent.Keycode;
        if (pressedKey == Key.None)
        {
            return;
        }

        if (IsShortcutPressed(pressedKey, _clientSettings.OpenSettingsKey, ClientSettings.DefaultOpenSettingsKey))
        {
            OpenSettingsPanel();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (IsShortcutPressed(pressedKey, _clientSettings.OpenWarehouseKey, ClientSettings.DefaultOpenWarehouseKey))
        {
            OpenWarehousePanel();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (IsShortcutPressed(pressedKey, _clientSettings.ToggleExplorationKey, ClientSettings.DefaultToggleExplorationKey))
        {
            _gameLoop.ToggleExploration();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (IsShortcutPressed(pressedKey, _clientSettings.ToggleSpeedKey, ClientSettings.DefaultToggleSpeedKey))
        {
            ExecuteToggleSpeed();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (IsShortcutPressed(pressedKey, _clientSettings.QuickSaveKey, ClientSettings.DefaultQuickSaveKey))
        {
            ExecuteQuickSave();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (IsShortcutPressed(pressedKey, _clientSettings.QuickLoadKey, ClientSettings.DefaultQuickLoadKey))
        {
            ExecuteQuickLoad();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (IsShortcutPressed(pressedKey, _clientSettings.QuickResetKey, ClientSettings.DefaultQuickResetKey))
        {
            ExecuteQuickReset();
            GetViewport().SetInputAsHandled();
        }
    }

    private static bool IsShortcutPressed(Key pressedKey, string configuredKey, string fallbackKey)
    {
        if (TryParseKey(configuredKey, out var parsedConfiguredKey))
        {
            return pressedKey == parsedConfiguredKey;
        }

        return TryParseKey(fallbackKey, out var parsedFallbackKey) && pressedKey == parsedFallbackKey;
    }

    private static bool TryParseKey(string? keyName, out Key parsedKey)
    {
        parsedKey = Key.None;
        if (string.IsNullOrWhiteSpace(keyName))
        {
            return false;
        }

        return Enum.TryParse<Key>(keyName, true, out parsedKey) && parsedKey != Key.None;
    }

    private void ExecuteToggleSpeed()
    {
        CycleSpeedScale();
    }

    private void ExecuteQuickSave()
    {
        var success = _saveSystem.Save(_gameLoop.State, out var saveMessage);
        if (success)
        {
            UpdateSavePreviewForSlot(_saveSystem.DefaultSlotKey);
        }

        AppendLog(saveMessage);
    }

    private void ExecuteQuickLoad()
    {
        _saveSystem.TryLoad(out var state, out var loadMessage);
        _gameLoop.LoadState(state);
        AppendLog(loadMessage);
    }

    private void ExecuteQuickReset()
    {
        _gameLoop.ResetState();
    }
}
