# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`TaskPanel`、`DisciplePanel`

## 目标与用户收益

- 目标：在前几批已把静态主题迁到 `GDScript` 的基础上，继续清理 `TaskPanel.cs` 与 `DisciplePanel.cs` 中不再走正式运行路径的历史样式 helper，让面板业务脚本进一步收口到权威逻辑、数据刷新与输入处理。
- 玩家可感知收益（10 分钟内）：玩家侧外观不变，但后续维护这些卷册面板时，样式调校路径更清晰，不易再误把视觉改回 C#。

## 实现范围

- 包含：
  - 清理 `TaskPanel.cs` 中已迁移到 `TaskPanelVisualFx.gd` 的历史样式工厂与废弃运行时 UI 构建代码
  - 清理 `DisciplePanel.cs` 中已迁移到 `DisciplePanelVisualFx.gd` 的历史静态样式 helper
  - 把 `TaskPanel` 页签按钮的字体选中态统一交给 `TaskPanelVisualFx.gd`
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改治宗规则、弟子属性、筛选排序业务逻辑、存档结构与小时结算
  - 不改现有开场 tween、pulse 与详情切换的调用语义
  - 不改 `DiscipleRadarChart` 的权威绘制数据链

## 实现拆解

1. 审查 `TaskPanel.cs` / `DisciplePanel.cs` 中仍被保留的旧样式 helper，确认正式运行路径已不再引用
2. 删除不再使用的静态样式工厂、历史 UI builder 与多余颜色常量
3. 如仍有少量必须的动态样式点，则保留最小内联样式，而不重新扩大跨脚本耦合
4. 保持业务层仅负责数据刷新、状态切换、提示文本和事件派发

## 验收标准（可测试）

- [ ] `TaskPanel.cs` 不再保留已迁移到 `GDScript` 的旧样式工厂与废弃运行时 UI 构建代码
- [ ] `DisciplePanel.cs` 不再保留已迁移到 `GDScript` 的旧静态样式 helper，仅保留必要的动态样式点与权威逻辑
- [ ] `TaskPanelVisualFx.gd` 能承接页签按钮字体选中态，不需要 `TaskPanel.cs` 直接改字体颜色
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：如果某段旧 helper 实际仍被隐藏路径引用，删掉后可能在极少数 fallback 路径下暴露问题；页签字体选中态若未同步交给 `GDScript`，可能出现高亮丢失
- 回滚方式：恢复 `TaskPanel.cs` / `DisciplePanel.cs` 中对应历史 helper，并回退 `TaskPanelVisualFx.gd` 的页签按钮状态承接代码即可
