# 功能卡：全景主操作 HUD（天地人一体）

## 功能信息

- 功能名：全景主操作 HUD（天地人一体）
- 优先级：`P1`
- 目标版本：`2026-03-21`
- 关联系统：`DL-097`、`Main.tscn`、`MainHUD.tscn`、`TopBar.tscn`、`BottomBar.tscn`、`JobsPanel.tscn`、`EventLogPanel.tscn`

## 目标与用户收益

- 目标：把原本割裂的顶栏、地图区、左右信息卷与底栏主操作收口为一套 `CanvasLayer` 全屏 HUD，让宗主在同一视野中同时掌握“天时 / 地利 / 人和”。
- 玩家可感知收益（10 分钟内）：进入主界面即可看到统一的节气资源顶栏、浮动地块检视卷、可展开的宗门纪事卷，以及令箭式底部主操作中枢；地图点击仍可用，不会被整层 HUD 阻断。

## 实现范围

- 包含：
  - 新增 `MainHUD.tscn` 作为主界面表现层 `CanvasLayer`
  - 顶栏重做为节气 / 日期 / 危兆 / 资源融合布局，并补天象玉佩摇摆动画
  - 左右浮窗承载现有院域营建卷与宗门纪事卷，改为悬浮式排布
  - 底栏重做为令箭式主操作中枢，接入 `库房 / 中枢 / 谱系 / 天工 / 政令` 五枚主令箭
  - 新增二级副卷弹出层与令箭拔出动画，保留地图可点击区域
  - 接入 `token_base / seal_mini / diamond_btn / jade_pendant` 四枚 HUD SVG 资产
- 不包含：
  - 不改动 `GameLoop` 结算顺序与核心数值规则
  - 不重写各卷册内部业务逻辑，只调整主界面入口与表现壳层
  - 不在本轮补完 Godot/F5 手动走查之外的完整美术 polish

## 实现拆解

1. 先在文档侧登记 `DL-097`，明确该 HUD 属于主操作页面整合而非单一弹窗改版。
2. 新增 `MainHUD.tscn`，把 `TopBar / BottomBar / JobsPanel / EventLogPanel` 组合为独立 `CanvasLayer`。
3. 重写 `TopBar.tscn` 与 `BottomBar.tscn`，接入天象玉佩、令箭主菜单、倍速珠与系统印位视觉。
4. 用 `CommandToken.gd` 与 `MainHudBottomBar.gd` 补齐拔出动画、副卷展开与底栏状态切换。
5. 调整 `Main.cs` 与相关入口路径，让现有主界面逻辑读写新 HUD 节点。

## 验收标准（可测试）

- [ ] `Main.tscn` 已接入独立 `CanvasLayer` 主 HUD，地图视图仍可作为中心主内容显示。
- [ ] 顶栏可显示人口、民心、灵谷、木石、灵石、危兆与日期，并保留季度/日进度条绑定路径。
- [ ] 左侧院域营建卷与右侧宗门纪事卷改为浮动悬停式布局，且 `MouseFilter` 不会阻断非交互空白区的地图点击。
- [ ] 底栏存在 `库房 / 中枢 / 谱系 / 天工 / 政令` 五枚令箭按钮，并具备悬停拔出 / 选中停驻反馈。
- [ ] 令箭上方可弹出二级副卷，收卷后按钮状态可回落，不影响既有卷册入口逻辑。
- [ ] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：主界面节点路径迁移到 `CanvasLayer` 后，若遗漏绑定路径，可能导致现有卷册按钮、见闻摘要或时令按钮失效；同时新 `tscn` / `gd` 表现层资源仍需 Godot 编辑器导入与手动走查。
- 回滚方式：回退 `Main.tscn`、`MainHUD.tscn`、`TopBar.tscn`、`BottomBar.tscn` 与相关 `Main*.cs` 路径调整，恢复原 `LayoutVBox` 主界面结构。
