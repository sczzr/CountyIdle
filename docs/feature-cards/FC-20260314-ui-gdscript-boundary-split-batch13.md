# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第十三批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`DisciplePanel`

## 目标与用户收益

- 目标：把 `DisciplePanel.cs` 中剩余的大块纯展示控件 `DiscipleRadarChart` 从 `C#` 下放到独立 `GDScript`，让弟子谱业务脚本进一步收口到名册数据、筛选排序和详情文本。
- 玩家可感知收益（10 分钟内）：弟子谱右侧雷达图外观与当前一致，但后续微调坐标轴、描边和面板展示时，不需要再进入 `C#` 业务代码。

## 实现范围

- 包含：
  - 新增独立 `DiscipleRadarChart.gd` 承接弟子谱雷达图绘制与轴标签布局
  - 在 `DisciplePanel.tscn` 中补上正式雷达图节点挂载
  - `DisciplePanel.cs` 改为只向雷达图传递展示数据，不再直接承担自绘逻辑
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改弟子名册生成、筛选排序、属性计算与文案解释逻辑
  - 不改 trait tag 的当前最小内联样式策略
  - 不改存档、小时结算、弟子规则与治理逻辑

## 实现拆解

1. 复核 `DisciplePanel.cs` 内嵌 `DiscipleRadarChart` 是否只承担展示职责
2. 新增 `GDScript` 雷达图控件并挂接到 `DisciplePanel.tscn`
3. 由 `DisciplePanel.cs` 保留统计值计算，改单向调用雷达图脚本刷新展示
4. 构建验证并回写看板状态

## 验收标准（可测试）

- [ ] `DisciplePanel.cs` 不再包含内嵌的雷达图自绘控件实现
- [ ] `DiscipleRadarChart.gd` 已承接轴线、环线、多边形与标签布局绘制
- [ ] `DisciplePanel.cs` 仍保留属性数据计算、筛选排序和详情文案
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：若 `C# -> GDScript` 数据格式不匹配，雷达图可能显示为空；若场景挂载点不对，右侧基础面板会缺失图形控件
- 回滚方式：恢复 `DisciplePanel.cs` 中的内嵌雷达图类，并移除新加的 `DiscipleRadarChart.gd` 场景挂载
