# 功能卡：宗门弟子独立属性界面（弟子谱）

- 功能名：宗门弟子独立属性界面（弟子谱）
- 任务 ID：`DL-043`
- 目标（玩家价值）：让玩家能在独立面板中浏览弟子名册、查看个体属性与当前差事，降低“只看总量不知个体状态”的理解门槛。
- 飞轮环节：人口繁衍 → 职司分化 → 反哺宗门
- 依赖：`GameState`、`PopulationRules.cs`、`SectGovernanceRules.cs`、`SectOrganizationRules.cs`、`DiscipleRosterSystem.cs`、`DisciplePanel.cs`、`Main.cs`、`WorldPanel.tscn`

## 交付范围

- 主地图页签行新增“弟子谱”入口；
- 新增独立弹窗，展示整册弟子列表与个体详情；
- 天衍峰山门图点击可视弟子或实体场所时，可直接联动打开“弟子谱”并定位代表弟子；
- 支持按真传 / 阵材 / 阵务 / 外事 / 推演 / 待命筛选；
- 支持按修为 / 潜力 / 心境 / 贡献排序；
- 详情显示年龄、修为、当前差事、居所、关联峰脉、特征、培养建议，以及 `气血 / 心境 / 潜力 / 战力 / 匠艺 / 悟性 / 执行 / 贡献` 八项属性。

## 完成标准（DoD）

- [x] `WorldPanel` 顶栏有单独“弟子谱”按钮，并能打开 / 关闭弹窗；
- [x] 弹窗内可浏览弟子名册，并能切换筛选与排序；
- [x] 选中弟子后可查看年龄、修为、当前差事、峰脉与八项属性详情；
- [x] 天衍峰山门图点击弟子或场所时，可直接联动弟子谱并定位代表弟子；
- [x] 名册由 `GameState.Clone()` 派生生成，不直接改写小时结算与存档；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [x] `dotnet build .\Finally.sln` 通过。
