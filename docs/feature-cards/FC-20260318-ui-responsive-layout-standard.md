# 功能卡模板（Feature Card）

## 功能信息

- 功能名：UI 响应式布局标准化（主界面与卷册）
- 优先级：`P1`
- 目标版本：2026-03-18
- 关联系统：主界面 UI、卷册弹窗、布局容器

## 目标与用户收益

- 目标：在不同分辨率与拉伸模式下保持布局稳定，避免 UI 溢出或遮挡关键按钮。
- 玩家可感知收益（10 分钟内）：主界面与卷册弹窗不再挤压溢出，滚动区域可顺畅浏览长列表/长文案。

## 实现范围

- 包含：主界面与核心卷册的锚点 + 容器化布局调整、必要的滚动/裁剪策略补齐，以及 `DisciplePanel` 命谱页、`CultivationPanel` 主卷页、`SectChroniclePanel` / `SectNamePanel`、`SettingsPanel`、`WorldPanel` 底部控制区与 `MapViewport` 地图背景/蒙层的低高度窗口补漏。
- 不包含：UI 视觉风格重做、玩法逻辑改动、数值平衡调整。

## 实现拆解

1. 主界面改为 `MarginContainer + VBox/HBox` 容器布局，左/中/右栏与顶/底栏使用 `Size Flags` 控制伸缩。
2. 核心卷册弹窗（仓储卷、机宜卷、留影录、治宗册、营建卷，以及后补的见闻报表卷、宗门命名卷）移除超视口固定尺寸，改为边距容器化布局。
3. 对长列表/长文案补齐 `ScrollContainer` 或裁剪策略，确保内容不撑破父容器。
4. `DisciplePanel` 命谱页与 `CultivationPanel` 主卷页补齐整页纵向滚动，确保 `720p` 下仍可完整浏览。
5. `SectChroniclePanel` 与 `SectNamePanel` 去掉固定 `CenterContainer + custom_minimum_size` 外框，改为边距化满屏承载。
6. `SettingsPanel` 设置区改为滚动承载，底部批复动作保持常驻可见。
7. `WorldPanel` 底部标签 / 调度行改为全宽锚点布局，不再依赖窄宽度固定 offset。
8. `WorldPanel/MapViewport` 接入专用 `background_map.png` 地图背景图，并保持随地图区域自适应铺满。
9. 在地图背景上叠加半透明压暗泛黄蒙层，弱化底图干扰、提升地图元素可读性。
10. 设置卷分辨率候选需保留 `1280x720` 下限，并自动补入当前屏幕最大分辨率。
11. 设置卷需改为“全屏复选框 + 窗口分辨率 + 画面缩放滑条”三件套，且回到窗口化后继续使用已登记的窗口分辨率。
12. 较高分辨率下默认展示更多内容而不是整体放大；内容放大/缩小由独立缩放滑条控制。

## 验收标准（可测试）

- [ ] 主界面与弹窗根节点不再硬编码超视口尺寸，支持不同分辨率下正常显示。
- [ ] 左/中/右栏与关键按钮始终在可见区域内，不被遮挡。
- [ ] 列表/长文案具备滚动或裁剪策略，不会撑破父容器。
- [ ] `WorldPanel/MapViewport` 在不同窗口尺寸下持续铺满专用地图背景图，不出现裸底色或背景比例错位，并通过半透明压暗泛黄蒙层保持地图元素可读性。
- [ ] `DisciplePanel` 命谱页、`CultivationPanel` 主卷页、`SectChroniclePanel` / `SectNamePanel`、`SettingsPanel` 与 `WorldPanel` 底部控制区在 `1280x720` 下仍可完整浏览。
- [ ] 设置卷可显示当前屏幕最大分辨率，且不会允许低于 `1280x720` 的窗口档位。
- [ ] 设置卷使用单个全屏复选框切换显示模式，并保持窗口化分辨率记忆。
- [ ] 分辨率切换后默认显示更多内容而不是放大画面，且可通过缩放滑条调整画面大小。
- [ ] `DisciplePanel` 命谱页、`CultivationPanel` 主卷页、`SectChroniclePanel` / `SectNamePanel`、`SettingsPanel` 与 `WorldPanel` 底部控制区在 `1280x720` 下可完整浏览，不因固定外框、固定 offset 或页面高度而丢失内容。

## 风险与回滚

- 风险：布局容器化后，部分面板视觉对齐可能与旧版略有差异。
- 回滚方式：回退对应 `*.tscn` 与 UI 路径修改到改动前版本。
