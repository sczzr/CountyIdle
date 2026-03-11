# 功能卡（Feature Card）

## 功能信息

- 功能名：宗门图 Layer 1 / Layer 2 Godot 贴图接入（运行时一期）
- 优先级：`P1`
- 目标版本：2026-03-12
- 关联系统：`CountyTownMapViewSystem`、`TownMapGeneratorSystem`、`TownMapData`、`docs/11_map_asset_production_spec.md`

## 目标与用户收益

- 目标：把天衍峰山门图从“程序化纯色底格 + 语义线条”推进到“Layer 1 基础地块 atlas + Layer 2 decal / connector”的 Godot 可运行版本，先接通真实贴图链路，再为后续正式国风素材替换留好接口。
- 玩家可感知收益（10 分钟内）：打开山门图后，地面不再只是抽象色块；平地、院坪、路网、临水地势会拥有更接近卷轴地图的贴图与层次，地图读感更接近成品表现。

## 实现范围

- 包含：
  - 为 Layer 1 接入现有六边形地块 atlas 与 manifest
  - 为 Layer 2 接入道路 / 院坪 / 水域 decal 贴图与连接逻辑
  - 在 `TownMapGeneratorSystem` 中补入最小可用 terrain 分布（ground / road / courtyard / water）
  - 回写 `docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`
- 不包含：
  - 本轮不接入 Layer 3 立体物件
  - 本轮不恢复弟子可视移动
  - 本轮不制作正式量产版国风地块全集，只先跑通接入链路

## 实现拆解

1. 为 Layer 1 建立 atlas manifest，复用现有 hex 草地贴图作为基础地块。
2. 为 Layer 2 增加道路线、水纹、院坪的 decal 贴图与连接绘制。
3. 在生成器中补地形语义，使渲染链路有真实输入。
4. 保持 `tile 选中 -> 左栏检视`、缩放和平移链路兼容。

## 验收标准（可测试）

- [ ] `CountyTownMapViewSystem` 能优先读取 Layer 1 atlas manifest，而不是只画纯色 hex。
- [ ] 道路 / 水域 / 院坪能以 Layer 2 decal + connector 的方式出现在宗门图上。
- [ ] `TownMapGeneratorSystem` 生成结果中已存在 `Road / Courtyard / Water` 语义，而不再是全图 `Ground`。
- [ ] `dotnet build .\Finally.sln` 通过。

## 风险与回滚

- 风险：
  - 现有临时 atlas 与 decal 资源不一定完全符合最终国风标准；
  - 若把 Layer 2 连线逻辑写死，后续切换正式连接贴图会受限。
- 回滚方式：
  - 保留 atlas manifest、Layer 2 decal 入口和 terrain 生成骨架；
  - 若表现不理想，可回退到 decal 贴图但保留连接逻辑与 manifest 接口，不回退到纯色块状态。
