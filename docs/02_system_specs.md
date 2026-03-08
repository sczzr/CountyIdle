# CountyIdle 系统规格（整合精简版）

> 本文是当前版本（v0.x）唯一系统规则源，所有机制改动先更新这里。

## 0. 全局基线

- 技术基线：Godot 4.2+ / C# / .NET 8
- 时间基线：`1 秒现实时间 = 1 游戏分钟`
- 倍速范围：`x1 / x2 / x4`（由 `GameLoop.SetTimeScale` 夹紧）
- 小时结算：每累计 `60` 游戏分钟触发一次
- 时间显示：采用架空“景禾历”，即 `12 月 * 30 日 * 24 时`，全年融入 `24` 节气
- 季度规则：每 `3` 个月为一个季度，每季度固定 `90` 日，对应春/夏/秋/冬四季
- 节气规则：每 `15` 日切换一个节气；每月固定覆盖 `2` 个节气
- UI 约束：顶部不显示“结算倒计时”，顶栏统一改为“日期 + 季度进度 + 当日进度 + 季节/节气/时辰”
- 核心要求：人口、产业、科技、职业、探险、郡县治理必须互相影响，禁止孤立子系统
- 价值观要求：职业分化不等于职业高低，系统与文案禁止职业歧视表达

## 1. 小时结算顺序（严格）

`Industry -> Resource -> Economy -> Research -> Population -> Breeding -> Combat -> CountyEvent`

- 由 `scripts/core/GameLoop.cs` 串行调度。
- 每个子系统只改自己负责的状态，不中断主循环。
- 结算完成后发布 `GameState.Clone()` 到 UI。
- 读档后小时内结算相位按 `GameMinutes % 60` 恢复，不能从整点重新起算。

## 2. 状态与安全约束（GameState）

- 人口、岗位、建筑数量、工具库存不得出现负值。
- 资源扣减路径必须有可支付检查；失败写日志，不抛异常中断。
- 物品/资源库存的最小单位为 `1`：
  - 玩家可见库存一律为整数，不显示小数；
  - 小时结算中的分数产出/分数消耗进入 `DiscreteInventoryProgress` 隐藏进度池，累计满 `1` 后才转为可见库存；
  - 玩家手动操作（建造、制工具、地图调度等）只消耗整数物品，不允许直接使用隐藏进度。
- 科技倍率下限为 `1.0`，防止旧存档或异常值导致 0 倍率。
- 存档兼容：新增字段依赖默认值保证旧存档可读。
- 具体岗位状态：
  - `JobRoleAssignments`：保存“具体岗位ID -> 已分配人数”，用于 `JobsPadding` 的逐项交互与读档恢复；
  - `PriorityJobRoleId`：保存当前“优先保留”的具体岗位；当人口或容量回退时，该岗位最后被裁减；
  - 上述字段只负责具体岗位分配与 UI 解释，不改变四大岗位汇总字段在经济系统中的结算含义。

## 3. 产业与岗位系统（Industry）

### 3.1 岗位容量公式

- 产业工人容量：`AgricultureBuildings*16 + WorkshopBuildings*12`
- 研发容量：`ResearchBuildings*10`
- 商业容量：`TradeBuildings*14`
- 管理容量：`AdministrationBuildings*8`

### 3.1.1 具体岗位面板规则（JobsPadding）

- 数据源：`CountyIdle/data/jobs.json`
- 每条具体岗位配置至少包含：
  - `id / name / icon`
  - `job_type`（最终汇总回 `Farmer / Worker / Merchant / Scholar`）
  - `building_type`
  - `base_slots_per_building`
  - `tech_slots_per_level`
  - `min_tech_level`
  - `unlock_building_count`
  - `sort_order`
  - `is_fallback`
  - `description`
- 解锁条件：
  - `buildingCount < unlock_building_count` 时，该岗位锁定
  - `TechLevel < min_tech_level` 时，该岗位锁定
  - 锁定岗位仍显示在面板中，但只能查看规则，不能分配人数
- 具体岗位岗额按“顺序切分大类容量”计算，避免细分后突破原有总容量：
  - 先求所属大类总容量 `categoryCapacity`
  - 同类岗位按 `sort_order` 从小到大依次计算
  - `rawRoleCap = buildingCount * base_slots_per_building + max(TechLevel - min_tech_level, 0) * tech_slots_per_level`
  - 若 `is_fallback = true`，则该岗位直接承接当前剩余容量
  - `roleCap = min(rawRoleCap, remainingCategoryCapacity)`
  - 每计算一个岗位，都从 `remainingCategoryCapacity` 中扣减，保证同类所有具体岗位上限总和不超过原公式容量
- 交互约束：
  - `+` 调整同时受 `roleCap`、空闲人口与所属大类剩余容量约束
  - `-` 调整不得低于 `0`
  - 点击岗位条目会显示该岗位的建筑来源、科技要求、已派人数与规则说明
  - “优先保留”作用于具体岗位；当人口减少或建筑缩编导致回退时，优先岗位最后裁减
- 汇总规则：
  - 具体岗位只负责“显示与分配”
  - `GameState` 的 `Farmers / Workers / Merchants / Scholars` 仍作为经济/人口系统的实际结算输入
  - 每次具体岗位分配变化后，都要同步回写四大岗位汇总值

### 3.2 工具与组织系数

- 所需工具：`Farmers*0.34 + Scholars*0.62 + Merchants*0.42`
- 工具覆盖率：`clamp(IndustryTools / RequiredTools, 0.25, 1.0)`
- 管理加成：`1 + clamp(Workers / Population, 0, 0.28)`

### 3.3 每小时产业结算

- 岗位超编自动回退到容量上限。
- 工具损耗：`Farmers*0.12 + Scholars*0.18 + Merchants*0.12`
- 覆盖率低于 `55%` 时写入工具紧缺日志。

### 3.4 手动建造与制工具

- 建造前置：`Workers > 0`
- 建筑消耗：
  - 农坊：木24 / 石12 / 金10
  - 工坊：木26 / 石18 / 金12
  - 学宫：木16 / 石22 / 金22
  - 市集：木18 / 石14 / 金24
  - 官署：木20 / 石20 / 金20
- 制工具前置：`Workers > 0` 且 `WorkshopBuildings > 0`
- 制工具消耗：
  - 木：`8 + WorkshopBuildings*2`
  - 石：`6 + WorkshopBuildings*1.5`
  - 金：`4 + Workers*0.25`
