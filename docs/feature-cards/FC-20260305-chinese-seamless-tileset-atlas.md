# 功能卡（Feature Card）

## 功能信息

- 功能名：中式无缝贴图 Atlas 资源交付（房屋/环境分组）
- 优先级：`P1`
- 目标版本：`v0.x`
- 关联系统：`资源管线 / 地图绘制`

## 目标与用户收益

- 目标：将已产出的无缝贴图整理为 Godot 可直接加载的 TileSet Atlas，降低地图搭建成本。
- 玩家可感知收益（10 分钟内）：开发侧可更快完成房屋与环境拼装，场景风格统一且无缝过渡。

## 实现范围

- 包含：
  - 生成房屋 Atlas 与环境 Atlas（256 网格）
  - 生成对应 `TileSet.tres` 资源（按房屋/环境分组）
  - 输出 Atlas 映射 JSON 与使用说明
- 不包含：
  - 新增玩法机制或数值改动
  - 新建 TileMap 场景与关卡摆放
  - 改动存档结构

## 实现拆解

1. 汇总两批无缝贴图并按 house/env 分类
2. 打包 Atlas 纹理并生成 Godot TileSet 资源
3. 产出映射文档并完成本地构建验证

## 验收标准（可测试）

- [x] 输出 `house/env` 两张 atlas 纹理并可见完整 tile 网格
- [x] 输出 `ChineseHouseTileSet.tres` / `ChineseEnvTileSet.tres`
- [x] 输出 JSON 映射（tile 名称与 atlas 坐标）
- [ ] Godot 编辑器中加载 TileSet 并可绘制（需人工打开编辑器确认）
- [x] `dotnet build .\\Finally.sln` 通过

## 风险与回滚

- 风险：`TileSet.tres` 文本配置在不同 Godot 小版本下可能存在兼容细节差异。
- 回滚方式：删除 `assets/tiles/atlas` 目录并回退本次新增资源文件。
