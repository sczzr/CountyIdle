# 改动提案（Change Proposal）

## 提案信息

- 标题：时间顶栏改为季度/天双进度条
- 日期：2026-03-07
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：现有顶部时间条只反映节气段进度，玩家难以同时判断长期季度节奏与短期日内流逝。
- 证据（数据/玩家反馈）：需求明确提出“时间进度条分为季度进度条和天进度条，随着时间的增长进度条也需要变化”。

## 改动内容

- 改什么：
  - 让 `GameCalendarSystem` 产出季度/天两层时间进度；
  - 顶栏新增季度进度条与天进度条，并保持日期/节气/时辰文本；
  - Legacy 与 Figma 布局统一采用双进度条时间反馈。
- 不改什么：
  - 不改主循环时间基线；
  - 不改小时结算触发条件；
  - 不改存档格式。
- 影响系统：`docs/02_system_specs.md`、`CountyIdle/scripts/models/GameCalendarInfo.cs`、`CountyIdle/scripts/systems/GameCalendarSystem.cs`、`CountyIdle/scripts/Main.cs`、`CountyIdle/scenes/ui/TopBar.tscn`、`CountyIdle/scenes/ui/figma/HUDTopBar.tscn`

## 预期结果

- 预期提升指标：
  - 玩家能同时感知季度级与日内级时间节奏；
  - 倍速切换时，时间反馈更连续直观；
  - 读档后能立即看懂当前季度/当天所处位置。
- 可接受副作用：顶栏高度小幅增加，但不遮挡核心经营操作区。

## 验证计划

- 验证方式：`dotnet build .\Finally.sln`，并检查 `GameMinutes` 连续增长时双进度条同步刷新。
- 观察周期：单次启动后至少观察 `1` 个游戏日与一次读档恢复。
- 成功判定阈值：季度条、天条与日期文案无错位，且 `60` 分钟结算节奏不变。

## 回滚条件

- 触发条件：出现双进度条不刷新、读档后进度错位、或顶栏布局严重遮挡。
- 回滚步骤：
1. 回退 `GameCalendarInfo` 与 `GameCalendarSystem` 的双进度字段。
2. 回退 `Main.cs` 顶栏绑定与刷新逻辑。
3. 回退 `TopBar.tscn` 与 `HUDTopBar.tscn`，恢复旧版单进度条。
