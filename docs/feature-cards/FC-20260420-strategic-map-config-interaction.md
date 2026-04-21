# 功能卡：战略地图配置交互数据块（DL-001 收口二段）

## 功能信息

- 功能名：战略地图配置交互数据块
- 优先级：`P1`
- 目标版本：当前开发分支
- 关联系统：`DL-001` 战略地图配置驱动接入、`StrategicMapConfigSystem`、`StrategicMapViewSystem`、世界格二级地图入口
- 关联提案：`docs/change-proposals/CP-20260420-strategic-map-config-interaction.md`
- 关联日志：`docs/balance-logs/BL-20260420-strategic-map-config-interaction.md`

## 目标与用户收益

- 目标：让 `strategic_maps.json` 的世界图不仅能配置静态轮廓、路线、节点和标签，还能携带一小块可交互世界数据，使配置源切换后仍保留 `hex 点选 -> 站点详情 -> 二级地图入口`。
- 玩家可感知收益（10 分钟内）：调试切换到配置源后，世界图不再退化为只能看的静态点线图；玩家仍可点击世界格或配置站点查看身份，并继续进入局部沙盘。

## 实现范围

- 包含：
  - 在世界图配置中增加 `interactive_world` 数据块，用少量 `cells/sites` 承载点选与二级地图需要的最小数据。
  - 配置加载阶段支持字符串枚举、空列表补齐与基础引用校验。
  - 世界图配置源读取到交互数据时缓存 `_xianxiaWorldMap`，继续复用既有点选、详情和二级地图生成链。
  - 配置源世界图绘制时保留配置轮廓、河流与标签，避免只剩空白 hex 外框。
- 不包含：
  - 不把世界图默认来源切回配置源。
  - 不把程序生成器完整序列化为配置。
  - 不新增二级地图专属玩法结算。

## 实现拆解

1. 扩展 `StrategicMapDefinition`，允许 `world.interactive_world` 直接承载精简的 `XianxiaWorldMapData`。
2. 扩展 `StrategicMapConfigSystem`，加载字符串枚举并校验 `cells/sites` 的空值与引用关系。
3. 调整 `StrategicMapViewSystem.ApplyWorldConfigDefinition()`，当配置携带交互数据时调用 `CacheWorldLayout()`，否则仍按静态配置源回退。
4. 补一组覆盖世界核心站点的配置样例，让配置源至少具备宗门候选地、坊市、灵脉、遗迹与河门等可点选入口。

## 验收标准（可测试）

- [x] `dotnet build .\Finally.sln` 通过。
- [x] `WorldPanel.tscn` Godot headless 加载通过。
- [ ] 切到配置源后，世界图仍可点选配置站点并触发左侧世界站点摘要刷新。（待 F5 交互走查）
- [ ] 对配置站点执行进入操作时，仍可生成二级局部沙盘。（待 F5 交互走查）

## 本轮验证记录

- `CountyIdle/data/strategic_maps.json` 已通过 PowerShell `ConvertFrom-Json` 语法解析。
- 使用最新 `CountyIdle/.godot/mono/temp/bin/Debug/CountyIdle.dll` 反序列化配置，通过；`interactive_world` 读取到 `19` 个 cells 与 `6` 个 sites。
- 新增 `dotnet run --project .\tools\StrategicMapSmoke\StrategicMapSmoke.csproj` 自动化护栏，当前输出 `strategic_map_smoke=OK`，并确认世界图配置存在 `6` 个交互节点、`8` 个显示标签、`19` 个 cells 与 `6` 个 sites，且交互站点与可见节点/标签对齐。
- `dotnet build .\Finally.sln -v:q` 通过。
- `tools/SaveSmoke/bin/Debug/net8.0/SaveSmoke.exe` 通过。
- 使用临时下载到工作区 `.tmp` 的 `Godot_v4.6-stable_mono_win64_console.exe` 执行 `--headless --path .\\CountyIdle --scene res://scenes/ui/WorldPanel.tscn --quit-after 1`，结果 `exit code 0`，未出现新的解析错误输出。
- F5 交互验证未在本轮完成；当前仍需人工确认配置源切换、站点点选与二级地图进入。

## 风险与回滚

- 风险：若配置站点坐标与可交互 cell 不一致，会出现节点可见但不可点选或点选落到错误地块。
- 风险：配置源同时绘制静态轮廓和 hex 交互层，视觉密度可能需要后续微调。
- 回滚方式：移除 `interactive_world` 配置块，并让 `ApplyWorldConfigDefinition()` 回到纯静态配置源清空 `_xianxiaWorldMap` 的旧行为。
