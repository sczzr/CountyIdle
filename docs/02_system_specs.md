# CountyIdle 系统规格（整合精简版）

> 本文是当前版本（v0.x）唯一系统规则源。  
> 世界观与术语先看 `docs/09_xianxia_sect_setting.md`；公式、顺序、边界以本文为准。

## 0. 全局基线

- 技术基线：Godot 4.2+ / C# / .NET 8
- 时间基线：`1 秒现实时间 = 1 游戏分钟`
- 倍速范围：`x1 / x2 / x4`（由 `GameLoop.SetTimeScale` 夹紧）
- 小时结算：每累计 `60` 游戏分钟触发一次
- 时间显示：采用架空“景禾历”，即 `12 月 * 30 日 * 24 时`，全年融入 `24` 节气
- 季度规则：每 `3` 个月为一个季度，每季度固定 `90` 日，对应春/夏/秋/冬四季
- 节气规则：每 `15` 日切换一个节气；每月固定覆盖 `2` 个节气
- UI 约束：顶部不显示“结算倒计时”，顶栏统一改为“日期 + 季度进度 + 当日进度 + 季节 / 节气 / 时辰”
- 叙事基线：玩家扮演浮云宗掌门 / 宗主，当前直接经营焦点为天衍峰，并通过青云峰三大总殿协调全宗事务
- 世界观基线：对外设计与文案统一为“浮云宗（青云州江陵府）+ 天衍峰 + 青云峰总殿 + 外域附庸圈层”
- 历史命名说明：文中引用 `County / Town / Prefecture` 等名称时，默认指代码层兼容命名，不代表玩家可见设定
- 核心要求：门人、产业、传承、职司、历练、宗门经营必须互相影响，禁止孤立子系统
- 价值观要求：职司分化不等于人的高低，系统与文案禁止职业歧视表达

### 0.1 玩家可见语义映射（强制）

| 技术名 | 当前文档与 UI 说法 | 说明 |
| --- | --- | --- |
| `Population` | 浮云宗门人总量 / 宗门人口池 | 包含凡俗依附者、杂役、弟子等抽象总和 |
| `Farmers / Workers / Merchants / Scholars` | 阵材 / 阵务 / 总坊外事 / 推演职司汇总 | 当前由宗主中枢方略自动折算，不再由玩家直接手动加减 |
| `ContributionPoints` | 贡献点 | 宗门内部流通货币，用于内务建设、锻器与天衍峰驻地调度 |
| `Research` | 传承研修 / 功法技艺推演 | 当前仍可保留“科技”技术说法 |
| `Breeding` | 灵根苗子培养 / 后辈培育 | 当前为精英繁育系统的对外语义 |
| `Combat` | 外务历练 / 护山战备 | 包含外域探索与后续防御场景 |
| `CountyEvent` | 宗门见闻 / 山门事件 | 坊市、讲法、袭扰等动态事件 |
| `CountyTown` | 浮云宗驻地 / 天衍峰山门图 | 仅为历史技术名 |
| `Prefecture` | 江陵府外域 / 附庸圈层 | 仅为历史技术名 |

## 1. 小时结算顺序（严格）

`Industry -> Resource -> Economy -> Research -> Population -> Breeding -> Combat -> CountyEvent`

- 由 `scripts/core/GameLoop.cs` 串行调度。
- 小时结算前置：先将 `TaskOrderUnits` 解析为四类职司汇总，再进入上述固定顺序。
- 其中 `CountyEvent` 对外语义为“宗门见闻 / 山门事件”。
- 每个子系统只改自己负责的状态，不中断主循环。
- 结算完成后发布 `GameState.Clone()` 到 UI。
- 结算完成后再次同步任务折算，确保人口变化、建筑扩建后 UI 立即看到最新职司摘要。
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
- 任务状态：
  - `ContributionPoints`：宗门内务货币，玩家可见整数值；
  - `TaskOrderUnits`：保存“治理条目 -> 当前力度刻度”，作为方略层到底层结算的兼容桥；
  - `TaskResolvedWorkers`：保存“治理条目 -> 底层自动折算出的实际投入人数”，仅供系统结算与读档恢复使用，不再作为玩家主界面重点信息；
  - 上述字段只负责方略层与旧结算层之间的兼容，`Farmers / Workers / Merchants / Scholars` 继续作为内部汇总层供现有系统结算。

## 3. 产业与岗位系统（Industry）

### 3.1 岗位容量公式

- 产业工人容量：`AgricultureBuildings*16 + WorkshopBuildings*12`
- 研发容量：`ResearchBuildings*10`
- 商业容量：`TradeBuildings*14`
- 管理容量：`AdministrationBuildings*8`

### 3.1.1 宗主中枢方略（强制）

- 玩家不再直接调整 `Farmers / Workers / Merchants / Scholars`。
- 玩家通过“宗主中枢”决定天衍峰的发展方向、法令力度与任务重点，当前治理条目固定为：
  - `阵材采炼` -> `Farmer`
  - `阵枢营造` -> `Worker`
  - `巡山警戒` -> `Worker`
  - `阵法推演` -> `Scholar`
  - `总坊值守` -> `Merchant`
  - `外事行商` -> `Merchant`
- 玩家可见交互只体现“收敛 / 推进 / 鼎力推进 / 恢复均衡”，不再暴露逐项人数配置。
- 当前兼容层实现：
  - `TaskOrderUnits` 继续保留为“力度刻度”；
  - 底层会按既有 `WorkforcePerOrder` 规则把力度刻度转换为内部请求人数；
  - 再按 `任务优先级 -> 该职司容量 -> 总人口` 逐项折算为 `TaskResolvedWorkers`；
  - 以上人数折算仅为兼容现有产业 / 经济 / 研究系统，宗主界面不直接展示。
- 当前优先级：
  - `阵材采炼 -> 阵枢营造 -> 巡山警戒 -> 阵法推演 -> 总坊值守 -> 外事行商`
- 同步时机（强制）：
  - `LoadState`
  - `ResetState`
  - 玩家调整法旨后
  - 小时结算开始前
  - 小时结算结束后（用于反映人口/建筑变化）

### 3.1.2 职司摘要旧面板（JobsPadding，已迁出主界面）

- `JobsPadding` 已从主界面左侧常驻区移除，不再占用 `Tile Inspector` 下方空间。
- 原“职司摘要 / 九峰概览 / 峰脉详批 / 协同峰令”能力已迁入独立 `SectOrganizationPanel` 卷册弹窗，由底部 `【峰令】谱系` 快捷按钮打开。
- 独立卷册继续复用 `SectOrganizationRules`、`SectPeakSupportRules` 与 `GameLoop.SetPeakSupport() / ResetPeakSupport()`，但不再回灌为主界面左栏常驻块。
- 该卷册支持 `Esc` 收卷，并可从谱系内直接跳转 `宗主中枢`。
- 新主界面以“选中 tile → 查看状态 → 执行动作”为优先动线，治理与组织信息退为二级入口。

### 3.1.3 宗主治理三层（发展方向 / 法令 / 育才）

- 宗主中枢当前固定包含三层治理选择：
  - `发展方向`：`均衡发展 / 供养优先 / 研修优先 / 护山优先 / 外务优先`
  - `宗门法令`：`宽和养民 / 严整戒律 / 尚功奖绩 / 开坛传习`
  - `育才方略`：`广纳新徒 / 阵师深造 / 执事磨砺 / 外务历练`
- 玩家交互规则：
  - 每层只允许同时激活 `1` 个选项；
  - 切换 `发展方向` 时，系统立即按该方向重排默认治理条目侧重；
  - 切换 `法令 / 育才` 时，立即影响现有小时结算加成；
  - “恢复均衡”会重置为：`均衡发展 / 宽和养民 / 广纳新徒`，并同步重排默认治理条目。
- 当前数值效果（一期最小闭环）：
  - `供养优先`：提升供养向产出，并提高相关治理条目默认侧重；
  - `研修优先`：提升研修积累，并提高 `阵法推演` 默认侧重；
  - `护山优先`：压低威胁、提高贡献回流，并提高 `巡山警戒` 默认侧重；
  - `外务优先`：提升灵石回流，并提高 `总坊值守 / 外事行商` 默认侧重；
  - `宽和养民 / 严整戒律 / 尚功奖绩 / 开坛传习` 分别影响 `民心 / 威胁 / 贡献 / 研修`；
  - `广纳新徒 / 阵师深造 / 执事磨砺 / 外务历练` 分别影响 `人口增长 / 研修积累 / 贡献回流 / 灵石回流`。

### 3.1.4 峰脉协同法旨（组织谱系卷册）

- `SectOrganizationPanel` 的峰脉详情区支持直接下发“本季协同峰”法旨，默认状态为 `诸峰均衡`。
- 当前视觉：卷册已统一为“峰令谱”书卷子面板，采用 `左右木轴 + 上下绫边 + 卷首横题 + 左览右批 + 墨线批令` 结构，并与 `治宗册 / 留影录 / 机宜卷 / 库房账册 / 弟子谱` 保持同族语汇。
- 当前可用协同峰：
  - `青云峰`：总殿统筹更稳，偏 `贡献 / 民心`
  - `天衍峰`：阵研与推演提速，偏 `研修 / 贡献 / 工器`
  - `天枢峰`：外务与物流更畅，偏 `灵石 / 贡献`
  - `天机峰`：传承研修更强，偏 `研修 / 民心`
  - `天工峰`：制造与工器更强，偏 `贡献 / 工器`
  - `天权峰`：护山戒备更强，偏 `贡献 / 威胁压制`
  - `天元峰`：后勤与育成更强，偏 `食物 / 人口增长 / 民心`
  - `天衡峰`：密防与肃查更强，偏 `研修 / 威胁压制`
  - `其余支柱峰`：诸堂并援，提供较弱但更均衡的综合加成
- 数值挂点（一期最小闭环）：
  - 小时结算：影响 `食物 / 灵石 / 贡献 / 研修` 四项回流；
  - 门人生息：影响 `人口增长 / 民心 / 威胁`；
  - 工器锻制：影响 `IndustryTools` 的单次锻制产出；
- 玩家交互规则：
  - 点选某峰后可直接“设为本季协同峰”；
  - 已激活的协同峰不可重复下令；
  - “恢复均衡”会回到 `诸峰均衡`，撤销单峰偏置。

### 3.1.5 季度法令（宗主治理三期先行）

- 宗主中枢追加第 `4` 条治理层：`季度法令`。
- 当前季度法令选项：
  - `本季无加令`：常态章程，无专项偏置；
  - `开库赈济`：偏 `供养 / 民心 / 人口增长`；
  - `开坛季讲`：偏 `研修 / 传承氛围`；
  - `护山检阅`：偏 `威胁压制 / 贡献回流`；
  - `坊市开榷`：偏 `灵石回流 / 内外账务流通`；
  - `百工会炼`：偏 `工器锻制 / 营造保障 / 贡献`
- 数值挂点（一期最小闭环）：
  - 小时结算：影响 `食物 / 灵石 / 贡献 / 研修`；
  - 门人生息：影响 `人口增长 / 民心 / 威胁`；
  - 工器锻制：影响 `IndustryTools` 单次产出；
- 季度轮换规则：
  - 法令记录为“按季度颁布”的专项法令；
  - 当季度切换到下一季时，若仍有上季法令，系统自动清空为 `本季无加令` 并提示玩家重新颁令；
  - “恢复均衡”会同步撤销当前季度法令。

### 3.1.6 门规树（一期：三支门规纲目）

