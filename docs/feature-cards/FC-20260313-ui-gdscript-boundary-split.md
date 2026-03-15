# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 首批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`WarehousePanel`、`SaveSlotsPanel`、`CountyTownMapViewSystem`

## 目标与用户收益

- 目标：在不改动权威玩法逻辑、存档格式与小时结算链路的前提下，将首批纯表现层逻辑拆到 `GDScript`，建立 `C# 管规则 / GDScript 管表现` 的边界。
- 玩家可感知收益（10 分钟内）：仓储卷、留影录与山门 hex 地图的打开 / 预览 / hover 反馈更顺滑，界面层次更清晰。

## 实现范围

- 包含：
  - 为 `WarehousePanel` 接入开场与分页切换表现脚本；
  - 为 `SaveSlotsPanel` 接入预览区切换与选中脉冲表现脚本；
  - 为 `CountyTownMapViewSystem` 接入 hex hover 高亮表现脚本；
  - 补齐场景节点与 `C# -> GDScript` 的单向调用边界；
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`。
- 不包含：
  - 不改人口、产业、战斗、研究、存档等权威公式；
  - 不改 `GameLoop`、`GameState`、`SaveSystem`、`SqliteSaveRepository`；
  - 不引入 `C++ / Rust` 运行时扩展。

## 实现拆解

1. 新增 `WarehousePanelTransition.gd`、`SavePreviewCrossfade.gd`、`HexHoverHighlight.gd`
2. 在对应 `.tscn` 中挂接辅助节点，保持主脚本仍为 `C#`
3. 由 `C#` 在面板打开、分页切换、预览更新、地图 hover 时单向调用表现脚本

## 验收标准（可测试）

- [ ] 打开仓储卷时有轻量开场动画，切换分页时有视觉反馈，且不影响库存数据刷新
- [ ] 选择留影录槽位时，预览框会做轻量淡入 / 脉冲反馈，且不影响存读档逻辑
- [ ] 山门 hex 地图随鼠标移动出现 hover 高亮，缩放 / 平移后高亮位置仍正确
- [ ] `C#` 仍是权威逻辑源，`GDScript` 不直接修改 `GameState`、存档与公式
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：场景节点路径变更后，表现脚本可能失效；地图 hover 可能在缩放 / 重绘时出现错位
- 回滚方式：移除新增 `GDScript` 节点与调用，保留现有 `C#` 逻辑链即可恢复
