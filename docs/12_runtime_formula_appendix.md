# CountyIdle 运行公式附录

> 本文用于补充主规格之外的运行公式、兼容层与实现细节。
> 若与 `docs/01_game_design_guide.md` 或 `docs/02_system_specs.md` 冲突，以主文档为准。

## 0. 使用范围

- 本文记录运行层公式、常量、兼容字段与细节参数。
- 本文不裁决项目主轴、题材边界或系统地位。
- 若某条细节已经上升为稳定主规则，应回收到 `docs/02_system_specs.md`。

## 1. 时间与结算口径

- 时间口径：`1 秒现实时间 = 10 游戏分钟`
- 倍速：`x1 / x2 / x4`
- 小时结算：每累计 `60` 游戏分钟一次
- 当前主结算链：
  - `Industry -> Resource -> Economy -> Research -> Population -> Breeding -> Combat -> CountyEvent`

## 2. 岗位与工具兼容层

- 当前仍保留四类内部汇总职司：
  - `Farmers`
  - `Workers`
  - `Merchants`
  - `Scholars`
- 当前通过 `TaskOrderUnits -> TaskResolvedWorkers -> 四类汇总职司` 做兼容折算。

### 2.1 已知关键公式

- 产业工人容量：`AgricultureBuildings*16 + WorkshopBuildings*12`
- 研发容量：`ResearchBuildings*10`
- 商业容量：`TradeBuildings*14`
- 管理容量：`AdministrationBuildings*8`
- 工具覆盖需求：`Farmers*0.34 + Scholars*0.62 + Merchants*0.42`
- 管理加成：`1 + clamp(Workers / Population, 0, 0.28)`

## 3. 仓储与材料链

- 当前仓储容量公式：
  - `900 + WarehouseLevel*260 + AdministrationBuildings*45`
- 当前前台主要材料层级：
  - `T0 / T1`
- 当前材料链方向：
  - 原材 -> 初加工 -> 民生 / 工具 / 建造 / 防务回流
- 当前运行重点：
  - `T0` 链路等级
  - `铜锭 / 熟铁`
  - 玩家可见库存整数化
  - 隐藏进度池 `DiscreteInventoryProgress`

## 4. 冶炼与工具

### 4.1 当前一期冶炼公式

- `铜矿 1.3 + 煤 0.45 -> 铜锭 0.9`
- `铁矿 1.9 + 煤 1.15 -> 熟铁 0.95`

### 4.2 当前制工具方向

- 若存在 `熟铁 / 铜锭`，优先消耗新链路材料。
- 旧链路仍保留兼容兜底。

## 5. 当前仍需保留的运行说明

### 5.1 门人、人口与繁育

- `Population` 当前仍是聚合态人口池，不是显式弟子实体总册。
- 门人生息、伤病恢复、住房 / 通勤 / 幸福等逻辑已接入主循环。
- `Breeding` 当前仍承担长期成长兼容层，后续可继续向“灵根苗子 / 后辈培育”显式化推进。

### 5.2 研修与突破

- `Research` 当前仍保留运行字段与 tier 结构。
- 对外语义统一为“传承研修 / 功法推演”。
- 若未来进入分支传承树，应在不破坏现有主循环与存档兼容的前提下推进。

### 5.3 外务、历练与威胁

- `Combat` 当前兼容承担“外务历练 / 护山战备”语义。
- `Threat` 仍是威胁总压力量化字段，需要继续保留可观测性。
- 后续护山闭环、首领压力、真传战力等应以此层为扩展基础，而不是另起平行战斗系统。

## 6. 维护原则

- 本文只补充，不反向裁决主规格。
- 公式若频繁变化，应继续在本文维护，不要污染主规格正文。
- 若某组公式已稳定且会影响系统理解，再考虑回提到 `docs/02_system_specs.md`。
