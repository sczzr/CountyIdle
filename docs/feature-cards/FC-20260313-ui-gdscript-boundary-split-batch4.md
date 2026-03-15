# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第四批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`BottomBar`、`Main`

## 目标与用户收益

- 目标：继续将纯表现反馈从 `C#` 下放到 `GDScript`，把主界面底栏快捷键、倍速键和存读设按钮的 hover / focus 灯笼强调反馈收口到场景自身。
- 玩家可感知收益（10 分钟内）：底栏按钮划过与键盘聚焦时反馈更统一，底栏动画不再和 `Main.cs` 的全局按钮逻辑耦在一起。

## 实现范围

- 包含：
  - 为 `BottomBar.tscn` 与 `figma/BottomBar.tscn` 接入统一的 `BottomBarLanternFx.gd`
  - 将底栏按钮 hover / focus 的缩放、染色与 hover 音效下放到 `GDScript`
  - 收窄 `Main.cs` 的全局灯笼 hover 绑定范围，跳过 `BottomBar` 内按钮
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改底栏按钮的业务点击逻辑、倍速切换、存读档流程与客户端设置持久化
  - 不改 `GameState`、存档结构与小时结算公式
  - 不引入 `C++ / Rust` 运行时扩展

## 实现拆解

1. 新增 `BottomBarLanternFx.gd`
2. 在旧版与 figma 两套 `BottomBar` 场景中挂接 `VisualFx` 节点
3. 保留 `Main.cs` 对底栏按钮点击事件的权威绑定，仅移除其对底栏 hover 动画的直接负责

## 验收标准（可测试）

- [ ] 旧版 `BottomBar` 的快捷键、倍速键、存读设按钮在 hover / focus 时有统一灯笼强调反馈
- [ ] `figma/BottomBar` 的对应按钮同样复用同一份 `GDScript` 表现
- [ ] `Main.cs` 不再为 `BottomBar` 内按钮直接管理 hover tween
- [ ] `GDScript` 不直接修改倍速状态、存档流程、客户端设置与核心公式
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若底栏场景路径后续调整但未同步脚本，可能导致部分按钮漏绑表现；hover 音效若重复触发过密，可能出现音频叠加感
- 回滚方式：移除 `BottomBar` 场景中的 `VisualFx` 节点，并恢复 `Main.cs` 对底栏按钮的全局 hover 绑定即可
