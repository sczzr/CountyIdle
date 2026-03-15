# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十五批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`MainWorldSitePanel`、`WorldPanel`

## 目标与用户收益

- 目标：把二级地图页中 `GeneratedSecondarySandboxView` 的壳层结构从 `C#` 运行时动态构建回收到 `WorldPanel.tscn`，让 `MainWorldSitePanel.cs` 更专注于 world-site 数据绑定与入口行为，而不再负责视觉层节点装配。
- 玩家可感知收益（10 分钟内）：二级地图页的局部沙盘壳层外观与交互保持一致，但后续调整 shell 结构时不需要再进 `C#` 代码动态拼节点。

## 实现范围

- 包含：
  - 在 `WorldPanel.tscn` 中正式补上 `GeneratedSecondarySandboxView` 及其必要子节点
  - 简化 `MainWorldSitePanel.cs` 中的 `EnsureWorldSiteSandboxMapView()`，移除运行时壳层动态构建
  - 继续通过 `WorldPanelVisualFx.gd` 承接 shell 样式
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改 world-site 数据生成、二级地图生成规则、按钮行为与入口文案
  - 不改 `WorldSiteLocalMapGeneratorSystem`、`GameState`、存档结构与小时结算

## 实现拆解

1. 审查 `WorldPanel.tscn` 与 `MainWorldSitePanel.cs` 中 sandbox shell 的当前职责分布
2. 将 `GeneratedSecondarySandboxView` 的静态结构与必要节点补回场景
3. 让 `MainWorldSitePanel.cs` 仅保留节点查找、数据绑定和 `C# -> GDScript` 调用
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `WorldPanel.tscn` 已正式包含 `GeneratedSecondarySandboxView`、`MapHintLabel`、`RegenerateButton`
- [ ] `MainWorldSitePanel.cs` 不再运行时 new sandbox shell 节点并插入布局
- [ ] 二级地图页仍可对 sandbox 调用 `SetExternalMap` / `ClearExternalMap`
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若场景节点路径或排序位置不对，二级地图页的局部沙盘可能不显示或插入位置异常
- 回滚方式：恢复 `MainWorldSitePanel.cs` 中的运行时构建逻辑，并移除 `WorldPanel.tscn` 中新增的 sandbox 节点
