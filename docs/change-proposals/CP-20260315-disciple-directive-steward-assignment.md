# 变更提案（Change Proposal）

## 提案信息

- 变更名：弟子谱交互指令接入（第三期：执事候选池自动补位）
- 提案ID：`CP-20260315-disciple-directive-steward-assignment`
- 变更级别：`L2`
- 关联功能卡：`docs/feature-cards/FC-20260315-disciple-directive-steward-assignment.md`

## 变更缘由

- 二期已经把 `执事培养` 接到治宗执行效率，但仍然停留在“有补位、看不到是谁在补”的抽象层。
- 为了继续把弟子个体与治宗交互绑紧，需要增加一层“候选池自动补位”的可解释映射。

## 现状问题

- 玩家知道执事培养生效了，但不知道当前到底是谁在负责哪条庶务。
- 弟子谱与治宗册之间仍缺一层更具体的对应关系。

## 方案裁定

- 从 `执事培养` 名册中自动筛选候选弟子。
- 按当前内务条目的侧重、优先级与弟子属性，自动为条目匹配最合适的补位执事。
- 治宗册条目与弟子谱详情必须能读取这层自动分配结果。
- 仍坚持“玩家定方向、系统自动排人”的治理边界，不改为逐条手动点将。

## 影响范围

- `CountyIdle/scripts/systems/DiscipleDirectiveRules.cs`
- `CountyIdle/scripts/systems/SectTaskRules.cs`
- `CountyIdle/scripts/systems/EconomySystem.cs`
- `CountyIdle/scripts/ui/DisciplePanel.cs`
- `docs/02_system_specs.md`
- `docs/05_feature_inventory.md`
- `docs/08_development_list.md`

## 验收指标

- 内务类条目可显示自动补位执事。
- 弟子谱能显示该弟子当前是否正在代行某条庶务。
- 外务条目不读执事候选池。
- `dotnet build .\Finally.sln` 通过。

## 回滚路径

- 若自动补位规则不稳定，可先撤回按人指派结果展示。
- 保留二期的执行效率修正与一期的弟子交互指令基础结构。
