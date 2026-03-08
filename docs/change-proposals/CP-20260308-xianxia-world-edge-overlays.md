# 改动提案：修仙世界河流/道路 edge overlay

> 历史兼容说明（2026-03-08）：本文保留旧阶段的“县城 / 郡图 / 郡县 / 州府”等表述以便追溯；在当前项目语义中，请分别按“天衍峰驻地 / 江陵府外域 / 天衍峰经营 / 世界地图”理解。


## 提案信息

- 标题：修仙世界河流与道路改为 hex 共边绘制
- 日期：2026-03-08
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：修仙世界图虽然已经生成了 hex 地块和河流/灵脉数据，但视觉层仍主要依赖 polyline 覆盖，和“每个 hex 内贴基础 Texture、连接语义走边”的目标不完全一致。
- 证据（数据/玩家反馈）：本轮明确要求“河流道路应该在 hextile 的边上进行绘制，而不是画在 hextile 里面”。

## 改动内容

- 改什么：
  - 世界图模式下读取 `RiverMask / RoadMask` 按 hex 共边绘制 overlay；
  - 为修仙世界新增道路生成链路，把附庸据点与宗门候选之间连接成道路网络；
  - 在 edge overlay 上继续补河岸、桥梁、路口与悬崖边细节；
  - 保留龙脉 polyline 作为灵性路线表现，河流 polyline 仅作为 fallback。
- 不改什么：
  - 不改县城/郡图现有生成器；
  - 不接入新的资源结算、探索规则或宗门经营逻辑；
  - 不引入新的世界图页签与编辑器。
- 影响系统：
  - `CountyIdle/scripts/systems/XianxiaWorldGeneratorSystem.cs`
  - `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`
  - `docs/02_system_specs.md`
  - `docs/08_development_list.md`

## 预期结果

- 预期提升指标：
  - 河流和道路的拓扑关系与 hex 邻接一致；
  - 基础地表 Texture 与连接类 overlay 分层更清晰；
  - 水岸、桥梁、路口与悬崖边能够在不增加基础 tile 组合量的前提下表达出来；
  - 后续替换成正式美术 Texture 时，不需要为道路/河流组合爆炸式增加 tile 数量。
- 可接受副作用：
  - 世界图绘制时每帧多出一层 edge band 几何；
  - 道路为一期自动连通网络，后续仍可继续细化桥梁、驿站与三岔表现。

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 直接调用生成器，统计 `RoadMask / RiverMask` 非空格数
- 观察周期：本次迭代即时验证
- 成功判定阈值：
  - 世界图可稳定构建并显示；
  - 默认配置至少生成一组非空 `RoadMask` 与 `RiverMask`；
  - 世界图异常时仍可回退到配置驱动世界图。

## 回滚条件

- 触发条件：edge overlay 导致世界图不可见、构建失败、明显绘制错位或性能异常。
- 回滚步骤：
  1. 暂时关闭 `StrategicMapViewSystem` 世界图的 edge overlay 绘制；
  2. 保留 `RoadMask / RiverMask` 数据结构与道路生成逻辑；
  3. 世界图回退为一期 polyline 表现，后续单独修复对齐问题。


