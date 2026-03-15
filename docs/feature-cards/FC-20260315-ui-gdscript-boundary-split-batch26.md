# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 世界点位页表现层 GDScript 第二十六批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`MainWorldSitePanel`

## 目标与用户收益

- 目标：在 `WorldPanelVisualFx.gd` 已经于 `_ready()` 承接 world-site 主题应用的前提下，继续回收 `MainWorldSitePanel.cs` 中重复触发 `apply_world_site_theme_styles` 的初始化残留。
- 玩家可感知收益（10 分钟内）：二级地图页 world-site 面板外观保持不变，但初始化链更清晰，不再由 `C#` 与 `GDScript` 双端重复应用同一套主题。

## 实现范围

- 包含：
  - 删除 `MainWorldSitePanel.cs` 中绑定 world-site 面板节点时对 `apply_world_site_theme_styles` 的重复触发
  - 保持 world-site 初始主题继续由 `WorldPanelVisualFx.gd` 的 `_ready()` 单点承接
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改 world-site 数据绑定、按钮行为、sandbox 注入、secondary map 入口与局部检视逻辑
  - 不改 `WorldPanelVisualFx.gd` 的样式语义与表现参数
  - 不改 `GameState`、存档结构与小时结算

## 实现拆解

1. 复查 world-site 面板初始化链，确认 `WorldPanelVisualFx.gd` `_ready()` 已单点承接主题应用
2. 删除 `MainWorldSitePanel.cs` 中重复的 `apply_world_site_theme_styles` 调用
3. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `MainWorldSitePanel.cs` 不再重复触发 `apply_world_site_theme_styles`
- [ ] world-site 初始主题继续由 `WorldPanelVisualFx.gd` 的 `_ready()` 单点承接
- [ ] world-site 数据、按钮、sandbox 注入与检视反馈逻辑保持不变
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 `WorldPanelVisualFx` 节点挂接顺序或路径未来变化，world-site 初始主题可能退化
- 回滚方式：恢复 `MainWorldSitePanel.cs` 中本批移除的 `apply_world_site_theme_styles` 调用即可
