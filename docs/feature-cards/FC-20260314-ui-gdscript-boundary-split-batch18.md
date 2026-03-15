# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十八批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`Main`

## 目标与用户收益

- 目标：回收清理 `Main.cs` 中已不再接入现行布局链路的旧 `job-row / priority` 视觉 helper、未接线字典与选中样式残留，让主界面脚本更聚焦于当前仍在运行的峰脉摘要、地图调度与面板入口逻辑。
- 玩家可感知收益（10 分钟内）：运行表现不变，但后续继续拆分主界面表现层时，不再被这些历史残留误导为“仍在活跃链上的 C# 表现逻辑”。

## 实现范围

- 包含：
  - 清理 `Main.cs` 中旧 `job-row` 选中样式 helper 与 priority helper
  - 清理未接线的 `_jobPriorityButtons` / `_jobRowBaseStyles` 等历史残留字段
  - 保留并继续使用当前峰脉摘要、地图调度与面板入口主链
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改峰脉摘要文本、地图调度、世界图 / 山门图逻辑
  - 不改 `GameState`、治理规则、存档结构与小时结算
  - 不重做旧 `job-row` UI，也不恢复历史岗位交互链路

## 实现拆解

1. 复查 `Main.cs` 中 `job-row / priority` 相关代码是否仍有现行调用链
2. 清理未接线的历史视觉 helper、字典与选中样式残留
3. 保证当前峰脉摘要与地图调度主链不受影响
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `Main.cs` 中已不再接线的 `_jobPriorityButtons` / `_jobRowBaseStyles` 及对应 helper 已清理
- [ ] `Main.cs` 中旧 `job-row` 选中样式与 priority 文本应用不再残留
- [ ] 主界面继续仅保留现行峰脉摘要、地图调度、面板入口与实际运行链逻辑
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：如果误判了某段旧岗位交互代码仍在运行链中，清理后可能导致历史布局分支下的对应交互失效
- 回滚方式：恢复 `Main.cs` 中本批删除的旧 `job-row / priority` 相关字段与 helper
