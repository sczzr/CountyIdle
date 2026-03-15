# 功能卡（Feature Card）

## 功能信息

- 功能名：世界图地形 tileset 元数据绑定
- 优先级：`P1`
- 目标版本：2026-03-16
- 关联系统：`StrategicMapViewSystem`、`L1_hex_tileset.tres`、`WorldTerrainTileLayer`

## 目标与用户收益

- 目标：允许通过 tileset 的 custom data layer 直接声明“地形家族 -> tile 坐标”映射，运行时自动读取并驱动世界图底盘。
- 玩家可感知收益（10 分钟内）：调整 tileset 元数据即可改世界图地貌贴图，无需改 `C#` 或场景脚本。

## 实现范围

- 包含：
  - `StrategicMapViewSystem` 优先从 tileset custom data layer 读取 `world_terrain_family` 绑定
  - 支持 `plain / spirit / rugged / snow / deep_water / shallow_water` 字符串标签
  - 若元数据缺失则回退到 `WorldTerrainTileLayer` 的绑定或默认映射
- 不包含：
  - 不新增新的 tileset 资源
  - 不改道路 / 河流 / 标签等叠加层
  - 不改变世界图 hex polygon 主绘制链路

## 实现拆解

1. 新增读取 tileset custom data layer 的绑定逻辑
2. 保留 tile layer / 默认映射作为 fallback
3. 回写 `02 / 05 / 08` 与本功能卡

## 验收标准（可测试）

- [ ] 在 `L1_hex_tileset.tres` 中设置 `world_terrain_family` 后，世界图底盘按元数据变化
- [ ] 元数据缺失时回退到既有绑定，不报错
- [ ] `_Draw()` 与 `TileMapLayer` 共用同一份绑定
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：元数据标签写错会导致地貌错位或回退默认映射。
- 回滚方式：清空 custom data layer 并回退到 tile layer / 默认映射。
