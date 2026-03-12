# CountyIdle 功能总台账

> 本文是 CountyIdle 的正式功能裁定簿。  
> 它只回答三件事：
>
> 1. 某功能当前属于 `已立住 / 未立稳 / 未入场` 哪一档
> 2. 继续推进时应先看哪里
> 3. 新需求应归到哪一条，不得各起炉灶
>
> 主设计裁决以 [01_game_design_guide.md](/E:/2_Personal/Finally/docs/01_game_design_guide.md) 为准。  
> 系统法则与运行边界以 [02_system_specs.md](/E:/2_Personal/Finally/docs/02_system_specs.md) 为准。  
> 开发顺序与功能包排程以 [08_development_list.md](/E:/2_Personal/Finally/docs/08_development_list.md) 为准。

## 1. 使用裁定

### 1.1 状态图例

- `✅ 已立住`：已接入主流程，当前版本可用
- `🟡 未立稳`：已有实现或骨架，但尚未形成完整闭环
- `⭕ 未入场`：已被主设计承认，但当前版本尚未落地

### 1.2 查阅顺序

开发前一律按以下顺序使用本表：

1. 先看第 `2` 节，判目标属于哪条主线
2. 再看第 `3/4/5` 节，判它当前处于哪一档
3. 再跳到对应入口文件或 `docs/02_system_specs.md`
4. 若本表没有该项，先登记到 `docs/08_development_list.md`
5. 开发完成后，必须回写本表状态

### 1.3 术语裁定

- 对外语义统一按“浮云宗 + 天衍峰 + 青云峰总殿协同”理解。
- 表中的 `County / Town / Prefecture` 仅用于定位历史技术实现，不构成玩家可见设定。
- `Population / Jobs / Research / Hero` 等旧技术词，默认按 `门人 / 职司 / 传承研修 / 真传与核心战力` 理解。

## 2. 核心玩法总判

| 主线条目 | 当前状态 | 当前裁定 |
| --- | --- | --- |
| 门人生息与宗门人口池 | ✅ | 已有增长、伤病、住房与通勤联动，能支撑当前主循环 |
| 产业供养与库藏周转 | ✅ | 已有 `Industry + Resource + Economy` 结算闭环 |
| 宗主治理中枢 | 🟡 | 方向、法令、育才、季度法令、门规树一期已落地，仍缺执事任命与更完整自动策略 |
| 传承研修 | 🟡 | 已有线性突破，尚未进入分支化传承树 |
| 职司分化 | ✅ | 基础岗位、容量约束与回退已成立 |
| 职司转任 | ⭕ | 尚未形成正式转任规则与成本收益回路 |
| 灵根苗子与后辈培养 | 🟡 | 基础繁育已落地，深度培育与血缘规则未完成 |
| 真传与核心战力 | 🟡 | 当前仍以精英人口池抽象承担，尚未实体化 |
| 装备 / 法器体系 | 🟡 | 已有掉落与品质词条，未形成正式打造链 |
| 外务历练与护山反压 | 🟡 | 基础战斗与威胁已存在，护山闭环与高层压力未立住 |
| 双层时间与长期岁月推进 | 🟡 | 当前细时间已成立，长时间层与战略相位仍在设计中 |

## 3. 已立住功能（✅）

