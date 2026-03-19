# 功能卡：修炼卷可复用修仙控件场景（SoulCore / JadeSlipCard / SpiritStatBar）

## 功能信息

- 功能名：修炼卷可复用修仙控件场景（SoulCore / JadeSlipCard / SpiritStatBar）
- 优先级：`P1`
- 目标版本：`2026-03-19`
- 关联系统：`CultivationPanel`、`godot-css-theme`、`xianxia.css`、SVG 控件资源、`CultivationPanelVisualFx.gd`

## 目标与用户收益

- 目标：把修炼卷里已经成形的阵眼 HUD、玉简决策卡与火候刻度条抽成独立可复用场景，避免继续把同类视觉结构硬编码在单一 `.tscn` 里。
- 玩家可感知收益（10 分钟内）：打开修炼卷后，左侧阵眼与右侧玉简卡的层次更统一，火候条更像“菱形宝石刻度”，后续弟子谱/治宗册复用这些修仙控件时能保持同一套视觉语义。

## 实现范围

- 包含：
  - 新增 `CountyIdle/scenes/ui/components/SoulCore.tscn`，封装旋转法阵、头像框与境界标签。
  - 新增 `CountyIdle/scenes/ui/components/JadeSlipCard.tscn`，封装 2x2 玉简卡片骨架、激活态切换与局部动效入口。
  - 新增 `CountyIdle/scenes/ui/components/SpiritStatBar.tscn`，封装五段菱形火候刻度。
  - 扩展 `CountyIdle/ui/xianxia.css`，为这些组件提供 `theme_type_variation` 对应的局部样式。
  - 在 `CountyIdle/scenes/ui/CultivationPanel.tscn` 首次接入上述组件，完成修炼卷最小闭环验证。
- 不包含：
  - 全项目范围的 Lottie 插件接入。
  - 其他卷册的全面替换。
  - 修炼结算公式与数值平衡改动。

## 实现拆解

1. 先补功能卡与开发列表登记，明确这是 `DL-094` CSS 原型之后的组件化延伸，而不是另一套孤立 UI。
2. 把修炼卷中最稳定的三类视觉块抽成独立场景，并让内部节点名尽量兼容现有 `CultivationPanel.cs` / `CultivationPanelVisualFx.gd` 的路径约定。
3. 用 `xianxia.css` 中的 `theme_type_variation` 负责基础皮肤，组件脚本只负责旋转、呼吸、激活态切换等轻动效。
4. 首轮先在 `CultivationPanel` 落地，确认构建通过，再决定是否扩到弟子谱或治宗册。

## 验收标准（可测试）

- [ ] 新增三个组件场景文件，且命名与职责清晰：`SoulCore.tscn`、`JadeSlipCard.tscn`、`SpiritStatBar.tscn`。
- [ ] `CultivationPanel.tscn` 已使用这些组件场景，而不是继续把对应结构完全内联。
- [ ] `xianxia.css` 已为组件提供局部变体，至少覆盖玉简卡、头像框/境界签与火候条的基础皮肤。
- [ ] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - 当前仓库尚未接入正式 Lottie 运行时，法阵层需要先用 SVG + 旋转动效近似，避免因为额外插件阻塞本轮交付。
  - `CultivationPanel.cs` 与 `CultivationPanelVisualFx.gd` 依赖较多固定节点路径，组件抽取时如果根节点或子节点命名漂移，容易引发运行时取节点失败。
- 回滚方式：回退 `CultivationPanel.tscn` 对组件场景的引用，保留 `xianxia.css` 与资源文件即可逐步回到内联结构。
