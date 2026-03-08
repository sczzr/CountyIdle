# 改动提案：宗门地图语义改造（V1）

## 提案信息

- 标题：宗门地图语义从县城表述切到宗门驻地表述
- 日期：2026-03-08
- 提案人：Codex
- 变更级别：`L2 机制`

## 改动背景

- 当前问题：虽然主地图入口已经压缩为宗门地图与世界地图，但宗门地图内部仍残留“县城 / 市集 / 学宫 / 官署 / 茶肆”等县城语义。
- 证据（数据/玩家反馈）：本轮继续推进“双地图”后，用户明确要求继续实现，而下一个最直接的缺口就是宗门地图内部语义尚未彻底宗门化。

## 改动内容

- 改什么：
  - 新增 `SectMapSemanticRules`；
  - 将场所标签切换为 `灵田 / 炼器坊 / 山门坊市 / 藏经阁 / 宗务殿 / 论道亭`；
  - 将场景脚本入口切到 `SectMapViewSystem`。
- 不改什么：
  - 不重写 `TownActivityAnchorType` 与 `TownMapData` 兼容结构；
  - 不改经济、人口和岗位公式。
- 影响系统：
  - `CountyIdle/scripts/systems/SectMapSemanticRules.cs`
  - `CountyIdle/scripts/systems/SectMapViewSystem.cs`
  - `CountyIdle/scripts/systems/TownMapGeneratorSystem.cs`
  - `CountyIdle/scripts/systems/CountyTownMapViewSystem.cs`
  - `CountyIdle/scripts/systems/CountyTownMapViewSystem.Anchors.cs`
  - `CountyIdle/scenes/ui/WorldPanel.tscn`

## 预期结果

- 预期提升指标：
  - 宗门地图主观感知更统一；
  - 后续接入山门、药园、演武场、藏经阁等资产时，不需要先回头清理旧县城文案。
- 可接受副作用：
  - 内部仍保留部分 `Town* / CountyTown*` 类型名作兼容；
  - 当前仅完成表现语义层，不代表玩法系统已全部宗门化。

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 检查宗门地图标签、选中提示和状态文本
- 观察周期：本次迭代即时验证
- 成功判定阈值：
  - 宗门地图不再显示旧县城场所命名；
  - 场景脚本入口切到 `SectMapViewSystem`；
  - 构建通过。

## 回滚条件

- 触发条件：宗门地图脚本无法加载、标签错误、构建失败。
- 回滚步骤：
  1. 恢复 `WorldPanel.tscn` 对 `CountyTownMapViewSystem.cs` 的脚本引用；
  2. 取消 `SectMapSemanticRules` 映射调用；
  3. 保留双地图入口，不回滚世界图。
