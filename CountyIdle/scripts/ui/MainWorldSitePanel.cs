using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle;

public partial class Main
{
    private VBoxContainer? _worldSitePanelRootVBox;
    private readonly WorldSiteLocalMapGeneratorSystem _worldSiteSandboxGenerator = new();
    private Label? _worldSitePanelTitleLabel;
    private Label? _worldSitePanelSubtitleLabel;
    private Label? _worldSitePanelTypeValueLabel;
    private Label? _worldSitePanelRegionValueLabel;
    private Label? _worldSitePanelRarityValueLabel;
    private Label? _worldSitePanelUnlockValueLabel;
    private Label? _worldSitePanelFocusValueLabel;
    private Label? _worldSitePanelYieldValueLabel;
    private Label? _worldSitePanelRiskValueLabel;
    private Label? _worldSitePanelDescriptionLabel;
    private Label? _worldSitePanelHintLabel;
    private Button? _worldSitePanelBackButton;
    private Button? _worldSitePanelActionButton;
    private SectMapViewSystem? _worldSiteSandboxMapView;

    private void BindWorldSitePanelNodes()
    {
        _worldSitePanelRootVBox = GetNodeOrNull<VBoxContainer>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox");
        _worldSitePanelTitleLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/HeaderBox/TitleLabel");
        _worldSitePanelSubtitleLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/HeaderBox/SubtitleLabel");
        _worldSitePanelTypeValueLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/TypeCard/TypeMargin/TypeVBox/Value");
        _worldSitePanelRegionValueLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/RegionCard/RegionMargin/RegionVBox/Value");
        _worldSitePanelRarityValueLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/RarityCard/RarityMargin/RarityVBox/Value");
        _worldSitePanelUnlockValueLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/UnlockCard/UnlockMargin/UnlockVBox/Value");
        _worldSitePanelFocusValueLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/FocusCard/FocusMargin/FocusVBox/Value");
        _worldSitePanelYieldValueLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/YieldCard/YieldMargin/YieldVBox/Value");
        _worldSitePanelRiskValueLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/RiskCard/RiskMargin/RiskVBox/Value");
        _worldSitePanelDescriptionLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/DescriptionCard/DescriptionMargin/DescriptionLabel");
        _worldSitePanelHintLabel = GetNodeOrNull<Label>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/HintLabel");
        _worldSitePanelBackButton = GetNodeOrNull<Button>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/ActionRow/BackButton");
        _worldSitePanelActionButton = GetNodeOrNull<Button>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/ActionRow/ActionButton");
        EnsureWorldSiteSandboxMapView();

        if (_worldSiteSandboxMapView != null)
        {
            _worldSiteSandboxMapView.SelectionSummaryChanged -= OnWorldSiteSandboxSelectionSummaryChanged;
            _worldSiteSandboxMapView.SelectionSummaryChanged += OnWorldSiteSandboxSelectionSummaryChanged;
        }

        if (_worldSitePanelBackButton != null)
        {
            _worldSitePanelBackButton.Pressed += OnWorldSitePanelBackPressed;
        }

        if (_worldSitePanelActionButton != null)
        {
            _worldSitePanelActionButton.Pressed += OnWorldSitePanelActionPressed;
        }

        RefreshWorldSitePanel();
    }

    private void ClearWorldSitePanelNodes()
    {
        if (_worldSitePanelBackButton != null)
        {
            _worldSitePanelBackButton.Pressed -= OnWorldSitePanelBackPressed;
        }

        if (_worldSitePanelActionButton != null)
        {
            _worldSitePanelActionButton.Pressed -= OnWorldSitePanelActionPressed;
        }

        if (_worldSiteSandboxMapView != null)
        {
            _worldSiteSandboxMapView.SelectionSummaryChanged -= OnWorldSiteSandboxSelectionSummaryChanged;
        }

        _worldSitePanelRootVBox = null;
        _worldSitePanelTitleLabel = null;
        _worldSitePanelSubtitleLabel = null;
        _worldSitePanelTypeValueLabel = null;
        _worldSitePanelRegionValueLabel = null;
        _worldSitePanelRarityValueLabel = null;
        _worldSitePanelUnlockValueLabel = null;
        _worldSitePanelFocusValueLabel = null;
        _worldSitePanelYieldValueLabel = null;
        _worldSitePanelRiskValueLabel = null;
        _worldSitePanelDescriptionLabel = null;
        _worldSitePanelHintLabel = null;
        _worldSitePanelBackButton = null;
        _worldSitePanelActionButton = null;
        _worldSiteSandboxMapView = null;
    }

