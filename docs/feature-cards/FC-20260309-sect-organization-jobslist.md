# 功能卡：宗门组织谱系展示（JobsList 对齐设定）

- 功能名：宗门组织谱系展示（JobsList 对齐设定）
- 任务 ID：`DL-039`
- 目标（玩家价值）：让 `JobsList` 不再像只有四条抽象职业，而是能看见浮云宗九峰、青云峰三总殿，以及天衍峰的附属部门体系。
- 飞轮环节：反哺宗门
- 依赖：`docs/09_xianxia_sect_setting.md`、`SectTaskRules`、`Main.cs`、`JobsPanel.tscn`

## 交付范围

- 为四条主体系补充“关联峰系 / 附属部门”说明；
- 在 `JobsList` 底部增加“九峰与附属部门”概览区；
- 组织谱系显示与 `09` 设定文档对齐，避免 UI 和文档割裂。

## 完成标准（DoD）

- [x] `JobsList` 内能看见青云峰三总殿、天衍峰主机构与多座协同峰；
- [x] 四条主体系的 Tooltip / 详情会明确对应峰系与部门；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [x] `dotnet build .\Finally.sln` 通过。
