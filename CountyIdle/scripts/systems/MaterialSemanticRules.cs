using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class MaterialSemanticRules
{
    private sealed record MaterialSemanticDefinition(string DisplayName, string Description);

    private static readonly IReadOnlyDictionary<string, MaterialSemanticDefinition> Definitions =
        new Dictionary<string, MaterialSemanticDefinition>
        {
            [nameof(GameState.Food)] = new("灵谷", "弟子口粮与日常辟谷储备。"),
            [nameof(GameState.Wood)] = new("灵木料", "灵木初加工后的建造与锻器用材。"),
            [nameof(GameState.Stone)] = new("青罡石料", "青罡原石切制后的护山石料。"),
            [nameof(GameState.Gold)] = new("灵石", "宗门流转、赏赐与扩建使用的灵石储备。"),
            [nameof(GameState.ContributionPoints)] = new("贡献点", "宗门内部流通的功绩凭据，用于内务建设与宗务调度。"),
            [nameof(GameState.IndustryTools)] = new("工器", "工坊与采集岗位使用的基础工器。"),
            [nameof(GameState.Timber)] = new("灵木", "采自山林的灵木原材，可锯作梁木与工料。"),
            [nameof(GameState.RawStone)] = new("青罡原石", "山脉采出的原生石材，可切制石料。"),
            [nameof(GameState.Clay)] = new("赤陶土", "可烧制砖瓦与陶器的赤陶土。"),
            [nameof(GameState.Brine)] = new("寒泉卤水", "灵泉盐眼析出的卤水，可煎为辟谷精盐。"),
            [nameof(GameState.Herbs)] = new("灵草", "采自山野的灵草，可配制养气散。"),
            [nameof(GameState.HempFiber)] = new("青麻", "可纺成布匹与轻工衣料的青麻纤维。"),
            [nameof(GameState.Reeds)] = new("青芦", "可编束并参与营造的青芦与蒲材。"),
            [nameof(GameState.Hides)] = new("灵兽皮", "灵兽皮与皮毛原料，可鞣制御寒衣料。"),
            [nameof(GameState.FineSalt)] = new("辟谷精盐", "稳定弟子日常与民生恢复的精炼盐材。"),
            [nameof(GameState.HerbalMedicine)] = new("养气散", "用于治疗与康复的基础丹散。"),
            [nameof(GameState.HempCloth)] = new("青麻布", "供衣袍与轻工制作使用的布匹储备。"),
            [nameof(GameState.Leather)] = new("灵皮革", "供御寒与后续打造使用的鞣制皮材。"),
            [nameof(GameState.IronOre)] = new("玄铁矿", "矿脉中的玄铁矿，是冶炼玄铁锭的前置矿料。"),
            [nameof(GameState.CopperOre)] = new("赤铜矿", "矿脉中的赤铜矿，是冶炼赤铜锭的前置矿料。"),
            [nameof(GameState.Coal)] = new("地火煤", "为冶炼炉与矿坊提供火力的燃料。"),
            [nameof(GameState.RareMaterial)] = new("天材", "用于探险掉落与后续高阶打造的珍稀材料。"),
            [nameof(GameState.CopperIngot)] = new("赤铜锭", "供铜作与后续制造使用的基础锭材。"),
            [nameof(GameState.WroughtIron)] = new("玄铁锭", "供工器、部件与营造使用的基础锭材。"),
            [nameof(GameState.CompositeMaterial)] = new("灵纹复材", "研究转化得到的进阶制造材料。"),
            [nameof(GameState.IndustrialParts)] = new("机关部件", "用于工坊升级与高阶制造的核心部件。"),
            [nameof(GameState.ConstructionMaterials)] = new("护山构件", "用于扩建与稳固宗门设施的构件。")
        };

    public static string GetDisplayName(string inventoryKey)
    {
        return Definitions.TryGetValue(inventoryKey, out var definition)
            ? definition.DisplayName
            : inventoryKey;
    }

    public static string GetDescription(string inventoryKey)
    {
        return Definitions.TryGetValue(inventoryKey, out var definition)
            ? definition.Description
            : inventoryKey;
    }

    public static string FormatDelta(string inventoryKey, int delta)
    {
        return $"{GetDisplayName(inventoryKey)}{delta:+#;-#;0}";
    }
}
