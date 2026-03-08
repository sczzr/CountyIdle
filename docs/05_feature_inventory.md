# CountyIdle 功能实现状态总表（开发看板）

> 目标：让开发时能快速判断“哪些功能已实现、哪些部分实现、哪些是 TODO”。

## 1) 状态图例

- `✅ 已实现`：代码已接入主流程，可在当前版本直接使用
- `🟡 部分实现`：有代码或 UI 骨架，但尚未形成完整闭环
- `⭕ 未实现（TODO）`：已在设计/里程碑中定义，但当前版本未落地

## 1.1 核心玩法对齐矩阵（目标 vs 当前）

| 核心玩法目标 | 当前状态 | 说明 |
| --- | --- | --- |
| 人口繁衍 | ✅ | 已有人口增长、患病恢复与住房/通勤联动 |
| 产业涌现（资源生产消耗） | ✅ | 已有 `Industry + Resource + Economy` 结算 |
| 科技涌现（科技树） | 🟡 | 已有线性 T1/T2/T3 突破，尚非分支科技树 |
| 职业分化 | ✅ | 已有四类岗位与容量约束 |
| 职业转职 | ⭕ | 尚无完整转职规则与流程 |
| 优秀后代概率受父母影响 | ⭕ | 当前繁育未使用父母质量关联 |
| 英雄单位冒险 | 🟡 | 当前以精英人口池参与探险，尚无英雄实体系统 |
| 装备打造系统 | ⭕ | 当前仅有探险掉落与工具制作，未有装备打造链路 |
| 人民与玩家共同建设郡县 | 🟡 | 已有建筑扩建、治理反馈与县城居民可视移动，一般居民协作逻辑仍需深化 |
| 郡县防护人民并带来富足 | 🟡 | 有威胁/袭扰/经济反馈，但守城闭环尚未完成 |

## 2) 已实现功能（✅）

