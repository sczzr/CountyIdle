# CountyIdle 地图 L1-L5 绘制实施方案（v0.1）

> 用途：将地图素材规范进一步落成“可执行的 Godot 绘制方案”，明确 L1-L5 每层画什么、在哪里画、如何接资源、如何排序、如何验证，作为 DL-049 后续表现层推进的实施文档。

## 1) 文档定位

- `docs/11_map_asset_production_spec.md` 负责回答“地图应该长成什么样、素材怎么组织”。
- 本文负责回答“这些层在当前项目里具体怎么画、由谁画、接到哪里、按什么顺序推进”。
- 若二者冲突：
  - 视觉目标以 `11` 为准；
  - 运行时落点、节点归属、绘制顺序以本文为准；
  - 若当前代码与本文冲突，应以“当前实现 + 已验证行为”为准，并回写本文。

## 2) 编号映射（强制）

为避免“Layer 0~4”与“L1~L5”并存造成歧义，后续统一按以下映射理解：

| 本文编号 | 既有规范编号 | 层名 | 主要作用 |
| --- | --- | --- | --- |
| `L1` | `Layer 0` | 卷轴底图层 | 全屏纸面与做旧气氛 |
| `L2` | `Layer 1` | 基础地块层 | hex 内地貌底盘 |
| `L3` | `Layer 2` | 地表装饰层 | 道路 / 河流 / 法阵 / 地表纹路 |
| `L4` | `Layer 3` | 立体物件层 | 山、建筑、树、奇观、场所 |
| `L5` | `Layer 4` | 氛围特效层 | 云雾、飞鸟、雷劫、边界迷雾 |

注意：

- `选中高亮 / 检视指引 / 调试标记` 不属于 `L1 ~ L5`，统一视为“交互覆盖层”，必须压在最上面。
- `L1` 是 UI 根背景，不参与地图局部 Y 排序。

## 3) 当前实现快照

截至当前版本：

- `L1`：已由主界面卷轴背景承担，属于 `Main.tscn` / 根 UI 表现层。
- `L2`：已改为由 `CountyTownMapViewSystem` 直接读取 `L1_hex_tileset.tres` 的 atlas source / region，并继续按现有 hex polygon 几何绘制；世界图当前也继续复用同一套 tileset 资源，但正式主链已回收到 `StrategicMapViewSystem._Draw()` 按 hex polygon 逐格投 atlas 区域，因为此前直接走 `WorldTerrainTileLayer` 的方片排布会在六边形裁切后留下连续白缝；旧 atlas manifest/自绘 atlas 仍保留为过渡 fallback，但不再是主链；旧版蜂窝背景网格也不再叠加。
- `L3`：已接入 `Road / Courtyard / Water` 的 decal 与连接纹理，但道路 / 河流的生产方向已切到“独立笔触装饰层”，旧连接纹理仅保留为过渡兼容。
- `L4`：已接入最小运行时闭环，`TownMapGeneratorSystem` 可生成基础 `Building / ActivityAnchor`，`CountyTownMapViewSystem.DrawStructures()` 已进入主绘制顺序。
- `L5`：尚未正式接入，仍缺专门的数据源、绘制入口与低缩放降噪策略。

## 4) 总体绘制架构

### 4.1 节点归属

推荐维持当前“主背景 + 地图视图”两段式：

- UI 根层：
  - `Main.tscn`
  - 根背景 `TextureRect` / 背景 shader
  - 承担 `L1`
- 地图绘制层：
  - `CountyTownMapViewSystem : PanelContainer`
  - 使用 `_Draw()` 承接 `L2 ~ L5` 与交互覆盖层
  - 保留当前缩放、平移、点选、检视链路