- 工具产出：`WorkshopBuildings*18 + Workers*1.8`

### 3.5 矿仓联建（产业手动操作）

- 矿仓联建前置：`Workers > 0`
- 联建成本（随等级增长）：
  - 木：`18 + MiningLevel*4 + WarehouseLevel*5`
  - 石：`22 + MiningLevel*6 + WarehouseLevel*6`
  - 金：`14 + MiningLevel*3 + WarehouseLevel*4`
  - 建材：`3 + MiningLevel*1.5`
- 联建结果：
  - `MiningLevel +1`
  - `WarehouseLevel +1`
  - `WarehouseCapacity` 按第 4 节重算

### 3.6 岗位面板进程化显示

- `JobsPadding` 内每个岗位行都可点击展开/收起规则详情；优先按钮、加减按钮继续保留原交互。
- 岗位标题显示“当前已解锁的具体岗位名称”，不再只显示笼统职业统称。
- 展开详情时至少显示：`已派人数 / 容量上限 / 容量公式 / 当前建筑快照 / 当前郡学层级 / 下一阶岗位解锁条件`。
- 具体岗位名称需受建筑与科技进程共同驱动，规则如下：
  - 农务线：`田亩农户 -> 垄作农师 -> 农械整备员 -> 良种司圃`
    - `田亩农户`：默认解锁
    - `垄作农师`：`TechLevel >= 1` 且 `AgricultureBuildings >= 3`
    - `农械整备员`：`TechLevel >= 2` 且 `AgricultureBuildings >= 3` 且 `WorkshopBuildings >= 2`
    - `良种司圃`：`TechLevel >= 3` 且 `AgricultureBuildings >= 5` 且 `WorkshopBuildings >= 2`
  - 工务线：`里甲书吏 -> 营造把头 -> 工坊监造 -> 都料匠正`
    - `里甲书吏`：默认解锁
    - `营造把头`：`TechLevel >= 1` 且 `AdministrationBuildings >= 3`
    - `工坊监造`：`TechLevel >= 2` 且 `AdministrationBuildings >= 3` 且 `WorkshopBuildings >= 3`
    - `都料匠正`：`TechLevel >= 3` 且 `AdministrationBuildings >= 5` 且 `WorkshopBuildings >= 3`
  - 商务线：`集市行商 -> 商路牙郎 -> 柜坊账房 -> 商栈掌柜`
    - `集市行商`：默认解锁
    - `商路牙郎`：`TechLevel >= 1` 且 `TradeBuildings >= 2`
    - `柜坊账房`：`TechLevel >= 2` 且 `TradeBuildings >= 3` 且 `AdministrationBuildings >= 2`
    - `商栈掌柜`：`TechLevel >= 3` 且 `TradeBuildings >= 5` 且 `AdministrationBuildings >= 3`
  - 学务线：`蒙学塾师 -> 郡学讲郎 -> 格物博士 -> 司天校书`
    - `蒙学塾师`：默认解锁
    - `郡学讲郎`：`TechLevel >= 1` 且 `ResearchBuildings >= 2`
    - `格物博士`：`TechLevel >= 2` 且 `ResearchBuildings >= 3`
    - `司天校书`：`TechLevel >= 3` 且 `ResearchBuildings >= 4` 且 `AdministrationBuildings >= 3`

## 4. 资源链与仓储系统（Resource）

### 4.1 仓储容量

- 仓储容量公式：`900 + WarehouseLevel*260 + AdministrationBuildings*45`
- 仓储占用计入：粮、木料、石料、林木、原石、黏土、卤水、药材、麻料、芦苇、皮毛、精盐、药剂、麻布、皮革、工具、稀材、铁矿、铜矿、煤矿、铜锭、熟铁、金属锭、复合材料、工业部件、建造构件
- 超容处理：按优先级挤压库存（煤→铜→铁→部件→建材→锭→复材→木→石→粮→工具→稀材）

### 4.2 开采与冶炼

- 开采劳力系数：`clamp((Workers + Farmers*0.35)/(MiningLevel*12), 0.35, 1.25)`
- 开采产出：
  - 铁矿：`MiningLevel*3.6*劳力系数*科技系数*等级系数`
  - 铜矿：`MiningLevel*2.3*劳力系数*科技系数*等级系数`
  - 煤矿：`MiningLevel*2.9*劳力系数*科技系数*等级系数`
- 其中：
  - 科技系数：`1 + TechLevel*0.08`
  - 等级系数：`1 + (MiningLevel-1)*0.10`
- 冶炼（V1 现行）：
  - `铜矿1.3 + 煤0.45 -> 铜锭0.9`
  - `铁矿1.9 + 煤1.15 -> 熟铁0.95`
- `MetalIngot` 仅作为旧档兼容汇总字段保留，不再作为当前主结算产物。

### 4.3 新材料研发与制造

- 新材料研发前置：`TechLevel >= 2` 且 `Scholars > 0`
- 研发转化：`熟铁0.9 + 铜锭0.35 + 科研5.5 -> 复合材料0.9`
- 产品制造：`熟铁0.55 + 复材0.45 -> 工业部件1.1`，并附带 `IndustryTools + 部件*0.55`
- 建材制造：`石2.1 + 木1.6 + 黏土0.9 + 芦苇0.65 -> 建造构件1.0`，若额外投入 `熟铁0.18` 则产出倍率提升至 `1.12`

### 4.4 原材料分层扩展（DL-023 / V1）

> 状态：`2026-03-08` 已推进到“五期地图原料标注”。当前运行版已接入 `T0/T1` 材料库存、民生产线、`铜锭/熟铁` 冶铸、工具材料消耗、`木/石` 脱离经济直产、四条可扩建 `T0` 链路、仓储展示、“可见库存整数化 + 隐藏进度池”，以及郡图环境锚点的自然原材料来源节点/标签。旧版抽象资源字段继续保留作兼容兜底；当前剩余验证项为 `Godot` 运行烟测与完整读档回归。

#### 4.4.1 资源层级定义（强制）

- 原材料层：地图上可直接采集、砍伐、开采、狩猎、抽卤获得的自然资源。
  - 示例：`林木 / 原石 / 黏土 / 卤水(岩盐) / 药材 / 麻料 / 芦苇 / 皮毛兽骨 / 铜矿 / 铁矿`
- 一次加工层：原材料经过初步处理后得到的基础材料，但尚未形成复杂配方、合金或高温深加工结果。
  - 示例：`木料 / 木炭 / 切石 / 砖坯 / 粗盐 / 干药 / 麻纤 / 芦束 / 生皮 / 铜锭 / 铁胚`
