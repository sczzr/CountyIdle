# CountyIdle 系统规格（整合精简版）

> 本文是当前版本（v0.x）唯一系统规则源，所有机制改动先更新这里。

## 0. 全局基线

- 技术基线：Godot 4.2+ / C# / .NET 8
- 时间基线：`1 秒现实时间 = 1 游戏分钟`
- 倍速范围：`x1 ~ x2`（由 `GameLoop.SetTimeScale` 夹紧）
- 小时结算：每累计 `60` 游戏分钟触发一次
- 核心要求：人口、产业、科技、职业、探险、郡县治理必须互相影响，禁止孤立子系统
- 价值观要求：职业分化不等于职业高低，系统与文案禁止职业歧视表达

## 1. 小时结算顺序（严格）

`Industry -> Resource -> Economy -> Research -> Population -> Breeding -> Combat -> CountyEvent`

- 由 `scripts/core/GameLoop.cs` 串行调度。
- 每个子系统只改自己负责的状态，不中断主循环。
- 结算完成后发布 `GameState.Clone()` 到 UI。

## 2. 状态与安全约束（GameState）

- 人口、岗位、建筑数量、工具库存不得出现负值。
- 资源扣减路径必须有可支付检查；失败写日志，不抛异常中断。
- 科技倍率下限为 `1.0`，防止旧存档或异常值导致 0 倍率。
- 存档兼容：新增字段依赖默认值保证旧存档可读。

## 3. 产业与岗位系统（Industry）

### 3.1 岗位容量公式

- 产业工人容量：`AgricultureBuildings*16 + WorkshopBuildings*12`
- 研发容量：`ResearchBuildings*10`
- 商业容量：`TradeBuildings*14`
- 管理容量：`AdministrationBuildings*8`

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

## 4. 资源链与仓储系统（Resource）

### 4.1 仓储容量

- 仓储容量公式：`900 + WarehouseLevel*260 + AdministrationBuildings*45`
- 仓储占用计入：粮、木、石、工具、稀材、铁矿、铜矿、煤矿、金属锭、复合材料、工业部件、建造构件
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
- 冶炼：`铁2.2 + 铜1.2 + 煤1.6 -> 金属锭0.95`（受工坊产能上限约束）

### 4.3 新材料研发与制造

- 新材料研发前置：`TechLevel >= 2` 且 `Scholars > 0`
- 研发转化：`金属锭1.4 + 科研5.5 -> 复合材料0.9`
- 产品制造：`金属锭0.8 + 复材0.45 -> 工业部件1.1`，并附带 `IndustryTools + 部件*0.55`
- 建材制造：`石2.6 + 木1.9 + 金属锭0.45 -> 建造构件1.0`

## 5. 经济系统（Economy）

### 4.1 有效岗位

- 有效产业工人：`min(Farmers, 产业容量)`
- 有效研发：`min(Scholars, 研发容量)`
- 有效商业：`min(Merchants, 商业容量)`

### 4.2 产出结算

- 生产系数：`管理加成 * 工具覆盖率`
- 粮食：`+有效产业工人 * 2.4 * FoodMultiplier * 生产系数`
- 木材：`+有效产业工人 * 0.62 * IndustryMultiplier * 生产系数`
- 石材：`+有效产业工人 * 0.48 * IndustryMultiplier * 生产系数`
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

## 12. 县城 2.5D 地图系统（TownMap）

