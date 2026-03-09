# 功能卡：弟子谱详情卡视觉重构

- 功能名：弟子谱详情卡视觉重构
- 任务 ID：`DL-043`
- 目标（玩家价值）：让弟子详情不再只是表格式属性堆叠，而是以更贴近修仙题材的暗色卡片视图呈现，降低阅读负担并强化个体辨识度。
- 飞轮环节：人口繁衍 → 职司分化 → 反哺宗门
- 依赖：`DiscipleProfile.cs`、`DiscipleRosterSystem.cs`、`DisciplePanel.cs`、`MainDisciplePanel.cs`、`WorldPanel.tscn`、`C:/Users/misoda/Downloads/ai_studio_code (2).html`

## 交付范围

- 保留弟子谱既有入口、筛选、排序与地图联动；
- 将详情区重构为暗色修仙卡片布局，头部显示头像框、身份徽章、修为徽章与骨龄 / 职司摘要；
- 主体左侧以八维雷达图承载 `气血 / 心境 / 潜力 / 战力 / 匠艺 / 悟性 / 执行 / 贡献`；
- 主体右侧拆分为“当前状况 / 个人特征 / 培养建议”三组信息卡；
- 指标值改为独立数值卡，避免雷达图之外缺少精确读数。

## 完成标准（DoD）

- [x] 弟子谱右侧详情区切换为暗色卡片视图；
- [x] 详情头部显示头像框、身份徽章、修为徽章与骨龄 / 职司摘要；
- [x] 雷达图与数值卡同时展示八维属性；
- [x] 当前状况、个人特征、培养建议拆为独立信息卡；
- [x] 名册筛选、排序、地图联动与弹窗关闭链路保持可用；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [ ] `dotnet build .\Finally.sln` 通过（当前被 `WarehousePanel.cs` 既有编译错误阻塞，需另行修复）。
