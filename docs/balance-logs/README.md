# Balance Logs 索引

本目录存放上线后结果记录（平衡日志）。  
命名规则：`BL-YYYYMMDD-canonical-topic[-NN].md`

> 归档语义说明（2026-03-08）：历史日志沿用旧 `canonical-topic` 文件名以保持链路一致；若正文提到 `县城 / 郡图 / 郡县`，默认按当前“天衍峰山门图 / 江陵府外域 / 宗门经营”解释。

## 记录覆盖范围

- 机制类：科研、产业、事件、装备掉落
- 交互类：按钮行为、布局切换、面板重构
- 视觉类：主题替换、地图表现、参考稿对齐（含 `countytown-* / prefecture-*` 历史 topic）
- 文档治理类：世界观重梳、术语收口、导航与流程统一
- 宗主治理类：治理方略、职司摘要、贡献点 / 灵石双轨内务、季度法令、门规树
- 组织谱系类：九峰概览、峰脉详情、附属部门可视化、协同峰法旨
- 宗门原型类：浮云宗、天衍峰、青云峰三总殿等默认设定锚定
- 归档桥接类：历史正文的当前语义提示、标题与摘要层收口

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
| `map-tabs-and-viewport` | `BL-20260306-map-tabs-and-viewport-01.md`、`BL-20260306-map-tabs-and-viewport-02.md`、`BL-20260306-map-tabs-and-viewport-04.md` |
| `countytown-25d-map` | `BL-20260306-countytown-25d-map-01.md`、`BL-20260306-countytown-25d-map-02.md` |
| `han-courtyard-theme-pack` | `BL-20260305-han-courtyard-theme-pack-01.md` ~ `BL-20260305-han-courtyard-theme-pack-03.md` |
| `sect-management-docs-reframe` | `BL-20260308-sect-management-docs-reframe.md` |
| `sect-task-orders-dual-currency` | `BL-20260308-sect-task-orders-dual-currency.md` |
| `sect-governance-strategy-layer` | `BL-20260309-sect-governance-strategy-layer.md` |
| `sect-governance-policies` | `BL-20260309-sect-governance-policies.md` |
| `sect-quarter-decrees` | `BL-20260309-sect-quarter-decrees.md` |
| `sect-rule-tree` | `BL-20260309-sect-rule-tree.md` |
| `sect-organization-jobslist` | `BL-20260309-sect-organization-jobslist.md` |
| `sect-organization-peak-browser` | `BL-20260309-sect-organization-peak-browser.md` |
| `sect-peak-support-directives` | `BL-20260309-sect-peak-support-directives.md` |
| `fuyun-sect-tianyan-setting-alignment` | `BL-20260309-fuyun-sect-tianyan-setting-alignment.md` |
| `archive-semantic-bridges` | `BL-20260308-archive-semantic-bridges.md` |
| 单体机制/功能 | `BL-20260305-research-breakthrough.md`、`BL-20260306-industrial-job-logic.md`、`BL-20260306-equipment-rarity-affix-drop.md`、`BL-20260306-county-dynamic-events.md`、`BL-20260306-ui-interaction-rewire.md`、`BL-20260306-all-buttons-interactive.md`、`BL-20260306-three-map-render-zoom.md` |