| 功能簇 | 当前裁定 | 主要入口 | 主要依据 |
| --- | --- | --- | --- |
| 世界观与宗门语义基线 | 已统一到“浮云宗 / 天衍峰 / 青云峰总殿协同” | `docs/09_xianxia_sect_setting.md`、`CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs` | `docs/01_game_design_guide.md` |
| 主循环时间推进 | 当前口径为 `1 秒现实时间 = 10 游戏分钟`，每 `60` 游戏分钟做一次小时结算，支持 `x1 / x2 / x4` | `CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/GameCalendarSystem.cs`、`CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md`、`docs/12_runtime_formula_appendix.md` |
| 门人生息、伤病恢复、住房与通勤 | 已接入主结算链，可持续反馈人口状态 | `CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/systems/PopulationRules.cs` | `docs/02_system_specs.md` |
| 职司容量与基础产业 | 基础岗位、建筑扩建、工具供给已成立 | `CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/systems/IndustryRules.cs` | `docs/02_system_specs.md` |
| 宗主治理中枢一期与二期 | 发展方向、法令、育才与治理折算已进入主流程 | `CountyIdle/scripts/systems/SectTaskSystem.cs`、`CountyIdle/scripts/systems/SectGovernanceSystem.cs`、`CountyIdle/scripts/ui/TaskPanel.cs` | `docs/02_system_specs.md` |
| 季度法令与门规树一期 | 已有季度法令入口与三支门规纲目 | `CountyIdle/scripts/systems/SectGovernanceSystem.cs`、`CountyIdle/scripts/systems/SectRuleTreeSystem.cs` | `docs/02_system_specs.md` |
| 供养、贡献点与库藏资源链 | 资源、薪资、惩罚、仓储与矿材链已接入运行版 | `CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/ResourceSystem.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs` | `docs/02_system_specs.md` |
| 传承研修基础突破 | 线性 `T1 / T2 / T3` 突破已可运行 | `CountyIdle/scripts/systems/ResearchSystem.cs` | `docs/02_system_specs.md` |
| 灵根苗子基础繁育 | 基础繁育与突变已落地 | `CountyIdle/scripts/systems/BreedingSystem.cs` | `docs/02_system_specs.md` |
| 外务历练基础回路 | 基础战斗、层数推进与收益损耗已存在 | `CountyIdle/scripts/systems/CombatSystem.cs` | `docs/02_system_specs.md` |
| 装备掉落与品质词条 | 掉落、品质与词条基础规则已成立 | `CountyIdle/scripts/systems/EquipmentSystem.cs` | `docs/02_system_specs.md` |
| 宗门见闻事件 | 坊市、讲法、袭扰等基础事件已接入 | `CountyIdle/scripts/systems/CountyEventSystem.cs` | `docs/02_system_specs.md` |
| 天衍峰山门图与世界图 | 山门图、世界图、外域备用视图与双地图入口已建立 | `CountyIdle/scripts/systems/SectMapViewSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/02_system_specs.md` |
| 弟子谱与宗门组织谱 | 弟子谱、峰令谱与卷册式总览已接入正式 UI | `CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md` |
| 多存档槽与 `SQLite` 存档 | 多槽、自动槽轮换、摘要预览与读写主链已成立 | `CountyIdle/scripts/core/SaveSystem.cs`、`CountyIdle/scripts/core/SqliteSaveRepository.cs`、`CountyIdle/scripts/ui/SaveSlotsPanel.cs` | `docs/13_runtime_support_appendix.md` |

## 4. 未立稳功能（🟡）