- 世界图底盘层：
- `StrategicMapViewSystem : PanelContainer`
- 正式主链：`_Draw()` 直接把 `L1_hex_tileset.tres` 的 atlas region 投到真实 hex polygon
- 备用基础设施：`WorldTerrainTileLayer : TileMapLayer` 目前保留但不承担正式底盘
- roads / rivers / labels / site markers 继续留在 `_Draw()` 叠加层
- 历史蜂窝底网不再绘制

### 4.2 `_Draw()` 的目标调用顺序

推荐固定为：

1. `DrawTerrain()` -> `L2`
2. `DrawTerrainSemanticOverlay()` -> `L3`
3. `DrawStructures()` -> `L4`
4. `DrawAtmospherics()` -> `L5`
5. `DrawSelectedCellOverlay()` -> 交互覆盖层
6. `DrawResidents()` -> 如未来恢复弟子可视移动，需与 `L4` 协调排序

补充裁定：

- `DrawTerrain()` 当前直接承担 L2 底盘绘制，但纹理来源必须优先读取 `L1_hex_tileset.tres`；
- L2 底盘继续沿用现有 hex polygon 几何，保持与点击、建筑、选中高亮共用同一格心；
- 基础地块默认不画边沿线，视觉目标以“无缝贴近”优先。

约束：

- `L4 / L5` 不能压过选中高亮。
- `L5` 只能柔化和增强气氛，不能影响点击归属。

### 4.3 数据职责

| 层 | 数据来源 | 当前主控系统 |
| --- | --- | --- |
| `L1` | UI 背景资源、卷轴 shader 参数 | `Main.cs` / `Main.tscn` |
| `L2` | `TownMapData.GetTerrain()` + `L1_hex_tileset.tres` | `TownMapGeneratorSystem` + `CountyTownMapViewSystem` |
| `L3` | terrain 邻接、连接 mask、法阵/水网语义 | `TownMapGeneratorSystem` + `CountyTownMapViewSystem` |
| `L4` | `TownMapData.Buildings`、`TownMapData.ActivityAnchors` | `TownMapGeneratorSystem` |
| `L5` | 区域标签、灵气/威胁/节气状态、边界位置 | 后续 `TownMapGeneratorSystem` / 运行时状态映射 |

## 5) L1 卷轴底图层实施方案

### 5.1 绘制内容

- 卷轴纸面
- 做旧纹理
- 轻微水渍
- 极弱的纸面纤维与模糊山水底稿

### 5.2 Godot 接法

- 节点归属：`Main.tscn` 根背景 `TextureRect`
- 资源形式：
  - 背景主图：`assets/ui/background/*.png`
  - 背景 shader：如现有 frosted glass / 轻颗粒噪声方案
- 尺寸策略：
  - 使用 cover 铺满
  - 由窗口大小变化时自动重排

### 5.3 运行规则

- 不跟随地图缩放和平移
- 不参与地图点击
- 不参与 Y 排序
- 对比度必须低于地图主体

### 5.4 完成标准

- 窗口尺寸变化时背景不拉伸变形
- 不干扰 `WorldPanel` 与左 / 右栏可读性
- 作为卷轴基底长期稳定存在，不因地图切换闪烁

## 6) L2 基础地块层实施方案

### 6.1 绘制内容

- 平原 / 灵脉 / 山麓 / 近岸 / 深水等基础底盘
- 必须限制在 hex 内
- 只负责“这格是什么地貌”，不负责连接关系

### 6.2 Godot 接法

- 节点归属：宗门图 / 局部沙盘走 `CountyTownMapViewSystem._Draw()`；世界图正式主链走 `StrategicMapViewSystem._Draw()` 的 hex polygon 投影，`WorldTerrainTileLayer` 当前仅作备用基础设施
- 主入口：`DrawTerrainBaseLayer()`
- 数据来源：`TownMapData.GetTerrain()`
- 资源形式：
  - `L1_hex_tileset.tres`
  - tileset 引用的 terrain atlas

### 6.3 当前建议

