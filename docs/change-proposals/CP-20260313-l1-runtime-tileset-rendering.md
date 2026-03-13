# 变更提案（Change Proposal）

## 提案信息

- 变更名：宗门图 L1 运行时 tileset 化
- 提案ID：`CP-20260313-l1-runtime-tileset-rendering`
- 变更级别：`L3`
- 关联功能卡：`docs/feature-cards/FC-20260313-l1-runtime-tileset-rendering.md`

## 变更缘由

- 当前仓库虽然已有 `L1_hex_tileset.tres`，但宗门图运行时仍主要走 `manifest + 图片切区 + _Draw()`。
- 用户已明确要求地图改为直接使用 `L1_hex_tileset.tres` 进行绘制，而不是继续停留在“编辑器资源已建好、运行时仍在画图片”的半接入状态。
- 当前最新 tileset 资源已进一步演进到 `L1_tilemap_c.png` 单源版本，如果运行时不直接消费 `.tres`，后续资源调整仍会持续出现文档、编辑器与运行时三处漂移。

## 现状问题

- `L1_hex_tileset.tres` 未进入宗门图主链。
- `l1_terrain_manifest.json` 与当前 `.tres` 资源口径可能分叉。
- 若直接把方形 atlas 当矩形 tile 渲染，会出现纹理外露、缝隙和几何错位，不符合 hex 视觉预期。

## 方案裁定

- 宗门图与二级局部沙盘的 L1 基础地块层，改由 `CountyTownMapViewSystem` 直接读取 `L1_hex_tileset.tres`。
- 运行时 terrain -> tile variant 的选择逻辑仍落在 `CountyTownMapViewSystem`。
- 从 tileset atlas source 读取 texture region，再按现有 hex polygon 几何直接投影，保证无缝贴近且不绘制基础边沿。
- `_Draw()` 继续承担 L1 基础地块本体绘制，但其纹理来源从 manifest 图片切区切到 tileset 资源本身；上层 overlay、建筑、选中高亮与交互覆盖继续保留。

## 影响范围

- `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`
- `docs/02_system_specs.md`
- `docs/05_feature_inventory.md`
- `docs/08_development_list.md`
- `docs/14_map_layer_rendering_implementation_plan.md`

## 验收指标

- 运行时 L1 地块直接由 `L1_hex_tileset.tres` 提供纹理 region，而不是继续读取 atlas 图片路径。
- 当前 `L1_tilemap_c.png` 不再以方片原图直接外露，hex 地块之间无缝贴近。
- 默认不绘制基础地块边沿线。
- 世界格二级局部沙盘继续可生成、可缩放、可点选、可回灌左栏检视。
- `dotnet build .\Finally.sln` 通过。

## 回滚路径

- 若 tileset region 驱动链路出现纹理错位，可先恢复到 manifest + polygon 自绘 fallback。
- 保留 tileset 加载与 variant 映射逻辑，不删除接口。
- 回滚期间维持 `L2-L4` 现有行为，避免把建筑与检视链路一并撤回。
