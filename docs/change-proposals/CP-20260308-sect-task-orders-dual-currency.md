# 改动提案：天衍峰法旨兼容底层与灵石 / 贡献点双轨内务结算

> 后续说明（2026-03-09）：当前玩家可见层已进一步升级为“宗主中枢（方略层去人头化）”；本文记录的是兼容底层和双轨内务边界的首轮提案。

## 提案信息

- 标题：建立“法旨兼容底层”，并引入宗门内外双货币边界
- 日期：2026-03-08
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：
  - 玩家仍在直接加减 `Farmers / Workers / Merchants / Scholars`，体验上更像调抽象人头，而不是宗主管理宗门方向。
  - 宗门内务与宗门外交易目前都只看 `Gold`，缺少修仙宗门里“贡献点”的内部流通层。
  - `JobsPadding` 仍强调岗位加减，而不是任务法旨与经营意图。
- 证据（数据/玩家反馈）：
  - 用户已明确要求“不再有具体的人员分布”，改为“玩家作为宗主发放任务来经营宗门”；
  - 用户明确要求“宗门内部的交易都是通过贡献点和灵石双轨制进行，但宗门外交易只能使用灵石”。

## 改动内容

- 改什么：
  - 新增天衍峰法旨兼容底层，玩家通过治理入口下达法旨；
  - 任务系统把法旨自动解析为 `农务 / 工务 / 商务 / 学务` 四类职司投入，供现有资源、产业、人口系统继续结算；
  - 新增 `ContributionPoints`，作为宗门内部流通货币；
  - 建造、锻器、`天衍峰山门图` 内务调度改用 `贡献点 + 灵石` 双轨成本；
  - 外域贸易任务与外域调度维持“仅灵石”结算；
  - 旧 `JobsPadding` 退为任务摘要与导流入口。
- 不改什么：
  - 不改 `GameState` 旧岗位字段名；
  - 不改小时结算的大系统顺序；
  - 不做剧情式任务链、委托榜、弟子个体 AI。
- 影响系统：
  - `CountyIdle/scripts/models/GameState.cs`
  - `CountyIdle/scripts/models/SectTaskType.cs`
  - `CountyIdle/scripts/systems/SectTaskRules.cs`
  - `CountyIdle/scripts/systems/SectTaskSystem.cs`
  - `CountyIdle/scripts/core/GameLoop.cs`
  - `CountyIdle/scripts/systems/EconomySystem.cs`
  - `CountyIdle/scripts/systems/IndustrySystem.cs`
  - `CountyIdle/scripts/systems/MapOperationalLinkSystem.cs`
  - `CountyIdle/scripts/systems/InventoryRules.cs`
  - `CountyIdle/scripts/systems/MaterialSemanticRules.cs`
  - `CountyIdle/scripts/Main.cs`
  - `CountyIdle/scripts/ui/TaskPanel.cs`
  - `CountyIdle/scripts/ui/MainTaskPanel.cs`
  - `CountyIdle/scenes/ui/TaskPanel.tscn`
  - `CountyIdle/scenes/ui/WorldPanel.tscn`
  - `CountyIdle/scenes/ui/JobsPanel.tscn`

## 预期结果

- 预期提升指标：
  - 玩家不再感觉自己在“硬点人头”，而是在“以宗主身份发任务”；
  - `贡献点` 成为宗门内部建设与调度的明确资源；
  - 宗门内外经济边界更清晰，世界观一致性更高。
- 可接受副作用：
  - 旧 `JobsPadding` 的 `+/-` 按钮会退化为“打开任务面板”的导流交互；
  - 旧岗位字段仍保留在底层，以兼容现有系统与存档；
  - 任务法旨是“抽象工作令”，不是弟子个体 AI 排班。

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 打开治理弹窗并调整法旨
  - 观察 `JobsPadding` 是否变为任务摘要
  - 验证宗门内务操作是否要求 `贡献点 + 灵石`
  - 验证外域相关交易/行动是否仍只看 `灵石`
- 观察周期：本次迭代即时验证
- 成功判定阈值：
  - 任务调整后四类职司汇总会同步变化；
  - 贡献点可增长、可被内务消耗；
  - 外域交易与外务调度不要求贡献点；
  - 构建通过。

## 回滚条件

- 触发条件：
  - 旧存档读档后任务字段异常，导致岗位为零或超编；
  - 贡献点永远无法增长，导致内务系统不可玩；
  - UI 入口失效，玩家无法重新安排治理方向。
- 回滚步骤：
  1. 保留 `ContributionPoints` 字段与任务面板骨架；
  2. 回退 `GameLoop` 中“法旨 -> 职司”的自动同步；
  3. 回退 `IndustrySystem / MapOperationalLinkSystem` 的贡献点消耗；
  4. 恢复 `JobsPadding` 的旧交互，仅在修复后重新推进任务制。

