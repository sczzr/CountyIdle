# 功能卡（Feature Card）

## 功能信息

- 功能名：世界地图底层接入与回退修正
- 优先级：`P1`
- 目标版本：2026-03-13
- 关联系统：`StrategicMapViewSystem`、`WorldPanel.tscn`、`L1_hex_tileset.tres`、`docs/14_map_layer_rendering_implementation_plan.md`

## 目标与用户收益

- 目标：让世界地图的基础地块层稳定复用 `L1_hex_tileset.tres`，并在不破坏 hex 几何与点选链路的前提下得到可用底盘；若 `TileMapLayer` 的方片排布在运行时会把底盘切碎，则必须及时回退到脚本按 hex polygon 投 atlas 区域的主链。
- 玩家可感知收益（10 分钟内）：世界舆图的底盘表现与后续宗门图 / 二级地图的素材复用方向更一致，缩放和平移时底层地貌更稳定，也为后续继续扩到二级地图提供了现成节点结构。

## 实现范围

- 包含：
  - 在 `WorldPanel.tscn` 的世界地图页补入 `WorldTerrainTileLayer`
  - `StrategicMapViewSystem` 负责让世界图继续复用 `L1_hex_tileset.tres`，并在 `TileMapLayer` 不满足视觉要求时回退到脚本 hex polygon 投影
  - 世界图继续保留脚本叠加层，承接道路、河流、站点、标签与选中反馈
  - 世界图底盘主链不再保留旧版蜂窝背景网格
  - 回写 `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`、`docs/14_map_layer_rendering_implementation_plan.md`
- 不包含：
  - 本轮不重做世界图的道路 / 河流为独立 `TileMapLayer`
  - 本轮不改二级地图与宗门图的现有交互链路
  - 本轮不引入新的世界图正式量产素材

## 实现拆解

1. 让世界地图继续复用 `L1_hex_tileset.tres` 的地貌资源。
2. 若 `TileMapLayer` 方片排布造成六边形白缝，则回退到脚本按 hex polygon 投 atlas 区域的主链。
3. 保持道路、河流、站点、标签和点选逻辑继续走脚本叠加层，避免一次性改动过大。
4. 将这次阶段成果并入 `DL-049`，作为“世界图底盘接入后完成一次可用性回退收口”的已落地节点。

## 验收标准（可测试）

- [ ] 世界地图页继续复用 `L1_hex_tileset.tres` 的地貌资源，且运行时不会再出现方片裁切后的连续白缝。
- [ ] 世界图缩放、平移后，底盘与道路 / 河流 / 站点 / 标签叠加层仍对齐。
- [ ] 世界图不再看到旧版六边形蜂窝背景网格。
- [ ] 左键点选世界格、右键清除选中、进入二级地图入口的现有链路不回退。
- [ ] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - 当前 `L1_hex_tileset.tres` 可用 tile 类型有限，世界图地貌会先以“分桶近似映射”接入，后续仍需更丰富的专用世界图 tileset。
  - 若未来再次尝试把世界格直接写入 `TileMapLayer`，必须先验证它不会把 hex 底盘切出白缝，再决定是否切回节点式主链。
- 回滚方式：
  - 若节点式 `TileMapLayer` 再次出现白缝或对齐错误，可继续保留 `WorldTerrainTileLayer` 基础设施，但正式主链维持脚本 hex polygon 投影；
  - 不回滚世界格点选、左栏检视和二级地图入口链路。