- 网格固定：`22x16`
- 默认种子：`20260306`（当传入种子为 0）
- 目标房屋数：`clamp(round(Pop*0.22 + Housing*0.05 + Elite*0.6), 18, 72)`
- 生成流程：主街 -> 支路 -> 路肩庭院 -> 建筑放置 -> 水域点缀
- 场所锚点：至少生成 `农田作业点 / 工坊作业点 / 市集 / 学宫 / 官署 / 茶肆` 六类活动锚点
- 场所实体：每个锚点除临路 `RoadCell` 外，还会生成相邻的 `LotCell / Facing / Floors / VisualVariant`，用于在地图上落成实体场所建筑
- 场所选址：优先选择紧邻道路的空地或庭院地块，避免与住宅建筑冲突；若局部过密则放宽邻近密度约束但仍不占用道路/水域
- 渲染资产：县城地表优先读取由 `assets/picture/Isometric_game_asset_sheet_topdown_view_zen_chines_delpmaspu.png` 提取得到的 `assets/tiles/county_reference_isometric/county_reference_isometric_atlas.png` 与 manifest；atlas 缺失时回退到原 seamless 纹理面片
- atlas 语义：`Ground -> grass_0..3`、`Courtyard -> courtyard_0..2`、`Water -> water_0`、`Road -> road_0..15（四向连通 mask）`
- 装饰规则：非建筑格位按 `layoutSeed + cell hash` 稳定抽取原图中的 `fern / flowers / rocks / stone slab / stepping` 等 overlay，不改写地图语义数据
- 立体层次：地表改为 atlas 驱动，住宅/场所建筑与居民仍保留 overlay 绘制，避免首轮改造同时重写主链路
- 地图仅用于可视化反馈，不改写经济/人口核心数值

## 12.1 战略地图配置系统（StrategicMap）

- 配置路径：`CountyIdle/data/strategic_maps.json`
- 配置目标：世界图/郡图的区域、边界、路线、河流、节点由数据定义
- 渲染约束：归一化坐标区间建议 `[-1.0, 1.0]`，超界会导致裁切
- 缩放约束：地图缩放统一夹紧为 `60% ~ 220%`，默认 `100%`
- 容错策略：配置文件缺失或反序列化失败时回退内置默认配置，不中断主循环
- 启动校验：加载时检查点数下限、坐标建议区间（`±1.20`）与颜色格式（`#RRGGBB/#RRGGBBAA`），异常写 `GD.PushWarning`

## 11.2 周边郡图程序化生成（Village 风格）

- 适用范围：`Prefecture`（周边郡图）模式启用程序化生成，`World`（天下州域）继续走配置驱动。
- 生成输入：人口、住房、威胁、小时结算数（均取非负）。
- 分桶策略（避免频繁重建）：
  - 人口桶：`population / 24`
  - 住房桶：`housing / 30`
  - 威胁桶：`floor(threat / 8)`
  - 结算桶：`hourSettlements / 6`
- 生成骨架：
  - 有机边界：`18~24` 点极坐标扰动，默认尺度扩大（开封城式大图）
  - 聚落节点：`clamp(16 + pop/60 + housing/120, 16, 34)` 个，环绕县城主节点分布
  - 自然/人工区域：森林、湖泊、农田、山脉、郡城内城区块
  - 道路：县城放射路 + 聚落环路 + 边界连接路 + 城内网格坊巷 + 御街主轴
  - 河流：主河（`7` 点）+ 支流（`4` 点）组合
  - 城内建筑：地标（府衙/州桥/寺院/码头/官仓等）+ `7~11 x 6~10` 的坊市网格高密建筑节点
- 标签系统：战略地图支持 `labels`（位置、文本、颜色、字号、`min_zoom`），用于地貌与建筑说明展示，并按缩放分级显隐。
- 郡图缩放增强：`Prefecture` 模式最大缩放提升到 `560%`，并采用平滑惯性缩放。
- 高倍率街区渲染：当郡图缩放进入高倍率时，叠加整座郡城级别的御街、横街、里巷纹理；坊市渲染为沿街连续街屋、市肆摊列与巷内庭院块面，地标/郡城渲染为院落式建筑群。
- 开封命名主题配置：`CountyIdle/data/prefecture_city_theme.json`
  - 可配置项：地图标题、城名、地貌名称、主街名称、城门名称（东/西/南/北）、地标名称列表、坊市名称池
  - 容错策略：缺失/空字段自动回退默认命名，不中断生成
- 设计约束：仅影响地图视觉反馈，不改经济/人口/战斗核心数值与结算顺序。

## 13. 战略地图视图与缩放（World / Prefecture）

- 世界地图与郡图均支持滚轮缩放（区间 `0.6 ~ 2.2`，默认 `1.0`）。
- 主界面缩放按钮仅对当前激活地图页生效（世界/郡图/县城）。
- 页签切换时刷新缩放显示，不影响主循环与结算状态。
- 当前实现：天下州域 = 配置驱动；周边郡图 = 程序化生成（Village 风格）。

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

