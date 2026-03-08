# CountyIdle 文档总导航

本目录目标：**让“这款游戏现在是怎样的修仙天衍峰经营游戏、规则如何落地、下一步做什么”在 5 分钟内查清**。

> 背景基线（2026-03-09 / V3）：当前项目唯一对外世界观为“修仙世界中的天衍峰经营 + 外域经营 + 历练探索”，默认宗门原型定锚为 **浮云宗（青云州江陵府）**，当前经营主视角为 **天衍峰**。
>
> 玩家身份：掌门 / 宗主。  
> 经营对象：山门、门人、凡俗附庸、外域附庸据点、灵脉与资源链。  
> `County / Town / Prefecture` 等命名仅保留在代码与历史归档中，作为兼容技术名使用。

## 1) 先读这 7 份（核心文档）

1. `09_xianxia_sect_setting.md`：宗门设定基线、术语表、文案门禁
2. `01_game_design_guide.md`：游戏愿景、飞轮、设计边界
3. `02_system_specs.md`：唯一可执行系统规格（公式 / 顺序 / 边界）
4. `03_change_management.md`：L1/L2/L3 改动流程与回滚规则
5. `04_weekly_flywheel_review.md`：每周飞轮体检模板
6. `05_feature_inventory.md`：实现状态看板（已实现 / 部分实现 / TODO）
7. `08_development_list.md`：顺序开发列表（继续完善时按条执行）

## 2) 推荐阅读顺序

### 2.1 理解背景与目标

先读：

1. `09_xianxia_sect_setting.md`
2. `01_game_design_guide.md`

用于回答：

- 玩家到底在经营什么宗门生态；
- “人口 / 科技 / 英雄 / 县城”这些历史技术词在当前世界观里分别代表什么；
- 新功能要服务哪一段宗门飞轮。

### 2.2 落地规则与开发顺序

再读：

1. `02_system_specs.md`
2. `05_feature_inventory.md`
3. `08_development_list.md`

用于回答：

- 当前规则已经做到哪里；
- 哪些系统已落地、哪些还只是半成品；
- 现在应该顺着哪个功能包继续做。

### 2.3 改动治理与复盘

最后读：

1. `03_change_management.md`
2. `04_weekly_flywheel_review.md`

用于回答：

- 这次改动属于 L1 / L2 / L3 哪一级；
- 需要补哪些提案、日志、回滚说明；
- 如何判断这轮改动有没有让宗门飞轮更健康。

## 3) 模板与执行入口

- `templates/feature-card-template.md`：功能卡，任何非琐碎任务的起点
- `templates/change-proposal-template.md`：机制 / 平衡改动提案
- `templates/balance-log-template.md`：改动结果记录
- `templates/system-spec-template.md`：新系统规格模板
- `templates/handoff-template.md`：多 Agent 交接模板

## 4) 归档治理（防文档膨胀）

- `06_archive_registry.md`：历史归档注册表、topic 合并建议、命名规范
- `07_archive_rename_migration_plan.md`：历史批量重命名迁移记录
- `feature-cards/`：功能卡归档（见 `feature-cards/README.md`）
- `change-proposals/`：提案归档（见 `change-proposals/README.md`）
- `balance-logs/`：结果归档（见 `balance-logs/README.md`）

> 归档用于追溯，不作为当前规则的唯一来源。  
> 设定语义以 `09_xianxia_sect_setting.md` 为准，系统规则以 `02_system_specs.md` 为准。

## 5) 当前标准流水线

1. 先对齐宗门背景与术语：看 `09`
2. 再对齐目标与飞轮：看 `01`
3. 查规格与现状：看 `02 / 05 / 08`
4. 先写文档：功能卡；如涉及机制/平衡再补提案
5. 实现最小闭环：`models -> systems -> core -> UI`
6. 执行最低验证：`dotnet build` + Godot 烟测
7. 记录结果：`BL` / 交接 / 状态回写

## 6) 文档维护原则

- **世界观单点收口**：宗门设定、术语解释优先维护在 `09_xianxia_sect_setting.md`
- **规则单一事实源**：公式、顺序、边界只在 `02_system_specs.md` 维护
- **看板驱动开发**：实现状态看 `05`，顺序推进看 `08`
- **历史归档化**：阶段性过程进入 `feature-cards/`、`change-proposals/`、`balance-logs/`
- **先对齐后编码**：涉及机制、术语、边界的改动，先改文档再写代码


