using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class SectOrganizationRules
{
	private sealed record PeakEntry(
		string PeakName,
		string Positioning,
		string CoreUnits,
		string Responsibility,
		string DepartmentDetails,
		bool IsCurrentPlayableFocus = false);

	public readonly record struct PeakProfile(
		string Name,
		string Positioning,
		string CoreUnits,
		string Responsibility,
		string DepartmentDetails,
		bool IsCurrentPlayableFocus);

	private static readonly IReadOnlyList<PeakEntry> PeakEntries =
	[
		new("青云峰", "宗门中枢", "内务总殿 / 外事总殿 / 传功总殿", "总账、外务协调、核心传承审核",
			"内务总殿：统摄九峰庶务殿，负责全宗户籍、贡献点总账、律法修订与资源调拨。\n外事总殿：统筹附庸、商路、外交与青云总坊，对九峰外务统一报备协调。\n传功总殿：掌《青云诀》、核心传承审核与镇派功法保管。"),
		new("天衍峰", "当前可玩主视角", "总枢殿 / 阵研阁 / 理法堂 / 图录堂 / 推演堂", "护山大阵、傀儡研发、天机推演",
			"总枢殿：玄机真人主持的行政、军务与研发总令中枢。\n阵研阁：吴长老坐镇的技术心脏，汇聚顶级阵法天才。\n理法堂：最前沿理论研究与阵法设计。\n图录堂：阵图、傀儡与战争机关设计图纸。\n推演堂：操盘周天星斗大阵缩影，负责沙盘推演与性能模拟。", true),
		new("天枢峰", "门户与行政", "总务殿 / 鸿胪司 / 讲法院 / 阵市 / 玄仓坪", "户籍、任务结算、接待、坊市与物流",
			"总务殿：弟子处 / 功勋处 / 任务处，负责户籍、贡献结算与任务后台。\n鸿胪司：迎宾处 / 通商处，负责外客接待、对外采买与技术贸易。\n讲法院：教务处 / 戒律处 / 执法阁，负责基础课程、门规教育与日常巡查。\n阵市 / 浮空渡口 / 矩阵穿梭舟总站：承担坊市、交通与峰际流通。\n玄仓坪 / 资源总库 / 灵石钱庄：承担中央仓储物流与灵石兑换。"),
		new("天机峰", "学术与传承", "衍法阁 / 传功总院 / 试炼幻境", "阵法教育、典籍传承、模拟演练",
			"衍法阁：阵基系 / 傀儡系 / 应用系，承担阵法、机关与应用课程。\n藏书阁：管理专业典籍、图纸与传承档案。\n传功总院：承接基础分院、百艺分院与高阶分院的进阶授业。\n试炼幻境：安全模拟阵法攻防、课题演练与实战考核。"),
		new("天工峰", "工业与制造", "铸机阁 / 阵基殿 / 傀儡工坊 / 资源调配司", "矿材冶铸、阵盘骨架、工坊流水线",
			"铸机阁：资源司 / 锻造坊 / 精工坊 / 品检司，负责材料入库、重型锻造、精密装配与质检。\n阵基殿：标准化阵盘、阵旗与大型阵基骨架生产。\n傀儡工坊：傀儡总装、调试与批量迭代改良。\n资源调配司：把资源总库与各生产线连成闭环。"),
		new("天权峰", "军事与执法", "承山堂 / 破阵营 / 天谕殿 / 执法总堂", "护山战备、剑阵突击、法术火力、门规裁决",
			"承山堂：演武场 / 药浴房，负责炼体士训练与高负荷阵眼支撑。\n破阵营：营部 / 战术阁，负责剑阵合击、护阵突击与战术推演。\n天谕殿：制符坊 / 试法场，负责术法火力与制式阵符。\n执法总堂：承担重大违纪、战时军令与护山裁决。"),
		new("天元峰", "生活与后勤", "济世堂 / 百兽苑 / 清音阁", "丹药后勤、阵兽培育、音律安神",
			"济世堂：丹心庐 / 百草园，负责问诊、疗养与辅助丹药灵草培育。\n百兽苑：育兽房 / 机巧房，负责灵兽孵化、血脉提纯与生物傀儡改造。\n清音阁：天籁居 / 鸣音坊，负责安神乐曲、音波校准与音律法器维护。"),
		new("天衡峰", "机密与禁地", "暗部 / 风纪堂 / 影卫堂", "技术保密、内部肃查、外部秘行",
			"暗部：统筹技术保密、反间与高危秘务。\n风纪堂：对内肃查技术窃密、弟子叛变与重大违纪。\n影卫堂：对外刺探、破坏、反间与护送机密行动。\n天机阁：整理各路情报并回供阵研阁战略推演。"),
		new("其余支柱峰", "宗门全局支援", "镇岳峰炼体堂 / 丹鼎峰丹堂 / 神工峰器堂 / 御灵峰灵兽堂 / 藏剑峰剑堂 / 万法峰法堂 / 无影峰影堂 / 烟霞峰天音堂", "在全宗范围提供炼体、丹药、军械、灵兽、剑修、术法、情报与神魂支援",
			"镇岳峰炼体堂：主力前排与重装守线。\n丹鼎峰丹堂：丹药、医疗与灵药园。\n神工峰器堂：法宝、战甲与大型战争机关。\n御灵峰灵兽堂：灵兽军团、空骑与驮阵灵兽。\n藏剑峰 / 万法峰 / 无影峰 / 烟霞峰：分别承担斩首攻坚、远程法轰、情报秘行与神魂支援。")
	];

	public static string BuildPeakOverviewText()
	{
		var lines = new List<string>(PeakEntries.Count);
		foreach (var entry in PeakEntries)
		{
			var focusTag = entry.IsCurrentPlayableFocus ? "【当前】" : string.Empty;
			lines.Add($"{focusTag}{entry.PeakName}：{entry.CoreUnits}｜{entry.Responsibility}");
		}

		return string.Join("\n", lines);
	}

	public static int GetPeakCount()
	{
		return PeakEntries.Count;
	}

	public static int NormalizePeakIndex(int peakIndex)
	{
		return Math.Clamp(peakIndex, 0, PeakEntries.Count - 1);
	}

	public static int GetDefaultPeakIndex()
	{
		for (var index = 0; index < PeakEntries.Count; index++)
		{
			if (PeakEntries[index].IsCurrentPlayableFocus)
			{
				return index;
			}
		}

		return 0;
	}

	public static int GetRecommendedPeakIndex(JobType jobType)
	{
		return jobType switch
		{
			JobType.Farmer => 6,
			JobType.Worker => 4,
			JobType.Merchant => 2,
			JobType.Scholar => 3,
			_ => GetDefaultPeakIndex()
		};
	}

	public static SectPeakSupportType GetSupportTypeForPeakIndex(int peakIndex)
	{
		return NormalizePeakIndex(peakIndex) switch
		{
			0 => SectPeakSupportType.Qingyun,
			1 => SectPeakSupportType.Tianyan,
			2 => SectPeakSupportType.Tianshu,
			3 => SectPeakSupportType.Tianji,
			4 => SectPeakSupportType.Tiangong,
			5 => SectPeakSupportType.Tianquan,
			6 => SectPeakSupportType.Tianyuan,
			7 => SectPeakSupportType.Tianheng,
			_ => SectPeakSupportType.PillarPeaks
		};
	}

	public static string GetPeakTitle(int peakIndex)
	{
		var entry = PeakEntries[NormalizePeakIndex(peakIndex)];
		return entry.PeakName;
	}

	public static PeakProfile GetPeakProfile(int peakIndex)
	{
		var entry = PeakEntries[NormalizePeakIndex(peakIndex)];
		return new PeakProfile(
			entry.PeakName,
			entry.Positioning,
			entry.CoreUnits,
			entry.Responsibility,
			entry.DepartmentDetails,
			entry.IsCurrentPlayableFocus);
	}

	public static string GetPeakSummary(int peakIndex)
	{
		var entry = PeakEntries[NormalizePeakIndex(peakIndex)];
		var focusTag = entry.IsCurrentPlayableFocus ? "【当前经营焦点】" : string.Empty;
		return $"{focusTag}{entry.Positioning}｜{entry.Responsibility}";
	}

	public static string BuildPeakDetailText(int peakIndex)
	{
		var entry = PeakEntries[NormalizePeakIndex(peakIndex)];
		var supportDefinition = SectPeakSupportRules.GetDefinition(GetSupportTypeForPeakIndex(peakIndex));
		var focusText = entry.IsCurrentPlayableFocus ? "经营焦点：当前玩家主视角峰\n" : string.Empty;
		return
			$"定位：{entry.Positioning}\n" +
			focusText +
			$"核心机构：{entry.CoreUnits}\n" +
			$"职责：{entry.Responsibility}\n" +
			$"附属部门与处室：\n{entry.DepartmentDetails}\n" +
			$"协同法旨：{supportDefinition.ModifierSummary}";
	}

	public static string GetLinkedPeakSummary(JobType jobType)
	{
		return jobType switch
		{
			JobType.Farmer => "天衍峰阵堂 / 天元峰济世堂 / 天工峰资源调配司",
			JobType.Worker => "天衍峰总枢殿 / 天工峰铸机阁 / 天权峰承山堂",
			JobType.Merchant => "青云峰外事总殿 / 天枢峰鸿胪司 / 阵市 / 玄仓坪",
			JobType.Scholar => "青云峰传功总殿 / 天机峰衍法阁 / 讲法院 / 传功总院",
			_ => "浮云宗协同体系"
		};
	}

	public static string GetLinkedDepartmentDetail(JobType jobType)
	{
		return jobType switch
		{
			JobType.Farmer => "关联部门：阵研阁负责阵材需求，济世堂负责药养补给，资源调配司负责峰内供料统筹。",
			JobType.Worker => "关联部门：总枢殿发出峰务令，铸机阁与阵基殿承担制造，承山堂与破阵营承担护山与重装配合。",
			JobType.Merchant => "关联部门：外事总殿统筹对外，鸿胪司负责迎来送往，阵市与玄仓坪承担商贸与物流。",
			JobType.Scholar => "关联部门：传功总殿定传承规制，衍法阁承担阵道深造，讲法院与传功总院负责基础与进阶授业。",
			_ => "关联部门：由青云峰总殿与天衍峰各机构协同落实。"
		};
	}
}
