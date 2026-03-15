# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第七批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`TaskPanel`、`SettingsPanel`

## 目标与用户收益

- 目标：继续将 `TaskPanel` 与 `SettingsPanel` 的书卷静态样式、字段皮肤与按钮外观从 `C#` 下放到 `GDScript`，让卷册视觉调校不再依赖面板业务脚本。
- 玩家可感知收益（10 分钟内）：治宗册与机宜卷打开后保持原有书卷外观，同时后续要微调边框、字色、字段框与滑条时可直接改 `GDScript` 表现层。

## 实现范围

- 包含：
  - 扩展 `TaskPanelVisualFx.gd`，承接治宗册的书卷容器、摘要卡、胶囊、箭头键、庶务列表与关闭钮的静态主题覆盖
  - 扩展 `SettingsPanelVisualFx.gd`，承接机宜卷的纸面、滚轴、字段框、按钮、滑条与快捷键按钮皮肤
  - `TaskPanel.cs` / `SettingsPanel.cs` 改为只保留权威数据刷新、输入处理与 `C# -> GDScript` 单向调用
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改 `GameState`、存档结构、快捷键持久化与小时结算公式
  - 不改治宗规则、季度法旨、门规定义与客户端设置业务判断
  - 不改现有开场 tween、切页 pulse 与录键强调的调用语义

## 实现拆解

1. 在两份现有 `VisualFx.gd` 中新增 `apply_theme_styles()`，把静态主题覆盖集中到表现脚本
2. 调整 `TaskPanel.cs` / `SettingsPanel.cs` 的 `_Ready()`，改为调用表现脚本应用样式，不再由面板脚本主动落主题
3. 保持业务层仅负责数据刷新、状态切换、提示文本和事件派发

## 验收标准（可测试）

- [ ] `TaskPanel` 的书卷框、摘要卡、胶囊、箭头键、庶务列表与关闭钮样式继续正常显示，且静态主题覆盖由 `TaskPanelVisualFx.gd` 承接
- [ ] `SettingsPanel` 的纸面、滚轴、字段框、滑条、动作按钮与快捷键按钮样式继续正常显示，且静态主题覆盖由 `SettingsPanelVisualFx.gd` 承接
- [ ] `TaskPanel.cs` / `SettingsPanel.cs` 继续只保留规则、输入与状态逻辑，不新增业务判断下放到 `GDScript`
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：场景节点路径变更后，表现脚本可能失效；若后续同时改 scene 与脚本，静态样式可能局部漏绑
- 回滚方式：恢复 `TaskPanel.cs` / `SettingsPanel.cs` 中原有主题应用调用，并回退对应 `VisualFx.gd` 的样式承接代码即可
