# CountyIdle 历史归档注册表（整合精简）

> 目标：避免同主题反复建档，统一“功能卡/提案/日志”三件套追踪。

## 1) Canonical Topic 注册表

| Canonical Topic | 当前归档文件（按阶段） | 状态 | 合并建议 |
| --- | --- | --- | --- |
| `research-breakthrough` | FC/CP/BL 全量齐备 | 稳定 | 作为“机制类三件套”样板 |
| `industrial-job-logic` | FC/CP/BL 全量齐备 | 稳定 | 与 `ui-interaction-rewire` 联查 |
| `equipment-rarity-affix-drop` | FC/CP/BL 全量齐备 | 稳定 | 与探险系统共同评估 |
| `county-dynamic-events` | FC/CP/BL 全量齐备 | 稳定 | 保持独立 topic |
| `countytown-25d-generator` | FC/CP/BL 全量齐备 | 稳定 | 与 `countytown-25d-textured-render` 归并为同一史诗 |
| `countytown-25d-textured-render` | FC/BL（无 CP） | 演进 | 后续并入 `countytown-25d-generator` |
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
| `chinese-seamless-tileset-atlas` | FC（无 CP/BL） | 资产交付 | 保持独立资产 topic |
| `building-elements-pack` | FC（无 CP/BL） | 资产交付 | 保持独立资产 topic |
| `client-settings-panel` | FC（无 CP/BL） | 已实现 | 下次涉及机制联动时补 CP/BL |
| `prefecture-village-style-generator` | FC/CP/BL（本次新增） | 演进 | 并入战略地图史诗链路，后续扩展可复用同 topic |
| `population-allocation-lifecycle-commute` | FC/CP/BL（本次新增） | 演进 | 已完成一期落地，后续沿用同 topic 持续调参 |
| `countytown-resident-walkers` | FC/BL（本次新增） | 演进 | 并入 `countytown-25d-map` 史诗链路，后续可替换正式人物美术 |

## 2) 建议收敛的“归档史诗”分组

1. `ui-layout-migration`  
   覆盖：`ui-scene-split`、`rimworld-modular-ui`、`figma-make-ui-skeleton`、`main-layout-switch`、`html-reference-ui-parity`、`reference-layout-rebuild`、`reference-layout-update-b`、`responsive-layout-fix`
2. `map-tabs-and-viewport`  
   覆盖：`map-switch-tabs`、`mapviewport-center-content-tabs`、`three-map-render-zoom`
3. `countytown-25d-map`  
   覆盖：`countytown-25d-generator`、`countytown-25d-textured-render`
4. `han-courtyard-theme-pack`  
   覆盖：`han-courtyard-theme`、`han-courtyard-textured-theme`、`lantern-hover-pulse`

> 说明：历史文件不强制重命名；后续新增改动直接按史诗 topic 归档，减少同义命名扩散。

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

