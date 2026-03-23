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
| 灵根苗子与后辈培养 | 🟡 | 基础繁育已落地，深度培育与血缘规则未完成 |
| 真传与核心战力 | 🟡 | 当前仍以精英人口池抽象承担，尚未实体化 |
| 装备 / 法器体系 | 🟡 | 已有掉落与品质词条，未形成正式打造链 |
| 外务历练与护山反压 | 🟡 | 基础战斗与威胁已存在，护山闭环与高层压力未立住 |
| 双层时间与长期岁月推进 | 🟡 | 当前细时间已成立，长时间层与战略相位仍在设计中 |

## 3. 已立住功能（✅）

| 功能簇 | 当前裁定 | 主要入口 | 主要依据 |
| --- | --- | --- | --- |
| 主操作页面（TopBar / BottomBar / 地图） | 新增独立主操作页面，聚焦顶栏/底栏/地图三段布局（待 Godot 走查），便于调试与后续迭代 | `CountyIdle/scenes/MainOperation.tscn`、`CountyIdle/scenes/ui/TopBar.tscn`、`CountyIdle/scenes/ui/BottomBar.tscn`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/feature-cards/FC-20260322-main-operation-page.md` |
| 世界观与宗门语义基线 | 已统一到“浮云宗 / 天衍峰 / 青云峰总殿协同” | `docs/09_xianxia_sect_setting.md`、`CountyIdle/scripts/systems/SectMapSemanticRules.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs` | `docs/01_game_design_guide.md` |
| 主循环时间推进 | 当前口径为 `1 秒现实时间 = 1 游戏分钟`，每 `60` 游戏分钟做一次小时结算，支持 `x1 / x2 / x4` | `CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/GameCalendarSystem.cs`、`CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md`、`docs/12_runtime_formula_appendix.md` |
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
| 试玩目标引导（右栏试玩目标） | 右栏新增试玩目标面板，展示时辰结算 / 研修突破 / 历练结算三项目标并随 `GameState` 刷新 | `CountyIdle/scenes/ui/EventLogPanel.tscn`、`CountyIdle/scripts/ui/DemoPanel.cs`、`CountyIdle/scripts/ui/MainDemoPanel.cs`、`CountyIdle/scripts/Main.cs` | `docs/feature-cards/FC-20260316-playable-demo.md` |
| 建筑列表与快捷建造 | 右栏新增营建清单面板，展示主要建筑数量与建造消耗，按钮直连建造逻辑 | `CountyIdle/scenes/ui/EventLogPanel.tscn`、`CountyIdle/scripts/ui/BuildingListPanel.cs`、`CountyIdle/scripts/ui/MainBuildingListPanel.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/Main.cs` | `docs/feature-cards/FC-20260316-building-list.md` |
| 建筑落地可视化（山门图建筑显影） | 山门图按产业建筑数量生成对应锚点建筑显影，建造优先落在选中地块并刷新可见，落点写入存档可持久化 | `CountyIdle/scripts/systems/TownMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/Main.cs` | `docs/feature-cards/FC-20260316-building-visualization.md`、`docs/feature-cards/FC-20260316-building-placement-persistence.md` |
| 文明式营建卷（城建建造） | 营建卷支持建筑列表/详情对比/队列进度与时辰结算推进，完工后落点回写山门图并可撤销排队 | `CountyIdle/scenes/ui/ConstructionPanel.tscn`、`CountyIdle/scripts/ui/ConstructionPanel.cs`、`CountyIdle/scripts/ui/MainConstructionPanel.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs` | `docs/feature-cards/FC-20260316-civ5-style-construction-ui.md` |
| 天衍峰山门图与世界图 | 山门图、世界图、外域备用视图与双地图入口已建立 | `CountyIdle/scripts/systems/SectMapViewSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/02_system_specs.md` |
| 弟子谱与宗门组织谱 | 弟子谱、峰令谱与卷册式总览已接入正式 UI | `CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md` |
| 弟子随机生成器（开发辅助） | 提供可复现随机弟子名册，用于弟子谱/系统预览，不写入 `GameState` | `CountyIdle/scripts/systems/DiscipleRosterSystem.cs` | `docs/feature-cards/FC-20260316-random-disciple-generator.md` |
| 弟子谱随机名册预览按钮（调试） | 弟子谱内提供随机名册预览入口，默认仅 Debug 构建可见 | `CountyIdle/scenes/ui/DisciplePanel.tscn`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd` | `docs/feature-cards/FC-20260316-disciple-roster-random-preview.md` |
| 弟子谱 UI 布局对齐参考版 | 已从旧的“左侧名册 + 右侧详情卡”厚重布局进一步重构为“左侧支脉导航 + 右侧血脉族谱”的清简玉简版树页，同时保留命谱详情页承接深度信息 | `CountyIdle/scenes/ui/DisciplePanel.tscn`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd` | `docs/feature-cards/FC-20260323-minimalist-jade-roster.md` |
| 弟子个人页详录分组补强 | 个人页概览补入修为，并将卷中详录分组为身份/修为/性情三段，补齐修为进度、气海蓄量、战力评定与灵根摘要 | `CountyIdle/scripts/ui/DisciplePanel.cs` | `docs/feature-cards/FC-20260316-disciple-personal-page-refine.md` |
| 弟子个人页评语深化 | 卷中详录补入根骨/心境/体魄评语与培养侧重，强化个人页文字解读 | `CountyIdle/scripts/ui/DisciplePanel.cs` | `docs/feature-cards/FC-20260316-disciple-personal-page-deepen.md` |
| 弟子个人页履历与修行摘要 | 卷中详录补入履历侧记与修行阶段，强化当前轨迹与修行档位的快速阅读 | `CountyIdle/scripts/ui/DisciplePanel.cs` | `docs/feature-cards/FC-20260316-disciple-personal-page-resume.md` |
| 弟子个人页修行安排摘要 | 卷中详录补入修行安排摘要，结合批注与培养侧重形成可读的修行策略提示 | `CountyIdle/scripts/ui/DisciplePanel.cs` | `docs/feature-cards/FC-20260316-disciple-personal-page-cultivation-plan.md` |
| 弟子个人页装备/法器摘要 | 卷中详录补入装备/法器摘要与战备品阶提示，补齐弟子行装概览 | `CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/systems/DiscipleEquipmentRules.cs`、`CountyIdle/scripts/models/GameState.cs` | `docs/feature-cards/FC-20260316-disciple-personal-page-equipment-summary.md` |
| 仓储卷修仙化 UI 改版（藏宝阁） | 仓储卷页签、预警文案与品阶配色升级为修仙语义，强化法宝/功法/丹药/天材分类感 | `CountyIdle/scenes/ui/WarehousePanel.tscn`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/ui/gd/WarehousePanelTransition.gd` | `docs/feature-cards/FC-20260316-ui-layout-migration-01.md` |
| 多存档槽与 `SQLite` 存档 | 多槽、自动槽轮换、摘要预览与读写主链已成立；留影录现已升级为“玉简档案架 + 展开画卷”结构，主动作视觉上收口为 `焚毁 / 另拓 / 覆写 / 启读`，并继续保留 `天道刻印` 自动槽语义；卷尾 `合卷` 动作现已复用机宜卷抽出的 `SealButton` 印章组件，进一步统一卷册家族视觉 | `CountyIdle/scripts/core/SaveSystem.cs`、`CountyIdle/scripts/core/SqliteSaveRepository.cs`、`CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/ui/SaveJadeSlipItem.cs`、`CountyIdle/scripts/ui/gd/SavePreviewCrossfade.gd`、`CountyIdle/scenes/ui/components/SealButton.tscn` | `docs/13_runtime_support_appendix.md`、`docs/feature-cards/FC-20260320-save-slots-archive-scroll-upgrade.md` |
| UI 表现层 `GDScript` 辅助首批接入 | 仓储卷开场 / 分页、留影录预览切换、山门 hex hover 高亮已下放至 `GDScript`，权威逻辑仍留在 `C#` | `CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/ui/gd/WarehousePanelTransition.gd`、`CountyIdle/scripts/ui/gd/SavePreviewCrossfade.gd`、`CountyIdle/scripts/map/gd/HexHoverHighlight.gd` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split.md` |
| UI 表现层 `GDScript` 辅助二批接入 | 治宗册开场 / 切页、弟子谱筛选 / 详情切换、中部地图页签 / 二级地图检视脉冲已下放至 `GDScript`，权威逻辑仍留在 `C#` | `CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/MainWorldSitePanel.cs`、`CountyIdle/scripts/ui/gd/TaskPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/WorldPanelVisualFx.gd` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch2.md` |
| UI 表现层 `GDScript` 辅助三批接入 | 设置卷开场 / 录键高亮、宗门组织谱切峰 / 切职司、中部地图顶部标签强调反馈已下放至 `GDScript`，权威逻辑仍留在 `C#` | `CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/gd/SettingsPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/SectOrganizationPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/MainTopTabVisualFx.gd` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch3.md` |

| UI 表现层 `GDScript` 辅助四批接入 | 主界面底栏快捷键 / 倍速 / 存读设按钮的 hover / focus 灯笼强调反馈已下放至 `GDScript`，`Main.cs` 仅保留点击与业务绑定，权威逻辑仍留在 `C#` | `CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/gd/BottomBarLanternFx.gd`、`CountyIdle/scenes/ui/BottomBar.tscn`、`CountyIdle/scenes/ui/figma/BottomBar.tscn` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch4.md` |

