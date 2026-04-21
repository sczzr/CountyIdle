# 改动提案：战略地图配置交互数据块

## 提案信息

- 标题：战略地图配置交互数据块
- 日期：2026-04-20
- 提案人：Codex
- 变更级别：`L2 机制`
- 关联功能卡：`docs/feature-cards/FC-20260420-strategic-map-config-interaction.md`
- 关联开发项：`DL-001`

## 改动背景

- 当前问题：
  - `DL-001` 已经具备配置源 / 生成源双轨、可见调试切换和配置覆盖补强。
  - 但世界图一旦走配置源，`ApplyWorldConfigDefinition()` 会清空 `_xianxiaWorldMap`，导致 hex 点选、世界站点详情与二级地图入口失效。
  - 因此世界图目前必须继续默认走生成源；配置源还不能承担产品主链。
- 证据（数据/玩家反馈）：
  - `docs/05_feature_inventory.md` 已记录世界图配置模型仍需扩展到 `hex cell / site / 地貌家族 / 二级入口`。
  - `docs/change-proposals/CP-20260420-strategic-map-config-coverage.md` 已明确不应在交互数据补齐前默认切回配置源。

## 改动内容

- 改什么：
  - 为 `StrategicMapDefinition` 增加可选 `interactive_world`。
  - 让配置加载器支持配置内的字符串枚举，并对交互世界数据做基础归一化。
  - 配置世界图存在 `interactive_world.cells` 时，`StrategicMapViewSystem` 缓存该数据并保留世界格点选链路。
  - 配置源世界图在绘制交互 hex 层时继续允许配置轮廓和河流参与表现，避免视觉退化。
- 不改什么：
  - 世界图默认仍保留程序生成主链。
  - 不改变小时结算、资源结算、存档格式与二级地图玩法规则。
  - 不将 `Main.cs` 扩成玩法计算层。
- 影响系统：
  - `CountyIdle/scripts/models/StrategicMapConfig.cs`
  - `CountyIdle/scripts/systems/StrategicMapConfigSystem.cs`
  - `CountyIdle/scripts/systems/StrategicMapViewSystem.cs`
  - `CountyIdle/data/strategic_maps.json`

## 预期结果

- 预期提升指标：
  - 配置源世界图从“静态可视 fallback”提升为“可点选、可进入二级地图的调试/配置链路”。
  - 后续默认切回配置源时，不必再重写世界站点面板与二级地图入口。
- 可接受副作用：
  - 初期 `interactive_world` 只覆盖一小块样例地带，不追求替代完整程序生成世界。
  - 配置源视觉密度可能因为静态轮廓 + hex 交互层叠加而需要后续美术调参。

## 验证计划

- 验证方式：
  - 运行 `dotnet build .\Finally.sln`。
  - 运行 `dotnet run --project .\tools\StrategicMapSmoke\StrategicMapSmoke.csproj`，确认配置样例的 `cells/sites`、节点与标签对齐关系没有回退。
  - 运行 Godot headless 加载 `res://scenes/ui/WorldPanel.tscn`。
  - 若可进行 F5 手测，打开世界舆图，使用调试切换切到配置源，点选配置站点并尝试进入二级地图。
- 观察周期：本轮实现后立即验证；后续若默认源切换再做完整 F5 走查。
- 成功判定阈值：
  - 构建通过，场景可加载。
  - 配置源不再清空世界交互数据。
  - 配置站点与 cell 引用无加载警告或仅有可接受的非阻塞提示。

## 回滚条件

- 触发条件：
  - 配置源切换导致 `WorldPanel` 加载失败。
  - 配置源点击世界图抛异常或阻断生成源主链。
  - 二级地图入口无法从配置站点生成局部沙盘。
- 回滚步骤：
  - 移除 `interactive_world` 配置块。
  - 将 `ApplyWorldConfigDefinition()` 恢复为清空 `_xianxiaWorldMap`。
  - 移除配置加载器中的交互世界校验扩展。
