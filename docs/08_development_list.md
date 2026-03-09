# CountyIdle 开发列表（顺序执行）

> 用途：保证后续开发按“完整功能包”推进，不做突兀的零散插入。
>
> 世界观基线：对外设计与文案统一使用“浮云宗（青云州江陵府）+ 天衍峰经营 + 青云峰总殿协同 + 外域附庸圈层”语义；文档中的 `County / Town / Prefecture` 仅视为历史技术命名或兼容层入口。
>
> 术语补充：本文中的“人口 / 职业 / 科技 / 英雄”等技术词，默认按 `docs/09_xianxia_sect_setting.md` 映射为“门人 / 职司 / 传承研修 / 真传与核心战力”理解。

## 1) 需求受理规则（对话即执行）

每次你提出新需求时，先执行以下检查：

1. 先查 `docs/05_feature_inventory.md`
   - 若是 `✅ 已实现`：按“优化/修复”处理，不重复造功能
   - 若是 `🟡 部分实现` 或 `⭕ TODO`：直接从对应条目继续推进
2. 若在 `05` 中找不到对应功能：
   - 将需求先写入本开发列表（新增一条完整功能包）
   - 再进入开发，不直接跳过立项
3. 每次“继续完善游戏”默认按本列表从上到下执行
   - 除非你明确指定某一条先做

## 2) 功能包定义（必须完整）

每条开发项必须包含：

- 目标（玩家价值）
- 飞轮环节（门人 / 职司 / 苗子 / 历练 / 反哺）
- 依赖（前置系统）
- 完成标准（可验收）

> 任何不满足以上四项的零散需求，不进入开发执行。

### 2.1 价值观与伦理约束（强制门禁）

- 职业系统只体现分工，不体现人群高低贵贱。
- 禁止职业歧视、仇恨、压迫、去人性化叙事和奖励机制。
- 宗门系统目标是“共同建设、共同受益、共同护持”。
- 功能上线前必须做一次“伦理与表述检查”（文案 + 机制）。

## 3) 当前开发列表（按顺序）

| ID | 功能包 | 当前状态 | 飞轮环节 | 完成标准（DoD） |
| --- | --- | --- | --- | --- |
| DL-001 | 战略地图配置驱动接入 | TODO | 反哺宗门 | 世界/江陵府外域图由 `data/strategic_maps.json` 驱动渲染，fallback 仍可运行 |
| DL-002 | 外域历练地图玩法闭环 | TODO | 武装探险 | `ExpeditionMapView` 接入节点、路线风险、遭遇结果并反馈到 `GameState` |
| DL-003 | 宗门见闻与统计报表实时化 | TODO | 反哺宗门 | 事件 / 报表页显示真实结算数据与历史摘要，不再是静态文案 |
| DL-004 | 职司优先级调配策略 | TODO | 职业分化 | 优先级按钮影响岗位自动回退 / 分配顺序，并有日志可解释 |
| DL-005 | 核心公式配置化（data 接入） | TODO | 职业分化/反哺 | 产业、经济、战斗关键参数由 `data/*.json` 读取并可回退默认值 |
| DL-006 | 传承 / 科技树分支化系统 | TODO | 科技涌现 | 从线性 T1/T2/T3 扩展为分支科技树，支持路线差异化 |
| DL-007 | 职司转任系统 | TODO | 职业分化 | 人口在条件满足时可转职，成本收益明确且可回退 |
| DL-008 | 装备 / 法器打造系统 | TODO | 武装探险 | 形成“材料->打造->品质结果”完整链路并联动探险 |
| DL-009 | 真传 / 英雄单位实体化历练 | TODO | 精英繁育→武装探险 | 从“精英人口池”升级为可成长英雄单位系统 |
| DL-010 | Boss / 妖王与词条克制系统 | TODO | 武装探险 | 探险高层出现 Boss 机制，克制关系影响胜负与掉落 |
| DL-011 | 护山战与守山压力闭环 | TODO | 武装探险→反哺 | 高威胁触发攻城，结果影响城防/人口/资源并可恢复 |
| DL-012 | 灵根苗子培育深度化 | TODO | 精英繁育 | 后代由父母属性/血缘规则决定，优秀后代概率与父母质量相关 |
| DL-013 | 地图与经营状态联动 | DONE（一期） | 反哺宗门 | 地图状态条与调度按钮已接入；经营状态会改变世界/江陵府外域/天衍峰山门图表现，地图调度可反向影响民心、威胁、道路与资源 |
| DL-014 | 仓储与矿产资源链系统 | DONE | 产业涌现→科技涌现→反哺宗门 | 新增仓储容量与溢出规则，矿石可开采并经研发转化为新材料，用于产品制造与建筑建造 |
| DL-015 | 江陵府外域备用视图 Village 风格程序化生成 | DONE（hex 底格补强） | 反哺宗门 | 江陵府外域备用视图由程序化算法生成有机边界/道路/河流/附庸据点节点，并按经营状态分桶重建；已扩展为外域州府式高密坊市、问道长街/坊巷、高倍街屋细化与 hex 战略底格 |
| DL-016 | 专用仓库管理界面 | DONE（五期：库房账册书卷化） | 产业涌现→反哺宗门 | 新增“仓储管理”入口与独立面板；当前仓储弹窗已重做为 `木轴账册 + 朱砂预警 + 左侧物资图鉴 + 右侧批红调度` 结构，支持 `全览/农桑/金石/百工` 页签，物资条目优先显示正式材料图片与整数库存，同时保留快捷操作、`Esc` 关闭与统一提示条反馈 |
| DL-017 | 人口分配与生活循环系统（住房/通勤/生老病死） | IN_PROGRESS（一期） | 人口繁衍→职业分化→产业涌现 | 人口分配受住房、食物、睡眠、情绪、工具与衣物覆盖约束；通勤距离影响到岗率并可在日志观测 |
| DL-018 | 快捷键配置系统 | DONE（设置面板已书卷化） | 产业涌现→反哺宗门 | 设置面板支持快捷键重绑定并持久化，支持打开设置/仓储、探险开关、倍速切换、存档读档重置快捷键，支持按键录制、统一提示条反馈、冲突交换提示与 `Esc` 取消/关闭；当前设置面板外观已统一为“机宜卷”书卷子面板 |
| DL-019 | 宗门弟子可视移动系统 | DONE（五期 场所选中） | 人口繁衍→反哺宗门 | 天衍峰山门图出现可见弟子形象，按作息时间在住房、具体工作地与休闲点之间移动，工作/休闲点以实体场所建筑呈现，并支持场所选中提示，不改写经济结算 |
| DL-020 | 架空农历与节气时间表现 | DONE（双进度条补强） | 反哺宗门 | 顶栏显示“某年某月某日 + 季度/天双进度条 + 节气”，不再出现结算倒计时，并支持 `x1/x2/x4` 倍率 |
| DL-021 | 岗位面板进程化交互 | DONE（已转治理摘要入口） | 职业分化→产业涌现→科技涌现 | `JobsPadding` 现已退为职司摘要与治理入口；具体调度改由“宗主中枢”决定方向与轻重 |
| DL-023 | 资源系统分层扩展（V1：T0-T1） | IN_PROGRESS（六期：修仙材料语义化） | 产业涌现→科技涌现→人口繁衍→反哺宗门 | 地图只产出自然原材料；`T0/T1` 形成“采集->加工->消费”闭环；玩家可见材料名统一切到修仙语义；物品最小单位为 `1` 且可见库存无小数；`青铜 / 纸张 / 玻璃 / 皮革 / 火药` 不再误入原料层；建造、民生、工具三类消费端可观测；`dotnet build .\Finally.sln` 通过 |
| DL-024 | SQLite 存档迁移（V1：默认槽 + 快照） | DONE | 反哺宗门 | 存档介质改为 SQLite；默认槽与快照表落地；旧版 `savegame.json` 可迁移；主界面与快捷键无需改 UI 即可继续存档/读档；`dotnet build .\Finally.sln` 通过 |
| DL-025 | 多存档槽管理界面（V1：手动槽） | DONE（八期：留影录书卷化） | 反哺宗门 | 主界面“存档 / 读档”打开多槽面板；支持查看、覆盖、读取、新建、复制、重命名、删除手动槽；存在 `自动存档 1 / 2 / 3` 三个受保护自动槽并按 `6` 次小时结算轮换写入；面板显示更完整的存档摘要预览，支持筛选排序与截图预览；当前外观已统一为“留影录”书卷子面板；快速存档 / 快速读档继续走默认槽；`dotnet build .\\Finally.sln` 通过 |
| DL-026 | 修仙 Hex 世界生成系统 | DONE（二期：edge overlay） | 反哺宗门 | 世界图默认使用修仙世界程序化生成，输出 `Biome / Terrain / Water / Cliff / Overlay / Resource / Spiritual Zone / Site / Wonder` 八层语义；河流/道路改为按 hex 边绘制 overlay，并保留配置驱动 fallback；`dotnet build .\\Finally.sln` 通过 |
| DL-027 | 双地图布局（天衍峰山门图 + 世界地图） | DONE（一期） | 反哺宗门 | 主界面地图区域只保留“天衍峰山门图 / 世界地图”两个入口；原江陵府外域备用视图/事件/报表/探险地图页退出主地图页签；宗门图复用现有 hex 天衍峰驻地视图链路；`dotnet build .\\Finally.sln` 通过 |
| DL-028 | 天衍峰山门图语义改造（V1） | DONE（一期） | 反哺宗门 | 天衍峰山门图继续复用现有 hex 驻地视图链路，但场所命名、提示文案、驻地功能标签与视图脚本入口统一切到宗门语义；`dotnet build .\\Finally.sln` 通过 |
| DL-029 | 宗门经营全局语义统一（V2） | DONE（一期） | 反哺宗门 | 岗位/仓储/研究/资源日志/事件提示/地图调度与世界图标题统一切到宗门术语；隐藏备用 Prefecture 标题改为“江陵府外域”；`dotnet build .\\Finally.sln` 通过 |
| DL-030 | 江陵府外域备用视图修仙语义化（V1） | DONE（一期） | 反哺宗门 | 隐藏备用 Prefecture 视图的标题、城名、地标、街区与配置 fallback 统一切到修仙外域语义；`strategic_maps.json` 的世界/外域标题同步改到 `世界地图 / 江陵府外域`；`dotnet build .\\Finally.sln` 通过 |
| DL-031 | 江陵府外域备用视图运营语义统一（V2） | DONE（一期） | 反哺宗门 | 隐藏备用外域视图的调度按钮、状态提示、采集日志与标题 fallback 统一切到 `灵道 / 附庸据点 / 抚恤附庸 / 峰外采办` 语义；`dotnet build .\\Finally.sln` 通过 |
| DL-032 | 修仙世界观文档统一 | DONE（本轮） | 反哺宗门 | `README / 01 / 02 / 05 / 08` 的对外设计语义统一为“宗门经营 + 外域探索”的修仙背景，并注明旧 `County*` 技术命名仅作兼容说明 |
| DL-033 | 天衍峰经营文档重梳（V2） | DONE（本轮） | 反哺宗门 | 新增 `09_xianxia_sect_setting.md` 作为术语总表，并重写导航 / 愿景 / 流程 / 周检 / 看板 / 开发列表，使“修仙天衍峰经营”成为唯一对外背景；`dotnet build .\\Finally.sln` 通过 |
| DL-034 | 历史归档语义桥接（V1） | DONE（本轮） | 反哺宗门 | 给旧 `FC / CP / BL` 批量补“历史兼容说明”，并为高频归档补“当前对外语义”与宗门化摘要层，降低打开旧文档时的语义跳戏；`dotnet build .\\Finally.sln` 通过 |
| DL-035 | 天衍峰法旨兼容底层与双轨内务结算 | DONE（兼容底层） | 职司分化→产业供养→反哺宗门 | 内务统一走 `贡献点 + 灵石` 双轨，外域交易仅使用灵石；`TaskOrderUnits` 继续作为旧系统兼容折算层，但不再要求玩家直看人数；`dotnet build .\\Finally.sln` 通过 |
| DL-036 | 浮云宗·天衍峰设定对齐 | DONE（本轮） | 反哺宗门 | 核心文档与玩家可见术语定锚到“浮云宗 / 青云峰三总殿 / 天衍峰阵堂”，任务、设施与地图标签按该设定统一；`dotnet build .\\Finally.sln` 通过 |
| DL-037 | 宗主治理中枢（一期：方略层去人头化） | DONE（本轮，治宗册书卷化） | 职司分化→产业供养→传承研修→反哺宗门 | 玩家只决定治理方向、法令力度与任务重点，不再直接看到人数配置；主界面与职司摘要同步改成“宗主定调 / 执事落实”语义；当前中枢弹窗已统一为“治宗册”书卷子面板；`dotnet build .\\Finally.sln` 通过 |
| DL-038 | 宗主治理中枢（二期：发展方向 / 法令 / 育才） | DONE（本轮） | 职司分化→产业供养→传承研修→人口繁衍→反哺宗门 | 新增三层治理选择并接入小时结算；切换方向会重排治理条目默认侧重；法令与育才能实际影响民心、威胁、研修、贡献与灵石回流；`dotnet build .\\Finally.sln` 通过 |
| DL-039 | 宗门组织谱系展示（卷册总览） | DONE（本轮） | 反哺宗门 | 组织谱系已从主界面左栏迁入独立卷册弹窗，底部 `【峰令】谱系` 可打开九峰、三总殿与天衍峰附属部门总览；当前卷册已补齐“峰令谱”书卷子面板外壳：左右木轴、上下绫边、卷首横题与墨线批令；`dotnet build .\\Finally.sln` 通过 |
| DL-040 | 宗门组织谱系详情浏览（卷册详批） | DONE（本轮） | 反哺宗门 | 独立卷册支持峰脉详情浏览，可在九峰与附属部门间切换；点选四条主职司时会自动聚焦推荐峰脉，并显示更细的处室 / 附属部门说明；峰脉详批已完全纳入统一卷轴子面板家族；`dotnet build .\\Finally.sln` 通过 |
| DL-041 | 峰脉协同法旨（卷册治理入口） | DONE（本轮） | 产业供养→传承研修→人口繁衍→反哺宗门 | 独立卷册峰脉详情区可直接下发“本季协同峰”，并把协同峰效果挂到食物、灵石、贡献、研修、人口增长、民心、威胁与工器锻制上；动作区已统一为“峰令谱”墨线批令风格；`dotnet build .\\Finally.sln` 通过 |
| DL-042 | 季度法令（宗主治理三期先行） | DONE（本轮） | 产业供养→传承研修→人口繁衍→反哺宗门 | 宗主中枢新增“季度法令”一层，法令效果会挂到小时结算、门人生息与工器锻制；季度轮换时上季法令会自动失效并等待新令；`dotnet build .\\Finally.sln` 通过 |
| DL-043 | 宗门弟子独立属性界面（弟子谱） | DONE（本轮） | 人口繁衍→职司分化→反哺宗门 | 主地图页签新增“弟子谱”入口与独立弹窗，可按真传/职司筛选弟子，并查看年龄、修为、心境、气血、潜力、战力、匠艺、悟性、执行与贡献等属性详情；详情区已继续补强为卷轴档案视图，含纵向名册、宣纸详情页、六维根基罗盘、修为/战力/气海条目与衍天批注；左侧名册已升级为固定宗门组织树，按“峰脉 → 堂口 / 机构 → 条线 / 班序 → 册级 → 弟子”多层展开，便于峰内分层管理；当前外层已进一步收口到与 `治宗册 / 留影录 / 机宜卷 / 库房账册 / 峰令谱` 同族的书卷子面板；天衍峰山门图点击可视弟子/场所时可直接联动弟子谱定位；`dotnet build .\\Finally.sln` 通过 |
| DL-044 | 门规树（一期：三支门规纲目） | DONE（本轮） | 产业供养→传承研修→人口繁衍→反哺宗门 | 宗主中枢新增 `庶务 / 传功 / 巡山` 三支门规纲目，门规会真实影响收益、人口、威胁与工器锻制；`dotnet build .\\Finally.sln` 通过 |
| DL-045 | 主界面六边形沙盘重构 | DONE（本轮） | 反哺宗门 | 主界面改为“中央 hex 沙盘 + 左侧地块检视 + 右侧宗门纪事 + 底部控制台”布局；天衍峰场所选中会同步刷新地块检视卡；不改小时结算与存档结构；`dotnet build .\\Finally.sln` 通过 |

