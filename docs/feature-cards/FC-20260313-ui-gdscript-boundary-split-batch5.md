# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第五批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`Main`

## 目标与用户收益

- 目标：继续将 `Main.cs` 中剩余的全局灯笼 hover / focus 表现从 `C#` 下放到 `GDScript`，让主界面其余按钮的划过反馈彻底转为场景侧负责。
- 玩家可感知收益（10 分钟内）：主界面顶部、左右侧、弹出卷册中的按钮 hover / focus 反馈更统一，后续视觉微调不再需要改 `Main.cs`。

## 实现范围

- 包含：
  - 新增 `MainLanternFx.gd`
  - 在 `Main.tscn` 中挂接 `LanternFx` 节点
  - 将 `Main.cs` 中全局按钮 hover tween、hover 音效、`OptionButton` popup 表现样式与 hover 锁定转移到 `GDScript`
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改按钮点击逻辑、地图切换、倍速状态、存读档流程与客户端设置持久化
  - 不改 `GameState`、存档结构与小时结算公式
  - 不改地图生成与任何系统公式

## 实现拆解

1. 新增 `MainLanternFx.gd` 承接主界面全局按钮 hover / focus 表现
2. `Main.cs` 保留一次性调用 `bind_hover_fx`，不再直接维护 tween / 音效状态
3. 保持 `BottomBar` 继续由自身 `BottomBarLanternFx.gd` 负责，避免双重绑定

## 验收标准（可测试）

- [ ] 主界面非底栏按钮 hover / focus 灯笼反馈由 `MainLanternFx.gd` 统一承接
- [ ] `OptionButton` 的 popup 样式、位置对齐与 hover 锁定仍正常
- [ ] `Main.cs` 不再直接持有 hover tween / hover 音效状态
- [ ] `GDScript` 不直接修改地图切换、存档、客户端设置与核心公式
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：动态创建的弹出卷册若在绑定时机之后新增按钮，可能漏绑 hover 表现；`OptionButton` popup 的位置对齐若路径变化，可能出现漂移
- 回滚方式：移除 `Main.tscn` 中的 `LanternFx` 节点，并恢复 `Main.cs` 原本的 hover 逻辑