| UI 表现层 `GDScript` 辅助五批接入 | `Main.cs` 中剩余全局 hover / focus 灯笼反馈、`OptionButton` popup 表现样式与 hover 锁定已下放至 `GDScript`，`Main.cs` 仅保留一次性绑定转发，权威逻辑仍留在 `C#` | `CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/gd/MainLanternFx.gd`、`CountyIdle/scenes/Main.tscn` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch5.md` |

| UI 表现层 `GDScript` 辅助六批接入 | 留影录右侧详情列的空态 / 预览态过渡已继续下放至 `GDScript`，预览框、详情文本、题名行与按钮行统一由 `SavePreviewCrossfade.gd` 承接，权威逻辑仍留在 `C#` | `CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/ui/gd/SavePreviewCrossfade.gd`、`CountyIdle/scenes/ui/SaveSlotsPanel.tscn` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch6.md` |

| UI 表现层 `GDScript` 辅助七批接入 | 治宗册与机宜卷的书卷静态样式、字段皮肤与按钮外观已继续下放至 `GDScript`，`TaskPanel.cs` / `SettingsPanel.cs` 仅保留权威逻辑与单向调用边界；本轮治宗册卷首 `收卷` 动作也已复用 `SealButton` 组件 | `CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/gd/TaskPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/SettingsPanelVisualFx.gd`、`CountyIdle/scenes/ui/components/SealButton.tscn` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch7.md` |

| UI 表现层 `GDScript` 辅助八批接入 | 弟子谱与峰令谱的书卷静态样式、筛选控件 / 卡片 / 动态导航外观已继续下放至 `GDScript`，`DisciplePanel.cs` / `SectOrganizationPanel.cs` 仅保留权威逻辑、动态数据与单向调用边界；本轮两者卷首 `收卷` 动作也已复用统一 `SealButton` 组件 | `CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/SectOrganizationPanelVisualFx.gd`、`CountyIdle/scenes/ui/components/SealButton.tscn` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch8.md` |

