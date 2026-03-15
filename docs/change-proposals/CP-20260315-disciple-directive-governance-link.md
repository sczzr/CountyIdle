# 变更提案（Change Proposal）

## 提案信息

- 变更名：弟子谱交互指令接入（第二期：治宗落实联动）
- 提案ID：`CP-20260315-disciple-directive-governance-link`
- 变更级别：`L2`
- 关联功能卡：`docs/feature-cards/FC-20260315-disciple-directive-governance-link.md`

## 变更缘由

- 一期已让弟子谱的 `外务候补 / 执事培养` 接入探险与贡献回流，但 `执事培养` 仍缺少直接的治宗侧反馈。
- 用户要求继续推进“弟子数据如何与游戏交互”，当前最自然的第二刀就是把它接到治宗落实链。

## 现状问题

- `执事培养` 当前主要表现为贡献回流倍率，和治宗册里的“执事落实”文案关联不够强。
- 玩家虽然能在弟子谱点定培养对象，但在治宗册中看不到对应的执行反馈。

## 方案裁定

- 为 `执事培养` 增加一条“内务执行效率”修正，作为内务类条目的额外效率乘区。
- `SectTaskRules` 的执行摘要、职司摘要与条目详情中，必须显式说明当前是否存在执事培养补位。
- `EconomySystem` 在计算内务类条目收益时，按该效率修正放大有效产出。
- `OuterTrade` 等外务专属条目不吃此修正，避免语义混淆。

## 影响范围

- `CountyIdle/scripts/systems/DiscipleDirectiveRules.cs`
- `CountyIdle/scripts/systems/SectTaskRules.cs`
- `CountyIdle/scripts/systems/EconomySystem.cs`
- `CountyIdle/scripts/ui/TaskPanel.cs`
- `docs/02_system_specs.md`
- `docs/05_feature_inventory.md`
- `docs/08_development_list.md`

## 验收指标

- 治宗册摘要能体现 `执事培养` 的执行补位效果。
- 内务类条目收益受到修正，外务类条目不受影响。
- 弟子谱、治宗册、小时结算的语义保持一致。
- `dotnet build .\Finally.sln` 通过。

## 回滚路径

- 若二期修正过强，可先撤回 `SectTaskRules` / `EconomySystem` 的执行效率接入。
- 保留一期 `DiscipleDirectives` 存档结构与弟子谱批注入口，避免破坏旧存档。