- 宗主中枢继续追加 `3` 条常设门规支线：
  - `庶务门规`
  - `传功门规`
  - `巡山门规`
- 当前门规节点：
  - `庶务门规`：`庶务常制 / 抚恤新徒 / 尚功明录 / 百工验收`
  - `传功门规`：`传功常制 / 静室修行 / 月考讲评 / 开阁阅卷`
  - `巡山门规`：`巡山常制 / 夜巡验符 / 执法先行 / 峰门互保`
- 数值挂点（一期最小闭环）：
  - `庶务门规`：影响 `食物 / 贡献 / 人口增长 / 工器锻制`
  - `传功门规`：影响 `研修 / 少量贡献 / 门人情绪`
  - `巡山门规`：影响 `威胁 / 少量贡献 / 门人情绪`
- 交互规则：
  - 每条门规支线同一时刻仅激活 `1` 个节点；
  - 门规属于常设章程，不随季度自动失效；
  - “恢复均衡”会把三支门规同时恢复到对应 `常制` 节点。

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
  - 阵材圃：木24 / 石12 / 灵石10 / 贡献10
  - 傀儡工坊：木26 / 石18 / 灵石12 / 贡献12
  - 传法院：木16 / 石22 / 灵石22 / 贡献18
  - 青云总坊：木18 / 石14 / 灵石24 / 贡献16
  - 庶务殿：木20 / 石20 / 灵石20 / 贡献14
- 制工具前置：`Workers > 0` 且 `WorkshopBuildings > 0`
- 制工具消耗：
  - 木：`8 + WorkshopBuildings*2`
  - 石：`6 + WorkshopBuildings*1.5`
  - 灵石：`4 + Workers*0.25`
  - 贡献：`6 + WorkshopBuildings*0.5`
- 工具产出：`WorkshopBuildings*18 + Workers*1.8`

### 3.5 矿仓联建（产业手动操作）

- 矿仓联建前置：`Workers > 0`
- 联建成本（随等级增长）：
  - 木：`18 + MiningLevel*4 + WarehouseLevel*5`
  - 石：`22 + MiningLevel*6 + WarehouseLevel*6`
  - 灵石：`14 + MiningLevel*3 + WarehouseLevel*4`
  - 贡献：`18 + MiningLevel*2 + WarehouseLevel*2`
  - 建材：`3 + MiningLevel*1.5`
- 联建结果：
  - `MiningLevel +1`
  - `WarehouseLevel +1`
  - `WarehouseCapacity` 按第 4 节重算

### 3.6 浮云宗·宗主中枢面板（TaskPanel）

- 入口：
  - 主界面底部 `【宗令】中枢` 快捷按钮
  - `SectOrganizationPanel` 内的“转宗主中枢”治理跳转按钮
- 当前视觉：已统一为“治宗册”书卷子面板，采用木轴、宣纸主卷、卷首批注、治宗法旨、治务条目与条目详批结构。
- 面板内容至少包含：
  - `灵石`
  - `贡献点`
  - `当前治宗重心 / 执事执行态势`
  - `发展方向 / 宗门法令 / 育才方略`
  - 治理条目列表与详情
- 交互约束：
  - 支持 `收敛 / 推进 / 鼎力推进 / 恢复均衡`
  - 关闭方式支持右上角关闭与 `Esc`
  - 面板刷新必须读取 `GameState.Clone()`，不直接持有可变主状态引用

### 3.7 浮云宗·弟子谱面板（DisciplePanel）

- 入口：
  - 主地图页签行新增“弟子谱”按钮
- 数据来源：
  - 基于 `GameState.Clone()` 经 `DiscipleRosterSystem` 派生整册弟子快照；
  - 仅作为独立 UI 展示层，不新增小时结算字段，也不额外写入存档结构。
- 面板内容至少包含：
  - 名册列表（姓名 / 身份 / 职司 / 修为）；
  - 筛选：`全部弟子 / 真传名册 / 阵材职司 / 阵务职司 / 外事职司 / 推演职司 / 待命轮值`；
  - 排序：`名册顺序 / 修为优先 / 潜力优先 / 心境优先 / 贡献优先`；
  - 布局结构：`左侧纵向名册卷册 + 右侧宣纸档案页`；
  - 当前视觉：已接入统一书卷子面板外壳，补齐 `左右木轴 + 上下绫边 + 卷首横题`，并将左栏收口为“峰内名录”目录语义；
  - 名册树层级：`峰脉 -> 堂口 / 机构 -> 条线 / 班序 -> 册级（真传 / 内门 / 外门 / 新苗 / 候值等） -> 弟子叶节点`；
  - 名册分组规则：峰脉与堂口优先按固定宗门组织树归类，不完全依赖弟子关联文本的第一段；
  - 档案头部：`姓名 / 骨龄 / 册录分组 / 职司 / 谱位` 与 `灵根圆环`；
  - 个体详情：`当前差事 / 居所 / 关联峰脉 / 特征 / 培养建议 / 衍天批注`；
  - 属性可视化：`悟性 / 潜力 / 根骨 / 匠艺 / 神魂 / 心境` 六维根基罗盘 + 修为进度 / 战力印鉴 / 气海储备。
- 交互约束：
  - 关闭方式支持右上角关闭与 `Esc`；
  - 状态刷新跟随主界面 `OnStateChanged` 同步，默认保持上次筛选与选中对象；
- 天衍峰山门图提供地块检视入口，但不提供弟子/场所点击联动，“弟子谱”仍通过底部入口打开；
  - 名册为派生展示层，不允许在该面板直接改动人口、岗位或资源数值。

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

> 状态：`2026-03-08` 已推进到“六期修仙材料语义化”。当前运行版已接入 `T0/T1` 材料库存、民生产线、`铜锭/熟铁` 冶铸、工具材料消耗、`木/石` 脱离经济直产、四条可扩建 `T0` 链路、仓储展示、“可见库存整数化 + 隐藏进度池”、江陵府外域环境锚点的自然原材料来源节点/标签，以及玩家可见材料名的修仙语义统一。旧版抽象资源字段继续保留作兼容兜底；`SQLite` 存档/读档回归已通过 `tools/SaveSmoke` 验证，当前剩余验证项为 `Godot` 运行烟测（本机环境暂未发现 `Godot` 可执行）。

- 六期玩家可见命名规则：
  - 前台与日志统一显示修仙材料名，例如 `灵谷 / 灵木料 / 青罡石料 / 灵木 / 青罡原石 / 寒泉卤水 / 灵草 / 青麻 / 青芦 / 灵兽皮 / 赤铜矿 / 玄铁矿 / 地火煤 / 辟谷精盐 / 养气散 / 青麻布 / 灵皮革 / 天材 / 赤铜锭 / 玄铁锭 / 灵纹复材 / 机关部件 / 护山构件`
  - `GameState` 与存档字段继续沿用 `Food / Wood / Stone / Timber / RawStone / Brine / Herbs / ...` 技术名，不做破坏兼容的重命名
  - 玩家可见命名统一由 `MaterialSemanticRules` 维护，避免仓储、日志、地图和主界面文案漂移

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

#### 4.4.3 T0：原生材料与附庸据点起步

- 目标：满足基础建造、仓储、居住、初步健康与生活资料需求，不引入金属冶铸、合金与高温精工。
- 解锁原材料：
  - `林木 / 原石 / 黏土 / 卤水(岩盐) / 药材 / 麻料 / 芦苇 / 皮毛兽骨`
- 解锁建筑：
  - `林场 / 采石场 / 黏土坑 / 盐井(盐场) / 采药营 / 麻田 / 芦苇荡 / 猎场`
  - `锯木坊 / 石作坊 / 砖瓦窑 / 陶坊 / 晒药棚 / 纺线坊 / 草编坊 / 制皮棚`
- 当前运行版（三期显式链路，六期前台语义）：
  - `灵木链`：`灵植园 / 伐木坊`
  - `石陶链`：`采罡场 / 赤陶窑 / 石作坊`
  - `盐丹链`：`盐泉 / 采药圃 / 丹房`
  - `织裘链`：`青麻圃 / 青芦泽 / 灵兽围 / 制裘坊`
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
- `峰外采办`：按有效农务人口、管理加成、工具覆盖率产出 `灵木 / 青罡原石 / 赤陶土 / 寒泉卤水 / 灵草 / 青麻 / 青芦 / 灵兽皮`
  - `工坊初加工`：`灵木 -> 灵木料`、`青罡原石 -> 青罡石料`
  - `民生产线`：`寒泉卤水 -> 辟谷精盐`、`灵草 -> 养气散`、`青麻 -> 青麻布`、`灵兽皮 -> 灵皮革`
- `T0` 链路倍率规则（三期）：
  - `灵木链` 提升 `灵木`
  - `石陶链` 提升 `青罡原石 / 赤陶土`
  - `盐丹链` 提升 `寒泉卤水 / 灵草`
  - `织裘链` 提升 `青麻 / 青芦 / 灵兽皮`
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
  - 江陵府外域环境锚点会生成“自然原材料来源”节点与标签，只展示 `灵木 / 灵草 / 灵兽皮`、`寒泉卤水 / 青芦 / 赤陶土`、`青罡原石 / 赤铜矿 / 玄铁矿`、`青麻`
  - 地图视觉与节点层不展示 `青铜 / 纸张 / 玻璃 / 皮革 / 火药` 等加工品名称
- `tools/SaveSmoke` 验证（2026-03-08）：
  - 输出 `slot_count=5 -> final_slot_count=4`，覆盖 `主存档 / 自动存档 1~3 / 手动槽` 的保存、读取与删除
  - 成功读取 `autosave_2`，返回 `schema=1`
  - 验证生成 `save-smoke.db`，确认 SQLite 存档链路可落盘

#### 4.4.7 与现有抽象资源的兼容映射

- 在未完全拆细账前，允许继续保留：
  - `MetalIngot`：由 `铜锭 / 熟铁` 汇总贡献
  - `CompositeMaterial`：暂不扩展，待 `T2/T3` 再接入
  - `IndustrialComponent`：由 `铁件 / 铜件` 等后续汇总
  - `BuildingComponent`：由 `木料 / 切石 / 砖坯` 等后续汇总
- 该映射用于保持主循环、存档结构与 UI 稳定，不构成最终细账模型。

## 5. 经济系统（Economy）

### 4.1 有效任务投入

- `有效阵材采炼 = floor(TaskResolvedWorkers[FieldDuty] * 劳动力可用系数)`
- `有效阵法推演 = floor(TaskResolvedWorkers[ScriptureStudy] * 劳动力可用系数)`
- `有效总坊值守 = floor(TaskResolvedWorkers[SectCommerce] * 劳动力可用系数)`
- `有效外事行商 = floor(TaskResolvedWorkers[OuterTrade] * 劳动力可用系数)`
- `劳动力可用系数 = 到岗率 * 睡眠系数 * 健康劳作系数`

### 4.2 产出结算

- 生产系数：`管理加成 * 工具覆盖率`
- 阵材与粮谷：`+有效阵材采炼 * 2.4 * FoodMultiplier * 生产系数`
- 木料 / 石料：不在经济系统中直产，统一由 `4.4.6` 的材料采集与初加工链提供
- 灵石：
  - `+有效总坊值守 * 0.26 * TradeMultiplier * 生产系数`
  - `+有效外事行商 * 1.18 * TradeMultiplier * 生产系数`
- 贡献点：
  - `+阵材采炼 * 0.08 * 生产系数`
  - `+阵枢营造 * 0.12 * 生产系数`
  - `+巡山警戒 * 0.18 * 生产系数`
  - `+阵法推演 * 0.10 * 生产系数`
  - `+总坊值守 * 0.45 * 生产系数`
  - `外事行商` 不产出贡献点
