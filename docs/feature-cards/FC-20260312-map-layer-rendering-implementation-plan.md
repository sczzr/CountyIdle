# 功能卡（Feature Card）

## 功能信息

- 功能名：地图 L1-L5 绘制实施方案文档
- 优先级：`P1`
- 目标版本：2026-03-12
- 关联系统：`CountyTownMapViewSystem`、`Main.tscn`、`WorldPanel.tscn`、`docs/11_map_asset_production_spec.md`

## 目标与用户收益

- 目标：把地图分层素材规范继续收口为一份可执行的绘制实施方案文档，明确 `L1 ~ L5` 每层画什么、由谁画、在哪个 Godot 节点或绘制阶段接入、资源如何组织、阶段上如何推进。
- 玩家可感知收益（10 分钟内）：虽然本轮不直接新增画面资产，但后续地图表现层的推进会更稳，更少返工，能更快进入“卷轴地表 -> 路网水网 -> 立体山门 -> 氛围层”的连续演进。

## 实现范围

- 包含：
  - 新增 `docs/14_map_layer_rendering_implementation_plan.md`
  - 统一 `L1 ~ L5` 的层级编号与既有 `Layer 0 ~ 4` 规范映射
  - 写明每层的绘制内容、Godot 实施方式、数据来源、资源格式与验收口径
  - 回写 `docs/README.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`
- 不包含：
  - 本轮不直接新增运行时代码
  - 本轮不直接绘制正式素材
  - 本轮不修改小时结算、交互链路与存档格式

## 实现拆解

1. 对齐现有地图素材规范、当前运行时接入状态与 L1-L5 的编号映射。
2. 输出“每层画什么、怎么画、怎么接”的具体实施方案文档。
3. 将该文档挂到 DL-049 主线下，作为后续 Layer 3 / Layer 4 / Layer 5 的执行依据。

## 验收标准（可测试）

- [ ] `docs/14_map_layer_rendering_implementation_plan.md` 已明确写出 L1-L5 的绘制内容与实施方式。
- [ ] 文档能明确回答“哪一层在 `Main/UI` 里画、哪一层在 `CountyTownMapViewSystem` 里画、哪一层必须参与 Y 排序”。
- [ ] 文档能直接指导后续 Layer 4 / Layer 5 实现，而不是停留在审美描述。
- [ ] `docs/README.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md` 已增加对应入口或状态说明。

## 风险与回滚

- 风险：
  - 若文档只讲艺术目标，不讲当前代码落点，后续仍会出现“知道想要什么，但不知道接哪”的问题。
  - 若层级职责划分过粗，会导致 L3 / L5 边界在后续实现时反复变化。
- 回滚方式：
  - 保留 `L1-L5` 职责拆分、Godot 节点归属与分阶段顺序三部分硬约束；
  - 对具体资源数量与个别表现细节保留后续微调空间。
