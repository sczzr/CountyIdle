# 功能卡（Feature Card）

## 功能信息

- 功能名：留影录画屏归档升级（玉简档案架 + 展卷详述）
- 优先级：`P1`
- 目标版本：`当前迭代`
- 关联系统：`SaveSlotsPanel / SaveJadeSlipItem / SavePreviewCrossfade.gd / MainSaveSlotsPanel / SaveSystem`

## 目标与用户收益

- 目标：将已书卷化的留影录继续升级为“左侧玉简档案架，右侧展开画卷”的归档体验，并统一动作文案与选中反馈。
- 玩家可感知收益（10 分钟内）：切换存档时会更像翻阅宗门旧卷，而不是操作普通列表；更容易通过玉简层次、画屏留影与横排关键摘要快速分辨不同阶段的存档。

## 实现范围

- 包含：
  - 将左侧槽位列表从 `ItemList` 升级为可滚动的玉简档案架，自定义选中/hover 反馈
  - 将右侧详情拆成“卷名 / 最近落卷 / 留影画屏 / 横排关键摘要 / 操作印章”结构，并对齐参考页面的留白比例
  - 底部主动作按参考页收口为 `焚毁 / 另拓 / 覆写 / 启读`，同时保留 `天道刻印` 自动槽语义
  - 扩展 `SavePreviewCrossfade.gd`，承接留影录卷轴展开、详情淡入与按钮主题外观
  - 二次打磨画屏边缘的水墨晕染、玉简类型差异色调与印章按钮强化，并补 Godot 场景烟测
- 不包含：
  - 不改 `SQLite` 存档结构、自动存档轮换策略与截图写入逻辑
  - 不新增云存档、导入导出或历史版本对比
  - 不改快速存档 / 快速读档默认走主卷的规则

## 实现拆解

1. 为留影录新增玉简条目组件，替代旧 `ItemList` 视觉承载
2. 重构 `SaveSlotsPanel.tscn`，把右侧详情拆成画屏卷轴布局并补齐参考页式统计行节点
3. 更新 `SaveSlotsPanel.cs`，保持权威逻辑在 `C#`，改为驱动自定义玉简列表与结构化详情文案
4. 扩展 `SavePreviewCrossfade.gd`，统一卷轴静态主题与切卷动效
5. 对画屏边框、印章按钮与自动/主卷玉简做第二轮视觉打磨
6. 回写 `docs/05_feature_inventory.md`、`docs/08_development_list.md` 并执行 `dotnet build .\Finally.sln` 与 Godot 场景烟测

## 验收标准（可测试）

- [ ] 左侧存档列表已升级为玉简档案架，可滚动浏览并对选中项做浮现/高亮反馈
- [ ] 右侧详情已升级为展开画卷结构，包含截图画屏、卷名、落卷时间、参考页式横排关键摘要与印章式按钮
- [ ] 底部主动作已对齐为“焚毁 / 另拓 / 覆写 / 启读”，并继续区分自动槽 `天道刻印`
- [ ] 画屏边缘已有额外水墨晕染/落款强化，主卷 / 天道刻印 / 手录分卷的玉简气质能被直接区分
- [ ] `SaveSlotsPanel.cs` 继续只负责筛选、选中、按钮可用性与事件转发，不把存档逻辑下放到 `GDScript`
- [ ] `dotnet build .\Finally.sln` 与 Godot 场景烟测通过

## 风险与回滚

- 风险：卷轴布局层级较深，若后续场景节点路径再变动，`SavePreviewCrossfade.gd` 与 `SaveSlotsPanel.cs` 都需要同步调整；自定义玉简组件若尺寸约束不稳，低分辨率下可能需要继续微调间距。
- 回滚方式：保留 `SaveSystem` / `MainSaveSlotsPanel` 主链，只回退 `SaveSlotsPanel.tscn`、`SaveSlotsPanel.cs`、`SavePreviewCrossfade.gd` 与 `SaveJadeSlipItem` 组件即可。
