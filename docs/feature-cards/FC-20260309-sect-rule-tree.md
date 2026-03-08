# 功能卡：门规树（一期：三支门规纲目）

- 功能名：门规树（一期：三支门规纲目）
- 任务 ID：`DL-044`
- 目标（玩家价值）：让宗主可以长期设定宗门常制，而不是只在方向和季度上做短期决策。
- 飞轮环节：产业供养 → 传承研修 → 人口繁衍 → 反哺宗门
- 依赖：`SectRuleTreeRules.cs`、`TaskPanel.cs`、`EconomySystem.cs`、`PopulationSystem.cs`、`IndustrySystem.cs`

## 交付范围

- 在宗主中枢新增 `庶务门规 / 传功门规 / 巡山门规` 三条门规支线；
- 每条支线提供多个可切换的门规节点；
- 门规效果接入收益、人口、威胁与工器锻制；
- “恢复均衡”可恢复三条门规的常制状态。

## 完成标准（DoD）

- [x] 玩家能在宗主中枢切换三条门规支线；
- [x] 门规至少影响一个收益链路与一个非收益链路；
- [x] 门规属于常设章程，不随季度自动失效；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [x] `dotnet build .\Finally.sln` 通过。
