# 功能卡（Feature Card）

## 功能信息

- 功能名：世界图地形纹理绑定外置
- 优先级：`P1`
- 目标版本：2026-03-16
- 关联系统：`StrategicMapViewSystem`、`WorldPanel.tscn`、`WorldTerrainTileLayer`、`L1_hex_tileset.tres`

## 目标与用户收益

- 目标：世界地图地貌与具体纹理的绑定可在 `GDScript / TileMapLayer` 上配置，运行时自动读取并回退到默认映射，避免每次调整贴图都改 `C#`。
- 玩家可感知收益（10 分钟内）：替换 tileset 或调整地貌贴图时，只需在场景或脚本上改绑定即可立即看到世界图底盘变化。

## 实现范围

- 包含：
  - 在 `WorldTerrainTileLayer` 上挂载 `WorldTerrainTileBindings.gd` 并导出 `Plain / Spirit / Rugged / Snow / DeepWater / ShallowWater` 的 tile 坐标列表
  - `StrategicMapViewSystem` 读取绑定并驱动 `_Draw()` 与 `TileMapLayer` 两条链路
  - 未配置或为空时回退到默认映射
- 不包含：
  - 不新增世界图专用 tileset 资源
  - 不改道路 / 河流 / 标签等叠加层
  - 不改变世界图主链的 hex polygon 绘制

## 实现拆解

1. 新增 `WorldTerrainTileBindings.gd` 导出可编辑映射并挂载到 `WorldTerrainTileLayer`
2. `StrategicMapViewSystem` 增加绑定读取与 fallback
3. 回写 `02 / 05 / 08` 与本功能卡

## 验收标准（可测试）

- [ ] `WorldTerrainTileLayer` 的绑定可在场景或 `GDScript` 中调整，世界图底盘随之变化
- [ ] 绑定为空 / 缺失时回退到默认映射，无报错
- [ ] `_Draw()` 与 `TileMapLayer` 复用同一绑定逻辑
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：绑定坐标与 tileset atlas 不一致会导致地貌错位。
- 回滚方式：移除绑定脚本或清空绑定并回退到默认映射。
