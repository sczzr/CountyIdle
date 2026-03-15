# 改动提案（Change Proposal）

## 提案信息

- 标题：世界格方向遮罩继承到二级地图
- 日期：2026-03-16
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：
  - 二级地图已经能继承一级 world hex 的地形家族，但局部沙盘里的道路入口、水体布局与 ridge 分布仍偏模板化；
  - `RoadMask / RiverMask / CliffMask` 这些已经存在于一级图中的方向信息，还没有稳定下钻到二级地图。
- 证据（数据/玩家反馈）：
  - 用户上一轮需求明确指出“二级地图需要依照一级地图的地形块来生成地图”；
  - 当前代码中 `PaintApproachPath()` 仍默认从左侧进场，水体/山脊也仍保留明显的固定落点。

## 改动内容

- 改什么：
  - 引入 world hex 遮罩到局部沙盘边界方向的映射；
  - 让局部道路、水体、高差分布优先继承一级图方向信息；
  - 让入口锚点与 facing 与主入口方向同步。
- 不改什么：
  - 不新增新的 tileset 资源；
  - 不修改 world hex 点击、左栏检视与二级地图进入流程；
  - 不在本轮补细碎桥梁/崖边/河道的专属动态控件。
- 影响系统：
  - `CountyIdle/scripts/systems/WorldSiteLocalMapGeneratorSystem.cs`
  - `CountyIdle/scripts/ui/MainWorldSitePanel.cs`
  - `CountyIdle/scripts/models/TownMapData.cs`（只复用既有视觉元数据，不扩新结构）
  - 相关文档回写：`docs/02_system_specs.md`、`docs/05_feature_inventory.md`、`docs/08_development_list.md`

## 预期结果

- 预期提升指标：
  - 二级地图与一级图之间的“同一世界格下钻感”更强；
  - 玩家能从道路/水体/高差方向更直观看出 world hex 的局部展开；
  - 后续补专属模板时，可在不推翻当前骨架的前提下继续叠加专属交互。
- 可接受副作用：
  - 在当前素材储备下，方向继承会先以“布局与边界方向变化”为主，connector 细节仍属近似表达。

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 手动抽查至少数个具不同 `RoadMask / RiverMask / CliffMask` 的 world hex，对比局部沙盘入口、水体、ridge 方向是否变化。
- 观察周期：
  - 本轮 build 后立即验证，后续 Godot 运行烟测继续观察。
- 成功判定阈值：
  - build 通过；
  - 局部地图入口、水体、高差的主方向不再固定模板化；
  - 世界格进入与检视主链不回退。

## 回滚条件

- 触发条件：
  - 新方向映射导致局部地图经常生成不可读布局，或入口/水体/高差压住核心区，明显破坏当前可读性。
- 回滚步骤：
  - 回退多方向叠加，仅保留主方向继承；
  - 若仍异常，再回退到固定模板布局，但保留已建立的 mask helper 与文档。
