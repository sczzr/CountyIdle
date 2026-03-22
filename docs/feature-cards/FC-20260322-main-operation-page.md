# 功能卡：主操作页面（TopBar + BottomBar + 地图）

## 功能信息

- 功能名：主操作页面（TopBar + BottomBar + 地图）
- 优先级：P1
- 目标版本：2026.03
- 关联系统：主界面 UI / 地图视图

## 目标与用户收益

- 目标：提供一个独立的主操作页面，专注展示顶栏、底栏与地图。
- 玩家可感知收益（10 分钟内）：进入场景即可看到核心 HUD 与地图，操作路径更清晰。

## 实现范围

- 包含：
  - 新建 `MainOperation.tscn`，包含 TopBar/WorldPanel/BottomBar 三段布局。
  - 地图区域自适应拉伸，顶栏/底栏固定。
  - 主入口保留 `TitleMenuOverlay`，进入游戏后展示主操作页面。
- 不包含：
  - 不修改 `Main.tscn` 与 `Main.cs` 主流程。
  - 不新增新的按钮逻辑或系统联动。

## 实现拆解

1. 新建 `MainOperation.tscn` 并搭建 RootMargin + VBox 布局。
2. 实例化 `TopBar`、`WorldPanel`、`BottomBar` 并设置尺寸/拉伸。
3. 文档登记与自检。

## 验收标准（可测试）

- [ ] 打开 `MainOperation.tscn` 可见 TopBar、地图、BottomBar。
- [ ] 地图区域在 `1280x720` 下不被遮挡并自适应拉伸。
- [ ] 现有 `Main.tscn` 与 `Main.cs` 不受影响。
- [ ] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：布局尺寸未覆盖导致地图被遮挡。
- 回滚方式：移除 `MainOperation.tscn` 并回退文档登记。
