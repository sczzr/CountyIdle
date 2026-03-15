# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十六批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`MainMapOperationalLink`、`WorldPanelVisualFx`

## 目标与用户收益

- 目标：把世界图 / 外域 / 山门地图页底部 `MapDirectiveRow` 的状态字色调与调度按钮强调样式继续从 `C#` 下放到 `GDScript`，让 `MainMapOperationalLink.cs` 只保留地图态势快照、按钮动作与文本绑定。
- 玩家可感知收益（10 分钟内）：地图页底部调度条仍会随当前地图态势切换强调色，但后续微调色调与按钮外观时不需要再进业务脚本改 `C#`。

## 实现范围

- 包含：
  - 在 `WorldPanelVisualFx.gd` 中新增 `MapDirectiveRow` 状态字与按钮强调样式的视觉承接
  - 简化 `MainMapOperationalLink.cs`，移除直接控制 `MapStatusLabel` 表现色调的代码
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改 `MapOperationalLinkSystem` 的态势评分、颜色语义、调度选项、成本收益与日志结果
  - 不改世界图 / 外域 / 山门地图切换逻辑
  - 不改存档结构、小时结算与 `GameState`

## 实现拆解

1. 复查 `Main.cs` 与地图页现状，确认仍在运行链上的纯视觉残留点
2. 将 `MapDirectiveRow` 的状态字色调与调度按钮强调样式统一下放到 `WorldPanelVisualFx.gd`
3. 让 `MainMapOperationalLink.cs` 仅保留快照读取、按钮文本 / tooltip / 禁用态绑定与指令触发
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `WorldPanelVisualFx.gd` 已承接 `MapDirectiveRow` 的状态字色调与按钮强调样式
- [ ] `MainMapOperationalLink.cs` 不再直接使用 `_mapStatusLabel.Modulate` 一类表现层代码
- [ ] 地图态势快照、按钮文案与执行动作仍保留在 `C#`
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 `WorldPanelVisualFx.gd` 中节点路径写错，地图页底部调度条可能丢失强调色或按钮外观异常
- 回滚方式：恢复 `MainMapOperationalLink.cs` 中的直接表现代码，并移除 `WorldPanelVisualFx.gd` 对 `MapDirectiveRow` 的新增样式方法
