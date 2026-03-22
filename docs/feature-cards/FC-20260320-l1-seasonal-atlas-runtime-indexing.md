# 功能卡（Feature Card）

## 功能信息

- 功能名：`L1_hex_tileset.tres` 四季 atlas 运行时索引接入（运行时五期）
- 优先级：`P1`
- 目标版本：2026-03-20
- 关联系统：`StrategicMapViewSystem`、`CountyTownMapViewSystem`、`CountyTownMapViewSystem.Residents.cs`、`Main.cs`、`L1_hex_tileset.tres`

## 目标与用户收益

- 目标：让世界图、宗门图与二级局部沙盘开始支持“跨全部 source 的四季 atlas 候选索引”，并由游戏历法驱动四季列切换，而不是继续把列当成无语义随机变体。
- 玩家可感知收益（10 分钟内）：当后续 tileset 补齐 `terrain_family / world_terrain_family / season` metadata 后，地图底盘可以自动切到对应季节，不需要继续为每增加一组图就改一轮 `C#` 文件名分支。

## 实现范围

- 包含：
  - `StrategicMapViewSystem` 跨全部 source 扫描 tileset custom data，并优先按 `world_terrain_family / terrain_family + season` 选世界图地块
  - `CountyTownMapViewSystem` 增加四季 atlas 的季节索引脚手架，优先按 `terrain_family + season` 选局部地块
  - `Main.cs` 把 `GameMinutes` 同步给地图渲染器，四季列切换统一由 `GameCalendarSystem` 季度索引驱动
  - 当 `season` metadata 缺失时，先按 `4x2` 约定用列号推导四季
- 不包含：
  - 本轮不强制把现有 `L1_hex_tileset.tres` 全量补齐 metadata
  - 本轮不移除历史文件名 / 坐标分支 fallback
  - 本轮不改 `Layer 2 ~ Layer 5` 的绘制主链

## 实现拆解

1. 为世界图补入跨全部 source 的 `family + season` 候选桶。
2. 为宗门图 / 二级地图补入同口径的四季候选桶。
3. 让历法季度驱动 `_currentSeasonIndex`，统一地图四季列。
4. 保留旧文件名分支与硬编码坐标作为 fallback，保证过渡期不掉图。

## 验收标准（可测试）

- [ ] `StrategicMapViewSystem` 不再只依赖第一个 source 读取四季候选
- [ ] `CountyTownMapViewSystem` 已具备 `terrain_family + season` 的候选索引脚手架
- [ ] `Main.cs` 已把 `GameMinutes` 同步给地图渲染器
- [ ] `dotnet build .\Finally.sln` 通过

## 验证记录

- 已执行 `dotnet build .\Finally.sln`
- 结果：通过，`0` error
- 已为 `L1_hex_tileset.tres` 的现有 `4` 组 source 补入首批 `terrain_family / world_terrain_family / season` metadata，供新的四季索引链路消费

## 风险与回滚

- 风险：
  - 若 metadata 尚未补齐，运行时仍会大量回退到历史文件名 / 坐标分支，因此“支持四季”与“现有素材已完整四季化”不是一回事。
  - 若美术后续给出的 `terrain_family` 文本不统一，会导致部分地块继续走 fallback。
- 回滚方式：
  - 保留当前新增的季节索引结构；
  - 若某批 metadata 不稳定，可先清空该批 metadata，让运行时继续回退到旧分支，而不回滚整个四季 atlas 框架。