### 3.1 DL-017 功能包详情（人口分配与生活循环）

- 目标（玩家价值）：让“人从哪里来、住在哪里、如何上工、为什么减员”形成可解释闭环，避免人口只作为抽象数字。
- 飞轮环节：人口繁衍 → 职业分化 → 产业涌现（通勤与状态反哺岗位有效产能）。
- 依赖（前置系统）：`PopulationSystem`、`IndustrySystem`、`EconomySystem`、`TownMap/PrefectureMap`（距离输入）、`GameState` 存档兼容。
- 完成标准（DoD）：
  - 住房不足时，睡眠恢复下降并影响次小时有效劳动力；
  - 食物、睡眠、衣物共同影响患病率与死亡率，且全程无负值异常；
  - 出生/衰老/死亡为独立可观测增减项（日志可解释）；
  - 住房到岗位平均距离转化为步行通勤时间，并影响岗位到岗率；
  - `dotnet build .\Finally.sln` 通过，`60 分钟`结算节奏不变。

### 3.2 DL-019 功能包详情（宗门弟子可视移动）

- 目标（玩家价值）：让人口不再只是面板数字，玩家能在天衍峰山门图上直观看到弟子活动与通勤。
- 飞轮环节：人口繁衍 → 反哺宗门（仅做视觉反馈，不直接改数值）。
- 依赖（前置系统）：`CountyTownMapViewSystem`、`TownMapGeneratorSystem`、`GameState` 人口/岗位数据、主界面地图刷新链路。
- 完成标准（DoD）：
- 天衍峰山门图上出现与岗位结构对应的弟子形象；
  - 居民会按作息时间从住房出门、通勤到具体工作地，并在收工后去休闲点再回家；
  - 工作地与休闲点以临路实体场所建筑表现，而不只是抽象圆点；
- 左键选中场所后，提示条能显示场所状态与当前可视弟子统计；
  - 地图重建、存档读档、倍率变化后居民显示不报错；
