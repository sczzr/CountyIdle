# 功能卡：主界面背景图接入

## 功能信息

- 功能名：主界面背景图接入（`background_2.png`）
- 优先级：`P2`
- 目标版本：`2026-03 主界面表现层优化`
- 关联系统：`DL-045 主界面六边形沙盘重构`、`CountyIdle/scenes/Main.tscn`、`docs/02_system_specs.md`

## 目标与用户收益

- 目标：将 `CountyIdle/assets/ui/background/background_2.png` 接入为主游戏背景，统一 Legacy 主界面的卷轴山水底图。
- 玩家可感知收益（10 分钟内）：进入游戏主界面时可直接看到完整卷轴山水背景，整体氛围更贴近当前天衍峰经营主题。

## 实现范围

- 包含：
  - 将 `background_2.png` 挂到 `Main.tscn` 根背景层；
  - 直接使用 `background_2.png` 整图作为实际背景源；
  - 为背景节点增加毛玻璃材质；
  - 停用旧的纯色背景与重复卷轴装饰底稿；
  - 保持现有主界面布局、按钮路径与交互逻辑不变。
- 不包含：
  - 不改 `GameLoop`、`GameState`、小时结算与存档结构；
  - 不重排 `TopBar / LeftPanel / RightPanel / BottomBar` 节点布局；
  - 不改 Figma 布局业务逻辑。

## 实现拆解

1. 对齐 `DL-045` 与主界面表现层文档描述，登记本次背景替换。
2. 在 `Main.tscn` 将根背景切换为 `background_2.png`，停用旧卷轴底稿节点。
3. 直接使用新背景整图参与全屏铺图。
4. 为背景节点挂载毛玻璃 shader，弱化山水细节并保留卷轴质感。
5. 为根背景补齐窗口缩放联动，确保始终填充整个游戏窗口。
6. 构建验证并回写 `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`。

## 验收标准（可测试）

- [ ] 进入 Legacy 主界面后，根背景显示 `background_2.png`
- [ ] 背景整图构图与目标画幅一致，不再额外裁切
- [ ] 背景存在可感知的毛玻璃效果，且不影响前景 UI 可读性
- [ ] 缩放游戏窗口时，背景会同步缩放并始终填满整个窗口
- [ ] 现有 `Tile Inspector / 山门近闻 / 底部控制台 / 地图面板` 节点路径与交互保持可用
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：背景图与现有卷轴骨架重复叠加，可能造成视觉层次冲突。
- 回滚方式：恢复 `CountyIdle/scenes/Main.tscn` 的纯色 `Background` 与旧卷轴底稿可见状态。
