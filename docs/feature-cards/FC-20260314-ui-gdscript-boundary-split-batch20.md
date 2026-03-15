# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 scene-side 第二十批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`Main`

## 目标与用户收益

- 目标：把主界面背景 `TextureRect` 的静态视觉布局参数从 `Main.cs` 运行时配置回收到 `Main.tscn`，让主界面脚本只保留背景资源加载 fallback，而不再维护这些固定 UI 表现参数。
- 玩家可感知收益（10 分钟内）：主界面背景表现保持一致，但后续调整背景节点的静态视觉布局时不需要再改 `C#` 代码。

## 实现范围

- 包含：
  - 在 `Main.tscn` 中直写背景节点的 `expand_mode / stretch_mode / z_index / self_modulate`
  - 简化 `Main.cs` 中背景节点的静态视觉配置
  - 清理与 full-rect 锚定重复等价的背景 resize 绑定逻辑
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改背景资源 fallback 加载逻辑
  - 不改 Legacy/Figma 布局切换
  - 不改世界图 / 山门图 / 存档结构与小时结算

## 实现拆解

1. 复查 `Main.cs` 中背景节点仍由运行时维护的静态视觉参数
2. 将这些参数直接回收到 `Main.tscn`
3. 删除等价重复的背景 resize 绑定代码
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `Main.tscn` 已承接背景 `TextureRect` 的静态视觉布局参数
- [ ] `Main.cs` 不再运行时重复设置这些背景静态参数
- [ ] 背景 resize 绑定逻辑已清理而不影响 full-rect 布局
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：如果场景参数填写不完整，Legacy 主界面背景可能出现拉伸模式或层级异常
- 回滚方式：恢复 `Main.cs` 中本批删除的背景静态配置与 resize 绑定逻辑，并撤回 `Main.tscn` 的对应参数
