# 改动提案（Change Proposal）

## 提案信息

- 标题：存档预览增强（V1：摘要字段 + 面板预览）
- 日期：2026-03-08
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：
  - 多存档面板目前主要展示人口、金钱与科技，存档之间的经营差异仍不够直观；
  - 自动档与手动档数量增加后，玩家更需要快速判断“应该读哪个档”。
- 证据（数据/玩家反馈）：
  - 看板中“多存档槽管理界面”条目已明确下一步为“存档预览、筛选排序与更完整的管理入口”。

## 改动内容

- 改什么：
  - 给 `save_slots` 增加更多预览摘要字段
  - 在存档列表与详情区域展示民心、威胁、探险层数、仓储负载等信息
  - 保持默认槽、手动槽、自动槽的读写规则不变
- 不改什么：
  - 不实现截图预览
  - 不实现搜索/筛选
  - 不改变自动存档轮换规则
- 影响系统：
  - `CountyIdle/scripts/core/SqliteMigrationRunner.cs`
  - `CountyIdle/scripts/core/SqliteSaveRepository.cs`
  - `CountyIdle/scripts/core/SaveSystem.cs`
  - `CountyIdle/scripts/models/SaveSlotSummary.cs`
  - `CountyIdle/scripts/ui/SaveSlotsPanel.cs`
  - `docs/02_system_specs.md`

## 预期结果

- 预期提升指标：
  - 玩家在多槽面板中更快找到目标存档
  - 自动档之间的差异更容易理解
  - 为后续截图预览和筛选排序打基础
- 可接受副作用：
  - 多槽面板文案略复杂

## 验证计划

- 验证方式：
  - 构建并打开多槽面板
  - 验证列表与详情包含增强后的摘要字段
  - 验证旧库迁移后仍可正常列出和读取
  - `dotnet build .\Finally.sln`
- 观察周期：一轮启动 + 一次打开多槽面板
- 成功判定阈值：
  - UI 信息可读且不报错
  - 旧库迁移无异常

## 回滚条件

- 触发条件：
  - 迁移导致旧库无法打开
  - 面板信息过载导致可读性明显下降
- 回滚步骤：
  1. 保留新列与迁移记录
  2. 回退多槽面板显示到旧版摘要