- 继续用 tileset 作为运行时唯一底盘资源入口，不再把 atlas 图片路径当主链
- 每种地貌支持 `2~6` 个变体
- 通过 `GetCellHash()` 做轻量随机轮换，降低平铺重复感
- 若 atlas 为方形切片，必须按 hex polygon 直接投影到六边形底盘，而不是把原始矩形 tile 直接摆上屏

### 6.4 后续扩展字段

建议未来补：

- `TerrainVariantGroup`
- `BiomeTag`
- `QiDensityBand`
- `HeightBand`
- `CorruptionBand`

当前已用于让世界图与宗门图共享同一套地块变体规则；后续只需继续细化世界图专属变体、专用 tileset 和 overlay 分层。

## 7) L3 地表装饰层实施方案

### 7.1 绘制内容

- 道路
- 河流 / 水道
- 法阵纹
- 院坪、驿道、灵脉裂隙等地表图案

### 7.2 Godot 接法

- 节点归属：`CountyTownMapViewSystem._Draw()`
- 主入口：`DrawTerrainSemanticOverlay()`
- 运行组成：
  - 独立道路 / 河流笔触资源
  - 必要时的法阵图案或院坪 decal
  - 必要时的补线 / 水纹 / 亮边强调

### 7.3 数据来源

- terrain 本体来自 `TownMapData`
- 道路 / 河流的摆放来源可以是预设路径、控制点、样条或后续人工布局数据
- 后续法阵建议从独立 `mask` 或 `tag` 读取，不直接塞进 `TownTerrainType`

### 7.4 实施规则

- 道路 / 河流默认不再要求六方向连接
- 当前运行时若仍在使用连接纹理，视为过渡兼容方案，不作为后续美术生产口径
- 后续正式版建议升级为：
  - `独立笔触资源 + 路径控制点 / 样条`
  - 法阵等规则图案可单独维护模块化样式集

### 7.5 与 L2 的边界

- `L2` 回答“这块地是什么”
- `L3` 回答“这片区域表面被画了什么、路径如何被视觉引导”

### 7.6 完成标准

- 道路 / 河流笔触自然，不出现被 hex 裁碎的机械断头
- 低缩放下仍能读出主通路
- 不遮挡格位判断和选中高亮

## 8) L4 立体物件层实施方案

### 8.1 绘制内容

- 山门建筑
- 居舍
- 工坊
- 传法院
- 庶务殿
- 总坊 / 坊市
- 巡山岗
- 后续山体、树木、奇观

### 8.2 Godot 接法

- 节点归属：`CountyTownMapViewSystem._Draw()`
- 主入口：`DrawStructures()`
- 数据来源：
  - `TownMapData.Buildings`
  - `TownMapData.ActivityAnchors`

### 8.3 排序规则

- 使用“按 `Y` 值排序后绘制”的 2D 伪立体方案
- 同 Y 时再按 `X` 与优先级排序
- 同格内：
  - 先建筑底盘
  - 再主体
  - 再屋檐 / 旗幡 / 点缀

### 8.4 破框规则

- 允许顶部越出本格上缘
- 不允许越界后改变逻辑归属
- 建筑点击归属仍按其 `lotCell`

### 8.5 近期实施顺序

先做：

- 宗门主殿类
- 居舍类
- 工坊类
- 传法院 / 治务类
- 巡山岗 / 休憩台类

再做：

- 山体
- 竹林
- 桃林
- 古树
- 奇观

### 8.6 与弟子可视移动的关系

- 未来若恢复 `DrawResidents()`：
  - 弟子应在道路和院坪上活动
  - 默认位于 `L4` 建筑主体之前或之后的独立排序段
  - 不能简单粗暴画在最上层，否则会破坏建筑遮挡关系

## 9) L5 氛围特效层实施方案

### 9.1 绘制内容

- 祥云
- 白雾
- 山间流云
- 雷劫闪光
- 飞鸟 / 鹤群
- 边界迷雾
- 局部天象法阵

