# 功能卡：弟子谱卷轴档案 UI 重构

- 功能名：弟子谱卷轴档案 UI 重构
- 任务 ID：`DL-043`
- 目标（玩家价值）：让弟子谱从现代卡片式面板进一步收敛为“宗门卷册 + 宣纸档案”的修仙档案页，更贴近题材氛围并提高角色阅读沉浸感。
- 飞轮环节：人口繁衍 → 职司分化 → 反哺宗门
- 依赖：`DiscipleProfile.cs`、`DiscipleRosterSystem.cs`、`DisciplePanel.cs`、`MainDisciplePanel.cs`、`WorldPanel.tscn`、`C:/Users/misoda/Downloads/ai_studio_code (10).html`

## 交付范围

- 保留弟子谱既有入口、筛选、排序与地图联动；
- 左侧名册改为纵向卷册风，按峰脉 / 册录分组显示弟子条目；
- 右侧详情改为宣纸档案页，顶部显示姓名、骨龄、册录信息与灵根圆环；
- 中部改为“根基罗盘 + 修为进度 / 战力印鉴 / 气海储备”双栏结构；
- 底部改为“性情印记 + 衍天批注”两段式信息区。

## 完成标准（DoD）

- [x] 弟子谱整体布局切到卷轴档案风；
- [x] 左侧名册支持分组卷册与当前选中高亮；
- [x] 右侧详情支持灵根圆环、根基罗盘、修为 / 战力 / 气海状态；
- [x] 特征以印记形式展示，建议改为衍天批注文案；
- [x] 筛选、排序、地图联动与弹窗关闭链路保持可用；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [x] `dotnet build .\Finally.sln` 通过。
