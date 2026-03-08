# 改动提案：宗门经营全局语义统一（V2）

> 历史兼容说明（2026-03-08）：本文保留旧阶段的“县城 / 郡图 / 郡县 / 州府”等表述以便追溯；在当前项目语义中，请分别按“天衍峰驻地 / 江陵府外域 / 天衍峰经营 / 世界地图”理解。


## 提案信息

- 标题：将宗门术语从地图层扩展到全局经营与提示层
- 日期：2026-03-08
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：双地图与天衍峰山门图 V1 已落地，但岗位、仓储、研究突破、资源日志、事件提示和地图经营条仍残留“县城 / 学宫 / 官署 / 市集 / 工坊”等旧词。
- 证据（数据/玩家反馈）：用户持续要求“继续”，并已明确产品方向为“只保留天衍峰山门图 + 世界地图”，因此剩余可见旧词会直接破坏整体修仙世界观一致性。

## 改动内容

- 改什么：
  - 扩展 `SectMapSemanticRules` 为全局显示层术语表；
  - 将岗位、仓储、研究、资源日志、事件讲习与地图调度统一改成宗门语义；
  - 把世界图默认标题固定为 `世界地图`，隐藏备用 Prefecture 文案改为 `江陵府外域`。
- 不改什么：
  - 不改 `GameState` 字段与建筑枚举；
  - 不调整人口、产能、科技阈值、调度消耗等数值。
- 影响系统：
  - `CountyIdle/scripts/systems/SectMapSemanticRules.cs`
  - `CountyIdle/scripts/systems/IndustrySystem.cs`
  - `CountyIdle/scripts/systems/JobProgressionRules.cs`
  - `CountyIdle/scripts/systems/ResearchSystem.cs`
  - `CountyIdle/scripts/systems/ResourceSystem.cs`
  - `CountyIdle/scripts/systems/MapOperationalLinkSystem.cs`
  - `CountyIdle/scripts/systems/CountyEventSystem.cs`
  - `CountyIdle/scripts/systems/StrategicMapConfigSystem.cs`
  - `CountyIdle/scripts/ui/WarehousePanel.cs`
  - `CountyIdle/scripts/Main.cs`
  - `CountyIdle/scenes/ui/WarehousePanel.tscn`
  - `CountyIdle/scenes/ui/JobsPanel.tscn`
  - `CountyIdle/scenes/ui/EventLogPanel.tscn`
  - `CountyIdle/scenes/ui/WorldPanel.tscn`

## 预期结果

- 预期提升指标：
  - 双地图主线下的所有高频经营界面语义统一；
  - 玩家在世界图与宗门图间切换时不再看到旧郡县跳词。
- 可接受副作用：
  - 内部仍保留 `CountyTown` / `Prefecture` 类名作兼容；
  - 隐藏备用 Prefecture 生成器仍保留部分旧结构与开封主题内容。

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 全局搜索高频旧词残留
  - 抽查岗位/仓储/天衍峰山门图状态文案
- 观察周期：本次迭代即时验证
- 成功判定阈值：
  - 主要可见界面不再显示 `县城 / 学宫 / 官署 / 市集 / 工坊 / 农坊`；
  - 世界图标题保持 `世界地图`；
  - 构建通过。

## 回滚条件

- 触发条件：文本替换导致构建失败、UI 丢文案或玩家无法识别功能入口。
- 回滚步骤：
  1. 保留 `SectMapSemanticRules` 新增接口；
  2. 将系统与 UI 的调用点回退到原有字串；
  3. 保留已完成的双地图结构与世界生成链路。