- 该系统仅影响视觉层，不改 `GameState` 核心结算字段；
- `dotnet build .\Finally.sln` 通过。
- 2026-03-08 补强：
  - 宗门主视图地表与格位中心统一切换为 hex tile 俯视；
  - 弟子移动、场所选中与建筑 / 场所 overlay 继续沿用现有链路，不改 `TownMapGeneratorSystem` 与结算逻辑。
  - 宗门住宅底盘、场所底座、场所高亮与命中区进一步收敛为 hex footprint。
  - 宗门道路与水域追加 hex 地形语义 overlay，补强通路连接感与岸线边界。

### 3.3 DL-020 功能包详情（架空农历与节气时间表现）

- 目标（玩家价值）：让时间信息更符合修仙宗门经营题材，玩家能直接理解当前“年/月/日/节气”，并快速切换节奏。
- 飞轮环节：反哺宗门（主要影响表现层与操作节奏，不改核心数值公式）。
- 依赖（前置系统）：`GameLoop`、`MainUI`、`Figma UI`、`CountyTownMapViewSystem`、读档流程。
- 完成标准（DoD）：
  - 顶栏显示为架空农历日期，并融入 `24` 节气；
  - 顶栏提供“季度进度条 + 当日进度条”双层时间反馈，其中季度条按 `90` 日推进、当日条按 `24` 时推进；
  - 主界面不再出现“结算倒计时”字样；
  - 倍率按钮与快捷键支持 `x1/x2/x4`；
  - 读档后小时结算相位保持连续；
  - `dotnet build .\Finally.sln` 通过。

### 3.4 DL-021 功能包详情（岗位面板进程化交互）

- 目标（玩家价值）：让岗位面板不再只给出抽象统称，玩家能直接看懂“当前有哪些具体岗位、为什么容量是这个数、下一步建筑/科技要怎么推进”。
- 飞轮环节：职业分化 → 产业涌现 → 科技涌现（岗位名称、容量规则与建筑/科技进度形成可解释反馈）。
- 依赖（前置系统）：`IndustryRules`、`ResearchSystem`、`GameLoop` 岗位调配、`jobs.json` 具体岗位配置、主界面 Legacy `JobsPadding` 面板。
- 完成标准（DoD）：
  - `JobsPadding` 内每条具体岗位都可点击查看规则详情，并支持逐项 `+/-` 调整；
  - 岗位标题固定显示真实岗位名称，而不是“农工岗/匠役岗/商贾岗/学士岗”这类统称；
  - 详情中可读到所属建筑、科技要求、当前岗额、已派人数与下一阶解锁条件；
- 岗位加减与人口/容量回退会同步保护“优先保留”的具体岗位；
- `dotnet build .\Finally.sln` 通过。
- 2026-03-08 任务制补充：
  - `JobsPadding` 已从“直接调岗位”退为“职司摘要 + 宗主中枢入口”；
  - 玩家不再直接加减四类岗位人数，而是通过“宗主中枢”决定治理方向与轻重；
  - 行内按钮与头部按钮默认打开宗主中枢，行展开内容改为显示治理侧重与执事执行说明。

### 3.6 DL-023 功能包详情（资源系统分层扩展 V1：T0-T1）

- 目标（玩家价值）：让玩家明确知道“地图上采到的是自然原料，宗门工坊里加工的是材料，最终消费的是成品”，并在前期就感知住房、药物、衣物、工具与基础冶铸的来源差异。
- 飞轮环节：产业涌现 → 科技涌现 → 人口繁衍 → 反哺宗门（资源分层先支撑建造、民生与工具，再为后续合金、轻工与特产贸易预留路径）。
- 依赖（前置系统）：`ResourceSystem`、`IndustrySystem`、`EconomySystem`、`PopulationSystem`、`WarehousePanel`、`GameState` 存档兼容、当前 `T1/T2/T3` 科技阶段。
- 当前进度（2026-03-07 / 一期）：
  - 已实现：`GameState` 新增 `T0/T1` 材料库存字段；`ResourceSystem` 接入自然原料采集、初加工、民生产线与 `铜锭/熟铁` 冶铸；`IndustrySystem` 制工具优先消耗 `熟铁/铜锭`；`PopulationSystem` 接入 `精盐/药剂/麻布/皮革` 对幸福、健康、衣物覆盖的影响；`WarehousePanel` 可观察新材料。
- 当前进度（2026-03-08 / 二期）：
  - 已实现：`EconomySystem` 不再直接产出 `木/石`；`木料/石料` 完全由 `林木/原石 -> 初加工` 链路提供；主界面木材提示可观察下一小时的 `木料/石料` 预计产量。
- 当前进度（2026-03-08 / 三期）：
  - 已实现：`T0` 建筑链显式化为四条可扩建链路：`灵木链（灵植园/伐木坊）`、`石陶链（采罡场/赤陶窑/石作坊）`、`盐丹链（盐泉/采药圃/丹房）`、`织裘链（青麻圃/青芦泽/灵兽围/制裘坊）`；仓储面板可直接扩建与观察等级。
- 当前进度（2026-03-08 / 四期）：
  - 已实现：物品/资源可见库存一律整数化；自动结算中的分数变化进入 `DiscreteInventoryProgress` 隐藏进度池；仓储面板、顶部资源条、主要事件日志不再显示物品小数；手动动作按整数物品结算。
- 当前进度（2026-03-08 / 五期）：
  - 已实现：江陵府外域环境锚点新增“自然原材料来源”节点与标签，只展示 `林木 / 药材 / 皮毛`、`卤水 / 芦苇 / 黏土`、`原石 / 铜矿 / 铁矿`、`麻料`；地图视觉与节点层不再出现 `青铜 / 纸张 / 玻璃 / 皮革 / 火药` 等加工品名称。
  - 待完成：`Godot` 运行烟测与完整读档回归。
- 当前进度（2026-03-08 / 六期）：
  - 已实现：新增 `MaterialSemanticRules` 集中维护玩家可见材料名与描述；仓储面板、主界面资源提示、小时结算日志、人口缺口提示与外域原料标签统一切到 `灵谷 / 灵木料 / 青罡石料 / 灵草 / 赤铜矿 / 玄铁矿 / 养气散 / 护山构件` 等修仙材料语义。
  - 验证补充：`tools/SaveSmoke` 已跑通 SQLite 存档/读档回归，输出覆盖 `主存档 / 自动存档 / 手动槽` 的保存、读取与删除链路。
  - 待完成：`Godot` 运行烟测（当前环境未发现 `Godot` 可执行），并在试玩中观察命名辨识度。
- 完成标准（DoD）：
  - 地图只产出自然原材料，不直接产出 `青铜 / 纸张 / 玻璃 / 皮革 / 火药` 等加工品；
  - `T0` 原材料：`林木 / 原石 / 黏土 / 卤水(岩盐) / 药材 / 麻料 / 芦苇 / 皮毛兽骨` 形成采集与初加工闭环；
  - `T1` 原材料：`铜矿 / 铁矿` 形成 `铜锭 / 熟铁` 闭环，并接入工具或商业前置；
  - `精盐 / 药剂 / 麻布 / 皮革` 至少影响幸福、健康、衣物覆盖中的两项；
- 玩家可见的 `T0/T1` 材料命名、日志和地图标签统一切到修仙语义；
- 仓储与 UI 不会在 `T0/T1` 提前展示 `T2/T3` 材料；
- `dotnet build .\Finally.sln` 通过，`60` 分钟结算节奏不变。

### 3.7 DL-024 功能包详情（SQLite 存档迁移 V1：默认槽 + 快照）

- 目标（玩家价值）：让玩家继续使用原有“存档 / 读档 / 快速存档 / 快速读档”操作，但底层拥有更稳的 SQLite 存档层，为后续多槽与历史快照扩展打基础。
- 飞轮环节：反哺宗门（本次不改变数值飞轮本身，而是提升存档稳定性、迁移能力与后续功能承载力）。
- 依赖（前置系统）：`GameState`、`SaveSystem`、`Main.cs`、`MainShortcutBindings.cs`、Godot `user://` 路径、`.NET 8` 包管理。
- 完成标准（DoD）：
  - 存档介质改为 `user://countyidle.db`；
  - 存在 `schema_migrations / save_slots / save_snapshots` 三张表；
  - 默认槽 `default / 主存档` 可完成完整 `GameState` 快照的写入与读取；
- 若数据库不存在但存在旧版 `user://savegame.json`，首次读档时可自动迁移；
- 现有主界面按钮与快捷键不需要额外 UI 改造；
- `dotnet build .\Finally.sln` 通过。

### 3.8 DL-025 功能包详情（多存档槽管理界面 V1：手动槽）

- 目标（玩家价值）：让 SQLite 存档不再只停留在底层实现，而是通过可视化面板让玩家直接管理多个手动存档槽。
- 飞轮环节：反哺宗门（本次主要提升 UI 与存档管理能力，不改变数值结算本身）。
- 依赖（前置系统）：`DL-024` SQLite 存档迁移、`PopupPanelBase`、主界面底栏存档按钮、快捷键系统。
- 完成标准（DoD）：
  - 主界面“存档 / 读档”按钮会打开多存档槽面板；
  - 面板支持：查看、覆盖所选槽、读取所选槽、新建手动槽、重命名、删除、刷新；
- 默认槽 `default` 保留为快速存档 / 快速读档入口，删除时有保护；
- 快捷键 `QuickSave / QuickLoad` 不受面板改造影响；
- `dotnet build .\Finally.sln` 通过。
- 二期补充：
  - 自动存档槽 `autosave / 自动存档 1` 已接入；
  - 每 `6` 次小时结算自动写入一次；
  - 自动槽只允许读取，不允许手动覆盖、重命名、删除。