    private void RefreshWorldSitePanel()
    {
        if (_worldSitePanelTitleLabel == null ||
            _worldSitePanelSubtitleLabel == null ||
            _worldSitePanelTypeValueLabel == null ||
            _worldSitePanelRegionValueLabel == null ||
            _worldSitePanelRarityValueLabel == null ||
            _worldSitePanelUnlockValueLabel == null ||
            _worldSitePanelFocusValueLabel == null ||
            _worldSitePanelYieldValueLabel == null ||
            _worldSitePanelRiskValueLabel == null ||
            _worldSitePanelDescriptionLabel == null ||
            _worldSitePanelHintLabel == null ||
            _worldSitePanelActionButton == null)
        {
            return;
        }

        var site = _worldMapRenderer?.SelectedWorldSite;
        if (site == null)
        {
            _worldSitePanelTitleLabel.Text = "二级地图";
            _worldSitePanelSubtitleLabel.Text = "尚未选中世界点位";
            _worldSitePanelTypeValueLabel.Text = "未选中";
            _worldSitePanelRegionValueLabel.Text = "未定区块";
            _worldSitePanelRarityValueLabel.Text = "常见";
            _worldSitePanelUnlockValueLabel.Text = "待判定";
            _worldSitePanelFocusValueLabel.Text = "先在世界舆图里点选一个站点。";
            _worldSitePanelYieldValueLabel.Text = "不同类型的点位会回流不同资源、关系或机缘。";
            _worldSitePanelRiskValueLabel.Text = "未选中时暂不显示具体风险。";
            _worldSitePanelDescriptionLabel.Text = "先返回世界舆图并点选一个外域点位，再从左侧检视器或这里进入对应的二级地图入口。";
            _worldSitePanelHintLabel.Text = "当前为二级地图占位页，后续会按点位类型展开不同的下级地图模板。";
            _worldSitePanelActionButton.Text = "返回后再选择点位";
            _worldSitePanelActionButton.Disabled = true;
            if (_worldSiteSandboxMapView != null)
            {
                _worldSiteSandboxMapView.ClearExternalMap();
                CallWorldPanelVisualFx("set_world_site_sandbox_visible", false);
            }
            CallWorldPanelVisualFx("apply_world_site_tone", "world");
            CallWorldPanelVisualFx("pulse_world_site_panel");
            return;
        }

        var sourceCell = _worldMapRenderer?.GetWorldCellForSite(site);
        var sandboxMap = _worldSiteSandboxGenerator.GenerateSandboxMap(site, sourceCell);
        var primaryTypeText = ResolveWorldPrimaryTypeText(site.PrimaryType);
        _worldSitePanelTitleLabel.Text = site.Label;
        _worldSitePanelSubtitleLabel.Text = $"{primaryTypeText} · {ResolveWorldSecondaryTagText(site.SecondaryTag)}";
        _worldSitePanelTypeValueLabel.Text = primaryTypeText;
        _worldSitePanelRegionValueLabel.Text = ResolveWorldRegionText(site.RegionId);
        _worldSitePanelRarityValueLabel.Text = ResolveWorldRarityText(site.RarityTier);
        _worldSitePanelUnlockValueLabel.Text = site.UnlockTier switch
        {
            <= 0 => "开局可至",
            1 => "中期开启",
            _ => "后期开启"
        };
        var templateInfo = BuildWorldSiteTemplateInfo(site);
        _worldSitePanelFocusValueLabel.Text = templateInfo.FocusText;
        _worldSitePanelYieldValueLabel.Text = templateInfo.YieldText;
        _worldSitePanelRiskValueLabel.Text = templateInfo.RiskText;
        _worldSitePanelDescriptionLabel.Text = BuildWorldSiteDescription(site, primaryTypeText, ResolveWorldRarityText(site.RarityTier));
        _worldSitePanelHintLabel.Text = BuildWorldSiteHint(site, sourceCell);
        _worldSitePanelActionButton.Text = ResolveWorldSiteActionText(site);
        _worldSitePanelActionButton.Disabled = false;
        if (_worldSiteSandboxMapView != null)
        {
            _worldSiteSandboxMapView.SetExternalMap(
                sandboxMap,
                $"{site.Label} · 局部沙盘",
                $"左键点选局部 hex 检视当前二级地图地块。当前依据 {site.PrimaryType} / {site.SecondaryTag} 生成。");
            CallWorldPanelVisualFx("set_world_site_sandbox_visible", true);
        }
        CallWorldPanelVisualFx("apply_world_site_tone", site.PrimaryType);
        CallWorldPanelVisualFx("pulse_world_site_panel");
    }

