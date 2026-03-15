# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 峰令谱表现层 GDScript 第二十三批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`SectOrganizationPanel`

## 目标与用户收益

- 目标：在动态卡片的光标、间距与留白都已迁到 `GDScript` 之后，继续回收 `SectOrganizationPanel.cs` 中退化为空壳的 helper 和分散的表现层调用残留，让面板脚本更明确地只保留业务链。
- 玩家可感知收益（10 分钟内）：功能表现不变，但后续维护峰令谱时，`C#` 与 `GDScript` 的边界更清晰，更不容易误把表现层改回业务脚本。

## 实现范围

- 包含：
  - 删除 `SectOrganizationPanel.cs` 中仅返回 `new MarginContainer()` 的薄壳 helper
  - 将面板内残留的 `_visualFx?.Call(...)` 直接调用统一收口到 `CallVisualFx(...)`
  - 保持动态卡片结构、输入与业务刷新逻辑不变
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改峰脉协同规则、治理入口、提示文案、tooltip 与选中态切换
  - 不改 `SectOrganizationPanelVisualFx.gd` 的表现语义
  - 不改 `GameState`、存档结构与小时结算

## 实现拆解

1. 复查 `SectOrganizationPanel.cs` 中在前两批拆分后已退化为空壳的 helper
2. 统一面板内的表现层单向调用入口
3. 删除不再提供额外语义价值的薄壳 helper
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `SectOrganizationPanel.cs` 不再保留仅返回 `new MarginContainer()` 的薄壳 helper
- [ ] 面板内指向 `VisualFx` 的单向调用已统一经由 `CallVisualFx(...)`
- [ ] 峰令谱动态卡片的点击、tooltip、选中态与治理入口逻辑保持不变
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若统一调用时漏改某个 `VisualFx` 入口，表现层可能退化，但业务逻辑仍可运行
- 回滚方式：恢复 `SectOrganizationPanel.cs` 中本批删除的 helper 和直接调用写法即可
