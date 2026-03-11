using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle;

public partial class Main
{
    private sealed record TileInspectorActionBinding(
        TileInspectorAction Action,
        string Text,
        string TooltipText,
        bool Enabled);

    private enum TileInspectorAction
    {
        None,
        OpenWorldSitePlaceholder,
        BuildAgriculture,
        BuildWorkshop,
        BuildResearch,
        BuildTrade,
        BuildAdministration,
        OpenTaskPanel,
        OpenDisciplePanel,
        OpenWarehousePanel
    }

    private Label? _tileInspectorTitleLabel;
    private Label? _tileInspectorSubtitleLabel;
    private Label? _tileInspectorBadgeLabel;
    private Label? _tileInspectorStatusLabel;
    private Label? _tileInspectorStatusValueLabel;
    private Label? _tileInspectorResidentLabel;
    private Label? _tileInspectorResidentValueLabel;
    private Label? _tileInspectorTransitLabel;
    private Label? _tileInspectorTransitValueLabel;
    private Label? _tileInspectorLocationLabel;
    private Label? _tileInspectorLocationValueLabel;
    private Label? _tileInspectorDescriptionLabel;
    private Label? _tileInspectorActionHintLabel;
    private Button? _tileInspectorPrimaryButton;
    private Button? _tileInspectorSecondaryButton;
    private Button? _tileInspectorTertiaryButton;

    private TileInspectorAction _tileInspectorPrimaryAction = TileInspectorAction.OpenTaskPanel;
    private TileInspectorAction _tileInspectorSecondaryAction = TileInspectorAction.OpenDisciplePanel;
    private TileInspectorAction _tileInspectorTertiaryAction = TileInspectorAction.OpenWarehousePanel;

    private void BindSectTileInspectorNodes()
    {
        _tileInspectorTitleLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/InspectorHeader/TileTitle");
        _tileInspectorSubtitleLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/InspectorHeader/TileSubtitle");
        _tileInspectorBadgeLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/InspectorHeader/TileBadgeBox/TileBadgeLabel");
        _tileInspectorStatusLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/StatusBox/AttrVBox/AttrLabel");
        _tileInspectorStatusValueLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/StatusBox/AttrVBox/AttrValue");
        _tileInspectorResidentLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/ResidentBox/AttrVBox/AttrLabel");
        _tileInspectorResidentValueLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/ResidentBox/AttrVBox/AttrValue");
        _tileInspectorTransitLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/TransitBox/AttrVBox/AttrLabel");
        _tileInspectorTransitValueLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/TransitBox/AttrVBox/AttrValue");
        _tileInspectorLocationLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/LocationBox/AttrVBox/AttrLabel");
        _tileInspectorLocationValueLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/LocationBox/AttrVBox/AttrValue");
        _tileInspectorDescriptionLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/InspectorDescription");
        _tileInspectorActionHintLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/ActionHintPanel/ActionHintLabel");
        _tileInspectorPrimaryButton = GetNodeOrNull<Button>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/ActionList/PrimaryActionButton");
        _tileInspectorSecondaryButton = GetNodeOrNull<Button>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/ActionList/SecondaryActionButton");
        _tileInspectorTertiaryButton = GetNodeOrNull<Button>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/ActionList/TertiaryActionButton");

        ApplySectTileInspectorSummary(TownMapSelectionSummary.CreateDefault());
    }

    private void ClearSectTileInspectorNodes()
    {
        _tileInspectorTitleLabel = null;
        _tileInspectorSubtitleLabel = null;
        _tileInspectorBadgeLabel = null;
        _tileInspectorStatusLabel = null;
        _tileInspectorStatusValueLabel = null;
        _tileInspectorResidentLabel = null;
        _tileInspectorResidentValueLabel = null;
        _tileInspectorTransitLabel = null;
        _tileInspectorTransitValueLabel = null;
        _tileInspectorLocationLabel = null;
        _tileInspectorLocationValueLabel = null;
        _tileInspectorDescriptionLabel = null;
        _tileInspectorActionHintLabel = null;
        _tileInspectorPrimaryButton = null;
        _tileInspectorSecondaryButton = null;
        _tileInspectorTertiaryButton = null;
    }

