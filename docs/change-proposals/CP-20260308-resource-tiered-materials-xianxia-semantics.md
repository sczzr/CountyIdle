# 改动提案（Change Proposal）

## 提案信息

- 标题：资源系统分层扩展（V1 六期：修仙材料语义化）
- 日期：2026-03-08
- 提案人：System Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：
  - `DL-023` 已完成材料链分层与闭环，但玩家可见命名仍偏通用经营素材语义，修仙宗门题材感知不够强。
  - 仓储、日志、主界面提示、外域原料标签中的材料名称分散定义，后续继续统一世界观时维护成本偏高。
  - 部分原料与加工品虽然已经处在正确层级上，但“材料是什么、在修仙世界里意味着什么”还不够鲜明。
- 证据（数据/玩家反馈）：
  - 本轮需求明确提出“材料也需要偏向于修仙背景”。
  - 当前仓储与日志中仍大量使用 `木料 / 石料 / 药材 / 铁矿 / 工业部件` 等通用命名。

## 改动内容

- 改什么：
  - 新增统一的玩家可见材料语义映射，集中维护显示名称与说明
  - 将 `T0/T1` 材料、民生产物、制造产物改为修仙风格命名
  - 将仓储面板、主界面提示、小时结算日志、人口缺口提示、外域原料来源标签统一改用该映射
  - 在系统规格中补充“技术字段名 vs 玩家可见修仙名”的说明
- 不改什么：
  - 不修改 `GameState` 字段名与存档 JSON 结构
  - 不改原料产量、加工配方、人口消耗、工具公式
  - 不提前开放 `T2/T3` 材料或新增法宝/丹药玩法
- 影响系统：
  - `docs/02_system_specs.md`
  - `docs/05_feature_inventory.md`
  - `docs/08_development_list.md`
  - `CountyIdle/scripts/systems/MaterialSemanticRules.cs`
  - `CountyIdle/scripts/systems/MaterialRules.cs`
  - `CountyIdle/scripts/systems/ResourceSystem.cs`
  - `CountyIdle/scripts/systems/IndustrySystem.cs`
  - `CountyIdle/scripts/systems/PopulationSystem.cs`
  - `CountyIdle/scripts/ui/WarehousePanel.cs`
  - `CountyIdle/scripts/Main.cs`
  - `CountyIdle/scripts/systems/PrefectureMapGeneratorSystem.cs`

## 预期结果

- 预期提升指标：
  - 玩家在仓储、日志、地图和主界面看到一致的修仙材料名
  - 同一材料的文案维护入口从“多处散落”收敛为“单点映射”
  - `DL-023` 的材料链在世界观层面更贴近“宗门经营 + 外域探索”
- 可接受副作用：
  - 玩家需要短时间适应新命名
  - 个别名称可能在后续试玩中继续微调

## 验证计划

- 验证方式：
  - 审查仓储面板、顶部资源提示、小时日志、外域原料标签是否统一改名
  - 审查代码是否通过集中语义映射访问玩家可见材料名
  - 执行 `dotnet build .\Finally.sln`
- 观察周期：本轮实现后即时验证 + 下一轮 `Godot` 运行烟测时复查
- 成功判定阈值：
  - 关键材料命名分散点收敛到集中入口
  - 玩家可见 `T0/T1` 材料链均切换到修仙语义
  - 不破坏现有构建与存档兼容

## 回滚条件

- 触发条件：
  - 命名导致关键材料功能难以辨认
  - 玩家可见区域仍出现明显混用或歧义
  - 构建或运行因语义层改动出现异常
- 回滚步骤：
  1. 保留集中语义映射入口
  2. 仅回滚映射值与显示文案，不回退资源链逻辑
  3. 必要时改为“修仙名 + 通用名”混合显示过渡
