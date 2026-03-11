# CountyIdle 历史归档注册表（整合精简）

> 目标：避免同主题反复建档，统一“功能卡/提案/日志”三件套追踪。
>
> 世界观兼容说明（2026-03-08）：注册表中的 `canonical-topic` 以可追溯性优先，不因世界观升级而重命名；阅读与复用历史归档时，统一按当前“修仙宗门经营 + 外域探索”语义解释。

## 0) 历史 Topic → 当前语义映射

| 历史 Topic / 关键词 | 当前解释 |
| --- | --- |
| `county-dynamic-events` / 郡县动态事件 | 宗门动态事件（坊市、讲法、袭扰） |
| `countytown-*` / 县城地图 | 天衍峰山门图 / 天衍峰驻地 / 弟子活动可视化 |
| `prefecture-*` / 郡图 / 郡城 | 江陵府外域 / 外域主附庸据点 / 外域主城 |
| `three-map-*` / 三地图 | 当前主流程的双地图（宗门 / 世界）+ 江陵府外域备用视图 |
| `han-courtyard-*` | 古风庭院美术基底，可继续服务修仙宗门表现 |

## 1) Canonical Topic 注册表

| Canonical Topic | 当前归档文件（按阶段） | 状态 | 合并建议 |
| --- | --- | --- | --- |
| `research-breakthrough` | FC/CP/BL 全量齐备 | 稳定 | 作为“机制类三件套”样板 |
| `industrial-job-logic` | FC/CP/BL 全量齐备 | 稳定 | 与 `ui-interaction-rewire` 联查 |
| `equipment-rarity-affix-drop` | FC/CP/BL 全量齐备 | 稳定 | 与探险系统共同评估 |
| `county-dynamic-events` | FC/CP/BL 全量齐备 | 稳定 | 当前对外语义为“宗门动态事件”，topic 名保持兼容 |
| `countytown-25d-generator` | FC/CP/BL 全量齐备 | 稳定 | 当前对外语义为“天衍峰山门图程序化生成”；与 `countytown-25d-textured-render` 归并为同一史诗 |
| `countytown-25d-textured-render` | FC/BL（无 CP） | 演进 | 当前对外语义为“天衍峰山门图 2.5D/材质化表现”；后续并入 `countytown-25d-generator` |
| `ui-interaction-rewire` | FC/CP/BL 全量齐备 | 稳定 | 作为主 UI 行为基线 |
| `all-buttons-interactive` | FC/CP/BL 全量齐备 | 稳定 | 后续可并入 `ui-interaction-rewire` |
| `main-layout-switch` | FC/BL（无 CP） | 演进 | 后续并入 `ui-layout-migration` |
| `map-switch-tabs` | FC/BL（无 CP） | 演进 | 与 `mapviewport-center-content-tabs` 合并 |
| `mapviewport-center-content-tabs` | FC/BL（无 CP） | 演进 | 并入 `map-switch-tabs` |
| `three-map-render-zoom` | FC（无 CP/BL） | 演进 | 并入 `map-switch-tabs` 史诗链路 |
| `ui-scene-split` | FC/BL（无 CP） | 历史 | 并入 `ui-layout-migration` |
| `rimworld-modular-ui` | FC/BL（无 CP） | 历史 | 并入 `ui-layout-migration` |
| `figma-make-ui-skeleton` | FC/BL（无 CP） | 演进 | 并入 `ui-layout-migration` |
| `html-reference-ui-parity` | FC/BL（无 CP） | 历史 | 并入 `ui-layout-migration` |
| `reference-layout-rebuild` | FC/BL（无 CP） | 历史 | 并入 `ui-layout-migration` |
| `reference-layout-update-b` | FC/BL（无 CP） | 历史 | 并入 `ui-layout-migration` |
| `responsive-layout-fix` | FC/BL（无 CP） | 演进 | 并入 `ui-layout-migration` |
| `han-courtyard-theme` | FC/BL（无 CP） | 历史 | 与 `han-courtyard-textured-theme` 合并 |
| `han-courtyard-textured-theme` | FC/BL（无 CP） | 稳定 | 作为主题最终版 |
| `lantern-hover-pulse` | FC/BL（无 CP） | 稳定 | 并入主题最终版附录 |
| `building-elements-pack` | FC（无 CP/BL） | 资产交付 | 保持独立资产 topic |
| `client-settings-panel` | FC（无 CP/BL） | 已实现 | 下次涉及机制联动时补 CP/BL |
| `prefecture-village-style-generator` | FC/CP/BL（本次新增） | 演进 | 当前对外语义为“江陵府外域程序化生成”；并入战略地图史诗链路，后续扩展可复用同 topic |
| `population-allocation-lifecycle-commute` | FC/CP/BL（已归档下线） | 历史 | 2026-03-12 按需求移除文档与功能实现，不再继续沿用该 topic |
| `countytown-resident-walkers` | FC/BL（本次新增） | 演进 | 当前对外语义为“宗门弟子可视移动”；并入 `countytown-25d-map` 史诗链路，后续可替换正式人物美术 |
| `sect-management-docs-reframe` | FC/BL（本次新增） | 稳定 | 用于“天衍峰经营文档重梳”类任务；后续文档治理迭代沿用同 topic |
| `archive-semantic-bridges` | FC/BL（本次新增） | 稳定 | 用于给历史归档补“当前对外语义”与摘要层宗门化说明，减少旧正文跳戏 |
| `sect-peak-support-directives` | FC/CP/BL（本次新增） | 演进 | 用于“峰脉协同法旨 / 协同峰支援”链路；后续季度法令、跨峰支援继续沿用同 topic |
| `sect-quarter-decrees` | FC/CP/BL（本次新增） | 演进 | 用于“季度法令 / 季度宗门方针”链路；后续门规树与季度模板可继续沿用同 topic |
| `sect-rule-tree` | FC/CP/BL（本次新增） | 演进 | 用于“庶务 / 传功 / 巡山常设门规”链路；后续完整树状解锁继续沿用同 topic |

