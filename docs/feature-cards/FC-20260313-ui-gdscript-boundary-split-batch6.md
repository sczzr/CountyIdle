# 功能卡（Feature Card）

## 功能信息

- 功能名：UI / 地图表现层 GDScript 第六批拆分
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`SaveSlotsPanel`

## 目标与用户收益

- 目标：继续将 `SaveSlotsPanel` 右侧详情列的空态 / 预览态过渡从 `C#` 下放到 `GDScript`，让留影与卷册详情切换更完整。
- 玩家可感知收益（10 分钟内）：切换卷册时，不只是预览图，右侧详情文本、题名行和操作按钮也会一起形成更完整的过渡反馈。

## 实现范围

- 包含：
  - 扩展 `SavePreviewCrossfade.gd`，接管详情列文本、题名行与按钮行的纯表现过渡
  - 保持 `SaveSlotsPanel.cs` 只负责槽位选择、详情内容填充与按钮可用性判断
  - 回写 `docs/05_feature_inventory.md` 与 `docs/08_development_list.md`
- 不包含：
  - 不改卷册筛选 / 排序、存读写、复制删除与命名逻辑
  - 不改 `GameState`、存档结构与小时结算公式
  - 不改预览图片加载规则

## 实现拆解

1. 扩展 `SavePreviewCrossfade.gd` 捕获详情列关键节点
2. 将 `transition_to_preview / transition_to_empty / pulse_on_select` 扩展为“预览框 + 详情列”联合表现
3. 保持 `SaveSlotsPanel.cs` 现有调用点不变，只复用已有单向调用边界

## 验收标准（可测试）

- [ ] 打开卷册面板并切换不同槽位时，右侧详情文本、题名行和按钮行会随预览区形成统一过渡
- [ ] 无留影或无可见卷册时，右侧详情列保持稳定空态，不闪烁
- [ ] `SaveSlotsPanel.cs` 不新增业务判断下放到 `GDScript`
- [ ] `dotnet build .\Finally.sln` 通过

## 风险与回滚

- 风险：详情列节点路径后续变更时，表现脚本可能失效；空态与有图态切换太频繁时，可能出现 tween 被打断
- 回滚方式：恢复 `SavePreviewCrossfade.gd` 到仅处理预览框版本即可
