# CountyIdle 代码结构指南

本文目标：用最短路径定位代码、资源与数据入口，便于快速接手或扩展功能。

## 仓库顶层结构

| 位置 | 作用 |
| --- | --- |
| `AGENTS.md` | 协作规范与文档驱动规则（改动前必读）。 |
| `Finally.sln` | .NET 解决方案入口。 |
| `CountyIdle/` | Godot 项目主体目录。 |
| `docs/` | 设计与流程文档（规则与开发流的事实源）。 |
| `tools/` | 辅助脚本与工具（如存档烟测）。 |
| `chat/`, `_codex_obj/` | 辅助产物与会话缓存（非业务代码）。 |

## CountyIdle 项目结构（重点目录）

| 位置 | 作用 | 备注 |
| --- | --- | --- |
| `CountyIdle/project.godot` | Godot 工程入口配置 | 运行时由 Godot 读取。 |
| `CountyIdle/Main.tscn` | 主场景 | 项目主入口场景。 |
| `CountyIdle/scripts/Main.cs` | 主场景脚本 | 仅负责 UI/交互组织与调度。 |
| `CountyIdle/scenes/` | 场景文件 | 以 `.tscn` 为主。 |
| `CountyIdle/scenes/ui/` | UI 场景集合 | 各面板、弹窗 UI 布局。 |
| `CountyIdle/scripts/core/` | 主循环与系统调度 | `GameLoop`、存档、事件总线。 |
| `CountyIdle/scripts/models/` | 数据模型与枚举 | `GameState` 及配置模型。 |
| `CountyIdle/scripts/systems/` | 玩法系统逻辑 | `*System.cs` 与 `*Rules.cs`。 |
| `CountyIdle/scripts/ui/` | UI 逻辑 | 面板脚本与 UI 交互。 |
| `CountyIdle/scripts/ui/gd/` | UI 视觉特效 GDScript | 过渡、动效、特效。 |
| `CountyIdle/scripts/map/gd/` | 地图相关 GDScript | 地图交互、视觉绑定。 |
| `CountyIdle/data/` | 静态配置 JSON | 建筑、物品、职业、地图等。 |
| `CountyIdle/assets/` | 美术/音频资源 | `audio/`, `characters/`, `map/`, `ui/`。 |
| `CountyIdle/themes/` | UI 主题资源 | `HanCourtyardTheme.tres`。 |
| `CountyIdle/addons/` | Godot 插件 | 当前包含 `rmlui`。 |
| `CountyIdle/.godot/` | Godot 自动生成 | 非必要不修改。 |

## 核心运行链路（读代码主线）

1. Godot 打开 `project.godot`，入口场景为 `Main.tscn`。  
2. `Main.cs` 负责 UI/交互组织与对 `GameLoop` 的调用。  
3. `GameLoop` 驱动时间推进：`1 秒现实时间 = 1 游戏分钟`，`60 分钟`触发一次小时结算。  
4. `GameLoop` 维护 `GameState`，调用各玩法 `System`，并通过 `EventBus` 发布状态与日志。  
5. UI 层订阅事件刷新面板与日志显示。  
6. 存档由 `SaveSystem` 统一管理，底层使用 SQLite（`SqliteSaveRepository`）。  

## 关键代码入口

| 入口 | 说明 |
| --- | --- |
| `CountyIdle/scripts/core/GameLoop.cs` | 游戏主循环与系统调度中枢。 |
| `CountyIdle/scripts/models/GameState.cs` | 全量运行态数据与迁移逻辑。 |
| `CountyIdle/scripts/core/SaveSystem.cs` | 存档/读档入口与自动存档策略。 |
| `CountyIdle/scripts/core/EventBus.cs` | UI/系统之间的事件发布通道。 |
| `CountyIdle/scripts/systems/*System.cs` | 玩法系统执行逻辑。 |
| `CountyIdle/scripts/systems/*Rules.cs` | 规则计算与边界处理。 |
| `CountyIdle/scripts/ui/*.cs` | UI 面板行为与交互。 |
| `CountyIdle/scenes/ui/*.tscn` | UI 布局与节点结构。 |

## 常见修改定位指南

| 需求类型 | 优先入口 |
| --- | --- |
| 规则/公式调整 | `docs/02_system_specs.md` → `scripts/systems/*Rules.cs` |
| 玩法逻辑增补 | `scripts/systems/*System.cs` → `GameLoop` 调度 |
| UI 布局改动 | `scenes/ui/*.tscn` |
| UI 行为改动 | `scripts/ui/*.cs` 与 `scripts/ui/gd/*.gd` |
| 地图生成/规则 | `scripts/systems/*Map*System.cs` 与 `scripts/models/*Map*.cs` |
| 静态配置 | `data/*.json` |
| 存档/读档 | `scripts/core/SaveSystem.cs` 与 `SqliteSaveRepository.cs` |

## 代码约束提醒（来自协作规范）

1. 玩法计算放在 `systems`，`Main.cs` 只负责 UI/交互组织。  
2. `GameState` 数据要保持非负、可结算、可存档。  
3. UI 发布状态保持克隆副本（`Clone()` 模式）。  
4. 非必要不修改 `.godot/` 与 `*.cs.uid` 生成文件。  
5. 改动前先对齐 `docs`，遵守文档驱动流程。  