## 2) 建议收敛的“归档史诗”分组

1. `ui-layout-migration`  
   覆盖：`ui-scene-split`、`rimworld-modular-ui`、`figma-make-ui-skeleton`、`main-layout-switch`、`html-reference-ui-parity`、`reference-layout-rebuild`、`reference-layout-update-b`、`responsive-layout-fix`
2. `map-tabs-and-viewport`  
   覆盖：`map-switch-tabs`、`mapviewport-center-content-tabs`、`three-map-render-zoom`
3. `countytown-25d-map`  
   覆盖：`countytown-25d-generator`、`countytown-25d-textured-render`
4. `han-courtyard-theme-pack`  
   覆盖：`han-courtyard-theme`、`han-courtyard-textured-theme`、`lantern-hover-pulse`

> 说明：历史文件不强制重命名；后续新增改动直接按史诗 topic 归档，减少同义命名扩散。若 topic 含旧世界观词汇，优先在正文和索引层补“当前语义解释”，不强行改历史 topic。

## 3) 新增文档命名规范（从本次起）

- 功能卡：`FC-YYYYMMDD-<canonical-topic>[-NN].md`
- 改动提案：`CP-YYYYMMDD-<canonical-topic>[-NN].md`
- 平衡日志：`BL-YYYYMMDD-<canonical-topic>[-NN].md`

`<canonical-topic>` 要求：

- 全小写，短横线分词
- 同一改动链路固定不变（FC/CP/BL 使用同一 topic）
- 默认不加 `-NN`；仅当“同目录 + 同日期 + 同 topic”会重名时，使用 `-01/-02` 序号

## 4) 执行规则（精简）

1. 机制或平衡改动：必须落 FC + CP + BL。
2. 纯表现层改动：至少落 FC + BL；若影响玩法感知阈值，补 CP。
3. 每月整理一次注册表，把“演进/历史”归并到对应史诗。



