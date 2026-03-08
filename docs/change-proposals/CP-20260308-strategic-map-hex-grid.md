# 改动提案（Change Proposal）

## 提案信息

- 标题：世界图 / 郡图战略视图补强为 hex 战略底格
- 日期：2026-03-08
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：世界图与郡图虽然已有配置驱动 / 程序化生成链路，但底层仍使用矩形辅助线，和“地图采用六角战略底格观看”的方向不一致。
- 证据（数据/玩家反馈）：用户在完成水彩页 hex 俯视后继续要求推进地图表现；现有 `StrategicMapViewSystem` 的 `DrawGrid` 仍是方格线框。

## 改动内容

- 改什么：
  - 将 `StrategicMapViewSystem` 的矩形辅助网格替换为 hex 战略底格
  - 默认节点标记改为六角标记，强化世界图 / 郡图的 hex 俯视感
  - 保持 `StrategicMapDefinition`、`strategic_maps.json`、`PrefectureMapGeneratorSystem` 的数据来源不变
- 不改什么：
- 不把战略地图改造成逐格存储的正式地图层系统
  - 不改 `Main.cs` 的页签切换、缩放按钮与主 UI 组织
  - 不改人口、产业、探险、科技、存档等核心结算
- 影响系统：
  - `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`
  - `docs/02_system_specs.md`
  - `docs/05_feature_inventory.md`
  - `docs/08_development_list.md`

## 预期结果

- 预期提升指标：
  - 世界图 / 郡图首次打开即呈现 hex 战略底格
  - 现有区域、道路、河流、节点、标签叠层保持可见且缩放不回归
  - 不破坏世界图配置驱动与郡图程序化生成链路
- 可接受副作用：
  - 这轮是“hex 战略底格 + 叠层矢量”的表现补强，而不是完整逐格战棋地图

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 运行 Godot，检查 `天下州域 / 周边郡图` 页的 hex 底格、缩放与标题更新
  - 验证经营状态联动后地图色调仍正常生效
- 观察周期：单次 5~10 分钟地图页交互验证
- 成功判定阈值：无构建错误、无切页回归、hex 底格稳定显示

## 回滚条件

- 触发条件：hex 底格造成地图过密、严重遮挡区域信息或缩放性能明显退化
- 回滚步骤：
  1. 回退 `StrategicMapViewSystem.cs` 的 hex 网格与六角节点绘制逻辑
  2. 回退规格、看板与开发列表中的 hex 战略底格描述
