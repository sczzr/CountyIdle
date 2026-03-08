# 改动提案（Change Proposal）

> 历史兼容说明（2026-03-08）：本文保留旧阶段的“县城 / 郡图 / 郡县 / 州府”等表述以便追溯；在当前项目语义中，请分别按“天衍峰驻地 / 江陵府外域 / 天衍峰经营 / 世界地图”理解。
> 当前对外语义：天衍峰山门图 / 天衍峰驻地链路（保留历史 countytown 技术名）


## 提案信息

- 标题：宗门建筑与场所底座收敛为 hex footprint
- 日期：2026-03-08
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：宗门主视图虽然已经切到 hex 地表，但住宅与场所底座、高亮与阴影仍保留菱形 footprint，整体观感还不够统一。
- 证据（数据/玩家反馈）：在上一轮宗门 hex 俯瞰落地后，建筑和场所底座仍明显保留旧等距痕迹。

## 改动内容

- 改什么：
  - 住宅建筑底盘改为 hex footprint 底座
  - 场所底座、选中高亮、阴影与命中判定统一改为 hex 几何
  - 保留现有屋顶、墙面、门洞与装饰 accent 表现，降低回归风险
- 不改什么：
  - 不改 `TownMapGeneratorSystem` 的生成逻辑
  - 不改弟子排班、通勤、存档与主循环
  - 不把住宅 / 场所全部重做为完整六边体模型
- 影响系统：
  - `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`
  - `CountyIdle/scripts/systems/CountyTownMapViewSystem.Anchors.cs`
  - `docs/02_system_specs.md`
  - `docs/08_development_list.md`

## 预期结果

- 预期提升指标：
  - 建筑与场所底座和 hex 地表更统一
  - 场所选中高亮与命中区和视觉形状一致
  - 不破坏现有屋顶、墙面和弟子链路
- 可接受副作用：
  - 这轮仍保留现有轻量 roof/wall 几何，只先统一底盘语义

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 运行 Godot，检查宗门建筑底座、场所高亮、选中命中区与遮挡顺序
- 观察周期：单次 5~10 分钟宗门页验证
- 成功判定阈值：无构建错误、无选中漂移、底座观感明显统一

## 回滚条件

- 触发条件：场所命中区明显偏移、底座遮挡异常、建筑观感显著变差
- 回滚步骤：
  1. 回退 `CountyTownMapViewSystem.cs` 与 `CountyTownMapViewSystem.Anchors.cs` 的 hex footprint 改动
  2. 回退规格与开发记录中的底座 hex 化描述