- 传承研修：`+有效阵法推演 * 0.46 * 生产系数`

### 4.3 双轨货币规则

- 宗门内部事项统一走 `贡献点 + 灵石` 双轨：
  - 建筑扩建
  - 锻制工器
  - `T0` 链路扩建
  - 矿仓联建
  - 天衍峰山门图内务调度（修整坊路、夜巡清巷）
- 宗门外交易 / 外务只使用灵石：
  - `外事行商`
  - 外域调度中的灵道整修、外域抚恤等外务
- 贡献点不能替代灵石用于宗门外交易。

### 4.4 薪资与惩罚

- 居民薪资：`Population*0.05`
- 管理薪资：`Workers*0.11`
- 灵石不足时：灵石归零，幸福 `-1.2`

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

## 11. 宗门动态事件系统（CountyEvent）

- 冷却中（`EventCooldownHours > 0`）仅减冷却，不触发。
- 候选事件：
  - 青云总坊：`Merchants >= 10 && Happiness >= 55`
  - 藏经讲法：`Scholars >= 8`
  - 山门袭扰：`Threat >= 42`
- 总触发概率：`clamp(0.12 + 候选数*0.08 + Threat*0.0015, 0.12, 0.48)`
- 抽取方式：按权重随机，触发后写资源变化与日志。

## 12. 宗门 hex 驻地地图系统（SectMap）

- 网格固定：`22x16`
- 默认种子：`20260306`（当传入种子为 0）
- 生成流程：当前已生成以 `Ground` 为主的六角地貌，并补入最小可用 `Road / Courtyard / Water` terrain 语义，用于地图表现层与地块检视摘要
- 宗门投影：宗门主视图使用 pointy-top hex tile 俯视布局；逻辑网格仍保留 `TownMapData(width,height)` 的二维坐标，不改生成规则
- 渲染方式：宗门地表已支持 `Layer 1` atlas + `Layer 2` decal / connector 的运行时接入；当前优先复用现有六边形草地图集与轻量 decal 贴图，作为正式国风量产素材接入前的过渡方案
- 正式入口：`WorldPanel` 当前主地图区域只保留 `天衍峰山门图 / 世界地图` 两类入口；仓储弹窗继续保留在顶部按钮，不再将江陵府外域备用视图/事件/报表/探险作为主地图页签
- 表现层次：当前已接入 `Layer 1` 基础地块与 `Layer 2` 道路 / 院坪 / 水域表现；`Layer 3` 建筑与立体物件、`Layer 4` 氛围层仍待后续接入
- 地形语义：当前已生成道路 / 水域 / 庭院语义，并服务于地图表现与地块检视；后续可继续扩展到更细的路网、水网与法阵连接规则
- 交互提示：支持全格点击检视；暂不提供弟子/场所可视对象联动
- 地图仅用于可视化反馈，不改写经济/人口核心数值
- 地图素材生产：正式表现层资产的分层、命名、锚点、拼接与交付规范统一参照 `docs/11_map_asset_production_spec.md`
- 运行时一期（Layer 1 / Layer 2）：宗门图现已允许优先读取 `Layer 1` atlas manifest，并在 `Layer 2` 叠加道路 / 院坪 / 水域 decal 与连接线；terrain 语义仍由 `TownMapGeneratorSystem` 提供最小可用输入，不改变小时结算与左栏检视链路

### 12.0 天衍峰院域坊局系统（设计中 / 一期）

- 当前地图主线：本章为天衍峰山门图后续迭代的唯一主线规格；其余同主题 map 分支文档与旧命名方案均视为废弃。
- 目标：将天衍峰山门图的单个 hex 从“纯背景格”升级为“可检视、可规划、可承载坊局组合”的固定院域底盘。
- 设计原则：
  - 外层 `hex 地块` 固定，内层 `院域坊局` 可变；
  - 同一地块可容纳多个子建筑，但共享该地块的固定灵气池；
  - 多建筑组合可以形成协同收益，但也会带来灵气分流、环境互扰和维护压力；
  - 随机性用于“改题”，不用于“掀桌”，不得无预警摧毁长期规划。
- 一期数据分层：
  - `SectHexCellData`：存地块固定底盘，至少包含 `coord / region / terrain / baseQiCapacity / qiRecovery / buildSlotCount / featureIds / roadMask / waterMask / variationSeed`
  - `SectHexCompoundState`：存地块内坊局组合，至少包含 `coord / buildings / synergyScore / qiCongestion / stability / activeModifierIds`
  - `SectSubBuildingState`：存地块内单建筑槽位，至少包含 `templateId / slotIndex / level / qiDemand / laborDemand / maintenanceDemand / currentQiShare / efficiency / synergyTagIds`
  - `SectHexInspectorData`：作为左侧 `Tile Inspector` 投影视图，统一输出 `title / subtitle / description / stats / actions / tags`
- 地块固定底盘：
  - 每个地块必须至少拥有 `灵气总量`、`灵气回复/波动`、`分区`、`天然特征`、`可建坊位数`
  - 地块底盘在同一局内固定，作为长期规划依据
  - 天然特征示例：`灵泉 / 古树 / 风口 / 石台 / 巡山角 / 晨雾坡`
- 坊局组合规则：
  - 同一地块内允许放入 `2~5` 个子建筑，具体上限由地块与后续扩建/科技决定
  - 子建筑组合按 `生产 / 服务 / 居住 / 休憩 / 特殊` 五类组织
  - 强组合优先设计为“核心建筑 + 支撑建筑 + 稳定建筑”，禁止鼓励纯同类堆叠成为唯一最优解
- 灵气共享规则：
  - 地块存在固定 `baseQiCapacity`
  - 各子建筑拥有理想 `qiDemand`
  - 当 `总需求 <= 地块灵气池` 时，各建筑按理想值运行
  - 当 `总需求 > 地块灵气池` 时，进入 `灵池分流` 状态，导致单体效率下降并累积 `qiCongestion`
  - 局部效率至少受 `地块适配 / 坊局协同 / 灵气满足率 / 人手满足率 / 稳定度` 五项共同影响
- 随机性来源（一期只定义来源，不硬写最终数值）：
  - 地块先天差异：五行偏性、天然特征、隐藏地脉 traits
  - 节气/季节波动：使“当前最优地块布局”随时间轻微偏移
  - 低频局部事件：灵泉喷涌、地火躁动、药田生异、夜雾遮山等
  - 驻守弟子差异：同样坊局在不同弟子驻守下表现不同
  - 宗门当前缺口：资源、治安、人口、研修压力会改变局部最优解
- 全格点击检视目标：
  - 任意 hex 都应可被点中，而非只限少数场所锚点
  - 点击后至少回答四件事：`这里是什么 / 它现在在干什么 / 为什么顺或为什么卡 / 我现在能做什么`
  - 左侧 `Tile Inspector` 一期统一展示 `状态 / 人数或坊位 / 效率或灵气 / 位置或邻接` 四格属性
  - 动作区一期保持 `1 个主动作 + 2 个辅助动作`，避免一次性塞满局部操作
- 一期典型模板（设计基线）：
  - `空地`：规划建造、查看推荐用途、标记预留
  - `坊路/山道`：整修道路、设巡查点、查看流量
  - `阵材圃/灵田`：扩建、调优先级、查仓储
  - `工坊`：扩建、切产线、查工料
  - `居舍`：修缮、改善供养、查弟子
  - `巡山岗`：增派巡山、整修岗哨、查看威胁
- 设计约束：
  - 一期先做“全格可检视 + 数据骨架”，不直接承诺完整地块内拖拽编辑器
  - 不得把全局资源库存、长段文案或完整岗位分配表直接塞进单格存档
  - 任何后续布局重排或 UI 改版，都不得破坏 `地图点击 -> 左栏检视 -> 推荐动作` 主链路

#### 12.0.1 字段职责表（一期）

- `SectHexCellData`
  - 作用：描述地块的固定底盘，是长期规划依据。
  - 建议字段：
    - `Coord`：逻辑坐标
    - `RegionId`：所属分区，例如 `前山 / 后山 / 坊市外缘 / 居舍区`
    - `Terrain`：基础地貌
    - `BaseQiCapacity`：地块灵气池上限
    - `QiRecoveryPerHour`：灵气恢复或自然波动
    - `QiAffinity`：五行偏性
    - `BuildSlotCount`：坊位数
    - `RoadMask / WaterMask`：边语义
    - `FeatureIds`：显性天然特征
    - `HiddenTraitIds`：隐性 traits，仅在满足条件后显露
    - `VariationSeed`：确保同地块随机事件可复现
- `SectHexCompoundState`
  - 作用：描述该地块当前的坊局结构和整体状态。
  - 建议字段：
    - `Coord`
    - `Buildings`
    - `QiCongestion`
    - `SynergyScore`
    - `Stability`
    - `ActiveModifierIds`
    - `ActiveEventId`
- `SectSubBuildingState`
  - 作用：描述坊局内单个建筑槽位。
  - 建议字段：
    - `BuildingId`
    - `TemplateId`
    - `SlotIndex`
    - `Level`
    - `QiDemand`
    - `LaborDemand`
    - `MaintenanceDemand`
    - `CurrentQiShare`
    - `Efficiency`
    - `SynergyTagIds`
- `SectHexInspectorData`
  - 作用：专供左侧 `Tile Inspector` 使用，不进入长期存档。
  - 建议字段：
    - `Title`
    - `Subtitle`
    - `Description`
    - `Stats`
    - `Actions`
    - `Tags`
- 存档约束：
  - `CellData` 与 `CompoundState` 进入正式状态层；
  - `InspectorData` 运行时生成；
  - 长段描述文案、按钮文字和推荐说明不直接写入存档。

#### 12.0.2 核心公式（设计草案）

- 地块可用灵气：
  - `QiAvailable = BaseQiCapacity + TerrainBonus + FeatureBonus + SeasonBonus + EventBonus`
- 单建筑灵气满足率：
  - `QiSatisfaction = clamp(CurrentQiShare / QiDemand, 0.15, 1.20)`
- 单建筑综合效率：
  - `BuildingEfficiency = BaseEfficiency * TerrainFit * SynergyBonus * QiSatisfaction * LaborSatisfaction * StabilityModifier`
- 地块灵气拥堵：
  - 当 `TotalQiDemand <= QiAvailable` 时，`QiCongestion = 0`
  - 当 `TotalQiDemand > QiAvailable` 时，`QiCongestion = (TotalQiDemand - QiAvailable) / max(QiAvailable, 1)`
- 坊局稳定度：
  - `Stability = clamp(1 - QiCongestion - ConflictPenalty - ThreatPenalty + SupportBonus, 0.35, 1.25)`
- 坊局协同分：
  - `SynergyScore = Sum(组合加成) - Sum(互扰惩罚)`
- 设计解释：
  - 高协同不等于高稳定；
  - 高灵气不等于高效率；
  - 玩家需要在“专精吃满灵气”和“多建筑拿协同”之间做取舍。

#### 12.0.3 坊局模板卡（一期基线）

- `空地`
  - 定位：规划入口
  - 默认显示：`可建性 / 邻接 / 灵气 / 坐标`
  - 主动作：`规划建造`
  - 辅助动作：`查看推荐用途`、`标记预留`
  - 飞轮影响：为后续产业、人口、治安提供布局空间
