# 改动提案（Change Proposal）

## 提案信息

- 标题：JobsPadding 具体岗位化与建筑/科技联动改造
- 日期：2026-03-07
- 提案人：Gameplay Agent
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：左侧岗位面板仍以“农工岗 / 匠役岗 / 商贾岗 / 学士岗”四个笼统条目展示，无法体现建筑扩张与科技推进带来的岗位变化，也无法逐个岗位解释数值来源。
- 证据（数据/玩家反馈）：当前 `JobsPadding` 仍停留在抽象岗位视角，虽已有进程化交互目标，但未形成“具体岗位配置 + 动态规则 + 逐项调配”的完整闭环。

## 改动内容

- 改什么：
  - 引入具体岗位配置（`jobs.json`）与规则系统（`JobRoleSystem`）
  - 将 `JobsPadding` 改为动态岗位列表，每个条目支持点击、加减、优先保留
  - 具体岗位人数受建筑数量、科技阶段、空闲人口与大类总容量共同约束
  - 新增“具体岗位 -> 四大岗位汇总”同步层，保持经济结算与旧系统兼容
  - 新增岗位说明提示，展示解锁条件、建筑来源与当前岗额
- 不改什么：
  - 不改变 `60 分钟小时结算`
  - 不改四大岗位在经济系统中的基础公式
  - 不在本次引入分支科技树与新建筑大类
- 影响系统：
  - `CountyIdle/data/jobs.json`
  - `CountyIdle/scripts/models/GameState.cs`
  - `CountyIdle/scripts/models/JobRoleDefinition.cs`
  - `CountyIdle/scripts/systems/JobRoleSystem.cs`
  - `CountyIdle/scripts/core/GameLoop.cs`
  - `CountyIdle/scripts/ui/JobsPanel.cs`
  - `CountyIdle/scripts/ui/JobRowView.cs`
  - `CountyIdle/scenes/ui/JobsPanel.tscn`
  - `CountyIdle/scenes/ui/JobRow.tscn`
  - `CountyIdle/scripts/Main.cs`

## 预期结果

- 预期提升指标：
  - 玩家能直观看到至少 `10+` 个具体岗位及其建筑/科技来源
  - 岗位面板从“抽象分类”升级为“可解释的具体岗位调配”
  - 旧存档进入新版本时无需重开也能看到具体岗位分配结果
- 可接受副作用：
  - 岗位面板更长，需要滚动查看
  - 初次接触时需要多读一层规则提示

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 检查旧存档/新开局都能正确生成具体岗位
  - 检查岗位锁定、加减上限、优先保留与读档恢复
- 观察周期：单次启动验证 + `10~15` 分钟操作窗口
- 成功判定阈值：无负值、无空引用、岗位说明与建筑/科技条件一致

## 回滚条件

- 触发条件：具体岗位同步导致四大岗位总量异常、面板交互失效或旧存档读档异常
- 回滚步骤：
  1. 暂停具体岗位 UI 渲染，退回四大岗位汇总面板
  2. 保留 `jobs.json` 与规则系统供后续继续完善
  3. 记录同步失败原因并回写平衡日志
