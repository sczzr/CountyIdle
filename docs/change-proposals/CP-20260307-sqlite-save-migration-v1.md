# 改动提案（Change Proposal）

## 提案信息

- 标题：SQLite 存档迁移（V1：默认槽 + 快照 + 兼容旧 JSON）
- 日期：2026-03-07
- 提案人：Gameplay Agent
- 变更级别：`L3 架构`

## 改动背景

- 当前问题：
  - 当前 `SaveSystem` 仅将完整 `GameState` 写入 `user://savegame.json`；
  - 随着资源系统、人口系统、地图系统持续扩展，单文件 JSON 存档缺乏多槽、迁移和历史快照管理能力；
  - 后续若引入更复杂的资源分层、英雄实体或统计报表，JSON 单文件会迅速变得难以维护。
- 证据（数据/玩家反馈）：
  - 当前代码仅存在 `savegame.json` 单文件落盘；
  - 本轮决策已明确“本地存档层采用 SQLite”，同时保留 JSON 作为快照表达形式。

## 改动内容

- 改什么：
  - 在项目内引入 `Microsoft.Data.Sqlite`
  - 新增 SQLite 数据库文件 `user://countyidle.db`
  - 新增 `schema_migrations / save_slots / save_snapshots` 三张表
  - `SaveSystem` 改为写入默认槽 `default`
  - 首次读取时自动兼容迁移旧版 `user://savegame.json`
  - 保持现有主界面按钮与快捷键接口不变
- 不改什么：
  - 不新增多槽 UI
  - 不引入 ORM
  - 不将 `GameState` 完全拆表
  - 不改动客户端设置存储方式
- 影响系统：
  - `CountyIdle/CountyIdle.csproj`
  - `CountyIdle/scripts/core/SaveSystem.cs`
  - `CountyIdle/scripts/core/SqliteMigrationRunner.cs`
  - `CountyIdle/scripts/core/SqliteSaveRepository.cs`
  - `CountyIdle/scripts/models/SaveSlotSummary.cs`
  - `CountyIdle/scripts/models/SaveSnapshotRecord.cs`
  - `docs/02_system_specs.md`

## 预期结果

- 预期提升指标：
  - 存档稳定性与扩展性提升
  - 支持默认槽 + 快照模型，为后续多槽管理与历史记录预留空间
  - 降低未来存档迁移成本
- 可接受副作用：
  - 增加 SQLite 依赖与数据库初始化成本
  - 运行时第一次读档会多一次旧 JSON 迁移判断

## 验证计划

- 验证方式：
  - `dotnet build .\Finally.sln`
  - 运行点击“存档/读档”
  - 验证快捷键 `QuickSave / QuickLoad`
  - 验证数据库文件、表结构与默认槽快照写入
  - 验证仅保留旧 JSON 时可自动迁移
- 观察周期：单轮启动 + 一次迁移 + 一次手动存档读档
- 成功判定阈值：
  - 不崩溃
  - 可读可写
  - 旧存档可迁移
  - 现有主循环与读档入口不需要 UI 重做

## 回滚条件

- 触发条件：
  - SQLite 运行期初始化失败
  - 读档导致 `GameState` 反序列化异常
  - Godot 导出运行环境缺失 SQLite 原生依赖
- 回滚步骤：
  1. 保留数据库与迁移器代码
  2. 将 `SaveSystem` 切回 JSON 文件实现
  3. 继续保留旧 JSON 兼容通道
