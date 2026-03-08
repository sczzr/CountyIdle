# Change Proposals 索引

本目录存放机制/平衡改动提案。  
命名规则：`CP-YYYYMMDD-topic.md`

> 归档语义说明（2026-03-08）：提案文件名中的 `county / countytown / prefecture` 仍作为历史 topic 保留；阅读提案内容时，默认映射到当前修仙宗门经营语义。

## 已覆盖主题

- 科研突破
- 产业岗位逻辑
- 装备品质与词条掉落
- 宗门动态事件（`canonical-topic` 保留 `county-dynamic-events`）
- UI 交互重连与按钮行为
- 宗主治理与双轨内务结算
- 宗门组织谱系与峰脉协同法旨

## 常用兼容映射

- `countytown-*`：天衍峰山门图 / 天衍峰驻地表现链路
- `prefecture-*`：江陵府外域 / 外域主附庸据点表现链路
- `county-dynamic-events`：宗门动态事件
- `sect-task-orders-dual-currency`：宗主中枢兼容底层、方略折算与贡献点/灵石双轨内务
- `sect-peak-support-directives`：宗主通过 `JobsList` 直接指定协同峰并影响小时结算
- `sect-quarter-decrees`：宗主按季度颁令并影响小时结算、人口与锻器
- `sect-rule-tree`：宗主设定庶务 / 传功 / 巡山三支常设门规纲目

## 使用建议

- 做 L2/L3 改动前，先参考已有提案结构。
- 结果是否达成，配合 `docs/balance-logs/` 对照查看。
- 新建提案时必须沿用对应功能卡的 `canonical-topic`（见 `docs/06_archive_registry.md`）。