    private void BindSectTileInspectorEvents()
    {
        if (_sectMapRenderer != null)
        {
            _sectMapRenderer.SelectionSummaryChanged += OnSectMapSelectionSummaryChanged;
        }

        if (_worldMapRenderer != null)
        {
            _worldMapRenderer.WorldSiteSelectionChanged += OnWorldSiteSelectionChanged;
        }

        if (_tileInspectorPrimaryButton != null)
        {
            _tileInspectorPrimaryButton.Pressed += OnTileInspectorPrimaryPressed;
        }

        if (_tileInspectorSecondaryButton != null)
        {
            _tileInspectorSecondaryButton.Pressed += OnTileInspectorSecondaryPressed;
        }

        if (_tileInspectorTertiaryButton != null)
        {
            _tileInspectorTertiaryButton.Pressed += OnTileInspectorTertiaryPressed;
        }
    }

    private void UnbindSectTileInspectorEvents()
    {
        if (_sectMapRenderer != null)
        {
            _sectMapRenderer.SelectionSummaryChanged -= OnSectMapSelectionSummaryChanged;
        }

        if (_worldMapRenderer != null)
        {
            _worldMapRenderer.WorldSiteSelectionChanged -= OnWorldSiteSelectionChanged;
        }

        if (_tileInspectorPrimaryButton != null)
        {
            _tileInspectorPrimaryButton.Pressed -= OnTileInspectorPrimaryPressed;
        }

        if (_tileInspectorSecondaryButton != null)
        {
            _tileInspectorSecondaryButton.Pressed -= OnTileInspectorSecondaryPressed;
        }

        if (_tileInspectorTertiaryButton != null)
        {
            _tileInspectorTertiaryButton.Pressed -= OnTileInspectorTertiaryPressed;
        }
    }

    private void OnSectMapSelectionSummaryChanged(TownMapSelectionSummary summary)
    {
        ApplySectTileInspectorSummary(summary);
    }

    private void OnWorldSiteSelectionChanged(XianxiaSiteData? site)
    {
        if (_currentMapTab != MapTab.World)
        {
            return;
        }

        ApplyWorldSiteInspectorSummary(site);
    }

    private void ApplySectTileInspectorSummary(TownMapSelectionSummary summary)
    {
        if (_tileInspectorTitleLabel == null ||
            _tileInspectorSubtitleLabel == null ||
            _tileInspectorBadgeLabel == null ||
            _tileInspectorStatusLabel == null ||
            _tileInspectorStatusValueLabel == null ||
            _tileInspectorResidentLabel == null ||
            _tileInspectorResidentValueLabel == null ||
            _tileInspectorTransitLabel == null ||
            _tileInspectorTransitValueLabel == null ||
            _tileInspectorLocationLabel == null ||
            _tileInspectorLocationValueLabel == null ||
            _tileInspectorDescriptionLabel == null ||
            _tileInspectorActionHintLabel == null)
        {
            return;
        }

        _tileInspectorTitleLabel.Text = summary.Title;
        _tileInspectorSubtitleLabel.Text = summary.Subtitle;
        _tileInspectorStatusLabel.Text = summary.StatusLabel;
        _tileInspectorStatusValueLabel.Text = summary.StatusText;
        _tileInspectorResidentLabel.Text = summary.ResidentLabel;
        _tileInspectorResidentValueLabel.Text = summary.ResidentText;
        _tileInspectorTransitLabel.Text = summary.TransitLabel;
        _tileInspectorTransitValueLabel.Text = summary.TransitText;
        _tileInspectorLocationLabel.Text = summary.LocationLabel;
        _tileInspectorLocationValueLabel.Text = summary.LocationText;
        _tileInspectorDescriptionLabel.Text = summary.DescriptionText;

        ConfigureTileInspectorActions(summary);
        ApplyTileInspectorVisualTone(summary);
    }

