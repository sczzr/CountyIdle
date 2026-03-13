# 功能卡（Feature Card）

## 功能信息

- 功能名：宗门图 Layer 3 最小运行时接入（Y 排序 + 破框起步）
- 优先级：`P1`
- 目标版本：2026-03-12
- 关联系统：`CountyTownMapViewSystem`、`TownMapGeneratorSystem`、`TownMapData`、`docs/11_map_asset_production_spec.md`

## 目标与用户收益

- 目标：把宗门图从“只有 Layer 1 / Layer 2 地表”推进到“存在可排序立体物件”的最小闭环，让 `Building / ActivityAnchor` 真正进入绘制链路，并验证 Y 排序与破框遮挡在当前山门图里成立。
- 玩家可感知收益（10 分钟内）：打开天衍峰山门图后，不再只看到地面与道路，而能看到庶务殿、传法院、总坊、巡山岗、居舍等立体场所出现在相应地块上，地图空间感明显增强。

## 实现范围

- 包含：
  - `TownMapGeneratorSystem` 生成最小可用的 `Building / ActivityAnchor`
  - `CountyTownMapViewSystem` 正式接入 `DrawStructures()` 进入主绘制链路
  - 场所锚点与立体建筑按 Y 值排序绘制
  - 回写 `docs/05_feature_inventory.md`、`docs/08_development_list.md`
- 不包含：
  - 本轮不接 Layer 4 氛围层
  - 本轮不恢复弟子可视移动
  - 本轮不制作独立山体 / 竹林 / 塔类新贴图资产，先复用现有 runtime 立体绘制

## 实现拆解

1. 按地块内容与临路关系，为山门图筛出可承载立体物件的格子。
2. 生成最小的一组 `Building / ActivityAnchor`，覆盖生产、治务、居舍、巡山与休憩语义。
3. 将 `DrawStructures()` 接入主绘制顺序，保持选中态覆盖在最上层。

## 验收标准（可测试）

- [ ] 天衍峰山门图存在可见 `Building / ActivityAnchor`，而不是只有地表。
- [ ] 立体物件按 Y 排序绘制，下方物件不会被上方物件错误盖住。
- [ ] 左侧检视器的选中链路不受破坏。
- [ ] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - 若结构数量过多，山门图会显得拥挤并削弱格位可读性。
  - 若锚点与道路朝向不一致，会出现视觉入口和逻辑入口错位。
- 回滚方式：
  - 保留 `DrawStructures()` 主链路与最小数据生成入口；
  - 若物件密度过高，可回退数量规则，但不回退 Layer 3 接入能力本身。
