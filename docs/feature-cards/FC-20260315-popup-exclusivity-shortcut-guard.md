# 功能卡（Feature Card）

## 功能信息

- 功能名：卷册弹窗排他与快捷键门禁收口
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`Main`、`SettingsPanel`、`WarehousePanel`、`TaskPanel`、`DisciplePanel`、`SectOrganizationPanel`、`SaveSlotsPanel`

## 目标与用户收益

- 目标：统一主界面卷册类弹窗的排他策略，并补全全局快捷键在卷册打开状态下的门禁，避免多卷叠层和误触全局操作。
- 玩家可感知收益（10 分钟内）：从底栏或快捷键打开任一卷册时，不会再与其他卷册长时间叠层；阅读治宗册、弟子谱、峰令谱时，也不会误触全局设置、仓储、速录或倍率入口。

## 实现范围

- 包含：
  - 给 `设置卷 / 仓储卷 / 治宗册 / 弟子谱 / 峰令谱 / 留影录` 增加统一的主界面排他收口
  - 补齐全局快捷键在上述卷册可见状态下的统一门禁
  - 回写 `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`
- 不包含：
  - 不恢复历史 `Prefecture / Event / Report / Expedition` 页签
  - 不改地图渲染 authority、世界图二级地图数据链与存档结构
  - 不改各卷册内部的业务逻辑、提示文案与 `VisualFx` 表现参数

## 实现拆解

1. 在 `Main` 层增加统一的卷册弹窗排他 helper
2. 给现行卷册面板补公共收卷入口，供主界面协调层调用
3. 补全 `_UnhandledInput/_UnhandledKeyInput` 的卷册可见门禁
4. 执行场景烟测与构建验证

## 验收标准（可测试）

- [ ] 从主界面打开任一卷册时，会先收起其他已打开的卷册弹窗
- [ ] `设置卷 / 仓储卷 / 留影录 / 治宗册 / 弟子谱 / 峰令谱` 任一可见时，全局快捷键默认让行
- [ ] 现有卷册按钮、关闭链、`Esc` 收卷与快速按钮 pressed 状态保持可用
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若排他 helper 误关了目标卷册或收卷顺序错误，可能造成按钮状态与实际可见面板不同步
- 回滚方式：恢复 `MainPopupCoordination.cs`、各 `Open...Panel()` 调用点与快捷键门禁修改，并回退本卡对应文档记录
