# 功能卡：修仙 Hex 世界生成系统

> 历史兼容说明（2026-03-08）：本文保留旧阶段的“县城 / 郡图 / 郡县 / 州府”等表述以便追溯；在当前项目语义中，请分别按“天衍峰驻地 / 江陵府外域 / 天衍峰经营 / 世界地图”理解。


## 功能信息

- 功能名：修仙 Hex 世界生成系统
- 优先级：`P1`
- 目标版本：当前迭代（2026-03-08）
- 关联系统：`StrategicMapViewSystem`、`XianxiaWorldGeneratorSystem`、`XianxiaWorldGenerationConfigSystem`

## 目标与用户收益

- 目标：将“天下州域”升级为具有修仙世界特征的六角地貌图，并复用现有世界图缩放与绘制链路。
- 玩家可感知收益（10 分钟内）：进入世界图即可看到灵脉、奇观、宗门候选、附庸据点与稀有资源分布，地图信息密度明显提升。

## 实现范围

- 包含：
  - 新增修仙世界生成配置与运行时数据结构；
  - 新增世界生成算法：地貌、河流、龙脉、灵气区、资源、奇观、宗门候选；
  - 将生成结果适配为 `StrategicMapDefinition` 并接入世界图；
  - 保留世界图配置驱动 fallback。
- 不包含：
  - 不接入新的经济、战斗、人口结算；
  - 不新增独立世界图编辑器与新页签；
  - 不替换郡图/县城现有生成器。

## 实现拆解

1. 设计 `XianxiaWorldGenerationConfig / XianxiaWorldMapData` 数据模型。
2. 实现配置加载与修仙世界生成器。
3. 产出 `StrategicMapDefinition` 适配层并接入 `StrategicMapViewSystem` 世界图。
4. 更新系统规格、功能看板、开发列表与平衡日志。

## 验收标准（可测试）

- [x] `dotnet build .\Finally.sln` 通过。
- [x] 生成器可输出 `2560` 个默认 hex 地块与世界级聚合数据。
- [x] 世界图默认显示修仙世界生成结果，生成异常时可回退到配置世界图。

## 风险与回滚

- 风险：世界图 region 数量大幅增加，若未来尺寸继续扩大，可能影响绘制性能。
- 回滚方式：将 `StrategicMapViewSystem` 世界图入口切回 `StrategicMapConfigSystem.GetWorldDefinition()`，保留生成器代码与配置文件待后续优化。


