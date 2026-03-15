using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed record SectPeakSupportDefinition(
	SectPeakSupportType SupportType,
	string DisplayName,
	string ShortEffect,
	string Description,
	string ModifierSummary);

public static class SectPeakSupportRules
{
	private const SectPeakSupportType DefaultSupport = SectPeakSupportType.Balanced;

	private static readonly IReadOnlyList<SectPeakSupportDefinition> Definitions =
	[
		new(
			SectPeakSupportType.Balanced,
			"诸峰均衡",
			"诸峰常态轮转",
			"不额外偏向单峰，维持当前宗门常态协同与轮转支援。",
			"不提供额外峰脉加成，适合作为常态治宗底盘。"),
		new(
			SectPeakSupportType.Qingyun,
			"青云峰",
			"总殿统筹更稳",
			"由内务总殿、外事总殿与传功总殿统一压实调度，适合强调中枢统筹与内务结算。",
			"贡献回流 +12%，每小时民心 +0.10。"),
		new(
			SectPeakSupportType.Tianyan,
			"天衍峰",
			"阵研与推演提速",
			"总枢殿、阵研阁、理法堂与推演堂集中支援，适合推进阵法研发与峰务迭代。",
			"研修回流 +12%，贡献回流 +6%，工器锻制 +8%。"),
		new(
			SectPeakSupportType.Tianshu,
			"天枢峰",
			"外务与物流更畅",
			"由总务殿、鸿胪司、阵市与玄仓坪加压协同，适合对外通商与峰内流通。",
			"灵石回流 +12%，贡献回流 +6%。"),
		new(
			SectPeakSupportType.Tianji,
			"天机峰",
			"传承研修更强",
			"衍法阁、传功总院与试炼幻境集中开课，适合加速阵道深造与传承沉淀。",
			"研修回流 +15%，每小时民心 +0.05。"),
		new(
			SectPeakSupportType.Tiangong,
			"天工峰",
			"制造与工器更强",
			"铸机阁、阵基殿、傀儡工坊与资源调配司优先支援，适合推进营造与工器保障。",
			"贡献回流 +8%，工器锻制 +18%。"),
		new(
			SectPeakSupportType.Tianquan,
			"天权峰",
			"护山戒备更强",
			"承山堂、破阵营、天谕殿与执法总堂加强值守，适合压低威胁与稳住秩序。",
			"贡献回流 +10%，每小时威胁 -0.70。"),
		new(
			SectPeakSupportType.Tianyuan,
			"天元峰",
			"后勤与育成更强",
			"济世堂、百兽苑与清音阁优先支援门人生息与恢复，适合稳住后勤与人口盘。",
			"食物回流 +10%，人口增长 +8%，每小时民心 +0.18。"),
		new(
			SectPeakSupportType.Tianheng,
			"天衡峰",
			"密防与肃查更强",
			"暗部、风纪堂、影卫堂与天机阁压实保密与反间链路，适合降低内外部风险。",
			"研修回流 +4%，每小时威胁 -0.50。"),
		new(
			SectPeakSupportType.PillarPeaks,
			"其余支柱峰",
			"诸堂并援更稳",
			"镇岳峰、丹鼎峰、神工峰、御灵峰、藏剑峰、万法峰、无影峰与烟霞峰联手补位。",
			"食物/灵石/贡献/研修回流各 +5%，人口增长 +4%，每小时民心 +0.05、威胁 -0.10，工器锻制 +5%。")
	];

	private static readonly IReadOnlyDictionary<SectPeakSupportType, SectPeakSupportDefinition> Lookup =
		Definitions.ToDictionary(static item => item.SupportType);

	public static void EnsureDefaults(GameState state)
	{
		state.ActivePeakSupport = NormalizeEnumValue(state.ActivePeakSupport, DefaultSupport);
	}

	public static SectPeakSupportDefinition GetDefinition(SectPeakSupportType supportType)
	{
		return Lookup[supportType];
	}

	public static SectPeakSupportDefinition GetActiveDefinition(GameState state)
	{
		EnsureDefaults(state);
		Enum.TryParse<SectPeakSupportType>(state.ActivePeakSupport, out var supportType);
		return Lookup[supportType];
	}

