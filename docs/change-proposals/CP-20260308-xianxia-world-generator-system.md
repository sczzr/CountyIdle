# 改动提案：修仙 Hex 世界生成系统

## 提案信息

- 标题：修仙 Hex 世界生成系统接入世界图
- 日期：2026-03-08
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：世界图虽然已有战略缩放与 hex 底格，但地图内容仍偏通用，缺少修仙世界所需的灵脉、奇观、宗门候选与灵性资源表达。
- 证据（数据/玩家反馈）：本轮需求明确要求“修仙世界 Hex 地图 256 Tile 完整系统”和 `XianxiaWorldGeneratorSystem` 数据结构与实现。

## 改动内容

- 改什么：
  - 新增修仙世界生成配置与数据结构；
  - 新增地貌/河流/龙脉/灵气区/资源/奇观/宗门候选生成逻辑；
  - 生成器同步输出 `StrategicMapDefinition`，直接接入世界图；
  - 世界图保留原 `strategic_maps.json` fallback。
- 不改什么：
  - 不改 `GameState` 与小时结算；
  - 不改郡图和县城现有生成器；
  - 不新增新的世界图交互页签。
- 影响系统：
  - `CountyIdle/scripts/models/XianxiaWorldGenerationConfig.cs`
  - `CountyIdle/scripts/models/XianxiaWorldMapData.cs`
  - `CountyIdle/scripts/systems/XianxiaWorldGenerationConfigSystem.cs`
  - `CountyIdle/scripts/systems/XianxiaWorldGeneratorSystem.cs`
  - `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`
  - `CountyIdle/data/xianxia_world_generation.json`

## 预期结果

- 预期提升指标：
  - 世界图从抽象区域图升级为具备修仙语义的 hex 世界图；
  - 世界图可直接读出灵脉、奇观、宗门候选、场所点位与稀有资源热点；
  - 继续沿用现有缩放与状态条，不引入额外 UI 成本。
- 可接受副作用：
  - 世界图 region 数量显著提升；
  - 颜色与节点信息密度提升后，首次进入世界图的视觉复杂度会增加。

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 直接调用 `XianxiaWorldGeneratorSystem.Generate(config)` 与 `GenerateStrategicDefinition(config, out world)` 检查输出数量
- 观察周期：本次迭代内即时验证
- 成功判定阈值：
  - 默认配置可稳定生成完整世界数据；
  - 世界图适配层输出非空 region / route / river / node / label；
  - 世界图异常时能 fallback，不阻断主界面。

## 回滚条件

- 触发条件：世界图生成报错、地图无法显示、绘制性能明显劣化或构建失败。
- 回滚步骤：
  1. 将 `StrategicMapViewSystem` 世界图入口切回 `StrategicMapConfigSystem.GetWorldDefinition()`；
  2. 保留修仙生成器源码与文档，不删除数据结构；
  3. 后续按性能/表现问题拆分子任务再逐步恢复接入。
