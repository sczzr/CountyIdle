# 功能卡：三相治宗循环重设计（季度战略相位 + 双层时间制）

## 功能信息

- 功能名：三相治宗循环重设计（季度战略相位 + 双层时间制）
- 优先级：`P1`
- 目标版本：`2026-03` 重设计迭代
- 关联系统：`docs/01_game_design_guide.md`、`docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/SectGovernanceSystem.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/ResearchSystem.cs`、`CountyIdle/scripts/systems/PopulationSystem.cs`

## 目标与用户收益

- 目标：把“并列系统各自增长”的体验重构为“季度战略选择驱动全局”的治理体验，并用双层时间制补足修仙题材应有的岁月感。
- 玩家可感知收益（10 分钟内）：切换战略相位后，日志、资源走向、威胁压力与历练回报会出现清晰倾向；同时玩家能更自然地理解“眼前运转”和“长期成长”是两层不同节奏。

## 实现范围

- 包含：
  - 定义三相节奏：`季度立纲 -> 月度筹划 -> 小时结算`；
  - 定义双层时间制：`细时间运转 + 长时间推进`；
  - 明确战略相位的输入、输出、增益、代价与失衡惩罚；
  - 对齐飞轮与伦理约束，确保“共同建设、共同受益、共同护持”不被破坏；
  - 输出可直接执行的最小实现工单。
- 不包含：
  - 不在本轮重写全部旧系统公式；
  - 不在本轮改动存档结构与历史存档迁移策略；
  - 不在本轮直接完成英雄实体化、护山战与完整转任系统。

## 实现拆解

1. 在开发看板与开发列表立项，确认本功能包依赖与 DoD。
2. 重写总纲，明确玩家决策层级、节奏、双层时间制与核心循环目标。
3. 在系统规格补充战略相位规则，以及双层时间制的定义、边界、正式时间口径与观测指标。
4. 输出首批实现工单，限定最小闭环为 `models -> systems -> core -> UI`。

## 验收标准（可测试）

- [x] `docs/05_feature_inventory.md` 与 `docs/08_development_list.md` 已登记该功能包。
- [x] `docs/01_game_design_guide.md` 已落地“季度立纲 -> 月度筹划 -> 小时结算”三相节奏。
- [x] `docs/02_system_specs.md` 已补输入/输出、公式、边界、失败护栏、双层时间制定义、观测指标与回滚条件。
- [ ] `dotnet build .\Finally.sln` 通过且战略相位最小闭环可在运行时观察到差异反馈。

## 风险与回滚

- 风险：新相位过强会导致单一路线垄断，压缩玩家选择空间；双层时间制若定义不清，会让玩家误解“哪些事在即时发生、哪些事在阶段推进”。
- 回滚方式：保留文档与参数表，运行时整体回退为上一版时间规则与治理路径，并记录失衡原因。