- 三期补充：
  - 自动存档扩展为 `自动存档 1 / 2 / 3` 三个轮换槽；
  - 自动存档按轮换顺序依次覆盖，保留最近三次自动记录。
- 四期补充：
  - `save_slots` 摘要字段已扩展为民心、威胁、探险层数、仓储负载；
  - 多槽列表与详情可更直观地比较不同存档的经营状态。
- 五期补充：
  - 多槽面板新增 `全部 / 主存档 / 手动槽 / 自动槽` 筛选；
  - 多槽面板支持按最近写入、游戏进度、人口、金钱、科技排序；
  - 切换筛选排序后会保留可见槽位选择，并提示当前列表计数。
- 六期补充：
  - 每次成功写入默认槽、手动槽、自动槽后，都会尝试保存当前画面截图；
  - 多槽面板详情区新增截图预览框，缺少预览图时会显示占位提示；
  - 删除手动槽时会同步清理该槽位对应的预览图文件。
- 七期补充：
  - 多槽面板新增“复制所选槽”按钮，可把主存档、自动档或手动档复制为新的手动槽；
  - 若未改目标名称或目标名称为空，会自动以“原名 副卷”生成新槽位名；
  - 复制槽位时会同步复制原槽位的截图预览文件（若存在）。
- 八期补充：
  - 多槽面板已统一切到“留影录”书卷子面板：木轴、宣纸主卷、卷册目录、卷页详录与墨线批令；
  - 提示语、按钮文案与状态反馈改用“卷册 / 落卷 / 誊录副卷 / 焚毁所选卷”等语义；
  - 不改 SQLite 存档结构、自动存档轮换和截图保存逻辑。

### 3.9 DL-026 功能包详情（修仙 Hex 世界生成系统）

- 目标（玩家价值）：把“世界地图”从抽象线框图升级为带修仙世界特征的六角地貌图，让玩家能直接看到灵脉、奇观、宗门候选地与稀有资源分布。
- 飞轮环节：反哺宗门（本次为世界图表现与策略信息层增强，不改写经济、人口、战斗主循环）。
- 依赖（前置系统）：`StrategicMapViewSystem`、`StrategicMapConfigSystem` fallback、`Godot` 运行时绘制链路、`CountyIdle/data/xianxia_world_generation.json`。
- 完成标准（DoD）：
  - 世界图默认由 `XianxiaWorldGeneratorSystem` 生成 `64x40` hex 地块数据；
  - 数据层包含 `Biome / Terrain / Water / Cliff / Overlay / Resource / Spiritual Zone / Site / Wonder`；
  - 生成结果可转换为 `StrategicMapDefinition`，直接复用现有世界图缩放与绘制链路；
  - 世界图能展示河流、灵脉、奇观、宗门候选、附庸据点/坊市/古遗迹与稀有资源标记；
  - 修仙生成异常时自动回退到原 `strategic_maps.json` 世界图，不阻断主界面；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 一期落地：
  - 新增 `XianxiaWorldGenerationConfig / XianxiaWorldMapData / XianxiaWorldGenerationConfigSystem / XianxiaWorldGeneratorSystem`；
  - 世界图接入 `data/xianxia_world_generation.json`，默认生成 `2560` 个六角地块，并产出河流、灵脉、奇观、宗门候选与附庸据点数据；
  - 生成器同步输出 `StrategicMapDefinition`：`2560` 个 hex region、`6` 条灵脉路线、`9` 条河流路线、`61` 个节点、`31` 条标签；
- 主界面世界图无需改页签，直接复用现有缩放与状态条链路；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 二期补强：
  - 世界图新增 `RoadMask` 生成链路，附庸据点与宗门候选之间会自动生成道路边连接；
  - `StrategicMapViewSystem` 在世界图模式下读取 `RiverMask / RoadMask`，改为沿 hex 共边绘制 overlay，而不再把河流画在 tile 中心；
  - 世界图进一步补上河岸、桥梁、路口与悬崖边 overlay 细节；
  - 河流 polyline 在修仙世界模式下退居为数据 fallback，正常显示优先走 edge overlay；
  - 默认配置验证结果：`242` 个河流边语义格、`111` 个道路边语义格、`38` 个悬崖边语义格、`8` 个桥梁重叠格、`10` 个路口格；
  - `dotnet build .\Finally.sln` 通过。

### 3.10 DL-027 功能包详情（双地图布局：天衍峰山门图 + 世界地图）

- 目标（玩家价值）：让地图入口更聚焦，玩家始终在“宗门经营”和“世界形势”两个核心视角之间切换，不再被多张地图页分散注意力。
- 飞轮环节：反哺宗门（UI 入口收敛，不改小时结算与核心玩法公式）。
- 依赖（前置系统）：`Main.cs` 地图页签切换、`WorldPanel.tscn` 结构、`CountyTownMapViewSystem`、`StrategicMapViewSystem`。
- 完成标准（DoD）：
  - 主界面顶部地图页签只显示 `天衍峰山门图 / 世界地图`；
  - 天衍峰山门图沿用当前 hex 驻地视图链路并统一天衍峰文案；
  - 世界地图沿用修仙世界生成与缩放链路；
- 原 `江陵府外域备用视图 / 事件面板 / 统计报表 / 野外探险` 不再占用主地图页签；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 一期落地：
  - `Main.cs` 地图默认页改为 `天衍峰山门图`，并把主地图切换约束为双地图模式；
  - `WorldPanel.tscn` 只保留 `天衍峰山门图 / 世界地图` 两个可见地图按钮；
  - 天衍峰山门图页签、提示文案与重建按钮统一改成宗门语义；
  - 世界图默认标题统一为 `世界地图`；
  - `dotnet build .\Finally.sln` 通过。

### 3.11 DL-028 功能包详情（天衍峰山门图语义改造 V1）

- 目标（玩家价值）：让天衍峰山门图不只是“县城图改名”，而是在场所命名、提示文案与视图脚本入口上真正体现天衍峰驻地语义。
- 飞轮环节：反哺宗门（本次只改天衍峰山门图表现语义，不改经济与人口主循环）。
- 依赖（前置系统）：`TownMapGeneratorSystem`、`CountyTownMapViewSystem`、`SectMapViewSystem`、`WorldPanel.tscn`。
- 完成标准（DoD）：
  - 天衍峰山门图场所标签切换为 `阵材圃 / 傀儡工坊 / 青云总坊 / 传法院 / 庶务殿 / 演阵台`；
  - 天衍峰山门图选中提示与状态文本切到宗门语义；
  - 地图场景脚本入口切到 `SectMapViewSystem`；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 一期落地：
  - 新增 `SectMapSemanticRules` 统一宗门场所命名、状态文案与交互提示；
  - 新增 `SectMapViewSystem` 作为天衍峰山门图视图脚本入口，复用既有 hex 驻地渲染链路；
  - `TownMapGeneratorSystem` 生成的场所标签切到天衍峰驻地命名；
  - 天衍峰山门图选中提示从“开工中 / 已收工 / 清闲中”切到“运转中 / 暂歇中 / 静修中 / 议事中 / 论道中”；
  - `dotnet build .\Finally.sln` 通过。

### 3.12 DL-029 功能包详情（宗门经营全局语义统一 V2）

- 目标（玩家价值）：让玩家离开天衍峰山门图主视图后，依然持续看到同一套宗门术语，不再在岗位、仓储、研究、事件日志和地图调度中突然跳回“县城 / 学宫 / 官署 / 市集”语境。
- 飞轮环节：反哺宗门（本次只做全局表现与提示语义统一，不改岗位容量、研究阈值、资源公式与调度数值）。
- 依赖（前置系统）：`SectMapSemanticRules`、`IndustrySystem`、`JobProgressionRules`、`ResearchSystem`、`ResourceSystem`、`MapOperationalLinkSystem`、`WarehousePanel`、`Main.cs`、`WorldPanel.tscn`。
- 完成标准（DoD）：
  - 仓储与岗位面板不再显示 `农坊 / 工坊 / 学宫 / 市集 / 官署` 旧词；
  - 研究突破、资源日志、事件讲习和天衍峰山门图调度提示统一切到宗门术语；
  - 世界图默认标题保持 `世界地图`，隐藏备用 Prefecture 标题改为 `江陵府外域`；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 一期落地：
  - 扩展 `SectMapSemanticRules`，补齐宗门建筑、科技线、世界图标题与宗门称谓统一方法；
  - `IndustrySystem / JobProgressionRules / ResearchSystem / ResourceSystem` 统一改用 `阵材圃 / 傀儡工坊 / 传法院 / 青云总坊 / 庶务殿` 文案；
  - `MapOperationalLinkSystem` 的宗门侧按钮、状态与提示从“县城 / 街坊”切到“宗门 / 坊路”；
  - `WarehousePanel`、`JobsPanel`、`EventLogPanel`、`WorldPanel` 与 `Main.cs` 的可见提示统一收口；
  - `dotnet build .\Finally.sln` 通过。

### 3.13 DL-030 功能包详情（江陵府外域备用视图修仙语义化 V1）