| 功能 | 状态 | 代码入口 | 对应文档 |
| --- | --- | --- | --- |
| 主循环时间推进（1s=1分钟，60分钟结算，顶栏为架空农历 + 季度/天双进度条 + 24节气，支持 `x1/x2/x4`） | ✅ | `CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/GameCalendarSystem.cs`、`CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md` |
| 人口增长/患病恢复/通勤联动 | ✅ | `CountyIdle/scripts/systems/PopulationSystem.cs`、`PopulationRules.cs` | `docs/02_system_specs.md` |
| 岗位容量、建筑扩建、制工具 | ✅ | `CountyIdle/scripts/systems/IndustrySystem.cs`、`IndustryRules.cs` | `docs/02_system_specs.md` |
| 岗位面板进程化交互（`JobsPadding` 具体岗位 + 建筑/科技联动） | ✅ | `CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/JobRoleSystem.cs`、`CountyIdle/scripts/ui/JobsPanel.cs`、`CountyIdle/scripts/ui/JobRowView.cs`、`CountyIdle/data/jobs.json` | `docs/02_system_specs.md` |
| 经济结算（资源、薪资、惩罚） | ✅ | `CountyIdle/scripts/systems/EconomySystem.cs` | `docs/02_system_specs.md` |
| 仓储与矿产资源链 | ✅ | `CountyIdle/scripts/systems/ResourceSystem.cs`、`IndustrySystem.cs` | `docs/02_system_specs.md` |
| 科研突破 T1/T2/T3 | ✅ | `CountyIdle/scripts/systems/ResearchSystem.cs` | `docs/02_system_specs.md` |
| 精英繁育与突变 | ✅ | `CountyIdle/scripts/systems/BreedingSystem.cs` | `docs/02_system_specs.md` |
| 探险战斗与层数推进 | ✅ | `CountyIdle/scripts/systems/CombatSystem.cs` | `docs/02_system_specs.md` |
| 装备品质/词条掉落 | ✅ | `CountyIdle/scripts/systems/EquipmentSystem.cs` | `docs/02_system_specs.md` |
| 郡县动态事件（商路/学宫/袭扰） | ✅ | `CountyIdle/scripts/systems/CountyEventSystem.cs` | `docs/02_system_specs.md` |
| 宗门 hex 驻地地图生成 + 缩放（纯绘制 + 道路/水域语义） | ✅ | `CountyIdle/scripts/systems/TownMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/systems/SectMapViewSystem.cs` | `docs/02_system_specs.md` |
| 宗门弟子可视移动（正式 sprite + 作息驱动 + 实体场所 + 场所选中提示） | ✅ | `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyTownMapViewSystem.Anchors.cs`、`CountyTownMapViewSystem.Residents.cs`、`TownMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`CountyIdle/assets/characters/residents/*.png` | `docs/02_system_specs.md` |
| 世界/郡图 hex 战略视图 + 缩放 | ✅ | `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`Main.cs` | `docs/02_system_specs.md` |
| 修仙 Hex 世界生成系统（地貌 / 河流 / 道路 / 灵脉 / 奇观 / 宗门候选 / edge overlay） | ✅ | `CountyIdle/scripts/models/XianxiaWorldGenerationConfig.cs`、`CountyIdle/scripts/models/XianxiaWorldMapData.cs`、`CountyIdle/scripts/systems/XianxiaWorldGenerationConfigSystem.cs`、`CountyIdle/scripts/systems/XianxiaWorldGeneratorSystem.cs`、`CountyIdle/data/xianxia_world_generation.json`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs` | `docs/02_system_specs.md` |
| 双地图布局（宗门地图 + 世界地图） | ✅ | `CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scripts/systems/SectMapViewSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs` | `docs/02_system_specs.md` |
| 宗门地图语义改造（V1） | ✅ | `CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`CountyIdle/scripts/systems/SectMapViewSystem.cs`、`CountyIdle/scripts/systems/TownMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/02_system_specs.md` |
| 宗门经营全局语义统一（V2） | ✅ | `CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`IndustrySystem.cs`、`JobProgressionRules.cs`、`ResearchSystem.cs`、`ResourceSystem.cs`、`MapOperationalLinkSystem.cs`、`CountyEventSystem.cs`、`StrategicMapConfigSystem.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WarehousePanel.tscn`、`JobsPanel.tscn`、`EventLogPanel.tscn`、`WorldPanel.tscn` | `docs/02_system_specs.md` |
| 外域态势备用视图程序化生成（Village 风格 + hex 战略底格） | ✅ | `CountyIdle/scripts/systems/PrefectureMapGeneratorSystem.cs`、`StrategicMapViewSystem.cs`、`PrefectureCityThemeConfigSystem.cs` | `docs/02_system_specs.md`（外域州府式大图、更高密坊市节点、惯性缩放、问道长街/坊巷纹理、高倍街屋/院落细化、修仙命名主题配置、hex 战略底格） |
| 外域备用视图修仙语义化（V1） | ✅ | `CountyIdle/data/prefecture_city_theme.json`、`CountyIdle/data/strategic_maps.json`、`CountyIdle/scripts/models/PrefectureCityThemeConfig.cs`、`CountyIdle/scripts/systems/PrefectureCityThemeConfigSystem.cs`、`CountyIdle/scripts/systems/PrefectureMapGeneratorSystem.cs` | `docs/02_system_specs.md` |
| 外域备用视图运营语义统一（V2） | ✅ | `CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`MapOperationalLinkSystem.cs`、`StrategicMapViewSystem.cs`、`ResourceSystem.cs` | `docs/02_system_specs.md` |
| 地图页签切换与缩放按钮 | ✅ | `CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/02_system_specs.md`（当前主界面仅保留“宗门地图 / 世界地图”双地图入口） |
| 地图与经营状态联动（状态条 + 调度按钮） | ✅ | `CountyIdle/scripts/systems/MapOperationalLinkSystem.cs`、`CountyIdle/scripts/ui/MainMapOperationalLink.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs` | `docs/02_system_specs.md` |
| 仓库管理独立入口（Esc 关闭 + 提示条） | ✅ | `CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/MainWarehousePanel.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/ui/PopupPanelBase.cs` | `docs/02_system_specs.md` |
| 存档/读档（SQLite 主存档 + 兼容旧 JSON 迁移） | ✅ | `CountyIdle/scripts/core/SaveSystem.cs`、`CountyIdle/scripts/core/SqliteSaveRepository.cs`、`CountyIdle/scripts/core/SqliteMigrationRunner.cs`、`CountyIdle/scripts/models/SaveSlotSummary.cs`、`CountyIdle/scripts/models/SaveSnapshotRecord.cs` | `docs/02_system_specs.md` |
| 客户端设置（语言/分辨率/字体/音量/快捷键） | ✅ | `CountyIdle/scripts/core/ClientSettingsSystem.cs`、`CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/MainShortcutBindings.cs`、`CountyIdle/scripts/ui/PopupPanelBase.cs` | `docs/02_system_specs.md` |
| 双布局（Legacy/Figma） | ✅ | `CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md` |

## 3) 部分实现（🟡）

| 功能 | 当前状态 | 差距（下一步） | 关联文件 |
| --- | --- | --- | --- |
| 科技树系统 | 当前为线性突破（T1/T2/T3） | 扩展为分支科技树（解锁条件、路径选择、互斥/协同） | `CountyIdle/scripts/systems/ResearchSystem.cs` |
| 战略地图数据驱动配置 | 世界图保留配置 fallback，默认主视图已切至修仙世界程序化生成；外域备用视图已切为程序化生成 | 补充“配置驱动/程序化”双轨切换开关与调试入口 | `CountyIdle/scripts/models/StrategicMapConfig.cs`、`CountyIdle/scripts/systems/StrategicMapConfigSystem.cs`、`CountyIdle/scripts/systems/XianxiaWorldGeneratorSystem.cs` |
| 野外探险地图页 | 页签与占位面板已存在 | 接入路线风险、节点事件、战斗遭遇结算 | `CountyIdle/scenes/ui/WorldPanel.tscn` |
| 事件面板/统计报表 | 面板容器与部分按钮接入已完成 | 将静态文案改为实时数据字段与历史统计 | `CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scripts/Main.cs` |
| 岗位优先级 | 已接入“具体岗位优先保留”与容量/人口回退保护 | 继续扩展为缺人时的全局自动补员与回退策略 | `CountyIdle/scripts/systems/JobRoleSystem.cs`、`CountyIdle/scripts/ui/JobsPanel.cs` |
| 人口分配与生活循环（住房/通勤/生老病死） | 已接入人口分层、患病恢复、衣物损耗、动态步行通勤到岗率（一期） | 接入住房到岗位空间映射输入、衣物产线供给与更完整人口面板 | `CountyIdle/scripts/models/GameState.cs`、`CountyIdle/scripts/systems/PopulationRules.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/systems/EconomySystem.cs` |
| 静态数据配置（buildings/jobs/items/monsters/traits） | 战略地图与岗位配置已接入，其余数据仍多为硬编码 | 继续建立统一配置加载层并替换经济/战斗等公式常量 | `CountyIdle/data/*.json`、`CountyIdle/scripts/systems/StrategicMapConfigSystem.cs`、`CountyIdle/scripts/systems/JobRoleSystem.cs` |
| 资源系统分层扩展（自然原料→加工链） | 五期已接入“可见库存整数化 + 隐藏进度池”与郡图自然原料来源节点/标签，物品最小单位为 `1`；`T0` 四条显式链路与 `木/石` 脱离经济直产继续保留 | 补 `Godot` 运行烟测与存档/读档回归；继续保持 `T2/T3` 不提前进入前台展示，禁止将 `青铜 / 纸张 / 玻璃 / 皮革 / 火药` 误放入原料层 | `docs/02_system_specs.md`、`CountyIdle/scripts/models/GameState.cs`、`CountyIdle/scripts/systems/InventoryRules.cs`、`CountyIdle/scripts/systems/MaterialRules.cs`、`CountyIdle/scripts/systems/ResourceSystem.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/systems/PrefectureMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs` |
| 多存档槽管理界面 | 已接入手动槽 + 3 槽轮换自动槽列表、覆盖、读取、新建、复制、重命名、删除，并显示强化后的存档摘要预览、筛选排序与截图预览 | 继续补外部导入导出等更完整的管理入口 | `CountyIdle/scripts/core/SaveSystem.cs`、`CountyIdle/scripts/core/SqliteSaveRepository.cs`、`CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/ui/MainSaveSlotsPanel.cs`、`CountyIdle/scripts/ui/MainSavePreview.cs` |
| 英雄单位系统 | 探险已存在但为人口池抽象 | 增加英雄实体、职业成长、受装备与科技影响的个体冒险 | `CountyIdle/scripts/systems/CombatSystem.cs` |

## 4) 未实现（TODO）

| TODO | 优先级 | 目标验收（完成定义） | 设计来源 |
| --- | --- | --- | --- |
| Boss 机制与词条克制（探险后期） | P1 | 进入高层探险时出现 Boss 回合逻辑，掉落与克制关系可观测 | `docs/01_game_design_guide.md` |
| 攻城战/守城压力闭环 | P1 | 威胁高位触发攻城，结果反哺城防与人口/资源状态 | `docs/01_game_design_guide.md` |
| 精英繁育深度化（父母属性/近亲惩罚） | P1 | 后代生成受父母与血缘规则影响，日志可解释 | `docs/01_game_design_guide.md` |
| 职业转职系统 | P1 | 人口在条件满足时可跨职业转职，成本与收益可观测 | `docs/01_game_design_guide.md` |
| 装备打造系统 | P1 | 增加材料->打造->品质结果链路，与探险与产业互相影响 | `docs/01_game_design_guide.md` |
| 科技树分支化 | P1 | 形成分支科技路径，不同选择带来不同发展方向 | `docs/01_game_design_guide.md` |
| 公式全面配置化（去硬编码） | P2 | 关键系统公式由配置文件驱动，支持热调整/版本对比 | `docs/03_change_management.md` |

## 5) 使用方式（开发前 2 分钟）

1. 先看本文件第 2/3/4 节，确认目标属于 `✅/🟡/⭕` 哪类。
2. 再跳到 `docs/02_system_specs.md` 查对应系统规则。
3. 若本文件没有该需求，先登记到 `docs/08_development_list.md` 再开发。
4. 开始开发前补功能卡；若是机制/平衡改动，补提案与日志。
5. 每次开发后更新本文件状态，确保“实现状态与代码一致”。
