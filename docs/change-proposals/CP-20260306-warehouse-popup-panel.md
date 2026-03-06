# 改动提案（Change Proposal）

## 提案信息

- 标题：仓储管理界面改为独立弹窗 tscn
- 日期：2026-03-06
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：仓储管理放在地图页签内，不符合“弹出新 tscn 管理仓储”的交互预期。
- 证据（数据/玩家反馈）：用户明确要求“分门别类显示，并弹出一个新的 tscn”。

## 改动内容

- 改什么：
  - 新建独立场景 `WarehousePanel.tscn` 承载仓储管理
  - 主界面新增弹窗接入逻辑并绑定仓储操作事件
  - 仓储信息改为分类展示（基础/矿石/材料产成品）
  - 旧内嵌仓储页节点从 `WorldPanel` 移除
- 不改什么：
  - 不改资源链结算公式
  - 不改 `GameState` 字段结构
  - 不改存档格式
- 影响系统：
  - `CountyIdle/scenes/ui/WarehousePanel.tscn`
  - `CountyIdle/scripts/ui/WarehousePanel.cs`
  - `CountyIdle/scripts/ui/MainWarehousePanel.cs`
  - `CountyIdle/scenes/ui/WorldPanel.tscn`
  - `CountyIdle/scripts/Main.cs`

## 预期结果

- 预期提升指标：
  - 仓储管理入口更明确，交互符合弹窗预期
  - 分类显示提升资源识别效率
  - 操作与信息集中，不打断地图页签浏览
- 可接受副作用：
  - 需要额外一次点击打开弹窗

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 手动打开/关闭仓储弹窗并验证分类展示
  - 验证弹窗内四个按钮对应功能正常
- 观察周期：5~10 分钟交互烟测
- 成功判定阈值：无空引用、无按钮失效、无弹窗层级异常

## 回滚条件

- 触发条件：弹窗交互导致高频异常或严重遮挡
- 回滚步骤：
  1. 恢复 `WorldPanel` 内嵌仓储页节点
  2. 移除 `MainWarehousePanel.cs` 与 `WarehousePanel.tscn` 接入
  3. 保留现有仓储系统逻辑不变
