# 功能卡（Feature Card）

## 功能信息

- 功能名：世界格地形家族继承到二级地图
- 优先级：`P1`
- 目标版本：2026-03-15
- 关联系统：`StrategicMapViewSystem`、`WorldSiteLocalMapGeneratorSystem`、`CountyTownMapViewSystem`、`TownMapData`、`L1_hex_tileset.tres`

## 目标与用户收益

- 目标：让世界图的一格在进入 `SecondaryMapView` 后，不再只生成“同模板不同文案”的局部沙盘，而是继续继承一级地图的基础地形家族与底盘贴图口径，使山地像山地、雪地像雪地、灵地像灵地、水域像水域。
- 玩家可感知收益（10 分钟内）：在世界舆图点进不同地貌格子后，二级地图会明显呈现不同的底盘与局部布局，不再出现“明明选的是雪峰/荒岭/灵地，进去却还是同一种普通地面”的割裂感。

## 实现范围

- 包含：
  - 为二级地图补一层“继承自 world hex 的地形家族”口径；
  - 统一一级图与二级图对 `Plain / Spirit / Rugged / Snow / ShallowWater / DeepWater` 的基础分桶；
  - `WorldSiteLocalMapGeneratorSystem` 按 world hex 地貌语义调整局部地图的 `Ridge / Spirit / Hazard / Water` 分布；
  - `CountyTownMapViewSystem` 在 external map 模式下优先按继承地形家族选择 `L1_hex_tileset.tres` 图块，而不再只按 `Ground / Courtyard / Water / Road` 粗分类随机取图；
  - 回写 `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`。
- 不包含：
  - 本轮不为二级地图新增独立 `Layer 4 / Layer 5` 氛围层；
  - 本轮不补四大类型的专属交互控件或独立结算；
  - 本轮不制作新的正式美术资源，继续复用现有 `L1_hex_tileset.tres` 与现有 connector 资源。

## 实现拆解

1. 抽出 world hex 到“基础地形家族”的共享判定规则，避免一级图和二级图各自漂移。
2. 在局部地图生成阶段，将 source world hex 的地貌家族写入 external map 的运行时数据。
3. 在二级地图渲染阶段，external map 优先按继承家族选图，普通山门图仍保留现有语义 fallback。
4. 保持世界图点选、左侧检视、二级地图进入/返回链路与当前缩放/点选节奏不变。

## 验收标准（可测试）

- [ ] 世界图进入二级地图后，至少能在 `Plain / Spirit / Rugged / Snow / Water` 几类底盘上看出稳定差异，而不再全部回落为同一套普通地面。
- [ ] `WorldSiteLocalMapGeneratorSystem` 生成的局部地图会根据 source world hex 的 `Terrain / Biome / Water` 语义调整 `Ridge / Spirit / Hazard / Water` 分布。
- [ ] `CountyTownMapViewSystem` 在 external map 模式下会优先按继承地形家族选取 `L1_hex_tileset.tres` 图块。
- [x] 山门图原有 `TownMapGeneratorSystem` 主链不被破坏，未提供继承家族的格子仍可按旧逻辑回退。
- [x] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - 现有 `L1_hex_tileset.tres` 的图块类型仍有限，`Rugged / Snow` 目前仍需复用现有 atlas 坐标近似表现，正式视觉差异仍受素材库存约束；
  - 若把二级地图完全写死为一级图贴图家族，后续做专属玩法模板时可能会压缩表现空间。
- 回滚方式：
  - 保留 `TownTerrainVisualFamily` 这一层运行时元数据；若表现不理想，可让 external map 回退到旧的 `TownTerrainType` 选图逻辑，但不回退 world hex → local map 的地貌分桶骨架。
