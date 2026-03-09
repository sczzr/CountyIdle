# 功能卡：卷轴子面板家族统一收口（弟子谱 / 峰令谱）

## 功能信息

- 功能名：卷轴子面板家族统一收口（弟子谱 / 峰令谱）
- 优先级：`P1`
- 目标版本：`v0.x`
- 关联系统：`DisciplePanel / SectOrganizationPanel / MainDisciplePanel / MainSectOrganizationPanel`

## 目标与用户收益

- 目标：将剩余两类独立卷册 `弟子谱` 与 `峰令谱` 完整纳入《浮云宗山河卷》同一套卷轴子面板规范。
- 玩家可感知收益（10 分钟内）：无论打开弟子谱、峰令谱、治宗册、留影录、机宜卷还是库房账册，都会获得一致的木轴、宣纸、卷首横题与墨线批令沉浸感，不再出现“古风内容 + 半卡片壳子”的割裂。

## 实现范围

- 包含：
  - 为 `DisciplePanel` 补齐左右木轴、上下绫边、卷首横题与“峰内名录 / 宣纸档案”卷面结构
  - 为 `SectOrganizationPanel` 补齐同族卷轴外壳，并将标题正式收口为“峰令谱”
  - 将峰令谱内卡片、治理按钮与关闭控件收口到更统一的墨线批令语义
  - 同步 `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`
- 不包含：
  - 不改弟子派生、地图联动、峰脉推荐、协同峰令或治理跳转逻辑
  - 不新增新的治理条目、过滤条件或存档字段

## 实现拆解

1. 比对剩余弹窗与已统一的 `治宗册 / 留影录 / 机宜卷 / 库房账册`
2. 为 `DisciplePanel.cs` 与 `SectOrganizationPanel.cs` 补齐卷轴子面板外壳
3. 收口标题、关闭控件、按钮与卡片语汇
4. 更新 `docs/02 / 05 / 08`
5. 执行 `dotnet build .\\Finally.sln`

## 验收标准（可测试）

- [x] `DisciplePanel` 与 `SectOrganizationPanel` 使用统一的卷轴子面板外壳
- [x] 两个弹窗保留原有交互与逻辑链路，不改 `GameState`、小时结算与存档结构
- [x] `dotnet build .\\Finally.sln` 通过

## 风险与回滚

- 风险：这两块 UI 都是动态构建，补壳后需要在 Godot 运行态确认极端窗口尺寸下的留白与滚动可读性。
- 回滚方式：仅回退 `DisciplePanel.cs`、`SectOrganizationPanel.cs` 的卷轴外壳与样式改动，保留弟子名册、峰脉浏览与协同峰逻辑。