- `坊路/山道`
  - 定位：基础设施
  - 默认显示：`通达 / 流量 / 治安 / 坐标`
  - 主动作：`整修道路`
  - 辅助动作：`设巡查点`、`查看流量`
  - 飞轮影响：通勤、巡山、物流效率
- `阵材圃/灵田`
  - 定位：基础生产
  - 默认显示：`状态 / 在场人数 / 产能效率 / 相邻仓储或临水加成`
  - 主动作：`扩建灵田`
  - 辅助动作：`调整优先级`、`打开仓储`
  - 飞轮影响：供养、基础材料、稳定产出
- `药圃`
  - 定位：灵植与丹材来源
  - 默认显示：`药势 / 湿润度 / 灵气 / 照料人数`
  - 主动作：`整治药圃`
  - 辅助动作：`改种药材`、`调配弟子`
  - 飞轮影响：丹药、恢复、研修支持
- `工坊`
  - 定位：加工与营造核心
  - 默认显示：`订单 / 工料 / 效率 / 拥堵`
  - 主动作：`扩建工坊`
  - 辅助动作：`切换产线`、`查看工料仓`
  - 飞轮影响：装备、构件、护山建设
- `仓阁/内库`
  - 定位：中转与吞吐
  - 默认显示：`容量 / 装载率 / 吞吐 / 关联地块`
  - 主动作：`扩容仓储`
  - 辅助动作：`调整收储优先级`、`查看相邻产线`
  - 飞轮影响：缓冲生产链和交通压力
- `传法院`
  - 定位：研修与讲法
  - 默认显示：`研修效率 / 在场弟子 / 灵气 / 静谧度`
  - 主动作：`扩建讲法位`
  - 辅助动作：`下达研修法旨`、`查阅弟子谱`
  - 飞轮影响：科技涌现、弟子成长
- `居舍`
  - 定位：居住与恢复
  - 默认显示：`舒适 / 入住人数 / 恢复效率 / 通勤`
  - 主动作：`修缮居舍`
  - 辅助动作：`改善供养`、`查看居住弟子`
  - 飞轮影响：人口恢复、情绪、健康
- `巡山岗`
  - 定位：安全与夜巡
  - 默认显示：`警戒范围 / 安全度 / 巡查频率 / 邻近风险`
  - 主动作：`增派巡山`
  - 辅助动作：`整修岗哨`、`查看威胁来源`
  - 飞轮影响：护山、事故率、夜间物流

#### 12.0.4 随机事件框架（一期原则）

- 事件设计原则：
  - 事件应改变解题环境，而不是直接抹除玩家布局价值；
  - 事件应有预警、持续时间和可应对动作；
  - 高频事件只做轻扰动，强事件必须低频。
- 事件来源：
  - 地块先天 traits：例如 `古泉暗涌 / 地火脉浅 / 常年晨雾`
  - 节气波动：例如春木旺、秋金肃、冬水寒
  - 坊局冲突：例如火工坊过多导致躁火，药圃过密导致湿郁
  - 驻守弟子差异：例如擅长炼丹者会提高丹房稳定度
  - 宗门全局缺口：例如粮荒、治安压力会放大某类地块风险
- 事件强度分级：
  - `轻事件`：短时增减 `5%~10%` 某类效率或风险
  - `中事件`：要求玩家在数小时内处理，否则进入惩罚态
  - `强事件`：低频，触发特殊选择或一次性转型机会
- 典型事件：
  - `灵泉喷涌`：短时提高水木系建筑效率，但增加周边拥堵
  - `地火躁动`：火系工坊产能上升，但稳定度下降
  - `夜雾封山`：道路流量降低，巡山岗重要性上升
  - `药田生异`：可选择保收成或赌高品质灵药

#### 12.0.5 分阶段落地（建议）

- 一期：全格点击检视 + 院域数据骨架
  - 目标：让每个地块都能被点中并输出统一中文检视内容
  - 不改存档，不做完整坊局编辑器
- 二期：基础坊局组合
  - 目标：开放 `2~3` 个子建筑槽位，接入共享灵气与协同/互扰
  - 先做 `灵田 / 工坊 / 居舍 / 巡山岗 / 仓阁`
- 三期：局部随机事件与弟子驻守联动
  - 目标：把节气、traits、驻守特长真正挂到地块玩法上
  - 再决定是否进入更重的地块内布局编辑

#### 12.0.6 建筑模板表（一期建议）

| 模板 ID | 名称 | 类别 | 默认坊位占用 | 默认灵气需求 | 默认人手需求 | 主要协同标签 | 主要互扰标签 | 推荐地块 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `empty_buildable` | 空地 | `empty` | `0` | `0` | `0` | `reserve` | `none` | 任意可建地 |
| `mountain_road` | 坊路/山道 | `infrastructure` | `1` | `4` | `1` | `traffic`, `patrol` | `erosion` | 临路、山口 |
| `spirit_field_t1` | 阵材圃/灵田 | `production` | `1` | `18` | `4` | `wood`, `food`, `water_friendly` | `wet_dense` | 临水、灵壤 |
| `herb_garden_t1` | 药圃 | `production` | `1` | `22` | `5` | `herb`, `water_friendly`, `alchemy_feed` | `wet_dense` | 临水、晨雾坡 |
| `workshop_puppet_t1` | 傀儡工坊 | `production` | `1` | `28` | `6` | `craft`, `forge`, `warehouse_link` | `noise`, `fire_restless` | 临路、近仓 |
| `warehouse_inner_t1` | 仓阁/内库 | `service` | `1` | `10` | `3` | `storage`, `traffic`, `loss_reduce` | `crowded` | 交通节点 |
| `academy_outer_hall_t1` | 传法院 | `service` | `1` | `26` | `5` | `research`, `teaching`, `quiet` | `noise` | 高灵气、清静地 |
| `disciple_residence_t1` | 居舍 | `residence` | `1` | `14` | `2` | `rest`, `recovery`, `stability` | `crowded`, `noise` | 近路、近供养 |
| `watch_post_t1` | 巡山岗 | `special` | `1` | `12` | `3` | `patrol`, `safety`, `threat_control` | `isolated` | 山口、边缘地 |

- 模板设计约束：
  - 一期单建筑默认占 `1` 个坊位，不引入多格建筑 footprint。
  - `warehouse_inner_t1`、`watch_post_t1` 允许低产值但高系统性收益，避免所有模板都以直接产出为目标。
  - `academy_outer_hall_t1` 与 `disciple_residence_t1` 视为“慢变量模板”，主要影响研修、恢复与长期成长，不以即时数值爽感为唯一目标。

#### 12.0.7 字段默认值建议（一期）

- 地块默认值：
  - `BaseQiCapacity`：普通地 `80~110`，灵壤地 `110~140`，特殊地 `140~180`
  - `QiRecoveryPerHour`：普通地 `4~8`，特殊地 `8~14`
  - `BuildSlotCount`：普通地 `2`，核心地 `3`，特殊地 `4`
  - `VariationSeed`：按地图种子 + 坐标生成，不手写
- 坊局默认值：
  - `QiCongestion`：初始 `0`
  - `SynergyScore`：初始 `0`
  - `Stability`：初始 `1.0`
- 建筑默认值：
  - `Level`：初始 `1`
  - `CurrentQiShare`：默认按地块剩余灵气均分
  - `Efficiency`：初始 `1.0`，再由地形、灵气、人手、稳定度修正
- 检视器默认值：
  - `Stats` 固定四格：`状态 / 人数或坊位 / 效率或灵气 / 位置或邻接`
  - `Actions` 固定三键：`主动作 + 辅助动作 + 信息跳转`
  - `Tags` 优先显示：`临路 / 临水 / 灵脉 / 拥堵 / 安定 / 预留`

#### 12.0.8 配置化草案（建议路径）

- 建议草案路径：`CountyIdle/data/sect_hex_compound_templates.draft.json`
- 设计意图：
  - 当前文件仅作为文档阶段的数据草案，不接入现有加载链路；
  - 等一期代码实现开始时，再决定是否转正为正式配置文件；
  - 命名采用 `*.draft.json`，避免被误认为当前生产配置。
- 建议顶层结构：
  - `version`
  - `slot_rules`
  - `cell_defaults`
  - `terrain_presets`
  - `feature_pool`
  - `building_templates`
  - `event_templates`
- 约束：
  - 所有模板必须有稳定 `id`
  - 可变说明文字优先在代码或文案层生成，不把长段描述硬编码进配置
  - 事件模板只描述触发条件、持续时间、影响标签和应对动作键，不直接写死 UI 文案

## 12.1 战略地图配置系统（StrategicMap）

- 配置路径：`CountyIdle/data/strategic_maps.json`
- 配置目标：世界图/江陵府外域图的区域、边界、路线、河流、节点由数据定义
- 底格语义：`grid_lines` 表示战略底格密度；当前视图层按 hex 战略底格渲染，而非矩形辅助线
- 渲染约束：归一化坐标区间建议 `[-1.0, 1.0]`，超界会导致裁切
- 缩放约束：地图缩放统一夹紧为 `60% ~ 220%`，默认 `100%`
- 容错策略：配置文件缺失或反序列化失败时回退内置默认配置，不中断主循环
- 启动校验：加载时检查点数下限、坐标建议区间（`±1.20`）与颜色格式（`#RRGGBB/#RRGGBBAA`），异常写 `GD.PushWarning`

## 12.2 江陵府外域备用视图程序化生成（Village 风格）

- 适用范围：`Prefecture` 隐藏备用视图启用程序化生成，对外标题统一为 `江陵府外域`；`World` 对外标题统一为 `世界地图`。
- 生成输入：人口、住房、威胁、小时结算数（均取非负）。
- 分桶策略（避免频繁重建）：
  - 人口桶：`population / 24`
  - 住房桶：`housing / 30`
  - 威胁桶：`floor(threat / 8)`
  - 结算桶：`hourSettlements / 6`
- 生成骨架：
- 有机边界：`18~24` 点极坐标扰动，默认尺度扩大（外域州府式大图）
  - 附庸据点节点：`clamp(16 + pop/60 + housing/120, 16, 34)` 个，环绕外域主附庸据点节点分布
- 自然/人工区域：森林、湖泊、农田、山脉、外域内城区块
  - 道路：主附庸据点放射灵道 + 附庸据点环路 + 边界连接路 + 城内网格坊巷 + 云桥主轴
  - 河流：主河（`7` 点）+ 支流（`4` 点）组合
  - 城内建筑：地标（山门楼/云桥/问道台/渡口/灵仓等）+ `7~11 x 6~10` 的坊市网格高密建筑节点
- 标签系统：战略地图支持 `labels`（位置、文本、颜色、字号、`min_zoom`），用于地貌与建筑说明展示，并按缩放分级显隐。
- 江陵府外域缩放增强：`Prefecture` 模式最大缩放提升到 `560%`，并采用平滑惯性缩放。
- 高倍率街区渲染：当江陵府外域备用视图缩放进入高倍率时，叠加整座外城级别的长街、横街、里巷纹理；坊市渲染为沿街连续街屋、市肆摊列与巷内庭院块面，地标/外城渲染为院落式建筑群。
- 外域命名主题配置：`CountyIdle/data/prefecture_city_theme.json`
  - 可配置项：地图标题、城名、地貌名称、主街名称、城门名称（东/西/南/北）、地标名称列表、坊市名称池
  - 容错策略：缺失/空字段自动回退默认命名，不中断生成
- 设计约束：仅影响地图视觉反馈，不改经济/人口/战斗核心数值与结算顺序。

## 13. 双地图视图与缩放（Sect / World）