    private void OnWorldSiteSandboxSelectionSummaryChanged(TownMapSelectionSummary summary)
    {
        if (_currentMapTab != MapTab.WorldSite)
        {
            return;
        }

        if (summary.HasSelection)
        {
            ApplySectTileInspectorSummary(summary);
            return;
        }

        ApplyWorldSiteInspectorSummary(_worldMapRenderer?.SelectedWorldSite);
    }

    private void EnsureWorldSiteSandboxMapView()
    {
        _worldSiteSandboxMapView = GetNodeOrNull<SectMapViewSystem>($"{CenterMapPagesPath}/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/GeneratedSecondarySandboxView");
        if (_worldSiteSandboxMapView == null)
        {
            return;
        }

        CallWorldPanelVisualFx("style_world_site_sandbox_shell", _worldSiteSandboxMapView);
    }

    private void OpenSelectedWorldSitePanel()
    {
        if (_worldMapRenderer?.SelectedWorldSite == null)
        {
            AppendLog("当前尚未选中世界点位，无法进入二级地图入口。");
            return;
        }

        RefreshWorldSitePanel();
        SetMapTab(MapTab.WorldSite);
    }

    private void OnWorldSitePanelBackPressed()
    {
        SetMapTab(MapTab.World);
    }

    private void OnWorldSitePanelActionPressed()
    {
        var site = _worldMapRenderer?.SelectedWorldSite;
        if (site == null)
        {
            return;
        }

        switch (site.PrimaryType)
        {
            case "Sect":
                OpenTaskPanel();
                AppendLog($"已从二级地图入口页转往【{site.Label}】相关的宗门筹备。");
                break;
            case "MortalRealm":
            case "Market":
            case "ImmortalCity":
                OpenWarehousePanel();
                AppendLog($"已从二级地图入口页查阅前往【{site.Label}】所需的物资准备。");
                break;
            case "Wilderness":
            case "CultivatorClan":
            case "Ruin":
                OpenDisciplePanel();
                AppendLog($"已从二级地图入口页查阅前往【{site.Label}】的人选与门人准备。");
                break;
            default:
                OpenTaskPanel();
                AppendLog($"已从二级地图入口页查阅【{site.Label}】相关的外域筹备。");
                break;
        }
    }

    private static string ResolveWorldSiteActionText(XianxiaSiteData site)
    {
        return site.PrimaryType switch
        {
            "Sect" => "转往宗门筹备",
            "MortalRealm" => "查阅供养准备",
            "Market" => "查阅贸易准备",
            "Wilderness" => "查阅历练人选",
            "CultivatorClan" => "查阅往来人选",
            "ImmortalCity" => "查阅远行补给",
            "Ruin" => "查阅历练人选",
            _ => "查阅相关筹备"
        };
    }

