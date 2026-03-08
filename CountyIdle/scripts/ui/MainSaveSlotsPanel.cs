using Godot;
using CountyIdle.Models;
using CountyIdle.UI;

namespace CountyIdle;

public partial class Main
{
    private const string SaveSlotsPanelScenePath = "res://scenes/ui/SaveSlotsPanel.tscn";
    private const int AutoSaveSettlementInterval = 6;

    private SaveSlotsPanel? _saveSlotsPanel;

    private void CreateSaveSlotsPanel()
    {
        var panelScene = GD.Load<PackedScene>(SaveSlotsPanelScenePath);
        if (panelScene == null)
        {
            return;
        }

        _saveSlotsPanel = panelScene.Instantiate<SaveSlotsPanel>();
        _saveSlotsPanel.SaveSelectedRequested += OnSaveSlotsPanelSaveSelectedRequested;
        _saveSlotsPanel.CreateSlotRequested += OnSaveSlotsPanelCreateSlotRequested;
        _saveSlotsPanel.CopySlotRequested += OnSaveSlotsPanelCopySlotRequested;
        _saveSlotsPanel.LoadSelectedRequested += OnSaveSlotsPanelLoadSelectedRequested;
        _saveSlotsPanel.RenameRequested += OnSaveSlotsPanelRenameRequested;
        _saveSlotsPanel.DeleteRequested += OnSaveSlotsPanelDeleteRequested;
        _saveSlotsPanel.RefreshRequested += OnSaveSlotsPanelRefreshRequested;
        AddChild(_saveSlotsPanel);
        MoveChild(_saveSlotsPanel, GetChildCount() - 1);
    }

    private void OpenSaveSlotsPanelForSave()
    {
        _saveSlotsPanel?.Open(_saveSystem.ListSlots(), SaveSlotsPanel.PanelIntent.Save, _saveSystem.DefaultSlotKey);
    }

    private void OpenSaveSlotsPanelForLoad()
    {
        _saveSlotsPanel?.Open(_saveSystem.ListSlots(), SaveSlotsPanel.PanelIntent.Load);
    }

    private void RefreshSaveSlotsPanel(string? preferredSlotKey = null, string? statusMessage = null)
    {
        if (_saveSlotsPanel == null)
        {
            return;
        }

        _saveSlotsPanel.RefreshSlots(_saveSystem.ListSlots(), preferredSlotKey, statusMessage);
    }

    private void OnSaveSlotsPanelSaveSelectedRequested(string slotKey, string slotName)
    {
        var success = _saveSystem.SaveToSlot(_gameLoop.State, slotKey, slotName, out var message);
        if (success)
        {
            UpdateSavePreviewForSlot(slotKey);
        }

        AppendLog(message);
        RefreshSaveSlotsPanel(slotKey, message);
    }

    private void OnSaveSlotsPanelCreateSlotRequested(string slotName)
    {
        var success = _saveSystem.SaveToNewSlot(_gameLoop.State, slotName, out var slotKey, out var message);
        if (success && !string.IsNullOrWhiteSpace(slotKey))
        {
            UpdateSavePreviewForSlot(slotKey);
        }

        AppendLog(message);
        RefreshSaveSlotsPanel(string.IsNullOrWhiteSpace(slotKey) ? null : slotKey, message);
    }

    private void OnSaveSlotsPanelCopySlotRequested(string sourceSlotKey, string targetSlotName)
    {
        var success = _saveSystem.CopySlotToNewSlot(sourceSlotKey, targetSlotName, out var slotKey, out var message);
        AppendLog(message);
        RefreshSaveSlotsPanel(success ? slotKey : sourceSlotKey, message);
    }

    private void OnSaveSlotsPanelLoadSelectedRequested(string slotKey)
    {
        var success = _saveSystem.TryLoadSlot(slotKey, out var state, out var message);
        if (success)
        {
            _gameLoop.LoadState(state);
            _saveSlotsPanel?.ClosePanel();
        }

        AppendLog(message);
        RefreshSaveSlotsPanel(slotKey, message);
    }

    private void OnSaveSlotsPanelRenameRequested(string slotKey, string slotName)
    {
        _saveSystem.RenameSlot(slotKey, slotName, out var message);
        AppendLog(message);
        RefreshSaveSlotsPanel(slotKey, message);
    }

    private void OnSaveSlotsPanelDeleteRequested(string slotKey)
    {
        var fallbackSlotKey = _saveSystem.DefaultSlotKey == slotKey ? null : _saveSystem.DefaultSlotKey;
        _saveSystem.DeleteSlot(slotKey, out var message);
        AppendLog(message);
        RefreshSaveSlotsPanel(fallbackSlotKey, message);
    }

    private void OnSaveSlotsPanelRefreshRequested()
    {
        RefreshSaveSlotsPanel(statusMessage: "已刷新存档槽列表。");
    }

    private void UnbindSaveSlotsPanelEvents()
    {
        if (_saveButton != null)
        {
            _saveButton.Pressed -= OpenSaveSlotsPanelForSave;
        }

        if (_loadButton != null)
        {
            _loadButton.Pressed -= OpenSaveSlotsPanelForLoad;
        }

        if (_saveSlotsPanel == null)
        {
            return;
        }

        _saveSlotsPanel.SaveSelectedRequested -= OnSaveSlotsPanelSaveSelectedRequested;
        _saveSlotsPanel.CreateSlotRequested -= OnSaveSlotsPanelCreateSlotRequested;
        _saveSlotsPanel.CopySlotRequested -= OnSaveSlotsPanelCopySlotRequested;
        _saveSlotsPanel.LoadSelectedRequested -= OnSaveSlotsPanelLoadSelectedRequested;
        _saveSlotsPanel.RenameRequested -= OnSaveSlotsPanelRenameRequested;
        _saveSlotsPanel.DeleteRequested -= OnSaveSlotsPanelDeleteRequested;
        _saveSlotsPanel.RefreshRequested -= OnSaveSlotsPanelRefreshRequested;
    }

    private void HandleAutoSaveFromState(GameState state)
    {
        if (_lastObservedHourSettlements < 0 || state.HourSettlements < _lastObservedHourSettlements)
        {
            _lastObservedHourSettlements = state.HourSettlements;
            return;
        }

        if (state.HourSettlements == _lastObservedHourSettlements)
        {
            return;
        }

        var previousBucket = _lastObservedHourSettlements / AutoSaveSettlementInterval;
        var currentBucket = state.HourSettlements / AutoSaveSettlementInterval;
        _lastObservedHourSettlements = state.HourSettlements;

        if (state.HourSettlements <= 0 || currentBucket <= previousBucket)
        {
            return;
        }

        var rotationIndex = (currentBucket - 1) % _saveSystem.AutoSaveSlotKeysView.Count;
        var success = _saveSystem.SaveAutoSlot(state, rotationIndex, out var slotKey, out var message);
        if (success)
        {
            UpdateSavePreviewForSlot(slotKey);
            RefreshSaveSlotsPanel(slotKey, message);
        }

        AppendLog(message);
    }
}
