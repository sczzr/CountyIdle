# 功能卡（Feature Card）

## 功能信息

- 功能名：宗门图 L1 运行时改为直接消费 `L1_hex_tileset.tres`
- 任务ID：`FC-20260313-l1-runtime-tileset-rendering`
- 优先级：`P1`
- 目标版本：2026-03-13
- 关联系统：`CountyTownMapViewSystem`、`SectMapViewSystem`、`WorldPanel.tscn`

## 目标与用户收益

- 目标：将天衍峰山门图与二级局部沙盘的基础地块层从“manifest + 图片自绘”切到“直接读取 `L1_hex_tileset.tres` 并按 hex polygon 投影”的真实运行时链路。
- 玩家可感知收益（10 分钟内）：L1 基础地块不再只是和 tileset 参数对齐，而是直接由当前 tileset 资源驱动；用户替换或调整 `L1_hex_tileset.tres` 后，地图运行时会立刻沿用同一套切片结果，同时保持现有 hex 沙盘的无缝贴近与点击习惯。

## 飞轮环节

- `反哺宗门 -> 地图表现层 -> 二级地图复用`

## 依赖

- `docs/02_system_specs.md`
- `docs/05_feature_inventory.md`
- `docs/08_development_list.md`
- `docs/14_map_layer_rendering_implementation_plan.md`
- `CountyIdle/assets/ui/tilemap/L1_hex_tileset.tres`

## 实现范围

- 包含：
  - 在 `CountyTownMapViewSystem` 内直接加载 `L1_hex_tileset.tres`
  - 对当前 `L1_tilemap_c.png` 单源 tileset 与旧 `L1_tilemap_a/b.png` 双源 tileset 保持兼容识别
  - 从 tileset atlas source 读取 texture region，并按现有 hex polygon 几何投影
  - 让 hex 底图无缝贴近，并默认不绘制基础地块边沿线
  - 保持当前缩放、平移、点选与左栏检视链路兼容
- 不包含：
  - 本轮不改 `TownMapGeneratorSystem` 的 terrain 语义生成
  - 本轮不重写 `Layer 2 / Layer 3 / Layer 4` 运行时结构
  - 本轮不推进世界图 `StrategicMapViewSystem` 的 tileset 化

## 实现拆解

1. 运行时读取 `L1_hex_tileset.tres`，按 terrain 类型选择 tile variant。
2. 从 tileset atlas source 提取 texture region，并直接投影到 hex polygon。
3. 将基础地块半径拉满到无缝贴近，并关闭默认边沿描边。
4. 保持 `_Draw()` 继续负责上层 overlay、建筑、选中高亮与交互覆盖。

## 验收标准（可测试）

- [ ] 天衍峰山门图的基础地块由 `L1_hex_tileset.tres` 驱动，而不是直接切图片 atlas。
- [ ] tileset 纹理在运行时以 hex polygon 方式投影，不出现原始矩形方片外露。
- [ ] hex 底图无缝贴近，且默认不绘制基础地块边沿线。
- [ ] `SetExternalMap()` 生成的局部沙盘同样沿用该 L1 tileset 链路。
- [ ] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - 若 tileset atlas 的切片 region 与实际美术内容不一致，可能出现纹理语义与地貌类型错位。
  - 若 tileset 更新而 terrain -> tile variant 映射未同步，可能出现地貌语义偏差。
  - 若底盘放大过度，可能在极端缩放下产生轻微纹理重叠。
- 回滚方式：
  - 保留现有 `_Draw()` 的 L1 fallback 入口；
  - 如 tileset 运行时链路异常，可暂时回退到 manifest + polygon 自绘，但保留 tileset variant 映射接口继续调试。
