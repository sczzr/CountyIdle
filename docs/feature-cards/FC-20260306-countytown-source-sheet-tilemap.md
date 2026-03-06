# 功能卡（Feature Card）

## 功能信息

- 功能名：县城原图素材等距 tilemap 接入
- 优先级：`P1`
- 目标版本：`v0.x`
- 关联系统：`CountyTownMapViewSystem`、`TownMapGeneratorSystem`、`美术资源管线`

## 目标与用户收益

- 目标：直接使用 `CountyIdle/assets/picture/Isometric_game_asset_sheet_topdown_view_zen_chines_delpmaspu.png` 中的真实素材切片，驱动县城俯瞰页的地表、道路和装饰绘制。
- 玩家可感知收益（10 分钟内）：县城地图不再是“按参考图临摹”的替代品，而是直接使用你提供的素材图中的实际草地 / 石径 / 水洼 / 花丛 / 石块组件。

## 实现范围

- 包含：
  - 从原图素材表提取组件，生成透明底切片、metadata、contact sheet
  - 将提取出的真实组件编排为县城运行时 atlas / manifest / TileSet
  - `CountyTownMapViewSystem` 优先使用这套 atlas 驱动地表、道路 mask 与装饰绘制
  - 保留现有房屋立体绘制、居民 sprite 与 atlas 缺失时的安全回退
- 不包含：
  - `TownMapGeneratorSystem` 路网/建筑生成规则重写
  - 周边郡图、天下州域地图的 tilemap 化
  - 经济、人口、探险等核心飞轮数值改动

## 实现拆解

1. 从原图素材表提取组件并生成 `source_sheet_extract_t30`
2. 依据县城现有语义（grass / courtyard / road mask / props）建立真实组件映射
3. 生成运行时 atlas 并完成县城视图接入、文档回写与构建验证

## 验收标准（可测试）

- [ ] 县城地图优先使用由原图 `Isometric_game_asset_sheet_topdown_view_zen_chines_delpmaspu.png` 提取出的 atlas 绘制地表
- [ ] 道路会根据四向连接关系切换不同 stone-path tile 形态
- [ ] 建筑与居民显示链路保持可用，不改写 `GameState` 核心结算字段
- [ ] atlas 缺失时仍可回退到旧版纹理面片绘制
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：原图中的部分素材不是完整 16 向 autotile 套装，因此当前道路 mask 存在“多 mask 复用同一素材”的折中映射。
- 回滚方式：回退 `CountyTownMapViewSystem.cs` 与 `assets/tiles/county_reference_isometric/` 资源目录即可恢复旧版 seamless 面片渲染。
