# 改动提案（Change Proposal）

## 提案信息

- 标题：客户端快捷键配置与重绑定
- 日期：2026-03-06
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：操作仅依赖按钮点击，缺少统一快捷键配置入口。
- 证据（数据/玩家反馈）：用户明确要求“搞一套快捷键配置功能”。

## 改动内容

- 改什么：
  - 在 `ClientSettings` 新增快捷键配置字段
  - 在设置面板新增快捷键配置区（下拉选择）
  - 在 `Main` 增加按键监听和动作分发
  - 保证设置可持久化并在运行时即时生效
- 不改什么：
  - 不改核心结算规则
  - 不改存档结构
  - 不支持组合键
- 影响系统：
  - `CountyIdle/scripts/models/ClientSettings.cs`
  - `CountyIdle/scripts/core/ClientSettingsSystem.cs`
  - `CountyIdle/scenes/ui/SettingsPanel.tscn`
  - `CountyIdle/scripts/ui/SettingsPanel.cs`
  - `CountyIdle/scripts/Main.cs`

## 预期结果

- 预期提升指标：
  - 核心操作可通过快捷键完成，减少鼠标操作频次
  - 玩家可按习惯自定义键位
  - 设置重启后保持一致
- 可接受副作用：
  - 设置面板复杂度上升

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 配置快捷键后立即触发并验证
  - 重启后检查快捷键读取是否正确
- 观察周期：10 分钟交互烟测
- 成功判定阈值：无空引用、无错触发、快捷键持久化有效

## 回滚条件

- 触发条件：快捷键误触发率高或高频冲突不可控
- 回滚步骤：
  1. 保留默认快捷键常量
  2. 移除设置面板重绑定项
  3. 维持按钮操作路径
