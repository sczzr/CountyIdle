# 功能卡（Feature Card）

## 功能信息

- 功能名：宗门图 Layer 2 六方向 connector branch 接入（运行时二期）
- 优先级：`P1`
- 目标版本：2026-03-12
- 关联系统：`CountyTownMapViewSystem`、`TownMapGeneratorSystem`、`docs/11_map_asset_production_spec.md`

## 目标与用户收益

- 目标：将天衍峰山门图的 Layer 2 从“中心 decal + 程序连线”推进到“中心 core + 六方向 branch connector”贴图方案，使道路和水域具备更接近正式资产管线的 hex 拼接表现。
- 玩家可感知收益（10 分钟内）：地图上的道路和水流不再主要依赖线条勾勒，而是拥有从格心向格边自然延展的连续贴图感，整体更接近卷轴地图成品。

## 实现范围

- 包含：
  - 新增道路 / 水域 branch connector 贴图
  - 在 `CountyTownMapViewSystem` 中按六方向邻接绘制 branch
  - 保留 Layer 2 core decal 与旧 fallback，避免一次性推翻
  - 回写 `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`
- 不包含：
  - 本轮不接入法阵连接类贴图
  - 本轮不引入 Layer 2 独立 atlas
  - 本轮不推进 Layer 3 立体物件

## 实现拆解

1. 增加 top-oriented 的道路 / 水域 branch 贴图。
2. 根据 hex 邻接方向，在运行时旋转并绘制 branch connector。
3. 保留原 core decal，并在 branch 贴图缺失时回退到旧连线逻辑。

## 验收标准（可测试）

- [ ] 道路与水域均可按六方向邻接绘制 branch connector。
- [ ] connector 由贴图主导，旧线条仅作为 fallback 或轻度描边存在。
- [ ] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - `DrawSetTransform` 旋转贴图若处理不当，可能影响同帧后续绘制。
  - branch 素材若尺寸不合适，容易在格边留下缝隙或过度重叠。
- 回滚方式：
  - 保留 branch 资源入口和绘制逻辑；
  - 如表现不理想，可暂时回退到 core decal + 旧 connector，但不移除 branch 接口。
