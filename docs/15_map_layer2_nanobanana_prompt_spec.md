# CountyIdle L2 道路河流 Nano Banana 提示词规范

> 用途：从现有地图文档中提取可执行的绘制规则，为 Nano Banana 生成 L2 独立道路与独立河流素材提示词。

## 1) 编号说明

当前仓库对地图层有两套编号：

- 在 [`docs/11_map_asset_production_spec.md`](D:\Files\CountyIdle\docs\11_map_asset_production_spec.md) 中，`Layer 2` 指 `道路 / 河流 / 法阵图案` 的地表装饰资产。
- 在 [`docs/14_map_layer_rendering_implementation_plan.md`](D:\Files\CountyIdle\docs\14_map_layer_rendering_implementation_plan.md) 中，运行时编号 `L3` 对应上面的 `Layer 2`。
- 本轮口径已明确：道路与河流不需要锚定 hex 框架，也不需要输出 `4x2` connector sheet，因此本文统一按“独立道路 / 独立河流”编写提示词。

## 2) 从文档提取出的绘制规则

### 2.1 视觉总基调

- 风格：`青绿山水 + 工笔重彩 + 羊皮古卷`
- 质感：毛笔勾线、宣纸晕染、做旧水渍、纸面纤维
- 避免：现代 UI 图块感、高饱和西幻霓虹、机械硬裁边、现代柏油路
- 色彩：使用中国传统色系，保持矿物感、纸面感、低霓虹

### 2.2 视角与构图

- 视角为 `2.5D 固定等距视角`
- 道路与河流不需要显式画出 hex 轮廓
- 资产应以单条卷轴笔触为主体组织，而不是切成网格小片
- 图形可以自然弯折、分叉或延展，但不能变成完整场景插画

### 2.3 L2 的职责边界

- 只画地表装饰关系与路径引导
- 本文只覆盖：道路、河流
- 不包含：高物件、建筑、山体、树木、云雾、角色
- 不要求 tile 化，不要求按六方向逻辑拼接

### 2.4 资源逻辑

- 每次生成一张独立资产
- 道路输出为单条古道笔触
- 河流输出为单条水道笔触
- 如需变化，优先通过宽窄、弯曲幅度、边缘毛刷感来做变体，而不是拆成 connector 状态

### 2.5 各类型专项要求

- 道路：保留笔刷和砂土肌理，像画在纸面上的古道，不像现代铺装路
- 河流：强调自然流向与水纹晕染，不能画成 UI 蓝色发光管线

## 3) 单张图规则

单张图一次只生成一个对象，要么是一条道路，要么是一条河流。

补充约束：

- 画面主体保持居中，四周保留足够透明或浅纸边，便于后续裁切和摆放。
- 背景只保留轻微纸面感，不要把整张图画成完整地图场景。
- 可以有自然弯曲，但不要复杂到无法复用。

## 4) Nano Banana 提示词写法建议

### 4.1 通用结构

建议每次都明确写出以下结构：

1. 资产用途：`single decorative road stroke` 或 `single decorative river stroke`
2. 图像布局：`single object on transparent or parchment-like background`
3. 风格锚点：`qinglu landscape, gongbi heavy color, ancient parchment scroll`
4. 逻辑约束：`not tied to hex grid, reusable, clean silhouette`
5. 排除项：`no buildings, no characters, no UI frame, no modern fantasy neon`

### 4.2 通用负向约束

可复用以下负向描述：

```text
no buildings, no mountains as main subject, no trees as main subject, no characters, no labels, no text, no interface frame, no icon sheet border, no western fantasy neon glow, no sci-fi circuit, no asphalt road, no photorealism, no hard cut edges, no modern game UI, no hex grid
```

## 5) 可直接使用的 Nano Banana 提示词

### 5.1 道路版

```text
Create a single decorative road asset for a Chinese xianxia map. One image only, showing one standalone ancient road stroke on a light parchment-like or transparent background, centered and easy to cut out. Style is qinglu landscape, gongbi heavy-color painting, hand-brushed linework, mineral pigments, rice-paper staining, ancient scroll texture. The road should look like an old cultivated path with dry earth, stone dust, subtle brush texture, softened ink edges, and a painted-on-paper feeling instead of a pasted UI decal. The shape can be gently curved or slightly organic, but it must remain reusable as one road segment, not a full scene illustration. No buildings, no characters, no mountains as the subject, no interface frame, no hex grid, no modern asphalt, no photorealism.
```

### 5.2 河流版

```text
Create a single decorative river asset for a Chinese xianxia map. One image only, showing one standalone river stroke on a light parchment-like or transparent background, centered and easy to cut out. Style is qinglu landscape, gongbi heavy-color painting, hand-painted ink contour, azurite and cyan-blue mineral colors, xuan paper diffusion, old scroll texture. The river should feel naturally flowing, with soft ripples, mineral-pigment gradients, painted water edges, and a scroll-map feeling instead of a UI effect. The shape can be gently curved and organic, but it must remain reusable as one river segment, not a complete landscape illustration. No bridges, no boats, no buildings, no characters, no interface frame, no hex grid, no glossy modern water, no photorealism.
```

## 6) 推荐使用方式

- 如果先做最通用的一张，优先出 `道路版`
- 第二张再出 `河流版`
- 如果 Nano Banana 容易把它画成插画而不是资产图，可在提示词末尾追加：

```text
game-ready decorative asset, isolated subject, reusable single stroke, not a full illustration scene
```

## 7) 接图时的检查项

- 是否确实只画了一条道路或一条河流
- 是否没有混入建筑、树、人物等 L3/L4 内容
- 是否保留卷轴国风纸面感，而不是现代平面 UI 贴纸感
- 是否便于后续裁切、旋转、拉伸或自由摆放
- 是否没有显式 hex 框架
