using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class WarehousePanel : PopupPanelBase
{
    private sealed record ResourceSlotDefinition(
        string InventoryKey,
        string DisplayName,
        string IconGlyph,
        string Description,
        Color AccentColor);

    private sealed record ResourceCategoryDefinition(
        string Title,
        string Description,
        ResourceSlotDefinition[] Slots);

    private sealed class ResourceSlotBinding
    {
        public ResourceSlotBinding(PanelContainer card, PanelContainer iconBadge, Label nameLabel, Label amountLabel)
        {
            Card = card;
            IconBadge = iconBadge;
            NameLabel = nameLabel;
            AmountLabel = amountLabel;
        }

        public PanelContainer Card { get; }
        public PanelContainer IconBadge { get; }
        public Label NameLabel { get; }
        public Label AmountLabel { get; }
    }

    private static readonly Color ActiveSlotModulate = Colors.White;
    private static readonly Color InactiveSlotModulate = new(1f, 1f, 1f, 0.55f);

    private static readonly ResourceCategoryDefinition[] ResourceCategories =
    [
        new(
            "天衍峰库藏",
            "维持天衍峰日常运转与门人修习的即用储备。",
            [
                new(nameof(GameState.Food), MaterialSemanticRules.GetDisplayName(nameof(GameState.Food)), "🌾", MaterialSemanticRules.GetDescription(nameof(GameState.Food)), new Color(0.86f, 0.73f, 0.24f)),
                new(nameof(GameState.Wood), MaterialSemanticRules.GetDisplayName(nameof(GameState.Wood)), "🪵", MaterialSemanticRules.GetDescription(nameof(GameState.Wood)), new Color(0.62f, 0.42f, 0.22f)),
                new(nameof(GameState.Stone), MaterialSemanticRules.GetDisplayName(nameof(GameState.Stone)), "🪨", MaterialSemanticRules.GetDescription(nameof(GameState.Stone)), new Color(0.57f, 0.60f, 0.65f)),
                new(nameof(GameState.Gold), MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold)), "🪙", MaterialSemanticRules.GetDescription(nameof(GameState.Gold)), new Color(0.87f, 0.74f, 0.28f)),
                new(nameof(GameState.ContributionPoints), MaterialSemanticRules.GetDisplayName(nameof(GameState.ContributionPoints)), "🏅", MaterialSemanticRules.GetDescription(nameof(GameState.ContributionPoints)), new Color(0.72f, 0.68f, 0.95f)),
                new(nameof(GameState.IndustryTools), MaterialSemanticRules.GetDisplayName(nameof(GameState.IndustryTools)), "🛠", MaterialSemanticRules.GetDescription(nameof(GameState.IndustryTools)), new Color(0.76f, 0.80f, 0.84f)),
                new(nameof(GameState.ClothingStock), MaterialSemanticRules.GetDisplayName(nameof(GameState.ClothingStock)), "👘", MaterialSemanticRules.GetDescription(nameof(GameState.ClothingStock)), new Color(0.56f, 0.67f, 0.88f))
            ]),
        new(
            "峰外灵材",
            "峰外采办与 `T0` 灵材链前的基础原料。",
            [
                new(nameof(GameState.Timber), MaterialSemanticRules.GetDisplayName(nameof(GameState.Timber)), "🌲", MaterialSemanticRules.GetDescription(nameof(GameState.Timber)), new Color(0.36f, 0.59f, 0.29f)),
                new(nameof(GameState.RawStone), MaterialSemanticRules.GetDisplayName(nameof(GameState.RawStone)), "🪨", MaterialSemanticRules.GetDescription(nameof(GameState.RawStone)), new Color(0.47f, 0.52f, 0.59f)),
                new(nameof(GameState.Clay), MaterialSemanticRules.GetDisplayName(nameof(GameState.Clay)), "🧱", MaterialSemanticRules.GetDescription(nameof(GameState.Clay)), new Color(0.79f, 0.48f, 0.30f)),
                new(nameof(GameState.Brine), MaterialSemanticRules.GetDisplayName(nameof(GameState.Brine)), "💧", MaterialSemanticRules.GetDescription(nameof(GameState.Brine)), new Color(0.36f, 0.72f, 0.88f)),
                new(nameof(GameState.Herbs), MaterialSemanticRules.GetDisplayName(nameof(GameState.Herbs)), "🌿", MaterialSemanticRules.GetDescription(nameof(GameState.Herbs)), new Color(0.29f, 0.71f, 0.39f)),
                new(nameof(GameState.HempFiber), MaterialSemanticRules.GetDisplayName(nameof(GameState.HempFiber)), "🧶", MaterialSemanticRules.GetDescription(nameof(GameState.HempFiber)), new Color(0.62f, 0.78f, 0.42f)),
                new(nameof(GameState.Reeds), MaterialSemanticRules.GetDisplayName(nameof(GameState.Reeds)), "🎋", MaterialSemanticRules.GetDescription(nameof(GameState.Reeds)), new Color(0.70f, 0.79f, 0.34f)),
                new(nameof(GameState.Hides), MaterialSemanticRules.GetDisplayName(nameof(GameState.Hides)), "🐾", MaterialSemanticRules.GetDescription(nameof(GameState.Hides)), new Color(0.72f, 0.58f, 0.40f))
            ]),
        new(
            "矿脉灵材",
            "矿仓、冶铸与进阶制造的前置矿料与锭材。",
            [
                new(nameof(GameState.IronOre), MaterialSemanticRules.GetDisplayName(nameof(GameState.IronOre)), "⛏", MaterialSemanticRules.GetDescription(nameof(GameState.IronOre)), new Color(0.58f, 0.63f, 0.72f)),
                new(nameof(GameState.CopperOre), MaterialSemanticRules.GetDisplayName(nameof(GameState.CopperOre)), "⛏", MaterialSemanticRules.GetDescription(nameof(GameState.CopperOre)), new Color(0.84f, 0.47f, 0.23f)),
                new(nameof(GameState.Coal), MaterialSemanticRules.GetDisplayName(nameof(GameState.Coal)), "🔥", MaterialSemanticRules.GetDescription(nameof(GameState.Coal)), new Color(0.29f, 0.31f, 0.35f)),
                new(nameof(GameState.RareMaterial), MaterialSemanticRules.GetDisplayName(nameof(GameState.RareMaterial)), "💎", MaterialSemanticRules.GetDescription(nameof(GameState.RareMaterial)), new Color(0.68f, 0.48f, 0.93f)),
                new(nameof(GameState.CopperIngot), MaterialSemanticRules.GetDisplayName(nameof(GameState.CopperIngot)), "🔶", MaterialSemanticRules.GetDescription(nameof(GameState.CopperIngot)), new Color(0.84f, 0.56f, 0.31f)),
                new(nameof(GameState.WroughtIron), MaterialSemanticRules.GetDisplayName(nameof(GameState.WroughtIron)), "⚙", MaterialSemanticRules.GetDescription(nameof(GameState.WroughtIron)), new Color(0.66f, 0.74f, 0.79f)),
                new(nameof(GameState.CompositeMaterial), MaterialSemanticRules.GetDisplayName(nameof(GameState.CompositeMaterial)), "🔷", MaterialSemanticRules.GetDescription(nameof(GameState.CompositeMaterial)), new Color(0.33f, 0.78f, 0.79f))
            ]),
        new(
            "炼坊产物",
            "民生恢复、锻器制造与护山营造的产线输出。",
            [
                new(nameof(GameState.FineSalt), MaterialSemanticRules.GetDisplayName(nameof(GameState.FineSalt)), "🧂", MaterialSemanticRules.GetDescription(nameof(GameState.FineSalt)), new Color(0.93f, 0.95f, 0.97f)),
                new(nameof(GameState.HerbalMedicine), MaterialSemanticRules.GetDisplayName(nameof(GameState.HerbalMedicine)), "⚗", MaterialSemanticRules.GetDescription(nameof(GameState.HerbalMedicine)), new Color(0.42f, 0.84f, 0.59f)),
                new(nameof(GameState.HempCloth), MaterialSemanticRules.GetDisplayName(nameof(GameState.HempCloth)), "🧵", MaterialSemanticRules.GetDescription(nameof(GameState.HempCloth)), new Color(0.71f, 0.72f, 0.57f)),
                new(nameof(GameState.Leather), MaterialSemanticRules.GetDisplayName(nameof(GameState.Leather)), "🥾", MaterialSemanticRules.GetDescription(nameof(GameState.Leather)), new Color(0.62f, 0.44f, 0.29f)),
                new(nameof(GameState.IndustrialParts), MaterialSemanticRules.GetDisplayName(nameof(GameState.IndustrialParts)), "⚙", MaterialSemanticRules.GetDescription(nameof(GameState.IndustrialParts)), new Color(0.47f, 0.78f, 0.95f)),
                new(nameof(GameState.ConstructionMaterials), MaterialSemanticRules.GetDisplayName(nameof(GameState.ConstructionMaterials)), "🏗", MaterialSemanticRules.GetDescription(nameof(GameState.ConstructionMaterials)), new Color(0.85f, 0.52f, 0.38f))
            ])
    ];

    private static readonly IReadOnlyDictionary<string, ResourceSlotDefinition> ResourceSlotLookup =
        ResourceCategories.SelectMany(static category => category.Slots).ToDictionary(static slot => slot.InventoryKey);

    private Label _warehouseStatusValue = null!;
    private ProgressBar _warehouseLoadBar = null!;
    private VBoxContainer _inventoryColumn = null!;
    private Label _tierZeroChainStatusValue = null!;
    private Button _upgradeButton = null!;
    private Button _craftToolsButton = null!;
    private Button _buildWorkshopButton = null!;
    private Button _buildAdministrationButton = null!;
    private Button _buildForestryChainButton = null!;
    private Button _buildMasonryChainButton = null!;
    private Button _buildMedicinalChainButton = null!;
    private Button _buildFiberChainButton = null!;
    private Button _closeButton = null!;
    private Button _cancelButton = null!;
    private readonly Dictionary<string, ResourceSlotBinding> _slotBindings = new();
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
        _warehouseStatusValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/StatusSection/WarehouseStatusValue");
        _warehouseLoadBar = GetNode<ProgressBar>("CenterLayer/Dialog/Margin/MainColumn/StatusSection/WarehouseLoadBar");
        _inventoryColumn = GetNode<VBoxContainer>("CenterLayer/Dialog/Margin/MainColumn/InventoryFrame/InventorySection/InventoryScroll/InventoryColumn");
        _tierZeroChainStatusValue = GetNode<Label>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroStatusValue");
        _upgradeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/PrimaryActionRow/UpgradeButton");
        _craftToolsButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/PrimaryActionRow/CraftToolsButton");
        _buildWorkshopButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/BuildActionRow/BuildWorkshopButton");
        _buildAdministrationButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/BuildActionRow/BuildAdministrationButton");
        _buildForestryChainButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroActionRowTop/BuildForestryChainButton");
        _buildMasonryChainButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroActionRowTop/BuildMasonryChainButton");
        _buildMedicinalChainButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroActionRowBottom/BuildMedicinalChainButton");
        _buildFiberChainButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/ActionSection/TierZeroActionRowBottom/BuildFiberChainButton");
        _closeButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/HeaderRow/CloseButton");
        _cancelButton = GetNode<Button>("CenterLayer/Dialog/Margin/MainColumn/FooterRow/CloseFooterButton");

        BuildResourceSections();
        InitializePopupHint("CenterLayer/Dialog/Margin/MainColumn/HintLabel");
        BindEvents();
        Hide();
    }

    public void Open(GameState state)
    {
        RefreshState(state);
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

    public void RefreshState(GameState state)
    {
        MaterialRules.EnsureDefaults(state);

        var used = state.GetWarehouseUsed();
        var capacity = Math.Max(state.WarehouseCapacity, 1.0);
        var usedRate = (used / capacity) * 100.0;
        _warehouseLoadRate = usedRate;

        _warehouseStatusValue.Text =
            $"仓储 Lv.{state.WarehouseLevel} · 矿坑 Lv.{state.MiningLevel} · 已占 {used:0}/{capacity:0} · 剩余 {Math.Max(capacity - used, 0):0}";
        _warehouseStatusValue.TooltipText = $"当前仓储占用率 {usedRate:0}%";
        _warehouseLoadBar.MaxValue = capacity;
        _warehouseLoadBar.Value = Math.Clamp(used, 0, capacity);
        _warehouseLoadBar.TooltipText = $"仓储负载 {usedRate:0}%";

        foreach (var (inventoryKey, binding) in _slotBindings)
        {
            if (!ResourceSlotLookup.TryGetValue(inventoryKey, out var definition))
            {
                continue;
            }

            var amount = InventoryRules.GetVisibleAmount(state, inventoryKey);
            binding.AmountLabel.Text = amount.ToString();
            binding.Card.Modulate = amount > 0 ? ActiveSlotModulate : InactiveSlotModulate;
            binding.IconBadge.Modulate = amount > 0 ? ActiveSlotModulate : new Color(1f, 1f, 1f, 0.78f);
            binding.NameLabel.Modulate = amount > 0 ? ActiveSlotModulate : new Color(0.86f, 0.88f, 0.92f, 0.82f);
            binding.Card.TooltipText = $"{definition.DisplayName}：{amount}\n{definition.Description}";
        }

        _tierZeroChainStatusValue.Text = MaterialRules.DescribeTierZeroChains(state);

        _upgradeButton.Text = state.TechLevel >= 1
            ? $"矿仓联建（矿 Lv.{state.MiningLevel}/仓 Lv.{state.WarehouseLevel}）"
            : "🔒 需科技 锻造术(T1)";
        _craftToolsButton.Text = $"锻制工器（库存 {InventoryRules.GetVisibleAmount(state, nameof(GameState.IndustryTools))}）";
        _buildWorkshopButton.Text = $"扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}（{state.WorkshopBuildings}）";
        _buildAdministrationButton.Text = $"扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)}（{state.AdministrationBuildings}）";
        _buildForestryChainButton.Text = $"灵木链 Lv.{state.ForestryChainLevel}";
        _buildMasonryChainButton.Text = $"石陶链 Lv.{state.MasonryChainLevel}";
        _buildMedicinalChainButton.Text = $"盐丹链 Lv.{state.MedicinalChainLevel}";
        _buildFiberChainButton.Text = $"织裘链 Lv.{state.FiberChainLevel}";
        RefreshPopupHint();
    }

    private void BuildResourceSections()
    {
        if (_inventoryColumn.GetChildCount() > 0)
        {
            return;
        }

        foreach (var category in ResourceCategories)
        {
            _inventoryColumn.AddChild(CreateCategorySection(category));
        }
    }

    private Control CreateCategorySection(ResourceCategoryDefinition category)
    {
        var section = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        section.AddThemeStyleboxOverride("panel", CreateSectionStyle());

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_top", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_bottom", 12);
        section.AddChild(margin);

        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 10);
        margin.AddChild(column);

        var titleLabel = new Label
        {
            Text = category.Title
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 15);
        titleLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.91f, 0.79f));
        column.AddChild(titleLabel);

        var descriptionLabel = new Label
        {
            Text = category.Description,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        descriptionLabel.Modulate = new Color(0.76f, 0.79f, 0.85f, 0.92f);
        column.AddChild(descriptionLabel);

        var grid = new GridContainer
        {
            Columns = 5
        };
        grid.AddThemeConstantOverride("h_separation", 10);
        grid.AddThemeConstantOverride("v_separation", 10);
        column.AddChild(grid);

        foreach (var slot in category.Slots)
        {
            grid.AddChild(CreateResourceSlot(slot));
        }

        return section;
    }

    private Control CreateResourceSlot(ResourceSlotDefinition slot)
    {
        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(150, 92),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        card.AddThemeStyleboxOverride("panel", CreateSlotStyle(slot.AccentColor));
        card.TooltipText = slot.Description;

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        card.AddChild(margin);

        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 8);
        margin.AddChild(column);

        var topRow = new HBoxContainer();
        topRow.Alignment = BoxContainer.AlignmentMode.Begin;
        column.AddChild(topRow);

        var iconBadge = new PanelContainer
        {
            CustomMinimumSize = new Vector2(40, 40)
        };
        iconBadge.AddThemeStyleboxOverride("panel", CreateIconBadgeStyle(slot.AccentColor));
        topRow.AddChild(iconBadge);

        var iconCenter = new CenterContainer();
        iconBadge.AddChild(iconCenter);

        var iconLabel = new Label
        {
            Text = slot.IconGlyph,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        iconLabel.AddThemeFontSizeOverride("font_size", 18);
        iconCenter.AddChild(iconLabel);

        var amountLabel = new Label
        {
            Text = "0",
            HorizontalAlignment = HorizontalAlignment.Right,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        amountLabel.AddThemeFontSizeOverride("font_size", 20);
        amountLabel.AddThemeColorOverride("font_color", slot.AccentColor.Lerp(new Color(0.98f, 0.95f, 0.86f), 0.45f));
        topRow.AddChild(amountLabel);

        var nameLabel = new Label
        {
            Text = slot.DisplayName,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 13);
        nameLabel.AddThemeColorOverride("font_color", new Color(0.89f, 0.90f, 0.93f));
        column.AddChild(nameLabel);

        _slotBindings[slot.InventoryKey] = new ResourceSlotBinding(card, iconBadge, nameLabel, amountLabel);
        return card;
    }

    private void BindEvents()
    {
        _closeButton.Pressed += ClosePopup;
        _cancelButton.Pressed += ClosePopup;
        _upgradeButton.Pressed += () => HandleWarehouseAction("已发送矿仓联建请求，请查看仓储容量与资源变化。", UpgradeMineWarehouseRequested);
        _craftToolsButton.Pressed += () => HandleWarehouseAction("已发送锻制工器请求，请查看工器库存与日志。", CraftToolsRequested);
        _buildWorkshopButton.Pressed += () => HandleWarehouseAction($"已发送扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)}请求，请查看建筑数量与资源变化。", BuildWorkshopRequested);
        _buildAdministrationButton.Pressed += () => HandleWarehouseAction($"已发送扩建{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)}请求，请查看建筑数量与资源变化。", BuildAdministrationRequested);
        _buildForestryChainButton.Pressed += () => HandleWarehouseAction("已发送灵木链扩建请求，请查看灵木与灵木料变化。", BuildForestryChainRequested);
        _buildMasonryChainButton.Pressed += () => HandleWarehouseAction("已发送石陶链扩建请求，请查看青罡原石、赤陶土与护山构件变化。", BuildMasonryChainRequested);
        _buildMedicinalChainButton.Pressed += () => HandleWarehouseAction("已发送盐丹链扩建请求，请查看寒泉卤水、灵草与民生产物变化。", BuildMedicinalChainRequested);
        _buildFiberChainButton.Pressed += () => HandleWarehouseAction("已发送织裘链扩建请求，请查看青麻、青芦、灵兽皮与衣料变化。", BuildFiberChainRequested);
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return _warehouseLoadRate >= 90.0
            ? "仓储接近满载，建议优先矿仓联建或尽快消耗材料。按 Esc 可快速关闭。"
            : "按分类查看灵材卡槽与库存，并直接扩建 T0 灵材链、矿仓与工器产能。按 Esc 可快速关闭。";
    }

    private void HandleWarehouseAction(string statusMessage, Action? requestedAction)
    {
        ShowPopupStatusMessage(statusMessage);
        requestedAction?.Invoke();
    }

    private static StyleBoxFlat CreateSectionStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.094f, 0.101f, 0.121f, 0.96f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.22f, 0.24f, 0.29f, 1f),
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusBottomLeft = 6
        };
    }

    private static StyleBoxFlat CreateSlotStyle(Color accentColor)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.118f, 0.125f, 0.145f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(accentColor.R, accentColor.G, accentColor.B, 0.65f),
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomRight = 5,
            CornerRadiusBottomLeft = 5
        };
    }

    private static StyleBoxFlat CreateIconBadgeStyle(Color accentColor)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(accentColor.R * 0.30f + 0.12f, accentColor.G * 0.30f + 0.12f, accentColor.B * 0.30f + 0.12f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(accentColor.R, accentColor.G, accentColor.B, 0.85f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4
        };
    }
}
