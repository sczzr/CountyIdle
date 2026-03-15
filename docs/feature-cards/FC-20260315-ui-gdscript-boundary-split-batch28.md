# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 左侧地块检视器表现层 GDScript 第二十八批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`MainSectTileInspector`

## 目标与用户收益

- 目标：在左侧地块检视器的 tone 早已由 `TileInspectorVisualFx.gd` 承接的前提下，继续回收 `MainSectTileInspector.cs` 中三类按钮绑定的薄壳 setter 与 disabled binding 构造重复。
- 玩家可感知收益（10 分钟内）：检视器表现与动作保持不变，但 `C#` 侧动作绑定链更集中，后续维护按钮语义与空态文案时更不容易出现分支漂移。

## 实现范围

- 包含：
  - 将三类按钮动作写入统一收口到单一 helper
  - 将 `TileInspectorAction.None` 的 disabled binding 重复构造统一收口到 helper
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改 world-site / 山门地块检视文案、按钮行为、tooltip、hint 与 tone 语义
  - 不改 `TileInspectorVisualFx.gd` 的表现参数
  - 不改 `GameState`、地图选择链与小时结算

## 实现拆解

1. 复查左侧地块检视器三类按钮绑定链中的薄壳 setter
2. 抽出统一 helper 收口按钮动作写入与 disabled binding 构造
3. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `MainSectTileInspector.cs` 不再保留三组薄壳按钮动作 setter
- [ ] `TileInspectorAction.None` 的 disabled binding 重复构造已统一收口
- [ ] 左侧检视器的摘要文案、按钮、tooltip、hint 与 tone 调用保持不变
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 helper 收口时传错目标按钮或动作字段，可能导致检视器按钮文案与实际行为不匹配
- 回滚方式：恢复 `MainSectTileInspector.cs` 中本批回收前的三组 setter 与 disabled binding 写法即可
