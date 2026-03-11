using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class TownActivityAnchorVisualRules
{
    private static readonly Color DefaultAccentColor = new("AFC6BF");
    private static readonly Color FarmsteadAccentColor = new("79D68C");
    private static readonly Color WorkshopAccentColor = new("E1B66A");
    private static readonly Color MarketAccentColor = new("F2C063");
    private static readonly Color AcademyAccentColor = new("8CB7FF");
    private static readonly Color AdministrationAccentColor = new("9AD7C9");
    private static readonly Color LeisureAccentColor = new("D39AE6");
    private static readonly Color InfrastructureAccentColor = new("A5C5D6");
    private static readonly Color ProductionAccentColor = new("D9B06D");
    private static readonly Color ServiceAccentColor = new("97CBB7");
    private static readonly Color ResidenceAccentColor = new("C7A7E5");
    private static readonly Color SpecialAccentColor = new("D68E8E");
    private static readonly Color EmptyAccentColor = new("BBC5B4");

    public static string GetBadgeText(TownActivityAnchorType? anchorType, bool hasSelection)
    {
        if (!hasSelection || anchorType == null)
        {
            return "未选中地块";
        }

        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => "🌾 灵田 / 供养",
            TownActivityAnchorType.Workshop => "🛠 工坊 / 阵务",
            TownActivityAnchorType.Market => "💰 总坊 / 外事",
            TownActivityAnchorType.Academy => "📜 传法院 / 推演",
            TownActivityAnchorType.Administration => "🏛 庶务 / 治理",
            TownActivityAnchorType.Leisure => "☯ 休憩 / 论道",
            _ => "⛰ 山门地块"
        };
    }

    public static Color GetAccentColor(TownActivityAnchorType? anchorType, bool hasSelection = true)
    {
        if (!hasSelection || anchorType == null)
        {
            return DefaultAccentColor;
        }

        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => FarmsteadAccentColor,
            TownActivityAnchorType.Workshop => WorkshopAccentColor,
            TownActivityAnchorType.Market => MarketAccentColor,
            TownActivityAnchorType.Academy => AcademyAccentColor,
            TownActivityAnchorType.Administration => AdministrationAccentColor,
            TownActivityAnchorType.Leisure => LeisureAccentColor,
            _ => DefaultAccentColor
        };
    }

    public static Color GetInspectorStatusColor(TownActivityAnchorType? anchorType, bool hasSelection = true)
    {
        return !hasSelection || anchorType == null
            ? Colors.White
            : GetAccentColor(anchorType).Lightened(0.18f);
    }

    public static Color GetMapBaseColor(TownActivityAnchorType anchorType)
    {
        var accent = GetAccentColor(anchorType);
        return new Color(accent.R, accent.G, accent.B, 0.92f);
    }

    public static Color GetSelectionHaloColor(TownActivityAnchorType? anchorType)
    {
        var glowColor = GetAccentColor(anchorType).Lightened(0.56f);
        return new Color(glowColor.R, glowColor.G, glowColor.B, 0.10f);
    }

    public static Color GetSelectionGlowColor(TownActivityAnchorType? anchorType)
    {
        var glowColor = GetAccentColor(anchorType).Lightened(0.34f);
        return new Color(glowColor.R, glowColor.G, glowColor.B, 0.22f);
    }

    public static Color GetSelectionOutlineColor(TownActivityAnchorType? anchorType)
    {
        var outlineColor = GetAccentColor(anchorType).Lightened(0.22f);
        return new Color(outlineColor.R, outlineColor.G, outlineColor.B, 0.96f);
    }

    public static Color GetSelectionPathColor(TownActivityAnchorType? anchorType)
    {
        var pathColor = GetAccentColor(anchorType).Lightened(0.08f);
        return new Color(pathColor.R, pathColor.G, pathColor.B, 0.78f);
    }

    public static Color GetAccentColor(TownCellContentKind? contentKind, bool hasSelection = true)
    {
        if (!hasSelection || contentKind == null)
        {
            return DefaultAccentColor;
        }

        return contentKind switch
        {
            TownCellContentKind.Infrastructure => InfrastructureAccentColor,
            TownCellContentKind.Production => ProductionAccentColor,
            TownCellContentKind.Service => ServiceAccentColor,
            TownCellContentKind.Residence => ResidenceAccentColor,
            TownCellContentKind.Special => SpecialAccentColor,
            TownCellContentKind.Empty => EmptyAccentColor,
            _ => DefaultAccentColor
        };
    }

    public static Color GetInspectorStatusColor(TownCellContentKind? contentKind, bool hasSelection = true)
    {
        return !hasSelection || contentKind == null
            ? Colors.White
            : GetAccentColor(contentKind).Lightened(0.18f);
    }

    public static Color GetSelectionHaloColor(TownCellContentKind? contentKind)
    {
        var glowColor = GetAccentColor(contentKind).Lightened(0.56f);
        return new Color(glowColor.R, glowColor.G, glowColor.B, 0.10f);
    }

    public static Color GetSelectionGlowColor(TownCellContentKind? contentKind)
    {
        var glowColor = GetAccentColor(contentKind).Lightened(0.34f);
        return new Color(glowColor.R, glowColor.G, glowColor.B, 0.22f);
    }

    public static Color GetSelectionOutlineColor(TownCellContentKind? contentKind)
    {
        var outlineColor = GetAccentColor(contentKind).Lightened(0.22f);
        return new Color(outlineColor.R, outlineColor.G, outlineColor.B, 0.96f);
    }
}
