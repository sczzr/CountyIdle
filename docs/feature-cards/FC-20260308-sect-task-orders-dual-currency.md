# 功能卡：天衍峰法旨兼容底层与双轨内务结算

> 后续说明（2026-03-09）：当前玩家可见层已继续推进为“宗主中枢（方略层去人头化）”；本文主要记录其兼容底层与双轨内务的首轮落地。

## 功能信息

- 功能名：天衍峰法旨兼容底层与双轨内务结算
- 优先级：`P0`
- 目标版本：当前迭代（2026-03-08）
- 关联系统：`docs/01_game_design_guide.md`、`docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`、`CountyIdle/scripts/models/GameState.cs`、`CountyIdle/scripts/core/GameLoop.cs`、`CountyIdle/scripts/systems/SectTaskRules.cs`、`CountyIdle/scripts/systems/SectTaskSystem.cs`、`CountyIdle/scripts/systems/EconomySystem.cs`、`CountyIdle/scripts/systems/IndustrySystem.cs`、`CountyIdle/scripts/systems/MapOperationalLinkSystem.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scripts/ui/TaskPanel.cs`

## 目标与用户收益

- 目标：
  - 取消玩家对 `Farmers / Workers / Merchants / Scholars` 的直接加减；
  - 改为由宗主通过治理入口下达法旨，由系统自动折算为职司投入；
  - 引入 `贡献点 + 灵石` 双轨结算，明确宗门内务与宗门外交易的货币边界。
- 玩家可感知收益（10 分钟内）：
  - 玩家打开主界面后，能通过治理入口调整阵材圃、营造、推演、坊务、外贸等方向；
  - 能在顶栏与任务面板中同时看到 `灵石 / 贡献点`；
  - 能明确区分“宗门内部事项要走贡献点 + 灵石”“宗门外交易只能用灵石”。

## 实现范围

- 包含：
  - 新增天衍峰法旨兼容底层与任务面板；
  - `GameState` 新增 `ContributionPoints / TaskOrderUnits / TaskResolvedWorkers`；
  - 小时结算前后同步“法旨 -> 四类职司”；
  - 内务建造、锻器、天衍峰山门图调度改为 `贡献点 + 灵石` 双轨消耗；
  - 外域贸易维持“仅灵石”结算；
  - `JobsPadding` 退为“职司摘要 + 治理入口”。
- 不包含：
  - 不做英雄实体化、任务链剧情、任务失败惩罚树；
  - 不改 `County / Prefecture / CountyTown` 等历史代码命名；
  - 不做 Figma 主布局完整重构（Legacy 为当前主入口）。

## 实现拆解

1. 在 `08_development_list` 登记完整功能包，并在 `02_system_specs` 明确任务制与双货币规则。
2. 新增 `SectTaskType / SectTaskRules / SectTaskSystem`，落地法旨解析与旧岗位兼容层。
3. 扩展经济、产业、地图调度系统，接入 `贡献点 + 灵石` 双轨消耗/产出。
4. 新增治理弹窗，重写 `JobsPadding` 为摘要视图与入口。
5. 回写看板与平衡日志，完成构建验证。

## 验收标准（可测试）

- [x] 玩家不再直接调整 `Farmers / Workers / Merchants / Scholars`，而是通过治理入口经营宗门。
- [x] 宗门内务建造 / 锻器 / 天衍峰山门图内务调度改为 `贡献点 + 灵石` 双轨消耗。
- [x] 宗门外交易只使用 `灵石`，不再要求 `贡献点`。
- [x] `JobsPadding` 显示任务折算后的职司摘要，并可打开治理面板。
- [x] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - 旧存档若没有任务字段，需要靠默认法旨从旧岗位值推导；
  - 任务分辨率过粗可能让玩家误判“法旨数”与“实际门人投入”的关系；
  - 双轨货币若产出/消耗不平衡，会导致内务操作过紧或过松。
- 回滚方式：
  - 保留 `ContributionPoints` 字段与治理面板框架；
  - 回退 `GameLoop / Economy / Industry / MapOperationalLink` 的任务同步与双货币消耗逻辑；
  - `JobsPadding` 可回退为旧岗位面板，但应保留文档与世界观更新记录。