- 二次加工层：需要配方、窑烧、鞣制、精炼或合金工艺得到的材料。
  - 示例：`陶器 / 熟铁 / 精盐 / 药剂 / 麻布 / 皮革`
- 成品层：可直接被建筑、人口、探险、治理、贸易等系统消费的最终物品。
  - 示例：`房梁 / 农具 / 铁件 / 衣物 / 绷带 / 皮衣`

#### 4.4.2 原材料层门禁（强制）

- 以下项目禁止进入原材料层：
  - `青铜`
  - `纸张`
  - `玻璃`
  - `皮革`
  - `火药`
  - `灰浆`
  - `建造构件`
  - `工业部件`
- 说明：
  - `青铜` 为 `铜锭 + 锡锭` 的合金结果，只能属于二次加工层；
  - `纸张` 为制浆后的轻工成品，不属于自然原料；
  - `玻璃` 为石英砂经高温工艺所得，不属于自然原料；
  - `皮革` 为生皮鞣制后的材料，不属于自然原料；
  - `火药` 为硫粉、硝粉、木炭混合后的后期成品。

#### 4.4.3 T0：原生材料与聚落起步

- 目标：满足基础建造、仓储、居住、初步健康与生活资料需求，不引入金属冶铸、合金与高温精工。
- 解锁原材料：
  - `林木 / 原石 / 黏土 / 卤水(岩盐) / 药材 / 麻料 / 芦苇 / 皮毛兽骨`
- 解锁建筑：
  - `林场 / 采石场 / 黏土坑 / 盐井(盐场) / 采药营 / 麻田 / 芦苇荡 / 猎场`
  - `锯木坊 / 石作坊 / 砖瓦窑 / 陶坊 / 晒药棚 / 纺线坊 / 草编坊 / 制皮棚`
- 当前运行版（三期显式链路）：
  - `林木链`：`林场 / 锯木坊`
  - `石陶链`：`采石场 / 黏土坑 / 石作坊`
  - `盐药链`：`盐井 / 采药营 / 晒药棚`
  - `纤皮链`：`麻田 / 芦苇荡 / 猎场 / 制皮棚`
- 以上四条链当前以 `ChainLevel` 形式存入 `GameState`，在仓储面板直接扩建；每条链等级越高，对应自然原料小时产出越高。
- 主要产物：
  - `木料 / 木炭 / 石料 / 切石 / 砖坯 / 陶器 / 粗盐 / 干药 / 麻纤 / 芦束 / 生皮`
- 系统作用：
  - 建筑扩张起步
  - 住房基础升级
  - 仓储容器建立
  - 初级健康保障
  - 初级衣物前置
  - 初级燃料建立

#### 4.4.4 T1：基础冶铸与民生整备

- 目标：引入铜、铁与更完整的民生加工链，建立工具效率、衣物覆盖、药剂恢复与基础商业器具。
- 解锁原材料：
  - `铜矿 / 铁矿`
- 解锁建筑：
  - `铜矿坑 / 铁矿坑 / 熔铜炉 / 冶铁炉 / 铜作坊 / 铁匠铺 / 煎盐坊 / 药铺 / 织布坊 / 制革坊`
- 主要产物：
  - `铜锭 / 铜器坯 / 铁胚 / 熟铁 / 精盐 / 药粉(药膏) / 麻线 / 麻布 / 皮革`
- 主要成品：
  - `铜器 / 钱币前置物 / 农具 / 铁件 / 药剂 / 衣物 / 绷带 / 皮衣`
- 系统作用：
  - 提升采集效率与产业效率
  - 建立衣物覆盖与健康恢复
  - 提升基础商业价值
  - 提供探险前置材料
- 限制：
  - `V1` 不解锁 `青铜 / 纸张 / 玻璃 / 火药`

#### 4.4.5 V1 资源到系统收益映射

- 建筑与基础设施：
  - `林木 / 原石 / 黏土` -> 建筑扩张、住房基础、仓储容器
- 民生与人口：
  - `卤水(岩盐) / 药材 / 麻料 / 芦苇 / 皮毛兽骨` -> 幸福、健康、衣物覆盖、居住舒适度
- 生产与产业：
  - `铜矿 / 铁矿` -> 工具效率、产业效率、基础商业器具
- 建议映射字段：
  - `ConstructionSpeedBonus`
  - `HousingQualityBonus`
  - `HappinessBonus`
  - `DiseaseRecoveryBonus`
  - `ClothingCoverage`
  - `GatheringEfficiency`
  - `IndustryEfficiency`
  - `TradeValueBonus`

#### 4.4.6 一期运行规则（已实现）

- 新增 `GameState` 材料字段：
  - `T0` 原料：`Timber / RawStone / Clay / Brine / Herbs / HempFiber / Reeds / Hides`
  - `民生材料`：`FineSalt / HerbalMedicine / HempCloth / Leather`
  - `T1` 冶铸：`CopperIngot / WroughtIron`
  - `T0` 链路等级：`ForestryChainLevel / MasonryChainLevel / MedicinalChainLevel / FiberChainLevel`
  - `隐藏进度池`：`DiscreteInventoryProgress`
- 旧存档兼容：
  - 若旧存档仍持有 `MetalIngot` 且 `CopperIngot / WroughtIron` 为空，则运行时自动拆分迁移为 `铜锭 + 熟铁`，不破坏旧存档读档。
  - 若旧存档没有 `T0` 链路等级，则按既有 `Agriculture / Workshop / Trade / Research` 建筑快照自动推导初始等级，避免读档后原料产线归零。
  - 若旧存档存在物品小数，则读档后自动拆为“整数可见库存 + 小数隐藏进度”。
- `ResourceSystem` 每小时新增三段结算：
- `山野采集`：按有效农务人口、管理加成、工具覆盖率产出 `林木 / 原石 / 黏土 / 卤水 / 药材 / 麻料 / 芦苇 / 皮毛`
  - `工坊初加工`：`林木 -> 木料`、`原石 -> 石料`
  - `民生产线`：`卤水 -> 精盐`、`药材 -> 药剂`、`麻料 -> 麻布`、`皮毛 -> 皮革`
- `T0` 链路倍率规则（三期）：
  - `林木链` 提升 `林木`
  - `石陶链` 提升 `原石 / 黏土`
  - `盐药链` 提升 `卤水 / 药材`
  - `纤皮链` 提升 `麻料 / 芦苇 / 皮毛`
