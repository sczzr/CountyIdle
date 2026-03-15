using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class WarehousePanel : PopupPanelBase
{
	private enum InventoryTab
	{
		All,
		Basic,
		Materials,
		Crafted
	}

	private enum ResourceGroup
	{
		Basic,
		Materials,
		Crafted
	}

	private sealed record ResourceSlotDefinition(
		string InventoryKey,
		string DisplayName,
		string FallbackGlyph,
		string Description,
		Color AccentColor,
		ResourceGroup Group,
		string? TexturePath);

	private sealed class ResourceSlotBinding
	{
		public ResourceSlotBinding(
			ResourceSlotDefinition definition,
			PanelContainer card,
			PanelContainer token,
			Label tokenGlyph,
			Label nameLabel,
			Label typeLabel,
			Label amountLabel)
		{
			Definition = definition;
			Card = card;
			Token = token;
			TokenGlyph = tokenGlyph;
			NameLabel = nameLabel;
			TypeLabel = typeLabel;
			AmountLabel = amountLabel;
		}

		public ResourceSlotDefinition Definition { get; }
		public PanelContainer Card { get; }
		public PanelContainer Token { get; }
		public Label TokenGlyph { get; }
		public Label NameLabel { get; }
		public Label TypeLabel { get; }
		public Label AmountLabel { get; }
	}

	private const float InventoryCardMinWidth = 240f;
	private const int InventoryMinColumns = 2;
	private const int InventoryMaxColumns = 6;

	private static readonly ResourceSlotDefinition[] ResourceSlots =
	[
		new(nameof(GameState.Food), MaterialSemanticRules.GetDisplayName(nameof(GameState.Food)), "🌾", MaterialSemanticRules.GetDescription(nameof(GameState.Food)), new Color(0.86f, 0.73f, 0.24f), ResourceGroup.Basic, "res://assets/ui/materials/food.png"),
		new(nameof(GameState.Gold), MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold)), "🟡", MaterialSemanticRules.GetDescription(nameof(GameState.Gold)), new Color(0.96f, 0.74f, 0.20f), ResourceGroup.Basic, "res://assets/ui/materials/gold.png"),
		new(nameof(GameState.ContributionPoints), MaterialSemanticRules.GetDisplayName(nameof(GameState.ContributionPoints)), "🏅", MaterialSemanticRules.GetDescription(nameof(GameState.ContributionPoints)), new Color(0.72f, 0.68f, 0.95f), ResourceGroup.Basic, null),
		new(nameof(GameState.Wood), MaterialSemanticRules.GetDisplayName(nameof(GameState.Wood)), "🪵", MaterialSemanticRules.GetDescription(nameof(GameState.Wood)), new Color(0.62f, 0.42f, 0.22f), ResourceGroup.Basic, "res://assets/ui/materials/wood.png"),
		new(nameof(GameState.Stone), MaterialSemanticRules.GetDisplayName(nameof(GameState.Stone)), "🪨", MaterialSemanticRules.GetDescription(nameof(GameState.Stone)), new Color(0.60f, 0.64f, 0.70f), ResourceGroup.Basic, "res://assets/ui/materials/stone.png"),
		new(nameof(GameState.IndustryTools), MaterialSemanticRules.GetDisplayName(nameof(GameState.IndustryTools)), "🛠", MaterialSemanticRules.GetDescription(nameof(GameState.IndustryTools)), new Color(0.74f, 0.80f, 0.86f), ResourceGroup.Basic, "res://assets/ui/materials/industry_tools.png"),
		new(nameof(GameState.Timber), MaterialSemanticRules.GetDisplayName(nameof(GameState.Timber)), "🌲", MaterialSemanticRules.GetDescription(nameof(GameState.Timber)), new Color(0.36f, 0.59f, 0.29f), ResourceGroup.Materials, "res://assets/ui/materials/timber.png"),
		new(nameof(GameState.RawStone), MaterialSemanticRules.GetDisplayName(nameof(GameState.RawStone)), "🪨", MaterialSemanticRules.GetDescription(nameof(GameState.RawStone)), new Color(0.47f, 0.52f, 0.59f), ResourceGroup.Materials, "res://assets/ui/materials/raw_stone.png"),
		new(nameof(GameState.Clay), MaterialSemanticRules.GetDisplayName(nameof(GameState.Clay)), "🧱", MaterialSemanticRules.GetDescription(nameof(GameState.Clay)), new Color(0.79f, 0.48f, 0.30f), ResourceGroup.Materials, "res://assets/ui/materials/clay.png"),
		new(nameof(GameState.Brine), MaterialSemanticRules.GetDisplayName(nameof(GameState.Brine)), "💧", MaterialSemanticRules.GetDescription(nameof(GameState.Brine)), new Color(0.36f, 0.72f, 0.88f), ResourceGroup.Materials, "res://assets/ui/materials/brine.png"),
		new(nameof(GameState.Herbs), MaterialSemanticRules.GetDisplayName(nameof(GameState.Herbs)), "🌿", MaterialSemanticRules.GetDescription(nameof(GameState.Herbs)), new Color(0.29f, 0.71f, 0.39f), ResourceGroup.Materials, "res://assets/ui/materials/herbs.png"),
		new(nameof(GameState.HempFiber), MaterialSemanticRules.GetDisplayName(nameof(GameState.HempFiber)), "🧶", MaterialSemanticRules.GetDescription(nameof(GameState.HempFiber)), new Color(0.62f, 0.78f, 0.42f), ResourceGroup.Materials, "res://assets/ui/materials/hemp_fiber.png"),
		new(nameof(GameState.Reeds), MaterialSemanticRules.GetDisplayName(nameof(GameState.Reeds)), "🎋", MaterialSemanticRules.GetDescription(nameof(GameState.Reeds)), new Color(0.70f, 0.79f, 0.34f), ResourceGroup.Materials, "res://assets/ui/materials/reeds.png"),
		new(nameof(GameState.Hides), MaterialSemanticRules.GetDisplayName(nameof(GameState.Hides)), "🐾", MaterialSemanticRules.GetDescription(nameof(GameState.Hides)), new Color(0.72f, 0.58f, 0.40f), ResourceGroup.Materials, "res://assets/ui/materials/hides.png"),
		new(nameof(GameState.IronOre), MaterialSemanticRules.GetDisplayName(nameof(GameState.IronOre)), "⛏", MaterialSemanticRules.GetDescription(nameof(GameState.IronOre)), new Color(0.58f, 0.63f, 0.72f), ResourceGroup.Materials, "res://assets/ui/materials/iron_ore.png"),
		new(nameof(GameState.CopperOre), MaterialSemanticRules.GetDisplayName(nameof(GameState.CopperOre)), "⛏", MaterialSemanticRules.GetDescription(nameof(GameState.CopperOre)), new Color(0.84f, 0.47f, 0.23f), ResourceGroup.Materials, "res://assets/ui/materials/copper_ore.png"),
		new(nameof(GameState.Coal), MaterialSemanticRules.GetDisplayName(nameof(GameState.Coal)), "🔥", MaterialSemanticRules.GetDescription(nameof(GameState.Coal)), new Color(0.29f, 0.31f, 0.35f), ResourceGroup.Materials, "res://assets/ui/materials/coal.png"),
		new(nameof(GameState.RareMaterial), MaterialSemanticRules.GetDisplayName(nameof(GameState.RareMaterial)), "💎", MaterialSemanticRules.GetDescription(nameof(GameState.RareMaterial)), new Color(0.68f, 0.48f, 0.93f), ResourceGroup.Materials, "res://assets/ui/materials/rare_material.png"),
		new(nameof(GameState.CopperIngot), MaterialSemanticRules.GetDisplayName(nameof(GameState.CopperIngot)), "🔶", MaterialSemanticRules.GetDescription(nameof(GameState.CopperIngot)), new Color(0.84f, 0.56f, 0.31f), ResourceGroup.Materials, "res://assets/ui/materials/copper_ingot.png"),
		new(nameof(GameState.WroughtIron), MaterialSemanticRules.GetDisplayName(nameof(GameState.WroughtIron)), "⚙", MaterialSemanticRules.GetDescription(nameof(GameState.WroughtIron)), new Color(0.66f, 0.74f, 0.79f), ResourceGroup.Materials, "res://assets/ui/materials/wrought_iron.png"),
		new(nameof(GameState.CompositeMaterial), MaterialSemanticRules.GetDisplayName(nameof(GameState.CompositeMaterial)), "🔷", MaterialSemanticRules.GetDescription(nameof(GameState.CompositeMaterial)), new Color(0.33f, 0.78f, 0.79f), ResourceGroup.Materials, "res://assets/ui/materials/composite_material.png"),
		new(nameof(GameState.FineSalt), MaterialSemanticRules.GetDisplayName(nameof(GameState.FineSalt)), "🧂", MaterialSemanticRules.GetDescription(nameof(GameState.FineSalt)), new Color(0.93f, 0.95f, 0.97f), ResourceGroup.Crafted, "res://assets/ui/materials/fine_salt.png"),
		new(nameof(GameState.HerbalMedicine), MaterialSemanticRules.GetDisplayName(nameof(GameState.HerbalMedicine)), "⚗", MaterialSemanticRules.GetDescription(nameof(GameState.HerbalMedicine)), new Color(0.42f, 0.84f, 0.59f), ResourceGroup.Crafted, "res://assets/ui/materials/herbal_medicine.png"),
		new(nameof(GameState.HempCloth), MaterialSemanticRules.GetDisplayName(nameof(GameState.HempCloth)), "🧵", MaterialSemanticRules.GetDescription(nameof(GameState.HempCloth)), new Color(0.71f, 0.72f, 0.57f), ResourceGroup.Crafted, "res://assets/ui/materials/hemp_cloth.png"),
		new(nameof(GameState.Leather), MaterialSemanticRules.GetDisplayName(nameof(GameState.Leather)), "🥾", MaterialSemanticRules.GetDescription(nameof(GameState.Leather)), new Color(0.62f, 0.44f, 0.29f), ResourceGroup.Crafted, "res://assets/ui/materials/leather.png"),
		new(nameof(GameState.IndustrialParts), MaterialSemanticRules.GetDisplayName(nameof(GameState.IndustrialParts)), "⚙", MaterialSemanticRules.GetDescription(nameof(GameState.IndustrialParts)), new Color(0.47f, 0.78f, 0.95f), ResourceGroup.Crafted, "res://assets/ui/materials/industrial_parts.png"),
		new(nameof(GameState.ConstructionMaterials), MaterialSemanticRules.GetDisplayName(nameof(GameState.ConstructionMaterials)), "🏗", MaterialSemanticRules.GetDescription(nameof(GameState.ConstructionMaterials)), new Color(0.85f, 0.52f, 0.38f), ResourceGroup.Crafted, "res://assets/ui/materials/construction_materials.png")
	];

	private Label _hintLabel = null!;
	private Label _warehouseStatusValue = null!;
	private Label _capacityValueLabel = null!;
	private ScrollContainer _inventoryScroll = null!;
	private GridContainer _inventoryGrid = null!;
	private PanelContainer _resourceSlotTemplate = null!;
	private Label _tierZeroChainStatusValue = null!;
	private Button _allTabButton = null!;
	private Button _basicTabButton = null!;
	private Button _materialsTabButton = null!;
	private Button _craftedTabButton = null!;
	private Button _closeButton = null!;
	private Button _lockedForgeButton = null!;
	private Button _upgradeButton = null!;
	private Button _craftToolsButton = null!;
	private Button _buildWorkshopButton = null!;
	private Button _buildAdministrationButton = null!;
	private Button _buildForestryChainButton = null!;
	private Button _buildMasonryChainButton = null!;
	private Button _buildMedicinalChainButton = null!;
	private Button _buildFiberChainButton = null!;
	private Node? _visualFx;
	private readonly List<ResourceSlotBinding> _slotBindings = new();
	private readonly Dictionary<string, Texture2D?> _textureCache = new();
	private InventoryTab _activeTab = InventoryTab.All;
	private GameState? _latestState;
	private double _warehouseLoadRate;

	public event Action? UpgradeMineWarehouseRequested;
	public event Action? CraftToolsRequested;
	public event Action? BuildWorkshopRequested;
	public event Action? BuildAdministrationRequested;
	public event Action? BuildForestryChainRequested;
	public event Action? BuildMasonryChainRequested;
	public event Action? BuildMedicinalChainRequested;
	public event Action? BuildFiberChainRequested;

	public override void _Ready()
	{
		_hintLabel = GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/StatusRow/StatusTextColumn/HintLabel");
		_warehouseStatusValue = GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/StatusRow/StatusTextColumn/WarehouseStatusValue");
		_capacityValueLabel = GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/StatusRow/CapacityValueLabel");
		_inventoryScroll = GetNode<ScrollContainer>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/InventoryScroll");
		_inventoryGrid = GetNode<GridContainer>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/InventoryScroll/InventoryGrid");
		_resourceSlotTemplate = GetNode<PanelContainer>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/ResourceSlotTemplate");
		_tierZeroChainStatusValue = GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/ChainInfoFrame/ChainInfoMargin/TierZeroStatusValue");
		_allTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/AllTabButton");
		_basicTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/BasicTabButton");
		_materialsTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/MaterialsTabButton");
		_craftedTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/CraftedTabButton");
		_closeButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/HeaderRow/CloseButton");
		_lockedForgeButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ManufactureSection/LockedForgeButton");
		_upgradeButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/BuildSection/UpgradeButton");
		_craftToolsButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ManufactureSection/CraftToolsButton");
		_buildWorkshopButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/BuildSection/BuildWorkshopButton");
		_buildAdministrationButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/BuildSection/BuildAdministrationButton");
		_buildForestryChainButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/BuildForestryChainButton");
		_buildMasonryChainButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/BuildMasonryChainButton");
		_buildMedicinalChainButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/BuildMedicinalChainButton");
		_buildFiberChainButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/BuildFiberChainButton");
		_visualFx = GetNodeOrNull<Node>("VisualFx");

		_resourceSlotTemplate.Visible = false;

		BuildInventoryGrid();
		RefreshTabStyles();
		UpdateInventoryColumns();
		InitializePopupHint(_hintLabel);
		BindEvents();
		Hide();
	}

	public void Open(GameState state)
	{
		RefreshState(state);
		UpdateInventoryColumns();
		OpenPopup();
		CallVisualFx("play_open");
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

	public override void _Notification(int what)
	{
		if (what == NotificationResized)
		{
			UpdateInventoryColumns();
		}
	}

	public void RefreshState(GameState state)
	{
		MaterialRules.EnsureDefaults(state);
		_latestState = state;

		var used = state.GetWarehouseUsed();
		var capacity = Math.Max(state.WarehouseCapacity, 1.0);
		_warehouseLoadRate = used / capacity * 100.0;

		_warehouseStatusValue.Text = $"仓储 {ToChineseTier(state.WarehouseLevel)}级 · 矿坑 {ToChineseTier(state.MiningLevel)}级";
		_warehouseStatusValue.TooltipText = $"仓储负载 {_warehouseLoadRate:0}%";
		_capacityValueLabel.Text = $"已占 {used:0} / {capacity:0}";
		_capacityValueLabel.TooltipText = $"剩余容量 {Math.Max(capacity - used, 0):0}";

		CallVisualFx("apply_capacity_visual", _warehouseLoadRate);
		RefreshInventoryState(state);
		RefreshActionButtons(state);
		RefreshPopupHint();
	}

	private void BindEvents()
	{
		_closeButton.Pressed += ClosePopup;
		_allTabButton.Pressed += () => SwitchTab(InventoryTab.All);
		_basicTabButton.Pressed += () => SwitchTab(InventoryTab.Basic);
		_materialsTabButton.Pressed += () => SwitchTab(InventoryTab.Materials);
		_craftedTabButton.Pressed += () => SwitchTab(InventoryTab.Crafted);
		_upgradeButton.Pressed += () => HandleWarehouseAction("已批复矿仓联建，请留意库容与矿脉账目变化。", UpgradeMineWarehouseRequested);
		_craftToolsButton.Pressed += () => HandleWarehouseAction("已批红锻制工器，请查验工器余量与工坊记录。", CraftToolsRequested);
		_buildWorkshopButton.Pressed += () => HandleWarehouseAction($"已批复扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}，请留意土木账目。", BuildWorkshopRequested);
		_buildAdministrationButton.Pressed += () => HandleWarehouseAction($"已批复扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)}，请留意度支消耗。", BuildAdministrationRequested);
		_buildForestryChainButton.Pressed += () => HandleWarehouseAction("已改定灵木链章程，请留意灵木与灵木料盈缺。", BuildForestryChainRequested);
		_buildMasonryChainButton.Pressed += () => HandleWarehouseAction("已改定石陶链章程，请留意青罡石料与护山构件盈缺。", BuildMasonryChainRequested);
		_buildMedicinalChainButton.Pressed += () => HandleWarehouseAction("已改定盐丹链章程，请留意灵草、卤水与丹散出入。", BuildMedicinalChainRequested);
		_buildFiberChainButton.Pressed += () => HandleWarehouseAction("已改定织裘链章程，请留意麻料、皮裘与袍服出入。", BuildFiberChainRequested);
	}

	protected override string GetPopupHintText()
	{
		if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
		{
			return PopupStatusMessage!;
		}

		return _warehouseLoadRate switch
		{
			>= 100.0 => "仓廪将满，亟需扩建或消耗度支。",
			>= 90.0 => "库藏偏满，宜先批复矿仓联建。",
			>= 70.0 => "库藏尚丰，可提前整顿工坊与产线。",
			_ => "翻阅账册可查全览、农桑、金石与百工诸项。"
		};
	}

	private void SwitchTab(InventoryTab tab)
	{
		if (_activeTab == tab)
		{
			return;
		}

		_activeTab = tab;
		RefreshTabStyles();
		BuildInventoryGrid();
		if (_latestState != null)
		{
			RefreshInventoryState(_latestState);
		}

		CallVisualFx("play_tab_switch", tab.ToString());
	}

	private void CallVisualFx(string methodName, params Variant[] args)
	{
		_visualFx?.Call(methodName, args);
	}

	private void RefreshTabStyles()
	{
		CallVisualFx("apply_tab_button_state", _activeTab.ToString());
	}

	private void UpdateInventoryColumns()
	{
		if (_inventoryScroll == null || _inventoryGrid == null)
		{
			return;
		}

		var availableWidth = _inventoryScroll.Size.X;
		if (availableWidth <= 0f)
		{
			return;
		}

		var columns = Mathf.Clamp(
			(int)Mathf.Floor(availableWidth / InventoryCardMinWidth),
			InventoryMinColumns,
			InventoryMaxColumns);

		if (_inventoryGrid.Columns != columns)
		{
			_inventoryGrid.Columns = columns;
		}
	}

	private void BuildInventoryGrid()
	{
		foreach (var child in _inventoryGrid.GetChildren())
		{
			child.QueueFree();
		}

		_slotBindings.Clear();
		var visibleSlots = GetVisibleSlots(_activeTab);
		foreach (var slot in visibleSlots)
		{
			_inventoryGrid.AddChild(CreateResourceSlot(slot));
		}
	}

	private void RefreshInventoryState(GameState state)
	{
		foreach (var binding in _slotBindings)
		{
			var amount = InventoryRules.GetVisibleAmount(state, binding.Definition.InventoryKey);
			var hasAmount = amount > 0;
			binding.AmountLabel.Text = amount.ToString("N0");
			binding.Card.TooltipText = $"{binding.Definition.DisplayName} × {amount:N0}\n{binding.Definition.Description}";
			CallVisualFx(
				"apply_resource_slot_state",
				binding.Card,
				binding.Token,
				binding.TokenGlyph,
				binding.NameLabel,
				binding.TypeLabel,
				binding.AmountLabel,
				hasAmount);
		}
	}

	private void RefreshActionButtons(GameState state)
	{
		_tierZeroChainStatusValue.Text =
			$"当前运作纲要：\n灵材：灵木链 {ToChineseTier(state.ForestryChainLevel)}阶 · 石陶链 {ToChineseTier(state.MasonryChainLevel)}阶\n" +
			$"丹鼎：盐丹链 {ToChineseTier(state.MedicinalChainLevel)}阶 · 织裘链 {ToChineseTier(state.FiberChainLevel)}阶";

		_upgradeButton.Disabled = state.TechLevel < 1;
		_upgradeButton.Text = state.TechLevel >= 1
			? $"矿仓联建 · 矿仓 {ToChineseTier(state.MiningLevel)} / 库藏 {ToChineseTier(state.WarehouseLevel)}"
			: "矿仓联建（未启）";
		_upgradeButton.TooltipText = state.TechLevel >= 1
			? "同时提升矿坑与库藏等级。"
			: "需先掌握【锻造术 壹阶】。";

		_lockedForgeButton.TooltipText = "待后续法器体系接入后开放。";
		_craftToolsButton.Text = $"锻制工器 · 余 {InventoryRules.GetVisibleAmount(state, nameof(GameState.IndustryTools)):N0}";
		_craftToolsButton.TooltipText = "消耗熟铁、铜锭等材料，补充工器库存。";

		_buildWorkshopButton.Text = $"扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} · {ToChineseTier(state.WorkshopBuildings)}级";
		_buildAdministrationButton.Text = $"扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} · {ToChineseTier(state.AdministrationBuildings)}级";
		_buildForestryChainButton.Text = $"配置 灵木链 · {ToChineseTier(state.ForestryChainLevel)}阶";
		_buildMasonryChainButton.Text = $"配置 石陶链 · {ToChineseTier(state.MasonryChainLevel)}阶";
		_buildMedicinalChainButton.Text = $"配置 盐丹链 · {ToChineseTier(state.MedicinalChainLevel)}阶";
		_buildFiberChainButton.Text = $"配置 织裘链 · {ToChineseTier(state.FiberChainLevel)}阶";
	}

	private Control CreateResourceSlot(ResourceSlotDefinition slot)
	{
		var card = (PanelContainer)_resourceSlotTemplate.Duplicate();
		card.Visible = true;

		var token = card.GetNode<PanelContainer>("SlotMargin/SlotRow/Token");
		var tokenCenter = card.GetNode<CenterContainer>("SlotMargin/SlotRow/Token/TokenCenter");
		var tokenGlyph = card.GetNode<Label>("SlotMargin/SlotRow/Token/TokenCenter/TokenGlyph");
		var nameLabel = card.GetNode<Label>("SlotMargin/SlotRow/InfoColumn/NameLabel");
		var typeLabel = card.GetNode<Label>("SlotMargin/SlotRow/InfoColumn/TypeLabel");
		var amountLabel = card.GetNode<Label>("SlotMargin/SlotRow/AmountLabel");

		tokenCenter.MouseFilter = MouseFilterEnum.Ignore;
		tokenGlyph.MouseFilter = MouseFilterEnum.Ignore;
		tokenGlyph.Text = GetTokenGlyph(slot);

		nameLabel.Text = slot.DisplayName;
		typeLabel.Text = GetGroupLabel(slot.Group);
		amountLabel.Text = "0";

		CallVisualFx(
			"style_resource_slot",
			card,
			token,
			tokenGlyph,
			nameLabel,
			typeLabel,
			amountLabel,
			slot.AccentColor);

		_slotBindings.Add(new ResourceSlotBinding(slot, card, token, tokenGlyph, nameLabel, typeLabel, amountLabel));
		return card;
	}

	private IReadOnlyList<ResourceSlotDefinition> GetVisibleSlots(InventoryTab tab)
	{
		return tab switch
		{
			InventoryTab.Basic => ResourceSlots.Where(static slot => slot.Group == ResourceGroup.Basic).ToArray(),
			InventoryTab.Materials => ResourceSlots.Where(static slot => slot.Group == ResourceGroup.Materials).ToArray(),
			InventoryTab.Crafted => ResourceSlots.Where(static slot => slot.Group == ResourceGroup.Crafted).ToArray(),
			_ => ResourceSlots
		};
	}

	private static string GetTokenGlyph(ResourceSlotDefinition slot)
	{
		var name = slot.DisplayName?.Trim();
		if (!string.IsNullOrWhiteSpace(name))
		{
			var enumerator = StringInfo.GetTextElementEnumerator(name);
			if (enumerator.MoveNext())
			{
				return enumerator.GetTextElement();
			}
		}

		return string.IsNullOrWhiteSpace(slot.FallbackGlyph) ? "?" : slot.FallbackGlyph;
	}

	private static string GetGroupLabel(ResourceGroup group)
	{
		return group switch
		{
			ResourceGroup.Basic => "农桑 · 基础",
			ResourceGroup.Materials => "金石 · 资材",
			ResourceGroup.Crafted => "百工 · 造物",
			_ => "库藏 · 综列"
		};
	}

	private Texture2D? TryLoadTexture(string? texturePath)
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			return null;
		}

		if (_textureCache.TryGetValue(texturePath, out var cachedTexture))
		{
			return cachedTexture;
		}

		var texture = GD.Load<Texture2D>(texturePath);
		_textureCache[texturePath] = texture;
		return texture;
	}

	private void HandleWarehouseAction(string statusMessage, Action? requestedAction)
	{
		ShowPopupStatusMessage(statusMessage);
		requestedAction?.Invoke();
	}

	private static string ToChineseTier(int value)
	{
		return value switch
		{
			<= 0 => "零",
			1 => "壹",
			2 => "贰",
			3 => "叁",
			4 => "肆",
			5 => "伍",
			6 => "陆",
			7 => "柒",
			8 => "捌",
			9 => "玖",
			_ => value.ToString()
		};
	}
}
