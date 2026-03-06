# AGENTS.md

本文件定义本仓库内 AI/开发者协作规范，作用域为仓库根目录及全部子目录。

## 1) 项目基线

- 项目：`CountyIdle`
- 引擎：`Godot 4.2+`
- 语言：`C#` + `.NET 8`
- 核心节奏：`1秒现实时间 = 1游戏分钟`，每 `60分钟` 小时结算一次
- 核心飞轮：人口繁衍 → 产业涌现（生产/消耗）→ 科技涌现（科技树）→ 职业分化与转职 → 精英/英雄冒险与装备成长 → 郡县共建共护并反哺人口与产业
- 核心玩法目标：人口繁衍、产业涌现、科技涌现、职业分化与转职、优秀后代培育、英雄冒险、装备打造、郡县共建共护
- 伦理底线：禁止职业歧视叙事与机制；内容须遵守人类基本准则和道德规范

## 2) 文档驱动开发（必须遵守）

所有开发与改动必须先对齐 `docs` 目录：

- 总导航：`docs/README.md`
- 愿景指南：`docs/01_game_design_guide.md`
- 系统规格：`docs/02_system_specs.md`
- 改动流程：`docs/03_change_management.md`
- 周体检：`docs/04_weekly_flywheel_review.md`
- 模板目录：`docs/templates/`

规则：
- 新功能先写 `feature-card-template.md` 再编码。
- 涉及机制/平衡改动先写 `change-proposal-template.md`。
- 改动后补 `balance-log-template.md`（至少记录改前/改后/结果）。
- 新系统先补系统规格，再进入实现。

### 2.1 需求受理与开发列表协议（强制）

每次收到需求时，必须先执行以下检查，不可跳过：

1. 先查 `docs/05_feature_inventory.md`（实现状态看板）
   - 若功能为 `✅ 已实现`：按“优化/修复”处理，不重复开发同功能。
   - 若功能为 `🟡 部分实现` 或 `⭕ TODO`：从对应条目继续推进。
2. 若在 `05` 中查不到该需求：
   - 先将需求登记到 `docs/08_development_list.md`（形成完整功能包）；
   - 再进入开发，不允许直接插入零散功能。
3. 当用户说“继续完善游戏”且未指定条目时：
   - 默认按 `docs/08_development_list.md` 从上到下逐条开发。

功能包必须具备：目标、飞轮环节、依赖、完成标准（DoD），避免突兀功能插入。

## 3) 目录职责

- `CountyIdle/scenes/`：场景定义（如 `Main.tscn`）
- `CountyIdle/scripts/core/`：主循环、事件、存档框架
- `CountyIdle/scripts/models/`：状态模型与枚举
- `CountyIdle/scripts/systems/`：玩法系统逻辑
- `CountyIdle/data/`：静态配置（JSON）

新增代码必须放在匹配目录，不将系统逻辑堆入 `Main.cs`。

## 4) 架构与代码约束

- `Main.cs` 仅负责 UI/交互组织，不承载复杂玩法计算。
- 玩法计算在 `systems`，调度在 `core/GameLoop`。
- `GameState` 更新需满足：非负、可结算、可存档。
- UI 状态发布保持克隆副本（延续 `Clone()` 模式）。
- 避免魔法数字，规则优先用 `const` 或配置化。
- 非必要不改 `.godot/` 和 `*.cs.uid` 生成文件。
- 不做与当前任务无关的大型重构/重命名。

## 5) 改动分级与执行流程

- L1 参数微调：系数、概率、产量、冷却
- L2 机制增强：新增词条、事件、建筑效果
- L3 架构调整：核心循环或系统依赖变更

执行流程（默认）：
1. 先查 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
2. 定位影响环节（飞轮哪一环）
3. 写功能卡/改动提案（`docs/templates`）
4. 最小闭环实现（`models` → `systems` → `core` → `UI`）
5. 本地验证
6. 回写状态（更新 `05/08` + 相关归档文档）

## 6) 每次改动最低验证清单

- `dotnet build .\Finally.sln` 通过
- Godot 可运行 `CountyIdle/project.godot`（`F5`）
- 小时结算频率正确（每 60 游戏分钟）
- 人口与职业分配无负值/超分配异常
- 探险开关、存档/读档流程正常