- 天衍峰山门图与世界地图均支持滚轮缩放（区间 `0.6 ~ 2.2`，默认 `1.0`）。
- 主界面缩放按钮仅对当前激活地图页生效（宗门/世界）。
- 页签切换时刷新缩放显示，不影响主循环与结算状态。
- Legacy 主界面的地图页签 / 调度按钮需与左侧地块检视器、底部快捷入口建立显式焦点桥接；中央地图区被点击后，方向焦点仍可退出到外层菜单。
- 世界图底层统一绘制 hex 战略底格；宗门图沿用独立 hex 驻地绘制链路。
- 默认节点标记采用六角标记；世界图 edge overlay 用于河流、道路、桥梁、河岸与悬崖边表现。
- 当前实现：世界地图 = 修仙 Hex 世界生成（`strategic_maps.json` 世界定义保留 fallback）；天衍峰山门图 = 复用当前 hex 驻地视图链路并统一天衍峰文案。
- 全局表现语义：除天衍峰山门图本体外，仓储、岗位、研究突破、资源日志、事件提示、地图经营按钮与世界图标题统一使用 `阵材圃 / 傀儡工坊 / 传法院 / 青云总坊 / 庶务殿 / 浮云宗 / 世界地图` 术语；隐藏备用 Prefecture 视图标题改为 `江陵府外域`，避免旧郡县文案漏出。
- 江陵府外域备用视图运营语义：隐藏备用 `Prefecture` 视图的状态与调度统一使用 `灵道 / 附庸据点 / 抚恤附庸 / 峰外采办` 术语；即使 fallback 到该视图，也不再出现旧外域行政式提示词。

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
  3. 根据地形与灵气区域分配资源、奇观、宗门候选、附庸据点/坊市/遗迹；
  4. 输出 `XianxiaWorldMapData` 与 `StrategicMapDefinition` 两份结果，后者直接喂给 `StrategicMapViewSystem`。
- 数据结构约束：
  - `XianxiaHexCellData` 为单格最小单位，持有 `coord / height / climate / biome / terrain / water / cliff / overlay / resource / spiritual_zone / structure / wonder / river_mask / cliff_mask / road_mask / render`
  - `DragonVeinPathData / RiverPathData / SectCandidateSiteData / WonderSiteData / XianxiaSiteData` 为世界级聚合对象
  - `render` 内维护 tile key、变体索引与皮肤 key，便于未来切换为真正的资源图块
- 世界图适配：
  - 世界图默认优先使用修仙生成器；若生成异常，回退 `StrategicMapConfigSystem.GetWorldDefinition()`
  - 适配层会把 hex cell 转成 `StrategicPolygonDefinition`；龙脉继续转成 `StrategicPolylineDefinition`；宗门候选/附庸据点/奇观/稀有资源转成 `StrategicNodeDefinition` 与 `StrategicLabelDefinition`
  - 世界图模式下会直接读取 `RiverMask / RoadMask` 沿 hex 共边绘制 overlay，因此河流和道路优先表达为边语义，而非单格中心纹理
  - edge overlay 二期细节：河岸按 `Water` 与相邻非水格边界绘制 shoreline；当 `RiverMask` 与 `RoadMask` 重叠时绘制桥梁；当 `RoadMask` 连接数 `>= 3` 时绘制路口节点；`CliffMask` 额外绘制悬崖暗边
  - 默认配置（`seed = 20260308`）一次生成结果已验证为：`2560` 地块、`9` 条河流、`6` 条灵脉、`9` 处奇观、`12` 个宗门候选、`22` 个场所点位
  - 二期补强后，默认配置额外验证为：`242` 个河流边语义格、`111` 个道路边语义格、`38` 个悬崖边语义格、`9` 个水域格、`8` 个桥梁边重叠格、`10` 个路口格
- 设计约束：只增强世界图信息表达，不改 `GameState`、小时结算、人口/产业/战斗核心公式。

## 13.2 世界格二级地图分层与入口系统（DL-048 / 设计中 / 文档一期）

- 目标：让世界地图上的可交互 hex 不只是“地貌 + 点位”，而是具有明确身份、风险和回流方向的空间节点；玩家点击后可进入对应的二级地图或二级地图检视界面，形成“世界择地 -> 局部经营 / 交涉 / 历练 -> 回流宗门”的分层体验。
- 设计约束：
  - 第一级地图 = 世界战略层，负责回答“去哪里”；
  - 第二级地图 = 地点玩法层，负责回答“到了这里主要做什么”；
  - 二级地图必须服务现有飞轮，不做纯观光景观图；
  - 进入与退出规则不得破坏 `1 秒 = 1 分钟` 与 `60 分钟小时结算` 节奏；
  - 不把凡俗势力、附庸据点或关系对象写成可随意牺牲的“资源块”，需符合“共同建设、共同受益、共同护持”底线。
- 数据建议：世界格至少应具备以下元数据字段：
  - `PrimaryType`：地点主类型；
  - `SecondaryTag`：地点子标签；
  - `FactionOwner`：归属势力；
  - `RiskTier`：风险层级；
  - `ResourceBias`：资源偏向；
  - `InteractionFocus`：主要交互；
  - `ReturnFlow`：回流宗门的主要收益；
  - `UnlockCondition`：解锁门槛；
  - `TimeCostProfile`：轻交互 / 重交互时间消耗模型。

### 13.2.1 PrimaryType（七类主类型）

- `Sect / 宗门`
  - 定位：修仙势力关系点。
  - 主要玩法：访问、结盟、论道、交换传承、驻点互动。
  - 核心产出：功法、人脉、弟子来源、盟友支持。
  - 核心风险：关系恶化、资源依赖、宗门冲突。
- `Wilderness / 野外`
  - 定位：日常历练与资源点。
  - 主要玩法：采集、巡查、护道、清妖、开路。
  - 核心产出：原材料、低中阶机缘、路线安全。
  - 核心风险：妖兽、迷途、耗时、轻中度战损。
- `MortalRealm / 凡俗国度`
  - 定位：人口与供养点。
  - 主要玩法：护持、赈济、安民、征调供给、招收苗子。
  - 核心产出：人口、粮草、供奉、稳定度、苗子。
  - 核心风险：民乱、灾荒、失德、供给断裂。
- `CultivatorClan / 修仙世家`
  - 定位：血脉与人脉点。
  - 主要玩法：走关系、谈合作、接家族委托、交换秘术。
  - 核心产出：血脉资源、人脉、客卿机会、专属材料。
  - 核心风险：结怨、失信、派系站队。
- `ImmortalCity / 仙城`
  - 定位：长期枢纽点。
  - 主要玩法：大宗交易、拍卖、驻点经营、接跨域任务。
  - 核心产出：高级交易渠道、情报、委托、跨势力接触。
  - 核心风险：竞争、税费、治安波动、声望门槛。
- `Market / 坊市`
  - 定位：短期机会点。
  - 主要玩法：短频快交易、打听消息、淘货、黑白市机会。
  - 核心产出：稀缺货、流通资源、传闻、临时任务。
  - 核心风险：被坑、价格波动、假货、黑市风险。
- `Ruin / 遗迹`
  - 定位：高风险高回报点。
  - 主要玩法：探索、破阵、夺宝、触发传承事件。
  - 核心产出：稀有掉落、古传承、法器胚、高价值线索。
  - 核心风险：高战损、封印、机关、一次性失败成本。

### 13.2.2 SecondaryTag（世界生成子标签）

- `Sect / 宗门`
  - `MountainGate`：山门本宗。
  - `BranchPeak`：分峰别院。
  - `OuterCourtyard`：外门院。
  - `AllianceHall`：盟宗驻馆。
  - `ForbiddenGround`：禁地。
- `Wilderness / 野外`
  - `SpiritForest`：灵林。
  - `OreRange`：矿岭。
  - `MarshWetland`：沼泽。
  - `CanyonPass`：峡道。
  - `SpiritVeinField`：灵脉荒野。
- `MortalRealm / 凡俗国度`
  - `CountySeat`：府县治所。
  - `FarmVillage`：农庄乡里。
  - `RiverTown`：水镇。
  - `BorderFort`：边寨关堡。
  - `DisasterLand`：灾区。
- `CultivatorClan / 修仙世家`
  - `AncestralEstate`：祖庭本家。
  - `GuestHall`：客卿别馆。
  - `SpiritFieldManor`：灵田庄园。
  - `ForgeLineage`：铸器世家。
  - `MedicineLineage`：丹药世家。
- `ImmortalCity / 仙城`
  - `GrandCity`：大城。
  - `TransitHub`：驿城。
  - `HarborCity`：河港仙城。
  - `FrontierCity`：边陲仙城。
  - `ImperialCultCity`：王朝修士都城。
- `Market / 坊市`
  - `SectMarket`：宗门坊。
  - `LooseCultivatorBazaar`：散修集。
  - `BlackMarket`：黑市。
  - `FestivalFair`：节令市。
  - `RoadsideMarket`：路市。
- `Ruin / 遗迹`
  - `AncientCave`：古修洞府。
  - `FallenPalace`：坠落仙府。
  - `SealedDungeon`：封印地宫。
  - `BattlefieldRemnant`：古战场遗址。
  - `TrialRealm`：试炼秘境。

### 13.2.3 类型边界与防重叠规则

- `ImmortalCity / 仙城` 与 `Market / 坊市` 的边界：
  - `仙城` 是长期枢纽，负责大宗事务、驻点、拍卖、跨势力任务；
  - `坊市` 是短期热点，负责捡漏、传闻、短交易、黑市与临时委托。
- `MortalRealm / 凡俗国度` 与 `CultivatorClan / 修仙世家` 的边界：
  - `凡俗国度` 的核心资产是人口、土地、秩序、供养；
  - `修仙世家` 的核心资产是血脉、秘术、人情、门客。
- `Wilderness / 野外` 与 `Ruin / 遗迹` 的边界：
  - `野外` 是可持续进入的日常历练与采集层；
  - `遗迹` 是高门槛、高风险、低频高收益的副本层。
- `Sect / 宗门` 不得退化为普通贸易城；`CultivatorClan / 修仙世家` 不得实现为缩小版宗门；`MortalRealm / 凡俗国度` 不得实现为普通城建图换皮。

### 13.2.4 二级地图模板建议

- 为避免七类地点在实现上变成七套完全独立框架，二级地图建议先抽象为四种模板：
  - `据点经营型`：适用于 `Sect / 宗门`、部分 `ImmortalCity / 仙城`；
  - `势力关系型`：适用于 `MortalRealm / 凡俗国度`、`CultivatorClan / 修仙世家`；
  - `野外历练型`：适用于 `Wilderness / 野外`；
  - `高风险副本型`：适用于 `Ruin / 遗迹`，以及少数高门槛 `ForbiddenGround / 禁地`。
- `Market / 坊市` 在一期可先复用 `势力关系型 + 轻量交易层` 组合，不单独建完整框架。

### 13.2.5 时间推进与入口/退出规则

- 世界格点击进入二级地图时，不暂停主世界时间。
- 二级地图内的交互分为两类：
  - `轻交互`：交易、接任务、关系查看、驻守布置；仅消耗极少固定分钟数，或不额外推进主时钟。
  - `重交互`：深入野外、探索遗迹、长期驻点、护道；按明确分钟数或小时数推进，并通过统一回流接口写回 `GameState`。
- 二级地图退出时必须明确写回：
  - 获得或消耗的资源；
  - 关系、稳定度或威胁变化；
  - 英雄/队伍状态变化；
  - 传闻、委托、地块解锁等衍生结果。

### 13.2.6 世界生成分布建议（文档一期）

