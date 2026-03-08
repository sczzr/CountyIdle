# 改动提案：外域备用视图运营语义统一（V2）

## 提案信息

- 标题：将外域备用视图的调度与状态提示切到修仙外域语义
- 日期：2026-03-08
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：隐藏备用外域视图的标题、城名和地标已修仙化，但运营提示仍保留 `驿路 / 乡里 / 郡路` 等旧郡县行政语义。
- 证据（数据/玩家反馈）：本轮继续沿着“外域备用视图防漏词”推进后，残留旧词已经集中在 `MapOperationalLinkSystem` 的 Prefecture 分支与通用采集日志中。

## 改动内容

- 改什么：
  - 将外域备用视图调度按钮与状态说明统一切到 `灵道 / 聚落 / 抚恤聚落`；
  - 将 `StrategicMapViewSystem` 的 Prefecture 标题 fallback 统一改为 `外域态势`；
  - 将基础采集日志统一改为 `山野采集`。
- 不改什么：
  - 不调整外域调度消耗与收益；
  - 不修改 `MapDirectiveAction` 内部枚举名与 `Prefecture` 模式类型名。
- 影响系统：
  - `CountyIdle/scripts/systems/SectMapSemanticRules.cs`
  - `CountyIdle/scripts/systems/MapOperationalLinkSystem.cs`
  - `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`
  - `CountyIdle/scripts/systems/ResourceSystem.cs`
  - `docs/02_system_specs.md`
  - `docs/05_feature_inventory.md`
  - `docs/08_development_list.md`

## 预期结果

- 预期提升指标：
  - 外域备用视图的运营语义与修仙主题一致；
  - fallback 到备用视图时不再漏出旧行政地图词汇。
- 可接受副作用：
  - 内部 `Prefecture / ReliefVillages / RepairCourierRoad` 仍保留旧代码命名作为兼容层。

## 验证计划

- 验证方式：
  - 搜索 `驿路 / 乡里 / 郡路 / 宗门外域`
  - `dotnet build .\Finally.sln`
- 观察周期：本次迭代即时验证
- 成功判定阈值：
  - 运行中代码与当前规格不再出现上述旧词；
  - 构建通过。

## 回滚条件

- 触发条件：外域备用视图提示不可读、与现有按钮含义不符，或后续决定改为完全数据驱动主题术语。
- 回滚步骤：
  1. 保留外域标题与地标主题；
  2. 将 `MapOperationalLinkSystem` 与 `ResourceSystem` 的语义字串回退；
  3. 保留双地图主入口不变。
