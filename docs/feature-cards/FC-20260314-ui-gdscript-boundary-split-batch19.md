# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十九批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`StrategicMapViewSystem`、`StrategicMapPanelToneFx`

## 目标与用户收益

- 目标：把世界图 `WorldTerrainTileLayer` 随地图态势变化的纯视觉 tint 继续从 `C#` 下放到局部 `GDScript` helper，让 `StrategicMapViewSystem.cs` 只保留 layer 的可见性、位置、缩放与地图绘制。
- 玩家可感知收益（10 分钟内）：世界图底盘仍会随当前态势变化 tint，但后续微调该视觉色调时不需要再进地图绘制业务脚本。

## 实现范围

- 包含：
  - 扩展 `StrategicMapPanelToneFx.gd` 承接 `WorldTerrainTileLayer` 的 tint
  - 简化 `StrategicMapViewSystem.cs` 中世界图 terrain layer 的直接着色代码
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改世界图 hex 投影、tile cell 布局、可见性、位置与缩放逻辑
  - 不改态势评分、世界站点选择、地图输入与存档结构
  - 不改山门图 / 外域图其他渲染规则

## 实现拆解

1. 复查 `StrategicMapViewSystem.cs` 中仍在活跃链上的 tile layer 纯视觉着色
2. 扩展 `StrategicMapPanelToneFx.gd` 承接世界图 terrain tint
3. 让 `StrategicMapViewSystem.cs` 仅通过单向调用触发表现层
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `StrategicMapPanelToneFx.gd` 已承接 `WorldTerrainTileLayer` 的地图态势 tint
- [ ] `StrategicMapViewSystem.cs` 不再直接使用 `_worldTerrainTileLayer.Modulate`
- [ ] 世界图 tile layer 的可见性、位置、缩放与地图绘制仍保留在 `C#`
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 helper 对 `WorldTerrainTileLayer` 的节点获取失败，世界图底盘可能丢失态势 tint
- 回滚方式：恢复 `StrategicMapViewSystem.cs` 中的直接 `_worldTerrainTileLayer.Modulate` 代码，并移除 helper 中新增的 terrain tint 方法
