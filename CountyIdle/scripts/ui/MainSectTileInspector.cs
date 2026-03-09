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
    private Label? _tileInspectorStatusValueLabel;
    private Label? _tileInspectorResidentValueLabel;
    private Label? _tileInspectorTransitValueLabel;
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
        _tileInspectorStatusValueLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/StatusBox/AttrVBox/AttrValue");
        _tileInspectorResidentValueLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/ResidentBox/AttrVBox/AttrValue");
        _tileInspectorTransitValueLabel = GetNodeOrNull<Label>($"{LeftPanelPath}/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/TransitBox/AttrVBox/AttrValue");
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
        _tileInspectorStatusValueLabel = null;
        _tileInspectorResidentValueLabel = null;
        _tileInspectorTransitValueLabel = null;
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

    private void ApplySectTileInspectorSummary(TownMapSelectionSummary summary)
    {
        if (_tileInspectorTitleLabel == null ||
            _tileInspectorSubtitleLabel == null ||
            _tileInspectorBadgeLabel == null ||
            _tileInspectorStatusValueLabel == null ||
            _tileInspectorResidentValueLabel == null ||
            _tileInspectorTransitValueLabel == null ||
            _tileInspectorLocationValueLabel == null ||
            _tileInspectorDescriptionLabel == null ||
            _tileInspectorActionHintLabel == null)
        {
            return;
        }

        _tileInspectorTitleLabel.Text = summary.Title;
        _tileInspectorSubtitleLabel.Text = summary.Subtitle;
        _tileInspectorStatusValueLabel.Text = summary.StatusText;
        _tileInspectorResidentValueLabel.Text = summary.ResidentText;
        _tileInspectorTransitValueLabel.Text = summary.TransitText;
        _tileInspectorLocationValueLabel.Text = summary.LocationText;
        _tileInspectorDescriptionLabel.Text = summary.DescriptionText;

        ConfigureTileInspectorActions(summary);
        ApplyTileInspectorVisualTone(summary.AnchorType, summary.HasSelection);
    }

    private void ConfigureTileInspectorActions(TownMapSelectionSummary summary)
    {
        if (!summary.HasSelection)
        {
            ApplyPrimaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "🔍 选择地块后可用",
                "请先在中央六边形沙盘上点选一个可交互 tile。",
                false));
            ApplySecondaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "👥 选择地块后可用",
                "选中 tile 后，这里会出现该地块的辅助操作。",
                false));
            ApplyTertiaryTileInspectorBinding(new TileInspectorActionBinding(
                TileInspectorAction.None,
                "📦 选择地块后可用",
                "选中 tile 后，这里会出现与该地块相关的补充入口。",
                false));
            ApplyTileInspectorActionHint("可执行项：请先点选中央 hex tile，左侧才会显示该地块的具体可操作项。");
            return;
        }

        var tileName = string.IsNullOrWhiteSpace(summary.Title) ? "当前地块" : summary.Title;

        var primaryBinding = summary.AnchorType switch
        {
            TownActivityAnchorType.Farmstead => new TileInspectorActionBinding(
                TileInspectorAction.BuildAgriculture,
                "🔨 扩建阵材圃",
                $"对【{tileName}】追加灵植 / 灵田产能，强化阵材与供养链路。",
                true),
            TownActivityAnchorType.Workshop => new TileInspectorActionBinding(
                TileInspectorAction.BuildWorkshop,
                "🔨 扩建傀儡工坊",
                $"对【{tileName}】追加营造与工器位，强化阵务与建设链路。",
                true),
            TownActivityAnchorType.Market => new TileInspectorActionBinding(
                TileInspectorAction.BuildTrade,
                "🔨 扩建青云总坊",
                $"对【{tileName}】追加流转与外事务位，提升总坊回流。",
                true),
            TownActivityAnchorType.Academy => new TileInspectorActionBinding(
                TileInspectorAction.BuildResearch,
                "🔨 扩建传法院",
                $"对【{tileName}】追加讲法与推演位，强化研修与突破链路。",
                true),
            TownActivityAnchorType.Administration => new TileInspectorActionBinding(
                TileInspectorAction.BuildAdministration,
                "🔨 扩建庶务殿",
                $"对【{tileName}】追加庶务与执事位，提升治理执行容量。",
                true),
            TownActivityAnchorType.Leisure => new TileInspectorActionBinding(
                TileInspectorAction.OpenDisciplePanel,
                "👥 查看驻留弟子",
                $"打开弟子谱，查看【{tileName}】附近的休憩 / 论道门人。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "📜 前往宗主中枢",
                $"打开宗主中枢，为【{tileName}】相关堂口调整治理侧重。",
                true)
        };

        var secondaryBinding = summary.AnchorType switch
        {
            TownActivityAnchorType.Farmstead => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "📜 调度阵材法旨",
                $"跳转到宗主中枢，为【{tileName}】调度阵材 / 供养相关法旨。",
                true),
            TownActivityAnchorType.Workshop => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "📜 调度阵务法旨",
                $"跳转到宗主中枢，为【{tileName}】调度阵务 / 工坊执行侧重。",
                true),
            TownActivityAnchorType.Market => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "📜 调度外事法旨",
                $"跳转到宗主中枢，为【{tileName}】调度总坊 / 外事务令。",
                true),
            TownActivityAnchorType.Academy => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "📜 调度推演法旨",
                $"跳转到宗主中枢，为【{tileName}】调度传法院与推演任务。",
                true),
            TownActivityAnchorType.Administration => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "📜 打开宗主中枢",
                $"从【{tileName}】直接进入宗主中枢，查看治理与法令。",
                true),
            TownActivityAnchorType.Leisure => new TileInspectorActionBinding(
                TileInspectorAction.OpenTaskPanel,
                "📜 调整门规法令",
                $"从【{tileName}】联动到宗主中枢，调整门规、法令与育才方向。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenDisciplePanel,
                "👥 查看弟子谱",
                $"打开弟子谱，查看与【{tileName}】相关的门人构成。",
                true)
        };

        var tertiaryBinding = summary.AnchorType switch
        {
            TownActivityAnchorType.Farmstead => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "📦 查看粮仓",
                $"打开仓储，检查【{tileName}】相关的灵谷与基础供养存量。",
                true),
            TownActivityAnchorType.Workshop => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "📦 查看工料仓",
                $"打开仓储，检查【{tileName}】相关的工料、构件与器材储量。",
                true),
            TownActivityAnchorType.Market => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "📦 查看流转仓",
                $"打开仓储，检查【{tileName}】相关的交易与流转物资储量。",
                true),
            TownActivityAnchorType.Academy => new TileInspectorActionBinding(
                TileInspectorAction.OpenDisciplePanel,
                "👥 查看研修弟子",
                $"打开弟子谱，查看【{tileName}】周边的研修与讲法门人。",
                true),
            TownActivityAnchorType.Administration => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "📦 查看宗门内库",
                $"打开仓储，检查【{tileName}】对应的内库与公共储备。",
                true),
            TownActivityAnchorType.Leisure => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "📦 查看供养储备",
                $"打开仓储，确认【{tileName}】休憩与论道所需的供养储备是否充足。",
                true),
            _ => new TileInspectorActionBinding(
                TileInspectorAction.OpenWarehousePanel,
                "📦 打开仓储",
                $"打开仓储，检查【{tileName}】所依赖的供给与材料储量。",
                true)
        };

        ApplyPrimaryTileInspectorBinding(primaryBinding);
        ApplySecondaryTileInspectorBinding(secondaryBinding);
        ApplyTertiaryTileInspectorBinding(tertiaryBinding);
        ApplyTileInspectorActionHint(
            $"可执行项：{primaryBinding.Text} / {secondaryBinding.Text} / {tertiaryBinding.Text}");
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

    private void ApplyTileInspectorVisualTone(TownActivityAnchorType? anchorType, bool hasSelection)
    {
        if (_tileInspectorTitleLabel == null ||
            _tileInspectorSubtitleLabel == null ||
            _tileInspectorBadgeLabel == null ||
            _tileInspectorStatusValueLabel == null)
        {
            return;
        }

        var badgeText = TownActivityAnchorVisualRules.GetBadgeText(anchorType, hasSelection);
        var accentColor = TownActivityAnchorVisualRules.GetAccentColor(anchorType, hasSelection);
        var statusColor = TownActivityAnchorVisualRules.GetInspectorStatusColor(anchorType, hasSelection);
        var inkTitleColor = new Color(0.176471f, 0.145098f, 0.12549f, 1f);
        var mutedAccentColor = inkTitleColor.Lerp(accentColor, 0.42f);
        _tileInspectorBadgeLabel.Text = badgeText;
        _tileInspectorBadgeLabel.AddThemeColorOverride("font_color", mutedAccentColor);
        _tileInspectorTitleLabel.AddThemeColorOverride("font_color", inkTitleColor);
        _tileInspectorSubtitleLabel.AddThemeColorOverride("font_color", mutedAccentColor);
        _tileInspectorStatusValueLabel.AddThemeColorOverride("font_color", statusColor);
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
}
