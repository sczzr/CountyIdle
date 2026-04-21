# 功能卡：战略地图配置覆盖补强（DL-001 收口）

## 功能信息

- 功能名：战略地图配置覆盖补强
- 关联提案：`docs/change-proposals/CP-20260420-strategic-map-config-coverage.md`
- 优先级：`P1`
- 目标版本：`2026.04`
- 关联系统：`StrategicMapConfigSystem`、`StrategicMapViewSystem`、`WorldPanel.tscn`、`strategic_maps.json`

## 目标与用户收益

- 目标：让 `data/strategic_maps.json` 不再只是早期点线样例，而能覆盖标题、轮廓、区域、路线、河流、节点类型与文本标签，并让江陵府外域备用图默认走配置源。
- 玩家可感知收益（10 分钟内）：打开外域备用图或用调试入口切到配置源时，能看到有地名、灵脉/河道、据点类型与附庸坊城层次的可读地图，而不是无名点线图。

## 实现范围

- 包含：
  - 补强 `strategic_maps.json` 的世界图配置：新增外轮廓、河流、更多节点、节点类型与地名标签。
  - 补强 `strategic_maps.json` 的江陵府外域配置：为节点补 `city / ward / settlement / raw_source / landmark` 类型，并补可缩放地名标签。
  - 将 `PrefectureMapView` 默认切到配置定义源，形成至少一条默认配置驱动的战略图链路。
  - 明确世界图暂不默认切回配置源，因为当前程序生成源仍承载世界 hex 点选、二级地图入口、L1 地貌贴图与世界格数据。
- 不包含：
  - 不移除 `XianxiaWorldGeneratorSystem`。
  - 不把世界图的 `XianxiaWorldMapData` 序列化进 `strategic_maps.json`。
  - 不改变外务历练、二级地图遭遇或战斗结算。

## 实现拆解

1. 补充世界图与江陵府外域配置字段覆盖。
2. 让江陵府外域备用图默认使用配置源，并继续保留调试按钮 / `F8` 双轨切换。
3. 回写 `05 / 08` 与平衡日志，记录“世界图暂不默认切回”的裁定原因。

## 验收标准（可测试）

- [x] `strategic_maps.json` 的 `world` 同时包含 `regions / outlines / routes / rivers / nodes / labels`。
- [x] `strategic_maps.json` 的 `prefecture` 节点包含城市、坊区、原料点、聚落与地标类型。
- [x] `PrefectureMapView` 默认导出为配置源。
- [x] `dotnet build .\Finally.sln` 通过。
- [x] Godot 4.6 headless 补验 `res://scenes/ui/WorldPanel.tscn`，确认配置图可启动且无资源解析错误。

## 风险与回滚

- 风险：世界图配置源仍是静态战略图，不具备生成世界图的 hex 点选与二级地图入口；若直接默认切回，会损失当前主玩法入口。headless 已确认场景可启动，但仍不能替代 F5 中的调试按钮切换与视觉差异走查。
- 回滚方式：恢复 `WorldPanel.tscn` 中 `PrefectureMapView` 的 `_useConfigDefinition` 默认值，并回退 `strategic_maps.json` 本轮新增字段。
