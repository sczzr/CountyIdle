# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 峰令谱表现层 GDScript 第二十一批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`SectOrganizationPanel`

## 目标与用户收益

- 目标：继续将 `SectOrganizationPanel` 动态生成卡片中剩余的纯视觉 shell 参数从 `C#` 下放到 `GDScript`，让峰脉导航卡 / 职司卡的交互光标与三类动态卡片的内容间距由 `SectOrganizationPanelVisualFx.gd` 统一承接。
- 玩家可感知收益（10 分钟内）：峰令谱的动态卡片交互手感保持一致，后续调整卡片交互光标或内间距时不再需要触碰业务脚本。

## 实现范围

- 包含：
  - 为 `SectOrganizationPanelVisualFx.gd` 增加动态卡片壳层 helper，承接峰脉导航卡 / 职司卡的 `mouse_default_cursor_shape`
  - 将三类动态卡片内部 `VBoxContainer` 的 `separation` 间距配置迁到 `GDScript`
  - `SectOrganizationPanel.cs` 改为仅负责卡片结构拼装、输入绑定与业务刷新
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改峰脉协同规则、治理入口、提示文案与选中态判定
  - 不改动态卡片的内容文本、tooltip 与点击行为
  - 不改 `GameState`、存档结构与小时结算

## 实现拆解

1. 复查 `SectOrganizationPanel.cs` 中动态卡片创建里残留的纯视觉参数
2. 在 `SectOrganizationPanelVisualFx.gd` 中新增对应 shell helper
3. 让 `SectOrganizationPanel.cs` 只做单向调用，不再直接设置光标或内间距外观
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `SectOrganizationPanelVisualFx.gd` 已承接动态峰脉导航卡 / 职司卡的光标，以及三类动态卡片的内间距壳层配置
- [ ] `SectOrganizationPanel.cs` 不再直接设置这些动态卡片的纯视觉 shell 参数
- [ ] 峰令谱动态卡片的点击、tooltip、选中态与治理入口逻辑保持不变
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 `VisualFx` 节点缺失或 helper 名称漂移，动态卡片仍可点击，但交互手型或间距会退化为默认表现
- 回滚方式：恢复 `SectOrganizationPanel.cs` 中本批移除的光标 / 间距配置，并撤回 `SectOrganizationPanelVisualFx.gd` 的对应 helper
