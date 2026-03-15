# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 表现层 GDScript 第二十九批收尾巡检
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`MainSectTileInspector`、`MainMapOperationalLink`、`MainWorldSitePanel`

## 目标与用户收益

- 目标：在连续二十八批 UI 表现层拆分 / 回收后，对剩余候选 C# UI 文件做最终巡检，只补做最后一个安全的尾差清理，并正式确认哪些残留必须继续保留在 `C#`。
- 玩家可感知收益（10 分钟内）：现有界面表现与交互不变，但这条拆分线会以明确边界收尾，后续继续维护地图调度、左侧检视器与多卷册 UI 时，不会再误把 authority 逻辑或业务可见性切到 `GDScript`。

## 实现范围

- 包含：
  - 复核剩余 UI / 地图调度 / 检视链候选文件中的纯表现残留
  - 将 `MainSectTileInspector.cs` 中末端按钮 helper 的无效 nullable 防御收紧为非空签名
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改 tooltip 文案、动作 hint、按钮启用条件与 world-site / 山门地块业务语义
  - 不改 `Visible` 业务切换、地图 layer 可见性 / 缩放 / 位置控制
  - 不改地图绘制 authority、存档读档、生成规则与数据绑定链

## 实现拆解

1. 复核剩余九个 UI 候选文件中仍留在 `C#` 的分支与 helper
2. 仅对已确认安全的末端 helper 做最小收口
3. 明确记录“不再继续拆”的边界并构建验证

## 验收标准（可测试）

- [ ] `MainSectTileInspector.cs` 中末端按钮 helper 已收紧为非空 `Button` 签名
- [ ] 经巡检确认剩余 tooltip / `Visible` / render authority / 数据绑定残留继续保留在 `C#`
- [ ] `docs/05_feature_inventory.md` 与 `docs/08_development_list.md` 已记录本轮收尾结论
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若误把业务语义的 `Visible` 切换、按钮禁用条件或地图 authority 当成纯表现层继续下放，可能导致界面状态与实际逻辑脱节
- 回滚方式：恢复 `MainSectTileInspector.cs` 的 helper 签名与本轮文档回写，并按本卡列出的保留边界重新审视
