# 数值平衡日志：战略地图配置交互数据块

## 记录信息

- 日期：2026-04-20
- 版本：当前开发分支
- 关联提案：`docs/change-proposals/CP-20260420-strategic-map-config-interaction.md`
- 关联功能卡：`docs/feature-cards/FC-20260420-strategic-map-config-interaction.md`

## 改动摘要

- 改动项：世界图配置源增加可选 `interactive_world`，用于承载可点选 cell 与站点。
- 改动前：
  - 配置源世界图只渲染静态 `regions/outlines/routes/rivers/nodes/labels`。
  - `ApplyWorldConfigDefinition()` 会清空 `_xianxiaWorldMap`，配置源下无法走世界格点选、站点详情与二级地图入口。
- 改动后：
  - 配置可提供少量 `cells/sites`，加载后复用既有 `_xianxiaWorldMap` 交互链。
  - 世界图默认来源不变，仍由程序生成主链承担完整产品体验。

## 结果数据

- JSON 语法验证：`CountyIdle/data/strategic_maps.json` 通过 `ConvertFrom-Json`。
- 配置反序列化验证：使用最新 `CountyIdle/.godot/mono/temp/bin/Debug/CountyIdle.dll` 与 `JsonStringEnumConverter` 反序列化通过，`interactive_world` 读到 `19` 个 cells、`6` 个 sites。
- 配置烟测验证：新增 `dotnet run --project .\tools\StrategicMapSmoke\StrategicMapSmoke.csproj`，当前输出 `strategic_map_smoke=OK`；并补齐 `东荒哨台 / 云水河门` 两个此前缺失的显示标签，使交互站点与显示标签一致。
- 构建验证：`dotnet build .\Finally.sln -v:q` 通过，`0 Error(s)`。
- 存档烟测：`tools/SaveSmoke/bin/Debug/net8.0/SaveSmoke.exe` 通过，能列槽、读槽并生成测试数据库。
- Godot headless 验证：通过；使用临时下载到工作区 `.tmp` 的 `Godot_v4.6-stable_mono_win64_console.exe` 执行 `--headless --path .\\CountyIdle --scene res://scenes/ui/WorldPanel.tscn --quit-after 1`，`exit code 0`，无新增解析错误输出。
- F5 交互验证：待人工运行时走查，重点检查配置源切换、站点点选与二级地图进入。

## 结论

- 是否达到预期：`部分`
- 下一步：`继续调参`

## 复盘

- 有效原因：
  - 该方案复用既有 `XianxiaWorldMapData`、世界站点面板与二级地图生成器，避免并行维护另一套交互模型。
- 无效原因：
  - 初始配置只覆盖样例地带，不足以替换完整世界生成。
  - 本轮仍缺 F5 下的配置源切换、点选与二级地图进入验证，暂不能把配置源视为可替代默认世界图源。
- 后续假设：
  - 当 `interactive_world` 能覆盖完整世界格与站点后，可再次评估世界图默认切回配置源。
