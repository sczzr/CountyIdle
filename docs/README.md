# CountyIdle 文档总导航（整合版）

本目录目标：**让“玩法现状、系统规则、改动流程”在 5 分钟内可查清**。

## 1) 先读这 6 份（核心文档）

1. `01_game_design_guide.md`：愿景与飞轮边界（不频繁改）
2. `02_system_specs.md`：可执行系统规格（公式/顺序/边界）
3. `03_change_management.md`：L1/L2/L3 改动流程与回滚规则
4. `04_weekly_flywheel_review.md`：每周飞轮体检模板
5. `05_feature_inventory.md`：实现状态看板（已实现 / 部分实现 / TODO）
6. `08_development_list.md`：顺序开发列表（继续完善时按条执行）

## 1.5) 归档治理（防文档膨胀）

- `06_archive_registry.md`：历史归档注册表、合并建议与命名规范
- `07_archive_rename_migration_plan.md`：批量重命名迁移计划（文件名级）

## 2) 模板与执行入口

- `templates/feature-card-template.md`：功能卡（任何开发任务起点）
- `templates/change-proposal-template.md`：机制/平衡改动提案
- `templates/balance-log-template.md`：改动结果记录
- `templates/system-spec-template.md`：新系统规格模板
- `templates/handoff-template.md`：多 Agent 交接模板

## 3) 历史归档（只读）

- `feature-cards/`：历史功能卡（见 `feature-cards/README.md`）
- `change-proposals/`：历史改动提案（见 `change-proposals/README.md`）
- `balance-logs/`：历史平衡日志（见 `balance-logs/README.md`）

> 归档用于追溯，不作为当前规则的唯一来源。当前规则以 `02_system_specs.md` 为准。

## 4) 精简后的标准流水线

1. 定义任务目标（对应飞轮哪一环）
2. 先写文档（功能卡；如涉及机制/平衡再补提案）
3. 实现最小闭环（`models -> systems -> core -> UI`）
4. 执行最低验证（`dotnet build` + 运行烟测）
5. 记录结果（平衡日志/交接信息）

## 5) 文档维护原则

- **单一事实源**：系统公式只在 `02_system_specs.md` 维护。
- **历史归档化**：阶段性记录进入各归档目录，不堆在核心文档。
- **先对齐后编码**：涉及机制和边界的改动，必须先更规格文档。
