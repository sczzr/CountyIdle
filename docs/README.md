# CountyIdle 文档导航

本目录用于把“游戏大纲”转成可执行的开发指南。

## 文档结构

- `01_game_design_guide.md`：总指南（愿景层）
- `02_system_specs.md`：系统规格（系统层）
- `03_change_management.md`：改动流程（治理层）
- `04_weekly_flywheel_review.md`：每周飞轮体检（运营层）
- `skills_usage.md`：项目 Skill 路由与使用方式
- `templates/system-spec-template.md`：系统规格模板
- `templates/feature-card-template.md`：功能卡模板
- `templates/change-proposal-template.md`：改动提案模板
- `templates/balance-log-template.md`：数值平衡日志模板
- `templates/handoff-template.md`：多 Agent 交接模板

## 推荐工作流

1. 在 `01_game_design_guide.md` 维护核心愿景，不频繁改动。
2. 新系统先在 `02_system_specs.md` 录入公式、输入输出、边界。
3. 每个开发项先写一张 `feature-card`，再开始编码。
4. 涉及平衡/玩法调整时先写 `change-proposal`，上线后写 `balance-log`。
5. 每周填写一次飞轮体检，判断是否偏离核心循环。

## 版本建议

- 大纲版本：`v0.x`（快速迭代阶段）
- 每次重大改动更新版本号，并在提案中写“改动原因与预期指标”。
