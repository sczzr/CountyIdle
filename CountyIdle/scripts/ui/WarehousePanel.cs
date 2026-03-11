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

	private static readonly Color PaperMainColor = new(0.95f, 0.92f, 0.84f, 1f);
	private static readonly Color PaperDarkColor = new(0.89f, 0.85f, 0.76f, 1f);
	private static readonly Color InkMainColor = new(0.17f, 0.15f, 0.13f, 1f);
	private static readonly Color InkMutedColor = new(0.42f, 0.37f, 0.33f, 1f);
	private static readonly Color SealRedColor = new(0.65f, 0.16f, 0.16f, 1f);
	private static readonly Color BorderNormalColor = new(0.29f, 0.25f, 0.21f, 1f);
	private static readonly Color AccentGoldColor = new(0.72f, 0.53f, 0.04f, 1f);
	private static readonly Color AccentBlueColor = new(0.19f, 0.33f, 0.54f, 1f);
	private static readonly Color DangerColor = SealRedColor;
	private static readonly Color WarningColor = new(0.72f, 0.53f, 0.04f, 1f);
	private static readonly Color ActiveSlotModulate = Colors.White;
	private static readonly Color InactiveSlotModulate = new(1f, 1f, 1f, 0.45f);
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

	private PanelContainer _paperPanel = null!;
	private PanelContainer _statusSection = null!;
	private Panel _capacityBarFrame = null!;
	private ProgressBar _capacityBar = null!;
	private TextureRect _capacityTickOverlay = null!;
	private PanelContainer _leftRoller = null!;
	private PanelContainer _rightRoller = null!;
	private PanelContainer _chainInfoFrame = null!;
	private Label _warningStampLabel = null!;
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
	private readonly Dictionary<InventoryTab, Button> _tabButtons = new();
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
		_paperPanel = GetNode<PanelContainer>("CenterLayer/LedgerWrapper/FrameRow/Paper");
		_leftRoller = GetNode<PanelContainer>("CenterLayer/LedgerWrapper/FrameRow/LeftRoller");
		_rightRoller = GetNode<PanelContainer>("CenterLayer/LedgerWrapper/FrameRow/RightRoller");
		_statusSection = GetNode<PanelContainer>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection");
		_capacityBarFrame = GetNode<Panel>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/CapacityBarFrame");
		_capacityBar = GetNode<ProgressBar>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/CapacityBarFrame/CapacityBar");
		_capacityTickOverlay = GetNode<TextureRect>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/CapacityBarFrame/CapacityTickOverlay");
		_chainInfoFrame = GetNode<PanelContainer>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/ChainInfoFrame");
		_warningStampLabel = GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/StatusSection/StatusMargin/StatusContent/StatusRow/WarningStampLabel");
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

		_tabButtons[InventoryTab.All] = _allTabButton;
		_tabButtons[InventoryTab.Basic] = _basicTabButton;
		_tabButtons[InventoryTab.Materials] = _materialsTabButton;
		_tabButtons[InventoryTab.Crafted] = _craftedTabButton;

		_resourceSlotTemplate.Visible = false;

		ApplyStaticStyles();
		BuildInventoryGrid();
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

		UpdateCapacityVisual(_warehouseLoadRate);
		RefreshInventoryState(state);
		RefreshActionButtons(state);
		RefreshPopupHint();
	}

	private void ApplyStaticStyles()
	{
		_paperPanel.AddThemeStyleboxOverride("panel", CreatePaperStyle());
		_leftRoller.AddThemeStyleboxOverride("panel", CreateRollerStyle());
		_rightRoller.AddThemeStyleboxOverride("panel", CreateRollerStyle());
		_statusSection.AddThemeStyleboxOverride("panel", CreateWarningPanelStyle());
		_capacityBarFrame.AddThemeStyleboxOverride("panel", CreateCapacityFrameStyle(SealRedColor));
		_capacityBar.AddThemeStyleboxOverride("background", CreateCapacityBarBackgroundStyle());
		_capacityBar.AddThemeStyleboxOverride("fill", CreateCapacityBarFillStyle(SealRedColor));
		_capacityBar.ShowPercentage = false;
		_capacityBar.MinValue = 0;
		_capacityBar.MaxValue = 100;
		_capacityBar.Value = 0;
		_capacityTickOverlay.Texture = CreateCapacityTickTexture();
		_capacityTickOverlay.StretchMode = TextureRect.StretchModeEnum.Tile;
		_capacityTickOverlay.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
		_capacityTickOverlay.Modulate = new Color(0.2f, 0.17f, 0.13f, 0.22f);
		_chainInfoFrame.AddThemeStyleboxOverride("panel", CreateNoteStyle());

		var titleLabel = GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/HeaderRow/TitleGroup/TitleLabel");
		titleLabel.AddThemeFontSizeOverride("font_size", 26);
		titleLabel.AddThemeColorOverride("font_color", InkMainColor);

		var subtitleLabel = GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/HeaderRow/TitleGroup/SubtitleLabel");
		subtitleLabel.AddThemeFontSizeOverride("font_size", 14);
		subtitleLabel.AddThemeColorOverride("font_color", InkMutedColor);

		_warningStampLabel.Rotation = -0.08f;
		_warningStampLabel.AddThemeFontSizeOverride("font_size", 14);
		_warningStampLabel.AddThemeColorOverride("font_color", SealRedColor);
		_hintLabel.AddThemeFontSizeOverride("font_size", 14);
		_warehouseStatusValue.AddThemeFontSizeOverride("font_size", 12);
		_warehouseStatusValue.AddThemeColorOverride("font_color", InkMutedColor);
		_capacityValueLabel.AddThemeFontSizeOverride("font_size", 20);
		_capacityValueLabel.AddThemeColorOverride("font_color", InkMainColor);
		_tierZeroChainStatusValue.AddThemeFontSizeOverride("font_size", 13);
		_tierZeroChainStatusValue.AddThemeColorOverride("font_color", InkMainColor);

		foreach (var label in new[]
				 {
					 GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ManufactureSection/ManufactureTitle"),
					 GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/BuildSection/BuildTitle"),
					 GetNode<Label>("CenterLayer/LedgerWrapper/FrameRow/Paper/PaperMargin/MainColumn/BodyRow/ActionArea/ActionColumn/ChainSection/ChainTitle")
				 })
		{
			label.AddThemeFontSizeOverride("font_size", 18);
			label.AddThemeColorOverride("font_color", InkMainColor);
		}

		ApplyCloseButtonStyle(_closeButton);
		ApplyOrderButtonStyle(_lockedForgeButton, true);
		ApplyActionButtonStyle(_upgradeButton);
		ApplyActionButtonStyle(_craftToolsButton);
		ApplyActionButtonStyle(_buildWorkshopButton);
		ApplyActionButtonStyle(_buildAdministrationButton);
		ApplyActionButtonStyle(_buildForestryChainButton);
		ApplyActionButtonStyle(_buildMasonryChainButton);
		ApplyActionButtonStyle(_buildMedicinalChainButton);
		ApplyActionButtonStyle(_buildFiberChainButton);
		RefreshTabStyles();
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
	}

	private void RefreshTabStyles()
	{
		foreach (var (tab, button) in _tabButtons)
		{
			ApplyTabButtonStyle(button, tab == _activeTab);
		}
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
			binding.Card.Modulate = hasAmount ? ActiveSlotModulate : InactiveSlotModulate;
			binding.Token.Modulate = hasAmount ? ActiveSlotModulate : new Color(1f, 1f, 1f, 0.62f);
			binding.TokenGlyph.Modulate = hasAmount ? ActiveSlotModulate : new Color(1f, 1f, 1f, 0.7f);
			binding.NameLabel.Modulate = hasAmount ? ActiveSlotModulate : new Color(1f, 1f, 1f, 0.62f);
			binding.TypeLabel.Modulate = hasAmount ? ActiveSlotModulate : new Color(1f, 1f, 1f, 0.5f);
			binding.AmountLabel.Modulate = hasAmount ? ActiveSlotModulate : new Color(1f, 1f, 1f, 0.7f);
			binding.Card.TooltipText = $"{binding.Definition.DisplayName} × {amount:N0}\n{binding.Definition.Description}";
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
		card.AddThemeStyleboxOverride("panel", CreateSlotStyle());

		var token = card.GetNode<PanelContainer>("SlotMargin/SlotRow/Token");
		var tokenCenter = card.GetNode<CenterContainer>("SlotMargin/SlotRow/Token/TokenCenter");
		var tokenGlyph = card.GetNode<Label>("SlotMargin/SlotRow/Token/TokenCenter/TokenGlyph");
		var nameLabel = card.GetNode<Label>("SlotMargin/SlotRow/InfoColumn/NameLabel");
		var typeLabel = card.GetNode<Label>("SlotMargin/SlotRow/InfoColumn/TypeLabel");
		var amountLabel = card.GetNode<Label>("SlotMargin/SlotRow/AmountLabel");

		token.AddThemeStyleboxOverride("panel", CreateTokenStyle(slot.AccentColor));

		tokenCenter.MouseFilter = MouseFilterEnum.Ignore;
		tokenGlyph.MouseFilter = MouseFilterEnum.Ignore;
		tokenGlyph.Text = GetTokenGlyph(slot);
		tokenGlyph.AddThemeFontSizeOverride("font_size", 20);
		tokenGlyph.AddThemeColorOverride("font_color", PaperMainColor);

		nameLabel.Text = slot.DisplayName;
		nameLabel.AddThemeFontSizeOverride("font_size", 17);
		nameLabel.AddThemeColorOverride("font_color", InkMainColor);

		typeLabel.Text = GetGroupLabel(slot.Group);
		typeLabel.AddThemeFontSizeOverride("font_size", 11);
		typeLabel.AddThemeColorOverride("font_color", InkMutedColor);

		amountLabel.Text = "0";
		amountLabel.AddThemeFontSizeOverride("font_size", 24);
		amountLabel.AddThemeColorOverride("font_color", InkMainColor);

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

	private void UpdateCapacityVisual(double loadRate)
	{
		var accent = loadRate >= 90.0
			? DangerColor
			: loadRate >= 70.0
				? WarningColor
				: AccentBlueColor;

		var background = loadRate >= 90.0
			? new Color(SealRedColor.R, SealRedColor.G, SealRedColor.B, 0.06f)
			: loadRate >= 70.0
				? new Color(AccentGoldColor.R, AccentGoldColor.G, AccentGoldColor.B, 0.06f)
				: new Color(0f, 0f, 0f, 0.02f);

		_statusSection.AddThemeStyleboxOverride("panel", CreateWarningPanelStyle(background, accent));
		_capacityBarFrame.AddThemeStyleboxOverride("panel", CreateCapacityFrameStyle(accent));
		_capacityBar.AddThemeStyleboxOverride("fill", CreateCapacityBarFillStyle(accent));
		_capacityBar.Value = Math.Clamp(loadRate, 0.0, 100.0);
		_warningStampLabel.Text = loadRate switch
		{
			>= 100.0 => "满溢",
			>= 90.0 => "将满",
			>= 70.0 => "偏满",
			_ => "安储"
		};
		_hintLabel.AddThemeColorOverride("font_color", accent);
		_warningStampLabel.AddThemeColorOverride("font_color", accent);
		_capacityValueLabel.AddThemeColorOverride("font_color", loadRate >= 90.0 ? InkMainColor : accent);
	}

	private void ApplyTabButtonStyle(Button button, bool active)
	{
		button.Flat = true;
		button.Alignment = HorizontalAlignment.Center;
		button.AddThemeFontSizeOverride("font_size", 16);
		button.AddThemeStyleboxOverride("normal", CreateTabStyle(active));
		button.AddThemeStyleboxOverride("hover", CreateTabStyle(true));
		button.AddThemeStyleboxOverride("pressed", CreateTabStyle(true));
		button.AddThemeStyleboxOverride("disabled", CreateTabStyle(false));
		button.AddThemeColorOverride("font_color", active ? InkMainColor : InkMutedColor);
		button.AddThemeColorOverride("font_hover_color", InkMainColor);
		button.AddThemeColorOverride("font_pressed_color", InkMainColor);
		button.AddThemeColorOverride("font_disabled_color", new Color(InkMutedColor.R, InkMutedColor.G, InkMutedColor.B, 0.7f));
	}

	private static void ApplyActionButtonStyle(Button button)
	{
		ApplyOrderButtonStyle(button, false);
	}

	private static void ApplyOrderButtonStyle(Button button, bool locked)
	{
		button.Flat = true;
		button.Alignment = HorizontalAlignment.Left;
		button.AddThemeFontSizeOverride("font_size", 15);
		button.AddThemeStyleboxOverride("normal", CreateOrderButtonStyle(locked));
		button.AddThemeStyleboxOverride("hover", CreateOrderButtonHoverStyle(locked));
		button.AddThemeStyleboxOverride("pressed", CreateOrderButtonHoverStyle(locked));
		button.AddThemeStyleboxOverride("disabled", CreateOrderButtonStyle(true));
		button.AddThemeColorOverride("font_color", locked ? InkMutedColor : InkMainColor);
		button.AddThemeColorOverride("font_hover_color", locked ? InkMutedColor : PaperMainColor);
		button.AddThemeColorOverride("font_pressed_color", locked ? InkMutedColor : PaperMainColor);
		button.AddThemeColorOverride("font_disabled_color", InkMutedColor);
	}

	private static void ApplyCloseButtonStyle(Button button)
	{
		button.Flat = true;
		button.Alignment = HorizontalAlignment.Center;
		button.AddThemeFontSizeOverride("font_size", 24);
		button.AddThemeStyleboxOverride("normal", CreateTransparentStyle());
		button.AddThemeStyleboxOverride("hover", CreateTransparentStyle());
		button.AddThemeStyleboxOverride("pressed", CreateTransparentStyle());
		button.AddThemeColorOverride("font_color", InkMainColor);
		button.AddThemeColorOverride("font_hover_color", SealRedColor);
		button.AddThemeColorOverride("font_pressed_color", SealRedColor);
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

	private static StyleBoxFlat CreatePaperStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = PaperMainColor,
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = new Color(0.48f, 0.42f, 0.35f, 0.45f),
			CornerRadiusTopLeft = 2,
			CornerRadiusTopRight = 2,
			CornerRadiusBottomRight = 2,
			CornerRadiusBottomLeft = 2,
			ShadowColor = new Color(0f, 0f, 0f, 0.35f),
			ShadowSize = 12
		};
	}

	private static StyleBoxFlat CreateRollerStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(0.29f, 0.19f, 0.13f, 1f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = new Color(0.14f, 0.09f, 0.05f, 1f),
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0
		};
	}

	private static StyleBoxFlat CreateWarningPanelStyle(Color? backgroundColor = null, Color? borderColor = null)
	{
		return new StyleBoxFlat
		{
			BgColor = backgroundColor ?? new Color(SealRedColor.R, SealRedColor.G, SealRedColor.B, 0.06f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = borderColor ?? SealRedColor,
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0
		};
	}

	private static StyleBoxFlat CreateNoteStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = PaperDarkColor,
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = new Color(0.64f, 0.58f, 0.50f, 1f),
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0
		};
	}

	private static StyleBoxFlat CreateTabStyle(bool active)
	{
		return new StyleBoxFlat
		{
			BgColor = active ? PaperDarkColor : new Color(PaperMainColor.R, PaperMainColor.G, PaperMainColor.B, 0f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = active ? 0 : 0,
			BorderColor = active ? BorderNormalColor : new Color(0f, 0f, 0f, 0f),
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0,
			ContentMarginLeft = 12,
			ContentMarginTop = 8,
			ContentMarginRight = 12,
			ContentMarginBottom = 8
		};
	}

	private static StyleBoxFlat CreateSlotStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(PaperMainColor.R, PaperMainColor.G, PaperMainColor.B, 0.32f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = new Color(BorderNormalColor.R, BorderNormalColor.G, BorderNormalColor.B, 0.55f),
			CornerRadiusTopLeft = 2,
			CornerRadiusTopRight = 2,
			CornerRadiusBottomRight = 2,
			CornerRadiusBottomLeft = 2
		};
	}

	private static StyleBoxFlat CreateTokenStyle(Color accentColor)
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(accentColor.R, accentColor.G, accentColor.B, 0.95f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = new Color(0.12f, 0.1f, 0.08f, 0.85f),
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomRight = 6,
			CornerRadiusBottomLeft = 6,
			ShadowColor = new Color(0f, 0f, 0f, 0.35f),
			ShadowSize = 6
		};
	}

	private static StyleBoxFlat CreateCapacityFrameStyle(Color accentColor)
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(PaperDarkColor.R, PaperDarkColor.G, PaperDarkColor.B, 0.35f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = new Color(accentColor.R, accentColor.G, accentColor.B, 0.9f),
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0
		};
	}

	private static StyleBoxFlat CreateCapacityBarBackgroundStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(PaperDarkColor.R, PaperDarkColor.G, PaperDarkColor.B, 0.55f),
			BorderWidthLeft = 0,
			BorderWidthTop = 0,
			BorderWidthRight = 0,
			BorderWidthBottom = 0
		};
	}

	private static StyleBoxFlat CreateCapacityBarFillStyle(Color accentColor)
	{
		return new StyleBoxFlat
		{
			BgColor = accentColor,
			BorderWidthLeft = 0,
			BorderWidthTop = 0,
			BorderWidthRight = 0,
			BorderWidthBottom = 0
		};
	}

	private static Texture2D CreateCapacityTickTexture()
	{
		const int width = 12;
		const int height = 6;
		var image = Image.Create(width, height, false, Image.Format.Rgba8);
		image.Fill(new Color(0f, 0f, 0f, 0f));

		for (var y = 0; y < height; y++)
		{
			image.SetPixel(width - 1, y, new Color(0f, 0f, 0f, 0.35f));
		}

		return ImageTexture.CreateFromImage(image);
	}

	private static StyleBoxFlat CreateIconFrameStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = PaperMainColor,
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = BorderNormalColor,
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0
		};
	}

	private static StyleBoxFlat CreateOrderButtonStyle(bool locked)
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(PaperMainColor.R, PaperMainColor.G, PaperMainColor.B, 0f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = locked ? new Color(InkMutedColor.R, InkMutedColor.G, InkMutedColor.B, 0.65f) : InkMainColor,
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0,
			ContentMarginLeft = 14,
			ContentMarginTop = 12,
			ContentMarginRight = 14,
			ContentMarginBottom = 12
		};
	}

	private static StyleBoxFlat CreateOrderButtonHoverStyle(bool locked)
	{
		if (locked)
		{
			return CreateOrderButtonStyle(true);
		}

		return new StyleBoxFlat
		{
			BgColor = InkMainColor,
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = InkMainColor,
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomRight = 0,
			CornerRadiusBottomLeft = 0,
			ContentMarginLeft = 14,
			ContentMarginTop = 12,
			ContentMarginRight = 14,
			ContentMarginBottom = 12
		};
	}

	private static StyleBoxFlat CreateTransparentStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(0f, 0f, 0f, 0f),
			BorderWidthLeft = 0,
			BorderWidthTop = 0,
			BorderWidthRight = 0,
			BorderWidthBottom = 0
		};
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