- 高频常见点：
  - `Wilderness + SpiritForest / OreRange / CanyonPass`
  - `Market + LooseCultivatorBazaar / SectMarket`
  - `MortalRealm + FarmVillage / CountySeat`
  - `Sect + BranchPeak`
  - `Ruin + AncientCave / BattlefieldRemnant`
- 稀有或中后期点：
  - `ImmortalCity + ImperialCultCity`
  - `CultivatorClan + AncestralEstate`
  - `Ruin + FallenPalace / TrialRealm`
  - `Sect + ForbiddenGround`
- 贴地形建议：
  - `OreRange` 优先贴山脉；
  - `SpiritForest` 优先贴林地与灵脉边；
  - `BorderFort` 优先贴边界与峡道；
  - `HarborCity`、`RiverTown` 优先贴主河道；
  - `Sect`、`ForbiddenGround` 优先贴灵脉；
  - `BlackMarket` 不单独刷在大道中央，应依附城市、坊市或边地节点；
  - `FallenPalace`、`TrialRealm` 为低频稀有点，不应大面积分布。

### 13.2.6.1 世界生成规则草案（文档二期）

- 目标：把“世界观上合理的地点分布”收口为一套可执行的生成流程，供后续 `XianxiaWorldGeneratorSystem`、配置文件和调试面板使用。
- 总原则：
  - 先生成骨架，再挂地点；
  - 先决定大区块，再决定点位类型；
  - 先决定常见层，再决定稀有层；
  - `PrimaryType` 之间不平均分布，修仙世界应体现“野外多、势力点少、遗迹稀有”的密度层级；
  - 高级点位随游戏进度逐步开放，不要求开局全部可进。

### 13.2.6.2 生成流程（推荐顺序）

1. 生成地貌骨架：
   - 山脉、平原、林地、湖泽、峡道、荒原、河谷。
2. 生成灵气骨架：
   - 主灵脉、支灵脉、枯竭带、灵潮节点、禁忌区。
3. 生成人路骨架：
   - 古道、商路、渡口、关隘、驿点。
4. 生成势力区块：
   - 凡俗王朝疆域、宗门辐射圈、世家经营圈、散修活跃带。
5. 生成特殊区块：
   - 古战场、封印区、陨落洞天、秘境裂口。
6. 按区块和邻接规则分配 `PrimaryType`：
   - 先放 `Sect / ImmortalCity / CultivatorClan` 等势力中枢；
   - 再放 `MortalRealm / Market` 等人口与交易节点；
   - 最后填充 `Wilderness` 并点缀 `Ruin`。
7. 根据 `PrimaryType` 决定 `SecondaryTag`：
   - 例如 `Wilderness` 再分为 `SpiritForest / OreRange / MarshWetland / CanyonPass / SpiritVeinField`。
8. 最后做稀有度、最小间距与进度门槛校正：
   - 防止高阶点位扎堆或初期可达内容过强。

### 13.2.6.3 世界大区块建议

- 为便于程序化生成与后续平衡，世界图可先分成五种大区块，每个 hex 至少隶属一种主区块：
  - `灵脉山域`：山脉 + 高灵气，适合 `Sect / Wilderness / Ruin`；
  - `凡俗腹地`：平原 + 水网 + 农业，适合 `MortalRealm / Market / CultivatorClan`；
  - `商路走廊`：道路密集 + 渡口/关隘，适合 `ImmortalCity / Market / MortalRealm`；
  - `边疆险地`：边界 + 峡谷 + 高威胁，适合 `Wilderness / BorderFort / BlackMarket / Ruin`；
  - `古迹断脉区`：枯竭灵脉 + 古战场/封印，适合 `Ruin / ForbiddenGround / TrialRealm`。
- 大区块不要求整齐切片，可彼此嵌套或边缘过渡，但必须能解释“这里为什么会长出这种点”。

### 13.2.6.4 各 PrimaryType 的生成规则草案

- `Sect / 宗门`
  - 选点优先级：`高质量灵脉 > 山地/险地 > 可达性 > 周边供养圈`。
  - 邻接偏好：至少邻接 `1` 个 `Wilderness`；优先邻接 `Market` 或 `MortalRealm` 供给圈，但不应紧贴大型凡俗腹心。
  - 数量层级：低量。
  - 最小间距：与其他 `Sect` 保持较大距离，避免顶级宗门密集挤在同一片山头。
- `Wilderness / 野外`
  - 选点优先级：作为默认填充层，根据地貌和灵气自动细分为林地、矿岭、沼泽、峡道、灵脉荒野。
  - 邻接偏好：可邻接任何类型，但更常包围 `Sect / Ruin / BorderFort`。
  - 数量层级：最高。
  - 最小间距：不设强限制，核心是保证子标签分布合理而非数量稀缺。
- `MortalRealm / 凡俗国度`
  - 选点优先级：`宜居性 > 水路/农地 > 道路通达 > 风险可控`。
  - 邻接偏好：优先与 `Market`、低风险 `Wilderness`、`CultivatorClan` 相邻；尽量远离高危 `Ruin` 核心区。
  - 数量层级：次高。
  - 最小间距：以片区形式成簇出现，单点间距可较近，但高阶治所之间保持中等距离。
- `CultivatorClan / 修仙世家`
  - 选点优先级：`专精资源 > 交通可达 > 富庶边缘 > 安全度`。
  - 邻接偏好：常位于 `MortalRealm`、`Market` 与 `Sect / ImmortalCity` 之间，扮演中介型势力。
  - 数量层级：中低。
  - 最小间距：不必像宗门那样稀疏，但应避免多个世家完全重叠在同一小区域。
- `ImmortalCity / 仙城`
  - 选点优先级：`多路交汇 > 大河/渡口 > 势力交界 > 中高灵气`。
  - 邻接偏好：优先与 `Market`、`MortalRealm`、`CultivatorClan` 相邻；可辐射多个道路节点。
  - 数量层级：很低。
  - 最小间距：全图大枢纽数量严格受限，避免“遍地仙城”。
- `Market / 坊市`
  - 选点优先级：伴生生成优先于独立生成。
  - 伴生规则：
    - `Sect` 周边可伴生 `SectMarket`；
    - `ImmortalCity` 周边固定伴生 `1~2` 个交易节点；
    - 渡口、驿路、边关可伴生 `RoadsideMarket`；
    - 边疆险地或遗迹入口附近可低概率伴生 `BlackMarket`。
  - 数量层级：中量。
  - 最小间距：允许较近，但同类高价值坊市不应连续刷在同一条路上。
- `Ruin / 遗迹`
  - 选点优先级：`险地 > 古迹区 > 断脉区 > 远离腹地 > 神秘性`。
  - 邻接偏好：优先邻接 `Wilderness`；少量可与 `Sect.ForbiddenGround` 或 `BorderFort` 形成特殊组合。
  - 数量层级：最低。
  - 最小间距：中高阶遗迹之间应保持大距离，超稀有点全图严格限量。

### 13.2.6.5 邻接加权规则草案

- 正向邻接：
  - `Sect <-> Wilderness`
  - `MortalRealm <-> Market`
  - `ImmortalCity <-> Market`
  - `ImmortalCity <-> MortalRealm`
  - `CultivatorClan <-> MortalRealm`
  - `CultivatorClan <-> Market`
  - `Ruin <-> Wilderness`
  - `BorderFort <-> CanyonPass`
- 条件邻接：
  - `BlackMarket` 更偏向 `FrontierCity / BorderFort / Ruin` 周边；
  - `SectMarket` 更偏向 `Sect` 山门外缘；
  - `HarborCity` 与 `RiverTown` 优先贴主河道。
- 负向邻接：
  - 高阶 `Ruin` 不应直接贴 `CountySeat`；
  - 顶级 `Sect` 不应直接压在大型 `ImmortalCity` 中心；
  - `ImperialCultCity` 不应与多个同级仙城连续紧邻。

### 13.2.6.6 稀有度与数量层级草案

- 全图数量级建议按以下层级控制：
  - `Wilderness`：默认填充层，约占可交互点位主体。
  - `MortalRealm`：第二层骨架，形成多个片区。
  - `Market`：中量，主要通过伴生和交通节点生长。
  - `CultivatorClan`：中低量，起连接作用。
  - `Sect`：低量，代表区域修仙中枢。
  - `ImmortalCity`：很低量，代表超级枢纽。
  - `Ruin`：最低量，其中高阶遗迹比普通遗迹更稀少。
- 稀有度分层建议：
  - `常见`：`SpiritForest`、`FarmVillage`、`SectMarket`、`RoadsideMarket`
  - `少见`：`BranchPeak`、`GuestHall`、`HarborCity`、`BattlefieldRemnant`
  - `稀有`：`MountainGate`、`GrandCity`、`BlackMarket`、`SealedDungeon`
  - `传说`：`ForbiddenGround`、`ImperialCultCity`、`FallenPalace`、`TrialRealm`

### 13.2.6.7 进度解锁规则草案

- 开局可见可进：
  - `Wilderness` 常见点；
  - 近域 `MortalRealm`；
  - 普通 `Market`；
  - 少量低阶 `Sect` 访问点；
  - 小型 `Ruin`，如 `AncientCave`。
- 中期开启：
  - `CultivatorClan` 主要节点；
  - `ImmortalCity`；
  - 边疆 `BorderFort`；
  - 中阶 `Ruin`，如 `BattlefieldRemnant / SealedDungeon`。
- 后期开启：
  - `ForbiddenGround`；
  - `ImperialCultCity`；
  - `TrialRealm`；
  - `FallenPalace`。
- 解锁条件可挂接：
  - 宗门声望；
  - 历练层数；
  - 英雄/真传平均战力；
  - 盟约或关系网络；
  - 特定传闻或前置遗迹线索。

### 13.2.6.8 配置化建议

- 后续若写入配置文件，建议至少拆为三层：
  - `WorldRegionProfile`：大区块参数；
  - `WorldPrimaryTypeSpawnRule`：主类型权重、邻接偏好、最小间距；
  - `WorldSecondaryTagSpawnRule`：子标签权重、伴生条件、解锁门槛。
- 调试入口建议支持查看：
  - 当前 hex 的区块类型；
  - 候选 `PrimaryType` 权重；
  - 邻接加权结果；
  - 稀有度与解锁门槛；
  - 最终被刷成该点位的原因摘要。

### 13.2.6.9 参数草案（文档三期 / 接近数据表）

- 目的：为后续 JSON 配置与 C# 模型提供第一版字段草案与推荐参数量级；本节不是最终平衡值，而是“结构先行、数值后细调”的起点。

#### A. `WorldRegionProfile` 参数草案

- 建议字段：
  - `RegionId`
  - `DisplayName`
  - `CoverageWeight`
  - `TerrainAffinity`
  - `SpiritualDensityRange`
  - `RoadDensityRange`
  - `ThreatBaseline`
  - `PrimaryTypeBias`
  - `RuinBias`
  - `MarketBias`
  - `UnlockTier`
- 推荐区块参数：

