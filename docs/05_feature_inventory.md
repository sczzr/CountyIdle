# CountyIdle 功能实现状态总表（开发看板）

> 目标：让开发时能快速判断“哪些功能已实现、哪些部分实现、哪些是 TODO”。

## 1) 状态图例

- `✅ 已实现`：代码已接入主流程，可在当前版本直接使用
- `🟡 部分实现`：有代码或 UI 骨架，但尚未形成完整闭环
- `⭕ 未实现（TODO）`：已在设计/里程碑中定义，但当前版本未落地

> 世界观基线：对外设计语义统一按“浮云宗（青云州江陵府）+ 天衍峰经营 + 外域附庸圈层 + 历练探索”理解；表格中的 `County* / Town* / Prefecture*` 代码入口仅用于定位历史技术实现。
>
> 术语补充：本文中的“人口 / 职业 / 科技 / 英雄”等技术词，默认按 `docs/09_xianxia_sect_setting.md` 映射为“门人 / 职司 / 传承研修 / 真传与核心战力”理解。

## 1.1 核心玩法对齐矩阵（目标 vs 当前）

| 核心玩法目标 | 当前状态 | 说明 |
| --- | --- | --- |
| 门人生息与传承 | ✅ | 已有人口增长、患病恢复与住房 / 通勤联动 |
| 产业供养（资源生产消耗） | ✅ | 已有 `Industry + Resource + Economy` 结算 |
| 宗主治理（发展方向 / 法令 / 任务 / 育才） | 🟡 | 当前已完成“发展方向 / 法令 / 任务 / 育才 + 季度法令 + 门规树一期”；后续补执事任命 |
| 传承研修（科技 / 传承树） | 🟡 | 已有线性 T1/T2/T3 突破，尚非分支科技树 |
| 职司分化 | ✅ | 已有四类岗位与容量约束 |
| 职司转任 | ⭕ | 尚无完整转任规则与流程 |
| 灵根苗子资质受父母影响 | ⭕ | 当前繁育未使用父母质量关联 |
| 真传 / 英雄单位历练 | 🟡 | 当前以精英人口池参与探险，尚无英雄实体系统 |
| 装备 / 法器打造系统 | ⭕ | 当前仅有探险掉落与工具制作，未有装备打造链路 |
| 弟子与玩家共同建设宗门 | 🟡 | 已有建筑扩建、治理反馈，一般弟子协作逻辑仍需深化 |
| 宗门护持弟子与外域附庸据点并带来富足 | 🟡 | 有威胁 / 袭扰 / 经济反馈，但护山闭环尚未完成 |

## 2) 已实现功能（✅）

