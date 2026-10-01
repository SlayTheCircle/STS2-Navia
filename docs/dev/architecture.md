# 架构与约定

## 注册体系（属性式自注册）

- 全部内容挂在 RitsuLib 特性上：`[RegisterCard(typeof(NaviaCardPool))]` / `[RegisterPower]` / `[RegisterRelic(typeof(NaviaRelicPool))]` / `[RegisterPotion(typeof(NaviaPotionPool))]` / `[RegisterCharacter]`。
- 三个池是 `TypeListCardPoolModel` 系（成员自动聚合，`GenerateAllCards` 被密封，**不需要也不允许**改池文件加内容）。
- `ModEntry.Init()` 做四类接线：`ModTypeDiscoveryHub.RegisterModAssembly`（归属声明，必第一条）、关键词注册（LOAD/SALVO/SUPPORT）、角色资产档案 `RegisterCharacterAssetReplacement`（复合条目键 `sts2_navia_character_navia`）、显式 Harmony PatchAll。

### 变体 Loader 与游戏模型发现

工坊候选布局由根目录 Loader 选择并加载 `lib/game-<目标>/STS2-Navia.dll`，PCK 由根清单的 `has_pck` 走游戏原生挂载。根壳和内容 DLL 的程序集身份必须不同，以避免同一个 AssemblyLoadContext 内同名冲突。

RitsuLib 的 `RegisterModAssembly` 建立库内的发现与归属关系；不能假定它已经完成游戏模型扫描所需的程序集登记。当前库在发现管线中尝试调用原生关联 API，但 0.107.1 没有该 API。`GameAssemblyRegistration` 显式维护游戏侧登记：0.111.0 经原生 `ModManager.AssociateAssemblyWithMod` 追加内容程序集；0.107.1 经 `OnModDetected` 在加载完成后将单个 `Mod.assembly` 槽改为内容程序集。旧版初始化器返回后会写入根壳，提前改槽会被覆盖。事件只处理本次加载的 Mod 实例，错误加载不暴露内容。

若内容 DLL 未进入游戏侧登记，`ReflectionHelper.ModTypes` 仍只扫描根壳，ModelDb 不会创建角色、卡牌与池模型；RitsuLib 随后解析已登记角色会抛 ModelNotFoundException。这已在 0.107.1 启动日志中确认。验证入口与证据边界见 [Loader 验证](../../tests/LoaderProbe/README.md)，游戏内状态见 [STATUS](../../STATUS.md)。

## 复合 ID（本地化与条目键的唯一约定）

`STS2_NAVIA_{CATEGORY}_<类名蛇形大写>`。卡 `.title/.description`；可见 Power 三件套 `.title/.description/.smartDescription`；遗物三件含 `.flavor`；药水两键；关键词 `STS2_NAVIA_KEYWORD_*`；角色 16 键。
PowerVar 占位符例外：`{LoadPower:diff()}` 用**类名**不经前缀。
新表可随时并入 pck 合并路径（`localization/<lang>/<table>.json`，如 card_selection/events）。

## Base 家族（新内容一律继承，勿直接继承 RitsuLib 模板）

| 基类 | 职责 |
|---|---|
| `Content/Cards/NaviaCardBase` | 卡图按类名解析 `images/cards/<类名>.png`，缺图回退 RitsuLib 占位；4 参构造转发 |
| `Content/Relics/NaviaRelicBase` | 遗物三槽（主图/`_outline` 描边/大图复用主图）——**缺槽 = 游戏 NOPE 缺图纹理** |
| `Content/Potions/NaviaPotionBase` | 药水双槽（主图+描边） |
| `Content/Powers/NaviaPowerBase` | 可见 Power 图标按类名解析；隐藏 Power（内部标记）直接继承 `PowerModel` |

## 主题色（金色）

三池 `EnergyColorName="navia"`；`NaviaCardPool.PoolFrameMaterial = MaterialUtils.CreateHsvShaderMaterial(0.15f, 1.2f, 1.2f)`（vanilla 全框共用底图纯 HSV 参数调色，红 .025/橙 .12/绿 .32/蓝 .55/粉 .965 校准）；能量球 `navia_energy_big.png`(256²)+`navia_energy_text.png`(24²，**文本内联原尺寸渲染，大了撑爆行**)；`DeckEntryCardColor E8B23A / EnergyOutlineColor 8A6210`；哲学家事件选项键 `COLORFUL_PHILOSOPHERS...options.NAVIA`。

