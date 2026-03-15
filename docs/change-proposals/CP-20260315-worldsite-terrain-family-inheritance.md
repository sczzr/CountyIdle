# 改动提案（Change Proposal）

## 提案信息

- 标题：世界格地形家族继承到二级地图
- 日期：2026-03-15
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：
  - 世界图已经能按 world hex 语义生成二级地图入口，但局部沙盘底盘仍主要按 `Ground / Road / Courtyard / Water` 的粗分类取图；
  - 玩家在世界图点进雪峰、荒岭、灵地等不同格子时，二级地图容易出现“布局有一点差异，但地表读感仍然像同一套图”的割裂感。
- 证据（数据/玩家反馈）：
  - 本轮需求明确提出：“地形生成算法中，具体的地形需要绑定好对应的图片。然后二级地图需要依照一级地图的地形块来生成地图”；
  - 代码现状中，一级图已有 world hex 到 L1 图块家族的分桶，但二级地图尚未把这层分桶稳定继承到 external map 渲染链。

## 改动内容

- 改什么：
  - 抽出一级图/二级图共享的 world terrain visual family 规则；
  - 在 `TownMapData` 增加 external map 可用的地形家族元数据；
  - 让 `WorldSiteLocalMapGeneratorSystem` 按 source world hex 的 `Terrain / Biome / Water / Qi / Corruption` 影响局部格的 `Ridge / Spirit / Hazard / Water` 分布，并写入继承地形家族；
  - 让 `CountyTownMapViewSystem` 在 external map 模式下优先按继承地形家族使用 `L1_hex_tileset.tres` 图块。
- 不改什么：
  - 不新增新的美术资源；
  - 不改世界图点击、左栏检视、二级地图进入/返回主链；
  - 不在本轮补齐二级地图专属经营/战斗 UI。
- 影响系统：
  - `CountyIdle/scripts/models/TownMapData.cs`
  - `CountyIdle/scripts/systems/WorldTerrainVisualRules.cs`
  - `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`
  - `CountyIdle/scripts/systems/WorldSiteLocalMapGeneratorSystem.cs`
  - `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`

## 预期结果

- 预期提升指标：
  - 二级地图对一级 world hex 地貌的继承感更强；
  - 世界图进入不同地貌格子后，玩家能在 1 次进入内感知到“底盘和局部布局确实不同”；
  - 世界图 / 二级地图的 L1 贴图口径更统一，减少后续继续扩展正式素材时的双轨分叉。
- 可接受副作用：
  - 在现有素材受限下，`Rugged / Snow` 仍可能表现为“更接近正确语义的近似贴图”，不是最终美术成品。

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 手动抽查世界图不同地貌 world hex 的二级地图进入结果，确认至少 `灵地 / 荒岭 / 雪地 / 水域` 有稳定差异。
- 观察周期：
  - 本轮开发后立即验证 + 下一轮继续做 Godot 运行烟测。
- 成功判定阈值：
  - build 通过；
  - 二级地图 external map 不再全部回落成同一类普通底盘；
  - 山门图主链不回退。

## 回滚条件

- 触发条件：
  - 新增的地形家族元数据导致 external map 渲染异常、山门图底盘错乱或世界图/二级图贴图口径明显漂移。
- 回滚步骤：
  - external map 回退到旧的 `TownTerrainType` 选图逻辑；
  - 保留共享地形家族工具类与生成骨架，等待素材或更细模板完备后再重新接回。
