# CountyIdle 功能实现状态总表（开发看板）

> 目标：让开发时能快速判断“哪些功能已实现、哪些部分实现、哪些是 TODO”。

## 1) 状态图例

- `✅ 已实现`：代码已接入主流程，可在当前版本直接使用
- `🟡 部分实现`：有代码或 UI 骨架，但尚未形成完整闭环
- `⭕ 未实现（TODO）`：已在设计/里程碑中定义，但当前版本未落地

## 1.1 核心玩法对齐矩阵（目标 vs 当前）

| 核心玩法目标 | 当前状态 | 说明 |
| --- | --- | --- |
| 人口繁衍 | ✅ | 已有人口增长/饥荒/幸福联动 |
| 产业涌现（资源生产消耗） | ✅ | 已有 Industry + Economy 结算 |
| 科技涌现（科技树） | 🟡 | 已有线性 T1/T2/T3 突破，尚非分支科技树 |
| 职业分化 | ✅ | 已有四类岗位与容量约束 |
| 职业转职 | ⭕ | 尚无完整转职规则与流程 |
| 优秀后代概率受父母影响 | ⭕ | 当前繁育未使用父母质量关联 |
| 英雄单位冒险 | 🟡 | 当前以精英人口池参与探险，尚无英雄实体系统 |
| 装备打造系统 | ⭕ | 当前仅有探险掉落与工具制作，未有装备打造链路 |
| 人民与玩家共同建设郡县 | 🟡 | 已有建筑扩建与治理反馈，居民协作行为仍需深化 |
| 郡县防护人民并带来富足 | 🟡 | 有威胁/袭扰/经济反馈，但守城闭环尚未完成 |

## 2) 已实现功能（✅）

| 功能 | 状态 | 代码入口 | 对应文档 |
| --- | --- | --- | --- |
| 主循环时间推进（1s=1分钟，60分钟结算） | ✅ | `CountyIdle/scripts/core/GameLoop.cs` | `docs/02_system_specs.md`（全局基线、小时结算顺序） |
| 人口增长/饥荒/幸福联动 | ✅ | `CountyIdle/scripts/systems/PopulationSystem.cs` | `docs/02_system_specs.md`（人口系统） |
| 岗位容量、建筑扩建、制工具 | ✅ | `CountyIdle/scripts/systems/IndustrySystem.cs`、`IndustryRules.cs` | `docs/02_system_specs.md`（产业与岗位系统） |
| 经济结算（资源、薪资、惩罚） | ✅ | `CountyIdle/scripts/systems/EconomySystem.cs` | `docs/02_system_specs.md`（经济系统） |
| 科研突破 T1/T2/T3 | ✅ | `CountyIdle/scripts/systems/ResearchSystem.cs` | `docs/02_system_specs.md`（科研突破系统） |
| 精英繁育与突变 | ✅ | `CountyIdle/scripts/systems/BreedingSystem.cs` | `docs/02_system_specs.md`（繁育系统） |
| 探险战斗与层数推进 | ✅ | `CountyIdle/scripts/systems/CombatSystem.cs` | `docs/02_system_specs.md`（探险与战斗系统） |
| 装备品质/词条掉落 | ✅ | `CountyIdle/scripts/systems/EquipmentSystem.cs` | `docs/02_system_specs.md`（装备掉落系统） |
| 郡县动态事件（商路/学宫/袭扰） | ✅ | `CountyIdle/scripts/systems/CountyEventSystem.cs` | `docs/02_system_specs.md`（郡县动态事件） |
| 县城 2.5D 地图生成 + 缩放 | ✅ | `CountyIdle/scripts/systems/TownMapGeneratorSystem.cs`、`CountyTownMapViewSystem.cs` | `docs/02_system_specs.md`（县城 2.5D 地图） |
| 世界/郡图战略视图 + 缩放 | ✅ | `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`、`Main.cs` | `docs/02_system_specs.md`（战略地图视图与缩放） |
| 地图页签切换与缩放按钮 | ✅ | `CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/WorldPanel.tscn` | `docs/02_system_specs.md`（主界面与客户端设置） |
| 存档/读档 | ✅ | `CountyIdle/scripts/core/SaveSystem.cs` | `docs/02_system_specs.md`（存档与兼容） |
| 客户端设置（语言/分辨率/字体/音量） | ✅ | `CountyIdle/scripts/core/ClientSettingsSystem.cs`、`CountyIdle/scripts/ui/SettingsPanel.cs` | `docs/02_system_specs.md`（主界面与客户端设置） |
| 双布局（Legacy/Figma） | ✅ | `CountyIdle/scripts/Main.cs` | `docs/02_system_specs.md`（主界面与客户端设置） |

