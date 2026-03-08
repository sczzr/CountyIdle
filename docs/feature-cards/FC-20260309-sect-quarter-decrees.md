# 功能卡：季度法令（宗主治理三期先行）

- 功能名：季度法令（宗主治理三期先行）
- 任务 ID：`DL-042`
- 目标（玩家价值）：让宗主可以按季度颁布专项法令，在短周期内强力牵引宗门经营方向。
- 飞轮环节：产业供养 → 传承研修 → 人口繁衍 → 反哺宗门
- 依赖：`SectGovernanceRules.cs`、`SectGovernanceSystem.cs`、`GameCalendarSystem.cs`、`TaskPanel.cs`、`EconomySystem.cs`、`PopulationSystem.cs`、`IndustrySystem.cs`

## 交付范围

- 在宗主中枢新增“季度法令”一层；
- 提供 `开库赈济 / 开坛季讲 / 护山检阅 / 坊市开榷 / 百工会炼` 等法令；
- 把法令效果接到小时收益、人口链路与工器锻制；
- 季度轮换时自动失效，等待新令。

## 完成标准（DoD）

- [x] 玩家能在宗主中枢切换季度法令；
- [x] 季度法令至少影响一个收益链路和一个非收益链路；
- [x] 季度轮换时会自动清空过期法令；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [x] `dotnet build .\Finally.sln` 通过。
