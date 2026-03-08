# 功能卡：外域备用视图修仙语义化（V1）

## 功能信息

- 功能名：外域备用视图修仙语义化（V1）
- 优先级：`P2`
- 目标版本：当前迭代（2026-03-08）
- 关联系统：`PrefectureCityThemeConfigSystem`、`PrefectureMapGeneratorSystem`、`StrategicMapConfigSystem`

## 目标与用户收益

- 目标：把隐藏备用 Prefecture 视图的旧开封/郡县命名收口为修仙外域主题，避免未来重新启用时出现世界观漏词。
- 玩家可感知收益（10 分钟内）：当前虽然主界面只保留宗门地图和世界地图，但后续若重新开放备用外域视图，标题、城名、地标和坊巷也会与修仙设定一致。

## 实现范围

- 包含：
  - 更新外域备用视图 JSON 配置与代码 fallback 主题；
  - 统一 `strategic_maps.json` 标题；
  - 回写当前规格和开发列表。
- 不包含：
  - 不重新开放第三张地图入口；
  - 不重写外域备用视图生成算法和节点密度规则。

## 实现拆解

1. 修正 `prefecture_city_theme.json` 的标题、城名、地标和街区名。
2. 同步更新 `PrefectureCityThemeConfig` 与 fallback 默认值。
3. 修正 `strategic_maps.json` 世界/外域标题。
4. 回写规格、看板与归档。

## 验收标准（可测试）

- [x] 外域备用视图不再出现 `周边郡图 / 开封郡城 / 开封府衙 / 州桥市集 / 天下州域`。
- [x] 当前 fallback 标题统一为 `世界地图 / 外域态势`。
- [x] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：若未来要恢复“历史都城/开封风格”路线，需要把当前主题与术语抽成可切换配置，而不是单一主题。
- 回滚方式：保留双地图主线，只将外域备用视图 JSON 与 fallback 文案改回旧主题即可。
