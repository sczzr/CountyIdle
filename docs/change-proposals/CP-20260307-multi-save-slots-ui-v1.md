# 改动提案（Change Proposal）

## 提案信息

- 标题：多存档槽管理界面（V1：手动槽）
- 日期：2026-03-07
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：
  - SQLite 默认槽已经可用，但主界面仍只有“直接存档 / 直接读档”两条路径；
  - 玩家无法在 UI 中管理多槽，也无法选择特定槽位读写；
  - 这会削弱 SQLite 存档层的实际玩家价值。
- 证据（数据/玩家反馈）：
  - 用户已明确提出“多存档 UI 要做”。

## 改动内容

- 改什么：
  - 新增多存档槽管理弹窗
  - 将主界面“存档 / 读档”按钮切换为打开面板
  - 面板内支持 `覆盖 / 读取 / 新建 / 重命名 / 删除 / 刷新`
  - 快捷键 `QuickSave / QuickLoad` 继续走默认槽
- 不改什么：
  - 不加入自动存档策略
  - 不加入截图预览与筛选搜索
  - 不改 SQLite 表结构主干
- 影响系统：
  - `CountyIdle/scripts/core/SaveSystem.cs`
  - `CountyIdle/scripts/core/SqliteSaveRepository.cs`
  - `CountyIdle/scripts/ui/SaveSlotsPanel.cs`
  - `CountyIdle/scripts/ui/MainSaveSlotsPanel.cs`
  - `CountyIdle/scenes/ui/SaveSlotsPanel.tscn`
  - `CountyIdle/scripts/Main.cs`
  - `CountyIdle/scripts/ui/MainShortcutBindings.cs`

## 预期结果

- 预期提升指标：
  - 多存档槽在玩家层面可见、可操作
  - 默认槽与自定义槽职责分离
  - 为后续自动存档槽和存档列表扩展打基础
- 可接受副作用：
  - 主界面按钮点击后多一步弹窗交互

## 验证计划

- 验证方式：
  - 打开面板并执行一轮 `新建 -> 覆盖 -> 读取 -> 重命名 -> 删除`
  - 验证主存档删除保护
  - 验证快捷键仍能直读直写默认槽
  - `dotnet build .\Finally.sln`
- 观察周期：单轮启动与一轮完整存档管理操作
- 成功判定阈值：
  - 面板功能可完整走通
  - 主循环与存档恢复不报错

## 回滚条件

- 触发条件：
  - 面板引入后出现存档误删或读档异常
  - 主界面按钮改造影响原有快捷流程
- 回滚步骤：
  1. 保留多槽仓库接口
  2. 将主界面按钮恢复为直接存档/读档
  3. 保留快捷键与默认槽不变
