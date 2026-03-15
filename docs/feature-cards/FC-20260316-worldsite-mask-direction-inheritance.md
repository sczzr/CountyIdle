# 功能卡（Feature Card）

## 功能信息

- 功能名：世界格方向遮罩继承到二级地图
- 优先级：`P1`
- 目标版本：2026-03-16
- 关联系统：`WorldSiteLocalMapGeneratorSystem`、`StrategicMapViewSystem.WorldCellSelection`、`CountyTownMapViewSystem`、`XianxiaWorldMapData`

## 目标与用户收益

- 目标：在“二级地图继承一级地图地形家族”基础上，继续把 world hex 的 `RoadMask / RiverMask / CliffMask` 与进出朝向带到局部沙盘里，使道路入口、水体走势与高差边界不再固定写死在同一侧。
- 玩家可感知收益（10 分钟内）：进入不同世界格后，局部地图不仅底盘颜色/材质不同，道路从哪边进、水从哪边压过来、山脊从哪侧逼近也会明显变化，更像是同一格子的下钻视图。

## 实现范围

- 包含：
  - 二级地图入口道路朝向继续继承 world hex 的 `RoadMask`；
  - 二级地图水体/河流优先继承 `RiverMask` 与 `Water` 方向，不再默认固定落在左侧或右下角；
  - 二级地图高差/险地优先继承 `CliffMask` 或山地朝向，不再始终由同一套固定 ridge 区域表达；
  - 局部沙盘中的入口锚点与朝向同步随主入口方向调整。
- 不包含：
  - 本轮不补全多边道路桥、河湾、断崖的精细 connector 美术；
  - 本轮不为不同二级模板新增专属战斗/经营控件；
  - 本轮不改世界图自身的 overlay 绘制规则。

## 实现拆解

1. 抽出 world hex mask 到局部沙盘边界方向的稳定映射规则。
2. 让 `PaintApproachPath()` 支持按 `RoadMask` 生成一条或多条进场路。
3. 让水体与 ridge 生成不再写死位置，而是按 `RiverMask / Water / CliffMask` 对应边界继承。
4. 让入口锚点与 facing 同步跟随主入口方向变化。

## 验收标准（可测试）

- [ ] world hex 存在不同 `RoadMask` 时，二级地图入口道路不会永远固定从左侧进入。
- [ ] world hex 存在 `RiverMask` 或显著水体时，二级地图水体边界/水路走势会随方向变化。
- [ ] 山地/断崖格进入二级地图后，ridge / hazard 的主要分布方向会受 `CliffMask` 或高差朝向影响。
- [ ] 入口锚点与朝向会随主入口边界变化，而不是永远固定在左侧中线。
- [x] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - 若方向映射过于激进，局部地图可能出现路径、水体与核心区冲突过多的情况；
  - 六边形世界方向与局部 offset grid 的映射若处理不稳，容易出现“方向有变，但观感不自然”。
- 回滚方式：
  - 保留边界方向映射 helper；
  - 若效果不理想，可先保留“单主方向继承”，回退多方向叠加，不回退整个地形家族继承链。
