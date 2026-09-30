# 素材规范

## 跟踪与来源

多媒体不入公开仓，不使用 LFS；母版与图像由组织私有美术仓承载，本地克隆经 `ART_SOURCE_DIR` 接入。文本 .tscn、.tres、.gd 和素材导入工程配置可随源码维护。PNG、母版、音视频、字体、编译纹理、导入缓存与侧车是本地输入或产物。美术授权为限定授权（见 [许可范围](../../LICENSING.md)），不随源码仓公开。

源码采用 MIT，素材的具体权利与分发仍单独确认，见 [许可范围](../../LICENSING.md)。完整包中的多媒体不因附带 LICENSE 而自动采用 MIT。

素材母版由 ART_SOURCE_DIR 定位，实际本机目录由私有导航维护。[prep-art](../../scripts/prep-art.sh)协调裁切、命名和派生；母版映射与各类生成职责位于 [scripts/art](../../scripts/art/)，assets 中的成品用于 Godot 导入和打包。普通 DLL 编译不要求美术；完整包构建要求全部资源存在。

## 尺寸与布局

| 类型 | 入包规格 | 路径 |
|---|---|---|
| 常规卡图 | 750×570，25:19；母版建议 1254×954 或等比 | images/cards/类名.png |
| 先古卡图 | 606×852，竖版 | images/cards/类名.png |
| 遗物 | 256²，主图及派生 _outline | images/relics/ |
| 药水 | 256²，主图及派生 _outline | images/potions/ |
| 可见 Power | 256²，供小／大图两个资源槽 | images/powers/ |
| 支援徽记 | 256²，透明底，统一徽记 | images/enchantments/navia_support.png |
| 能量图标 | big 256²，text 24² | images/energy/ |
| 战斗能量计 | 128² 图窗，复用 big 纹理 | scenes/combat/navia_energy_counter.tscn |
| 世界线立绘 | 1672×941 | 全局 images/timeline/epoch_portraits/ |
| 事件肖像 | 当前母版 1672×941，由事件图窗适配 | images/events/类名.png |
| 选人半身 | 当前手工母版 637×917，派生宽 264 | images/characters/navia_select.png 及 locked 变体 |

普通资源挂在 res://STS2-Navia/；世界线立绘按引擎推导使用全局路径。源图先由 Godot 导入，PCK 包含侧车和 .godot/imported 编译纹理；有侧车的源 PNG 不重复入包。

## 接入与验收

1. 对照母版、素材映射、代码和当前画面核对，不仅依赖旧需求单。
2. 补齐映射与派生逻辑，再生成素材。只覆盖生成文件可能在重跑时丢失。全量再生成要求所有已映射母版与角色／剧情输入齐全；缺件时在写入前失败，不以已有成品掩盖缺失。脚本保留已有输出与导入侧车，按字节更新生成结果；不清空资源目录，也不以母版 mtime 或成品已存在为理由跳过更新。未使用的旧资源需单独核对消费方再清理。
3. 输出剥离元数据并按字节比较，相同内容不替换；不得只依赖 mtime 判断母版变化。
4. 运行完整素材检查、构建与 PCK 验证，然后实测对应图窗、资源槽和动态效果。

能量主图母版当前为单张透明金玫瑰，费用图标与战斗能量计共用；不把文件名中的“分层”当作独立图层交付证据。能量计使用原版 NEnergyCounter，供齐 Label、Layers、RotationLayers、EnergyVfxBack、EnergyVfxFront 节点。纹理在旋转层中显示，耗尽变暗与数值更新由原版驱动；两侧粒子容器提供有效的粒子数组，获得能量时播放金色粒子。标准版 Godot 的 PCK 验证只证明纹理与场景文件入包，游戏 C# 生命周期和动效须在游戏内验收。

当前缺件与接线、验收状态由 STATUS 维护；公开的[美术贡献需求](../design/artwork.md)说明资源规格与贡献方式。母版位于组织私有美术仓，本地经 `ART_SOURCE_DIR` 定位，不随源码仓分发。占位图满足资源存在性时，仍需明确其美术状态。
