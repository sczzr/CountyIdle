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

`Industry -> Economy -> Research -> Population -> Breeding -> Combat -> CountyEvent`

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

## 4. 经济系统（Economy）

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

## 5. 科研突破系统（Research）

- T1 阈值：`Research >= 30`
- T2 阈值：`Research >= 90`
- T3 阈值：`Research >= 180`

突破效果（只升不降）：

- T1：`FoodProductionMultiplier = 1.15`
- T2：`IndustryProductionMultiplier = 1.15`，`TradeProductionMultiplier = 1.10`
- T3：`PopulationGrowthMultiplier = 1.20`

## 6. 人口系统（Population）

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

## 7. 繁育系统（Breeding）

- 触发前置：`Population >= 100`
- 触发概率：`0.12 + Happiness/400`
- 成功后精英增长：
  - `8%` 概率出生 2 名
  - 否则出生 1 名
- `16%` 概率触发突变：`AvgGearScore +0.6`

## 8. 探险与战斗系统（Combat）

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
- 进入装备掉落判定（见第 9 节）

### 8.4 失败结果

- 威胁：`+2.4`
- 若 `ElitePopulation > 1`，`22%` 概率精英 `-1`

## 9. 装备掉落系统（Equipment）

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

## 10. 郡县动态事件系统（CountyEvent）

- 冷却中（`EventCooldownHours > 0`）仅减冷却，不触发。
- 候选事件：
  - 商路集市：`Merchants >= 10 && Happiness >= 55`
  - 学宫讲习：`Scholars >= 8`
  - 边境袭扰：`Threat >= 42`
- 总触发概率：`clamp(0.12 + 候选数*0.08 + Threat*0.0015, 0.12, 0.48)`
- 抽取方式：按权重随机，触发后写资源变化与日志。

## 11. 县城 2.5D 地图系统（TownMap）

- 网格固定：`22x16`
- 默认种子：`20260306`（当传入种子为 0）
- 目标房屋数：`clamp(round(Pop*0.22 + Housing*0.05 + Elite*0.6), 18, 72)`
- 生成流程：主街 -> 支路 -> 路肩庭院 -> 建筑放置 -> 水域点缀
- 地图仅用于可视化反馈，不改写经济/人口核心数值

## 11.1 战略地图配置系统（StrategicMap）

- 配置路径：`CountyIdle/data/strategic_maps.json`
- 配置目标：世界图/郡图的区域、边界、路线、河流、节点由数据定义
- 渲染约束：归一化坐标区间建议 `[-1.0, 1.0]`，超界会导致裁切
- 缩放约束：地图缩放统一夹紧为 `60% ~ 220%`，默认 `100%`
- 容错策略：配置文件缺失或反序列化失败时回退内置默认配置，不中断主循环
- 启动校验：加载时检查点数下限、坐标建议区间（`±1.20`）与颜色格式（`#RRGGBB/#RRGGBBAA`），异常写 `GD.PushWarning`

## 12. 战略地图视图与缩放（World / Prefecture）

- 世界地图与郡图均支持滚轮缩放（区间 `0.6 ~ 2.2`，默认 `1.0`）。
- 主界面缩放按钮仅对当前激活地图页生效（世界/郡图/县城）。
- 页签切换时刷新缩放显示，不影响主循环与结算状态。
- 当前实现以程序化绘制为主；配置驱动 (`StrategicMapConfigSystem`) 为后续扩展位。

## 13. 主界面与客户端设置

- 双布局（Legacy/Figma）共享同一 `GameLoop` 与 `GameState`。
- 倍速按钮只改时间流速，不改“60 分钟结算”规则。
- 客户端设置存储 `user://client_settings.json`：
  - 语言：`zh_CN / en`
  - 分辨率白名单：`1280x720 / 1600x900 / 1920x1080 / 2560x1440`
  - 字体缩放：`0.85 ~ 1.30`
  - 主音量：`0.0 ~ 1.0`

## 14. 存档与兼容

- 存档路径：`user://savegame.json`
- 写入方式：完整 `GameState` JSON
- 兼容策略：新增字段保留默认值，读取失败回退新状态并写日志

---

## 影响摘要（Flywheel）

- 人口繁衍：由食物/住房/幸福多因子控制，具备增长与退化反馈。
- 职业分化：岗位容量、工具覆盖、管理加成共同决定产能。
- 精英繁育：低频高价值增量，与探险战力形成联动。
- 武装探险：按 3 小时节奏产出金币/稀材/装备并影响威胁。
- 反哺郡县：科研突破、事件收益和建造扩产回流主城发展。

## 可执行工单句（交接用）

“在不改变 `60 分钟小时结算` 与 `GameState` 存档兼容的前提下，仅对目标系统做单一目标调整，并同步更新本文对应章节与改动记录。”