### 9.2 Godot 接法

- 节点归属：仍建议由 `CountyTownMapViewSystem` 统一 `_Draw()` 驱动
- 主入口建议新增：`DrawAtmospherics()`
- 数据来源：
  - 地块分区
  - 灵气密度
  - 威胁状态
  - 节气 / 天候
  - 地图边缘与高差区域

### 9.3 实现方式建议

- 首版优先用低频缓动：
  - 正弦偏移
  - 透明度脉动
  - 缓速横移
- 不建议一开始就上粒子系统大铺量
- 边界迷雾优先做静态或半静态大块，降低 CPU / Draw 开销

### 9.4 降噪规则

- 缩放小于阈值时，自动减弱或隐藏小型氛围元素
- 只保留：
  - 边界迷雾
  - 关键仙山云海
  - 极少量高识别度天象

### 9.5 完成标准

- 有气氛，但不影响点击
- 不把地图糊成一片
- 不遮挡左栏检视链路的视觉反馈

## 10) 资源与目录实施建议

### 10.1 目录分工

```text
CountyIdle/assets/map/
  scroll/          # L1
  terrain/         # L2
  connectors/      # L3
    roads/
    rivers/
    arrays/
  props/           # L4
    buildings/
    mountains/
    vegetation/
    wonders/
  atmospherics/    # L5
  manifests/
```

### 10.2 manifest 责任

- `L2` atlas 必须有 manifest
- `L3` 正式连接 atlas 也应有 manifest
- `L4` 若走独立大图，不强制 atlas，但必须有锚点说明
- `L5` 若走散资源，至少要有清单文档记录缩放策略和推荐覆盖区域

## 11) 分阶段实施顺序

### 11.1 已完成

- `L2`：atlas manifest 接入
- `L3`：decal + 连接纹理接入
- `L4`：最小运行时闭环接入

### 11.2 下一阶段

1. 将 `L4` 从 runtime 体块升级为正式国风建筑 / 山体 / 植被贴图
2. 为 `L5` 建立独立绘制入口 `DrawAtmospherics()`
3. 将 `L3` 从“连接纹理 + 逻辑补线”升级到正式连接 atlas
4. 在世界图已接入底盘复用后，继续补二级地图与世界图高层 overlay 的统一分层口径

## 12) 每层的验收口径

| 层 | 最低验收 |
| --- | --- |
| `L1` | 背景稳、卷轴感足、不抢 UI |
| `L2` | 地貌可读、平铺不太重复 |
| `L3` | 连接连续、主通路清晰 |
| `L4` | 有空间感、Y 排序正确、破框不误导点击 |
| `L5` | 有氛围、不糊图、不挡交互 |

## 13) 禁止事项

- 不要把 `L1` 也塞进地图局部 `_Draw()`
- 不要把 `L5` 做成遮住选中高亮的大雾墙
- 不要让 `L4` 的点击归属跟视觉破框一起漂移
- 不要把法阵、道路、水路全部混进一个 `TownTerrainType`
- 不要为了“像画”牺牲掉 hex 可读性

## 14) 结论

后续地图表现层应统一按以下执行：

- `L1` 在 UI 根层画
- 宗门图 / 局部沙盘的 `L2 ~ L5` 在 `CountyTownMapViewSystem` 里分阶段画；世界图当前由 `StrategicMapViewSystem._Draw()` 直接承接 `L2` 的 hex polygon 投影，其余 overlay 同样继续留在 `StrategicMapViewSystem._Draw()`
- `L2` 优先走 atlas / manifest；`L3` 的道路 / 河流优先走独立笔触资源与自由摆放
- `L4` 走锚点 + Y 排序 + 破框
- `L5` 走低频缓动 + 低缩放降噪

这份文档的作用不是定义最终审美，而是让后续每推进一层时，大家都知道“该往哪接、怎么验收、不能踩哪些坑”。