| 功能 | 状态 | 代码入口 | 对应文档 |
| --- | --- | --- | --- |
| 浮云宗·天衍峰设定锚定 | ✅ | `docs/09_xianxia_sect_setting.md`、`docs/01_game_design_guide.md`、`docs/02_system_specs.md`、`CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs` | `docs/09_xianxia_sect_setting.md` |
| 主循环时间推进（1s=1分钟，60分钟结算，顶栏为架空农历 + 季度/天双进度条 + 24节气，支持 `x1/x2/x4`） | ✅ | `CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/GameCalendarSystem.cs`、`CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md` |
| 门人生息 / 伤病恢复 / 通勤联动 | ✅ | `CountyIdle/scripts/systems/PopulationSystem.cs`、`PopulationRules.cs` | `docs/02_system_specs.md` |
| 职司容量、宗门建筑扩建、制工具 | ✅ | `CountyIdle/scripts/systems/IndustrySystem.cs`、`IndustryRules.cs` | `docs/02_system_specs.md` |
| 宗主治理中枢（一期：方略层去人头化 + 双轨内务） | ✅ | `CountyIdle/scripts/models/SectTaskType.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs`、`CountyIdle/scripts/systems/SectTaskSystem.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/systems/MapOperationalLinkSystem.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/MainTaskPanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/TaskPanel.tscn`、`CountyIdle/scenes/ui/JobsPanel.tscn`、`CountyIdle/scenes/ui/WorldPanel.tscn`（当前中枢弹窗已统一为“治宗册”书卷子面板） | `docs/02_system_specs.md` |
| 宗主治理中枢（二期：发展方向 / 法令 / 育才） | ✅ | `CountyIdle/scripts/models/SectGovernanceTypes.cs`、`CountyIdle/scripts/systems/SectGovernanceRules.cs`、`CountyIdle/scripts/systems/SectGovernanceSystem.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/MainTaskPanel.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md` |
| 供养结算（资源、薪资、惩罚） | ✅ | `CountyIdle/scripts/systems/EconomySystem.cs` | `docs/02_system_specs.md` |
| 库藏与矿材资源链 | ✅ | `CountyIdle/scripts/systems/ResourceSystem.cs`、`IndustrySystem.cs` | `docs/02_system_specs.md` |
| 传承研修突破 T1/T2/T3 | ✅ | `CountyIdle/scripts/systems/ResearchSystem.cs` | `docs/02_system_specs.md` |
| 灵根苗子繁育与突变 | ✅ | `CountyIdle/scripts/systems/BreedingSystem.cs` | `docs/02_system_specs.md` |
| 外务历练战斗与层数推进 | ✅ | `CountyIdle/scripts/systems/CombatSystem.cs` | `docs/02_system_specs.md` |
| 装备 / 法器品质与词条掉落 | ✅ | `CountyIdle/scripts/systems/EquipmentSystem.cs` | `docs/02_system_specs.md` |
| 宗门动态事件（坊市/讲法/袭扰） | ✅ | `CountyIdle/scripts/systems/CountyEventSystem.cs` | `docs/02_system_specs.md` |
| 宗门 hex 驻地地图生成 + 缩放（纯绘制 + 道路/水域语义） | ✅ | `CountyIdle/scripts/systems/TownMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/systems/SectMapViewSystem.cs` | `docs/02_system_specs.md` |
| 宗门弟子可视移动（当前停用） | ⭕ | `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyTownMapViewSystem.Anchors.cs`、`CountyTownMapViewSystem.Residents.cs`、`TownMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`CountyIdle/assets/characters/residents/*.png` | `docs/02_system_specs.md` |
| 宗门弟子独立属性界面（弟子谱：固定宗门树 + 卷轴档案 UI） | ✅ | `CountyIdle/scripts/models/DiscipleProfile.cs`、`CountyIdle/scripts/systems/DiscipleRosterSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.Residents.cs`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/MainDisciplePanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/DisciplePanel.tscn`、`CountyIdle/scenes/ui/WorldPanel.tscn`（当前弹窗已继续补齐统一卷轴子面板外壳：左右木轴、上下绫边、卷首横题与“峰内名录 / 宣纸档案”分栏；左侧名册已继续收口到固定宗门组织树） | `docs/02_system_specs.md` |
| 宗门组织谱系展示（卷册总览） | ✅ | `CountyIdle/scripts/systems/SectOrganizationRules.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/ui/MainSectOrganizationPanel.cs`、`CountyIdle/scenes/ui/SectOrganizationPanel.tscn`、`CountyIdle/scenes/ui/BottomBar.tscn`（当前卷册已进一步收口为“峰令谱”书卷子面板：左右木轴、上下绫边、卷首横题与墨线批令） | `docs/02_system_specs.md` |
| 宗门组织谱系详情浏览（卷册详批） | ✅ | `CountyIdle/scripts/systems/SectOrganizationRules.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/ui/MainSectOrganizationPanel.cs`、`CountyIdle/scenes/ui/SectOrganizationPanel.tscn`、`CountyIdle/scenes/ui/BottomBar.tscn`（峰令谱卷册改为三栏式全景布局：左侧九峰名录导航、中栏峰脉纪要+部门卡片、右侧职司导览+法旨批令；正文移除深色内嵌滚动框，统一宣纸直写） | `docs/02_system_specs.md` |
| 峰脉协同法旨（卷册治理入口） | ✅ | `CountyIdle/scripts/models/SectPeakSupportType.cs`、`CountyIdle/scripts/systems/SectPeakSupportRules.cs`、`CountyIdle/scripts/systems/SectPeakSupportSystem.cs`、`CountyIdle/scripts/systems/SectOrganizationRules.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/ui/MainSectOrganizationPanel.cs`、`CountyIdle/scenes/ui/SectOrganizationPanel.tscn`、`CountyIdle/scenes/ui/BottomBar.tscn`（动作区收口至右栏批令区，主按钮强调印章刻印感，协同峰逻辑保持不变） | `docs/02_system_specs.md` |
| 季度法令（宗主治理三期先行） | ✅ | `CountyIdle/scripts/models/SectQuarterDecreeType.cs`、`CountyIdle/scripts/systems/SectGovernanceRules.cs`、`CountyIdle/scripts/systems/SectGovernanceSystem.cs`、`CountyIdle/scripts/systems/GameCalendarSystem.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/MainTaskPanel.cs` | `docs/02_system_specs.md` |
| 门规树（一期：三支门规纲目） | ✅ | `CountyIdle/scripts/models/SectRuleTreeTypes.cs`、`CountyIdle/scripts/systems/SectRuleTreeRules.cs`、`CountyIdle/scripts/systems/SectRuleTreeSystem.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/MainTaskPanel.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs` | `docs/02_system_specs.md` |
| 世界/江陵府外域 hex 战略视图 + 缩放 | ✅ | `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`Main.cs` | `docs/02_system_specs.md` |
| 修仙 Hex 世界生成系统（地貌 / 河流 / 道路 / 灵脉 / 奇观 / 宗门候选 / edge overlay） | ✅ | `CountyIdle/scripts/models/XianxiaWorldGenerationConfig.cs`、`CountyIdle/scripts/models/XianxiaWorldMapData.cs`、`CountyIdle/scripts/systems/XianxiaWorldGenerationConfigSystem.cs`、`CountyIdle/scripts/systems/XianxiaWorldGeneratorSystem.cs`、`CountyIdle/data/xianxia_world_generation.json`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs` | `docs/02_system_specs.md` |
| 双地图布局（天衍峰山门图 + 世界地图） | ✅ | `CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scripts/systems/SectMapViewSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs` | `docs/02_system_specs.md` |
| 天衍峰山门图语义改造（V1） | ✅ | `CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`CountyIdle/scripts/systems/SectMapViewSystem.cs`、`CountyIdle/scripts/systems/TownMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/02_system_specs.md` |
| 宗门经营全局语义统一（V2） | ✅ | `CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`IndustrySystem.cs`、`JobProgressionRules.cs`、`ResearchSystem.cs`、`ResourceSystem.cs`、`MapOperationalLinkSystem.cs`、`CountyEventSystem.cs`、`StrategicMapConfigSystem.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WarehousePanel.tscn`、`JobsPanel.tscn`、`EventLogPanel.tscn`、`WorldPanel.tscn` | `docs/02_system_specs.md` |
| 江陵府外域备用视图程序化生成（Village 风格 + hex 战略底格） | ✅ | `CountyIdle/scripts/systems/PrefectureMapGeneratorSystem.cs`、`StrategicMapViewSystem.cs`、`PrefectureCityThemeConfigSystem.cs` | `docs/02_system_specs.md`（外域州府式大图、更高密坊市节点、惯性缩放、问道长街/坊巷纹理、高倍街屋/院落细化、修仙命名主题配置、hex 战略底格） |
| 江陵府外域备用视图修仙语义化（V1） | ✅ | `CountyIdle/data/prefecture_city_theme.json`、`CountyIdle/data/strategic_maps.json`、`CountyIdle/scripts/models/PrefectureCityThemeConfig.cs`、`CountyIdle/scripts/systems/PrefectureCityThemeConfigSystem.cs`、`CountyIdle/scripts/systems/PrefectureMapGeneratorSystem.cs` | `docs/02_system_specs.md` |
| 江陵府外域备用视图运营语义统一（V2） | ✅ | `CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`MapOperationalLinkSystem.cs`、`StrategicMapViewSystem.cs`、`ResourceSystem.cs` | `docs/02_system_specs.md` |
| 地图页签切换与缩放按钮 | ✅ | `CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/02_system_specs.md`（当前主界面仅保留“天衍峰山门图 / 世界地图”双地图入口） |
| 地图与经营状态联动（状态条 + 调度按钮） | ✅ | `CountyIdle/scripts/systems/MapOperationalLinkSystem.cs`、`CountyIdle/scripts/ui/MainMapOperationalLink.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs` | `docs/02_system_specs.md` |
| 主界面六边形沙盘布局 + 地块检视器 | ✅ | `CountyIdle/scenes/Main.tscn`、`CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scenes/ui/JobsPanel.tscn`、`CountyIdle/scenes/ui/TopBar.tscn`、`CountyIdle/scenes/ui/BottomBar.tscn`、`CountyIdle/scenes/ui/EventLogPanel.tscn`、`CountyIdle/scripts/models/TownMapSelectionSummary.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.Selection.cs`、`CountyIdle/scripts/systems/TownActivityAnchorVisualRules.cs`、`CountyIdle/scripts/ui/MainSectTileInspector.cs`、`CountyIdle/scripts/ui/MainSectChroniclePanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/assets/ui/background/background_frosted_glass.gdshader`（当前 Legacy 主界面已进一步切为“书卷/画轴”主题，tile 选中后左栏会出现具体信息与可操作项；右栏日志区本轮继续收口为“山门近闻 + 天衍峰札记”札记卷；2026-03-10 起根背景改用 `background_2.png` 作为卷轴山水底图，并在窗口缩放时以 cover 方式持续铺满整个游戏窗口；当前背景额外挂了毛玻璃 shader，通过轻微散射、乳白染色与细颗粒降低对前景 UI 的干扰；当前进入参考 HTML / 卷轴图 1:1 视觉对齐优化阶段，不改变 tile 左栏联动链路） | `docs/02_system_specs.md` |
| 仓库管理独立入口（Esc 关闭 + 提示条） | ✅ | `CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/MainWarehousePanel.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/ui/PopupPanelBase.cs`（当前仓储弹窗已升级为“库藏大卷”全幅布局：木轴宣纸主卷、库存区独占画幅、`全览/农桑/金石/百工` 页签与 4~6 列自适应卡片、刻度仓储进度条与朱红满载提示，物资条目 token 化并优先显示整数库存，零库存灰态降噪） | `docs/02_system_specs.md` |
| 存档/读档（SQLite 主存档 + 兼容旧 JSON 迁移） | ✅ | `CountyIdle/scripts/core/SaveSystem.cs`、`CountyIdle/scripts/core/SqliteSaveRepository.cs`、`CountyIdle/scripts/core/SqliteMigrationRunner.cs`、`CountyIdle/scripts/models/SaveSlotSummary.cs`、`CountyIdle/scripts/models/SaveSnapshotRecord.cs` | `docs/02_system_specs.md` |
| 客户端设置（语言/分辨率/字体/音量/快捷键） | ✅ | `CountyIdle/scripts/core/ClientSettingsSystem.cs`、`CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/MainShortcutBindings.cs`、`CountyIdle/scripts/ui/PopupPanelBase.cs`（当前设置面板已统一为“机宜卷”书卷子面板，沿用宣纸、木轴、墨线控件与“符令”语义） | `docs/02_system_specs.md` |
| 双布局（Legacy/Figma） | ✅ | `CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md` |

## 3) 部分实现（🟡）

| 功能 | 当前状态 | 差距（下一步） | 关联文件 |
| --- | --- | --- | --- |
| 传承 / 科技树系统 | 当前为线性突破（T1/T2/T3） | 扩展为分支科技树（解锁条件、路径选择、互斥/协同） | `CountyIdle/scripts/systems/ResearchSystem.cs` |
| 战略地图数据驱动配置 | 世界图保留配置 fallback，默认主视图已切至修仙世界程序化生成；江陵府外域备用视图已切为程序化生成 | 补充“配置驱动/程序化”双轨切换开关与调试入口 | `CountyIdle/scripts/models/StrategicMapConfig.cs`、`CountyIdle/scripts/systems/StrategicMapConfigSystem.cs`、`CountyIdle/scripts/systems/XianxiaWorldGeneratorSystem.cs` |
| 外域历练地图页 | 页签与占位面板已存在 | 接入路线风险、节点事件、战斗遭遇结算 | `CountyIdle/scenes/ui/WorldPanel.tscn` |
| 宗门见闻面板 / 宗务报表 | 面板容器与部分按钮接入已完成 | 将静态文案改为实时数据字段与历史统计 | `CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scripts/Main.cs` |
| 宗主治理中枢（三期：门规树 / 季度法令 / 执事任命） | 当前已完成“发展方向 / 法令 / 育才 + 季度法令 + 门规树一期”，仍缺执事任命模块 | 补执事任命与人才培养协同 | `CountyIdle/scripts/systems/SectGovernanceRules.cs`、`CountyIdle/scripts/systems/SectRuleTreeRules.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`、`docs/02_system_specs.md` |
| 宗主中枢模板 / 自动回退策略 | 当前已接入“治理力度 -> 职司”自动折算、岗位容量保护与旧存档默认方略推导 | 继续扩展为季度模板、事件驱动自动切换与更细的默认方案 | `CountyIdle/scripts/systems/SectTaskRules.cs`、`CountyIdle/scripts/systems/SectTaskSystem.cs`、`CountyIdle/scripts/Main.cs` |
| 门人分配与生活循环（住房 / 通勤 / 生老病死） | 已接入人口分层、患病恢复、衣物损耗、动态步行通勤到岗率（一期） | 接入住房到岗位空间映射输入、衣物产线供给与更完整人口面板 | `CountyIdle/scripts/models/GameState.cs`、`CountyIdle/scripts/systems/PopulationRules.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/systems/EconomySystem.cs` |
| 静态数据配置（buildings/jobs/items/monsters/traits） | 战略地图与岗位配置已接入，其余数据仍多为硬编码 | 继续建立统一配置加载层并替换经济/战斗等公式常量 | `CountyIdle/data/*.json`、`CountyIdle/scripts/systems/StrategicMapConfigSystem.cs`、`CountyIdle/scripts/systems/JobRoleSystem.cs` |
| 资源系统分层扩展（自然原料→加工链） | 六期已补“修仙材料语义化”：仓储面板、顶部资源提示、小时结算日志、人口缺口提示与外域原料来源标签统一切到 `灵木 / 灵草 / 玄铁矿 / 养气散 / 护山构件` 等修仙命名；`tools/SaveSmoke` 已补通 `SQLite` 存档/读档回归；五期的“可见库存整数化 + 隐藏进度池”与江陵府外域自然原料来源节点/标签继续保留，物品最小单位为 `1` | 剩余补 `Godot` 运行烟测；继续保持 `T2/T3` 不提前进入前台展示，禁止将 `青铜 / 纸张 / 玻璃 / 皮革 / 火药` 误放入原料层，并在后续试玩中观察修仙材料命名是否需要微调 | `docs/02_system_specs.md`、`CountyIdle/scripts/models/GameState.cs`、`CountyIdle/scripts/systems/InventoryRules.cs`、`CountyIdle/scripts/systems/MaterialRules.cs`、`CountyIdle/scripts/systems/MaterialSemanticRules.cs`、`CountyIdle/scripts/systems/ResourceSystem.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/systems/PrefectureMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/Main.cs`、`tools/SaveSmoke/bin/Debug/net8.0/SaveSmoke.exe` |
| 多存档槽管理界面 | 已接入手动槽 + 3 槽轮换自动槽列表、覆盖、读取、新建、复制、重命名、删除，并显示强化后的存档摘要预览、筛选排序与截图预览；本轮已补“留影录”书卷化外观与卷册语义文案 | 继续补外部导入导出等更完整的管理入口 | `CountyIdle/scripts/core/SaveSystem.cs`、`CountyIdle/scripts/core/SqliteSaveRepository.cs`、`CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/ui/MainSaveSlotsPanel.cs`、`CountyIdle/scripts/ui/MainSavePreview.cs` |
| 真传 / 英雄单位系统 | 探险已存在但为人口池抽象 | 增加英雄实体、职业成长、受装备与科技影响的个体冒险 | `CountyIdle/scripts/systems/CombatSystem.cs` |

## 4) 未实现（TODO）

| TODO | 优先级 | 目标验收（完成定义） | 设计来源 |
| --- | --- | --- | --- |
| Boss / 妖王机制与词条克制（历练后期） | P1 | 进入高层历练时出现 Boss 回合逻辑，掉落与克制关系可观测 | `docs/01_game_design_guide.md` |
| 护山战 / 守山压力闭环 | P1 | 威胁高位触发护山战，结果反哺城防与人口 / 资源状态 | `docs/01_game_design_guide.md` |
| 灵根苗子培育深度化（父母属性 / 近亲惩罚） | P1 | 后代生成受父母与血缘规则影响，日志可解释 | `docs/01_game_design_guide.md` |
| 职司转任系统 | P1 | 人口在条件满足时可跨职司转任，成本与收益可观测 | `docs/01_game_design_guide.md` |
| 装备 / 法器打造系统 | P1 | 增加材料 -> 打造 -> 品质结果链路，与历练与产业互相影响 | `docs/01_game_design_guide.md` |
| 传承 / 科技树分支化 | P1 | 形成分支科技路径，不同选择带来不同发展方向 | `docs/01_game_design_guide.md` |
| 公式全面配置化（去硬编码） | P2 | 关键系统公式由配置文件驱动，支持热调整/版本对比 | `docs/03_change_management.md` |

## 5) 使用方式（开发前 2 分钟）

1. 先看本文件第 2/3/4 节，确认目标属于 `✅/🟡/⭕` 哪类。
2. 再跳到 `docs/02_system_specs.md` 查对应系统规则。
3. 若本文件没有该需求，先登记到 `docs/08_development_list.md` 再开发。
4. 开始开发前补功能卡；若是机制/平衡改动，补提案与日志。
5. 每次开发后更新本文件状态，确保“实现状态与代码一致”。

## 6) 2026-03-09 表现层补记

- `DL-045 主界面六边形沙盘重构`：继续按卷轴参考图收敛视觉，已清理左侧动作区、岗位摘要卡与右侧日志正文中的残留黑底，统一回宣纸底色 + 墨字配色；tile 点击后的左栏信息 / 操作联动链路保持不变。
- 同日追加：左栏三个 tile 操作按钮补齐 `disabled` 态的卷轴样式，未选中时不再出现默认黑底按钮。
- 同日再次调整：主界面左栏已移除 `JobsPadding` 常驻摘要区，左侧只保留 `Tile Inspector` 主链路。
- 同日补足迁移闭环：原峰脉详批 / 协同峰令现已迁入独立 `SectOrganizationPanel` 卷册，由底部 `【峰令】谱系` 打开；主界面不再混放组织谱系信息，但规则层与协同峰结算继续保留。
- 同日继续收口：`DisciplePanel` 与 `SectOrganizationPanel` 已补齐标准卷轴外壳；至此现有 `PopupPanelBase` 弹窗与右栏札记卷的表现语汇已统一到同一套“书卷子面板家族”。
- 同日再补一轮：底部控制台、中央地图页签与缩放按钮补齐 `hover / pressed` 卷册态；左侧 `Tile Inspector` 动作文案去掉 emoji 并统一改成墨线批令语义。
- 同日修正：`PopupPanelBase` 统一抬升前景层级，修复点击人物打开“弟子谱”时主界面 hex 沙盘压到卷册正文上的 Z 轴问题。
- 2026-03-10 继续按参考图收敛：顶栏压成单行资源条；左栏重排为“浮云宗抬头 + 四宫格属性 + 精简动作”；右栏改为“天衍峰记事 + 峰务札记”卷栏；底栏隐藏 `重` 按钮与 `2x` 可见入口，保留既有绑定链路与功能可达性。
- 2026-03-10 治宗册二级菜单重排：`TaskPanel` 改为“左侧经脉导航 + 右侧卷宗滚动区”，将治理项拆为 `大政方针 / 节气法旨 / 门规戒律 / 庶务调度` 四栏；治理切换按钮统一改为 `◀ / ▶` 胶囊控件；右侧条目改竹简卡片并保留原有治理事件链与执事调度逻辑。
- 2026-03-10 弟子谱卷宗分页：`DisciplePanel` 改为“左侧导航 + 右侧卷宗滚动区”，拆分为 `峰内名录 / 人物档案 / 根基修为 / 印记批注`；名录卷保留筛选与名册树，其余卷按主题聚合当前弟子信息，不改结算与存档结构。
- 2026-03-10 弟子谱同页全展开：弹窗扩展为近全屏画幅，右侧三段（人物档案 / 根基修为 / 印记批注）与左侧名录同屏呈现，不再分页切换；保留筛选、名册树与详情联动链路。
- 2026-03-10 弟子谱宽屏仪表盘：右侧改为“先天根基 / 修为造化”双列，属性块改为 3x2 阵列，战力评定收敛为朱砂签样式；名录树降为两层分组，避免深层折叠。
- 2026-03-10 库藏卷扩幅重构：`WarehousePanel.tscn` 扩宽画幅并移除右侧建造/生产菜单，库存区改为 4~6 列自适应；新增刻度进度条与朱红满载提示；物资卡片 token 化、数量优先级提升，零库存灰态降噪。
- 2026-03-10 底部控制台选中态强化：快按入口（库房 / 中枢 / 谱系 / 弟子）改为靛青选中态，打开卷册时按钮高对比度高亮，悬停改为轻微放大并切换为靛青强调色并触发提示音，提升焦点辨识度。
- 2026-03-11 机宜卷即时生效分组：音量与分辨率调整即时生效，设置项分为“即时生效/批复生效”，并修复下拉框放大闪烁、点击后保持放大态与对齐，统一下拉框配色。