- 目标（玩家价值）：即使未来重新打开隐藏的江陵府外域备用视图，也不会突然看到“开封郡城 / 周边郡图 / 天下州域”这类与当前修仙宗门世界观割裂的旧文案。
- 飞轮环节：反哺宗门（本次只改隐藏备用视图的主题配置与 fallback 文案，不改主界面双地图结构与经营数值）。
- 依赖（前置系统）：`PrefectureCityThemeConfigSystem`、`PrefectureMapGeneratorSystem`、`StrategicMapConfigSystem`、`prefecture_city_theme.json`、`strategic_maps.json`。
- 完成标准（DoD）：
  - 江陵府外域备用视图标题、城名、地标、街区命名统一切到修仙外域语义；
  - `strategic_maps.json` 中 `world / prefecture` 标题与当前主界面语义一致；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 一期落地：
  - `prefecture_city_theme.json` 改为 `江陵府外域（附庸圈层） / 云泽附庸坊城` 主题，并把地标、坊市、街区名称切到外域修仙风格；
  - `PrefectureCityThemeConfig` 与 `PrefectureCityThemeConfigSystem` 的默认值/fallback 同步更新；
  - `PrefectureMapGeneratorSystem` 的地标 fallback 与街区名称兜底同步改成修仙外域术语；
  - `strategic_maps.json` 标题同步改为 `世界地图 / 江陵府外域`；
  - `dotnet build .\Finally.sln` 通过。

### 3.14 DL-031 功能包详情（江陵府外域备用视图运营语义统一 V2）

- 目标（玩家价值）：让隐藏备用外域视图在重新启用或 fallback 时，不只“长得像修仙地图”，连运营按钮、状态提示和资源日志也能保持同一套修仙外域语义。
- 飞轮环节：反哺宗门（本次只改运营提示、状态描述与标题 fallback，不改调度消耗、状态公式和资源产量）。
- 依赖（前置系统）：`SectMapSemanticRules`、`MapOperationalLinkSystem`、`StrategicMapViewSystem`、`ResourceSystem`。
- 完成标准（DoD）：
- 江陵府外域备用视图调度文案不再出现旧外域行政式提示词；
  - 江陵府外域备用视图状态提示统一切到 `灵道 / 附庸据点 / 抚恤附庸`；
  - 采集日志统一改为 `峰外采办`；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 一期落地：
  - 新增 `SectMapSemanticRules` 的外域灵道、附庸据点、抚恤与峰外采办术语；
  - `MapOperationalLinkSystem` 的 Prefecture 分支调度按钮、失败提示、成功日志和状态说明统一切到外域修仙语义；
  - `StrategicMapViewSystem` 的 Prefecture 标题 fallback 统一改为 `江陵府外域`；
  - `ResourceSystem` 的基础采集日志切到 `峰外采办`；
  - `dotnet build .\Finally.sln` 通过。

### 3.15 DL-032 功能包详情（修仙世界观文档统一）

- 目标（玩家价值）：让玩家、策划与开发在阅读核心文档时看到同一套修仙宗门世界观，不再在愿景、规格与看板之间来回切换旧郡县语境。
- 飞轮环节：反哺宗门（统一世界观基线，降低后续功能设计与文案落地的语义偏差）。
- 依赖（前置系统）：`docs/README.md`、`docs/01_game_design_guide.md`、`docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/06_archive_registry.md`、`docs/07_archive_rename_migration_plan.md`、`docs/08_development_list.md`。
- 完成标准（DoD）：
  - 核心愿景、飞轮描述与状态看板统一使用“宗门经营 + 外域探索”修仙语义；
  - 系统规格中玩家可见建筑、事件、地图与场所描述统一切到宗门/外域表述；
  - 文档明确说明 `County / Town / Prefecture` 等仅为代码层历史技术命名；
  - 不再把项目主设定描述为“郡县经营”。
- 2026-03-08 本轮落地：
  - 更新 `README / 01 / 02 / 05 / 08` 的世界观基线说明；
  - 将高层飞轮、功能看板与开发列表中的 `反哺郡县 / 县城地图 / 郡图` 统一改为宗门/外域语义；
  - 保留 `CountyEvent / CountyTown / Prefecture` 等代码名，用文档注释明确其兼容性质。
  - 追加更新 `feature-cards/README.md`、`change-proposals/README.md`、`balance-logs/README.md`、`06_archive_registry.md`、`07_archive_rename_migration_plan.md`，为历史归档补齐修仙语义兼容映射。

### 3.16 DL-033 功能包详情（天衍峰经营文档重梳 V2）

- 目标（玩家价值）：让玩家、策划与开发在阅读核心文档时，不只知道“这是修仙题材”，还能够明确理解自己经营的是“宗门生态”，以及旧技术词在当前背景中的含义。
- 飞轮环节：反哺宗门（通过统一设定入口、术语和执行顺序，降低后续设计与实现的语义偏差）。
- 依赖（前置系统）：`docs/09_xianxia_sect_setting.md`、`docs/README.md`、`docs/01_game_design_guide.md`、`docs/02_system_specs.md`、`docs/03_change_management.md`、`docs/04_weekly_flywheel_review.md`、`docs/05_feature_inventory.md`、`docs/06_archive_registry.md`、`docs/08_development_list.md`。
- 完成标准（DoD）：
  - 新增一份可直接解释 `Population / CountyTown / Prefecture` 等技术词的宗门设定与术语文档；
  - 核心导航、愿景、流程、周检、实现看板与开发列表统一切到“天衍峰经营”主语义；
  - 文档不再把项目主设定描述为“郡县经营”；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 本轮落地：
  - 新增 `09_xianxia_sect_setting.md`，集中说明玩家身份、管理对象、人口池解释与术语映射；
  - 重写 `README / 01 / 03 / 04`，明确文档阅读顺序与天衍峰经营主线；
  - 补强 `02 / 05 / 08 / 06` 的前言和看板命名，使规则、看板、开发列表与宗门设定一致；
  - 新增 `FC-20260308-sect-management-docs-reframe.md` 与 `BL-20260308-sect-management-docs-reframe.md` 作为本轮归档。

### 3.17 DL-034 功能包详情（历史归档语义桥接 V1）

- 目标（玩家价值）：让玩家、策划与开发在打开旧功能卡、旧提案和旧平衡日志时，不必重新做“县城 / 郡图 / 郡县”到“宗门 / 外域”的脑内翻译。
- 飞轮环节：反哺宗门（降低历史知识检索成本，使旧归档能继续服务当前天衍峰经营背景）。
- 依赖（前置系统）：`docs/feature-cards/*.md`、`docs/change-proposals/*.md`、`docs/balance-logs/*.md`、`docs/06_archive_registry.md`、`docs/08_development_list.md`。
- 完成标准（DoD）：
  - 批量为旧归档正文补“历史兼容说明”；
  - 为高频历史链路补“当前对外语义”头注；
  - 至少覆盖天衍峰山门图、江陵府外域、宗门见闻、地图联动四类代表性旧归档；
  - `countytown-* / prefecture-*` 旧归档全文完成宗门化收口；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 本轮落地：
  - 为 `79` 份旧 `FC / CP / BL` 批量补齐“历史兼容说明”；
  - 为 `county-dynamic-events`、`countytown-25d-map`、`countytown-resident-walkers`、`prefecture-village-style-generator`、`map-operation-link` 等高频链路追加“当前对外语义”头注；
  - 将若干代表性旧归档的标题、目标、改动摘要改写为宗门 / 外域语义，但保留历史技术链路可追溯性；
  - 继续对 `41` 份 `countytown-* / prefecture-*` 目标归档执行全文清理，现已全部补齐“当前对外语义”头注，并完成正文语义收口；
  - 新增 `FC-20260308-archive-semantic-bridges.md` 与 `BL-20260308-archive-semantic-bridges.md` 作为本轮归档。

### 3.18 DL-035 功能包详情（天衍峰法旨兼容底层与双轨内务结算）

- 目标（玩家价值）：为宗主治理层提供一个稳定的兼容底层，让内外经济边界清晰，同时不破坏现有小时结算。
- 飞轮环节：职司分化 → 产业供养 → 传承研修 → 反哺宗门。
- 依赖（前置系统）：`GameState`、`GameLoop`、`EconomySystem`、`IndustrySystem`、`MapOperationalLinkSystem`、`Main.cs`、`PopupPanelBase`、Legacy `JobsPadding`。
- 完成标准（DoD）：
  - `TaskOrderUnits` 可稳定作为旧系统兼容折算层；
  - 任务系统会自动折算出四类职司投入，并在小时结算前后同步到现有系统；
  - 宗门内部建设、锻器、天衍峰山门图内务调度统一要求 `贡献点 + 灵石`；
  - 宗门外交易 / 外务仍只使用 `灵石`；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-08 本轮落地：
  - 新增 `SectTaskType / SectTaskRules / SectTaskSystem`，提供法旨定义、默认法旨推导、法旨 -> 职司同步；
  - `GameState` 新增 `ContributionPoints / TaskOrderUnits / TaskResolvedWorkers`，保持旧存档兼容；
  - `EconomySystem` 区分宗门内务与外事行商：内部任务生成贡献点，外域贸易只生成灵石；
  - `IndustrySystem` 与 `MapOperationalLinkSystem` 的宗门内务操作追加 `贡献点` 成本；
  - 新增 `TaskPanel.tscn` 与 `TaskPanel.cs`，为后续宗主治理层提供兼容入口；
  - `JobsPadding` 改为职司摘要，只展示法旨折算结果并导流到治理入口；
  - 新增 `FC-20260308-sect-task-orders-dual-currency.md`、`CP-20260308-sect-task-orders-dual-currency.md`、`BL-20260308-sect-task-orders-dual-currency.md`。