- `IndustrySystem.TryBuildTierZeroChain`：
  - 可直接扩建上述四条 `T0` 链路
  - 资源消耗使用 `木料 / 石料 / 金 / 建造构件`
  - 成功后即时发布日志与状态刷新
- `InventoryRules` 四期规则：
  - 自动结算中的分数产出/分数消耗通过 `ApplyDelta` 写入隐藏进度；
  - 可见库存始终保持整数；
  - 仓储面板、顶部资源条、事件日志中的物品数量均以整数显示；
  - 手动建造/制工具/矿仓联建/T0 链扩建的成本按整数结算。
- `EconomySystem` 二期规则：
  - 经济结算不再直接增加 `木/石`
  - `木料 / 石料` 的新增只来自 `4.4.6` 的 `工坊初加工`
- `T1` 冶铸一期公式：
  - `铜矿 1.3 + 煤 0.45 -> 铜锭 0.9`
  - `铁矿 1.9 + 煤 1.15 -> 熟铁 0.95`
- `IndustrySystem.TryCraftTools` 一期规则：
  - 若存在 `熟铁/铜锭`，制工具优先消耗 `熟铁 + 铜锭 + 工业部件`
  - 若旧档尚未产出新材料，则回退到 `铁矿` 旧链路，保证兼容
- `PopulationSystem` 一期收益挂钩：
  - `精盐` 影响幸福与患病率
  - `药剂` 影响康复率与死亡率
  - `麻布 / 皮革` 与 `ClothingStock` 共同决定衣物覆盖率
- `WarehousePanel` 一期展示：
  - 可直接观察 `T0` 原料、`民生材料`、`铜锭/熟铁` 与现有 `工业部件/建造构件`
  - `T2/T3` 材料仍禁止提前进入前台展示
- `PrefectureMapGeneratorSystem / StrategicMapViewSystem` 五期规则：
  - 郡图环境锚点会生成“自然原材料来源”节点与标签，只展示 `林木 / 药材 / 皮毛`、`卤水 / 芦苇 / 黏土`、`原石 / 铜矿 / 铁矿`、`麻料`
  - 地图视觉与节点层不展示 `青铜 / 纸张 / 玻璃 / 皮革 / 火药` 等加工品名称

#### 4.4.7 与现有抽象资源的兼容映射

- 在未完全拆细账前，允许继续保留：
  - `MetalIngot`：由 `铜锭 / 熟铁` 汇总贡献
  - `CompositeMaterial`：暂不扩展，待 `T2/T3` 再接入
  - `IndustrialComponent`：由 `铁件 / 铜件` 等后续汇总
  - `BuildingComponent`：由 `木料 / 切石 / 砖坯` 等后续汇总
- 该映射用于保持主循环、存档结构与 UI 稳定，不构成最终细账模型。

## 5. 经济系统（Economy）

### 4.1 有效岗位

- 有效产业工人：`min(Farmers, 产业容量)`
- 有效研发：`min(Scholars, 研发容量)`
- 有效商业：`min(Merchants, 商业容量)`

### 4.2 产出结算

- 生产系数：`管理加成 * 工具覆盖率`
- 粮食：`+有效产业工人 * 2.4 * FoodMultiplier * 生产系数`
- 木料 / 石料：不在经济系统中直产，统一由 `4.4.6` 的材料采集与初加工链提供
- 金币：`+有效商业 * 0.86 * TradeMultiplier * 生产系数`
- 科研：`+有效研发 * 0.46 * 生产系数`

### 4.3 薪资与惩罚

- 居民薪资：`Population*0.05`
- 管理薪资：`Workers*0.11`
- 金币不足时：金币归零，幸福 `-1.2`

## 6. 科研突破系统（Research）

- T1 阈值：`Research >= 30`
- T2 阈值：`Research >= 90`
- T3 阈值：`Research >= 180`

突破效果（只升不降）：

- T1：`FoodProductionMultiplier = 1.15`
- T2：`IndustryProductionMultiplier = 1.15`，`TradeProductionMultiplier = 1.10`
- T3：`PopulationGrowthMultiplier = 1.20`

## 7. 人口系统（Population）

### 6.1 基础结算

- 粮食消耗：`Population*0.65`
- 若粮食为负：
  - 人口损失：`ceil(abs(Food)*0.08)`（人口最低 `20`）
  - 粮食置零
  - 幸福 `-4.5`

### 6.2 增长公式

- `housingFactor = clamp(HousingCapacity / Population, 0.5, 1.15)`
- `foodReserveFactor = clamp(Food / (Population*2), 0.4, 1.2)`
- `happinessFactor = clamp(Happiness/100, 0.3, 1.3)`
- `growthRate = 0.006 * housingFactor * foodReserveFactor * happinessFactor * PopulationGrowthMultiplier`
- 新增人口：`floor(Population * growthRate)`（不为负）

### 6.3 幸福修正

- 粮食心情：高储备 `+0.8`，否则 `-0.3`
- 住房心情：容量够 `+0.5`，不足 `-1.0`
- 威胁心情：`-(Threat*0.06)`
- 富裕心情：金币高于人口 `+0.35`，否则 `-0.25`
- 最终幸福：`clamp(Happiness, 5, 100)`

### 6.4 岗位溢出回退顺序

`Scholar -> Merchant -> Worker -> Farmer`

## 8. 繁育系统（Breeding）

- 触发前置：`Population >= 100`
- 触发概率：`0.12 + Happiness/400`
- 成功后精英增长：
  - `8%` 概率出生 2 名
  - 否则出生 1 名
- `16%` 概率触发突变：`AvgGearScore +0.6`

## 9. 探险与战斗系统（Combat）

### 8.1 触发条件

- 若探险关闭或精英人数为 0：仅威胁 `+0.2`，不结算战斗。
- 战斗每 `3` 小时结算一次（`ExplorationProgressHours`）。

### 8.2 胜负判定

- 敌方强度：`9 + ExplorationDepth*1.6`
- 队伍强度：`ElitePopulation*0.95 + AvgGearScore*1.1`
- 胜率：`clamp(0.2 + (teamPower-enemyPower)/28, 0.12, 0.9)`

### 8.3 胜利结果

- 金币：`+18 + depth*3`
- 稀有素材：`+1`，额外 `35%` 概率再 `+1`
- 威胁：`-2.2`（下限 0）
- `38%` 概率层数 `+1`
- 进入装备掉落判定（见第 10 节）

