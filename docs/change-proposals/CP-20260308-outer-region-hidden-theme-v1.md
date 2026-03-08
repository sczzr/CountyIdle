# 改动提案：江陵府外域备用视图修仙语义化（V1）

> 历史兼容说明（2026-03-08）：本文保留旧阶段的“县城 / 郡图 / 郡县 / 州府”等表述以便追溯；在当前项目语义中，请分别按“天衍峰驻地 / 江陵府外域 / 天衍峰经营 / 世界地图”理解。


## 提案信息

- 标题：将隐藏备用外域视图的旧郡县/开封主题切到修仙外域主题
- 日期：2026-03-08
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：主界面已只保留 `天衍峰山门图 / 世界地图`，但隐藏备用 Prefecture 视图的 JSON、fallback 标题与地标命名仍残留 `周边郡图 / 开封郡城 / 天下州域` 等旧语义。
- 证据（数据/玩家反馈）：上一轮全局语义复扫后，剩余旧词已集中在 `prefecture_city_theme.json`、`strategic_maps.json` 与 Prefecture fallback 代码中，属于典型“未来可能漏出”的主题残留。

## 改动内容

- 改什么：
  - 将隐藏备用外域视图的标题、城名、地标、街区名统一切到修仙外域术语；
  - 将 `strategic_maps.json` 中 `world / prefecture` 标题统一改为 `世界地图 / 江陵府外域`；
  - 将 Prefecture 相关 fallback 默认值同步改掉，避免配置缺失时回漏旧词。
- 不改什么：
  - 不重新启用第三张地图入口；
  - 不改江陵府外域备用视图的生成算法、道路/河流布局与缩放规则。
- 影响系统：
  - `CountyIdle/data/prefecture_city_theme.json`
  - `CountyIdle/data/strategic_maps.json`
  - `CountyIdle/scripts/models/PrefectureCityThemeConfig.cs`
  - `CountyIdle/scripts/systems/PrefectureCityThemeConfigSystem.cs`
  - `CountyIdle/scripts/systems/PrefectureMapGeneratorSystem.cs`
  - `docs/02_system_specs.md`
  - `docs/05_feature_inventory.md`
  - `docs/08_development_list.md`

## 预期结果

- 预期提升指标：
  - 隐藏备用地图链路与当前修仙双地图方向保持一致；
  - 回退到配置驱动标题时不再漏出 `天下州域 / 周边郡图`。
- 可接受副作用：
  - 旧“开封城式”主题被当前版本的修仙外域主题替代；
  - 若以后想恢复历史都城路线，需要再做主题切换机制。

## 验证计划

- 验证方式：
  - 全局搜索 `周边郡图 / 开封 / 郡城 / 天下州域 / 州桥市集`
  - `dotnet build .\Finally.sln`
- 观察周期：本次迭代即时验证
- 成功判定阈值：
  - 运行中配置与代码 fallback 不再出现上述旧词；
  - 构建通过。

## 回滚条件

- 触发条件：江陵府外域备用视图配置缺失、fallback 文案异常或未来恢复旧主题需求明确。
- 回滚步骤：
  1. 恢复 `prefecture_city_theme.json` 与 Prefecture fallback 字符串；
  2. 恢复 `strategic_maps.json` 中旧标题；
  3. 保持主界面双地图结构不变。


