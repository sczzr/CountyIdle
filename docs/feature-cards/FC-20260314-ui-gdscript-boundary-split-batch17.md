# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十七批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`StrategicMapViewSystem`、`CountyTownMapViewSystem`

## 目标与用户收益

- 目标：把世界图 / 外域图标题与山门图 `MapHintLabel` 的地图态势色调继续从 `C#` 下放到局部 `GDScript` helper，让地图绘制脚本只保留态势快照、标题文案、提示文本与地图绘制。
- 玩家可感知收益（10 分钟内）：地图标题与山门图提示文字仍会随当前地图态势变化颜色，但后续微调这些表现层色调时不需要再改地图绘制业务脚本。

## 实现范围

- 包含：
  - 为 `WorldMapView` / `PrefectureMapView` / `CountyTownMapView` 增加局部 `ToneFx` 节点
  - 新增 `StrategicMapPanelToneFx.gd` 与 `CountyTownMapHintFx.gd`
  - 简化 `StrategicMapViewSystem.cs` / `CountyTownMapViewSystem.cs` 中对应 label 的直接着色代码
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改地图生成、世界地块绘制、山门图结构绘制与地形 tint 逻辑
  - 不改态势评分、标题文本、提示文本、输入行为与日志结果
  - 不改 `GameState`、存档结构与小时结算

## 实现拆解

1. 审查地图视图当前仍在运行链上的 label 着色残留
2. 为战略地图与山门图补上局部 `ToneFx` helper
3. 让 `C#` 仅保留态势快照与文本更新，通过单向调用触发视觉层
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `StrategicMapPanelToneFx.gd` 已承接 `WorldMapView` / `PrefectureMapView` 标题 label 的地图态势色调
- [ ] `CountyTownMapHintFx.gd` 已承接 `CountyTownMapView/MapHintLabel` 的地图态势色调
- [ ] `StrategicMapViewSystem.cs` / `CountyTownMapViewSystem.cs` 不再直接使用这些 label 的 `Modulate` 表现代码
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 `ToneFx` 节点路径或挂接位置不对，地图标题或山门图提示文字可能失去态势色调同步
- 回滚方式：恢复两份地图系统中的直接着色代码，并移除 `WorldPanel.tscn` 里新增的局部 `ToneFx` 节点与对应脚本