| RegionId | 中文 | CoverageWeight | SpiritualDensity | RoadDensity | ThreatBaseline | 主类型倾向 |
| --- | --- | --- | --- | --- | --- | --- |
| `SpiritMountain` | 灵脉山域 | `0.18 ~ 0.24` | `0.75 ~ 1.00` | `0.20 ~ 0.40` | `0.45 ~ 0.65` | `Sect / Wilderness / Ruin` |
| `MortalHeartland` | 凡俗腹地 | `0.26 ~ 0.34` | `0.20 ~ 0.45` | `0.55 ~ 0.85` | `0.10 ~ 0.30` | `MortalRealm / Market / CultivatorClan` |
| `TradeCorridor` | 商路走廊 | `0.12 ~ 0.18` | `0.30 ~ 0.55` | `0.75 ~ 1.00` | `0.20 ~ 0.40` | `ImmortalCity / Market / MortalRealm` |
| `FrontierWilds` | 边疆险地 | `0.16 ~ 0.22` | `0.35 ~ 0.65` | `0.20 ~ 0.50` | `0.55 ~ 0.85` | `Wilderness / BorderFort / Ruin / BlackMarket` |
| `BrokenVeinRuins` | 古迹断脉区 | `0.08 ~ 0.14` | `0.10 ~ 0.35` | `0.05 ~ 0.20` | `0.70 ~ 1.00` | `Ruin / ForbiddenGround / TrialRealm` |

- 护栏：
  - `CoverageWeight` 归一化后总和为 `1.0`；
  - `BrokenVeinRuins` 与 `ImmortalCity` 的直接重叠概率应极低；
  - `MortalHeartland` 不得成为高阶遗迹的主要出生区。

#### B. `WorldPrimaryTypeSpawnRule` 参数草案

- 建议字段：
  - `PrimaryType`
  - `BaseWeight`
  - `RegionWeightMultiplier`
  - `TerrainWeightMultiplier`
  - `SpiritualWeightCurve`
  - `RoadWeightCurve`
  - `ThreatWeightCurve`
  - `MinHexDistance`
  - `SoftCapPerRegion`
  - `GlobalCap`
  - `CompanionSpawnRules`
  - `UnlockTier`
  - `VisibilityTier`
- 推荐权重与约束：

| PrimaryType | BaseWeight | MinHexDistance | SoftCapPerRegion | GlobalCap | UnlockTier | 备注 |
| --- | --- | --- | --- | --- | --- | --- |
| `Sect` | `0.42` | `10 ~ 14` | `1 ~ 3` | `6 ~ 10` | `0` | 低量，强依赖灵脉与山地 |
| `Wilderness` | `1.00` | `0` | `无` | `无` | `0` | 默认填充层 |
| `MortalRealm` | `0.82` | `3 ~ 6` | `6 ~ 12` | `无` | `0` | 以片区簇状生成 |
| `CultivatorClan` | `0.36` | `6 ~ 9` | `2 ~ 4` | `8 ~ 14` | `1` | 资源与交通双偏好 |
| `ImmortalCity` | `0.18` | `14 ~ 20` | `0 ~ 2` | `3 ~ 5` | `1` | 超级枢纽，严格限量 |
| `Market` | `0.64` | `2 ~ 4` | `4 ~ 8` | `无` | `0` | 伴生优先，高流动 |
| `Ruin` | `0.24` | `8 ~ 16` | `1 ~ 4` | `8 ~ 16` | `0` | 稀有，按阶层再拆 |

- 调参说明：
  - `BaseWeight` 只表达“默认倾向”，真正落点还要叠加区块、地形、邻接与门槛修正；
  - `Wilderness` 作为默认填充层，可不走严格 `GlobalCap`；
  - `ImmortalCity`、高阶 `Sect`、高阶 `Ruin` 必须同时受 `MinHexDistance` 与 `GlobalCap` 双重限制。

#### C. `WorldSecondaryTagSpawnRule` 参数草案

- 建议字段：
  - `PrimaryType`
  - `SecondaryTag`
  - `BaseWeight`
  - `RegionBias`
  - `TerrainBias`
  - `RequiresAdjacency`
  - `AvoidsAdjacency`
  - `UnlockTier`
  - `RarityTier`
  - `CanCompanionSpawn`
- 推荐子标签参数：

| PrimaryType | SecondaryTag | BaseWeight | UnlockTier | RarityTier | 关键条件 |
| --- | --- | --- | --- | --- | --- |
| `Sect` | `MountainGate` | `0.20` | `0` | `Rare` | 高灵脉 + 山地 |
| `Sect` | `BranchPeak` | `0.42` | `0` | `Uncommon` | 靠近主宗门辐射圈 |
| `Sect` | `OuterCourtyard` | `0.24` | `0` | `Common` | 可达性较高 |
| `Sect` | `AllianceHall` | `0.10` | `1` | `Rare` | 靠近仙城或盟宗 |
| `Sect` | `ForbiddenGround` | `0.04` | `2` | `Legendary` | 断脉/高威胁/禁区 |
| `Wilderness` | `SpiritForest` | `0.28` | `0` | `Common` | 林地 + 中高灵气 |
| `Wilderness` | `OreRange` | `0.24` | `0` | `Common` | 山地 + 矿脉 |
| `Wilderness` | `MarshWetland` | `0.16` | `0` | `Uncommon` | 沼泽/河湖 |
| `Wilderness` | `CanyonPass` | `0.18` | `0` | `Uncommon` | 峡谷/边关 |
| `Wilderness` | `SpiritVeinField` | `0.14` | `1` | `Rare` | 支灵脉无人区 |
| `MortalRealm` | `CountySeat` | `0.22` | `0` | `Uncommon` | 平原 + 路网 |
| `MortalRealm` | `FarmVillage` | `0.38` | `0` | `Common` | 平原 + 水网 |
| `MortalRealm` | `RiverTown` | `0.22` | `0` | `Uncommon` | 主河道 |
| `MortalRealm` | `BorderFort` | `0.12` | `1` | `Rare` | 边界 + 峡道 |
| `MortalRealm` | `DisasterLand` | `0.06` | `1` | `Rare` | 高威胁/低稳态 |
| `CultivatorClan` | `AncestralEstate` | `0.20` | `1` | `Rare` | 富庶区边缘 + 稳定 |
| `CultivatorClan` | `GuestHall` | `0.24` | `1` | `Uncommon` | 贴交通点 |
| `CultivatorClan` | `SpiritFieldManor` | `0.22` | `1` | `Uncommon` | 资源专精区 |
| `CultivatorClan` | `ForgeLineage` | `0.18` | `1` | `Rare` | 贴矿岭/工路 |
| `CultivatorClan` | `MedicineLineage` | `0.16` | `1` | `Rare` | 贴灵林/药泽 |
| `ImmortalCity` | `GrandCity` | `0.34` | `1` | `Rare` | 多路交汇 |
| `ImmortalCity` | `TransitHub` | `0.28` | `1` | `Uncommon` | 驿路枢纽 |
| `ImmortalCity` | `HarborCity` | `0.20` | `1` | `Rare` | 大河/渡口 |
| `ImmortalCity` | `FrontierCity` | `0.12` | `2` | `Rare` | 边疆险地 |
| `ImmortalCity` | `ImperialCultCity` | `0.06` | `2` | `Legendary` | 王朝核心 + 声望门槛 |
| `Market` | `SectMarket` | `0.24` | `0` | `Common` | 需邻接宗门 |
| `Market` | `LooseCultivatorBazaar` | `0.30` | `0` | `Common` | 散修活跃带 |
| `Market` | `BlackMarket` | `0.08` | `1` | `Rare` | 边地/遗迹/前线 |
| `Market` | `FestivalFair` | `0.10` | `1` | `Rare` | 节令/事件驱动 |
| `Market` | `RoadsideMarket` | `0.28` | `0` | `Common` | 需邻接道路/渡口 |
| `Ruin` | `AncientCave` | `0.34` | `0` | `Common` | 山野/隐蔽点 |
| `Ruin` | `FallenPalace` | `0.08` | `2` | `Legendary` | 断脉/稀有奇观 |
| `Ruin` | `SealedDungeon` | `0.20` | `1` | `Rare` | 封印区/高威胁 |
| `Ruin` | `BattlefieldRemnant` | `0.24` | `1` | `Uncommon` | 古战场区 |
| `Ruin` | `TrialRealm` | `0.14` | `2` | `Legendary` | 高灵潮/试炼门槛 |

#### D. 邻接权重参数草案

- 可在 `WorldAdjacencyWeightRule` 中配置：
  - `SourceType`
  - `TargetType`
  - `WeightDelta`
  - `Radius`
  - `RuleMode`：`Attract / Repel / Require`
- 推荐值：

| Source -> Target | WeightDelta | Radius | RuleMode | 说明 |
| --- | --- | --- | --- | --- |
| `Sect -> Wilderness` | `+0.30 ~ +0.45` | `2` | `Attract` | 宗门周边应有历练圈 |
| `MortalRealm -> Market` | `+0.25 ~ +0.40` | `2` | `Attract` | 人口与交易共生 |
| `ImmortalCity -> Market` | `+0.35 ~ +0.50` | `2` | `Attract` | 大城带动坊市 |
| `CultivatorClan -> Market` | `+0.18 ~ +0.28` | `2` | `Attract` | 世家偏好交易与客卿 |
| `Ruin -> Wilderness` | `+0.22 ~ +0.35` | `2` | `Attract` | 遗迹外侧应被野外包裹 |
| `BlackMarket -> FrontierCity` | `+0.28 ~ +0.42` | `2` | `Attract` | 黑市依附边地 |
| `HighRuin -> CountySeat` | `-0.40 ~ -0.60` | `3` | `Repel` | 高危遗迹远离腹地 |
| `ImmortalCity -> ImmortalCity` | `-0.55 ~ -0.80` | `5` | `Repel` | 大城避免扎堆 |
| `Sect -> Sect` | `-0.45 ~ -0.70` | `4` | `Repel` | 顶级宗门保持势力空间 |

#### E. 稀有度与可见性参数草案

- 可在 `WorldRarityProfile` 中配置：
  - `RarityTier`
  - `SpawnMultiplier`
  - `RevealByDefault`
  - `FogPriority`
  - `DiscoveryHintChance`
- 推荐值：

| RarityTier | SpawnMultiplier | RevealByDefault | DiscoveryHintChance | 用途 |
| --- | --- | --- | --- | --- |
| `Common` | `1.00` | `true` | `0.00 ~ 0.05` | 常规可见点 |
| `Uncommon` | `0.60` | `true` | `0.05 ~ 0.15` | 需要轻探索 |
| `Rare` | `0.28` | `false` | `0.15 ~ 0.35` | 依赖传闻或接近后发现 |
| `Legendary` | `0.08` | `false` | `0.35 ~ 0.65` | 强依赖线索、门槛或事件链 |

#### F. 解锁门槛参数草案

- 可在 `WorldUnlockRule` 中配置：
  - `UnlockTier`
  - `MinSectReputation`
  - `MinExpeditionDepth`
  - `MinHeroPower`
  - `RequiredRumorTags`
  - `RequiredFactionRelation`
- 推荐门槛：

| UnlockTier | 宗门声望 | 历练层数 | 英雄平均战力 | 关系/线索要求 |
| --- | --- | --- | --- | --- |
| `0` | `0` | `0` | `0` | 无 |
| `1` | `20 ~ 40` | `3 ~ 6` | `80 ~ 140` | 至少一种传闻或基础关系 |
| `2` | `60 ~ 100` | `8 ~ 12` | `180 ~ 260` | 需要特定传闻、盟约或遗迹前置 |

#### G. 伴生生成参数草案

- 可在 `WorldCompanionSpawnRule` 中配置：
  - `HostType`
  - `HostTag`
  - `CompanionType`
  - `CompanionTag`
  - `SpawnChance`
  - `MinDistanceFromHost`
  - `MaxDistanceFromHost`
- 推荐伴生规则：