## 关键机制实现位

- 装填：`Content/Powers/LoadPower.cs`（上限 6/9，`Gain()` 唯一入口，每 2 层 +1 格挡=敏捷等价但不可偷取）
- 礼炮轰鸣：`Content/Mechanics/Salvo.cs`（无炮生成/有炮全体加伤）
- 摩拉计数：`ShiningMora.CountMoraPlayed`（每 3 张装填 1；监听方挂 `MoraCounterPower` 的 `AfterPowerAmountChanged`）
- 角色视觉：`Content/Visuals/NaviaCharacterVisuals.cs`（四态换装/浮动/受击/技能姿态/开局预热）+ **宿主=刺玫会徽常驻钩子**（AfterObtained 预热 / AfterPlayerTurnStart 刷新 / AfterDamageReceived 受击 / AfterDeath 倒下 / AfterCardPlayed 技能姿态）
- 预览感知三件套与升级键名规范：见 [卡牌手册](cards.md) §4/§6（**伤害=ExtraDamage，格挡=CalculationExtra**）

## 角色资产档案（ModEntry 内逐字段有注释）

17 个 CharacterModel 推导路径已全量对账覆盖；能量条/开局转场/出牌轨迹暂借铁甲资产（`fade_transition_mat.tres` / `card_trail_ironclad.tscn` / `ironclad_energy_counter.tscn`）。场景件在 `assets/STS2-Navia/scenes/characters/`（文本 .tscn 直接入包，脚本按 `res://src/Core/...` 路径引用游戏本体 C# 类）。

## 目录与职责

| 位置 | 职责 |
|---|---|
| src/STS2-Navia/ModEntry.cs | 注册与资产替换的组合入口、显式补丁应用 |
| src/STS2-Navia.Loader/ | 变体选择与初始化器转发、游戏程序集登记兼容 |
| Content/Cards、Powers、Mechanics、Enchantments | 卡牌、状态与共用战斗机制 |
| Content/Relics、Potions 及各池 | 常驻内容、药水及获取／解锁过滤 |
| Content/Events、Timeline | 事件、剧情纪元及角色联动 |
| Content/Characters、Visuals | 角色定义、资产槽与运行视觉 |
| localization/ | 双语运行文本、关键词、事件与先古对话 |
| assets/ | 文本场景／工程公开，媒体和导入产物本地维护 |
| scripts/、tools/ | 环境、检查、素材派生、构建、打包与部署 |
| docs/design、dev、history | 设计意图、当前工程契约、技术历史 |

配置位置和依赖恢复见[构建管线](pipeline.md)；各内容模块入口见[开发索引](README.md)。内容依赖共用机制，由 ModEntry 组合注册；构建不把游戏或依赖 DLL 随本模组复制分发。公开文档和检查不得依赖维护者的本机资料。

## 模块边界与目录演进

模块化、解耦、禁止上帝文件／类／函数和优先收纳过多平铺文件的要求由[工程规范](style.md)维护。目录表说明当前布局，不要求同类内容永远平铺在一层；新增内容前按稳定职责判断是否需要子目录。

目录与命名空间的职责是组织代码，模型类名、复合 ID、资源名和序列化身份承担运行时契约。纯目录重构应保留后者；需要改变身份时另行说明迁移与兼容范围。注册发现与代码引用也须检查，不能假定移动文件天然没有影响。

Cards 已按[卡牌目录组织](../history/design/card-organization.md)收纳：Basic、Ancient、Tokens 集中存放特殊卡；普通池按 Attacks／Skills／Powers，再按 Common／Uncommon／Rare 分组。NaviaCardBase 保留在根目录。文件位置用于导航，卡牌命名空间仍为 NaviaMod.Content.Cards，代码调用方沿用同一类型身份。

audit-roster、audit-placeholders、audit-loc-coverage 已递归枚举卡牌，audit-assets 对内容家族使用递归扫描；占位符检查找不到对应卡牌类时报告错误。SDK 项目使用默认递归 Compile 包含，资源仍按类名取用，不随源码目录分组移动。

Powers 等其他内容目录尚未完成同类收纳；audit-loc-coverage 对这些目录仍按直属文件枚举。引入其他内容子目录时必须同步检查对应扫描范围，避免模型、本地化和资源漏检。
