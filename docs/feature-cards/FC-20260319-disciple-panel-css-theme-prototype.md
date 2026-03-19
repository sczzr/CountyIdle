# 功能卡：弟子谱 CSS 主题原型（godot-css-theme）

## 功能信息

- 功能名：弟子谱 CSS 主题原型（godot-css-theme）
- 优先级：`P1`
- 目标版本：`2026-03-19`
- 关联系统：`DisciplePanel`、`godot-css-theme` 插件、`xianxia.css`、`docs/ui-prototypes/character_profile.html`

## 目标与用户收益

- 目标：把弟子谱双页拆分里的“深邃暗黑修仙”视觉语言，先以 CSS 主题资源的方式稳定落到 Godot 场景上，减少后续继续调 StyleBox / Theme `.tres` 的人工成本。
- 玩家可感知收益（10 分钟内）：打开弟子谱时，树状宗门大谱与命谱详情会统一呈现暗色玻璃面板、暗金边框、警示红战力签与灵气青进度条，不再只依赖全局汉庭主题的默认样式。

## 实现范围

- 包含：
  - 新增 `CountyIdle/ui/xianxia.css`，作为弟子谱子树的局部 Theme 原型。
  - `DisciplePanel.tscn` 挂载 CSS Theme，并给关键节点接入 `theme_type_variation`。
  - 通过 `PanelContainerJade / Seal / Badge / Warning / Terminal` 与 `ButtonGhost` 等变体，对齐双页原型图的主要视觉层级。
- 不包含：
  - 全局主题切换 UI。
  - 其他卷册统一迁移到 CSS Theme。
  - 运行时通过 metadata 动态切 class；当前仍以 Godot `theme_type_variation` 为主。

## 实现拆解

1. 在 `CountyIdle/ui/xianxia.css` 中定义基础控件样式与少量局部变体。
2. 将 `DisciplePanel` 根节点绑定该 CSS 导入出的 Theme。
3. 给卷首印章、命谱玉牌、警示战力签、终端日志盒等关键节点指定变体。
4. 保持现有 `DisciplePanel.cs` 数据绑定与 `DisciplePanelVisualFx.gd` 动效职责不变，只改视觉皮肤入口。

## 验收标准（可测试）

- [ ] `DisciplePanel.tscn` 已引用 `res://ui/xianxia.css`，且不影响 `dotnet build .\Finally.sln`。
- [ ] 弟子谱大谱页与命谱详情页至少有一组暗金边框卡片、一组警示红标签、一组灵气青进度条来自 CSS Theme。
- [ ] 关键变体通过 `theme_type_variation` 挂接，不需要再为这些节点单独写大段 `StyleBoxFlat` 资源。

## 风险与回滚

- 风险：
  - 当前 `godot-css-theme` 插件导入逻辑更接近“CSS -> Theme 资源”，并非完整 Web CSS 运行时树；若误用 `Metadata class` 工作流，可能不会生效。
  - 主题变体名依赖 `NodeType + ClassGroup.capitalize()` 生成规则，后续继续扩展时要保持类名简单可控。
- 回滚方式：移除 `DisciplePanel.tscn` 对 `res://ui/xianxia.css` 的引用，并删除该 CSS 文件即可恢复到现有全局主题表现。
