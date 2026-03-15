# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十四批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`DisciplePanel`

## 目标与用户收益

- 目标：继续回收 `DisciplePanel.cs` 中剩余的小块纯视觉 helper，把指标值颜色切换与 trait tag 外观下放到 `DisciplePanelVisualFx.gd`，让弟子谱业务脚本进一步收口到名册数据、筛选排序与详情文案。
- 玩家可感知收益（10 分钟内）：弟子谱右侧指标色彩与 trait 标签外观保持一致，但后续调整表现层时不再需要进入业务脚本修改 `StyleBox` 或颜色 override。

## 实现范围

- 包含：
  - 将指标值颜色切换从 `DisciplePanel.cs` 下放到 `DisciplePanelVisualFx.gd`
  - 将 trait tag 的纯视觉样式从 `DisciplePanel.cs` 下放到 `DisciplePanelVisualFx.gd`
  - 保持 trait 文案来源、属性数值与筛选排序逻辑留在 `C#`
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改弟子 roster 生成、详情文本、雷达图数据组织与存档结构
  - 不改小时结算、治理规则与弟子数值公式

## 实现拆解

1. 复核 `DisciplePanel.cs` 剩余的小块 `AddThemeColorOverride` 与 `StyleBoxFlat` 代码
2. 扩展 `DisciplePanelVisualFx.gd` 承接指标值 tone 与 trait tag 样式
3. 让 `DisciplePanel.cs` 仅保留文本、数值与节点生命周期
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `DisciplePanel.cs` 不再直接承担指标值颜色切换与 trait tag 的纯视觉样式
- [ ] `DisciplePanelVisualFx.gd` 已承接指标值 tone 与 trait tag 皮肤
- [ ] `DisciplePanel.cs` 仍保留弟子数据整理、筛选排序和文案解释逻辑
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 C# 与 GDScript 之间传入的节点引用不匹配，可能导致指标色或 trait tag 样式缺失
- 回滚方式：恢复 `DisciplePanel.cs` 中对应颜色 / StyleBox helper，并移除 `DisciplePanelVisualFx.gd` 的新增接口
