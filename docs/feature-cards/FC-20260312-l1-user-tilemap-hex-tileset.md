# FC-20260312 L1 User Tilemap Hex Tileset

- 任务ID：`FC-20260312-l1-user-tilemap-hex-tileset`
- 主责 Agent：`Gameplay Agent`
- 协作 Agent：`System Agent`、`Release Agent`
- 当前状态：`已完成`

## 目标

- 将用户提供的 `L1_tilemap_a.png` 与 `L1_tilemap_b.png` 接为 Godot 可直接复用的 `pointy-top` 六边形 `TileSet`。
- 同步修正 `L1` terrain manifest 的 atlas 来源，确保运行时 atlas/manifest 与编辑器 tileset 使用同一批资源。

## 飞轮环节

- `反哺宗门 -> 地图表现层 -> 二级地图复用`

## 依赖

- `docs/11_map_asset_production_spec.md`
- `docs/14_map_layer_rendering_implementation_plan.md`
- `docs/05_feature_inventory.md` 中“地图素材分层资产流水线”
- `docs/08_development_list.md` 中 `DL-049`

## 实现范围

- 新增 `CountyIdle/assets/ui/tilemap/L1_hex_tileset.tres`
- 使用两张 `4x2` atlas 建立 `TileSetAtlasSource`
- `L1_tilemap_a.png` 使用 `margins = (38, 18)`、`separation = (30, 15)`、`texture_region_size = (646, 742)`
- `L1_tilemap_b.png` 使用 `margins = (24, 10)`、`separation = (48, 14)`、`texture_region_size = (640, 748)`
- 将 `TileSet` 设为 `tile_shape = HEXAGON`、`tile_layout = STACKED`、`tile_offset_axis = HORIZONTAL`
- 回写 `CountyIdle/assets/map/manifests/l1_terrain_manifest.json` 的 atlas 来源与逐 atlas 切片参数

## 完成标准（DoD）

- Godot 中可直接打开 `L1_hex_tileset.tres` 并看到两组 `4x2` hex atlas
- `l1_terrain_manifest.json` 与 tileset 均指向 `L1_tilemap_a/b.png`
- 每张 atlas 的 8 个 hex 单元都已注册为 tile
- 运行时 atlas manifest 支持按 atlas 独立读取 `tile_pixel_size / render_anchor`
- 不改变现有 `TownMapGeneratorSystem` / `CountyTownMapViewSystem` 的调用边界

## 验证记录

- 已确认两张源图尺寸均为 `2752x1536`
- 已按 Godot 编辑器手调结果对齐 atlas 切片规则：
- `L1_tilemap_a.png`：首格偏移 `(38, 18)`，单格尺寸 `646x742`，列间距 `30`，行间距 `15`
- `L1_tilemap_b.png`：首格偏移 `(24, 10)`，单格尺寸 `640x748`，列间距 `48`，行间距 `14`
- 已执行 `dotnet build .\\Finally.sln`

## 风险与建议

- 当前环境未直接提供 `godot` 可执行命令，未完成编辑器内可视化复核；建议在本地 Godot 4.6 打开 `L1_hex_tileset.tres` 并确认两张 atlas 的 8 个 tile 选区与截图一致
- 现有运行时主链仍以 manifest + 自绘 hex 为主，若后续要让 `TileMapLayer` 直接消费该资源，仍建议再补一个预览/适配入口
