# CountyIdle 归档批量重命名迁移记录

> 本文是归档重命名迁移的历史执行记录。  
> 它负责保留“当时改了什么、按什么规则改、如何追溯”；不负责继续充当当前执行计划。
>
> 当前归档命名与 topic 收敛规则，以 [06_archive_registry.md](/E:/2_Personal/Finally/docs/06_archive_registry.md) 为准。  
> 本记录对应的批量重命名已于 `2026-03-06` 执行完成（共重命名 `32` 个文件）。
>
> 世界观兼容说明（2026-03-08）：本记录只规范文件名，不要求把历史 `canonical-topic` 从 `county / countytown / prefecture` 强行改成修仙语义；当前做法是保留 topic 追溯性，并在索引/正文中补“宗门 / 外域”解释。

## 0. 文档地位

- 本文是一次已完成迁移的执行归档，不是当前待执行任务单。
- 后续若再次发生批量迁移，应另开新记录，不在本文追加新的待办计划。
- 若当前命名规则与本文历史做法不一致，以 `06_archive_registry.md` 的现行规则优先。

## 1. 迁移原则

1. 仅改文件名，不改文档正文内容。
2. 历史文件按“史诗 topic”归并，保留日期前缀。
3. 出现同目录同日同 topic 重名时，使用 `-01/-02/...` 序号后缀。
4. 先 `FC`，再 `CP`，最后 `BL`，便于链路核验。

## 2. 目标命名规则

- `FC-YYYYMMDD-<canonical-topic>[-NN].md`
- `CP-YYYYMMDD-<canonical-topic>[-NN].md`
- `BL-YYYYMMDD-<canonical-topic>[-NN].md`

## 3. 批量重命名映射（已执行）

### 3.1 Feature Cards（FC）

| 旧文件名 | 新文件名 |
| --- | --- |
| `FC-20260305-han-courtyard-theme.md` | `FC-20260305-han-courtyard-theme-pack-01.md` |
| `FC-20260305-han-courtyard-textured-theme.md` | `FC-20260305-han-courtyard-theme-pack-02.md` |
| `FC-20260305-lantern-hover-pulse.md` | `FC-20260305-han-courtyard-theme-pack-03.md` |
| `FC-20260305-rimworld-modular-ui.md` | `FC-20260305-ui-layout-migration-01.md` |
| `FC-20260305-ui-scene-split.md` | `FC-20260305-ui-layout-migration-02.md` |
| `FC-20260306-figma-make-ui-skeleton.md` | `FC-20260306-ui-layout-migration-03.md` |
| `FC-20260306-main-layout-switch.md` | `FC-20260306-ui-layout-migration-04.md` |
| `FC-20260306-html-reference-ui-parity.md` | `FC-20260306-ui-layout-migration-05.md` |
| `FC-20260306-reference-layout-rebuild.md` | `FC-20260306-ui-layout-migration-06.md` |
| `FC-20260306-reference-layout-update-b.md` | `FC-20260306-ui-layout-migration-07.md` |
| `FC-20260306-responsive-layout-fix.md` | `FC-20260306-ui-layout-migration-08.md` |
| `FC-20260306-map-switch-tabs.md` | `FC-20260306-map-tabs-and-viewport-01.md` |
| `FC-20260306-mapviewport-center-content-tabs.md` | `FC-20260306-map-tabs-and-viewport-02.md` |
| `FC-20260306-three-map-render-zoom.md` | `FC-20260306-map-tabs-and-viewport-03.md` |
| `FC-20260306-countytown-25d-generator.md` | `FC-20260306-countytown-25d-map-01.md` |
| `FC-20260306-countytown-25d-textured-render.md` | `FC-20260306-countytown-25d-map-02.md` |

### 3.2 Change Proposals（CP）

| 旧文件名 | 新文件名 |
| --- | --- |
| `CP-20260306-countytown-25d-generator.md` | `CP-20260306-countytown-25d-map.md` |

### 3.3 Balance Logs（BL）

| 旧文件名 | 新文件名 |
| --- | --- |
| `BL-20260305-han-courtyard-theme.md` | `BL-20260305-han-courtyard-theme-pack-01.md` |
| `BL-20260305-han-courtyard-textured-theme.md` | `BL-20260305-han-courtyard-theme-pack-02.md` |
| `BL-20260305-lantern-hover-pulse.md` | `BL-20260305-han-courtyard-theme-pack-03.md` |
| `BL-20260305-rimworld-modular-ui.md` | `BL-20260305-ui-layout-migration-01.md` |
| `BL-20260305-ui-scene-split.md` | `BL-20260305-ui-layout-migration-02.md` |
| `BL-20260306-figma-make-ui-skeleton.md` | `BL-20260306-ui-layout-migration-03.md` |
| `BL-20260306-main-layout-switch.md` | `BL-20260306-ui-layout-migration-04.md` |
| `BL-20260306-html-reference-ui-parity.md` | `BL-20260306-ui-layout-migration-05.md` |
| `BL-20260306-reference-layout-rebuild.md` | `BL-20260306-ui-layout-migration-06.md` |
| `BL-20260306-reference-layout-update-b.md` | `BL-20260306-ui-layout-migration-07.md` |
| `BL-20260306-responsive-layout-fix.md` | `BL-20260306-ui-layout-migration-08.md` |
| `BL-20260306-map-switch-tabs.md` | `BL-20260306-map-tabs-and-viewport-01.md` |
| `BL-20260306-mapviewport-center-content-tabs.md` | `BL-20260306-map-tabs-and-viewport-02.md` |
| `BL-20260306-countytown-25d-generator.md` | `BL-20260306-countytown-25d-map-01.md` |
| `BL-20260306-countytown-25d-textured-render.md` | `BL-20260306-countytown-25d-map-02.md` |

## 4. 执行记录（2026-03-06）

1. 已按 3.1 -> 3.2 -> 3.3 顺序完成批量重命名。
2. 已同步修正文档引用（含提案路径更新）。
3. 已完成构建校验：`dotnet build .\\Finally.sln` 通过。
4. 后续新增文档继续遵循 `06_archive_registry.md` 的命名与去重规则。

## 5. 历史执行示例（PowerShell）

```powershell
# 示例：逐条替换为计划中的 old/new
git mv docs/feature-cards/FC-20260305-han-courtyard-theme.md docs/feature-cards/FC-20260305-han-courtyard-theme-pack-01.md
git mv docs/change-proposals/CP-20260306-countytown-25d-generator.md docs/change-proposals/CP-20260306-countytown-25d-map.md
git mv docs/balance-logs/BL-20260306-map-switch-tabs.md docs/balance-logs/BL-20260306-map-tabs-and-viewport-01.md
```

> 裁定说明：以上仅用于保留当次迁移的执行样式，不构成当前批处理脚本模板。

## 6. 迁移后预期收益

- 同一史诗链路可按 topic 聚类检索。
- 后续 FC/CP/BL 命名规则统一，减少重复开题。
- 历史资产交付类与机制类文档边界更清晰。
- 世界观升级时可在索引层补充兼容映射，而不破坏历史 topic 的可追溯性。