	public static SectPeakSupportType GetActiveSupport(GameState state)
	{
		EnsureDefaults(state);
		Enum.TryParse<SectPeakSupportType>(state.ActivePeakSupport, out var supportType);
		return supportType;
	}

	public static SectPeakSupportDefinition SetActiveSupport(GameState state, SectPeakSupportType supportType)
	{
		state.ActivePeakSupport = supportType.ToString();
		return Lookup[supportType];
	}

	public static void ResetToBalanced(GameState state)
	{
		state.ActivePeakSupport = DefaultSupport.ToString();
	}

	public static double GetFoodYieldModifier(GameState state)
	{
		return GetActiveSupport(state) switch
		{
			SectPeakSupportType.Tianyuan => 1.10,
			SectPeakSupportType.PillarPeaks => 1.05,
			_ => 1.0
		};
	}

	public static double GetGoldYieldModifier(GameState state)
	{
		return GetActiveSupport(state) switch
		{
			SectPeakSupportType.Tianshu => 1.12,
			SectPeakSupportType.PillarPeaks => 1.05,
			_ => 1.0
		};
	}

	public static double GetContributionYieldModifier(GameState state)
	{
		return GetActiveSupport(state) switch
		{
			SectPeakSupportType.Qingyun => 1.12,
			SectPeakSupportType.Tianyan => 1.06,
			SectPeakSupportType.Tianshu => 1.06,
			SectPeakSupportType.Tiangong => 1.08,
			SectPeakSupportType.Tianquan => 1.10,
			SectPeakSupportType.PillarPeaks => 1.05,
			_ => 1.0
		};
	}

	public static double GetResearchYieldModifier(GameState state)
	{
		return GetActiveSupport(state) switch
		{
			SectPeakSupportType.Tianyan => 1.12,
			SectPeakSupportType.Tianji => 1.15,
			SectPeakSupportType.Tianheng => 1.04,
			SectPeakSupportType.PillarPeaks => 1.05,
			_ => 1.0
		};
	}

	public static double GetPopulationGrowthModifier(GameState state)
	{
		return GetActiveSupport(state) switch
		{
			SectPeakSupportType.Tianyuan => 1.08,
			SectPeakSupportType.PillarPeaks => 1.04,
			_ => 1.0
		};
	}

	public static double GetHourlyHappinessDelta(GameState state)
	{
		return GetActiveSupport(state) switch
		{
			SectPeakSupportType.Qingyun => 0.10,
			SectPeakSupportType.Tianji => 0.05,
			SectPeakSupportType.Tianyuan => 0.18,
			SectPeakSupportType.PillarPeaks => 0.05,
			_ => 0.0
		};
	}

	public static double GetHourlyThreatDelta(GameState state)
	{
		return GetActiveSupport(state) switch
		{
			SectPeakSupportType.Tianquan => -0.70,
			SectPeakSupportType.Tianheng => -0.50,
			SectPeakSupportType.PillarPeaks => -0.10,
			_ => 0.0
		};
	}

	public static double GetToolCraftModifier(GameState state)
	{
		return GetActiveSupport(state) switch
		{
			SectPeakSupportType.Tianyan => 1.08,
			SectPeakSupportType.Tiangong => 1.18,
			SectPeakSupportType.PillarPeaks => 1.05,
			_ => 1.0
		};
	}

	public static string BuildActiveSupportStatus(GameState state)
	{
		var definition = GetActiveDefinition(state);
		return $"{definition.DisplayName}｜{definition.ShortEffect}";
	}

	public static string BuildSelectionPreview(SectPeakSupportType supportType)
	{
		var definition = GetDefinition(supportType);
		return $"{definition.DisplayName}｜{definition.ModifierSummary}";
	}

	private static string NormalizeEnumValue<TEnum>(string rawValue, TEnum fallback)
		where TEnum : struct, Enum
	{
		return Enum.TryParse<TEnum>(rawValue, out var parsed) ? parsed.ToString() : fallback.ToString();
	}
}
