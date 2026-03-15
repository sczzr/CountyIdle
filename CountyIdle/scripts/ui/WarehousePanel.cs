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
		Relics,
		Manuals,
		Elixirs,
		Treasures,
		Currency
	}

	private enum ResourceGroup
	{
		Relics,
		Manuals,
		Elixirs,
		Treasures,
		Currency
	}

	private enum ResourceRank
	{
		Heaven,
		Earth,
		Mystic,
		Yellow,
		Mortal
	}

	private sealed record ResourceSlotDefinition(
		string InventoryKey,
		string DisplayName,
		string FallbackGlyph,
		string Description,
		ResourceGroup Group,
		ResourceRank Rank,
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

	private const float InventoryCardMinWidth = 420f;
	private const int InventoryMinColumns = 2;
	private const int InventoryMaxColumns = 2;

private static readonly ResourceSlotDefinition[] ResourceSlots =
[
	new(nameof(GameState.Food), MaterialSemanticRules.GetDisplayName(nameof(GameState.Food)), "🌾", MaterialSemanticRules.GetDescription(nameof(GameState.Food)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/food.png"),
	new(nameof(GameState.Gold), MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold)), "🟡", MaterialSemanticRules.GetDescription(nameof(GameState.Gold)), ResourceGroup.Currency, ResourceRank.Mortal, "res://assets/ui/materials/gold.png"),
	new(nameof(GameState.ContributionPoints), MaterialSemanticRules.GetDisplayName(nameof(GameState.ContributionPoints)), "🏅", MaterialSemanticRules.GetDescription(nameof(GameState.ContributionPoints)), ResourceGroup.Currency, ResourceRank.Mortal, null),
	new(nameof(GameState.Wood), MaterialSemanticRules.GetDisplayName(nameof(GameState.Wood)), "🪵", MaterialSemanticRules.GetDescription(nameof(GameState.Wood)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/wood.png"),
	new(nameof(GameState.Stone), MaterialSemanticRules.GetDisplayName(nameof(GameState.Stone)), "🪨", MaterialSemanticRules.GetDescription(nameof(GameState.Stone)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/stone.png"),
	new(nameof(GameState.IndustryTools), MaterialSemanticRules.GetDisplayName(nameof(GameState.IndustryTools)), "🛠", MaterialSemanticRules.GetDescription(nameof(GameState.IndustryTools)), ResourceGroup.Relics, ResourceRank.Mystic, "res://assets/ui/materials/industry_tools.png"),
	new(nameof(GameState.Timber), MaterialSemanticRules.GetDisplayName(nameof(GameState.Timber)), "🌲", MaterialSemanticRules.GetDescription(nameof(GameState.Timber)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/timber.png"),
	new(nameof(GameState.RawStone), MaterialSemanticRules.GetDisplayName(nameof(GameState.RawStone)), "🪨", MaterialSemanticRules.GetDescription(nameof(GameState.RawStone)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/raw_stone.png"),
	new(nameof(GameState.Clay), MaterialSemanticRules.GetDisplayName(nameof(GameState.Clay)), "🧱", MaterialSemanticRules.GetDescription(nameof(GameState.Clay)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/clay.png"),
	new(nameof(GameState.Brine), MaterialSemanticRules.GetDisplayName(nameof(GameState.Brine)), "💧", MaterialSemanticRules.GetDescription(nameof(GameState.Brine)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/brine.png"),
	new(nameof(GameState.Herbs), MaterialSemanticRules.GetDisplayName(nameof(GameState.Herbs)), "🌿", MaterialSemanticRules.GetDescription(nameof(GameState.Herbs)), ResourceGroup.Elixirs, ResourceRank.Yellow, "res://assets/ui/materials/herbs.png"),
	new(nameof(GameState.HempFiber), MaterialSemanticRules.GetDisplayName(nameof(GameState.HempFiber)), "🧶", MaterialSemanticRules.GetDescription(nameof(GameState.HempFiber)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/hemp_fiber.png"),
	new(nameof(GameState.Reeds), MaterialSemanticRules.GetDisplayName(nameof(GameState.Reeds)), "🎋", MaterialSemanticRules.GetDescription(nameof(GameState.Reeds)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/reeds.png"),
	new(nameof(GameState.Hides), MaterialSemanticRules.GetDisplayName(nameof(GameState.Hides)), "🐾", MaterialSemanticRules.GetDescription(nameof(GameState.Hides)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/hides.png"),
	new(nameof(GameState.IronOre), MaterialSemanticRules.GetDisplayName(nameof(GameState.IronOre)), "⛏", MaterialSemanticRules.GetDescription(nameof(GameState.IronOre)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/iron_ore.png"),
	new(nameof(GameState.CopperOre), MaterialSemanticRules.GetDisplayName(nameof(GameState.CopperOre)), "⛏", MaterialSemanticRules.GetDescription(nameof(GameState.CopperOre)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/copper_ore.png"),
	new(nameof(GameState.Coal), MaterialSemanticRules.GetDisplayName(nameof(GameState.Coal)), "🔥", MaterialSemanticRules.GetDescription(nameof(GameState.Coal)), ResourceGroup.Treasures, ResourceRank.Yellow, "res://assets/ui/materials/coal.png"),
	new(nameof(GameState.RareMaterial), MaterialSemanticRules.GetDisplayName(nameof(GameState.RareMaterial)), "💎", MaterialSemanticRules.GetDescription(nameof(GameState.RareMaterial)), ResourceGroup.Treasures, ResourceRank.Heaven, "res://assets/ui/materials/rare_material.png"),
	new(nameof(GameState.CopperIngot), MaterialSemanticRules.GetDisplayName(nameof(GameState.CopperIngot)), "🔶", MaterialSemanticRules.GetDescription(nameof(GameState.CopperIngot)), ResourceGroup.Relics, ResourceRank.Mystic, "res://assets/ui/materials/copper_ingot.png"),
	new(nameof(GameState.WroughtIron), MaterialSemanticRules.GetDisplayName(nameof(GameState.WroughtIron)), "⚙", MaterialSemanticRules.GetDescription(nameof(GameState.WroughtIron)), ResourceGroup.Relics, ResourceRank.Mystic, "res://assets/ui/materials/wrought_iron.png"),
	new(nameof(GameState.CompositeMaterial), MaterialSemanticRules.GetDisplayName(nameof(GameState.CompositeMaterial)), "🔷", MaterialSemanticRules.GetDescription(nameof(GameState.CompositeMaterial)), ResourceGroup.Manuals, ResourceRank.Earth, "res://assets/ui/materials/composite_material.png"),
	new(nameof(GameState.FineSalt), MaterialSemanticRules.GetDisplayName(nameof(GameState.FineSalt)), "🧂", MaterialSemanticRules.GetDescription(nameof(GameState.FineSalt)), ResourceGroup.Elixirs, ResourceRank.Yellow, "res://assets/ui/materials/fine_salt.png"),
	new(nameof(GameState.HerbalMedicine), MaterialSemanticRules.GetDisplayName(nameof(GameState.HerbalMedicine)), "⚗", MaterialSemanticRules.GetDescription(nameof(GameState.HerbalMedicine)), ResourceGroup.Elixirs, ResourceRank.Mystic, "res://assets/ui/materials/herbal_medicine.png"),
	new(nameof(GameState.HempCloth), MaterialSemanticRules.GetDisplayName(nameof(GameState.HempCloth)), "🧵", MaterialSemanticRules.GetDescription(nameof(GameState.HempCloth)), ResourceGroup.Relics, ResourceRank.Yellow, "res://assets/ui/materials/hemp_cloth.png"),
	new(nameof(GameState.Leather), MaterialSemanticRules.GetDisplayName(nameof(GameState.Leather)), "🥾", MaterialSemanticRules.GetDescription(nameof(GameState.Leather)), ResourceGroup.Relics, ResourceRank.Yellow, "res://assets/ui/materials/leather.png"),
	new(nameof(GameState.IndustrialParts), MaterialSemanticRules.GetDisplayName(nameof(GameState.IndustrialParts)), "⚙", MaterialSemanticRules.GetDescription(nameof(GameState.IndustrialParts)), ResourceGroup.Relics, ResourceRank.Earth, "res://assets/ui/materials/industrial_parts.png"),
	new(nameof(GameState.ConstructionMaterials), MaterialSemanticRules.GetDisplayName(nameof(GameState.ConstructionMaterials)), "🏗", MaterialSemanticRules.GetDescription(nameof(GameState.ConstructionMaterials)), ResourceGroup.Relics, ResourceRank.Yellow, "res://assets/ui/materials/construction_materials.png")
];

	private Label _hintLabel = null!;
	private Label _warehouseStatusValue = null!;
	private Label _capacityValueLabel = null!;
	private ScrollContainer _inventoryScroll = null!;
	private GridContainer _inventoryGrid = null!;
	private PanelContainer _resourceSlotTemplate = null!;
	private Label _tierZeroChainStatusValue = null!;
	private Button _allTabButton = null!;
	private Button _relicsTabButton = null!;
	private Button _manualsTabButton = null!;
	private Button _elixirsTabButton = null!;
	private Button _treasuresTabButton = null!;
	private Button _currencyTabButton = null!;
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
		_relicsTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/BasicTabButton");
		_manualsTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/MaterialsTabButton");
		_elixirsTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/CraftedTabButton");
		_treasuresTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/TreasuresTabButton");
		_currencyTabButton = GetNode<Button>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/InventoryArea/TabRow/CurrencyTabButton");
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

		_warehouseStatusValue.Text = $"当前品阶：须弥空间 {ToChineseTier(state.WarehouseLevel)}阶 · 聚灵大阵 {ToChineseTier(state.MiningLevel)}阶";
		_warehouseStatusValue.TooltipText = $"须弥负载 {_warehouseLoadRate:0}%";
		_capacityValueLabel.Text = $"{used:N0} / {capacity:N0}";
		_capacityValueLabel.TooltipText = $"余量 {Math.Max(capacity - used, 0):0}";

		CallVisualFx("apply_capacity_visual", _warehouseLoadRate);
		RefreshInventoryState(state);
		RefreshActionButtons(state);
		RefreshPopupHint();
	}

	private void BindEvents()
	{
		_closeButton.Pressed += ClosePopup;
		_allTabButton.Pressed += () => SwitchTab(InventoryTab.All);
		_relicsTabButton.Pressed += () => SwitchTab(InventoryTab.Relics);
		_manualsTabButton.Pressed += () => SwitchTab(InventoryTab.Manuals);
		_elixirsTabButton.Pressed += () => SwitchTab(InventoryTab.Elixirs);
		_treasuresTabButton.Pressed += () => SwitchTab(InventoryTab.Treasures);
		_currencyTabButton.Pressed += () => SwitchTab(InventoryTab.Currency);
		_upgradeButton.Pressed += () => HandleWarehouseAction("已批复须弥扩容，请留意库容与聚灵阵纹变化。", UpgradeMineWarehouseRequested);
		_craftToolsButton.Pressed += () => HandleWarehouseAction("已批红祭炼阵旗，请查验阵旗余量与炉室记录。", CraftToolsRequested);
		_buildWorkshopButton.Pressed += () => HandleWarehouseAction("已批复辟建地火丹室，请留意土木与丹火账目。", BuildWorkshopRequested);
		_buildAdministrationButton.Pressed += () => HandleWarehouseAction("已批复扩建传功阁，请留意度支消耗。", BuildAdministrationRequested);
		_buildForestryChainButton.Pressed += () => HandleWarehouseAction("已改定聚灵阵章程，请留意灵木与灵气盈缺。", BuildForestryChainRequested);
		_buildMasonryChainButton.Pressed += () => HandleWarehouseAction("已改定引星阵章程，请留意青罡石料与护山构件盈缺。", BuildMasonryChainRequested);
		_buildMedicinalChainButton.Pressed += () => HandleWarehouseAction("已改定五行阵章程，请留意灵草、卤水与丹散出入。", BuildMedicinalChainRequested);
		_buildFiberChainButton.Pressed += () => HandleWarehouseAction("已改定炼魔阵章程，请留意麻料、皮裘与袍服出入。", BuildFiberChainRequested);
	}

	protected override string GetPopupHintText()
	{
		if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
		{
			return PopupStatusMessage!;
		}

		return _warehouseLoadRate switch
		{
			>= 100.0 => "须弥芥子空间满溢，亟需铭刻扩容阵纹。",
			>= 90.0 => "须弥空间逼近极限，宜先扩容或调拨外库。",
			>= 70.0 => "宝库尚丰，可择机整顿法阵与丹室。",
			_ => "翻阅宝库可查神兵、功法、丹药与天材地宝诸项。"
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
			$"当前周天运转：\n天地：聚灵阵 {ToChineseTier(state.ForestryChainLevel)}阶 · 引星阵 {ToChineseTier(state.MasonryChainLevel)}阶\n" +
			$"造化：五行阵 {ToChineseTier(state.MedicinalChainLevel)}阶 · 炼魔阵 {ToChineseTier(state.FiberChainLevel)}阶";

		_upgradeButton.Disabled = state.TechLevel < 1;
		_upgradeButton.Text = state.TechLevel >= 1
			? $"须弥扩容 · 须弥 {ToChineseTier(state.WarehouseLevel)} / 聚灵 {ToChineseTier(state.MiningLevel)}"
			: "须弥扩容（未启）";
		_upgradeButton.TooltipText = state.TechLevel >= 1
			? "扩容须弥空间并提升聚灵阵品阶。"
			: "需先掌握【锻造术 壹阶】。";

		_lockedForgeButton.TooltipText = "待法宝体系接入后开放。";
		_craftToolsButton.Text = $"祭炼阵旗 · 余 {InventoryRules.GetVisibleAmount(state, nameof(GameState.IndustryTools)):N0}";
		_craftToolsButton.TooltipText = "消耗玄铁、赤铜等材料，祭炼阵旗以稳固宗门法阵。";

		_buildWorkshopButton.Text = $"辟建地火丹室 · {ToChineseTier(state.WorkshopBuildings)}级";
		_buildAdministrationButton.Text = $"扩建传功阁 · {ToChineseTier(state.AdministrationBuildings)}级";
		_buildForestryChainButton.Text = $"布置 聚灵阵 · {ToChineseTier(state.ForestryChainLevel)}阶";
		_buildMasonryChainButton.Text = $"布置 引星阵 · {ToChineseTier(state.MasonryChainLevel)}阶";
		_buildMedicinalChainButton.Text = $"布置 五行阵 · {ToChineseTier(state.MedicinalChainLevel)}阶";
		_buildFiberChainButton.Text = $"布置 炼魔阵 · {ToChineseTier(state.FiberChainLevel)}阶";
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
		typeLabel.Text = $"{GetRankLabel(slot.Rank)} · {GetGroupLabel(slot.Group)}";
		amountLabel.Text = "0";

		CallVisualFx(
			"style_resource_slot",
			card,
			token,
			tokenGlyph,
			nameLabel,
			typeLabel,
			amountLabel,
			ResolveRankColor(slot.Rank));

		_slotBindings.Add(new ResourceSlotBinding(slot, card, token, tokenGlyph, nameLabel, typeLabel, amountLabel));
		return card;
	}

	private IReadOnlyList<ResourceSlotDefinition> GetVisibleSlots(InventoryTab tab)
	{
		return tab switch
		{
			InventoryTab.Relics => ResourceSlots.Where(static slot => slot.Group == ResourceGroup.Relics).ToArray(),
			InventoryTab.Manuals => ResourceSlots.Where(static slot => slot.Group == ResourceGroup.Manuals).ToArray(),
			InventoryTab.Elixirs => ResourceSlots.Where(static slot => slot.Group == ResourceGroup.Elixirs).ToArray(),
			InventoryTab.Treasures => ResourceSlots.Where(static slot => slot.Group == ResourceGroup.Treasures).ToArray(),
			InventoryTab.Currency => ResourceSlots.Where(static slot => slot.Group == ResourceGroup.Currency).ToArray(),
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
			ResourceGroup.Relics => "神兵法宝",
			ResourceGroup.Manuals => "功法秘籍",
			ResourceGroup.Elixirs => "丹药灵草",
			ResourceGroup.Treasures => "天材地宝",
			ResourceGroup.Currency => "宗门玉钱",
			_ => "森罗全象"
		};
	}

	private static string GetRankLabel(ResourceRank rank)
	{
		return rank switch
		{
			ResourceRank.Heaven => "天阶",
			ResourceRank.Earth => "地阶",
			ResourceRank.Mystic => "玄阶",
			ResourceRank.Yellow => "黄阶",
			_ => "凡品"
		};
	}

	private static Color ResolveRankColor(ResourceRank rank)
	{
		return rank switch
		{
			ResourceRank.Heaven => new Color(0.862f, 0.659f, 0.141f),
			ResourceRank.Earth => new Color(0.561f, 0.329f, 0.788f),
			ResourceRank.Mystic => new Color(0.231f, 0.510f, 0.769f),
			ResourceRank.Yellow => new Color(0.302f, 0.600f, 0.365f),
			_ => new Color(0.541f, 0.533f, 0.514f)
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