| 功能簇 | 当前裁定 | 下一步收口 | 主要入口 |
| --- | --- | --- | --- |
| 宗主治理中枢三期 | 季度法令与门规树一期已落地，仍缺执事任命 | 补执事任命与人才培养协同 | `CountyIdle/scripts/systems/SectGovernanceRules.cs`、`CountyIdle/scripts/ui/TaskPanel.cs` |
| 宗主中枢模板与自动回退 | 已有“治理力度 -> 职司”折算与容量保护 | 扩到季度模板、事件驱动自动切换与更细默认方案 | `CountyIdle/scripts/systems/SectTaskRules.cs`、`CountyIdle/scripts/systems/SectTaskSystem.cs` |
| 传承分支化 | 当前仍为线性突破 | 形成分支树、路径差异与互斥 / 协同关系 | `CountyIdle/scripts/systems/ResearchSystem.cs` |
| 门人生活循环深化 | 已有住房、伤病、通勤与基础恢复 | 补住房到岗位空间映射、衣物供给与更完整人口面板 | `CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/models/GameState.cs` |
| 资源系统分层扩展 | 前台 `T0 / T1` 与修仙材料语义已收口 | 继续稳住前台层级，并补完整运行烟测 | `CountyIdle/scripts/systems/MaterialSemanticRules.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`docs/12_runtime_formula_appendix.md` |
| 静态数据配置化 | 战略地图与部分岗位配置已接入 | 继续将关键公式与常量从硬编码迁到 `data/*.json` | `CountyIdle/data/*.json`、`CountyIdle/scripts/systems/StrategicMapConfigSystem.cs` |
| 战略地图配置驱动 | 已有配置 fallback，默认主视图转为程序化生成 | 补“双轨切换 + 调试入口” | `CountyIdle/scripts/models/StrategicMapConfig.cs`、`CountyIdle/scripts/systems/XianxiaWorldGeneratorSystem.cs` |
| 外域历练地图页 | 页签与占位面板已存在 | 补路线风险、节点事件与遭遇结算 | `CountyIdle/scenes/ui/WorldPanel.tscn` |
| 宗门见闻面板 / 宗务报表 | 面板容器已在，数据仍偏静态 | 改为实时字段与历史统计 | `CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scripts/Main.cs` |
| 真传 / 英雄实体化 | 当前仍由精英人口池抽象承担 | 形成可成长个体、职业成长与装备联动 | `CountyIdle/scripts/systems/CombatSystem.cs` |
| 天衍峰院域坊局与全格检视 | 已有全格点击检视，且可在当前 hex 间切换主修 / 协同 / 稳态坊局 | 补院域事件触发与小时结算联动 | `CountyIdle/scripts/models/TownCellCompoundData.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.CompoundPlanning.cs`、`CountyIdle/scripts/ui/MainSectTileInspector.cs` |
| 世界格二级地图分层与入口 | 已支持世界地图任意 hex 点选后在左侧显示详情，并通过进入按钮生成与山门沙盘同形的下一层 hex 沙盘 | 继续细化 `宗门 / 野外 / 坊市 / 遗迹` 四类专属模板与真实玩法 | `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.WorldCellSelection.cs`、`CountyIdle/scripts/systems/WorldSiteLocalMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/ui/MainWorldSitePanel.cs` |
| 地图素材分层资产流水线 | 文档与 `Layer 1 / 2` 接入已开始 | 补正式量产素材与 `Layer 3 / 4` 运行时接入 | `docs/11_map_asset_production_spec.md`、`CountyIdle/assets/map/manifests/l1_terrain_manifest.json` |
| 弟子可视移动 | 代码链仍在，当前运行版停用 | 若要恢复，需按 `01 -> 02 -> 实现` 重新立项 | `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs` |

## 5. 未入场主项（⭕）

| 主项 | 优先级 | 完成定义 | 设计依据 |
| --- | --- | --- | --- |
| 职司转任系统 | P1 | 门人可按条件跨职司转任，成本、收益与回退可观测 | `docs/01_game_design_guide.md` |
| 护山战与守山压力闭环 | P1 | 高威胁触发护山战，结果反哺人口、资源与防线 | `docs/01_game_design_guide.md` |
| 灵根苗子深度培育 | P1 | 后辈生成受父母属性与血缘规则影响，日志可解释 | `docs/01_game_design_guide.md` |
| 装备 / 法器打造系统 | P1 | 形成“材料 -> 打造 -> 品质结果”正式链路，并联动历练与供养 | `docs/01_game_design_guide.md` |
| Boss / 妖王机制与词条克制 | P1 | 高层历练出现首领机制，克制关系影响胜负与掉落 | `docs/01_game_design_guide.md` |
| 三相治宗循环 | P1 | 季度战略相位接入主循环，并与双层时间共同塑造长期节奏 | `docs/08_development_list.md` |
| 院域坊局正式系统 | P1 | 山门地块可承载多子建筑、共享灵气与局部协同 / 分薄 | `docs/01_game_design_guide.md` |
| 公式全面配置化 | P2 | 关键系统公式转为配置驱动，并支持版本对比与热调整 | `docs/03_change_management.md` |

## 6. 维护门规

- 本表只保留当前有效的状态裁定，不保留施工流水。
- 若某项状态变化，必须同步回写 `02 / 08` 与对应功能卡。
- 若一项内容只是历史参考、视觉迭代记录或迁移痕迹，不得写入本表正文。
