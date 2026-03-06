# 数值平衡日志（Balance Log）

## 记录信息

- 日期：2026-03-06
- 版本：v0.x
- 关联提案：`CP-20260306-prefecture-village-style-generator-03.md`

## 改动摘要

- 改动项：开封城郡图命名改为 JSON 配置驱动
- 改动前：城门/坊市/地标/地貌名称写死在 `PrefectureMapGeneratorSystem.cs`
- 改动后：命名从 `data/prefecture_city_theme.json` 读取，支持直接改数据生效

## 结果数据

- 指标 1：新增 `prefecture_city_theme.json`，可配置城门四门名称与坊市名称池
- 指标 2：新增 `PrefectureCityThemeConfigSystem`，配置缺失时回退默认命名
- 指标 3：郡图标签继续支持 `min_zoom` 分级显隐，命名数据化不影响可读性策略

## 结论

- 是否达到预期：`是`
- 下一步：`保留`

## 复盘

- 有效原因：配置与生成解耦后，风格调整成本明显降低。
- 无效原因：当前命名配置仍是单主题文件，尚未扩展为多主题切换。
- 后续假设：可追加多主题配置并在设置面板中切换（宋/唐/架空风格）。
