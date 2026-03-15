# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 多卷册表现层 GDScript 第二十五批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`DisciplePanel`、`SaveSlotsPanel`、`SettingsPanel`、`TaskPanel`、`WarehousePanel`

## 目标与用户收益

- 目标：在多个卷册面板都已由各自 `VisualFx.gd` 的 `_ready()` 承接初始主题应用后，继续回收面板 `C#` 脚本里重复触发 `apply_theme_styles` 的残留。
- 玩家可感知收益（10 分钟内）：卷册外观保持不变，但初始化链更清晰，不再由 `C#` 与 `GDScript` 双端重复应用同一套主题。

## 实现范围

- 包含：
  - 删除 `DisciplePanel.cs`、`SaveSlotsPanel.cs`、`SettingsPanel.cs`、`TaskPanel.cs`、`WarehousePanel.cs` 中 `_Ready()` 对 `apply_theme_styles` 的重复触发
  - 保持初始主题继续由对应 `VisualFx.gd` 的 `_ready()` 单点承接
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改任何卷册的业务数据、点击输入、按钮行为、筛选排序或存档逻辑
  - 不改 `VisualFx.gd` 里的样式语义与表现参数
  - 不改 `GameState`、存档结构与小时结算

## 实现拆解

1. 复查已接入 `VisualFx.gd` 的卷册面板，确认 `_ready()` 已单点承接主题应用
2. 删除各自 `C#` 面板中重复的 `apply_theme_styles` 调用
3. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] 五个卷册面板的 `C#` `_Ready()` 不再重复触发 `apply_theme_styles`
- [ ] 初始主题继续由对应 `VisualFx.gd` 的 `_ready()` 单点承接
- [ ] 弟子谱、留影录、机宜卷、治宗册与仓储卷的数据、点击、tooltip 与业务逻辑保持不变
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若某个 `VisualFx` 节点加载顺序或挂接路径发生漂移，个别卷册初始主题可能退化
- 回滚方式：恢复各面板 `C#` `_Ready()` 中本批移除的 `apply_theme_styles` 调用即可