| UI 表现层 `GDScript` 辅助九批接入 | 留影录与二级地图页的书卷静态样式、字段皮肤、预览区与 world-site sandbox 壳层外观已继续下放至 `GDScript`，`SaveSlotsPanel.cs` / `MainWorldSitePanel.cs` 仅保留权威逻辑、地图数据与单向调用边界 | `CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/ui/MainWorldSitePanel.cs`、`CountyIdle/scripts/ui/gd/SavePreviewCrossfade.gd`、`CountyIdle/scripts/ui/gd/WorldPanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch9.md` |

| UI 表现层 `GDScript` 辅助十批回收 | 治宗册与弟子谱中已迁移到 `GDScript` 的历史静态样式 helper 已从 `C#` 回收清理，`TaskPanel.cs` / `DisciplePanel.cs` 进一步收口到权威逻辑、数据刷新与输入处理 | `CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/gd/TaskPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch10.md` |

| UI 表现层 `GDScript` 辅助十一批拆分 | 仓储卷剩余的静态主题、页签选中态、库容负载色调与资源卡片纯视觉样式继续下放至 `GDScript`，`WarehousePanel.cs` 仅保留库存数据、按钮事件与提示文本逻辑 | `CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/ui/gd/WarehousePanelTransition.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch11.md` |

| UI 表现层 `GDScript` 辅助十二批拆分 | 左侧地块检视器中世界点位 / 山门地块的标题、副标题、徽签与状态值纯色调切换已继续下放至 `GDScript`，`MainSectTileInspector.cs` 仅保留按钮绑定、描述文案、badge 语义与规则判断 | `CountyIdle/scripts/ui/MainSectTileInspector.cs`、`CountyIdle/scripts/ui/gd/TileInspectorVisualFx.gd`、`CountyIdle/scenes/Main.tscn` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch12.md` |

| UI 表现层 `GDScript` 辅助十三批拆分 | 弟子谱中的雷达图展示控件已继续下放至独立 `GDScript`，`DisciplePanel.cs` 不再内嵌自绘雷达图实现，仅保留名册数据、筛选排序与详情文案 | `CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/gd/DiscipleRadarChart.gd`、`CountyIdle/scenes/ui/DisciplePanel.tscn` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch13.md` |

| UI 表现层 `GDScript` 辅助十四批拆分 | 弟子谱中剩余的指标值颜色切换与 trait tag 纯视觉样式已继续下放至 `GDScript`，`DisciplePanel.cs` 进一步收口到名册数据、筛选排序与详情文案 | `CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch14.md` |

| UI 表现层 `GDScript` 辅助十五批拆分 | 二级地图页的 `GeneratedSecondarySandboxView` 壳层结构已从 `C#` 运行时动态构建回收到 `WorldPanel.tscn`，`MainWorldSitePanel.cs` 仅保留 world-site 数据绑定、sandbox 数据注入与入口行为 | `CountyIdle/scripts/ui/MainWorldSitePanel.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scripts/ui/gd/WorldPanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch15.md` |

| UI 表现层 `GDScript` 辅助十六批拆分 | 世界/外域/山门地图页底部 `MapDirectiveRow` 的状态字色调与调度按钮强调样式已继续下放至 `GDScript`，`MainMapOperationalLink.cs` 仅保留地图态势快照、按钮动作与文案绑定 | `CountyIdle/scripts/ui/MainMapOperationalLink.cs`、`CountyIdle/scripts/ui/gd/WorldPanelVisualFx.gd`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch16.md` |

| UI 表现层 `GDScript` 辅助十七批拆分 | 世界图 / 外域图标题与山门图 `MapHintLabel` 的地图态势色调已继续下放至局部 `GDScript` helper，`StrategicMapViewSystem.cs` / `CountyTownMapViewSystem.cs` 仅保留态势快照、标题文案与地图绘制 | `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/ui/gd/StrategicMapPanelToneFx.gd`、`CountyIdle/scripts/ui/gd/CountyTownMapHintFx.gd`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch17.md` |

| UI 表现层 `GDScript` 辅助十八批回收 | `Main.cs` 中旧 `job-row / priority` 视觉 helper、未接线字典与选中样式残留已从 `C#` 回收清理；主界面继续只保留现行峰脉摘要、地图调度与面板入口逻辑 | `CountyIdle/scripts/Main.cs` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch18.md` |

| UI 表现层 `GDScript` 辅助十九批拆分 | 世界图 `WorldTerrainTileLayer` 的地图态势 tint 已继续下放至局部 `GDScript` helper，`StrategicMapViewSystem.cs` 仅保留 terrain layer 的可见性、位置与缩放控制 | `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/ui/gd/StrategicMapPanelToneFx.gd`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch19.md` |

| UI 表现层 scene-side 第二十批回收 | 主界面背景 `TextureRect` 的静态视觉布局参数已回收到 `Main.tscn`，`Main.cs` 不再在运行时重复设置背景显隐 / 拉伸 / 层级 / 默认色调或注册冗余 resize 校正逻辑 | `CountyIdle/scenes/Main.tscn`、`CountyIdle/scripts/Main.cs` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch20.md` |

| UI 表现层 `GDScript` 辅助二十一批拆分 | 峰令谱动态峰脉导航卡 / 职司卡的交互光标与三类动态卡片的内间距壳层已继续下放至 `SectOrganizationPanelVisualFx.gd`，`SectOrganizationPanel.cs` 仅保留动态卡片生成、输入与业务刷新 | `CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/ui/gd/SectOrganizationPanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch21.md` |

| UI 表现层 `GDScript` 辅助二十二批拆分 | 峰令谱动态卡片 `MarginContainer` 的统一留白壳层已继续下放至 `SectOrganizationPanelVisualFx.gd`，`SectOrganizationPanel.cs` 不再直接写入卡片内边距常量 | `CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/ui/gd/SectOrganizationPanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch22.md` |

| UI 表现层 `GDScript` 辅助二十三批回收 | 峰令谱中已收口到 `VisualFx` 边界后的冗余 `CreateMarginContainer()` helper 与分散 `_visualFx?.Call(...)` 残留已从 `SectOrganizationPanel.cs` 回收；面板继续仅保留动态卡片生成、输入与业务刷新 | `CountyIdle/scripts/ui/SectOrganizationPanel.cs` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch23.md` |