### 8.4 失败结果

- 威胁：`+2.4`
- 若 `ElitePopulation > 1`，`22%` 概率精英 `-1`

## 10. 装备掉落系统（Equipment）

- 掉落概率：`clamp(0.35 + depth*0.03, 0.35, 0.80)`
- 词条概率：`clamp(0.22 + depth*0.015, 0.22, 0.60)`
- 词条评分倍率：`1.35`

品质权重（随层数变化）：

- 普通：`max(20, 62-depth*2)`
- 精良：`min(40, 26+depth*1.5)`
- 史诗：`min(25, 10+depth*0.6)`
- 传说：`min(15, 2+depth*0.2)`

品质基础评分增量：

- 普通 `+0.25`
- 精良 `+0.55`
- 史诗 `+0.95`
- 传说 `+1.60`

落地结果：

- 更新 `AvgGearScore`
- 更新各品质计数（`Common/Rare/Epic/Legendary`）
- 产出可视日志

## 11. 郡县动态事件系统（CountyEvent）

- 冷却中（`EventCooldownHours > 0`）仅减冷却，不触发。
- 候选事件：
  - 商路集市：`Merchants >= 10 && Happiness >= 55`
  - 学宫讲习：`Scholars >= 8`
  - 边境袭扰：`Threat >= 42`
- 总触发概率：`clamp(0.12 + 候选数*0.08 + Threat*0.0015, 0.12, 0.48)`
- 抽取方式：按权重随机，触发后写资源变化与日志。

## 12. 宗门 hex 驻地地图系统（SectMap）

- 网格固定：`22x16`
- 默认种子：`20260306`（当传入种子为 0）
- 目标房屋数：`clamp(round(Pop*0.22 + Housing*0.05 + Elite*0.6), 18, 72)`
- 生成流程：主街 -> 支路 -> 路肩庭院 -> 建筑放置 -> 水域点缀
- 场所锚点：至少生成 `灵田 / 炼器坊 / 山门坊市 / 藏经阁 / 宗务殿 / 论道亭` 六类活动锚点（底层仍复用原 `TownActivityAnchorType` 枚举以保持兼容）
- 场所实体：每个锚点除临路 `RoadCell` 外，还会生成相邻的 `LotCell / Facing / Floors / VisualVariant`，用于在地图上落成实体场所建筑
- 场所选址：优先选择紧邻道路的空地或庭院地块，避免与住宅建筑冲突；若局部过密则放宽邻近密度约束但仍不占用道路/水域
- 宗门投影：宗门主视图使用 pointy-top hex tile 俯视布局；逻辑网格仍保留 `TownMapData(width,height)` 的二维坐标，不改生成规则
- 渲染方式：宗门地表与建筑改为运行时程序化 hex 俯视纯绘制，不依赖独立 tile 资产目录
- 正式入口：`WorldPanel` 当前主地图区域只保留 `宗门地图 / 世界地图` 两类入口；仓储弹窗继续保留在顶部按钮，不再将郡图/事件/报表/探险作为主地图页签
- 表现层次：hex 地表、建筑、场所与居民继续分层绘制，保持地图可读性与缩放反馈
- footprint 语义：住宅建筑底盘、场所底座、场所选中高亮与命中区统一按 hex 几何表达；屋顶、墙面与装饰 accent 继续使用轻量 overlay
- 地形语义：道路格会额外绘制 hex 核心通路与相邻格连接带；水域格会额外绘制内层水面、岸线高亮与轻量波纹，强化宗门 hex 地形识别
- 地图仅用于可视化反馈，不改写经济/人口核心数值

## 12.1 战略地图配置系统（StrategicMap）

- 配置路径：`CountyIdle/data/strategic_maps.json`
- 配置目标：世界图/郡图的区域、边界、路线、河流、节点由数据定义
- 底格语义：`grid_lines` 表示战略底格密度；当前视图层按 hex 战略底格渲染，而非矩形辅助线
- 渲染约束：归一化坐标区间建议 `[-1.0, 1.0]`，超界会导致裁切
- 缩放约束：地图缩放统一夹紧为 `60% ~ 220%`，默认 `100%`
- 容错策略：配置文件缺失或反序列化失败时回退内置默认配置，不中断主循环
- 启动校验：加载时检查点数下限、坐标建议区间（`±1.20`）与颜色格式（`#RRGGBB/#RRGGBBAA`），异常写 `GD.PushWarning`

## 11.2 外域态势备用视图程序化生成（Village 风格）

- 适用范围：`Prefecture` 隐藏备用视图启用程序化生成，对外标题统一为 `外域态势`；`World` 对外标题统一为 `世界地图`。
- 生成输入：人口、住房、威胁、小时结算数（均取非负）。
- 分桶策略（避免频繁重建）：
  - 人口桶：`population / 24`
  - 住房桶：`housing / 30`
  - 威胁桶：`floor(threat / 8)`
  - 结算桶：`hourSettlements / 6`
- 生成骨架：
- 有机边界：`18~24` 点极坐标扰动，默认尺度扩大（外域州府式大图）
  - 聚落节点：`clamp(16 + pop/60 + housing/120, 16, 34)` 个，环绕县城主节点分布
- 自然/人工区域：森林、湖泊、农田、山脉、外域内城区块
  - 道路：县城放射路 + 聚落环路 + 边界连接路 + 城内网格坊巷 + 御街主轴
  - 河流：主河（`7` 点）+ 支流（`4` 点）组合
  - 城内建筑：地标（府衙/州桥/寺院/码头/官仓等）+ `7~11 x 6~10` 的坊市网格高密建筑节点
- 标签系统：战略地图支持 `labels`（位置、文本、颜色、字号、`min_zoom`），用于地貌与建筑说明展示，并按缩放分级显隐。
- 郡图缩放增强：`Prefecture` 模式最大缩放提升到 `560%`，并采用平滑惯性缩放。
- 高倍率街区渲染：当外域备用视图缩放进入高倍率时，叠加整座外城级别的长街、横街、里巷纹理；坊市渲染为沿街连续街屋、市肆摊列与巷内庭院块面，地标/外城渲染为院落式建筑群。
- 外域命名主题配置：`CountyIdle/data/prefecture_city_theme.json`
  - 可配置项：地图标题、城名、地貌名称、主街名称、城门名称（东/西/南/北）、地标名称列表、坊市名称池
  - 容错策略：缺失/空字段自动回退默认命名，不中断生成
