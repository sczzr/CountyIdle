# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图调度条表现层 GDScript 第二十七批回收
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`MainMapOperationalLink`

## 目标与用户收益

- 目标：在地图调度条 tone reset 已由 `WorldPanelVisualFx.gd` 承接的前提下，继续回收 `MainMapOperationalLink.cs` 中“无状态 / 行隐藏”两处重复的收尾逻辑。
- 玩家可感知收益（10 分钟内）：地图调度条外观与行为保持不变，但 `C#` 的状态分支更清晰，后续维护不容易把重复 reset 分支改出差异。

## 实现范围

- 包含：
  - 将 `MainMapOperationalLink.cs` 中两处重复的“隐藏行并 reset tone”逻辑收口到单一 helper
  - 保持地图调度条 tone reset 继续由 `WorldPanelVisualFx.gd` 承接
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改地图态势快照、按钮动作、tooltip 文案与禁用逻辑
  - 不改 `WorldPanelVisualFx.gd` 的 tone 语义与按钮表现参数
  - 不改 `GameState`、地图选择链与小时结算

## 实现拆解

1. 复查地图调度条在“无状态 / 行隐藏”分支中的重复收尾逻辑
2. 抽出单一 helper 收口“隐藏行并 reset tone”
3. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `MainMapOperationalLink.cs` 不再保留两处重复的地图调度条 reset 分支
- [ ] 地图调度条 tone reset 继续由 `WorldPanelVisualFx.gd` 单向承接
- [ ] 地图态势、按钮、tooltip 与指令逻辑保持不变
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 helper 收口时遗漏分支，地图调度条在个别状态下可能出现未及时隐藏或未重置色调
- 回滚方式：恢复 `MainMapOperationalLink.cs` 中本批合并前的两处分支写法即可