## 3) 部分实现（🟡）

| 功能 | 当前状态 | 差距（下一步） | 关联文件 |
| --- | --- | --- | --- |
| 科技树系统 | 当前为线性突破（T1/T2/T3） | 扩展为分支科技树（解锁条件、路径选择、互斥/协同） | `CountyIdle/scripts/systems/ResearchSystem.cs` |
| 战略地图数据驱动配置 | `StrategicMapConfig*` 已存在，但未接入渲染主链路 | 将 `StrategicMapViewSystem` 切到配置驱动并接入 `data/strategic_maps.json` | `CountyIdle/scripts/models/StrategicMapConfig.cs`、`CountyIdle/scripts/systems/StrategicMapConfigSystem.cs` |
| 野外探险地图页 | 页签与占位面板已存在 | 接入路线风险、节点事件、战斗遭遇结算 | `CountyIdle/scenes/ui/WorldPanel.tscn`（`ExpeditionMapView`） |
| 事件面板/统计报表 | 面板容器与部分按钮接入已完成 | 将静态文案改为实时数据字段与历史统计 | `CountyIdle/scenes/ui/WorldPanel.tscn`、`CountyIdle/scripts/Main.cs` |
| 岗位优先级 | 按钮与日志已实现 | 把优先级接入岗位自动调配策略 | `CountyIdle/scripts/Main.cs` |
| 静态数据配置（buildings/jobs/items/monsters/traits） | 数据文件存在但系统计算基本未消费 | 建立配置加载层并替换硬编码常量 | `CountyIdle/data/*.json` |
| 英雄单位系统 | 探险已存在但为人口池抽象 | 增加英雄实体、职业成长、受装备与科技影响的个体冒险 | `CountyIdle/scripts/systems/CombatSystem.cs` |

## 4) 未实现（TODO）

| TODO | 优先级 | 目标验收（完成定义） | 设计来源 |
| --- | --- | --- | --- |
| Boss 机制与词条克制（探险后期） | P1 | 进入高层探险时出现 Boss 回合逻辑，掉落与克制关系可观测 | `docs/01_game_design_guide.md`（M3） |
| 攻城战/守城压力闭环 | P1 | 威胁高位触发攻城，结果反哺城防与人口/资源状态 | `docs/01_game_design_guide.md`（M3） |
| 精英繁育深度化（父母属性/近亲惩罚） | P1 | 后代生成受父母与血缘规则影响，日志可解释 | `docs/01_game_design_guide.md`（核心飞轮） |
| 职业转职系统 | P1 | 人口在条件满足时可跨职业转职，成本与收益可观测 | `docs/01_game_design_guide.md`（核心飞轮） |
| 装备打造系统 | P1 | 增加材料->打造->品质结果链路，与探险与产业互相影响 | `docs/01_game_design_guide.md`（核心飞轮） |
| 科技树分支化 | P1 | 形成分支科技路径，不同选择带来不同发展方向 | `docs/01_game_design_guide.md`（M2） |
| 地图与经营状态联动 | P2 | 建筑/地块变化影响地图表现，地图事件反向影响经营指标 | `docs/02_system_specs.md`（县城地图扩展方向） |
| 公式全面配置化（去硬编码） | P2 | 关键系统公式由配置文件驱动，支持热调整/版本对比 | `docs/03_change_management.md` |

## 5) 使用方式（开发前 2 分钟）

1. 先看本文件第 2/3/4 节，确认目标属于 `✅/🟡/⭕` 哪类。
2. 再跳到 `docs/02_system_specs.md` 查对应系统规则。
3. 若本文件没有该需求，先登记到 `docs/08_development_list.md` 再开发。
4. 开始开发前补功能卡；若是机制/平衡改动，补提案与日志。
5. 每次开发后更新本文件状态，确保“实现状态与代码一致”。