| UI 表现层 `GDScript` 辅助二十四批回收 | 峰令谱初始主题应用的重复触发已从 `SectOrganizationPanel.cs` 回收，改由 `SectOrganizationPanelVisualFx.gd` 在 `_ready()` 中单点承接；面板继续仅保留动态卡片生成、输入与业务刷新 | `CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/ui/gd/SectOrganizationPanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch24.md` |

| UI 表现层 `GDScript` 辅助二十五批回收 | 弟子谱、留影录、机宜卷、治宗册与仓储卷中重复的初始主题触发已从 `C#` 回收，统一改由各自 `VisualFx.gd` 的 `_ready()` 单点承接；对应面板继续仅保留数据、输入与业务刷新 | `CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch25.md` |

| UI 表现层 `GDScript` 辅助二十六批回收 | 二级地图页 world-site 主题初始化的重复触发已从 `MainWorldSitePanel.cs` 回收，统一改由 `WorldPanelVisualFx.gd` 的 `_ready()` 单点承接；主脚本继续仅保留 world-site 数据绑定、入口行为与 sandbox 注入 | `CountyIdle/scripts/ui/MainWorldSitePanel.cs`、`CountyIdle/scripts/ui/gd/WorldPanelVisualFx.gd` | `docs/feature-cards/FC-20260315-ui-gdscript-boundary-split-batch26.md` |

| UI 表现层 `GDScript` 辅助二十七批回收 | 地图调度条在“无状态 / 行隐藏”两种分支下共享的收尾逻辑已从 `MainMapOperationalLink.cs` 内联重复处回收到单一 helper，继续明确 `C#` 仅负责状态分支与 `WorldPanelVisualFx.gd` 的单向 reset 调用 | `CountyIdle/scripts/ui/MainMapOperationalLink.cs` | `docs/feature-cards/FC-20260315-ui-gdscript-boundary-split-batch27.md` |

| UI 表现层 `GDScript` 辅助二十八批回收 | 左侧地块检视器中三类按钮绑定的薄壳 setter 与 disabled binding 构造重复已从 `MainSectTileInspector.cs` 收口到统一 helper，继续明确 `C#` 仅保留检视摘要、动作语义与对 `TileInspectorVisualFx.gd` 的单向 tone 调用 | `CountyIdle/scripts/ui/MainSectTileInspector.cs` | `docs/feature-cards/FC-20260315-ui-gdscript-boundary-split-batch28.md` |

