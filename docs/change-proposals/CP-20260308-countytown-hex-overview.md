# 改动提案（Change Proposal）

## 提案信息

- 标题：县城俯瞰主视图切换为 hex tile 地表投影
- 日期：2026-03-08
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：县城主视图仍使用等距投影，而世界图 / 郡图已经切到 hex 俯视表达，三层地图风格不统一。
- 证据（数据/玩家反馈）：用户明确要求“县城俯瞰也使用 hex tile”。

## 改动内容

- 改什么：
  - 将 `CountyTownMapViewSystem` 的地表格位投影从等距菱形切为 hex 俯视布局
  - 居民路径、场所选中、县城建筑与场所 overlay 跟随新的 hex 坐标中心渲染
  - 保持 `TownMapGeneratorSystem` 的生成输入、`TownMapData` 的网格与 `GameState` 结算不变
- 不改什么：
  - 不改人口、产业、探险、存档等核心系统
  - 不把县城主图改成逐格 `TileMapLayer`
  - 不重写县城道路 / 建筑 / 场所生成规则
- 影响系统：
  - `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`
  - `CountyIdle/scripts/systems/CountyTownMapViewSystem.Anchors.cs`
  - `CountyIdle/scripts/systems/CountyTownMapViewSystem.Residents.cs`
  - `docs/02_system_specs.md`
  - `docs/05_feature_inventory.md`
  - `docs/08_development_list.md`

## 预期结果

- 预期提升指标：
  - 县城俯瞰首次打开即表现为 hex tile 俯视地表
  - 住宅、场所实体、居民移动与选中提示继续可用
  - 地图缩放、经营状态染色与主循环不回归
- 可接受副作用：
  - 这轮只切换县城地表投影与坐标中心，住宅 / 场所 overlay 仍沿用现有轻量几何表现

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 运行 Godot，检查县城俯瞰 hex 地表、居民通勤、场所选中与缩放
  - 切换经营状态后确认地块 / 建筑色调仍正常
- 观察周期：单次 5~10 分钟地图页交互验证
- 成功判定阈值：无构建错误、无选中错位、无居民移动回归

## 回滚条件

- 触发条件：hex 投影导致场所选不中、居民明显漂移、县城视图遮挡异常
- 回滚步骤：
  1. 回退 `CountyTownMapViewSystem*` 到等距投影版本
  2. 回退规格、看板与开发列表中的县城 hex 表述
