# 功能卡（Feature Card）

## 功能信息

- 功能名：SQLite 存档迁移（V1：默认槽 + 快照 + 兼容旧 JSON）
- 优先级：`P1`
- 目标版本：`v0.x`
- 关联系统：`SaveSystem / GameState / MainUI / MainShortcutBindings / ClientSettings / GameLoop`

## 目标与用户收益

- 目标：将当前本地 JSON 存档升级为 SQLite 存档层，并保留旧版 `savegame.json` 的兼容迁移路径。
- 玩家可感知收益（10 分钟内）：继续使用原有“存档 / 读档 / 快速存档 / 快速读档”操作，但底层变为更稳的 SQLite 存档；旧档可自动迁移；后续可扩展多存档槽与历史快照。

## 实现范围

- 包含：
  - 新增 SQLite 本地存档文件：`user://countyidle.db`
  - 新增表：
    - `schema_migrations`
    - `save_slots`
    - `save_snapshots`
  - 保留 `GameState` 的 JSON 快照存储方式
  - 默认槽 `default`（显示名：`主存档`）
  - 启动/读档时自动兼容迁移旧版 `user://savegame.json`
  - 现有按钮与快捷键无感切换到底层 SQLite
- 不包含：
  - 多存档槽 UI
  - 存档列表页与删除/重命名槽位界面
  - 资源流水表、事件日志表
  - 云存档与联网同步
  - 将 `GameState` 完全拆为关系表

## 实现拆解

1. 补充系统规格中的 SQLite 存档规则
2. 引入 SQLite 依赖并新增迁移器与仓库层
3. 将 `SaveSystem` 改为 SQLite 默认槽写入与读取
4. 增加旧版 JSON -> SQLite 迁移
5. 用现有 UI 与快捷键完成回归验证

## 验收标准（可测试）

- [ ] 点击主界面“存档”后会在 `user://countyidle.db` 写入默认槽快照
- [ ] 点击主界面“读档”可从 SQLite 默认槽恢复 `GameState`
- [ ] 快速存档、快速读档继续可用，无需 UI 改动
- [ ] 若数据库不存在但存在旧版 `user://savegame.json`，首次读档可自动迁移到 SQLite
- [ ] 旧版 JSON 迁移后不破坏 `GameState` 兼容
- [ ] `schema_migrations / save_slots / save_snapshots` 三张表可成功创建
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：SQLite 原生库在 Godot 运行时导出场景下可能出现打包差异；若一次性做多槽 UI，风险会明显上升。
- 回滚方式：保留旧 JSON 读取兼容；若 SQLite 运行期异常，则临时回退到 JSON `SaveSystem`，数据库文件保留作后续排查。
