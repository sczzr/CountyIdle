# 数值平衡日志：修仙世界河流/道路 edge overlay

## 记录信息

- 日期：2026-03-08
- 版本：当前工作区
- 关联提案：`docs/change-proposals/CP-20260308-xianxia-world-edge-overlays.md`

## 改动摘要

- 改动项：世界图把河流/道路从 polyline 主表现切到 hex 共边 overlay，并补上道路自动连通网络、河岸、桥梁、路口与悬崖边细节。
- 改动前：修仙世界图河流主要由 polyline 覆盖，`RoadMask` 尚未真正生成和显示。
- 改动后：世界图按 `RiverMask / RoadMask` 沿 hex 共边绘制 overlay；附庸据点与宗门候选之间会自动生成道路连接。

## 结果数据

- 指标 1：默认配置 `RiverMask != None` 的格数 = `242`，`Water != None` 的格数 = `9`
- 指标 2：默认配置 `RoadMask != None` 的格数 = `111`，其中桥梁重叠格 = `8`，路口格 = `10`
- 指标 3：默认配置 `CliffMask != None` 的格数 = `38`，且 `dotnet build .\Finally.sln` 通过

## 结论

- 是否达到预期：`是`
- 下一步：`保留`

## 复盘

- 有效原因：把“连接关系”拆回 hex 边语义后，基础地表与连接 overlay 分层清晰，桥梁/河岸/路口/悬崖边也能继续复用同一套边语义。
- 无效原因：当前桥梁与路口仍是轻量几何表现，尚未接入正式美术纹理和更细的 corner 规则。
- 后续假设：下一轮可把桥面、河岸转角、道路分级、悬崖明暗面和建筑入口正式抽象成 edge/corner texture slot。