    private static string BuildWorldSiteHint(XianxiaSiteData site, XianxiaHexCellData? sourceCell)
    {
        var detailHint = sourceCell == null
            ? "当前使用点位基础语义生成下层地图。"
            : $"当前下层地图将按 {sourceCell.Biome} / {sourceCell.Terrain} / {sourceCell.Water} / 灵气 {sourceCell.QiDensity:0.00} 生成。";

        var primaryHint = site.PrimaryType switch
        {
            "Sect" => "当前占位页后续将承接宗门访问、结盟、论道与传承交换等宗门型二级地图。",
            "MortalRealm" => "当前占位页后续将承接凡俗国度的供养、安民、附庸护持与苗子招揽等二级地图。",
            "Market" => "当前占位页后续将承接坊市交易、传闻、短期委托与黑白市机会等二级地图。",
            "Wilderness" => "当前占位页后续将承接野外采集、路径推进、遭遇事件与局部历练等二级地图。",
            "CultivatorClan" => "当前占位页后续将承接世家关系、客卿合作、血脉与专精资源往来等二级地图。",
            "ImmortalCity" => "当前占位页后续将承接仙城驻点、大宗交易、拍卖与跨域任务等二级地图。",
            "Ruin" => "当前占位页后续将承接遗迹探索、试炼、机缘与高风险回报等二级地图。",
            _ => "当前占位页后续将承接该点位的专属二级地图。"
        };
        return $"{detailHint}{primaryHint}";
    }

    private static WorldSiteTemplateInfo BuildWorldSiteTemplateInfo(XianxiaSiteData site)
    {
        return site.PrimaryType switch
        {
            "Sect" => new WorldSiteTemplateInfo(
                "访问、结盟、论道、交换传承与驻点往来。",
                "功法、人脉、弟子来源、盟友支持。",
                "关系恶化、资源依赖、宗门冲突。"),
            "MortalRealm" => new WorldSiteTemplateInfo(
                "护持、赈济、安民、征调供给与招收苗子。",
                "人口、粮草、供奉、稳定度、苗子来源。",
                "民乱、灾荒、失德、供给断裂。"),
            "Market" => new WorldSiteTemplateInfo(
                "短频交易、淘货、打听消息、接短委托。",
                "稀缺货、流通资源、传闻、临时机会。",
                "被坑、价格波动、黑市风险、真假难辨。"),
            "Wilderness" => new WorldSiteTemplateInfo(
                "探路、采集、遭遇战、护送与野外历练推进。",
                "材料、情报、路径控制、局部机缘与历练收益。",
                "迷路、伤损、妖兽袭扰、补给见底。"),
            "CultivatorClan" => new WorldSiteTemplateInfo(
                "走关系、谈合作、接家族委托、换秘术与门客。",
                "血脉资源、人情网络、专属材料、客卿机会。",
                "结怨、失信、派系站队、家族禁忌。"),
            "ImmortalCity" => new WorldSiteTemplateInfo(
                "大宗交易、拍卖、驻点经营、跨域任务中转。",
                "高级交易渠道、情报、委托、跨势力接触。",
                "竞争、税费、治安波动、声望门槛。"),
            "Ruin" => new WorldSiteTemplateInfo(
                "探索、破阵、试炼、夺宝与传承触发。",
                "稀有掉落、古传承、法器胚、高价值线索。",
                "高战损、封印、机关、一次性失败成本。"),
            _ => new WorldSiteTemplateInfo(
                "承接当前世界点位的下级地图玩法。",
                "回流宗门的资源、关系或情报。",
                "尚待后续根据类型细化。")
        };
    }

    private sealed record WorldSiteTemplateInfo(
        string FocusText,
        string YieldText,
        string RiskText);
}
