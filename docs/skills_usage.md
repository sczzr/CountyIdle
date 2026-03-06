# CountyIdle Skill 路由（精简版）

## 四类 Skill

- `countyidle-system-design`：机制设计 / 规格更新
- `countyidle-feature-delivery`：功能实现 / 最小闭环交付
- `countyidle-balance-lab`：参数实验 / 指标对比 / 回滚准备
- `countyidle-agent-handoff`：多 Agent 交接 / 并行协作治理

## 默认顺序

1. 机制任务：`system-design -> feature-delivery -> agent-handoff`
2. 平衡任务：`balance-lab -> agent-handoff`

## 与文档联动

- 核心规格：`02_system_specs.md`
- 功能总表：`05_feature_inventory.md`
- 流程规则：`03_change_management.md`
- 模板入口：`templates/`
