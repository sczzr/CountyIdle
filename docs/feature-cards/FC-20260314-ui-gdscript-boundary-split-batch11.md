# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十一批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`WarehousePanel`

## 目标与用户收益

- 目标：把 `WarehousePanel.cs` 中剩余的大块静态主题、页签外观、库容色调切换与资源卡片纯视觉样式继续下放到 `WarehousePanelTransition.gd`，让仓储卷业务脚本进一步收口到库存数据、操作事件与提示文本。
- 玩家可感知收益（10 分钟内）：仓储卷的卷轴、库容色调、页签与资源卡片外观保持一致，但后续继续微调卷册皮肤时，不再需要翻进 `C#` 业务脚本寻找样式工厂。

## 实现范围

- 包含：
  - 将 `WarehousePanel.cs` 的静态主题覆盖迁入 `WarehousePanelTransition.gd`
  - 将页签按钮选中态与库容负载对应的纯视觉色调切换迁入 `WarehousePanelTransition.gd`
  - 将资源卡片模板的纯视觉装配（卡片 / token / 字号 / 颜色）迁入 `WarehousePanelTransition.gd`
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改库存统计、按钮动作、产线文本、Tooltip 内容与提示文案生成逻辑
  - 不改 `GameState`、材料规则、小时结算与存档结构
  - 不把资源列表筛选、分组或页签状态判断下放到 `GDScript`

## 实现拆解

1. 复核 `WarehousePanel.cs` 中仍承担的视觉样式工厂与动态外观切换
2. 扩展 `WarehousePanelTransition.gd`，承接静态主题、页签状态、库容色调和资源卡片样式
3. 让 `WarehousePanel.cs` 仅保留数据刷新、按钮事件、文案拼装与 `C# -> GDScript` 单向调用
4. 构建验证并回写开发看板

## 验收标准（可测试）

- [ ] `WarehousePanelTransition.gd` 已承接仓储卷的静态主题覆盖、页签选中态与库容色调外观
- [ ] `WarehousePanel.cs` 不再保留大块 `StyleBoxFlat` 样式工厂与按钮外观 helper
- [ ] 资源卡片的纯视觉样式由 `GDScript` 承接，`WarehousePanel.cs` 仅保留绑定与数值刷新
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若资源卡片节点在 `C#` 与 `GDScript` 间传递不稳，可能导致卡片样式缺失；若库容色调切换遗漏，可能出现告警印章与进度条配色不一致
- 回滚方式：恢复 `WarehousePanel.cs` 中对应样式 helper，并回退 `WarehousePanelTransition.gd` 新增的样式承接接口