| UI 表现层 `GDScript` 辅助二十九批收尾巡检 | 对 `MainMapOperationalLink.cs`、`MainSectTileInspector.cs`、`MainWorldSitePanel.cs` 与多卷册 UI 剩余边界做最终巡检后，仅补做 `MainSectTileInspector.cs` 末端按钮 helper 的非空签名收紧；其余残留已确认属于 tooltip / `Visible` 业务切换 / 地图渲染 authority / 数据绑定边界，继续保留在 `C#` | `CountyIdle/scripts/ui/MainSectTileInspector.cs`、`CountyIdle/scripts/ui/MainMapOperationalLink.cs`、`CountyIdle/scripts/ui/MainWorldSitePanel.cs`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs` | `docs/feature-cards/FC-20260315-ui-gdscript-boundary-split-batch29.md` |

| 卷册弹窗排他与快捷键门禁收口 | 主界面打开设置卷 / 仓储卷 / 治宗册 / 弟子谱 / 峰令谱 / 留影录时，现已统一先收起其他卷册弹窗；对应全局快捷键在这些卷册可见时也会统一让行，避免多卷叠层与误触全局操作 | `CountyIdle/scripts/ui/MainPopupCoordination.cs`、`CountyIdle/scripts/ui/MainClientSettings.cs`、`CountyIdle/scripts/ui/MainWarehousePanel.cs`、`CountyIdle/scripts/ui/MainTaskPanel.cs`、`CountyIdle/scripts/ui/MainDisciplePanel.cs`、`CountyIdle/scripts/ui/MainSectOrganizationPanel.cs`、`CountyIdle/scripts/ui/MainSaveSlotsPanel.cs`、`CountyIdle/scripts/ui/MainShortcutBindings.cs`、`CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs` | `docs/feature-cards/FC-20260315-popup-exclusivity-shortcut-guard.md` |

| 双地图兼容页签入口收口 | 主界面地图页签现继续只保留 `山门沙盘 / 世界舆图` 两个可交互入口；历史兼容的 `Prefecture / Event / Report / Expedition` 页签已统一退为隐藏且禁用状态，`Main.cs` 也不再把这些兼容按钮纳入现行点击绑定与双地图主链必需节点 | `CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/feature-cards/FC-20260315-dual-map-compat-tab-cleanup.md` |

## 4. 未立稳功能（🟡）

| 功能簇 | 当前裁定 | 下一步收口 | 主要入口 |
| --- | --- | --- | --- |
| UI 响应式布局标准化 | 主界面与核心卷册已开始容器化改造，且已把项目基础视口与窗口最小尺寸收口到 `1280x720 (720p)`；设置卷现已进一步收口为“左玉简页签 + 右宣纸正文 + 诗文瀑布背景”的卷册式布局，同时保留“全屏复选框 / 窗口分辨率 / 画面缩放滑条 / 快捷键录入”权威逻辑，高分辨率下默认显示更多内容而不是把画面放大；本轮又按实机参考图继续收口：音画页改成 `天籁总纲 / 仙乐玄音 / 金石交鸣 / 须弥幻境 / 灵光显像 / 乾坤视野` 的长滑条与书法单选结构，符节页改成 `步法与神通 / 法器与内视` 两段分组卡片格，法则页改成 `文字同源 / 铭刻天机 / 气血浮沉 / 煞气留痕` 的语言单选与三枚敕令开关；并已将设计文档中的 `paper_grain / jade_slip_base / seal_character_chi / seal_button_shape` 四枚 SVG 正式接到设置卷纸面底纹、玉简页签、敕令开关与卷尾印章，且将 `JadeTab / SealToggle / SealButton` 抽成可复用组件场景；客户端设置模型也已补入 `ShowDamageText / EnableBloodGore` 等字段；`dotnet build` 与设置卷 Godot headless 场景检查已通过，仍待全量 F5 走查 | 补 Godot/F5 走查，重点检查设置卷页签切换、卷首标题更新、BGM/SFX 滑条、画质/分辨率书法单选、符节页九宫格卡片、法则页三枚敕令开关，以及修炼卷/弟子谱命谱页/见闻报表卷与中部地图区在 `1280x720`、更高分辨率与窗口缩放下的滚动/裁剪、背景铺满与地图可读性 | `CountyIdle/project.godot`、`CountyIdle/scenes/Main.tscn`、`CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scenes/ui/SettingsPanel.tscn`、`CountyIdle/scenes/ui/components/JadeTab.tscn`、`CountyIdle/scenes/ui/components/SealToggle.tscn`、`CountyIdle/scenes/ui/components/SealButton.tscn`、`CountyIdle/scenes/ui/DisciplePanel.tscn`、`CountyIdle/scenes/ui/CultivationPanel.tscn`、`CountyIdle/scenes/ui/SectChroniclePanel.tscn`、`CountyIdle/scripts/core/ClientSettingsSystem.cs`、`CountyIdle/scripts/models/ClientSettings.cs`、`CountyIdle/scripts/ui/MainClientSettings.cs`、`CountyIdle/scripts/ui/MainSaveSlotsPanel.cs`、`CountyIdle/scripts/ui/MainShortcutBindings.cs`、`docs/feature-cards/FC-20260318-ui-responsive-layout-standard.md`、`docs/feature-cards/FC-20260321-client-settings-panel-expansion.md` |
| 国风题屏主菜单（玉简起卷） | `Main.tscn` 已补宣纸底 / 山水底图水墨化背景 / 诗文瀑布 / 玉简菜单 / 印章落款的启动遮罩，并把 `开始 / 读取 / 设置 / 退出` 接到正式入口；题屏玉简现已切回 SVG 直显，并改由 Godot `StyleBoxFlat` 提供描边与投影，以降低贴边灰边与锯齿感；本轮已改用 `background_frosted_glass.gdshader` 处理 `background_2.png`，并补入低频呼吸动画，使 `distortion_strength` 在 `1.5 ~ 3.0`、`diffusion_strength` 在 `0.0 ~ 0.6` 间缓慢摆动，同时把诗文瀑布改为仅在题屏打开时激活且默认行数下调；`dotnet build` 已通过，且设置卷的 Godot headless 场景检查已补过，不再阻塞题屏联调；当前仍有 `xianxia.css` 资源 UID warning 等既有工程问题，`MOD` 仍为提示占位，待补 F5 实机视觉巡检 | 重点验证题屏期间 `GameLoop` 暂停、主界面快捷键让行、`SaveSlotsPanel` / `SettingsPanel` 叠层顺序、山水底图经 frosted-glass shader 处理后是否仍具水墨感且不过度模糊、呼吸节奏是否过快，以及题屏打开时帧耗是否明显下降 | `CountyIdle/scenes/ui/title/TitleMenuOverlay.tscn`、`CountyIdle/scenes/ui/title/JadeMenuItem.tscn`、`CountyIdle/scripts/ui/title/TitleMenuOverlay.cs`、`CountyIdle/scripts/ui/title/JadeMenuItem.cs`、`CountyIdle/scripts/ui/title/PoemWaterfallManager.cs`、`CountyIdle/scripts/ui/MainTitleMenu.cs`、`CountyIdle/scripts/ui/MainSaveSlotsPanel.cs`、`CountyIdle/scripts/ui/MainShortcutBindings.cs`、`CountyIdle/assets/ui/background/background_2.png`、`CountyIdle/assets/ui/background/background_frosted_glass.gdshader`、`CountyIdle/assets/ui/title/jade_leaf.svg`、`docs/feature-cards/FC-20260320-title-menu-jade-scroll.md` |
| 修仙控件 SVG 主题图标包 | `HanCourtyardTheme.tres` 与 `xianxia.tres` 都已接入 checkbox / radio / slider / option arrow / toggle 的 SVG 资源，`dotnet build` 已通过，但仍缺 Godot/F5 走查 | 补设置卷、留影录、弟子谱与后续 `CheckButton` 实例的实机视觉巡检 | `CountyIdle/themes/HanCourtyardTheme.tres`、`CountyIdle/themes/xianxia.tres`、`CountyIdle/scripts/ui/gd/SettingsPanelVisualFx.gd`、`CountyIdle/assets/ui/icons/*.svg`、`docs/feature-cards/FC-20260319-ui-control-svg-icon-theme.md` |
| 弟子谱 CSS 主题原型 | 现已作为命谱详情页与弟子谱局部主题底座继续保留；树页则进一步收口为“清简留白”玉简导航与族谱节点风格，`dotnet build` 已通过，但仍缺 Godot/F5 走查与树页实机排版校验 | 先验证 `DisciplePanel` 树页与详情页同屏切换表现，再决定是否扩展到治宗册、修炼卷等其他卷册 | `CountyIdle/ui/xianxia.css`、`CountyIdle/scenes/ui/DisciplePanel.tscn`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`docs/feature-cards/FC-20260323-minimalist-jade-roster.md` |
| 修炼卷可复用修仙控件场景 | 修炼卷已抽出 `SoulCore`、`JadeSlipCard`、`SpiritStatBar` 三类可复用场景，用于封装阵眼 HUD、玉简决策卡与菱形火候条；本轮又继续按《山海机局》总纲把 `CultivationPanel` 收口到“宣纸 + 玉璧 + 朱砂敕令”语义，补入 `玲珑心位`、竖排 `壹时辰` 与 `exquisite_heart / decree_talisman` 资源底座；当前仍待 Godot/F5 表现确认与后续跨卷复用范围裁定 | 先验证 `CultivationPanel` 的宣纸卷面、玲珑心位与敕令印章表现，再决定是否扩展到弟子谱、治宗册等卷册 | `CountyIdle/scenes/ui/components/SoulCore.tscn`、`CountyIdle/scenes/ui/components/JadeSlipCard.tscn`、`CountyIdle/scenes/ui/components/SpiritStatBar.tscn`、`CountyIdle/scenes/ui/CultivationPanel.tscn`、`CountyIdle/scripts/ui/CultivationPanel.cs`、`CountyIdle/scripts/ui/gd/CultivationPanelVisualFx.gd`、`CountyIdle/assets/ui/cultivation/exquisite_heart.svg`、`CountyIdle/assets/ui/cultivation/decree_talisman.svg`、`CountyIdle/ui/xianxia.css`、`docs/feature-cards/FC-20260319-xianxia-prefab-controls.md`、`docs/feature-cards/FC-20260323-cultivation-scroll-linglong-layout.md` |
| 宗主治理中枢三期 | 季度法令与门规树一期已落地，仍缺执事任命 | 补执事任命与人才培养协同 | `CountyIdle/scripts/systems/SectGovernanceRules.cs`、`CountyIdle/scripts/ui/TaskPanel.cs` |
| 宗主中枢模板与自动回退 | 已有“治理力度 -> 职司”折算与容量保护 | 扩到季度模板、事件驱动自动切换与更细默认方案 | `CountyIdle/scripts/systems/SectTaskRules.cs`、`CountyIdle/scripts/systems/SectTaskSystem.cs` |
| 传承分支化 | 当前仍为线性突破 | 形成分支树、路径差异与互斥 / 协同关系 | `CountyIdle/scripts/systems/ResearchSystem.cs` |
| 门人生活循环深化 | 已有住房、伤病、通勤与基础恢复 | 补住房到岗位空间映射、衣物供给与更完整人口面板 | `CountyIdle/scripts/systems/PopulationSystem.cs`、`CountyIdle/scripts/models/GameState.cs` |
| 资源系统分层扩展 | 前台 `T0 / T1` 与修仙材料语义已收口 | 继续稳住前台层级，并补完整运行烟测 | `CountyIdle/scripts/systems/MaterialSemanticRules.cs`、`CountyIdle/scripts/ui/WarehousePanel.cs`、`docs/12_runtime_formula_appendix.md` |
| 仓储入库分层（行囊/成品） | 行囊与工坊成品暂存已接入，`dotnet build` 当前失败（`MainSectTileInspector.cs` 编译错误），仍缺运行验证 | 补仓储卷走查、Godot/F5 与存档/结算烟测 | `CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/systems/WorkshopCraftedInventoryRules.cs` |
| 静态数据配置化 | 战略地图与部分岗位配置已接入 | 继续将关键公式与常量从硬编码迁到 `data/*.json` | `CountyIdle/data/*.json`、`CountyIdle/scripts/systems/StrategicMapConfigSystem.cs` |
| 战略地图配置驱动 | 已有配置 fallback，默认主视图转为程序化生成，调试入口已补 | 补配置内容覆盖，并评估默认切回配置驱动 | `CountyIdle/scripts/models/StrategicMapConfig.cs`、`CountyIdle/scripts/systems/XianxiaWorldGeneratorSystem.cs` |
| 外域历练地图页 | 页签与占位面板已存在 | 补路线风险、节点事件与遭遇结算 | `CountyIdle/scenes/ui/WorldPanel.tscn` |
| 宗门见闻面板 / 宗务报表 | 右栏“天衍峰记事”摘要已可展开为“见闻报表卷”原型，卷内包含实时警讯、分类筛读近时札记、经营快照、近几次时辰结算回看与季度/年度摘要；仍缺更长周期统计与更细事件分类回溯 | 继续补季度/年度统计深化、事件类型细分与更细的报表对照 | `CountyIdle/scenes/ui/EventLogPanel.tscn`、`CountyIdle/scenes/ui/SectChroniclePanel.tscn`、`CountyIdle/scripts/ui/SectChroniclePanel.cs`、`CountyIdle/scripts/ui/MainSectChronicleHistory.cs`、`CountyIdle/scripts/systems/SectChronicleRules.cs`、`CountyIdle/scripts/Main.cs` |
| 弟子谱双页拆分（宗门大谱 / 命谱详情） | 树页已继续升级为“灵鉴录·清简留白”版本：左侧支脉导航、右侧血脉族谱、弟子玉简节点点击后进入命谱详情；`dotnet build` 已通过，仍待 Godot 场景走查 | 宗门大谱采用极简胶囊玉简谱系展示，命谱详情继续承接雷达图、六维属性、修炼与敕令操作 | `CountyIdle/scenes/ui/DisciplePanel.tscn`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd`、`docs/ui-prototypes/sect_tree.html`、`docs/ui-prototypes/character_profile.html`、`docs/feature-cards/FC-20260323-minimalist-jade-roster.md` |
| 弟子谱交互指令（三期） | 弟子谱已可对个体下达 `常制观察 / 外务候补 / 执事培养` 三档批注，并分别接入历练战力 / 外务回流、内务执行效率与贡献回流；系统还能从执事培养名册中自动为当前内务条目匹配补位执事，且弟子谱 / 治宗册会显示同一补位结果；仍缺正式执事任命与长期履历 | 继续扩到正式执事任命、季度任命与长期履历 | `CountyIdle/scripts/systems/DiscipleDirectiveRules.cs`、`CountyIdle/scripts/systems/DiscipleDirectiveSystem.cs`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/CombatSystem.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs`、`CountyIdle/scripts/ui/TaskPanel.cs` |
| 弟子修炼卷（修炼安排页） | 修炼卷已支持按弟子登记技能修炼 / 功法打磨 / 技艺练习 / 打坐修炼四类主修安排，状态写入 `GameState` 并可从弟子谱直接跳转定位；当前已接入时辰结算，可折算为传承研修、贡献、工器、民心与危兆缓冲等聚合收益，并新增逐弟子的根基磨砺 / 功法火候 / 技艺手感 / 静修积气长期成长摘要、近时履历回看、个体表现温和联动、培养路数/路数批语、外务候补 / 执事培养 / 内务补位的轻量差事优势联动，以及基于长期火候周期生成的修炼感悟札记；札记触发时还会结出轻量专属机缘，并在弟子谱 / 修炼卷突出显示最近感悟；卷面也已继续收口为“弟子灵鉴 + 玲珑心位 + 符命法门库”的宣纸卷册版式：左侧保持玉璧与玉色火候条，右上按资质显示 1~3 枚 `玲珑心位`，法门卡切到更轻的符纸与竖排 `壹时辰` 表达；同一路数还能继续细化为基于次强火候推导的专修分支，并进一步映照进功法/技艺专精称谓、分支成形履历与专精效用，但尚缺成体系专属事件链 | 接入更细技能/功法成长联动、长期培养分支、成体系专属事件与 Godot/F5 卷面实机校验，重点验证玲珑心位文案不会误导为多法门并行 | `CountyIdle/scenes/ui/CultivationPanel.tscn`、`CountyIdle/scripts/ui/CultivationPanel.cs`、`CountyIdle/scripts/ui/MainCultivationPanel.cs`、`CountyIdle/scripts/ui/gd/CultivationPanelVisualFx.gd`、`CountyIdle/scripts/systems/DiscipleCultivationRules.cs`、`CountyIdle/scripts/systems/DiscipleCultivationSystem.cs`、`CountyIdle/scripts/systems/DiscipleCultivationSettlementSystem.cs`、`CountyIdle/scripts/systems/DiscipleRosterSystem.cs`、`CountyIdle/scripts/systems/DiscipleDirectiveRules.cs`、`CountyIdle/scripts/systems/DiscipleDirectiveSystem.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs`、`CountyIdle/scripts/systems/CombatSystem.cs`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/models/GameState.cs`、`CountyIdle/scripts/Main.cs`、`docs/feature-cards/FC-20260323-cultivation-scroll-linglong-layout.md` |
| 真传 / 英雄实体化 | 当前仍由精英人口池抽象承担 | 形成可成长个体、职业成长与装备联动 | `CountyIdle/scripts/systems/CombatSystem.cs` |
| 天衍峰院域坊局与全格检视 | 已有全格点击检视，且可在当前 hex 间切换主修 / 协同 / 稳态坊局；历史“左侧检视器”已收口为常驻浮动“院域营建卷”，当前只保留地貌、位序、建筑摘要与“对该地块营建”入口，旧的弟子 / 仓储 / 调度动作区已从卷面移除，并将建筑总览前置为主视觉 | 院域事件触发与小时结算联动、继续细化建造专用卷册的字段精简与定位体验 | `CountyIdle/scripts/models/TownCellCompoundData.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.CompoundPlanning.cs`、`CountyIdle/scripts/ui/MainSectTileInspector.cs`、`CountyIdle/scenes/ui/JobsPanel.tscn`、`CountyIdle/scenes/Main.tscn` |
| 世界格二级地图分层与入口 | 已支持世界地图任意 hex 点选后在左侧显示详情，并通过进入按钮生成与山门沙盘同形的下一层 hex 沙盘；局部沙盘点选也会复用左侧检视器，形成同族检视闭环；当前又进一步补上“继承自 world hex 的地形家族”口径，二级地图 external map 会按 `Plain / Spirit / Rugged / Snow / Water` 底盘分桶复用现有 `L1_hex_tileset.tres`，不再只按 `Ground / Courtyard / Water / Road` 粗分类回退同一种地面；并且 `RoadMask / RiverMask / CliffMask` 已开始继续影响局部道路入口、水体边界与 ridge / hazard 的主要方向，多方向会合成主轴并收口到同一焦点，对向成对时生成贯通线，入口方向随合成主轴稳定落点 | 继续细化 `宗门 / 野外 / 坊市 / 遗迹` 四类专属模板与真实玩法，以及二级地图内专属交互控件 | `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.WorldCellSelection.cs`、`CountyIdle/scripts/systems/WorldSiteLocalMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/WorldTerrainVisualRules.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/models/TownMapData.cs`、`CountyIdle/scripts/ui/MainWorldSitePanel.cs`、`CountyIdle/scripts/ui/MainSectTileInspector.cs`、`CountyIdle/scripts/Main.cs` |
| 全景主操作 HUD（天地人一体） | 主界面已接入 `CanvasLayer` 全屏 HUD：顶栏聚合节气/日期/资源，左右浮窗承载地块检视与宗门纪事，底栏改为令箭式主操作中枢并补二级卷轴；当前已保持地图点击与 HUD 交互分层，但仍待 Godot/F5 走查与细化副卷数据对接 | `CountyIdle/scenes/ui/MainHUD.tscn`、`CountyIdle/scenes/ui/TopBar.tscn`、`CountyIdle/scenes/ui/BottomBar.tscn`、`CountyIdle/scripts/ui/gd/CommandToken.gd`、`CountyIdle/scripts/ui/gd/MainHudBottomBar.gd`、`CountyIdle/scripts/Main.cs` | `docs/feature-cards/FC-20260321-main-hud-fullscreen.md` |
| 地图素材分层资产流水线 | 文档三期 + 运行时五期已接入：已新增正式规格文档与 L1-L5 绘制实施方案，当前宗门图已接入 `Layer 2` decal / 连接纹理，并补上 `Layer 3` 最小运行时闭环；其中基础地块层已进一步改为由 `CountyTownMapViewSystem` 直接读取 `L1_hex_tileset.tres` 的 atlas source / region，并继续沿用现有 hex polygon 几何进行无缝底盘绘制，不再停留在“运行时只切 atlas 图片”的过渡状态；世界图当前同样继续复用 `L1_hex_tileset.tres`，但正式主链已收回到 `StrategicMapViewSystem` 按 hex polygon 逐格投 atlas 区域的脚本绘制，`WorldTerrainTileLayer` 仅保留为备用基础设施，不再承担正式底盘，以避免方片排布经过六边形裁切后出现连续白缝；世界图地形贴图的 tile 绑定现支持优先读取 tileset custom data layer `world_terrain_family`，并在缺失时回退到 `WorldTerrainTileLayer` 的 `GDScript` 绑定与默认映射；`L1_hex_tileset.tres` 现又补上多图源 `4x2` 四季图集编排规范：每个 source 一张图、每行一个地块的四季、每张图承载两类地块，后续新增 source 继续沿用同规约；本轮运行时又补上跨全部 source 的 `terrain_family / world_terrain_family + season` 季节索引脚手架，世界图与宗门图会优先按当前季度对应的四季列选图，再回退到历史文件名 / 坐标分支；同时也已为当前 `4` 组 source 补入首批 `terrain_family / world_terrain_family / season` metadata，供新链路直接消费，但现有 family 归类仍属于过渡映射，后续可继续精修；道路 / 河流 / 标签 / 点位 overlay 仍继续由脚本叠加层承接，并且旧版蜂窝背景网格保持关闭；世界图底盘当前改为直接透出 `background_map.png`，L1 纹理面积约为六边形单元的 `0.8x`，并叠加黑灰双墨线轮廓；同时地图显示逻辑已继续收口为“默认平地留白、特色地貌显影”，世界图与山门/二级沙盘都会用稳定排序把空白 hex 与显式地形贴图 hex 的目标比例压到约 `1.618 : 1`，避免地图再次回到满屏贴图；`TownMapGeneratorSystem` 现会生成基础 `Building / ActivityAnchor`，`CountyTownMapViewSystem` 已将 `DrawStructures()` 接入主绘制顺序并启用 Y 排序遮挡；本轮又把世界图与二级地图之间的 L1 底盘分桶统一为共享地形家族规则，确保局部沙盘也能继承一级地图的地貌贴图口径；正式量产素材、独立山体/树木资产与 `Layer 5` 氛围层仍待后续接入 | 继续扩到正式国风地块、立体物件、氛围层与二级地图更高层复用，并补 Godot/F5 走查验证留白比例与重点地貌可读性 | `docs/11_map_asset_production_spec.md`、`docs/14_map_layer_rendering_implementation_plan.md`、`docs/feature-cards/FC-20260312-map-asset-production-spec.md`、`docs/feature-cards/FC-20260312-map-layer-rendering-implementation-plan.md`、`docs/feature-cards/FC-20260312-layer2-freeform-road-river.md`、`docs/feature-cards/FC-20260312-sect-map-layer1-layer2-godot-integration.md`、`docs/feature-cards/FC-20260312-sect-map-layer3-minimal-runtime.md`、`docs/feature-cards/FC-20260312-l1-user-tilemap-hex-tileset.md`、`docs/feature-cards/FC-20260313-l1-runtime-tileset-rendering.md`、`docs/feature-cards/FC-20260313-world-map-tilemaplayer-rendering.md`、`docs/feature-cards/FC-20260315-worldsite-terrain-family-inheritance.md`、`docs/feature-cards/FC-20260316-worldmap-terrain-texture-binding.md`、`docs/feature-cards/FC-20260316-worldmap-tileset-metadata-binding.md`、`docs/feature-cards/FC-20260320-l1-seasonal-atlas-layout.md`、`docs/feature-cards/FC-20260320-l1-seasonal-atlas-runtime-indexing.md`、`docs/feature-cards/FC-20260323-worldmap-golden-ratio-terrain-sparsity.md`、`docs/change-proposals/CP-20260313-l1-runtime-tileset-rendering.md`、`docs/change-proposals/CP-20260315-worldsite-terrain-family-inheritance.md`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.Residents.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/systems/WorldTerrainVisualRules.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/assets/ui/tilemap/L1_hex_tileset.tres`、`CountyIdle/assets/ui/tilemap/world_hex_tile_clip.gdshader` |
| 弟子可视移动 | 代码链仍在，当前运行版停用 | 若要恢复，需按 `01 -> 02 -> 实现` 重新立项 | `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs` |

## 5. 未入场主项（⭕）

| 主项 | 优先级 | 完成定义 | 设计依据 |
| --- | --- | --- | --- |
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