    private void ApplyWorldSiteInspectorSummary(XianxiaSiteData? site)
    {
        if (_tileInspectorTitleLabel == null ||
            _tileInspectorSubtitleLabel == null ||
            _tileInspectorBadgeLabel == null ||
            _tileInspectorStatusLabel == null ||
            _tileInspectorStatusValueLabel == null ||
            _tileInspectorResidentLabel == null ||
            _tileInspectorResidentValueLabel == null ||
            _tileInspectorTransitLabel == null ||
            _tileInspectorTransitValueLabel == null ||
            _tileInspectorLocationLabel == null ||
            _tileInspectorLocationValueLabel == null ||
            _tileInspectorDescriptionLabel == null ||
            _tileInspectorActionHintLabel == null)
        {
            return;
        }

        if (site == null)
        {
            _tileInspectorTitleLabel.Text = "世界地图";
            _tileInspectorSubtitleLabel.Text = "尚未选中外域点位";
            _tileInspectorBadgeLabel.Text = "世界层";
            _tileInspectorStatusLabel.Text = "点位态势";
            _tileInspectorStatusValueLabel.Text = "等待点选";
            _tileInspectorResidentLabel.Text = "主类型";
            _tileInspectorResidentValueLabel.Text = "未选中";
            _tileInspectorTransitLabel.Text = "开放层级";
            _tileInspectorTransitValueLabel.Text = "待判定";
            _tileInspectorLocationLabel.Text = "所属区块";
            _tileInspectorLocationValueLabel.Text = "待识别";
            _tileInspectorDescriptionLabel.Text = "左键点选世界地图中的宗门、凡俗据点、坊市、世家、仙城或遗迹节点后，这里会显示对应的分层信息与建议去向。";
            ApplyPrimaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "等待选中世界点位",
                "先从世界地图点选一个外域点位。",
                false));
            ApplySecondaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "等待选中世界点位",
                "当前尚未选中外域点位。",
                false));
            ApplyTertiaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "等待选中世界点位",
                "选中点位后，这里会显示对应的联动入口。",
                false));
            ApplyTileInspectorActionHint("可执行项：左键点选世界点位查看分层信息；右键可清除当前选中。");
            ApplyWorldInspectorVisualTone("world", false);
            return;
        }

        var primaryTypeText = ResolveWorldPrimaryTypeText(site.PrimaryType);
        var rarityText = ResolveWorldRarityText(site.RarityTier);
        _tileInspectorTitleLabel.Text = site.Label;
        _tileInspectorSubtitleLabel.Text = $"{primaryTypeText} · {ResolveWorldSecondaryTagText(site.SecondaryTag)}";
        _tileInspectorBadgeLabel.Text = $"{primaryTypeText}点";
        _tileInspectorStatusLabel.Text = "稀有度";
        _tileInspectorStatusValueLabel.Text = rarityText;
        _tileInspectorResidentLabel.Text = "主类型";
        _tileInspectorResidentValueLabel.Text = primaryTypeText;
        _tileInspectorTransitLabel.Text = "开放层级";
        _tileInspectorTransitValueLabel.Text = site.UnlockTier switch
        {
            <= 0 => "开局可至",
            1 => "中期开启",
            _ => "后期开启"
        };
        _tileInspectorLocationLabel.Text = "所属区块";
        _tileInspectorLocationValueLabel.Text = ResolveWorldRegionText(site.RegionId);
        _tileInspectorDescriptionLabel.Text = BuildWorldSiteDescription(site, primaryTypeText, rarityText);

        ConfigureWorldSiteInspectorActions(site, primaryTypeText);
        ApplyWorldInspectorVisualTone(site.PrimaryType, true);
    }

    private void ConfigureTileInspectorActions(TownMapSelectionSummary summary)
    {
        if (!summary.HasSelection)
        {
            ApplyPrimaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "当前无选中院域",
                "左键点选任意天衍峰六角地块后，即可查看对应院域详情。",
                false));
            ApplySecondaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "等待选中地块",
                "当前尚未选中院域，暂不提供局部治理入口。",
                false));
            ApplyTertiaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "等待选中地块",
                "选中院域后，可继续打开仓储、弟子谱或宗主中枢联动视图。",
                false));
            ApplyTileInspectorActionHint("可执行项：左键点选任意院域后，可查看灵气、坊位、天然特征与推荐坊局；右键可清除当前选中。");
            return;
        }

        var tileName = string.IsNullOrWhiteSpace(summary.Title) ? "当前地块" : summary.Title;

        if (summary.AnchorType == null)
        {
            ConfigureCompoundTileInspectorActions(summary, tileName);
            return;
        }

        var primaryBinding = summary.AnchorType switch
        {
            TownActivityAnchorType.Farmstead => new TileInspectorActionBinding(
                TileInspectorAction.BuildAgriculture,
                "扩建阵材圃",
                $"对【{tileName}】追加灵植 / 灵田产能，强化阵材与供养链路。",
                true),
            TownActivityAnchorType.Workshop => new TileInspectorActionBinding(
                TileInspectorAction.BuildWorkshop,
                "扩建傀儡工坊",
                $"对【{tileName}】追加营造与工器位，强化阵务与建设链路。",
                true),
            TownActivityAnchorType.Market => new TileInspectorActionBinding(
                TileInspectorAction.BuildTrade,
                "扩建青云总坊",
                $"对【{tileName}】追加流转与外事务位，提升总坊回流。",
                true),
            TownActivityAnchorType.Academy => new TileInspectorActionBinding(
                TileInspectorAction.BuildResearch,
                "扩建传法院",
                $"对【{tileName}】追加讲法与推演位，强化研修与突破链路。",
                true),
            TownActivityAnchorType.Administration => new TileInspectorActionBinding(
                TileInspectorAction.BuildAdministration,
                "扩建庶务殿",
                $"对【{tileName}】追加庶务与执事位，提升治理执行容量。",
                true),
            TownActivityAnchorType.Leisure => new TileInspectorActionBinding(
                TileInspectorAction.OpenDisciplePanel,
                "查阅驻留弟子",
                $"打开弟子谱，查看【{tileName}】附近的休憩 / 论道门人。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "前往宗主中枢",
                $"打开宗主中枢，为【{tileName}】相关堂口调整治理侧重。",
                true)
        };

        var secondaryBinding = summary.AnchorType switch
        {
            TownActivityAnchorType.Farmstead => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "调度阵材法旨",
                $"跳转到宗主中枢，为【{tileName}】调度阵材 / 供养相关法旨。",
                true),
            TownActivityAnchorType.Workshop => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "调度阵务法旨",
                $"跳转到宗主中枢，为【{tileName}】调度阵务 / 工坊执行侧重。",
                true),
            TownActivityAnchorType.Market => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "调度外事法旨",
                $"跳转到宗主中枢，为【{tileName}】调度总坊 / 外事务令。",
                true),
            TownActivityAnchorType.Academy => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "调度推演法旨",
                $"跳转到宗主中枢，为【{tileName}】调度传法院与推演任务。",
                true),
            TownActivityAnchorType.Administration => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "打开宗主中枢",
                $"从【{tileName}】直接进入宗主中枢，查看治理与法令。",
                true),
            TownActivityAnchorType.Leisure => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "调整门规法令",
                $"从【{tileName}】联动到宗主中枢，调整门规、法令与育才方向。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenDisciplePanel,
                "查阅弟子谱",
                $"打开弟子谱，查看与【{tileName}】相关的门人构成。",
                true)
        };

        var tertiaryBinding = summary.AnchorType switch
        {
            TownActivityAnchorType.Farmstead => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "查阅粮仓",
                $"打开仓储，检查【{tileName}】相关的灵谷与基础供养存量。",
                true),
            TownActivityAnchorType.Workshop => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "查阅工料仓",
                $"打开仓储，检查【{tileName}】相关的工料、构件与器材储量。",
                true),
            TownActivityAnchorType.Market => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "查阅流转仓",
                $"打开仓储，检查【{tileName}】相关的交易与流转物资储量。",
                true),
            TownActivityAnchorType.Academy => new TileInspectorActionBinding(
                TileInspectorAction.OpenDisciplePanel,
                "查阅研修弟子",
                $"打开弟子谱，查看【{tileName}】周边的研修与讲法门人。",
                true),
            TownActivityAnchorType.Administration => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "查阅宗门内库",
                $"打开仓储，检查【{tileName}】对应的内库与公共储备。",
                true),
            TownActivityAnchorType.Leisure => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "查阅供养储备",
                $"打开仓储，确认【{tileName}】休憩与论道所需的供养储备是否充足。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "打开仓储",
                $"打开仓储，检查【{tileName}】所依赖的供给与材料储量。",
                true)
        };

        ApplyPrimaryTileInspectorBinding(primaryBinding);
        ApplySecondaryTileInspectorBinding(secondaryBinding);
        ApplyTertiaryTileInspectorBinding(tertiaryBinding);
        ApplyTileInspectorActionHint(
            $"可执行项：{primaryBinding.Text} / {secondaryBinding.Text} / {tertiaryBinding.Text}");
    }

    private void ConfigureCompoundTileInspectorActions(TownMapSelectionSummary summary, string tileName)
    {
        var primaryBinding = summary.SuggestedBuildType switch
        {
            IndustryBuildingType.Agriculture => new TileInspectorActionBinding(
                TileInspectorAction.BuildAgriculture,
                "规划阵材圃",
                $"对【{tileName}】优先规划阵材圃与灵田，吃满当前院域的木水系地脉。",
                true),
            IndustryBuildingType.Workshop => new TileInspectorActionBinding(
                TileInspectorAction.BuildWorkshop,
                "规划傀儡工坊",
                $"对【{tileName}】优先规划工坊坊局，利用当前地块的工务与交通条件。",
                true),
            IndustryBuildingType.Research => new TileInspectorActionBinding(
                TileInspectorAction.BuildResearch,
                "规划传法院",
                $"对【{tileName}】优先规划传法院与静修坊位，放大研修与讲法收益。",
                true),
            IndustryBuildingType.Trade => new TileInspectorActionBinding(
                TileInspectorAction.BuildTrade,
                "规划青云总坊",
                $"对【{tileName}】优先规划总坊与仓阁，强化流转与吞吐。",
                true),
            IndustryBuildingType.Administration => new TileInspectorActionBinding(
                TileInspectorAction.BuildAdministration,
                "规划庶务院",
                $"对【{tileName}】优先规划庶务与巡查节点，提升治理与交通组织。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "规划院域法旨",
                $"打开宗主中枢，为【{tileName}】设定当前院域的建设与治理方向。",
                true)
        };

        var secondaryBinding = summary.ContentKind switch
        {
            TownCellContentKind.Production => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "查看仓储联动",
                $"打开仓储，检查【{tileName}】规划中的产出链和吞吐压力。",
                true),
            TownCellContentKind.Residence => new TileInspectorActionBinding(
                TileInspectorAction.OpenDisciplePanel,
                "查看弟子安置",
                $"打开弟子谱，查看【{tileName}】附近的居舍和恢复对象。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "查看治理联动",
                $"打开宗主中枢，查看【{tileName}】可承接的法旨与院域治理。",
                true)
        };

        var tertiaryBinding = summary.ContentKind switch
        {
            TownCellContentKind.Infrastructure or TownCellContentKind.Special => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "安排巡山与修整",
                $"从【{tileName}】联动宗主中枢，安排巡山、修路或整备事项。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "查看资源准备",
                $"打开仓储，确认【{tileName}】当前院域坊局所需的材料和供给。",
                true)
        };

        ApplyPrimaryTileInspectorBinding(primaryBinding);
        ApplySecondaryTileInspectorBinding(secondaryBinding);
        ApplyTertiaryTileInspectorBinding(tertiaryBinding);
        ApplyTileInspectorActionHint(
            $"可执行项：{primaryBinding.Text} / {secondaryBinding.Text} / {tertiaryBinding.Text}。{GetCompoundActionFocus(summary.StatusText)}");
    }

    private void ApplyPrimaryTileInspectorBinding(TileInspectorActionBinding binding)
    {
        _tileInspectorPrimaryAction = binding.Action;
        ApplyTileInspectorButtonBinding(_tileInspectorPrimaryButton, binding);
    }

    private void ApplySecondaryTileInspectorBinding(TileInspectorActionBinding binding)
    {
        _tileInspectorSecondaryAction = binding.Action;
        ApplyTileInspectorButtonBinding(_tileInspectorSecondaryButton, binding);
    }

    private void ApplyTertiaryTileInspectorBinding(TileInspectorActionBinding binding)
    {
        _tileInspectorTertiaryAction = binding.Action;
        ApplyTileInspectorButtonBinding(_tileInspectorTertiaryButton, binding);
    }

    private static void ApplyTileInspectorButtonBinding(Button? button, TileInspectorActionBinding binding)
    {
        if (button == null)
        {
            return;
        }

        button.Text = binding.Text;
        button.TooltipText = binding.TooltipText;
        button.Disabled = !binding.Enabled || binding.Action == TileInspectorAction.None;
    }

    private void ApplyTileInspectorActionHint(string hintText)
    {
        if (_tileInspectorActionHintLabel == null)
        {
            return;
        }

        _tileInspectorActionHintLabel.Text = hintText;
        _tileInspectorActionHintLabel.TooltipText = hintText;
    }

    private void ConfigureWorldSiteInspectorActions(XianxiaSiteData site, string primaryTypeText)
    {
        var primaryBinding = new TileInspectorActionBinding(
            TileInspectorAction.OpenWorldSitePlaceholder,
            "前往二级地图",
            $"为【{site.Label}】预留的二级地图入口；后续会从这里进入{primaryTypeText}对应的下级地图。",
            true);

        var secondaryBinding = site.PrimaryType switch
        {
            "Sect" => new TileInspectorActionBinding(TileInspectorAction.OpenTaskPanel, "查阅宗门交涉", $"打开宗主中枢，先从全局层面筹备与【{site.Label}】有关的宗门交涉与治理。", true),
            "MortalRealm" => new TileInspectorActionBinding(TileInspectorAction.OpenWarehousePanel, "查阅供养储备", $"打开仓储，检查前往【{site.Label}】所需的供给与物资准备。", true),
            "Market" => new TileInspectorActionBinding(TileInspectorAction.OpenWarehousePanel, "查阅贸易物资", $"打开仓储，查看适合带往【{site.Label}】流转的物资。", true),
            "CultivatorClan" => new TileInspectorActionBinding(TileInspectorAction.OpenDisciplePanel, "查阅门人名录", $"打开弟子谱，查看适合前往【{site.Label}】接触世家的门人与真传。", true),
            "ImmortalCity" => new TileInspectorActionBinding(TileInspectorAction.OpenWarehousePanel, "查阅远行补给", $"打开仓储，查看前往【{site.Label}】这类大型枢纽所需的补给与交易品。", true),
            "Ruin" => new TileInspectorActionBinding(TileInspectorAction.OpenDisciplePanel, "查阅历练人选", $"打开弟子谱，查看适合前往【{site.Label}】的历练人选。", true),
            _ => new TileInspectorActionBinding(TileInspectorAction.OpenTaskPanel, "查看外域筹备", $"打开宗主中枢，查看与【{site.Label}】相关的筹备事项。", true)
        };

        var tertiaryBinding = site.PrimaryType switch
        {
            "Sect" or "CultivatorClan" => new TileInspectorActionBinding(TileInspectorAction.OpenDisciplePanel, "查阅往来门人", $"打开弟子谱，查看适合与【{site.Label}】往来的门人。", true),
            "Market" or "ImmortalCity" => new TileInspectorActionBinding(TileInspectorAction.OpenTaskPanel, "查看外务法旨", $"打开宗主中枢，筹备与【{site.Label}】相关的外事务令。", true),
            "Ruin" => new TileInspectorActionBinding(TileInspectorAction.OpenWarehousePanel, "查阅探险物资", $"打开仓储，确认前往【{site.Label}】所需的探险物资。", true),
            _ => new TileInspectorActionBinding(TileInspectorAction.OpenTaskPanel, "查看外域法旨", $"打开宗主中枢，查看【{site.Label}】相关的外域法旨。", true)
        };

        ApplyPrimaryTileInspectorBinding(primaryBinding);
        ApplySecondaryTileInspectorBinding(secondaryBinding);
        ApplyTertiaryTileInspectorBinding(tertiaryBinding);
        ApplyTileInspectorActionHint($"可执行项：{primaryBinding.Text} / {secondaryBinding.Text} / {tertiaryBinding.Text}。当前点位属于{primaryTypeText}分层。");
    }

    private void ApplyWorldInspectorVisualTone(string primaryType, bool hasSelection)
    {
        if (_tileInspectorTitleLabel == null ||
            _tileInspectorSubtitleLabel == null ||
            _tileInspectorBadgeLabel == null ||
            _tileInspectorStatusValueLabel == null)
        {
            return;
        }

        if (!hasSelection)
        {
            _tileInspectorTitleLabel.AddThemeColorOverride("font_color", new Color(0.176471f, 0.145098f, 0.12549f, 1f));
            _tileInspectorSubtitleLabel.AddThemeColorOverride("font_color", new Color(0.38f, 0.33f, 0.27f, 0.95f));
            _tileInspectorBadgeLabel.AddThemeColorOverride("font_color", new Color(0.52f, 0.45f, 0.31f, 0.95f));
            _tileInspectorStatusValueLabel.AddThemeColorOverride("font_color", new Color(0.34f, 0.29f, 0.24f, 0.92f));
            return;
        }

        var accent = primaryType switch
        {
            "Sect" => new Color(0.27f, 0.50f, 0.31f, 1f),
            "MortalRealm" => new Color(0.56f, 0.41f, 0.20f, 1f),
            "Market" => new Color(0.69f, 0.31f, 0.16f, 1f),
            "CultivatorClan" => new Color(0.48f, 0.38f, 0.17f, 1f),
            "ImmortalCity" => new Color(0.17f, 0.43f, 0.52f, 1f),
            "Ruin" => new Color(0.41f, 0.31f, 0.34f, 1f),
            _ => new Color(0.32f, 0.26f, 0.18f, 1f)
        };

        _tileInspectorTitleLabel.AddThemeColorOverride("font_color", accent);
        _tileInspectorSubtitleLabel.AddThemeColorOverride("font_color", accent.Lightened(0.12f));
        _tileInspectorBadgeLabel.AddThemeColorOverride("font_color", accent);
        _tileInspectorStatusValueLabel.AddThemeColorOverride("font_color", accent.Darkened(0.08f));
    }

    private void ApplyTileInspectorVisualTone(TownMapSelectionSummary summary)
    {
        if (_tileInspectorTitleLabel == null ||
            _tileInspectorSubtitleLabel == null ||
            _tileInspectorBadgeLabel == null ||
            _tileInspectorStatusValueLabel == null)
        {
            return;
        }

        var badgeText = string.IsNullOrWhiteSpace(summary.BadgeText)
            ? TownActivityAnchorVisualRules.GetBadgeText(summary.AnchorType, summary.HasSelection)
            : summary.BadgeText;
        var accentColor = summary.AnchorType != null
            ? TownActivityAnchorVisualRules.GetAccentColor(summary.AnchorType, summary.HasSelection)
            : TownActivityAnchorVisualRules.GetAccentColor(summary.ContentKind, summary.HasSelection);
        var statusColor = summary.AnchorType != null
            ? TownActivityAnchorVisualRules.GetInspectorStatusColor(summary.AnchorType, summary.HasSelection)
            : TownActivityAnchorVisualRules.GetInspectorStatusColor(summary.ContentKind, summary.HasSelection);
        var inkTitleColor = new Color(0.176471f, 0.145098f, 0.12549f, 1f);
        var mutedAccentColor = inkTitleColor.Lerp(accentColor, 0.42f);
        _tileInspectorBadgeLabel.Text = badgeText;
        _tileInspectorBadgeLabel.AddThemeColorOverride("font_color", mutedAccentColor);
        _tileInspectorTitleLabel.AddThemeColorOverride("font_color", inkTitleColor);
        _tileInspectorSubtitleLabel.AddThemeColorOverride("font_color", mutedAccentColor);
        _tileInspectorStatusValueLabel.AddThemeColorOverride("font_color", statusColor);
    }

    private static string GetCompoundActionFocus(string statusText)
    {
        return statusText switch
        {
            "灵池过载" => "当前更适合先缓解高耗坊位，再做扩建。",
            "灵池分流" => "当前更适合补回灵或调整组合顺序。",
            "坊局互扰" => "当前更适合拆散互相掣肘的坊位标签。",
            "稳态成局" => "当前更适合沿既有组合继续叠加协同。",
            "坊局协同" => "当前可以围绕核心坊位继续做配套放大收益。",
            "坊位受限" => "当前优先确认地块定位，再决定是否投入核心建筑。",
            _ => "当前可先按建议坊局落第一轮基础配置。"
        };
    }

    private void OnTileInspectorPrimaryPressed()
    {
        ExecuteTileInspectorAction(_tileInspectorPrimaryAction);
    }

    private void OnTileInspectorSecondaryPressed()
    {
        ExecuteTileInspectorAction(_tileInspectorSecondaryAction);
    }

    private void OnTileInspectorTertiaryPressed()
    {
        ExecuteTileInspectorAction(_tileInspectorTertiaryAction);
    }

    private void ExecuteTileInspectorAction(TileInspectorAction action)
    {
        var tileName = _tileInspectorTitleLabel?.Text ?? "当前地块";

        switch (action)
        {
            case TileInspectorAction.OpenWorldSitePlaceholder:
                OpenSelectedWorldSitePanel();
                break;
            case TileInspectorAction.BuildAgriculture:
                _gameLoop?.BuildIndustryBuilding(IndustryBuildingType.Agriculture);
                break;
            case TileInspectorAction.BuildWorkshop:
                _gameLoop?.BuildIndustryBuilding(IndustryBuildingType.Workshop);
                break;
            case TileInspectorAction.BuildResearch:
                _gameLoop?.BuildIndustryBuilding(IndustryBuildingType.Research);
                break;
            case TileInspectorAction.BuildTrade:
                _gameLoop?.BuildIndustryBuilding(IndustryBuildingType.Trade);
                break;
            case TileInspectorAction.BuildAdministration:
                _gameLoop?.BuildIndustryBuilding(IndustryBuildingType.Administration);
                break;
            case TileInspectorAction.OpenTaskPanel:
                OpenTaskPanel();
                AppendLog($"已从地块检视器打开【{tileName}】对应的宗主中枢入口。");
                break;
            case TileInspectorAction.OpenDisciplePanel:
                OpenDisciplePanel();
                AppendLog($"已从地块检视器查看【{tileName}】相关弟子。");
                break;
            case TileInspectorAction.OpenWarehousePanel:
                OpenWarehousePanel();
                AppendLog($"已从地块检视器打开【{tileName}】相关仓储视图。");
                break;
        }
    }

    private static string ResolveWorldPrimaryTypeText(string primaryType)
    {
        return primaryType switch
        {
            "Sect" => "宗门",
            "Wilderness" => "野外",
            "MortalRealm" => "凡俗国度",
            "CultivatorClan" => "修仙世家",
            "ImmortalCity" => "仙城",
            "Market" => "坊市",
            "Ruin" => "遗迹",
            _ => "外域点位"
        };
    }

    private static string ResolveWorldSecondaryTagText(string secondaryTag)
    {
        return secondaryTag switch
        {
            "MountainGate" => "山门本宗",
            "BranchPeak" => "分峰别院",
            "OuterCourtyard" => "外门院",
            "SectMarket" => "宗门坊",
            "LooseCultivatorBazaar" => "散修集",
            "RoadsideMarket" => "路市",
            "CountySeat" => "府县治所",
            "FarmVillage" => "农庄乡里",
            "RiverTown" => "水镇",
            "AncestralEstate" => "祖庭本家",
            "GuestHall" => "客卿别馆",
            "SpiritFieldManor" => "灵田庄园",
            "ForgeLineage" => "铸器世家",
            "MedicineLineage" => "丹药世家",
            "GrandCity" => "大城",
            "TransitHub" => "驿城",
            "HarborCity" => "河港仙城",
            "FrontierCity" => "边陲仙城",
            "ImperialCultCity" => "王朝修士都城",
            "AncientCave" => "古修洞府",
            "BattlefieldRemnant" => "古战场遗址",
            "SealedDungeon" => "封印地宫",
            "TrialRealm" => "试炼秘境",
            _ => string.IsNullOrWhiteSpace(secondaryTag) ? "未定子类" : secondaryTag
        };
    }

    private static string ResolveWorldRegionText(string regionId)
    {
        return regionId switch
        {
            "SpiritMountain" => "灵脉山域",
            "MortalHeartland" => "凡俗腹地",
            "TradeCorridor" => "商路走廊",
            "FrontierWilds" => "边疆险地",
            "BrokenVeinRuins" => "古迹断脉区",
            _ => "未定区块"
        };
    }

    private static string ResolveWorldRarityText(string rarityTier)
    {
        return rarityTier switch
        {
            "Legendary" => "传说",
            "Rare" => "稀有",
            "Uncommon" => "少见",
            _ => "常见"
        };
    }

    private static string BuildWorldSiteDescription(XianxiaSiteData site, string primaryTypeText, string rarityText)
    {
        var focus = site.PrimaryType switch
        {
            "Sect" => "此地更偏向宗门交涉、传承往来与势力关系。",
            "MortalRealm" => "此地更偏向供养、人口、安民与附庸护持。",
            "Market" => "此地更偏向交易、传闻、物资流转与短期机会。",
            "CultivatorClan" => "此地更偏向血脉、人脉、客卿合作与家族委托。",
            "ImmortalCity" => "此地更偏向大宗交易、驻点经营与跨域枢纽往来。",
            "Ruin" => "此地更偏向探索、试炼、机缘与高风险回报。",
            _ => "此地承接外域层的分层玩法。"
        };

        return $"【{site.Label}】属于{primaryTypeText}层，子类为“{ResolveWorldSecondaryTagText(site.SecondaryTag)}”，当前稀有度为{rarityText}，所在区块为{ResolveWorldRegionText(site.RegionId)}。{focus}";
    }
}
