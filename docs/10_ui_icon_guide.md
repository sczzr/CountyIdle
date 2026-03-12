# CountyIdle 交互 Icon 规范（v0.1）

> 本文是 CountyIdle 的交互 icon 正式规范。  
> 它只回答三件事：
>
> 1. 当前 UI 应采用什么 icon 语义与视觉边界
> 2. icon 在当前正式入口、备用入口与通用操作中如何落位
> 3. 资源命名、图集组织与 Godot 接入应如何统一
>
> 主设计语义以 [01_game_design_guide.md](/E:/2_Personal/Finally/docs/01_game_design_guide.md) 为准。  
> 系统入口与当前运行边界以 [02_system_specs.md](/E:/2_Personal/Finally/docs/02_system_specs.md) 和 [08_development_list.md](/E:/2_Personal/Finally/docs/08_development_list.md) 为准。  
> 世界观与术语门禁以 [09_xianxia_sect_setting.md](/E:/2_Personal/Finally/docs/09_xianxia_sect_setting.md) 为准。

## 0. 使用裁定

- 本文只规范交互 icon 的语义、样式、资源组织与接入方式，不裁定具体玩法规则。
- 若按钮节点名仍保留 `County / Prefecture` 等历史技术命名，应按当前宗门语义理解，不得反向带偏玩家可见文案。
- 本文中的入口必须区分“当前正式入口”“隐藏备用入口”“通用快捷入口”；不得混写为同一优先级。
- 若 icon 与当前正式入口结构冲突，以运行版实际入口和 `docs/08_development_list.md` 的当前裁定为准。

## 1. 适用范围

- 底部控制台入口（库房 / 中枢 / 谱系 / 弟子 / 历练）
- 当前正式地图入口与地图工具按钮
- 隐藏备用入口与历史面板入口
- 常用面板入口与通用操作（设置 / 存档 / 关闭 / 返回 / 确认 / 取消）
- 状态提示类小图标（警示 / 提示 / 成功）

## 2. 视觉语言

- 语义基调：书卷、印章、木轴、墨线、山水、朱印
- 线条风格：单线为主，局部实心点缀；线宽统一，避免过细
- 形状原则：几何化与纹样化并存，但不过度装饰
- 避免事项：现代科技感扁平符号、表情化符号、细节过多

## 3. 尺寸与对齐

- 基准网格：`24x24`
- 底栏按钮：容器 `28x28`，绘制区约 `20x20`
- 页签/工具：`18~20` 视觉宽高
- 线宽建议：`1.5px`（24 基准）或 `2px`（28 基准）
- 对齐原则：像素对齐；圆角建议 `2px` 内

## 4. 状态与颜色语义

- 常态：墨色线条
- 悬停：墨色加深，局部朱印点缀
- 按下：靛青强调或压暗描边
- 激活：靛青主色 + 朱印辅助
- 禁用：淡墨低对比，降低不透明度
- 危险/警示：朱砂强调

## 5. 交互 Icon 清单

### 5.1 底部控制台入口

| 入口 | 建议图形 | 说明 |
| --- | --- | --- |
| 库房 | 仓印 / 木箱 | 对应“库藏大卷”与库存管理 |
| 中枢 | 令牌 / 印章 / 卷轴 | 对应“治宗册”宗主中枢 |
| 谱系 | 山峰 + 谱牒 | 对应“峰令谱”组织卷册 |
| 弟子 | 竹简名册 | 对应“弟子谱” |
| 历练 | 剑 / 行囊 | 对应历练入口 |

### 5.2 当前正式地图入口（WorldPanel）

| 入口 | 按钮节点 | 建议图形 | 备注 |
| --- | --- | --- | --- |
| 天衍峰山门图 | `CountyTownMapButton` | 山门 / 峰影 | 主视图入口 |
| 世界地图 | `WorldMapButton` | 罗盘 / 山海卷 | 世界总览 |

### 5.3 隐藏备用入口与历史节点名

| 入口 | 按钮节点 | 建议图形 | 当前裁定 |
| --- | --- | --- | --- |
| 江陵府外域图 | `PrefectureMapButton` | 城郭 / 府印 | 隐藏备用外域视图，不属于当前主地图正式页签 |
| 宗门见闻 | `EventPanelButton` | 铃 / 札 | 历史/备用面板入口，后续恢复时沿用本规范 |
| 宗务报表 | `ReportPanelButton` | 卷册 / 表格 | 历史/备用面板入口，后续恢复时沿用本规范 |
| 外域历练 | `ExpeditionMapButton` | 路线 / 行囊 | 独立历练入口语义预留，不代表当前主地图页签常驻入口 |

### 5.4 常用快捷入口（非地图页签）