- 设计约束：仅影响地图视觉反馈，不改经济/人口/战斗核心数值与结算顺序。

## 13. 双地图视图与缩放（Sect / World）

- 宗门地图与世界地图均支持滚轮缩放（区间 `0.6 ~ 2.2`，默认 `1.0`）。
- 主界面缩放按钮仅对当前激活地图页生效（宗门/世界）。
- 页签切换时刷新缩放显示，不影响主循环与结算状态。
- 世界图底层统一绘制 hex 战略底格；宗门图沿用独立 hex 驻地绘制链路。
- 默认节点标记采用六角标记；世界图 edge overlay 用于河流、道路、桥梁、河岸与悬崖边表现。
- 当前实现：世界地图 = 修仙 Hex 世界生成（`strategic_maps.json` 世界定义保留 fallback）；宗门地图 = 复用当前 hex 驻地视图链路并统一宗门文案。
- 全局表现语义：除宗门地图本体外，仓储、岗位、研究突破、资源日志、事件提示、地图经营按钮与世界图标题统一使用 `灵田 / 炼器坊 / 藏经阁 / 山门坊市 / 宗务殿 / 宗门 / 世界地图` 术语；隐藏备用 Prefecture 视图标题改为 `外域态势`，避免旧郡县文案漏出。
- 外域备用视图运营语义：隐藏备用 `Prefecture` 视图的状态与调度统一使用 `灵道 / 聚落 / 抚恤聚落 / 山野采集` 术语；即使 fallback 到该视图，也不再出现旧外域行政式提示词。

## 13.1 修仙 Hex 世界生成系统（Xianxia World Generator）

- 配置路径：`CountyIdle/data/xianxia_world_generation.json`
- 入口系统：`XianxiaWorldGenerationConfigSystem` 负责加载/归一化配置；`XianxiaWorldGeneratorSystem` 负责生成 hex 世界数据与战略视图适配层。
- 世界尺寸：默认 `64x40`（`2560` 个 hex cell），配置允许区间 `24x16 ~ 128x96`。
- 世界结构：
  - `Biome`：`temperate_plains / bamboo_valley / misty_mountains / sacred_forest / jade_highlands / snow_peaks / crystal_fields / volcanic_wastes / spirit_swamps / ancient_ruins_land / desert_badlands / floating_isles`
  - `Terrain`：草地/森林/山地/湿地/雪地/火山/晶域/灵壤/古遗迹/浮空地基等 `24` 类基础地形
  - `Water / Cliff / Overlay / Resource / SpiritualZone / Structure / Wonder`：作为 cell 层语义字段叠加，不单独改写主循环结算
  - `RiverMask / CliffMask / RoadMask`：作为 hex 共边语义字段，供河流、道路、悬崖 edge overlay 使用
- 生成流程：
  1. 基于 axial hex 坐标生成高度、温度、湿度、肥力、腐化、灵气密度等基础场；
  2. 叠加山脉、生水、河流、悬崖、龙脉与灵气区域；
  3. 根据地形与灵气区域分配资源、奇观、宗门候选、聚落/坊市/遗迹；
  4. 输出 `XianxiaWorldMapData` 与 `StrategicMapDefinition` 两份结果，后者直接喂给 `StrategicMapViewSystem`。
- 数据结构约束：
  - `XianxiaHexCellData` 为单格最小单位，持有 `coord / height / climate / biome / terrain / water / cliff / overlay / resource / spiritual_zone / structure / wonder / river_mask / cliff_mask / road_mask / render`
  - `DragonVeinPathData / RiverPathData / SectCandidateSiteData / WonderSiteData / XianxiaSiteData` 为世界级聚合对象
  - `render` 内维护 tile key、变体索引与皮肤 key，便于未来切换为真正的资源图块
- 世界图适配：
  - 世界图默认优先使用修仙生成器；若生成异常，回退 `StrategicMapConfigSystem.GetWorldDefinition()`
  - 适配层会把 hex cell 转成 `StrategicPolygonDefinition`；龙脉继续转成 `StrategicPolylineDefinition`；宗门候选/聚落/奇观/稀有资源转成 `StrategicNodeDefinition` 与 `StrategicLabelDefinition`
  - 世界图模式下会直接读取 `RiverMask / RoadMask` 沿 hex 共边绘制 overlay，因此河流和道路优先表达为边语义，而非单格中心纹理
  - edge overlay 二期细节：河岸按 `Water` 与相邻非水格边界绘制 shoreline；当 `RiverMask` 与 `RoadMask` 重叠时绘制桥梁；当 `RoadMask` 连接数 `>= 3` 时绘制路口节点；`CliffMask` 额外绘制悬崖暗边
  - 默认配置（`seed = 20260308`）一次生成结果已验证为：`2560` 地块、`9` 条河流、`6` 条灵脉、`9` 处奇观、`12` 个宗门候选、`22` 个场所点位
  - 二期补强后，默认配置额外验证为：`242` 个河流边语义格、`111` 个道路边语义格、`38` 个悬崖边语义格、`9` 个水域格、`8` 个桥梁边重叠格、`10` 个路口格
- 设计约束：只增强世界图信息表达，不改 `GameState`、小时结算、人口/产业/战斗核心公式。

## 14. 主界面与客户端设置

- 双布局（Legacy/Figma）共享同一 `GameLoop` 与 `GameState`。
- 倍速按钮只改时间流速，不改“60 分钟结算”规则。
- Legacy 布局新增 `仓储弹窗` 入口：
  - 由 `WarehousePanel.tscn` 作为独立弹出式界面
  - 按类别展示库存：基础资源 / 矿石资源 / 材料与产成品
  - 提供快捷操作：矿仓联建、制工具、扩建工坊、扩建官署
  - 弹窗打开时支持 `Esc` 快速关闭
  - 提示条根据仓储负载显示操作建议，执行快捷操作后短暂显示请求反馈
- 客户端设置存储 `user://client_settings.json`：
  - 语言：`zh_CN / en`
  - 分辨率白名单：`1280x720 / 1600x900 / 1920x1080 / 2560x1440`
  - 字体缩放：`0.85 ~ 1.30`
  - 主音量：`0.0 ~ 1.0`
  - 快捷键：支持打开设置、打开仓储、探险开关、倍速切换、快速存档、快速读档、快速重置，且可在设置面板点击后按键录制
  - 设置/仓储弹窗共享提示条倒计时与 `Esc` 关闭交互
  - 录制支持冲突自动交换，并在提示条显示绑定结果；录制中 `Esc` 取消录制，非录制状态下 `Esc` 关闭设置面板

