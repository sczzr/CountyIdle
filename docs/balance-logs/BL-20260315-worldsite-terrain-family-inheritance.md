# 数值 / 表现平衡日志

## 记录信息

- 日期：2026-03-15
- 版本：地图空间分层增量
- 关联提案：`CP-20260315-worldsite-terrain-family-inheritance.md`

## 改动摘要

- 改动项：世界格地形家族继承到二级地图
- 改动前：
  - 世界图底盘已经能按 world hex 语义分桶；
  - 二级地图入口已存在，但 external map 底盘更多按 `TownTerrainType` 粗分类取图，不同一级地貌进入后视觉差异偏弱。
- 改动后：
  - world hex 的基础地形家族收口为共享规则；
  - 二级地图 external map 会写入并读取继承地形家族；
  - 局部地图生成会进一步根据 source world hex 调整 `Ridge / Spirit / Hazard / Water` 分布。

## 结果数据

- 指标 1：一级图与二级图共用同一套 `Plain / Spirit / Rugged / Snow / ShallowWater / DeepWater` 分桶口径
- 指标 2：external map 渲染已由“粗 terrain 随机取图”提升为“优先按继承家族取图”
- 指标 3：`2026-03-15` 已执行 `dotnet build .\Finally.sln`，结果 `0 Error(s)`；Godot 本轮未能在当前环境中重新拉起运行烟测，暂以仓库内同日 `godot_smoke.log` 作为历史参考

## 结论

- 是否达到预期：`部分`
- 下一步：`保留本轮共享地形家族骨架，并继续做 Godot 运行烟测与四类二级模板的专属交互`

## 复盘

- 有效原因：
  - 不额外引入新资源，先复用现有 `L1_hex_tileset.tres` 与现有 world hex 语义，可在当前链路内形成最小闭环。
- 无效原因：
  - 现有 atlas 资源类型仍有限，`Rugged / Snow` 暂仍属于近似映射，不是最终量产表现。
- 后续假设：
  - 后续继续补正式国风素材时，只需要往共享地形家族补资源与映射，不必重写 world-site 生成与 external map 渲染边界。
