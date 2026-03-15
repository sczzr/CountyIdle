# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 峰令谱表现层 GDScript 第二十四批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`SectOrganizationPanel`

## 目标与用户收益

- 目标：在 `SectOrganizationPanelVisualFx.gd` 已经于 `_ready()` 承接初始主题应用的前提下，继续回收 `SectOrganizationPanel.cs` 中重复触发 `apply_theme_styles` 的初始化残留。
- 玩家可感知收益（10 分钟内）：峰令谱表现不变，但初始化链更清晰，不会再出现同一套主题在 C# 与 GDScript 双端重复应用。

## 实现范围

- 包含：
  - 删除 `SectOrganizationPanel.cs` 中 `_Ready()` 里对 `apply_theme_styles` 的重复调用
  - 保持初始主题应用继续由 `SectOrganizationPanelVisualFx.gd` 的 `_ready()` 承接
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改动态卡片结构、输入绑定、治理入口与峰脉协同逻辑
  - 不改 `SectOrganizationPanelVisualFx.gd` 的主题语义与表现参数
  - 不改 `GameState`、存档结构与小时结算

## 实现拆解

1. 复查峰令谱初始主题应用链，确认 `apply_theme_styles` 已由 `GDScript` `_ready()` 承接
2. 删除 `SectOrganizationPanel.cs` 中重复的初始化调用
3. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `SectOrganizationPanel.cs` 不再在 `_Ready()` 中重复调用 `apply_theme_styles`
- [ ] 初始主题应用继续由 `SectOrganizationPanelVisualFx.gd` 的 `_ready()` 单点承接
- [ ] 峰令谱动态卡片、点击、tooltip、选中态与治理入口逻辑保持不变
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若后续 `VisualFx` 节点加载顺序变化，可能导致主题未按预期初始化
- 回滚方式：恢复 `SectOrganizationPanel.cs` 中本批移除的 `apply_theme_styles` 调用即可