## 15. 存档与兼容

- 存档介质：`user://countyidle.db`（SQLite）
- 旧版兼容：若数据库不存在但检测到 `user://savegame.json`，首次读档时自动迁移为默认槽快照
- 当前写入策略：完整 `GameState` JSON 快照写入 SQLite，不在本阶段将 `GameState` 完全拆表
- 默认存档槽：
  - `slot_key = default`
  - `slot_name = 主存档`
- 当前表结构：
  - `schema_migrations`：记录 SQLite schema 版本
  - `save_slots`：记录槽位元信息（键、名称、更新时间、游戏分钟、人口、金钱、科技等级、民心、威胁、探险层数、仓储负载）
  - `save_snapshots`：记录 `GameState` JSON 快照
- 当前交互：
  - 主界面“存档 / 读档”按钮打开多存档槽面板
  - 面板支持：查看、覆盖所选槽、读取所选槽、新建手动槽、复制所选槽为新手动槽、重命名、删除、刷新
  - 列表与详情区域会显示槽位日期、科技、民心、威胁、探险层数、仓储负载等摘要
  - 面板支持按 `全部 / 主存档 / 手动槽 / 自动槽` 筛选，并可按最近写入、游戏进度、人口、金钱、科技排序
  - 每次写入槽位后会尝试保存当前画面截图到 `user://save_previews/<slot_key>.png`，并在面板详情区显示预览
  - 快速存档 / 快速读档继续直连默认槽 `default`
  - 自动存档槽：`autosave / autosave_2 / autosave_3`
- 自动存档规则（二期：3 槽轮换）：
  - 每跨过 `6` 次小时结算自动写入一次自动存档
  - 自动存档按 `自动存档 1 -> 自动存档 2 -> 自动存档 3` 循环覆盖
  - 三个自动存档槽均可读取，但不可手动覆盖、重命名、删除
- 兼容策略：
  - 新增字段保留默认值，保证旧快照与旧 JSON 可读
  - SQLite 初始化失败时不得破坏当前运行中的 `GameState`
  - 读档失败回退新状态并写日志
- V1 边界：
  - 已实现默认槽与快照表
  - 已实现手动多槽管理 UI
  - 已实现 3 槽轮换自动存档
  - 已实现存档摘要预览增强
  - 已实现多槽面板筛选与排序
  - 已实现截图预览
  - 已实现槽位复制分支
  - 未实现云存档、事件流水表、资源流水表、外部导入导出

## 16. 人口分配与生活循环系统（Population Allocation，一期）

### 16.1 设计目标与边界

- 主目标：让人口受“住房、食物、睡眠、情绪、衣物、通勤”共同约束，并可解释地影响岗位有效产能。
- 结算边界：仍按每 `60` 游戏分钟小时结算，不引入分钟级实时路径模拟。
- 伦理约束：职业只体现分工，不对职业群体赋予高低价值评价。

### 16.2 状态输入与新增字段（建议）

- 输入复用：`Population`、`HousingCapacity`、`Food`、`Happiness`、`IndustryTools`、岗位分配。
- 建议新增字段（默认值保证旧存档可读）：
  - `ChildPopulation`（默认 `0`）
  - `AdultPopulation`（默认 `Population`）
  - `ElderPopulation`（默认 `0`）
  - `SickPopulation`（默认 `0`）
  - `ClothingStock`（默认 `0`）
  - `AverageCommuteDistanceKm`（默认 `1.2`）
  - `RoadMobilityMultiplier`（默认 `1.0`，夹紧 `0.7~1.3`）
- 一期输入说明：`AverageCommuteDistanceKm` 由“人口规模 + 住房承压 + 产业建筑数量”动态估算，后续再接入地图空间映射。

### 16.3 住房与睡眠承载

- 每小时睡眠需求：`sleepNeed = Population * 0.33`
- 每小时可睡容量：`sleepCapacity = HousingCapacity * 0.40`
- 睡眠恢复系数：`sleepFactor = clamp(sleepCapacity / max(sleepNeed, 1), 0.55, 1.05)`
- 居住拥挤度：`housingPressure = clamp((Population - HousingCapacity) / max(Population, 1), 0, 0.45)`
- 影响规则：
  - `housingPressure > 0`：幸福追加惩罚 `-(housingPressure * 2.4)`
  - `sleepFactor < 0.8`：岗位到岗率与疾病恢复速度同时下降

### 16.4 食物、衣物与健康

- 衣物覆盖：`clothingCoverage = clamp(ClothingStock / max(Population, 1), 0.30, 1.00)`
- 每小时衣物损耗：`Population * 0.012`
- 患病率：`sicknessRate = 0.002 + housingPressure*0.012 + max(0, 0.75-sleepFactor)*0.010 + (1-clothingCoverage)*0.006`
- 新增病患：`newSick = floor(AdultPopulation * sicknessRate)`
- 每小时恢复：`recover = floor(SickPopulation * (0.11 + clothingCoverage*0.06))`
- 约束：`SickPopulation` 夹紧到 `[0, AdultPopulation]`

### 16.5 生老病死（小时聚合）

- 出生：`births = floor(AdultPopulation * 0.0032 * clamp(Happiness/100, 0.5, 1.2) * clamp(HousingCapacity/max(Population,1), 0.6, 1.1))`
- 成长：`childToAdult = floor(ChildPopulation * 0.0018)`
- 衰老：`adultToElder = floor(AdultPopulation * 0.0009)`
- 死亡率：`deathRate = 0.0004 + (SickPopulation/max(Population,1))*0.0015 + max(0, -Food)/max(Population,1)*0.004`
- 死亡人数：`deaths = ceil(Population * deathRate)`（下限 `0`，人口总量下限 `20`）

### 16.6 通勤步行与到岗率

- 单程通勤时长（分钟）：`commuteMinutes = clamp(AverageCommuteDistanceKm / (4.2 * RoadMobilityMultiplier) * 60, 3, 45)`
- 到岗系数：`onDutyFactor = clamp((60 - commuteMinutes) / 60, 0.25, 0.95)`
- 健康劳动力系数：`healthLaborFactor = clamp(1 - SickPopulation / max(AdultPopulation, 1), 0.45, 1.0)`
- 岗位有效人数（一期已接入经济结算）：`effectiveJob = floor(jobAssigned * onDutyFactor * sleepFactor * healthLaborFactor)`
- 观测要求：`commuteMinutes >= 35` 时写“远距通勤导致迟到”日志。

