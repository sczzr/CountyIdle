# 改动提案：战略地图配置覆盖补强与默认源裁定

## 提案信息

- 标题：战略地图配置覆盖补强与默认源裁定
- 日期：2026-04-20
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：
  - `DL-001` 虽已具备配置/生成双轨切换，但 `strategic_maps.json` 仍偏早期样例，世界图与江陵府外域缺少足够的轮廓、河流、标签与节点类型，切到配置源时地图可读性不足。
  - 世界图当前主链已经绑定 `XianxiaWorldMapData`、world hex 点选、二级地图入口与 L1 地貌贴图；若在模型尚未扩展前直接默认切回配置源，会把地图从“可交互战略图”降回“静态点线图”。
- 证据（数据/玩家反馈）：
  - `docs/05_feature_inventory.md` 与 `docs/08_development_list.md` 都把 `DL-001` 标为“配置驱动仍需继续收口”。
  - `StrategicMapViewSystem` 当前 `_useConfigDefinition` 默认仍为 `false`，且世界图配置源不会生成 `_xianxiaWorldMap`，因此不能提供 world-site 点选与二级地图入口。

## 改动内容

- 改什么：
  - 补强 `strategic_maps.json` 的世界图与江陵府外域配置覆盖，使其能承载区域、外轮廓、路线、河流、节点类型与文本标签。
  - 让江陵府外域备用图默认使用配置源，形成至少一条默认配置驱动链。
  - 保持世界图默认继续使用程序生成源，并在文档中明确这是“当前主链裁定”，不是遗留未清。
- 不改什么：
  - 不移除 `XianxiaWorldGeneratorSystem`。
  - 不把世界图的 hex cell / site / 地貌家族 / 二级入口直接压成当前 `strategic_maps.json` 的静态点线结构。
  - 不改二级地图、外务历练或战斗结算规则。
- 影响系统：
  - `CountyIdle/data/strategic_maps.json`
  - `CountyIdle/scenes/ui/WorldPanel.tscn`
  - `StrategicMapConfigSystem`
  - `StrategicMapViewSystem`
  - `docs/05_feature_inventory.md`
  - `docs/08_development_list.md`

## 预期结果

- 预期提升指标：
  - 配置源地图从“可切换样例图”提升为“有足够可读性的战略图”。
  - 江陵府外域备用图默认即可验证配置驱动，不再必须先走调试切换。
  - 文档层明确“世界图为什么还不默认切回配置源”，减少后续误判为漏改。
- 可接受副作用：
  - 世界图在本轮结束后仍保持“生成源默认、配置源可调试切换”的双轨状态。
  - 需要额外补一轮后续模型扩展，才能真正评估世界图默认切回配置源。

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - JSON 解析检查 `strategic_maps.json`
  - Godot headless 启动 `res://scenes/ui/WorldPanel.tscn`
  - 观察 `PrefectureMapView` 是否默认 `_useConfigDefinition = true`
- 观察周期：本轮开发内完成
- 成功判定阈值：
  - 构建通过；
  - `WorldPanel` 可启动；
  - 配置文件字段覆盖补全；
  - 文档将世界图默认源裁定写清楚。

## 回滚条件

- 触发条件：
  - 配置扩充导致 `WorldPanel` 解析失败；
  - 外域备用图切到配置源后无法正常显示；
  - 文档裁定与实际运行链路不一致。
- 回滚步骤：
  1. 回退 `strategic_maps.json` 本轮新增字段；
  2. 回退 `WorldPanel.tscn` 中 `PrefectureMapView` 的 `_useConfigDefinition` 默认值；
  3. 回退 `05 / 08 / FC / BL` 的本轮裁定文本。
