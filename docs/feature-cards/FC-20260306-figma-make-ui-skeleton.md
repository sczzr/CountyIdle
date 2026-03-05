# 功能卡（Feature Card）

## 功能信息

- 功能名：Figma Make 主界面 Godot 骨架落地
- 优先级：`P1`
- 目标版本：`v0.x`
- 关联系统：`UI`

## 目标与用户收益

- 目标：将 Figma Make 导出的五块主界面结构转为 Godot 可复用子场景与 C# 绑定脚本骨架。
- 玩家可感知收益（10 分钟内）：可在 Godot 中直接预览与迭代目标 UI 分区，后续接入数据与交互成本更低。

## 实现范围

- 包含：
  - 新增 `HUDTopBar / EquipmentPanel / CenterView / TimelinePanel / BottomBar` 五个子场景骨架
  - 新增对应 C# 脚本，提供基础节点绑定与占位数据填充
  - 新增一个组合场景，将五个子场景按 Figma 分区布局拼装
- 不包含：
  - `GameLoop`、`GameState`、`systems` 数值与机制改动
  - 存档格式改动
  - 替换当前 `Main.tscn` 的现有生产 UI 绑定

## 实现拆解

1. 新建文档记录并冻结本次范围
2. 创建子场景与脚本的最小可运行骨架
3. 组装总场景并确保 `dotnet build` 通过

## 验收标准（可测试）

- [ ] `scenes/ui/figma/` 下存在 5 个 Figma 对应子场景与 1 个组合场景
- [ ] `scripts/ui/figma/` 下存在对应 C# 脚本并可被场景引用
- [ ] 骨架场景可在 Godot 中打开且分区布局正确（顶部/左中右/底部）
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：命名接近既有 UI 资源，若误替换可能影响 `Main.cs` 节点路径绑定
- 回滚方式：删除本次新增的 `scenes/ui/figma/*` 与 `scripts/ui/figma/*` 文件
