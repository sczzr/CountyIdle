# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第九批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`SaveSlotsPanel`、`MainWorldSitePanel`

## 目标与用户收益

- 目标：继续将 `SaveSlotsPanel` 与 `MainWorldSitePanel` 的书卷静态样式、字段皮肤、预览区外观与 world-site sandbox 壳层表现从 `C#` 下放到 `GDScript`，让留影录与二级地图页的视觉调整不再依赖业务脚本。
- 玩家可感知收益（10 分钟内）：留影录与二级地图页保持原有书卷风格，筛选框、卷册按钮、预览框、world-site summary card 与局部沙盘壳层可在 `GDScript` 侧统一调校。

## 实现范围

- 包含：
  - 扩展 `SavePreviewCrossfade.gd`，承接留影录纸面、滚轴、卷册按钮、筛选框、命名框与列表的静态主题覆盖
  - 扩展 `WorldPanelVisualFx.gd`，承接 world-site summary card、标题色调与 sandbox 壳层主题覆盖
  - `SaveSlotsPanel.cs` / `MainWorldSitePanel.cs` 改为只保留权威数据刷新、地图数据、输入处理与 `C# -> GDScript` 单向调用
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改卷册筛选 / 排序、存读写、复制删除与命名逻辑
  - 不改 world-site 语义、局部沙盘数据生成、地图选择与 `GameState`
  - 不改现有开场 tween、预览过渡与 panel pulse 的调用语义

## 实现拆解

1. 在 `SavePreviewCrossfade.gd` 中新增 `apply_theme_styles()`，把留影录静态主题覆盖集中到表现脚本
2. 调整 `SaveSlotsPanel.cs` 的 `_Ready()`，改为通过表现脚本应用样式，不再由面板脚本主动落主题
3. 在 `WorldPanelVisualFx.gd` 中新增 world-site summary card / sandbox 壳层主题与色调接口
4. 保持业务层仅负责数据刷新、状态切换、提示文本和事件派发

## 验收标准（可测试）

- [ ] `SaveSlotsPanel` 的纸面、滚轴、筛选框、命名框、卷册按钮与列表样式继续正常显示，且静态主题覆盖由 `SavePreviewCrossfade.gd` 承接
- [ ] `MainWorldSitePanel` 的 world-site summary card、标题色调与 sandbox 壳层样式继续正常显示，且静态主题覆盖由 `WorldPanelVisualFx.gd` 承接
- [ ] `SaveSlotsPanel.cs` / `MainWorldSitePanel.cs` 继续只保留规则、输入、地图数据与状态逻辑，不新增业务判断下放到 `GDScript`
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：场景节点路径或 world-site sandbox 壳层结构后续变更时，表现脚本可能失效；如果 `VisualFx` 节点缺失，逻辑仍在但视觉会退化
- 回滚方式：恢复 `SaveSlotsPanel.cs` / `MainWorldSitePanel.cs` 中原有静态主题应用调用，并回退对应 `GDScript` 的样式承接代码即可
