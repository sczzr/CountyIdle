# 功能卡：宗门组织谱系详情浏览（JobsList 二期）

- 功能名：宗门组织谱系详情浏览（JobsList 二期）
- 任务 ID：`DL-040`
- 目标（玩家价值）：让玩家能在 `JobsList` 内切换浏览九峰与附属部门详情，不再只停留在总览级别。
- 飞轮环节：反哺宗门
- 依赖：`docs/09_xianxia_sect_setting.md`、`D:/Files/Novel/asset/qidian/cangxuan/浮云宗-天衍峰.md`、`SectOrganizationRules.cs`、`Main.cs`、`JobsPanel.tscn`

## 交付范围

- 在 `JobsList` 底部新增“峰脉详情浏览”块；
- 支持 `上一峰 / 下一峰` 在九峰与其余支柱峰之间切换；
- 点击四条主职司时自动聚焦关联峰脉；
- 峰脉详情显示定位、核心机构、职责与处室 / 附属部门说明。

## 完成标准（DoD）

- [x] `JobsList` 内可切换浏览九峰及其余支柱峰详情；
- [x] 行点击除展开职司摘要外，还会自动跳转到推荐峰脉；
- [x] 天枢峰、天机峰、天工峰、天权峰、天元峰、天衡峰的处室级内容已在详情中体现；
- [x] `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已同步；
- [x] `dotnet build .\Finally.sln` 通过。

## 2026-03-09 迁移补记

- 为适配主界面 `Tile Inspector` 单链路，峰脉详情浏览已从主界面左栏迁入独立 `SectOrganizationPanel` 卷册；
- 打开入口改为底部 `【峰令】谱系` 快捷按钮，生命周期接线由 `Main.cs + MainSectOrganizationPanel.cs` 维护；
- 浏览逻辑、推荐峰脉定位与峰系详情内容保持不变，只调整承载界面与入口位置。