涉及平衡改动时，必须写出“改动前后差异 + 指标结果”。

## 7) 提交说明规范

- 改动目的（为什么）
- 关键改动（改了什么）
- 验证结果（如何确认）
- 风险与后续（下一步计划）

## 8) 多 Agent 协作团队规范（严格执行）

### 8.1 角色分工（推荐最小编制）

- `Planner Agent`：需求拆解、优先级、里程碑与任务分发
- `System Agent`：系统规格维护（公式、边界、依赖）
- `Gameplay Agent`：功能实现（`models/systems/core/UI` 最小闭环）
- `Balance Agent`：数值实验、平衡评估、日志沉淀
- `QA Agent`：验证清单执行与回归风险报告
- `Release Agent`：版本说明、合并门禁、发布节奏管理

一个任务最多一个主责 Agent，其余为协作 Agent。

### 8.2 标准协作流水线

1. `Planner Agent` 创建功能卡（`feature-card`）
2. `System Agent` 更新系统规格（若涉及机制）
3. `Gameplay Agent` 实现最小闭环
4. `QA Agent` 执行最低验证清单
5. `Balance Agent` 补平衡日志（若涉及数值）
6. `Release Agent` 汇总并发布

禁止跳过文档直接编码；禁止无提案进行 L2/L3 改动。

### 8.3 交接（Handoff）协议

每次交接必须附带以下字段：

- `任务ID`
- `当前状态`（进行中/阻塞/完成）
- `已改文件`
- `待改文件`
- `验证结果`
- `风险与建议`
- `下一位责任 Agent`

若信息不完整，下一位 Agent 有权拒绝接手并退回补充。

### 8.4 并行开发与文件所有权

- 同一时段同一文件只允许一个 Agent 主写。
- 并行任务优先按目录拆分：`systems`、`models`、`core`、`UI/docs`。
- 涉及 `GameState`、`GameLoop`、存档格式的改动必须串行处理。
- 冲突处理优先级：`稳定性 > 存档兼容 > 玩法收益 > 表现优化`。

### 8.5 质量门禁（DoD）

任务标记完成前必须满足：

- 有对应文档记录（功能卡/提案/平衡日志）
- `dotnet build .\Finally.sln` 通过
- 可运行 `CountyIdle/project.godot`（`F5`）
- 核心飞轮未被破坏（至少给出一句评估）
- 提交说明包含风险与后续动作

### 8.6 失败与回滚协作

- 若改动导致核心指标恶化，`Release Agent` 可触发回滚。
- 回滚后由 `Planner Agent` 组织 15 分钟内复盘，产出下一轮可执行项。
- 禁止在未复盘前重复同方向高风险改动。

## 9) 项目 Skill 路由（建议默认启用）

以下 Skill 位于 `C:\Users\misoda\.codex\skills\`，面向 CountyIdle 开发：

- `countyidle-system-design`
  - 用于：新系统设计、规则重构、公式与边界定义、飞轮影响评估
  - 典型触发：更新 `docs/02_system_specs.md` 或讨论机制设计
- `countyidle-feature-delivery`
  - 用于：将需求落地为 C# 功能改动（`models/systems/core/UI` 最小闭环）
  - 典型触发：实现新玩法、调整已有逻辑、补齐开发交付记录
- `countyidle-balance-lab`
  - 用于：数值平衡调参、指标对比、改动提案与平衡日志
  - 典型触发：增长曲线、掉率、资源节奏、难度曲线优化
- `countyidle-agent-handoff`
  - 用于：多 Agent 任务交接、并行协作、冲突归因与优先级仲裁
  - 典型触发：任务切换负责人、阶段性交付、阻塞移交

推荐调用顺序：
1. 机制类任务：`countyidle-system-design` → `countyidle-feature-delivery` → `countyidle-agent-handoff`
2. 平衡类任务：`countyidle-balance-lab` → `countyidle-agent-handoff`

---

若新增子系统（科技树、建筑链、任务系统等），先更新系统规格文档，再开始编码。
