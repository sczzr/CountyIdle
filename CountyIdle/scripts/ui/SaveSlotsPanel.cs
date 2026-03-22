using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

/// <summary>
/// 留影录主面板：负责卷册筛选、玉简列表选择、卷轴详情展示与存读档动作转发。
/// </summary>
public partial class SaveSlotsPanel : PopupPanelBase
{
    private const string SlipItemScenePath = "res://scenes/ui/components/SaveJadeSlipItem.tscn";

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

    private PackedScene? _slipItemScene;
    private PanelContainer _dialog = null!;
    private PanelContainer _previewFrame = null!;
    private VBoxContainer _slipShelf = null!;
    private Label _slotListTitle = null!;
    private Label _modeLabel = null!;
    private Label _slotTitleLabel = null!;
    private Label _slotUpdatedLabel = null!;
    private Label _slotDetailLabel = null!;
    private Label _slotUpdatedDetailLabel = null!;
    private Label _populationStatValueLabel = null!;
    private Label _goldStatValueLabel = null!;
    private Label _explorationStatValueLabel = null!;
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
    private readonly List<SaveJadeSlipItem> _slipCards = new();
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
        _slipItemScene = GD.Load<PackedScene>(SlipItemScenePath);
        _dialog = GetNode<PanelContainer>("CenterLayer/Dialog");
        _slipShelf = GetNode<VBoxContainer>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlipShelfFrame/SlipShelfMargin/SlipShelfScroll/SlipShelf");
        _slotListTitle = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/SlotListTitle");
        _modeLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ModeLabel");
        _slotTitleLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ScrollTitleRow/ScrollTitleStack/SlotTitleLabel");
        _slotUpdatedLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ScrollTitleRow/ScrollTitleStack/SlotUpdatedLabel");
        _slotDetailLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/DetailBody/SlotDetailLabel");
        _slotUpdatedDetailLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/DetailBody/SlotUpdatedDetailLabel");
        _populationStatValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/PopulationStatCard/PopulationStatValueLabel");
        _goldStatValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/GoldStatCard/GoldStatValueLabel");
        _explorationStatValueLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/StatRow/ExplorationStatCard/ExplorationStatValueLabel");
        _previewFrame = GetNode<PanelContainer>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame");
        _previewTexture = GetNode<TextureRect>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame/PreviewMargin/PreviewColumn/PreviewTexture");
        _previewHintLabel = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/PreviewFrame/PreviewMargin/PreviewColumn/PreviewHintLabel");
        _slotNameEdit = GetNode<LineEdit>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/NameRow/SlotNameEdit");
        _filterOptionButton = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/FilterRow/FilterOptionButton");
        _sortOptionButton = GetNode<OptionButton>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/SlotColumn/FilterRow/SortOptionButton");
        _saveSelectedButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary/SaveSelectedButton");
        _loadSelectedButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary/LoadSelectedButton");
        _createSlotButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowSecondary/CreateSlotButton");
        _renameSlotButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/NameRow/RenameSlotButton");
        _copySlotButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary/CopySlotButton");
        _deleteSlotButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowPrimary/DeleteSlotButton");
        _refreshButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ContentRow/DetailColumn/ScrollFrame/ScrollMargin/ScrollColumn/ActionRowSecondary/RefreshButton");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _footerCloseButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CloseFooterButton");
        _previewVisualFx = GetNodeOrNull<Node>("PreviewVisualFx");

        InitializeFilterControls();
        InitializePopupHint("CenterLayer/Dialog/Margin/MainColumn/HintLabel");
        BindEvents();
        ApplyStaticButtonText();
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
            ? "检阅玉简，可择旧卷旧梦重写，或另拓副卷以分岔前缘。按 Esc 可合卷。"
            : "检阅玉简，可开启前尘，也可顺手另拓副卷与重题卷名。按 Esc 可合卷。";
    }

    private void BindEvents()
    {
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

    private void ApplyStaticButtonText()
    {
        _saveSelectedButton.Text = "覆写";
        _loadSelectedButton.Text = "启读";
        _createSlotButton.Text = "新卷";
        _renameSlotButton.Text = "修改卷名";
        _copySlotButton.Text = "另拓";
        _deleteSlotButton.Text = "焚毁";
        _refreshButton.Text = "重整";
        _footerCloseButton.Text = "合卷";
    }

    private void ApplyIntentText()
    {
        _modeLabel.Text = _currentIntent == PanelIntent.Save
            ? "当前案由：旧梦重写。可择旧卷覆写，亦可题新卷名另拓分卷。"
            : "当前案由：开启前尘。请选择欲启读之卷，也可誊录副卷或整修卷题。";
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
        UpdateSlipSelectionVisuals();

        if (string.IsNullOrWhiteSpace(slotKey))
        {
            ApplyEmptyDetailState();
            _slotNameEdit.Text = string.Empty;
            RefreshActionState();
            return;
        }

        var selectedSlot = _visibleSlots.FirstOrDefault(slot => slot.SlotKey == slotKey);
        if (selectedSlot != null)
        {
            UpdateSelectedSlotDisplay(selectedSlot);
            RefreshActionState();
            return;
        }

        ApplyEmptyDetailState();
        _slotNameEdit.Text = string.Empty;
        RefreshActionState();
    }

    private void UpdateSelectedSlotDisplay(SaveSlotSummary slot)
    {
        var calendarInfo = _calendarSystem.Describe(slot.GameMinutes);
        var warehouseRate = slot.WarehouseCapacity <= 0
            ? 0.0
            : Math.Clamp(slot.WarehouseUsed / slot.WarehouseCapacity * 100.0, 0.0, 999.0);
        var displayName = BuildDisplaySlotName(slot);

        _slotTitleLabel.Text = $"卷名：{displayName}";
        _slotUpdatedLabel.Text = $"最近落卷：{slot.UpdatedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
        _slotDetailLabel.Text =
            $"{calendarInfo.DateText} · {calendarInfo.DetailText}\n" +
            $"{BuildSlotBadge(slot)}｜科技 T{Math.Max(slot.TechLevel + 1, 1)}｜民心 {slot.Happiness:0.#}｜威胁 {slot.Threat:0.#}";
        _slotUpdatedDetailLabel.Text = BuildNarrativeSummary(slot, warehouseRate);
        _populationStatValueLabel.Text = $"{slot.Population} · 灵";
        _goldStatValueLabel.Text = $"{slot.Gold:0} · 錠";
        _explorationStatValueLabel.Text = FormatExplorationDepth(slot.ExplorationDepth);

        _slotNameEdit.Text = slot.SlotName;
        _slotNameEdit.CaretColumn = _slotNameEdit.Text.Length;
        UpdatePreviewDisplay(slot);
        CallPreviewVisualFx("pulse_on_select");
    }

    private string BuildNarrativeSummary(SaveSlotSummary slot, double warehouseRate)
    {
        var protectionText = IsProtectedSlot(slot)
            ? "此卷受宗门戒律与天道刻印庇护，不可焚毁亦不可覆写题名。"
            : "此卷可重题卷名、另拓副卷，也可在必要时因果火解。";
        return $"灵石 {slot.Gold:0}、人口 {slot.Population}、库藏负载 {warehouseRate:0}% 共同勾勒此卷气象。{protectionText}";
    }

    private static string BuildDisplaySlotName(SaveSlotSummary slot)
    {
        if (slot.IsAutosave)
        {
            return slot.SlotName.Replace("自动存档", "天道刻印", StringComparison.Ordinal);
        }

        if (string.Equals(slot.SlotKey, "default", StringComparison.Ordinal))
        {
            return $"主卷 · {slot.SlotName}";
        }

        return slot.SlotName;
    }

    private static string BuildSlotBadge(SaveSlotSummary slot)
    {
        if (string.Equals(slot.SlotKey, "default", StringComparison.Ordinal))
        {
            return "本命主卷";
        }

        return slot.IsAutosave ? "天道刻印" : "手录分卷";
    }

    private static string FormatExplorationDepth(int depth)
    {
        return depth <= 0 ? "未入层" : $"{depth}层";
    }

    private void OnSlipCardActivated(string slotKey)
    {
        _selectedSlotKey = slotKey;
        SelectSlotByKey(slotKey);
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
            ShowPopupStatusMessage("请先择定欲旧梦重写之卷。");
            return;
        }

        SaveSelectedRequested?.Invoke(selectedSlot.SlotKey, selectedSlot.SlotName);
    }

    private void HandleLoadSelectedPressed()
    {
        var selectedSlot = GetSelectedSlot();
        if (selectedSlot == null)
        {
            ShowPopupStatusMessage("请先择定欲开启前尘之卷。");
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
            ShowPopupStatusMessage("请先择定欲重题之卷。");
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
            ShowPopupStatusMessage("请先择定欲因果火解之卷。");
            return;
        }

        DeleteRequested?.Invoke(selectedSlot.SlotKey);
    }

    private void HandleCopySlotPressed()
    {
        var selectedSlot = GetSelectedSlot();
        if (selectedSlot == null)
        {
            ShowPopupStatusMessage("请先择定欲另拓之卷。");
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
            _filterOptionButton.AddItem("天道刻印");
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

        RebuildSlipShelf();
        UpdateListTitle();
        var nextSelectedKey = DetermineSelectedSlotKey(preferredSlotKey);
        SelectSlotByKey(nextSelectedKey);
        RefreshActionState();
    }

    private void RebuildSlipShelf()
    {
        foreach (var child in _slipShelf.GetChildren())
        {
            child.QueueFree();
        }

        _slipCards.Clear();

        foreach (var slot in _visibleSlots)
        {
            var slipItem = _slipItemScene?.Instantiate<SaveJadeSlipItem>();
            if (slipItem == null)
            {
                continue;
            }

            slipItem.SetDisplay(
                slot.SlotKey,
                BuildDisplaySlotName(slot),
                BuildSlipSubtitle(slot),
                BuildSlotBadge(slot),
                BuildSlipState(slot),
                string.Equals(slot.SlotKey, _selectedSlotKey, StringComparison.Ordinal));
            slipItem.Activated += OnSlipCardActivated;
            _slipShelf.AddChild(slipItem);
            _slipCards.Add(slipItem);
        }
    }

    private string BuildSlipSubtitle(SaveSlotSummary slot)
    {
        var calendarInfo = _calendarSystem.Describe(slot.GameMinutes);
        return $"{calendarInfo.DateText}｜{slot.UpdatedAtUtc.ToLocalTime():MM-dd HH:mm}";
    }

    private static string BuildSlipState(SaveSlotSummary slot)
    {
        return string.IsNullOrWhiteSpace(slot.PreviewImagePath) ? "暂无留影" : "有留影";
    }

    private void UpdateSlipSelectionVisuals()
    {
        foreach (var slipCard in _slipCards)
        {
            slipCard.SetSelectedState(string.Equals(slipCard.SlotKey, _selectedSlotKey, StringComparison.Ordinal));
        }
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
        _slotListTitle.Text = $"玉简档案架（{_visibleSlots.Count}/{_allSlots.Count}）";
    }

    private void ApplyEmptyDetailState()
    {
        _slotTitleLabel.Text = "卷名：暂无卷册";
        _slotUpdatedLabel.Text = _allSlots.Count == 0 ? "因果未曾成卷" : "当前筛选下暂无可阅卷册";
        _slotDetailLabel.Text = BuildEmptyDetailText();
        _slotUpdatedDetailLabel.Text = _allSlots.Count == 0
            ? "待你第一次落卷后，右侧画屏会记住当时景象与宗门气象。"
            : "可切换筛选、排序，或直接题名另拓新卷。";
        _populationStatValueLabel.Text = "—";
        _goldStatValueLabel.Text = "—";
        _explorationStatValueLabel.Text = "—";
        ClearPreviewDisplay();
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
            _previewTexture.Visible = true;
            _previewHintLabel.Visible = false;
            CallPreviewVisualFx("transition_to_preview");
            return;
        }

        _previewTexture.Texture = null;
        _previewTexture.Visible = false;
        _previewHintLabel.Text = "此卷尚未留下可阅画屏，待下次落卷后会在此映出旧景。";
        _previewHintLabel.Visible = true;
        CallPreviewVisualFx("transition_to_empty");
    }

    private void ClearPreviewDisplay()
    {
        _previewTexture.Texture = null;
        _previewTexture.Visible = false;
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