### 3.19 DL-036 功能包详情（浮云宗·天衍峰设定对齐）

- 目标（玩家价值）：让玩家读到和点到的，不再只是抽象“某个宗门”，而是有明确组织结构来源的 **浮云宗 / 青云峰三总殿 / 天衍峰阵堂**。
- 飞轮环节：反哺宗门（强化世界观锚点与管理对象可解释性，不改数值飞轮公式）。
- 依赖（前置系统）：`docs/09_xianxia_sect_setting.md`、`docs/01_game_design_guide.md`、`docs/02_system_specs.md`、`SectMapSemanticRules`、`SectTaskRules`、`TaskPanel`、`JobsPanel`。
- 完成标准（DoD）：
  - 核心文档明确写出“浮云宗（青云州江陵府）+ 天衍峰 + 青云峰三总殿”；
  - 任务法旨、建筑名、地图场所与任务弹窗按天衍峰语义更新；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-09 本轮落地：
  - `09 / 01 / 02 / 05 / 08 / README` 的世界观基线改成浮云宗天衍峰版本；
  - `SectMapSemanticRules` 统一切到 `阵材圃 / 傀儡工坊 / 传法院 / 青云总坊 / 庶务殿 / 江陵府外域`；
  - `SectTaskRules` 的法旨切到 `阵材采炼 / 阵枢营造 / 巡山警戒 / 阵法推演 / 总坊值守 / 外事行商`；
  - `TaskPanel`、`JobsPanel`、`WorldPanel` 的标题与入口改成天衍峰治理语义；
  - 新增 `FC-20260309-fuyun-sect-tianyan-setting-alignment.md` 与 `BL-20260309-fuyun-sect-tianyan-setting-alignment.md`。

### 3.20 DL-037 功能包详情（宗主治理中枢：一期方略层去人头化）

- 目标（玩家价值）：让玩家真正站在宗主视角，思考宗门发展方向、法令力度与任务重点，而不是亲自点派多少人干活。
- 飞轮环节：职司分化 → 产业供养 → 传承研修 → 反哺宗门。
- 依赖（前置系统）：`DL-035` 兼容底层、`TaskPanel`、`SectTaskRules`、`SectTaskSystem`、`Main.cs`、`JobsPadding`。
- 完成标准（DoD）：
  - 玩家主界面与中枢面板不再直接显示“请求人数 / 实派人数 / 空闲门人”；
  - 玩家只看到“治理条目 / 当前侧重 / 执事落实态势”；
  - `WorldPanel` 入口、`JobsPanel` 摘要、`TaskPanel` 标题与按钮统一改成“宗主中枢”语义；
  - 明确写出“宗主定调、执事自动落实”，避免继续出现执事层排班感；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-09 本轮落地：
  - `TaskPanel` 标题改为“浮云宗·宗主中枢”，按钮改为 `收敛 / 推进 / 鼎力推进 / 恢复均衡`；
  - `SectTaskRules` 的玩家可见摘要从“法旨道数 / 请求 / 实派”改为“治理侧重 / 落实态势”；
  - `Main.cs`、`WorldPanel.tscn`、`JobsPanel.tscn` 同步改成“宗主中枢 / 宗主治理摘要”；
  - `02 / 05 / 08` 同步回写，把当前系统定义为“宗主治理中枢一期”，并登记二期的门规 / 育才缺口；
  - 新增 `FC-20260309-sect-governance-strategy-layer.md`、`CP-20260309-sect-governance-strategy-layer.md`、`BL-20260309-sect-governance-strategy-layer.md`。

### 3.21 DL-040 功能包详情（宗门组织谱系详情浏览）

- 目标（玩家价值）：让玩家不只看到“九峰概览”，还能像浏览宗门谱系图一样切换查看每一峰的定位、核心机构与处室细节。
- 飞轮环节：反哺宗门。
- 依赖（前置系统）：`DL-039`、`docs/09_xianxia_sect_setting.md`、`D:/Files/Novel/asset/qidian/cangxuan/浮云宗-天衍峰.md`、`SectOrganizationRules.cs`、`Main.cs`、`JobsPanel.tscn`。
- 完成标准（DoD）：
  - `JobsList` 除九峰概览外，还能切换浏览峰脉详情；
  - 点击四条主职司时，会自动定位到推荐峰脉；
  - 峰脉详情能明确展示定位、职责、核心机构与处室 / 附属部门；
  - `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 与 FC / BL 已同步；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-09 本轮落地：
  - `JobsPanel.tscn` 新增“峰脉详情浏览”块，提供 `上一峰 / 下一峰` 切换、峰名、摘要与详情正文；
  - `Main.cs` 新增峰脉浏览状态，支持行点击自动跳到推荐峰脉，并与 `JobsList` 刷新联动；
  - `SectOrganizationRules.cs` 按《浮云宗-天衍峰》补充天枢峰、天机峰、天工峰、天权峰、天元峰、天衡峰的处室级说明；
  - 新增 `FC-20260309-sect-organization-peak-browser.md` 与 `BL-20260309-sect-organization-peak-browser.md`。
- 2026-03-09 迁移补强：
  - 为配合 `DL-045` 的主界面书卷化重构，峰脉详情浏览已从 `JobsPadding` 常驻块迁入独立 `SectOrganizationPanel`；
  - `BottomBar.tscn` 新增 `【峰令】谱系` 快捷按钮，`Main.cs` 负责弹窗创建、刷新与关闭解绑；
  - 独立卷册仍保留“四条职司 -> 推荐峰脉”的自动定位链路，不再与 `Tile Inspector` 混排。
  - 2026-03-09 家族统一补强：卷册外层补齐 `左右木轴 + 上下绫边 + 卷首横题`，标题正式收口为“峰令谱”，并把详情卡与治理按钮统一为方直墨线账册风。

### 3.22 DL-041 功能包详情（峰脉协同法旨）

- 目标（玩家价值）：让“九峰详情”不只是可读信息，而能成为宗主直接下发协同法旨、影响小时结算的真实治理入口。
- 飞轮环节：产业供养 → 传承研修 → 人口繁衍 → 反哺宗门。
- 依赖（前置系统）：`DL-040`、`SectOrganizationRules.cs`、`GameLoop.cs`、`EconomySystem.cs`、`PopulationSystem.cs`、`IndustrySystem.cs`。
- 完成标准（DoD）：
  - 峰脉详情区能直接设定“本季协同峰”与“恢复均衡”；
  - 协同峰效果会实际影响小时结算与工器锻制；
  - 职司摘要与峰脉详情能看见当前协同峰状态；
  - `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 与 FC / CP / BL 已同步；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-09 本轮落地：
  - 新增 `SectPeakSupportType / SectPeakSupportRules / SectPeakSupportSystem`，提供“诸峰均衡 + 九峰协同”的规则层；
  - `JobsPanel.tscn` 与 `Main.cs` 新增协同状态、下发按钮与恢复均衡按钮；
  - `EconomySystem`、`PopulationSystem`、`IndustrySystem` 接入协同峰加成；
  - `SectTaskRules` 的职司详情会同步展示当前峰脉协同；
  - 新增 `FC-20260309-sect-peak-support-directives.md`、`CP-20260309-sect-peak-support-directives.md`、`BL-20260309-sect-peak-support-directives.md`。
- 2026-03-09 迁移补强：
  - 协同峰入口已改由 `SectOrganizationPanel` 独立卷册承载，主界面左栏不再常驻显示协同法旨；
  - 卷册内提供“立协同峰令 / 复均衡轮转 / 转宗主中枢”三段治理动作，继续复用 `GameLoop.SetPeakSupport()` 与 `ResetPeakSupport()`；
  - 本轮仅迁移入口与视觉承载，不改协同峰的小时结算挂点与数值效果。
  - 2026-03-09 家族统一补强：协同峰动作区继续沿用原逻辑，但视觉已统一到“峰令谱”墨线批令样式，与治宗册、留影录、机宜卷的按钮语汇保持一致。

### 3.23 DL-042 功能包详情（季度法令）