| Host | Companion | SpawnChance | 距离建议 | 说明 |
| --- | --- | --- | --- | --- |
| `Sect` | `Market.SectMarket` | `0.35 ~ 0.60` | `1 ~ 2` | 山门外宗门坊 |
| `ImmortalCity` | `Market.RoadsideMarket` | `0.40 ~ 0.70` | `1 ~ 2` | 城郊交易点 |
| `ImmortalCity` | `Market.BlackMarket` | `0.08 ~ 0.18` | `1 ~ 3` | 大城暗面 |
| `Ruin` | `Market.BlackMarket` | `0.10 ~ 0.22` | `1 ~ 2` | 遗迹外围倒货 |
| `MortalRealm.CountySeat` | `Market.RoadsideMarket` | `0.22 ~ 0.40` | `1 ~ 2` | 县镇集市 |
| `MortalRealm.BorderFort` | `Market.BlackMarket` | `0.12 ~ 0.24` | `1 ~ 2` | 边地私市 |

#### H. 调参优先级建议

- 若首版生成结果“不像修仙世界”，优先调：
  - `WorldRegionProfile.CoverageWeight`
  - `Sect / ImmortalCity / Ruin` 的 `MinHexDistance`
  - `Market` 的伴生概率
  - `Rare / Legendary` 的 `SpawnMultiplier`
- 若首版结果“像修仙世界但不好玩”，优先调：
  - `UnlockTier`
  - `DiscoveryHintChance`
  - `MortalRealm / Market` 的密度
  - `Wilderness` 到 `Ruin` 的过渡强度
- 不建议在第一轮就精细到单个 tag 的最终平衡；先让世界骨架与点位层级正确，再做局部调权。

### 13.2.7 一期实现顺序建议

- 一期先落地高差异四类：
  - `Sect / 宗门`
  - `Wilderness / 野外`
  - `Market / 坊市`
  - `Ruin / 遗迹`
- 二期补 `MortalRealm / 凡俗国度`、`CultivatorClan / 修仙世家`、`ImmortalCity / 仙城` 的关系经营与长期驻点逻辑。
- 原因：后三类若没有关系系统、委托系统、供给系统支撑，容易先做成仅名字不同的 UI 空壳。

## 14. 主界面与客户端设置

- 双布局（Legacy/Figma）共享同一 `GameLoop` 与 `GameState`。
- Legacy 主界面当前额外采用“案几 + 宣纸 + 左右画轴”的卷轴隐喻：`Main.tscn` 负责桌面背景、卷轴纸面、木轴与上下界栏，所有 Legacy UI 默认写在卷面而非漂浮黑底。
- Legacy 主界面根背景已切换为 `CountyIdle/assets/ui/background/background_2.png` 对应的卷轴山水底图；该图本身已对齐内框长画幅构图，运行时直接整图作为背景源，并随窗口尺寸变化自动重排，以 cover 方式始终填满整个游戏窗口；背景节点额外挂载毛玻璃 shader，通过轻微散射、乳白染色与细颗粒压低山水细节，减少对前景 UI 的干扰。
- Legacy 主界面当前采用“中央六边形沙盘 + 左侧地块检视器 + 右侧宗门纪事 + 底部控制台”的 overlay 布局；中央视觉优先留给 `WorldPanel` 中的天衍峰山门图 / 世界地图。
- 天衍峰山门图当前仅展示地貌，左侧检视卡支持全格检视；弟子/场所可视化仍停用。
- `宗主中枢 / 弟子谱 / 仓储` 的常用入口当前收口到底部控制台；地图页签区域只保留地图切换与缩放相关控件，降低中央地图上沿的按钮密度。
- 左侧 `Tile Inspector` 当前采用“主标题 + 地块副标题 + 4 格属性卡 + 三级动作按钮 + 卷内批注”的层次，玩家优先读取地块信息与当前可执行项。
- 左侧 `Tile Inspector` 在未选中时默认禁用动作按钮；选中不同 tile 后，三级动作会按 `灵田 / 工坊 / 总坊 / 传法院 / 庶务殿 / 休憩点` 等类型切换为更具体的“扩建 / 调度 / 打开对应面板”入口。
- 左侧 `Tile Inspector` 进一步补入 `tile 类型徽记 + 可执行项摘要` 两层可视反馈：选中不同 tile 时会切换强调色、徽记文案，并把三条可执行项直接汇总到按钮下方。
- 中央沙盘中被选中的 tile 当前会使用与左侧检视卡同源的强调色，绘制双层 halo、外描边与入口路径高亮，进一步强化“地图选中 -> 左侧检视”的单点联动感。
- 原 `JobsPadding` 主界面摘要区已移除；与峰脉详批、协同峰令相关的治理信息后续应迁入专门治理界面，而非继续挂在主界面左栏。
- 顶栏 / 底栏 / 左右侧栏当前统一切到“墨字 + 纸色 + 细界线”风格：顶栏资源与日期使用卷首批注写法，底栏按钮改为卷尾法令式描边按钮，右栏日志改为纸边墨迹而非浮窗卡片。
- 底部控制台、中央地图页签与缩放按钮当前补齐 `normal / hover / pressed` 三态：默认淡墨描边、悬停浅纸色、选中为更深的卷面压色，避免卷尾法令与地图切换长期停留在“同一描边”状态。
- 左侧 `Tile Inspector` 的三级动作文案统一去除 emoji，改为 `扩建阵材圃 / 调度堂口法旨 / 查阅门人谱` 等墨线批令语义；未选中 tile 时继续保留禁用逻辑。
- Legacy 主界面后续视觉优化以参考 HTML / 卷轴图为主参照，优先保持卷面骨架、左右栏比例、顶部题头、底部控制台与地图控件落位接近参考布局；任何布局重排不得破坏 `tile 选中 -> 左侧 Tile Inspector 刷新 -> 对应操作入口` 链路。
- 右侧 `宗门纪事` 当前进一步收口为“札记卷式微缩日志窗”：顶部只保留少量高优先风闻卡，题头统一为 `山门近闻 / 天衍峰札记`，正文日志采用更紧凑的标题层级与留白，让右栏维持情报感而不抢占沙盘注意力。
- 右侧两条风闻卡由当前宗门状态实时生成，优先读取 `威胁 / 仓储负载 / 药剂覆盖 / 住房压力 / 探险状态 / 季度法令 / 协同峰` 等字段，不改变日志流与核心结算。
- 倍速按钮只改时间流速，不改“60 分钟结算”规则。
- Legacy 布局新增 `宗门组织谱系弹窗` 入口：
  - 由 `SectOrganizationPanel.tscn` 作为独立弹出式界面
  - 底部 `【峰令】谱系` 负责打开，支持 `Esc` 收卷
  - 左侧展示九峰总览与四条职司导览，右侧展示峰脉详批、协同峰令与“转宗主中枢”跳转
  - 与 `治宗册 / 弟子谱 / 仓储 / 留影录 / 机宜卷` 共同复用“卷轴子面板家族”外壳：左右木轴、上下绫边、卷首横题与墨线批令
- 所有 `PopupPanelBase` 卷册弹窗统一使用独立前景层：
  - `ZAsRelative = false`
  - `ZIndex = 200`
  - 打开时额外调用 `MoveToFront()`
- 目的：避免主界面卷轴骨架、顶底栏与中央 hex 沙盘因自定义 `z_index` 压到弹窗正文之上。
- Legacy 布局新增 `仓储弹窗` 入口：
  - 由 `WarehousePanel.tscn` 作为独立弹出式界面
- 视觉结构改为 `木轴账册 + 宣纸主卷 + 标题卷首 + 朱砂预警`
- 左侧库存区改为“物资图鉴清单”双列卡片，并提供 `全览 / 农桑 / 金石 / 百工` 页签切换
- 每个物资条目优先显示正式材料图片，辅以品阶挂缀、名称与整数库存；零库存条目保留但弱化显示
- 右侧操作区改为 `天工造物 / 宗门土木 / 供养法旨` 三组批令
- 满载预警以朱砂印记与批红警语呈现，不再使用现代负载进度条
- 提供快捷操作：矿仓联建、制工具、扩建傀儡工坊、扩建庶务殿与四条 `T0` 链路配置
  - 弹窗打开时支持 `Esc` 快速关闭
  - 提示条根据仓储负载显示操作建议，执行快捷操作后短暂显示请求反馈
- 客户端设置存储 `user://client_settings.json`：
  - 语言：`zh_CN / en`
  - 分辨率白名单：`1280x720 / 1600x900 / 1920x1080 / 2560x1440`
  - 字体缩放：`0.85 ~ 1.30`
  - 主音量：`0.0 ~ 1.0`
  - 快捷键：支持打开设置、打开仓储、探险开关、倍速切换、快速存档、快速读档、快速重置，且可在设置面板点击后按键录制
  - 设置面板视觉已统一为“机宜卷”书卷子面板：宣纸主卷、木轴装裱、墨线控件与符令语义提示
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
  - 面板视觉已统一为“留影录”书卷子面板：宣纸主卷、木轴装裱、卷册目录、卷页详录与墨线批令
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

## 影响摘要（Flywheel）

- 人口繁衍：由食物/住房/睡眠/衣物/通勤/疾病多因子控制，具备出生、衰老、患病、死亡与恢复反馈。
- 职业分化：岗位容量、工具覆盖、管理加成共同决定产能。
- 资源深加工：矿石经冶炼与研发形成新材料，再反哺工具、建材与扩建节奏。
- 精英繁育：低频高价值增量，与探险战力形成联动。
- 武装探险：按 3 小时节奏产出金币/稀材/装备并影响威胁。
- 反哺宗门：科研突破、事件收益和建造扩产回流山门发展。

## 可执行工单句（交接用）

“在不改变 `60 分钟小时结算` 与 `GameState` 存档兼容的前提下，按 `Industry -> Resource -> Economy` 串行结算资源链，并同步更新本文对应章节与改动记录。”

## 17. 宗门弟子可视移动系统（Town Residents）

- 状态：当前停用。
- 作用范围：天衍峰山门图仅保留地貌、道路与水域渲染，不生成住宅、场所与弟子可视对象。
- 交互规则：不提供场所/门人选中与“弟子谱”联动；地块检视入口保留。
- 恢复方式：如需重启，可按历史“弟子可视移动”规则恢复生成与渲染链路。

## 18. 地图与经营状态联动（Map Operation Link）

- 作用范围：`World / Prefecture / CountyTown` 三类地图页共用一套经营状态映射，其中 `CountyTown` 对外语义为天衍峰山门图。
- 经营 → 地图：
  - 世界图重点读取：`Happiness`、`Threat`、`Gold`、`Food`；
  - 江陵府外域备用视图重点读取：`Threat`、`Happiness`、`Gold`、`Food`；
  - 宗门图重点读取：`HousingCapacity / Population`、`Threat`、`Happiness`、`ContributionPoints`。
- 地图状态档位：`繁荣 / 平稳 / 吃紧 / 紧张`，至少影响：
  - 地图标题附加状态词；
  - 地图状态条提示文案；
  - 地图主体色调（江陵府外域备用视图/世界图）或建筑/地块色调（宗门图）。
- 地图 → 经营：
  - 地图页提供双按钮调度入口；
  - 当前调度包括：外域 `整修灵道 / 抚恤附庸` 与宗门 `修整坊路 / 夜巡清巷`；
  - 调度即时改变 `Happiness`、`Threat` 与部分资源库存，并立刻发布日志与状态刷新。
- 护栏：
  - 调度不得使资源或威胁等字段出现负值异常；
  - 不改变小时结算顺序；
  - Figma 布局可暂不暴露地图调度入口，但核心状态计算需保持兼容。