| 入口 | 按钮节点 | 建议图形 | 备注 |
| --- | --- | --- | --- |
| 库房 | `WarehousePanelButton` | 仓印 / 木箱 | 快捷进入库房 |
| 中枢 | `TaskPanelButton` | 令牌 / 印章 | 快捷进入中枢 |
| 弟子谱 | `DisciplePanelButton` | 竹简名册 | 快捷进入弟子谱 |

### 5.5 地图工具（WorldPanel）

| 功能 | 按钮节点 | 建议图形 |
| --- | --- | --- |
| 缩小 | `MapZoomOutButton` | “-” / 缩小镜 |
| 放大 | `MapZoomInButton` | “+” / 放大镜 |
| 复位 | `MapZoomResetButton` | 靶心 / 归位印 |
| 主令 | `MapPrimaryActionButton` | 令旗 / 主印 |
| 副令 | `MapSecondaryActionButton` | 侧令 / 副印 |
| 重绘 | `RegenerateButton` | 旋笔 / 重绘符 |

### 5.6 常用面板与通用操作

| 功能 | 建议图形 | 说明 |
| --- | --- | --- |
| 设置（机宜卷） | 符令 / 齿轮 | 设置面板入口 |
| 存档（留影录） | 印记 / 卷轴 | 存档管理入口 |
| 关闭 | 叉 / 收卷符 | 弹窗关闭 |
| 返回 | 回箭 / 归卷符 | 返回上层 |
| 确认 | 勾 | 确认操作 |
| 取消 | 叉 | 取消操作 |
| 筛选 | 漏斗 | 列表筛选 |
| 排序 | 双箭 | 排序切换 |
| 搜索 | 放大镜 | 搜索入口 |

### 5.7 状态与提示

| 状态 | 建议图形 | 说明 |
| --- | --- | --- |
| 警示 | 叹号 | 危险与异常提示 |
| 提示 | 圆点 / 小铃 | 中性提示 |
| 成功 | 勾 | 结果确认 |

## 6. 命名与资源组织建议

- 目录建议：`CountyIdle/assets/ui/icons/`
- 命名约定：`icon_{key}_{size}`，例如 `icon_warehouse_24`
- 建议 key 列表（分组写法）：
- `warehouse` `task_hub` `organization` `disciple` `expedition`
- `sect_map` `prefecture_map` `world_map` `event_log` `report`
- `map_zoom_in` `map_zoom_out` `map_zoom_reset` `map_primary_action` `map_secondary_action` `map_regenerate`
- `settings` `save` `close` `back` `confirm` `cancel` `filter` `sort` `search`
- `warning` `info` `success`

## 7. 图集存储规范（Atlas）

- 推荐单图集路径：`CountyIdle/assets/ui/icons/icon_atlas_24.png`
- 可选高清图集：`CountyIdle/assets/ui/icons/icon_atlas_48.png`（2x）
- 图集要求：透明背景；若保留宣纸底，单独出“卡片类图集”，不要与小按钮混用
- 网格建议：`cell_size=24`，`spacing=4`，`margin=4`
- 坐标约定：左上角为原点，`row/col` 从 `0` 开始

计算规则（用于切片）：

```text
x = margin + col * (cell_size + spacing)
y = margin + row * (cell_size + spacing)
width = cell_size
height = cell_size
```

## 8. 映射表（CSV）

- 映射表文件：`docs/icon_atlas_map.csv`
- 字段约定：`key,label,atlas,row,col,width,height,notes`
- 建议保持“key”与本规范的 key 列表一致

推荐排序原则（从左上到右下）：

| 顺序 | 分组 |
| --- | --- |
| 1 | 底栏入口（库房/中枢/谱系/弟子/历练） |
| 2 | 当前正式地图入口（天衍峰山门图/世界地图） |
| 3 | 隐藏备用入口（外域/见闻/报表/历练） |
| 4 | 地图工具（缩小/放大/复位/主令/副令/重绘） |
| 5 | 通用操作（设置/存档/关闭/返回/确认/取消/筛选/排序/搜索/警示/提示/成功） |

## 9. Godot 接入建议

- `Button` 使用 `theme_override_icons` 或设置 `TextureRect` + `region`
- `AtlasTexture` 用法：`atlas` 指向图集，`region` 使用第 7 节计算规则
- 若 icon 与文案并存：优先保留文本可读性，icon 做辅助识别
- 若入口当前处于隐藏备用状态，可先准备资源与映射，不必强行在现行 UI 中占位

## 10. 落地与替换策略

- 先保持“文字 + icon 并存”，不以 icon 完全替代文案
- 替换顺序建议：底栏入口 → 当前正式地图入口与工具 → 常用面板 → 隐藏备用入口
- 若 icon 与文案语义不一致，以文案为准并优先修正文案
