# 功能卡：修仙控件 SVG 主题图标包

## 功能信息

- 功能名：修仙控件 SVG 主题图标包（复选 / 单选 / 滑条 / 下拉 / 开关）
- 优先级：`P1`
- 目标版本：`2026-03-19`
- 关联系统：全局 UI Theme、`HanCourtyardTheme.tres`、`xianxia.tres`、`SettingsPanelVisualFx.gd`、控件图标资源

## 目标与用户收益

- 目标：为 Godot 原生表单控件补入统一的修仙科幻 SVG 图标资源，替换默认的塑料质感 checkbox / radio / slider grabber / option arrow / toggle icon。
- 玩家可感知收益（10 分钟内）：设置卷、留影录、弟子谱等界面的滑条与下拉箭头会立即改成更统一的暗金 / 灵气青图标，后续新增 `CheckBox` / `CheckButton` / `Tree` 也可自动复用同套主题资源。

## 实现范围

- 包含：新增 SVG 贴图资源；全局 Theme 与备用修仙主题的控件图标槽位接线；`SettingsPanelVisualFx.gd` 改为复用共享 slider 图标；补文档说明。
- 不包含：整套全局主题切换、卷册配色大改、玩法逻辑或数值平衡调整。

## 实现拆解

1. 在 `CountyIdle/assets/ui/icons/` 下新增 checkbox / radio / slider / arrow / toggle 的 SVG 资源，并用 Godot 兼容的层叠描边表现发光感。
2. 将当前项目实际启用的 `CountyIdle/themes/HanCourtyardTheme.tres` 与备用的 `CountyIdle/themes/xianxia.tres` 同步接入 `OptionButton`、`HSlider/VSlider`、`Tree`、`CheckBox`、`CheckButton` 的图标资源。
3. 调整 `CountyIdle/scripts/ui/gd/SettingsPanelVisualFx.gd`，避免设置卷继续覆盖为运行时纯色方块手柄，统一改用共享主题贴图。

## 验收标准（可测试）

- [ ] `CountyIdle/assets/ui/icons/` 下存在可复用的 SVG 资源，命名和用途清晰。
- [ ] `HanCourtyardTheme.tres` 与 `xianxia.tres` 已为 `OptionButton`、`HSlider/VSlider`、`Tree` 接入自定义图标，当前运行界面或后续切换主题时都不再依赖 Godot 默认箭头 / grabber。
- [ ] `SettingsPanelVisualFx.gd` 不再生成纯色 slider 手柄，而是复用共享 SVG 资源。

## 风险与回滚

- 风险：Godot 对 SVG 高级滤镜支持有限，若直接使用 `drop-shadow` 可能导入效果不稳定；不同卷册的局部 `VisualFx` 覆盖也可能压住全局主题图标。
- 回滚方式：回退 `HanCourtyardTheme.tres`、`xianxia.tres`、`SettingsPanelVisualFx.gd` 与新增的 `assets/ui/icons/*.svg` 即可恢复到默认控件图标。