- 存档路径：`user://savegame.json`
- 写入方式：完整 `GameState` JSON
- 兼容策略：新增字段保留默认值，读取失败回退新状态并写日志

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

- 作用范围：仅影响 `CountyTownMapViewSystem` 视觉层，不改写经济、人口、战斗核心数值。
- 生成输入：`Population`、`Farmers`、`Workers`、`Merchants`、`Scholars`、县城住房建筑分布。
- 可视人数：`clamp(Population / 12, 8, 24)`，按岗位占比切分到居民队列。
- 居民形象：优先加载 `assets/characters/residents/*.png` 正式 sprite 资源，按职业类别区分；资源缺失时才回退运行时占位贴图。
- 作息规则：以 `GameMinutes % 1440` 作为日程时钟，居民按“居家 -> 通勤上工 -> 工作 -> 外出休闲 -> 回家”顺序切换。
- 职业作息：农工、匠役、商贾、学士使用不同上工/收工时段，并允许 `±12` 分钟队列错峰。
- 场所绑定：工作地与休闲点优先绑定 `TownActivityAnchorData` 具体锚点；农工去农田作业点、匠役去工坊作业点、商贾去市集、学士去学宫，收工后统一去茶肆等休闲点。
- 场所实体：每个 `TownActivityAnchorData` 额外持有 `LotCell / Facing / Floors / VisualVariant`，并在县城地图上绘制为小型场所建筑，不再只是圆点标记。
- 渲染层级：住宅与场所实体统一按等距深度排序，保证前后遮挡关系稳定；居民仍以临路 `RoadCell` 作为到岗/休闲停留点。
- 交互规则：县城地图支持左键选中实体场所、右键取消选中；提示条会显示场所名称、当前状态与可视居民统计。
- 状态提示：工作类场所根据“前往中 / 开工中 / 已收工”展示当前时段，休闲类场所展示“有人前往 / 热闹中 / 清闲中”，官署当前作为政务节点展示。
- 运动规则：仅在通勤与外出阶段沿既定路线移动；居家与工作/休闲阶段停留在住房入口、工作点或休闲点，不做随机游走。
- 刷新规则：当县城地图重建、岗位结构变化或人口档位明显变化时，重建居民队列。
- 性能护栏：单次绘制居民上限 `24`，仅在县城地图视图中更新动画。
- 验收要求：地图上存在可见居民移动、地图刷新无异常、`dotnet build .\Finally.sln` 通过。

## 18. 地图与经营状态联动（Map Operation Link）

- 作用范围：`World / Prefecture / CountyTown` 三类地图页共用一套经营状态映射。
- 经营 → 地图：
  - 世界图重点读取：`Happiness`、`Threat`、`Gold`、`Food`；
  - 郡图重点读取：`Threat`、`Happiness`、`AverageCommuteDistanceKm`、`RoadMobilityMultiplier`；
  - 县城图重点读取：`HousingCapacity / Population`、`Threat`、`AverageCommuteDistanceKm`、`RoadMobilityMultiplier`。
- 地图状态档位：`繁荣 / 平稳 / 吃紧 / 紧张`，至少影响：
  - 地图标题附加状态词；
  - 地图状态条提示文案；
  - 地图主体色调（郡图/世界图）或建筑/地块色调（县城图）。
- 地图 → 经营：
  - 地图页提供双按钮调度入口；
  - 一期调度包括：驿路整修、乡里赈济、街坊修整、夜巡清巷；
  - 调度即时改变 `Happiness`、`Threat`、`AverageCommuteDistanceKm`、`RoadMobilityMultiplier` 及部分资源库存，并立刻发布日志与状态刷新。
  - 道路类调度通过 `MapCommuteReductionBonusKm` 与 `MapRoadMobilityBonus` 持久化到 `GameState`，再由 `PopulationRules.RefreshDynamicCommute` 回写到实时通勤表现。
- 护栏：
  - 调度不得使资源、威胁、通勤等字段出现负值异常；
  - 不改变小时结算顺序；
  - Figma 布局可暂不暴露地图调度入口，但核心状态计算需保持兼容。
