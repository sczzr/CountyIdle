# 数值平衡日志（Balance Log）

## 记录信息

- 日期：2026-03-06
- 版本：`v0.x`
- 关联提案：无（视觉表现升级）

## 改动摘要

- 改动项：县城俯瞰图改为直接使用 `Isometric_game_asset_sheet_topdown_view_zen_chines_delpmaspu.png` 的真实组件切片，并按道路连通关系切换 stone-path tile。
- 改动前：
  - 县城地表使用“参考图风格重建”的 atlas
  - 视觉接近原图，但并未直接消费你提供的图片素材
- 改动后：
  - 县城地表优先使用从原图切出的真实组件 atlas
  - `Ground / Courtyard / Water / RoadMask(0~15)` 均由原图中的 tile 或组件映射而来
  - 建筑与居民链路保持 overlay，不影响结算层

## 结果数据

- 指标 1：从原图提取独立组件 `66` 个，生成 contact sheet 与 metadata
- 指标 2：县城运行时 atlas 由真实原图组件重组生成，不再是重绘版资源
- 指标 3：`dotnet build .\Finally.sln` 通过（0 warning / 0 error）

## 结论

- 是否达到预期：`是`
- 下一步：`保留`

## 复盘

- 有效原因：把“素材切片”和“县城语义映射”拆开后，既能保证直接使用原图，又能维持现有 `TownMapGeneratorSystem` 数据结构不变。
- 无效原因：原图不是完整 autotile 套装，所以当前 `road_0..15` 中仍有部分 mask 复用同一原图组件。
- 后续假设：如果你后面再补一张专门的道路/水岸补全图，可以无痛替换当前 mask 映射表。
