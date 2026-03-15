# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十二批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`MainSectTileInspector`

## 目标与用户收益

- 目标：把主界面左侧地块检视器中与世界点位 / 山门地块相关的纯视觉 tone 切换继续从 `C#` 下放到 `GDScript`，让 `MainSectTileInspector.cs` 更专注于检视数据、动作绑定与说明文本。
- 玩家可感知收益（10 分钟内）：点选世界点位或山门地块时，左侧检视器的标题、徽签、副标题与状态字段配色仍保持一致，但后续微调色调时不需要再进入业务逻辑代码。

## 实现范围

- 包含：
  - 新增地块检视器视觉脚本，承接世界层 / 本地层的纯配色切换
  - 将 `MainSectTileInspector.cs` 中纯色调 override 逻辑迁出
  - 保持 `TownActivityAnchorVisualRules`、点位说明文本、按钮动作绑定与业务分流仍留在 `C#`
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改世界点位描述文案、按钮文案、地图进入逻辑与地块坊局规则
  - 不改 `TownActivityAnchorVisualRules` / `XianxiaSiteData` / `GameState`
  - 不额外改动二级地图生成、存档、小时结算与玩法规则

## 实现拆解

1. 审查 `MainSectTileInspector.cs` 中仍由 `C#` 直接写入的字体颜色 / 徽签色调
2. 新增 `GDScript` 视觉脚本并挂到 `Main.tscn`
3. 由 `C#` 保留语义判断与颜色来源，改为调用 `GDScript` 统一落到控件
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `MainSectTileInspector.cs` 不再直接承担世界点位 / 本地地块检视器的纯 `AddThemeColorOverride` 色调切换
- [ ] 新的 `GDScript` 视觉脚本已承接左侧检视器标题、副标题、徽签与状态值的纯视觉配色落点
- [ ] `C#` 仍保留按钮绑定、描述文案、badge 语义与规则判断
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若脚本挂载点或节点路径不对，可能导致左侧检视器配色不更新；若 `C# -> GDScript` 参数传递不全，可能出现世界层和本地层色调不一致
- 回滚方式：恢复 `MainSectTileInspector.cs` 中原有的颜色 override 逻辑，并移除新增的视觉脚本节点即可