- 目标（玩家价值）：让宗主能以“本季度专项法令”的形式，对宗门阶段性经营重心进行一次更强、更短周期的宏观推动。
- 飞轮环节：产业供养 → 传承研修 → 人口繁衍 → 反哺宗门。
- 依赖（前置系统）：`DL-038`、`SectGovernanceRules.cs`、`SectGovernanceSystem.cs`、`GameCalendarSystem.cs`、`TaskPanel.cs`。
- 完成标准（DoD）：
  - 宗主中枢新增“季度法令”一层，并可直接切换；
  - 法令效果会真实影响小时结算、人口链路或工器锻制；
  - 季度切换时，上季法令会自动过期并给出提示；
  - `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 与 FC / CP / BL 已同步；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-09 本轮落地：
  - 新增 `SectQuarterDecreeType` 与季度法令定义，补入 `SectGovernanceRules.cs`；
  - `TaskPanel.cs` 增加第四条“季度法令”治理层；
  - `EconomySystem`、`PopulationSystem`、`IndustrySystem` 接入季度法令加成；
  - `GameLoop.cs` 依据季度轮换自动清空过期法令；
  - 新增 `FC-20260309-sect-quarter-decrees.md`、`CP-20260309-sect-quarter-decrees.md`、`BL-20260309-sect-quarter-decrees.md`。

### 3.24 DL-043 功能包详情（宗门弟子独立属性界面）

- 目标（玩家价值）：让玩家能在独立界面里直接查看弟子名册与个体属性，不必只靠总人口、职司摘要和地图行走去猜测谁在承担什么职责。
- 飞轮环节：人口繁衍 → 职司分化 → 反哺宗门。
- 依赖（前置系统）：`GameState`、`PopulationRules.cs`、`SectGovernanceRules.cs`、`SectOrganizationRules.cs`、`DiscipleRosterSystem.cs`、`DisciplePanel.cs`、`Main.cs`、`WorldPanel.tscn`。
- 完成标准（DoD）：
  - 主地图页签行新增“弟子谱”独立入口与弹窗；
  - 弹窗支持按真传 / 职司筛选，并支持按修为 / 潜力 / 心境 / 贡献排序；
  - 详情能展示年龄、修为、当前差事、居所、关联峰脉、特征、培养建议与八项属性；
  - 详情区支持卷轴档案式布局，包含纵向名册、灵根圆环、根基罗盘、修为 / 战力 / 气海状态与批注栏；
  - 左侧名册需支持 `峰脉 -> 堂口 / 机构 -> 册级 -> 弟子` 的多层树状管理；
  - 天衍峰山门图点击弟子或场所时，能直接联动弟子谱并尽量定位对应弟子；
  - 名册基于 `GameState.Clone()` 派生生成，不直接改写小时结算与存档格式；
  - `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 与 FC / BL 已同步；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-09 本轮落地：
  - 新增 `DiscipleProfile` 与 `DiscipleRosterSystem`，基于人口、职司、真传、法令、育才方略与当前时间派生弟子名册；
  - 新增 `DisciplePanel.cs` 与 `DisciplePanel.tscn`，提供独立“弟子谱”弹窗、筛选、排序与详情展示；
  - `WorldPanel.tscn` 与 `Main.cs` 接入“弟子谱”按钮、弹窗创建、刷新与关闭链路；
  - `CountyTownMapViewSystem` 新增可视弟子 / 实体场所点击联动，能把地图中的代表弟子直接定位到“弟子谱”；
  - `docs/02 / 05 / 08` 同步补上弟子谱规格与状态看板；
  - 新增 `FC-20260309-disciple-attribute-panel.md` 与 `BL-20260309-disciple-attribute-panel.md`。
  - 2026-03-09 视觉补强（卷轴档案）：弟子谱整体改为“左侧纵向名册 + 右侧宣纸档案”布局，主体显示灵根圆环、根基罗盘、修为进度、战力印鉴、气海储备与衍天批注；
  - 新增 `FC-20260309-disciple-detail-card-refresh.md` 与 `BL-20260309-disciple-detail-card-refresh.md`。
  - 新增 `FC-20260309-disciple-scroll-dossier-refresh.md` 与 `BL-20260309-disciple-scroll-dossier-refresh.md`。
  - 2026-03-09 名册树补强：左侧卷册名册切为真正的多层树控件，按 `峰脉 -> 堂口 / 机构 -> 条线 / 班序 -> 册级 -> 弟子` 收口，方便在各峰内继续分层管理门人；
  - 2026-03-09 家族统一补强：弟子谱外层补齐 `左右木轴 + 上下绫边 + 卷首横题`，左栏目录语义收口为“峰内名录”，整体正式纳入统一卷轴子面板家族。
  - 2026-03-09 层级修正：`PopupPanelBase` 统一切到 `ZAsRelative = false + ZIndex = 200 + MoveToFront()`，修复主界面卷轴骨架与中央 hex 沙盘盖到“弟子谱”正文上的 Z 轴问题。
  - 新增 `FC-20260309-disciple-roster-tree.md` 与 `BL-20260309-disciple-roster-tree.md`。
  - 2026-03-09 固定宗门树补强：峰脉与堂口优先按固定宗门组织结构归类，不再主要依赖关联文本首段，并补入 `总枢亲传线 / 阵枢营造线 / 护山检修线 / 推演研修线 / 商路采办线` 等稳定条线；
  - 新增 `FC-20260309-disciple-fixed-organization-tree.md` 与 `BL-20260309-disciple-fixed-organization-tree.md`。

### 3.25 DL-044 功能包详情（门规树：一期三支门规纲目）

- 目标（玩家价值）：让宗主不只决定“方向”和“本季法令”，还能长期设定宗门常制，明确庶务、传功与巡山三条规则底盘。
- 飞轮环节：产业供养 → 传承研修 → 人口繁衍 → 反哺宗门。
- 依赖（前置系统）：`DL-038`、`TaskPanel.cs`、`SectRuleTreeRules.cs`、`EconomySystem.cs`、`PopulationSystem.cs`、`IndustrySystem.cs`。
- 完成标准（DoD）：
  - 宗主中枢新增 `庶务门规 / 传功门规 / 巡山门规` 三条支线；
  - 门规效果会真实影响收益、人口、威胁或工器锻制；
  - “恢复均衡”能把三支门规恢复到常制状态；
  - `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 与 FC / CP / BL 已同步；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-09 本轮落地：
  - 新增 `SectRuleTreeTypes / SectRuleTreeRules / SectRuleTreeSystem`；
  - `TaskPanel.cs` 追加三条门规支线并接入中枢交互；
  - `EconomySystem`、`PopulationSystem`、`IndustrySystem` 与 `SectTaskRules` 接入门规效果；
  - 新增 `FC-20260309-sect-rule-tree.md`、`CP-20260309-sect-rule-tree.md`、`BL-20260309-sect-rule-tree.md`。

### 3.26 DL-045 功能包详情（主界面六边形沙盘重构）

- 目标（玩家价值）：让玩家进入主界面后，第一眼看到的就是天衍峰六边形沙盘，并围绕“选中场所 -> 看状态 -> 打开治理动作”展开操作，而不是被厚重边框和大面板挤压地图空间。
- 飞轮环节：反哺宗门（本次只重构表现层与操作入口，不改小时结算公式、人口分配或存档格式）。
- 依赖（前置系统）：`DL-027`、`DL-028`、`DL-037`、`CountyIdle/scenes/Main.tscn`、`WorldPanel.tscn`、`JobsPanel.tscn`、`TopBar.tscn`、`BottomBar.tscn`、`EventLogPanel.tscn`、`CountyTownMapViewSystem.cs`。
- 完成标准（DoD）：
  - 主界面采用“中央沙盘 + 左侧检视 + 右侧日志 + 底部控制台”浮动布局；
  - 中央 `WorldPanel` 把地图页签收拢到底部悬浮控制台，地图区域明显放大；
  - 天衍峰山门图左键选中场所时，左侧检视卡同步展示场所名、状态、可视值守、路上门人和 Hex 坐标；
  - 宗主中枢、弟子谱、仓储、存档/读档、倍速入口继续可用；
  - `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 与 FC 已同步；
  - `dotnet build .\Finally.sln` 通过。
- 2026-03-09 本轮落地：
  - `Main.tscn` 改为中央地图优先的浮动式框景布局；
  - `WorldPanel.tscn` 把地图页签、缩放和弹窗入口收口到底部悬浮控制台，同时保留原有节点路径；
  - `JobsPanel.tscn` 新增“地块检视”卡，并通过 `TownMapSelectionSummary` 接收天衍峰场所选中摘要；
  - `TopBar.tscn`、`BottomBar.tscn`、`EventLogPanel.tscn` 切到更轻的琉璃面板风格；
  - 新增 `FC-20260309-main-hex-sandbox-layout.md`。
- 2026-03-09 二轮补强：
  - `Main.tscn` 继续压窄左右悬浮栏，给中央沙盘让出更多横向空间；
  - `WorldPanel.tscn` 将地图调度条从上沿收到底部页签上方，降低对地图首屏的遮挡；
  - `BottomBar.tscn` 进一步压缩按钮高度与整体宽度，保留控制台感但减少体积；
  - `Main.cs` 将世界图入口文案统一为 `世界大地图`。
- 2026-03-09 三轮补强：
  - `TopBar.tscn` 改为更薄的“顶部天幕”比例，压缩高度、边角与阴影，让顶部更像画框天幕而不是厚卡片；
  - `Main.tscn` 同步继续上移中央地图与左右浮动栏，进一步扩大首屏沙盘可视面积。
- 2026-03-09 四轮补强：
  - `BottomBar.tscn` 新增底部快捷动作组，将 `仓储 / 宗主中枢 / 弟子谱` 收到底部控制台；
  - `WorldPanel.tscn` 隐藏顶端同类按钮，只保留地图页签与缩放控件；
  - `MainWarehousePanel.cs`、`MainTaskPanel.cs`、`MainDisciplePanel.cs` 改为优先绑定底部快捷按钮，保持原路径为兜底兼容。
- 2026-03-09 五轮补强：
  - `JobsPanel.tscn` 将左侧检视器改成更明确的 `Tile Inspector` 层次：主标题、地块副标题、属性格、主次动作按钮与次级治理摘要；
  - `TownMapSelectionSummary` 与 `CountyTownMapViewSystem.Selection.cs` 的默认文案 / 选中文案改为更偏“地块检视卡”语气，弱化底层 `Lot / Road` 技术感。
