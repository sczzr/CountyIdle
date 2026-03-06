# 改动提案（Change Proposal）

## 提案信息

- 标题：周边郡图 Village 风格程序化生成接入
- 日期：2026-03-06
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：周边郡图依赖固定点位配置，重复观察时缺少“聚落自然生长”的空间反馈。
- 证据（数据/玩家反馈）：新增需求明确要求参考 `watabou village-generator` 的生成观感，将郡图改为类似的聚落地图生成。

## 改动内容

- 改什么：
  - 新增 `PrefectureMapGeneratorSystem`，生成有机边界、放射道路、环路、河流与村镇节点
  - `StrategicMapViewSystem` 在 `Prefecture` 模式改为程序化定义，保留缩放与现有绘制链路
  - `Main` 在状态刷新时同步郡图生成输入（人口/住房/威胁/小时结算）
- 不改什么：
  - 不接入第三方站点源码，不做代码级“提取”
  - 不改 `GameLoop` 结算节奏与系统调用顺序
  - 不改存档格式与读档兼容策略
- 影响系统：
  - `scripts/systems/PrefectureMapGeneratorSystem.cs`（新增）
  - `scripts/systems/StrategicMapViewSystem.cs`
  - `scripts/Main.cs`
  - `docs/02_system_specs.md`

## 预期结果

- 预期提升指标：
  - 郡图具备“每局差异化”的地形轮廓与道路网络
  - 郡图变化可与县域状态分桶联动（人口/威胁）
  - 玩家在地图页 10 分钟内可明显感知地图生命力提升
- 可接受副作用：
  - 同一存档在关键状态跨桶时会出现郡图重新布局（视觉重构）

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 运行 Godot，观察周边郡图是否出现程序化道路/河流/节点网络
  - 调整人口或推进小时结算后确认“跨桶重建、同桶不重建”
- 观察周期：单次 10~15 分钟玩法流转验证
- 成功判定阈值：无报错、无主循环回归、地图可读性不低于现有版本

## 回滚条件

- 触发条件：郡图渲染出现严重裁切、路线纠缠不可读或明显性能回退
- 回滚步骤：
  1. 回退 `Main.cs` 对程序化郡图刷新调用
  2. 回退 `StrategicMapViewSystem.cs` 的程序化接入
  3. 删除 `PrefectureMapGeneratorSystem.cs` 并恢复静态配置路径
