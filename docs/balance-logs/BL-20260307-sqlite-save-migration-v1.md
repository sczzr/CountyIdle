# 数值平衡日志（Balance Log）

## 记录信息

- 日期：2026-03-07
- 版本：v0.x
- 关联提案：`docs/change-proposals/CP-20260307-sqlite-save-migration-v1.md`

## 改动摘要

- 改动项：将本地存档层从单文件 JSON 升级为 SQLite 默认槽 + 快照模型
- 改动前：`GameState` 直接写入 `user://savegame.json`
- 改动后：默认写入 `user://countyidle.db`，旧版 JSON 在首次读档时可迁移到 SQLite

## 结果数据

- 指标 1：存档介质 `JSON 文件 -> SQLite 数据库`
- 指标 2：默认槽数量 `0 -> 1`（`default / 主存档`）
- 指标 3：快照表数量 `0 -> 1`
- 指标 4：旧版存档兼容状态 `支持 JSON 迁移`

## 结论

- 是否达到预期：`部分`
- 下一步：`继续调参`

## 复盘

- 有效原因：采用“SQLite 外壳 + JSON 快照内核”能最大限度减少对现有 `GameState` 的侵入。
- 无效原因：当前仍未进入多槽 UI 与历史流水阶段，`save_slots` 的能力尚未完全对玩家可见。
- 后续假设：若默认槽链路稳定，可在下一阶段补充存档列表 UI 与自动存档槽。
