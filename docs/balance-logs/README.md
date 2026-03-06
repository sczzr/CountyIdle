# Balance Logs 索引

本目录存放上线后结果记录（平衡日志）。  
命名规则：`BL-YYYYMMDD-canonical-topic[-NN].md`

## 记录覆盖范围

- 机制类：科研、产业、事件、装备掉落
- 交互类：按钮行为、布局切换、面板重构
- 视觉类：主题替换、地图表现、参考稿对齐

## 最低记录标准

每条日志至少包含：

1. 改动前
2. 改动后
3. 结果指标与结论（保留/继续调参/回滚）

> 若与提案不一致，需在日志中写明原因与后续动作。

补充规则：

- 日志文件名与关联 FC/CP 共享同一 `canonical-topic`。
- 归档收敛与 topic 归并建议见 `docs/06_archive_registry.md`。

## 新文件名快速索引

| 主题 | 文件名 |
| --- | --- |
| `ui-layout-migration` | `BL-20260305-ui-layout-migration-01.md` ~ `BL-20260306-ui-layout-migration-08.md` |
| `map-tabs-and-viewport` | `BL-20260306-map-tabs-and-viewport-01.md`、`BL-20260306-map-tabs-and-viewport-02.md`、`BL-20260306-map-tabs-and-viewport-03.md`、`BL-20260306-map-tabs-and-viewport-04.md`、`BL-20260306-map-tabs-and-viewport-05.md` |
| `countytown-25d-map` | `BL-20260306-countytown-25d-map-01.md`、`BL-20260306-countytown-25d-map-02.md` |
| `han-courtyard-theme-pack` | `BL-20260305-han-courtyard-theme-pack-01.md` ~ `BL-20260305-han-courtyard-theme-pack-03.md` |
| 单体机制/功能 | `BL-20260305-research-breakthrough.md`、`BL-20260306-industrial-job-logic.md`、`BL-20260306-equipment-rarity-affix-drop.md`、`BL-20260306-county-dynamic-events.md`、`BL-20260306-ui-interaction-rewire.md`、`BL-20260306-all-buttons-interactive.md`、`BL-20260306-three-map-render-zoom.md` |