- 2026-03-09 六轮补强：
  - `EventLogPanel.tscn` 将右侧日志区压成更轻更密的“微缩日志窗”：顶部仅保留两条高优先近讯，正文标题与留白同步收敛；
  - 保持 `PanelContent/MainVBox/LogBox/LogVBox/LogContent/LogLabel` 节点路径不变，继续兼容 `Main.cs` 的日志绑定。
- 2026-03-09 七轮补强：
  - 新增 `SectChronicleRules.cs` 与 `MainSectChroniclePanel.cs`，将右侧两条风闻卡改为读取当前经营状态实时生成；
  - 风闻优先反映 `威胁 / 仓储负载 / 民生覆盖 / 探险 / 法令 / 协同峰` 等摘要，但不介入小时结算与事件日志写入链。
- 2026-03-09 八轮补强：
  - `MainSectTileInspector.cs` 将左侧检视卡动作区改为真正的“tile 专属操作项”：未选中前禁用，选中后按地块类型切换为扩建 / 调度 / 打开对应面板；
  - `TownMapSelectionSummary.CreateDefault()` 默认文案改为更明确的“点选中央 hex tile 后显示具体信息与可操作项”提示。
- 2026-03-09 九轮补强：
  - `JobsPanel.tscn` 在左侧检视卡补入 `tile 类型徽记` 与 `可执行项摘要`，让卡面更接近参考示例图的“被选中地块专属信息卡”；
  - `MainSectTileInspector.cs` 根据 tile 类型切换徽记文案、强调色与状态色，进一步拉开灵田 / 工坊 / 总坊 / 传法院 / 庶务殿 / 休憩点的视觉差异。
- 2026-03-09 十轮补强：
  - 新增 `TownActivityAnchorVisualRules.cs` 统一维护 tile 类型配色，让左侧检视卡与中央沙盘高亮使用同一套强调色；
  - `CountyTownMapViewSystem.Anchors.cs` 为选中 tile 增加双层 halo、描边与入口路径高亮，强化“选中地块”的中央反馈。
- 2026-03-09 十一轮补强：
  - `Main.tscn` 改为“案几 + 宣纸 + 左右画轴”的卷轴骨架，并用上下横线与左右界栏重组 Legacy 主界面视觉框架；
  - `TopBar.tscn`、`BottomBar.tscn`、`JobsPanel.tscn`、`EventLogPanel.tscn`、`WorldPanel.tscn` 与 `Main.cs` 文案统一切到卷轴式墨书风格，进一步对齐参考图的书卷 UI。
- 2026-03-09 十二轮补强：
  - 按参考 HTML / 卷轴图继续收敛 Legacy 主界面布局，重点将卷面骨架、左右栏占比、顶部题头、底部控制台与地图控件排布进一步贴近一比一参考效果；
  - 本轮只处理表现层与场景布局，不改 `GameLoop`、`GameState`、存档结构与小时结算；
  - 保留 `TownMapSelectionSummary -> MainSectTileInspector` 数据通路，以及中央 tile 高亮与左栏状态 / 动作同步刷新链路。
- 2026-03-09 十三轮补强：
  - 清理左右卷栏残留黑底：左侧动作按钮、下方岗位卡与右侧日志正文统一改回宣纸底色与墨字配色；
  - 为左栏摘要与右栏日志的 `RichTextLabel` 增加空样式覆盖，避免主题默认底色再次在卷面内出现黑块；
  - 本轮仅修正表现层配色，不改 `TownMapSelectionSummary -> MainSectTileInspector` 联动链路，也不改小时结算、存档结构与玩法数据。
- 2026-03-09 十四轮补强：
  - 根据最新截图继续清理左栏残留深色块，补齐三个 tile 操作按钮的 `disabled` 样式与禁用文字颜色；
  - 未选中 tile 时，按钮仍保持禁用逻辑，但视觉上统一为卷轴纸面淡墨态，不再回退到 Godot 默认黑底按钮；
  - 本轮仍只处理表现层，不改玩法数据、tile 选中联动和存档结构。
- 2026-03-09 十五轮补强：
  - 从主界面左栏移除 `JobsPadding` 常驻区，左侧彻底收口为 `Tile Inspector + 批注` 结构，不再混放职司摘要、峰脉详批与协同峰令；
  - `Main.cs` 不再在主界面绑定 `JobsPadding` 节点，避免场景残留节点继续参与刷新与事件注册；
  - `SectOrganizationRules / SectPeakSupportRules / GameLoop.SetPeakSupport()` 等底层规则暂保留，后续再迁入独立治理面板或 `宗主中枢` 子页。
- 2026-03-09 十六轮补强：
  - `BottomBar.tscn` 新增底部 `【峰令】谱系` 快捷按钮，组织谱系与协同峰功能改由独立 `SectOrganizationPanel` 弹窗承载；
  - `Main.cs` 在 `_Ready / OnStateChanged / _ExitTree` 中补齐该弹窗的创建、刷新与解绑，保持与仓储 / 中枢 / 弟子谱弹窗一致的生命周期；
  - 主界面继续保持“tile 检视优先”，组织治理退为二级入口，但不删除峰脉规则与协同峰结算。
- 2026-03-09 十七轮补强：
  - `EventLogPanel.tscn` 右栏题头正式切到 `山门近闻 / 天衍峰札记` 语义，并补细金边与纸面札记感；
  - 保持 `AlertItem1 / AlertItem2 / LogLabel` 现有节点路径不变，继续兼容 `MainSectChroniclePanel.cs` 与 `Main.cs` 的绑定；
  - 本轮只改右栏表现层和示例文案，不改风闻生成与小时结算日志链路。
- 2026-03-09 十八轮补强：
  - `BottomBar.tscn` 与 `WorldPanel.tscn` 的底栏控制条、地图页签和缩放按钮补齐 `normal / hover / pressed` 三态，卷尾法令与卷中页签不再停留在“同一描边”状态；
  - `JobsPanel.tscn` 与 `MainSectTileInspector.cs` 统一去除左侧动作按钮中的 emoji，改成 `扩建阵材圃 / 调度堂口法旨 / 查阅门人谱` 等墨线批令语义；
  - `WorldPanel.tscn` 的备用报表卡、状态警示卡与新建卡底色统一回宣纸系，为后续恢复这些备用页签预留一致风格。
- 2026-03-10 十九轮补强：
  - `Main.tscn` 根背景改为 `CountyIdle/assets/ui/background/background_1.png`，统一主界面卷轴山水底图；
  - 旧 `PaperSheet / LeftRoller / RightRoller / HeaderRule / FooterRule / LeftGuide / RightGuide` 装饰底稿默认停用，避免与新背景卷轴边框重复叠加；
  - 本轮仅处理表现层，不改 `GameLoop`、`GameState`、地图页签、tile 检视联动与存档结构；新增 `FC-20260310-main-background-image.md`。
- 2026-03-10 二十轮补强：
  - `Main.cs` 为根背景节点补齐显式窗口缩放适配：监听主界面与 viewport 尺寸变化，实时重排背景 `TextureRect`；
  - `background_1.png` 统一按 cover 方式铺满整个游戏窗口，窗口放大/缩小时始终保持全屏填充；
  - 本轮仅强化表现层适配，不改 `GameLoop`、`GameState`、地图联动与存档结构。
- 2026-03-10 二十一轮补强：
  - `Main.cs` 将 `background_1.png` 改为先裁切卷轴中间画幅，再作为根背景源图使用，避免把卷轴木轴与外围留白一起拉进游戏背景；
  - 裁切后的画幅继续按 cover 方式铺满整个游戏窗口，窗口变化时保持居中与全屏填充；
  - 继续微调裁切框，使背景更贴近参考图中的内框长画幅构图；本轮仅调整表现层背景取样，不改 `GameLoop`、`GameState`、地图联动与存档结构。
- 2026-03-10 二十二轮补强：
  - 根背景资源正式切换为 `CountyIdle/assets/ui/background/background_2.png`；
  - 由于 `background_2.png` 本身已经对齐目标画幅构图，`Main.cs` 取消额外裁切，直接整图作为背景源；
  - 背景继续监听窗口与 viewport 尺寸变化，并以 cover 方式持续铺满整个游戏窗口；本轮不改 `GameLoop`、`GameState`、地图联动与存档结构。
- 2026-03-10 二十三轮补强：
  - 为 `Main.tscn` 根背景节点新增 `background_blur.gdshader`，对 `background_2.png` 应用高斯模糊；
  - 模糊仅作用于背景 `TextureRect`，不影响前景 UI、地图交互与弹窗层级；
  - 本轮仅处理表现层材质，不改 `GameLoop`、`GameState`、地图联动与存档结构。
- 2026-03-10 二十四轮补强：
  - 按最新表现要求，将根背景材质从高斯模糊改为 `background_frosted_glass.gdshader` 毛玻璃效果；
  - 毛玻璃 shader 通过轻微散射采样、乳白 tint、对比度压低与细颗粒噪声，让背景保持卷轴气质但不过分抢前景；
  - 效果仅作用于背景 `TextureRect`，不影响前景 UI、地图交互与弹窗层级；本轮不改 `GameLoop`、`GameState`、地图联动与存档结构。

## 4) 执行与回写规则

每完成一条开发项，必须同步更新：

1. `docs/05_feature_inventory.md`（状态变更）
2. `docs/02_system_specs.md`（新增或变更规则）
3. 对应 FC/CP/BL 归档（如涉及机制/平衡）
4. 本文状态列（`TODO -> DONE`）




