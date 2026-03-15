# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第八批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`DisciplePanel`、`SectOrganizationPanel`

## 目标与用户收益

- 目标：继续将 `DisciplePanel` 与 `SectOrganizationPanel` 的书卷静态样式、筛选控件、卡片外观与动态导航选中态从 `C#` 下放到 `GDScript`，让弟子谱 / 峰令谱的视觉调整不再依赖面板业务脚本。
- 玩家可感知收益（10 分钟内）：弟子谱与峰令谱打开后保持原有书卷风格，筛选控件、名册树、峰脉导航与职司卡片的外观可在 `GDScript` 侧统一调校。

## 实现范围

- 包含：
  - 扩展 `DisciplePanelVisualFx.gd`，承接弟子谱主框、筛选控件、名册树、指标格、进度条与关闭钮的静态主题覆盖
  - 扩展 `SectOrganizationPanelVisualFx.gd`，承接峰令谱书卷主框、动作按钮、动态峰脉导航卡、职司卡与部门卡的样式和选中态外观
  - `DisciplePanel.cs` / `SectOrganizationPanel.cs` 改为只保留权威数据刷新、输入处理与 `C# -> GDScript` 单向调用
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改弟子属性计算、筛选业务逻辑、峰脉协同逻辑、治理入口事件与地图选择结构
  - 不改 `GameState`、存档结构与小时结算公式
  - 不改现有开场 tween、pulse 与详情切换的调用语义

## 实现拆解

1. 在两份现有 `VisualFx.gd` 中新增 `apply_theme_styles()`，把静态主题覆盖集中到表现脚本
2. 调整 `DisciplePanel.cs` / `SectOrganizationPanel.cs` 的 `_Ready()`，改为调用表现脚本应用样式，不再由面板脚本主动落主题
3. 对 `SectOrganizationPanel` 的动态峰脉导航卡 / 职司卡 / 部门卡，改为由 `GDScript` 承接静态外观与选中态切换
4. 保持业务层仅负责数据刷新、状态切换、提示文本和事件派发

## 验收标准（可测试）

- [ ] `DisciplePanel` 的书卷主框、筛选控件、名册树、指标格与进度条样式继续正常显示，且静态主题覆盖由 `DisciplePanelVisualFx.gd` 承接
- [ ] `SectOrganizationPanel` 的书卷主框、动作按钮、峰脉导航卡、职司卡与部门卡样式继续正常显示，且动态选中态由 `SectOrganizationPanelVisualFx.gd` 承接
- [ ] `DisciplePanel.cs` / `SectOrganizationPanel.cs` 继续只保留规则、输入与状态逻辑，不新增业务判断下放到 `GDScript`
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：场景节点路径或动态卡片结构后续变更时，表现脚本可能失效；若 `VisualFx` 节点丢失，功能逻辑仍在但视觉会退化
- 回滚方式：恢复 `DisciplePanel.cs` / `SectOrganizationPanel.cs` 中原有主题应用调用，并回退对应 `VisualFx.gd` 的样式承接代码即可