### 16.7 情绪修正扩展（在第 7.3 基础上叠加）

- 睡眠情绪：`sleepFactor >= 0.95` 则 `+0.4`，`<0.8` 则 `-0.9`
- 衣物情绪：`clothingCoverage >= 0.9` 则 `+0.3`，`<0.6` 则 `-0.7`
- 通勤情绪：`-(commuteMinutes/60)*0.8`
- 疾病情绪：`-(SickPopulation/max(Population,1))*1.6`
- 最终幸福仍夹紧：`clamp(Happiness, 5, 100)`

### 16.8 验收与回滚护栏

- 验收（实现后）：
  - 人口增减由出生/衰老/死亡/患病恢复四类日志可解释；
  - 住房或通勤恶化会在 `1~2` 小时内反馈到有效岗位产能；
  - 全部人口与岗位相关字段保持非负，存档可读。
- 回滚触发：连续 `6` 小时出现“有效岗位下降且食物/金币同步恶化”。
- 回滚方式：保留字段，停用本节系数，退回第 7 节当前人口增长与幸福规则。

---

## 影响摘要（Flywheel）

- 人口繁衍：由食物/住房/睡眠/衣物/通勤/疾病多因子控制，具备出生、衰老、患病、死亡与恢复反馈。
- 职业分化：岗位容量、工具覆盖、管理加成共同决定产能。
- 资源深加工：矿石经冶炼与研发形成新材料，再反哺工具、建材与扩建节奏。
- 精英繁育：低频高价值增量，与探险战力形成联动。
- 武装探险：按 3 小时节奏产出金币/稀材/装备并影响威胁。
- 反哺郡县：科研突破、事件收益和建造扩产回流主城发展。

## 可执行工单句（交接用）

“在不改变 `60 分钟小时结算` 与 `GameState` 存档兼容的前提下，按 `Industry -> Resource -> Economy` 串行结算资源链，并同步更新本文对应章节与改动记录。”

“在保持 `1 秒=1 分钟` 与小时结算节奏不变的前提下，为人口系统接入住房睡眠、衣物健康与步行通勤到岗率，并将有效岗位人数替换为 `onDutyFactor * sleepFactor * healthLaborFactor` 的聚合计算。”

## 17. 县城居民可视移动系统（Town Residents）

- 作用范围：仅影响 `SectMapViewSystem / CountyTownMapViewSystem` 视觉层，不改写经济、人口、战斗核心数值。
- 生成输入：`Population`、`Farmers`、`Workers`、`Merchants`、`Scholars`、宗门住房建筑分布。
- 可视人数：`clamp(Population / 12, 8, 24)`，按岗位占比切分到居民队列。
- 居民形象：优先加载 `assets/characters/residents/*.png` 正式 sprite 资源，按职业类别区分；资源缺失时才回退运行时占位贴图。
- 作息规则：以 `GameMinutes % 1440` 作为日程时钟，居民按“居家 -> 通勤上工 -> 工作 -> 外出休闲 -> 回家”顺序切换。
- 职业作息：农工、匠役、商贾、学士使用不同上工/收工时段，并允许 `±12` 分钟队列错峰。
- 场所绑定：工作地与休闲点优先绑定 `TownActivityAnchorData` 具体锚点；农工去灵田、匠役去炼器坊、商贾去山门坊市、学士去藏经阁，收工后统一去论道亭等休闲点。
- 场所实体：每个 `TownActivityAnchorData` 额外持有 `LotCell / Facing / Floors / VisualVariant`，并在宗门地图上绘制为小型场所建筑，不再只是圆点标记。
- 渲染层级：住宅与场所实体统一按 hex 行中心的屏幕纵向排序，保证前后遮挡关系稳定；居民仍以临路 `RoadCell` 作为到岗/休闲停留点。
- 交互规则：宗门地图支持左键选中实体场所、右键取消选中；提示条会显示场所名称、当前状态与可视弟子统计。
- 状态提示：工作类场所根据“前往中 / 运转中 / 暂歇中”展示当前时段，休闲类场所展示“有人前往 / 论道中 / 静修中”，宗务殿当前作为议事节点展示。
- 运动规则：仅在通勤与外出阶段沿既定路线移动；居家与工作/休闲阶段停留在住房入口、工作点或休闲点，不做随机游走。
- 刷新规则：当宗门地图重建、岗位结构变化或人口档位明显变化时，重建居民队列。
- 性能护栏：单次绘制居民上限 `24`，仅在宗门地图视图中更新动画。
- 验收要求：地图上存在可见居民移动、地图刷新无异常、`dotnet build .\Finally.sln` 通过。

## 18. 地图与经营状态联动（Map Operation Link）

- 作用范围：`World / Prefecture / CountyTown` 三类地图页共用一套经营状态映射。
- 经营 → 地图：
  - 世界图重点读取：`Happiness`、`Threat`、`Gold`、`Food`；
  - 外域备用视图重点读取：`Threat`、`Happiness`、`AverageCommuteDistanceKm`、`RoadMobilityMultiplier`；
  - 县城图重点读取：`HousingCapacity / Population`、`Threat`、`AverageCommuteDistanceKm`、`RoadMobilityMultiplier`。
- 地图状态档位：`繁荣 / 平稳 / 吃紧 / 紧张`，至少影响：
  - 地图标题附加状态词；
  - 地图状态条提示文案；
  - 地图主体色调（外域备用视图/世界图）或建筑/地块色调（县城图）。
- 地图 → 经营：
  - 地图页提供双按钮调度入口；
  - 当前调度包括：外域 `整修灵道 / 抚恤聚落` 与宗门 `修整坊路 / 夜巡清巷`；
  - 调度即时改变 `Happiness`、`Threat`、`AverageCommuteDistanceKm`、`RoadMobilityMultiplier` 及部分资源库存，并立刻发布日志与状态刷新。
  - 道路类调度通过 `MapCommuteReductionBonusKm` 与 `MapRoadMobilityBonus` 持久化到 `GameState`，再由 `PopulationRules.RefreshDynamicCommute` 回写到实时通勤表现。
- 护栏：
  - 调度不得使资源、威胁、通勤等字段出现负值异常；
  - 不改变小时结算顺序；
  - Figma 布局可暂不暴露地图调度入口，但核心状态计算需保持兼容。
