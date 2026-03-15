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
| UI 表现层 `GDScript` 辅助首批接入 | 仓储卷开场 / 分页、留影录预览切换、山门 hex hover 高亮已下放至 `GDScript`，权威逻辑仍留在 `C#` | `CountyIdle/scripts/ui/WarehousePanel.cs`、`CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/ui/gd/WarehousePanelTransition.gd`、`CountyIdle/scripts/ui/gd/SavePreviewCrossfade.gd`、`CountyIdle/scripts/map/gd/HexHoverHighlight.gd` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split.md` |
| UI 表现层 `GDScript` 辅助二批接入 | 治宗册开场 / 切页、弟子谱筛选 / 详情切换、中部地图页签 / 二级地图检视脉冲已下放至 `GDScript`，权威逻辑仍留在 `C#` | `CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/MainWorldSitePanel.cs`、`CountyIdle/scripts/ui/gd/TaskPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/WorldPanelVisualFx.gd` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch2.md` |
| UI 表现层 `GDScript` 辅助三批接入 | 设置卷开场 / 录键高亮、宗门组织谱切峰 / 切职司、中部地图顶部标签强调反馈已下放至 `GDScript`，权威逻辑仍留在 `C#` | `CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/gd/SettingsPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/SectOrganizationPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/MainTopTabVisualFx.gd` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch3.md` |

| UI 表现层 `GDScript` 辅助四批接入 | 主界面底栏快捷键 / 倍速 / 存读设按钮的 hover / focus 灯笼强调反馈已下放至 `GDScript`，`Main.cs` 仅保留点击与业务绑定，权威逻辑仍留在 `C#` | `CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/gd/BottomBarLanternFx.gd`、`CountyIdle/scenes/ui/BottomBar.tscn`、`CountyIdle/scenes/ui/figma/BottomBar.tscn` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch4.md` |

| UI 表现层 `GDScript` 辅助五批接入 | `Main.cs` 中剩余全局 hover / focus 灯笼反馈、`OptionButton` popup 表现样式与 hover 锁定已下放至 `GDScript`，`Main.cs` 仅保留一次性绑定转发，权威逻辑仍留在 `C#` | `CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/gd/MainLanternFx.gd`、`CountyIdle/scenes/Main.tscn` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch5.md` |

| UI 表现层 `GDScript` 辅助六批接入 | 留影录右侧详情列的空态 / 预览态过渡已继续下放至 `GDScript`，预览框、详情文本、题名行与按钮行统一由 `SavePreviewCrossfade.gd` 承接，权威逻辑仍留在 `C#` | `CountyIdle/scripts/ui/SaveSlotsPanel.cs`、`CountyIdle/scripts/ui/gd/SavePreviewCrossfade.gd`、`CountyIdle/scenes/ui/SaveSlotsPanel.tscn` | `docs/feature-cards/FC-20260313-ui-gdscript-boundary-split-batch6.md` |

| UI 表现层 `GDScript` 辅助七批接入 | 治宗册与机宜卷的书卷静态样式、字段皮肤与按钮外观已继续下放至 `GDScript`，`TaskPanel.cs` / `SettingsPanel.cs` 仅保留权威逻辑与单向调用边界 | `CountyIdle/scripts/ui/TaskPanel.cs`、`CountyIdle/scripts/ui/SettingsPanel.cs`、`CountyIdle/scripts/ui/gd/TaskPanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/SettingsPanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch7.md` |

| UI 表现层 `GDScript` 辅助八批接入 | 弟子谱与峰令谱的书卷静态样式、筛选控件 / 卡片 / 动态导航外观已继续下放至 `GDScript`，`DisciplePanel.cs` / `SectOrganizationPanel.cs` 仅保留权威逻辑、动态数据与单向调用边界 | `CountyIdle/scripts/ui/DisciplePanel.cs`、`CountyIdle/scripts/ui/SectOrganizationPanel.cs`、`CountyIdle/scripts/ui/gd/DisciplePanelVisualFx.gd`、`CountyIdle/scripts/ui/gd/SectOrganizationPanelVisualFx.gd` | `docs/feature-cards/FC-20260314-ui-gdscript-boundary-split-batch8.md` |

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
| 世界格二级地图分层与入口 | 已支持世界地图任意 hex 点选后在左侧显示详情，并通过进入按钮生成与山门沙盘同形的下一层 hex 沙盘；局部沙盘点选也会复用左侧检视器，形成同族检视闭环 | 继续细化 `宗门 / 野外 / 坊市 / 遗迹` 四类专属模板与真实玩法，以及二级地图内专属交互控件 | `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.WorldCellSelection.cs`、`CountyIdle/scripts/systems/WorldSiteLocalMapGeneratorSystem.cs`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/ui/MainWorldSitePanel.cs`、`CountyIdle/scripts/ui/MainSectTileInspector.cs`、`CountyIdle/scripts/Main.cs` |
| 地图素材分层资产流水线 | 文档二期 + 运行时四期已接入：已新增正式规格文档与 L1-L5 绘制实施方案，当前宗门图已接入 `Layer 2` decal / 连接纹理，并补上 `Layer 3` 最小运行时闭环；其中基础地块层已进一步改为由 `CountyTownMapViewSystem` 直接读取 `L1_hex_tileset.tres` 的 atlas source / region，并继续沿用现有 hex polygon 几何进行无缝底盘绘制，不再停留在“运行时只切 atlas 图片”的过渡状态；世界图当前同样继续复用 `L1_hex_tileset.tres`，但正式主链已收回到 `StrategicMapViewSystem` 按 hex polygon 逐格投 atlas 区域的脚本绘制，`WorldTerrainTileLayer` 仅保留为备用基础设施，不再承担正式底盘，以避免方片排布经过六边形裁切后出现连续白缝；道路 / 河流 / 标签 / 点位 overlay 仍继续由脚本叠加层承接，并且旧版蜂窝背景网格保持关闭；同时默认关闭基础地块边沿描边，避免格线把底盘切碎；`TownMapGeneratorSystem` 现会生成基础 `Building / ActivityAnchor`，`CountyTownMapViewSystem` 已将 `DrawStructures()` 接入主绘制顺序并启用 Y 排序遮挡；正式量产素材、独立山体/树木资产与 `Layer 5` 氛围层仍待后续接入 | 继续扩到正式国风地块、立体物件、氛围层与二级地图更高层复用 | `docs/11_map_asset_production_spec.md`、`docs/14_map_layer_rendering_implementation_plan.md`、`docs/feature-cards/FC-20260312-map-asset-production-spec.md`、`docs/feature-cards/FC-20260312-map-layer-rendering-implementation-plan.md`、`docs/feature-cards/FC-20260312-layer2-freeform-road-river.md`、`docs/feature-cards/FC-20260312-sect-map-layer1-layer2-godot-integration.md`、`docs/feature-cards/FC-20260312-sect-map-layer3-minimal-runtime.md`、`docs/feature-cards/FC-20260312-l1-user-tilemap-hex-tileset.md`、`docs/feature-cards/FC-20260313-l1-runtime-tileset-rendering.md`、`docs/feature-cards/FC-20260313-world-map-tilemaplayer-rendering.md`、`docs/change-proposals/CP-20260313-l1-runtime-tileset-rendering.md`、`CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`、`CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/assets/ui/tilemap/L1_hex_tileset.tres`、`CountyIdle/assets/ui/tilemap/world_hex_tile_clip.gdshader` |
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




