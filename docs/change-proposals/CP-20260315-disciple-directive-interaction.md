# 变更提案（Change Proposal）

## 提案信息

- 变更名：弟子谱交互指令接入（一期）
- 提案ID：`CP-20260315-disciple-directive-interaction`
- 变更级别：`L2`
- 关联功能卡：`docs/feature-cards/FC-20260315-disciple-directive-interaction.md`

## 变更缘由

- 当前弟子谱已能显示个体信息，但仍停留在“看卷”层，用户明确提出希望这些数据能真正和游戏交互起来。
- 现有版本尚未进入正式逐人排班，因此需要一条不破坏挂机主链、但能把个体弟子批注接入系统结算的轻量路径。

## 现状问题

- 弟子 `身份 / 功法 / 技艺 / 属性` 目前主要服务展示，不直接影响小时结算。
- 玩家无法在弟子层面表达“此人想往哪条线培养”。
- 治宗中枢与历练系统只能按聚合人口工作，缺少一层“个体重点名册”的缓冲带。

## 方案裁定

- 在 `GameState` 中新增 `DiscipleDirectives`，以 `弟子ID -> 指令标签` 的形式保存最小交互状态。
- 一期只开放三档：
  - `常制观察`
  - `外务候补`
  - `执事培养`
- `外务候补` 接入 `CombatSystem`，以“重点候补前列弟子”折算为额外队伍战力与外务回流修正。
- `执事培养` 接入 `EconomySystem`，以“重点培养执事前列弟子”折算为贡献回流修正。
- 弟子谱 UI 必须显式说明当前批注与其即时效果摘要，避免隐藏规则。

## 影响范围

- `CountyIdle/scripts/models/GameState.cs`
- `CountyIdle/scripts/models/DiscipleDirectiveType.cs`
- `CountyIdle/scripts/models/DiscipleProfile.cs`
- `CountyIdle/scripts/systems/DiscipleDirectiveRules.cs`
- `CountyIdle/scripts/systems/DiscipleDirectiveSystem.cs`
- `CountyIdle/scripts/systems/DiscipleRosterSystem.cs`
- `CountyIdle/scripts/systems/CombatSystem.cs`
- `CountyIdle/scripts/systems/EconomySystem.cs`
- `CountyIdle/scripts/core/GameLoop.cs`
- `CountyIdle/scripts/ui/DisciplePanel.cs`
- `CountyIdle/scripts/ui/MainDisciplePanel.cs`
- `CountyIdle/scenes/ui/DisciplePanel.tscn`

## 验收指标

- 弟子交互指令能随 `Clone()` 与 JSON/SQLite 存档正常读写。
- 探险日志可解释 `外务候补` 的接入结果。
- 贡献回流会受 `执事培养` 的折算影响。
- 弟子谱详情必须能说明当前交互批注及其即时效果摘要。
- `dotnet build .\Finally.sln` 通过。

## 回滚路径

- 若个体批注公式不稳，可先将 `CombatSystem` / `EconomySystem` 中的加成链断开。
- 保留 `DiscipleDirectiveType` 与 `DiscipleDirectives` 存档字段，避免回滚时破坏旧存档可读性。
- 若 UI 交互造成混乱，可只回退弟子谱按钮与提示，保留规则层空载待后续重接。
