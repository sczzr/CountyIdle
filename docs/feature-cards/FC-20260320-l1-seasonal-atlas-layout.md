# 功能卡（Feature Card）

## 功能信息

- 功能名：`L1_hex_tileset.tres` 多图源四季地块图集编排规范（文档三期）
- 优先级：`P1`
- 目标版本：2026-03-20
- 关联系统：`L1_hex_tileset.tres`、`StrategicMapViewSystem`、`CountyTownMapViewSystem`、`docs/11_map_asset_production_spec.md`、`docs/14_map_layer_rendering_implementation_plan.md`

## 目标与用户收益

- 目标：把 `L1_hex_tileset.tres` 从“少量固定 source 的临时图块集合”收口为“可持续扩容的多图源四季 atlas 规范”，明确每张图 `4x2` 的行列语义、metadata 口径和后续新增 source 的接入方式。
- 玩家可感知收益（10 分钟内）：后续新增地貌图不会再因为 source 顺序、文件名或手写坐标分支而接错；世界图 / 宗门图 / 二级地图可以围绕同一套四季地块资产持续扩容。

## 实现范围

- 包含：
  - 在 `docs/11_map_asset_production_spec.md` 中明确 `4x2` source 的行列语义与 metadata 要求
  - 在 `docs/14_map_layer_rendering_implementation_plan.md` 中明确多图源四季 atlas 的运行时索引方案与 fallback
  - 在 `docs/05_feature_inventory.md`、`docs/08_development_list.md` 中回写该设计阶段结论
- 不包含：
  - 本轮不直接改 `C#` 实现为全量多 source metadata 驱动
  - 本轮不强制重命名现有历史 png 文件
  - 本轮不新增正式量产地块素材

## 实现拆解

1. 固定 `4x2` 图的行列语义：列为四季，行为该图内的两类地块。
2. 固定 tileset metadata 字段：`terrain_family / world_terrain_family / terrain_group / row_slot / season / variant_weight`。
3. 把 runtime 未来正式方向收口为“扫描全部 source，按 `terrain_family + season` 索引候选，再做稳定抽选”。
4. 明确历史兼容链路与回退顺序，避免新旧 atlas 并存期间出现断图。

## 验收标准（可测试）

- [ ] 文档能明确回答“单张 `4x2` 图的列和行分别代表什么”
- [ ] 文档能明确回答“后续新增 source 时，运行时不应再依赖 source 顺序或文件名推断地貌”
- [ ] 文档能明确回答“世界图 / 宗门图 / 二级地图的共同选图主键与 fallback 顺序”
- [ ] `docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步回写该阶段设计结论

## 风险与回滚

- 风险：
  - 若继续把语义绑在 source 顺序或文件名上，后续每加一张图都要改代码，扩容成本会持续升高。
  - 若 `terrain_family` 与 `world_terrain_family` 口径不统一，世界图与局部沙盘会出现同地貌不同贴图的割裂感。
- 回滚方式：
  - 保留“每 source 一张 `4x2` 图、列固定四季、行固定两类地块”的硬约束；
  - 若 metadata 方案需要简化，可先保留 `terrain_family + season` 最小主键，再逐步补 `terrain_group / variant_weight`。
