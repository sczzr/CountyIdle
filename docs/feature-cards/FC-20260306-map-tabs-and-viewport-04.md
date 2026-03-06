# 功能卡（Feature Card）

## 功能信息

- 功能名：县城俯瞰与周边郡图改用 `ExtractedTileSet.tres` 绘制
- 优先级：`P1`
- 目标版本：`v0.x`
- 关联系统：`地图显示`、`UI`

## 目标与用户收益

- 目标：将县城俯瞰地图与周边郡图统一接入提取瓦片集资源，减少风格割裂。
- 玩家可感知收益（10 分钟内）：地图页切换后视觉素材来源统一，县城图与郡图贴图风格更一致。

## 实现范围

- 包含：
  - 县城图地表与建筑贴图优先从 `res://assets/tiles/extracted_tilemap/ExtractedTileSet.tres` 读取
  - 周边郡图底图改为基于 `ExtractedTileSet` 的瓦片铺底
  - 保留资源缺失时的安全回退逻辑
- 不包含：
  - 天下州域地图绘制规则重做
  - 地图玩法逻辑、寻路与事件机制改动

## 实现拆解

1. 在县城图系统中接入 `TileSetAtlasSource` 并建立 atlas 坐标到纹理的映射。
2. 在战略地图系统（郡图模式）加载 `ExtractedTileSet` 并生成瓦片底图。
3. 完成最小构建验证，确保现有地图交互与缩放接口不变。

## 验收标准（可测试）

- [ ] 县城俯瞰图渲染优先使用 `ExtractedTileSet.tres` 纹理
- [ ] 周边郡图可见基于 `ExtractedTileSet.tres` 的瓦片底图
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：atlas 坐标语义不匹配时，局部纹理表现可能与预期地形不完全一致。
- 回滚方式：回退 `CountyTownMapViewSystem.cs`、`StrategicMapViewSystem.cs` 与本功能卡文件。
