# 功能卡：弟子谱多层树状名册

- 功能名：弟子谱多层树状名册
- 任务 ID：`DL-043`
- 目标（玩家价值）：让各峰弟子不再只是平铺列表，而能在峰内按堂口、册级继续分层浏览，便于后续做任命、培养和管理。
- 飞轮环节：人口繁衍 → 职司分化 → 反哺宗门
- 依赖：`DiscipleProfile.cs`、`DiscipleRosterSystem.cs`、`DisciplePanel.cs`、`MainDisciplePanel.cs`

## 交付范围

- 左侧名册从手工分组列表改为真正的 Godot `Tree` 控件；
- 名册层级固定为：`峰脉 -> 堂口 / 机构 -> 册级 -> 弟子`；
- 点击树叶节点可切换右侧档案详情；
- 筛选与排序仍作用于整棵树的生成结果；
- 保持地图联动打开弟子谱并定位到对应弟子。

## 完成标准（DoD）

- [x] 左侧名册改为多层树状结构；
- [x] 各峰内可继续展开堂口与册级；
- [x] 点击树叶节点可刷新右侧弟子档案；
- [x] 筛选、排序和地图联动保持可用；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [x] `dotnet build .\Finally.sln` 通过。
