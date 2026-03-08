# 改动提案（Change Proposal）

## 提案信息

- 标题：多存档面板截图预览（V1）
- 日期：2026-03-08
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：
  - 多槽面板虽然已支持摘要预览与筛选排序，但仍主要依赖文字信息判断存档内容；
  - 当玩家在相近数值档之间切换时，仍缺乏直观画面锚点。
- 证据（数据/玩家反馈）：
  - `docs/05_feature_inventory.md` 中多存档界面剩余差距已经指向“存档截图预览”。

## 改动内容

- 改什么：
  - 为每个槽位引入独立 PNG 预览图文件
  - 在成功存档后保存当前视口截图
  - 在多槽面板详情区展示所选槽位截图
  - 删除手动槽时同步删除对应 PNG
- 不改什么：
  - 不改 SQLite 表结构
  - 不把截图二进制写入数据库
  - 不改变现有多槽读写规则与自动存档轮换
- 影响系统：
  - `CountyIdle/scripts/core/SaveSystem.cs`
  - `CountyIdle/scripts/models/SaveSlotSummary.cs`
  - `CountyIdle/scripts/ui/MainSaveSlotsPanel.cs`
  - `CountyIdle/scripts/ui/MainShortcutBindings.cs`
  - `CountyIdle/scripts/ui/MainSavePreview.cs`
  - `CountyIdle/scripts/ui/SaveSlotsPanel.cs`
  - `CountyIdle/scenes/ui/SaveSlotsPanel.tscn`

## 预期结果

- 预期提升指标：
  - 玩家更快识别目标存档
  - 多个相近经营档更容易区分
  - 为后续更完整的存档管理入口提供视觉基础
- 可接受副作用：
  - `user://save_previews` 会新增若干 PNG 文件

## 验证计划

- 验证方式：
  - 执行快速存档、手动存档、自动存档流程
  - 打开多槽面板，确认详情区预览图显示正常
  - 删除手动槽，确认预览图也被清理
  - `dotnet build .\Finally.sln`
- 观察周期：一轮启动 + 一次手动存档 + 一次自动存档
- 成功判定阈值：
  - 保存成功时不影响原有存档流程
  - 预览图缺失时 UI 仍稳定

## 回滚条件

- 触发条件：
  - 预览图读写导致存档流程失败
  - 详情区贴图显示异常或明显影响操作
- 回滚步骤：
  1. 回退 PNG 预览图生成逻辑
  2. 回退面板预览区域展示
  3. 保留 SQLite 存档、多槽、自动档与筛选排序功能
