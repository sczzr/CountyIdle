# 功能卡（Feature Card）

## 功能信息

- 功能名：机宜卷分类切换与客户端设置扩容（一期）
- 优先级：`P1`
- 目标版本：`v0.x`
- 关联系统：`UI`、`core`、`models`

## 目标与用户收益

- 目标：在现有机宜卷基础上，明确“万象声色 / 敕令符节 / 世间法则”三类设置语义，并按实机参考图把卷面排版、字段顺序与交互样式收拢到目标效果。
- 玩家可感知收益（10 分钟内）：切换页签时可看到与目标图一致的三页结构——音画页的长滑条与书法单选、符节页的分组九宫格、法则页的语言与三枚敕令开关。

## 实现范围

- 包含：
  - 设置卷页签标题辞书与卷首副标题更新
  - `ClientSettings` / `ClientSettingsSystem` 扩充更多客户端设置字段与规范化逻辑
  - 自动存档开关 / 间隔、速归确认、多声道音量的基础运行时接线
  - 设置卷场景文案统一到“世间法则”语义
  - 按参考图重排音画 / 符节 / 法则三页的卷面布局与可见字段
  - 将设计文档中的 `paper_grain / jade_slip_base / seal_character_chi / seal_button_shape` 四枚 SVG 正式落入项目并接到机宜卷背景、玉简页签、敕令开关与卷尾印章
  - 将玉简页签 / 朱砂敕令 / 卷尾印章抽成可复用组件场景（`JadeTab / SealToggle / SealButton`）
- 不包含：
  - 全部新字段在设置卷中的完整可视编辑控件
  - 新增音频总线或更细的画质运行时渲染策略
  - Godot `F5` 全流程实机视觉巡检

## 实现拆解

1. 扩充客户端设置模型、规范化逻辑与主界面应用入口。
2. 调整设置卷标题辞书与玩法页签文案，统一三分类语义。
3. 为自动存档与速归确认补基础行为接线，并执行 `dotnet build` 验证。

## 验收标准（可测试）

- [x] 设置卷切换到第二、三类页签时，卷首标题会切换到对应语义
- [x] `ClientSettings` 支持更多音画 / 游戏性字段并可通过规范化逻辑保存
- [x] 设置卷卷面已补入 `BGM / SFX / 画质 / 自动存档开关 / 自动存档间隔 / 速归确认` 的可编辑控件
- [x] 设置卷现已进一步按参考图收口：音画页使用长滑条 + 书法单选，符节页改为分组卡片格，法则页改为语言单选 + 三枚敕令开关
- [x] 设计文档给出的 `paper_grain / jade_slip_base / seal_character_chi / seal_button_shape` 四枚 SVG 已接入设置卷实际节点
- [x] `JadeTab.tscn`、`SealToggle.tscn`、`SealButton.tscn` 已落入可复用组件目录，并被 `SettingsPanel.tscn` 实例化复用
- [x] 速归初局在启用确认门禁时需要二次按键确认
- [x] `dotnet build .\Finally.sln` 通过
- [x] `Godot --headless --scene res://scenes/ui/SettingsPanel.tscn --quit-after 1` 通过

## 风险与回滚

- 风险：当前卷面已接入文档 SVG 素材、补做页签/印章细调并抽成复用组件，但 SVG 在不同 DPI / Godot SVG 渲染路径下仍可能出现细边、滤镜简化或轻微偏色，仍需后续 `F5` 实机微调；此外自动存档间隔与速归确认仍保留在数据层/隐藏控件中，后续需再裁定是否重新露出。
- 回滚方式：回退 `ClientSettings*`、`MainClientSettings.cs`、`MainSaveSlotsPanel.cs`、`MainShortcutBindings.cs` 与 `SettingsPanel.*` 本次改动。
