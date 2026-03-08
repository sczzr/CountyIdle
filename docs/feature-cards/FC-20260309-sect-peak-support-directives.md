# 功能卡：峰脉协同法旨（JobsList 三期）

- 功能名：峰脉协同法旨（JobsList 三期）
- 任务 ID：`DL-041`
- 目标（玩家价值）：让玩家在浏览九峰详情时，能直接把某一峰立为“本季协同峰”，从而真实影响宗门经营结果。
- 飞轮环节：产业供养 → 传承研修 → 人口繁衍 → 反哺宗门
- 依赖：`docs/09_xianxia_sect_setting.md`、`D:/Files/Novel/asset/qidian/cangxuan/浮云宗-天衍峰.md`、`SectOrganizationRules.cs`、`GameLoop.cs`、`EconomySystem.cs`、`PopulationSystem.cs`、`IndustrySystem.cs`

## 交付范围

- 在峰脉详情区新增“设为本季协同峰 / 恢复均衡”操作；
- 引入 `诸峰均衡 + 九峰协同` 的状态持久化；
- 将协同效果接到小时结算、门人生息与工器锻制；
- 在摘要与详情区露出当前协同峰状态。

## 完成标准（DoD）

- [x] 玩家能在 `JobsList` 峰脉详情区直接下发协同峰令；
- [x] 协同峰效果会影响 `食物 / 灵石 / 贡献 / 研修 / 人口增长 / 民心 / 威胁 / 工器锻制` 的至少一部分；
- [x] 当前协同峰状态能在玩家界面直接看到；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [x] `dotnet build .\Finally.sln` 通过。
