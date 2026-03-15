using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class SaveSlotsPanel : PopupPanelBase
{
    public enum PanelIntent
    {
        Save,
        Load
    }

    private enum SlotFilterMode
    {
        All,
        Primary,
        Manual,
        Autosave
    }

    private enum SlotSortMode
    {
        UpdatedDesc,
        UpdatedAsc,
        ProgressDesc,
        PopulationDesc,
        GoldDesc,
        TechDesc
    }

    private PanelContainer _dialog = null!;
    private PanelContainer _previewFrame = null!;
    private ItemList _slotList = null!;
    private Label _slotListTitle = null!;
    private Label _modeLabel = null!;
    private Label _slotDetailLabel = null!;
    private TextureRect _previewTexture = null!;
    private Label _previewHintLabel = null!;
    private LineEdit _slotNameEdit = null!;
    private OptionButton _filterOptionButton = null!;
    private OptionButton _sortOptionButton = null!;
    private Button _saveSelectedButton = null!;
    private Button _loadSelectedButton = null!;
    private Button _createSlotButton = null!;
    private Button _renameSlotButton = null!;
    private Button _copySlotButton = null!;
    private Button _deleteSlotButton = null!;
    private Button _refreshButton = null!;
    private Button _closeButton = null!;
    private Button _footerCloseButton = null!;
    private Node? _previewVisualFx;

    private readonly List<SaveSlotSummary> _allSlots = new();
    private readonly List<SaveSlotSummary> _visibleSlots = new();
    private readonly GameCalendarSystem _calendarSystem = new();
    private string? _selectedSlotKey;
    private PanelIntent _currentIntent;

    public event Action<string, string>? SaveSelectedRequested;
    public event Action<string>? CreateSlotRequested;
    public event Action<string, string>? CopySlotRequested;
    public event Action<string>? LoadSelectedRequested;
    public event Action<string, string>? RenameRequested;
    public event Action<string>? DeleteRequested;
    public event Action? RefreshRequested;

    public override void _Ready()
    {
        _dialog = GetNode<PanelContainer>("CenterLayer/Dialog");
        _slotList = GetNode<ItemList>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlotList");
        _slotListTitle = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlotListTitle");
        _modeLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ModeLabel");
        _slotDetailLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/SlotDetailLabel");
        _previewFrame = GetNode<PanelContainer>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/PreviewFrame");
        _previewTexture = GetNode<TextureRect>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/PreviewFrame/PreviewMargin/PreviewColumn/PreviewTexture");
        _previewHintLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/PreviewFrame/PreviewMargin/PreviewColumn/PreviewHintLabel");
        _slotNameEdit = GetNode<LineEdit>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/NameRow/SlotNameEdit");
        _filterOptionButton = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/FilterRow/FilterOptionButton");
        _sortOptionButton = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/FilterRow/SortOptionButton");
        _saveSelectedButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowPrimary/SaveSelectedButton");
        _loadSelectedButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowPrimary/LoadSelectedButton");
        _createSlotButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowSecondary/CreateSlotButton");
        _renameSlotButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowSecondary/RenameSlotButton");
        _copySlotButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowSecondary/CopySlotButton");
        _deleteSlotButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowTertiary/DeleteSlotButton");
        _refreshButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ActionRowTertiary/RefreshButton");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _footerCloseButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CloseFooterButton");
        _previewVisualFx = GetNodeOrNull<Node>("PreviewVisualFx");

        InitializeFilterControls();
        InitializePopupHint("CenterLayer/Dialog/Margin/MainColumn/HintLabel");
        BindEvents();
        Hide();
    }

    public void Open(IReadOnlyList<SaveSlotSummary> slots, PanelIntent intent, string? preferredSlotKey = null)
    {
        _currentIntent = intent;
        ApplyIntentText();
        ApplySlots(slots, preferredSlotKey);
        OpenPopup();
        CallPreviewVisualFx("pulse_on_select");
    }

    public void RefreshSlots(IReadOnlyList<SaveSlotSummary> slots, string? preferredSlotKey = null, string? statusMessage = null)
    {
        ApplySlots(slots, preferredSlotKey);
        if (!string.IsNullOrWhiteSpace(statusMessage))
        {
            ShowPopupStatusMessage(statusMessage!);
        }
    }

    public void ClosePanel()
    {
        ClosePopup();
    }

    public override void _Process(double delta)
    {
        TickPopupStatus(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!TryHandlePopupClose(@event))
        {
            return;
        }

        GetViewport().SetInputAsHandled();
    }

    protected override void OnPopupClosing()
    {
        _slotNameEdit.Text = string.Empty;
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return _currentIntent == PanelIntent.Save
            ? "可择旧卷覆写，或题新卷名另录分卷；快速存档仍直入主卷。按 Esc 可合卷。"
            : "可择卷启读，也可顺手誊录副卷或整修卷题；快速读档仍优先读取主卷。按 Esc 可合卷。";
    }

    private void BindEvents()
    {
        _slotList.ItemSelected += OnSlotSelected;
        _filterOptionButton.ItemSelected += OnFilterOptionSelected;
        _sortOptionButton.ItemSelected += OnSortOptionSelected;
        _slotNameEdit.TextChanged += _ => RefreshActionState();
        _saveSelectedButton.Pressed += HandleSaveSelectedPressed;
        _loadSelectedButton.Pressed += HandleLoadSelectedPressed;
        _createSlotButton.Pressed += HandleCreateSlotPressed;
        _renameSlotButton.Pressed += HandleRenameSlotPressed;
        _copySlotButton.Pressed += HandleCopySlotPressed;
        _deleteSlotButton.Pressed += HandleDeleteSlotPressed;
        _refreshButton.Pressed += () => RefreshRequested?.Invoke();
        _closeButton.Pressed += ClosePopup;
        _footerCloseButton.Pressed += ClosePopup;
    }

    private void ApplyIntentText()
    {
        _modeLabel.Text = _currentIntent == PanelIntent.Save
            ? "当前案由：存档。可择旧卷覆写，或题新卷名另录分卷。"
            : "当前案由：读档。请选择欲启读之卷，也可誊录副卷或改题旧卷。";
    }

    private void ApplySlots(IReadOnlyList<SaveSlotSummary> slots, string? preferredSlotKey)
    {
        _allSlots.Clear();
        _allSlots.AddRange(slots);
        RebuildVisibleSlots(preferredSlotKey);
    }

    private string? DetermineSelectedSlotKey(string? preferredSlotKey)
    {
        if (!string.IsNullOrWhiteSpace(preferredSlotKey) && _visibleSlots.Any(slot => slot.SlotKey == preferredSlotKey))
        {
            return preferredSlotKey;
        }

        if (!string.IsNullOrWhiteSpace(_selectedSlotKey) && _visibleSlots.Any(slot => slot.SlotKey == _selectedSlotKey))
        {
            return _selectedSlotKey;
        }

        return _visibleSlots.Count > 0 ? _visibleSlots[0].SlotKey : null;
    }

    private void SelectSlotByKey(string? slotKey)
    {
        _selectedSlotKey = slotKey;

        _slotList.DeselectAll();
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            _slotDetailLabel.Text = BuildEmptyDetailText();
            ClearPreviewDisplay();
            _slotNameEdit.Text = string.Empty;
            return;
        }

        for (var index = 0; index < _visibleSlots.Count; index++)
        {
            if (_visibleSlots[index].SlotKey != slotKey)
            {
                continue;
            }

            _slotList.Select(index);
            UpdateSelectedSlotDisplay(_visibleSlots[index]);
            return;
        }

        _slotDetailLabel.Text = BuildEmptyDetailText();
        ClearPreviewDisplay();
        _slotNameEdit.Text = string.Empty;
    }

    private void UpdateSelectedSlotDisplay(SaveSlotSummary slot)
    {
        var calendarInfo = _calendarSystem.Describe(slot.GameMinutes);
        var saveTypeText = slot.IsAutosave ? "自动卷" : "手卷";
        var warehouseRate = slot.WarehouseCapacity <= 0
            ? 0.0
            : Math.Clamp(slot.WarehouseUsed / slot.WarehouseCapacity * 100.0, 0.0, 999.0);
        _slotDetailLabel.Text =
            $"{slot.SlotName}\n" +
            $"{calendarInfo.DateText} · {calendarInfo.DetailText}\n" +
            $"人口 {slot.Population} · 灵石 {slot.Gold:0} · 科技 T{Math.Max(slot.TechLevel + 1, 1)} · {saveTypeText}\n" +
            $"民心 {slot.Happiness:0.#} · 威胁 {slot.Threat:0.#} · 历练层数 {slot.ExplorationDepth}\n" +
            $"库藏 {slot.WarehouseUsed:0}/{Math.Max(slot.WarehouseCapacity, 1):0} ({warehouseRate:0}%)\n" +
            $"最近落卷：{slot.UpdatedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

        _slotNameEdit.Text = slot.SlotName;
        _slotNameEdit.CaretColumn = _slotNameEdit.Text.Length;
        UpdatePreviewDisplay(slot);
        CallPreviewVisualFx("pulse_on_select");
    }

    private string BuildSlotListText(SaveSlotSummary slot)
    {
        var calendarInfo = _calendarSystem.Describe(slot.GameMinutes);
        var tag = slot.SlotKey switch
        {
            "default" => "主卷",
            _ when slot.IsAutosave => "自录",
            _ => "手卷"
        };
        return $"【{tag}】{slot.SlotName} · {calendarInfo.DateText} · T{Math.Max(slot.TechLevel + 1, 1)} · 民{slot.Happiness:0}/威{slot.Threat:0}";
    }

    private void OnSlotSelected(long index)
    {
        var safeIndex = (int)index;
        if (safeIndex < 0 || safeIndex >= _visibleSlots.Count)
        {
            return;
        }

        _selectedSlotKey = _visibleSlots[safeIndex].SlotKey;
        UpdateSelectedSlotDisplay(_visibleSlots[safeIndex]);
        RefreshActionState();
    }

    private void RefreshActionState()
    {
        var selectedSlot = GetSelectedSlot();
        var hasSelectedSlot = selectedSlot != null;
        var hasNameInput = !string.IsNullOrWhiteSpace(_slotNameEdit.Text);
        var isProtectedSlotSelected = IsProtectedSlot(selectedSlot);

        _saveSelectedButton.Disabled = !hasSelectedSlot || isProtectedSlotSelected;
        _loadSelectedButton.Disabled = !hasSelectedSlot;
        _renameSlotButton.Disabled = !hasSelectedSlot || !hasNameInput || isProtectedSlotSelected;
        _copySlotButton.Disabled = !hasSelectedSlot;
        _deleteSlotButton.Disabled = !hasSelectedSlot || isProtectedSlotSelected;
        _createSlotButton.Disabled = !hasNameInput;
    }

    private SaveSlotSummary? GetSelectedSlot()
    {
        if (string.IsNullOrWhiteSpace(_selectedSlotKey))
        {
            return null;
        }

        return _visibleSlots.FirstOrDefault(slot => slot.SlotKey == _selectedSlotKey);
    }

    private void HandleSaveSelectedPressed()
    {
        var selectedSlot = GetSelectedSlot();
        if (selectedSlot == null)
        {
            ShowPopupStatusMessage("请先择定欲覆写之卷。");
            return;
        }

        SaveSelectedRequested?.Invoke(selectedSlot.SlotKey, selectedSlot.SlotName);
    }

    private void HandleLoadSelectedPressed()
    {
        var selectedSlot = GetSelectedSlot();
        if (selectedSlot == null)
        {
            ShowPopupStatusMessage("请先择定欲启读之卷。");
            return;
        }

        LoadSelectedRequested?.Invoke(selectedSlot.SlotKey);
    }

    private void HandleCreateSlotPressed()
    {
        var slotName = _slotNameEdit.Text.Trim();
        if (string.IsNullOrWhiteSpace(slotName))
        {
            ShowPopupStatusMessage("请先题写新卷之名。");
            return;
        }

        CreateSlotRequested?.Invoke(slotName);
    }

    private void HandleRenameSlotPressed()
    {
        var selectedSlot = GetSelectedSlot();
        if (selectedSlot == null)
        {
            ShowPopupStatusMessage("请先择定欲更题之卷。");
            return;
        }

        var slotName = _slotNameEdit.Text.Trim();
        if (string.IsNullOrWhiteSpace(slotName))
        {
            ShowPopupStatusMessage("请先题写新的卷名。");
            return;
        }

        RenameRequested?.Invoke(selectedSlot.SlotKey, slotName);
    }

    private void HandleDeleteSlotPressed()
    {
        var selectedSlot = GetSelectedSlot();
        if (selectedSlot == null)
        {
            ShowPopupStatusMessage("请先择定欲焚毁之卷。");
            return;
        }

        DeleteRequested?.Invoke(selectedSlot.SlotKey);
    }

    private void HandleCopySlotPressed()
    {
        var selectedSlot = GetSelectedSlot();
        if (selectedSlot == null)
        {
            ShowPopupStatusMessage("请先择定欲誊录之卷。");
            return;
        }

        var targetSlotName = ResolveCopyTargetName(selectedSlot);
        CopySlotRequested?.Invoke(selectedSlot.SlotKey, targetSlotName);
    }

    private void InitializeFilterControls()
    {
        if (_filterOptionButton.ItemCount == 0)
        {
            _filterOptionButton.AddItem("全部卷册");
            _filterOptionButton.AddItem("主卷");
            _filterOptionButton.AddItem("手卷");
            _filterOptionButton.AddItem("自动卷");
        }

        if (_sortOptionButton.ItemCount == 0)
        {
            _sortOptionButton.AddItem("按最近落卷");
            _sortOptionButton.AddItem("按最早落卷");
            _sortOptionButton.AddItem("按宗门进度");
            _sortOptionButton.AddItem("按人口");
            _sortOptionButton.AddItem("按灵石");
            _sortOptionButton.AddItem("按科技");
        }

        _filterOptionButton.Select((int)SlotFilterMode.All);
        _sortOptionButton.Select((int)SlotSortMode.UpdatedDesc);
    }

    private void OnFilterOptionSelected(long index)
    {
        _filterOptionButton.Select((int)index);
        RebuildVisibleSlots(_selectedSlotKey);
    }

    private void OnSortOptionSelected(long index)
    {
        _sortOptionButton.Select((int)index);
        RebuildVisibleSlots(_selectedSlotKey);
    }

    private void RebuildVisibleSlots(string? preferredSlotKey)
    {
        _visibleSlots.Clear();

        var filteredSlots = _allSlots.Where(MatchesActiveFilter);
        _visibleSlots.AddRange(ApplyActiveSort(filteredSlots));

        _slotList.Clear();
        foreach (var slot in _visibleSlots)
        {
            _slotList.AddItem(BuildSlotListText(slot));
        }

        UpdateListTitle();
        var nextSelectedKey = DetermineSelectedSlotKey(preferredSlotKey);
        SelectSlotByKey(nextSelectedKey);
        RefreshActionState();
    }

    private bool MatchesActiveFilter(SaveSlotSummary slot)
    {
        return GetActiveFilterMode() switch
        {
            SlotFilterMode.Primary => string.Equals(slot.SlotKey, "default", StringComparison.Ordinal),
            SlotFilterMode.Manual => !string.Equals(slot.SlotKey, "default", StringComparison.Ordinal) && !slot.IsAutosave,
            SlotFilterMode.Autosave => slot.IsAutosave,
            _ => true
        };
    }

    private IEnumerable<SaveSlotSummary> ApplyActiveSort(IEnumerable<SaveSlotSummary> slots)
    {
        return GetActiveSortMode() switch
        {
            SlotSortMode.UpdatedAsc => slots.OrderBy(slot => slot.UpdatedAtUtc).ThenBy(slot => slot.SlotName, StringComparer.Ordinal),
            SlotSortMode.ProgressDesc => slots.OrderByDescending(slot => slot.GameMinutes).ThenByDescending(slot => slot.UpdatedAtUtc),
            SlotSortMode.PopulationDesc => slots.OrderByDescending(slot => slot.Population).ThenByDescending(slot => slot.GameMinutes),
            SlotSortMode.GoldDesc => slots.OrderByDescending(slot => slot.Gold).ThenByDescending(slot => slot.GameMinutes),
            SlotSortMode.TechDesc => slots.OrderByDescending(slot => slot.TechLevel).ThenByDescending(slot => slot.GameMinutes),
            _ => slots.OrderByDescending(slot => slot.UpdatedAtUtc).ThenBy(slot => slot.SlotName, StringComparer.Ordinal)
        };
    }

    private SlotFilterMode GetActiveFilterMode()
    {
        return Enum.IsDefined(typeof(SlotFilterMode), _filterOptionButton.Selected)
            ? (SlotFilterMode)_filterOptionButton.Selected
            : SlotFilterMode.All;
    }

    private SlotSortMode GetActiveSortMode()
    {
        return Enum.IsDefined(typeof(SlotSortMode), _sortOptionButton.Selected)
            ? (SlotSortMode)_sortOptionButton.Selected
            : SlotSortMode.UpdatedDesc;
    }

    private void UpdateListTitle()
    {
        _slotListTitle.Text = $"卷册目录（{_visibleSlots.Count}/{_allSlots.Count}）";
    }

    private string BuildEmptyDetailText()
    {
        if (_allSlots.Count == 0)
        {
            return "暂未立卷。可先题写卷名，再将当前宗门进度收录成卷。";
        }

        return "当前筛选条件下暂无可阅卷册。可切换筛选后再查，或直接题名另录新卷。";
    }

    private void UpdatePreviewDisplay(SaveSlotSummary slot)
    {
        if (TryLoadPreviewTexture(slot, out var previewTexture))
        {
            _previewTexture.Texture = previewTexture;
            _previewHintLabel.Visible = false;
            CallPreviewVisualFx("transition_to_preview");
            return;
        }

        _previewTexture.Texture = null;
        _previewHintLabel.Text = "暂无线索留影。待卷册写成后，会在此显出当时景象。";
        _previewHintLabel.Visible = true;
        CallPreviewVisualFx("transition_to_empty");
    }

    private void ClearPreviewDisplay()
    {
        _previewTexture.Texture = null;
        _previewHintLabel.Text = _allSlots.Count == 0
            ? "暂无线索留影。待任意卷册写成后，会在此显出当时景象。"
            : "当前所筛卷册暂无可阅留影。";
        _previewHintLabel.Visible = true;
        CallPreviewVisualFx("transition_to_empty");
    }

    private void CallPreviewVisualFx(string methodName, params Variant[] args)
    {
        _previewVisualFx?.Call(methodName, args);
    }

    private static bool TryLoadPreviewTexture(SaveSlotSummary slot, out Texture2D? previewTexture)
    {
        previewTexture = null;
        if (string.IsNullOrWhiteSpace(slot.PreviewImagePath) || !File.Exists(slot.PreviewImagePath))
        {
            return false;
        }

        var image = new Image();
        if (image.Load(slot.PreviewImagePath) != Error.Ok || image.GetWidth() <= 0 || image.GetHeight() <= 0)
        {
            return false;
        }

        previewTexture = ImageTexture.CreateFromImage(image);
        return previewTexture != null;
    }

    private string ResolveCopyTargetName(SaveSlotSummary selectedSlot)
    {
        var requestedName = _slotNameEdit.Text.Trim();
        if (string.IsNullOrWhiteSpace(requestedName) ||
            string.Equals(requestedName, selectedSlot.SlotName, StringComparison.Ordinal))
        {
            return $"{selectedSlot.SlotName} 副卷";
        }

        return requestedName;
    }

    private static bool IsProtectedSlot(SaveSlotSummary? slot)
    {
        if (slot == null)
        {
            return false;
        }

        return string.Equals(slot.SlotKey, "default", StringComparison.Ordinal) || slot.IsAutosave;
    }

}
