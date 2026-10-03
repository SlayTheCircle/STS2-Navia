# 事件批静态审查(2026-09-29)

调查日期 2026-09-29，游戏目标 0.111.0，RitsuLib 0.6.2。保留注册、本地化、门控与补丁通道调查。参考工程用于核对 API；本模组接线的调查结果见各项记录。

历史勘误：第六项漏查本模组初始化器；加载器日志不能独立证明原版追加选项已应用。素材状态、行号和数量为调查时快照。本轮未重做游戏验收；现行实现见[事件手册](../../dev/events.md)，当前证据见 [STATUS](../../../STATUS.md)。

## 一、参照系盘点

| 参照 | 结果 |
|---|---|
| Hikari 1.0.0(已发布可用) | **无自定义事件**(类型清单只有卡/角色内容)。可比项三件:①Entry 公式第三方旁证——loc 键 `HIKARI_KEYWORD_*`、`HIKARI_CHARACTER_HIKARI_CHARACTER.*` 与 `{MODID}_{CATEGORY}_{TYPENAME}` 完全一致;②**events 表合并实证**——其 events.json 仅一条 `COLORFUL_PHILOSOPHERS.pages.INITIAL.options.HIKARI.*` 键,在已发布模组中实际渲染(哲学家选项);③pck 结构/manifest 纪律与我们同构 |
| 当时参考样本 | 当时检查的 RitsuLib 参考实现未提供自定义事件先例；结论需回到 RitsuLib API 与游戏机制，不将本机样本当成社区完整统计 |
| Act4Heart | 独立框架(Dolso):`[ModInitializer("Initialize")]` + 自建 Harmony 通道在野可用——与我们走的「无初始化器→官方自动 PatchAll」构成**双通道互证**:mod dll 里打 Harmony 补丁在本游戏完全可行 |
| Watcher / BaseLib | 无事件实现,不构成参照 |

## 二、当时逐环核对与勘误

1. **注册链**:`[RegisterActEvent(typeof(Act))]` → `RegisterScopedModel(RegisteredActEvents)`(ModContentRegistry.cs)
   → RitsuLib `DynamicActContentPatcher` 对每个 ActModel 子类的 `AllEvents` getter 打 postfix 调
   `AppendActEvents` 合入(Bootstrap 打在 ModelDb.Init);vanilla `ActModel.cs:334 AllEvents.Concat(...)` 消费。逐行读过。
2. **Entry 公式**:`GetFixedPublicEntry = {MODID}_{CATEGORY}_{TYPENAME}`;`SlugifyCategory("EventModel")` 剥
   `_MODEL` 后缀 → `EVENT`;得 `STS2_NAVIA_EVENT_<类名蛇形>`。第三方旁证见 Hikari 行。
3. **五原版事件 Entry 对账**:`StringHelper.Slugify` = 驼峰断词加下划线再大写——TeaMaster→TEA_MASTER、
   StoneOfAllTime→STONE_OF_ALL_TIME、Trial→TRIAL、SpiralingWhirlpool→SPIRALING_WHIRLPOOL、Bugslayer→BUGSLAYER,
   补丁文件里的字面键全部与之一致。
4. **loc 对账(机械)**:代码字面键 10 个(五选项前缀+五结果页)、推导键 29 个(三事件 title/INITIAL/选项/结果页),
   zhs+eng 双语 0 缺失;选项键为前缀,游戏自拼 `.title/.description`,已按前缀+后缀复核。
5. **解锁过滤地雷(已排)**:`GeneratedRoomEventUnlockFilterPatch` 剔除「锁定」mod 事件,但
   `ModUnlockRegistry.IsUnlocked` 对无注册要求的模型 `TryGetValue` 失手即 `return true`——我们的三事件
   无解锁要求,缺省已解锁,不会被剔池。
6. **Harmony 通道(本项审查有误,2026-09-29 实测勘误)**:审查时断言「我们的 dll 无 ModInitializerAttribute,
   官方自动 PatchAll 一直在生效」——**错**。ModEntry 自立项起就带 `[ModInitializer("Init")]`(RitsuLib 归属声明需要),
   官方语义是初始化器与自动 PatchAll **二选一**,故自动补丁从未跑过,五选项扩充全部失效。机制本身(源码)没读错,
   错在没 grep 自家程序集就断言自身状态。修复 = Init() 内显式 PatchAll，参考带初始化器的补丁接线方式。
   引以为鉴:**引用外部机制做静态结论时,自家状态也要对账**——本审查对 RitsuLib 链路逐环核实,却漏了这条自检。
7. **事件处理器 API**:三事件+五扩充的每个命令都有 vanilla 实件出处(UnrestSite/AromaOfChaos/PotionCourier/
   Wellspring/ZenWeaver/SelfHelpBook/WaterloggedScriptorium/RanwidTheElder),出处写在各类 XML 注释里。

## 三、静态排除不了的残余风险(游戏内验收关注点)

- `{Heal}`/`{GiftGold}` 占位符在事件页 LocString 的实际渲染(与卡牌同管线,信心高但非零)。
- AssetProfile 占位:程序化 PNG 加载、复用原版背景 tscn(`unrest_site`/`round_tea_party`/`drowning_beacon`,
  全小写与 SceneHelper 拼法一致)的显示观感。
- **日志证据边界（公开整理时勘误）**：新事件注册日志证明对应发现／注册步骤；本模组带初始化器，自动 PatchAll 日志并非必要信号。需核对 Init 中的显式补丁应用，并实际观察五个追加选项与处理结果；不能仅凭两条加载器日志宣布整个链路验收完毕。
- **免运气验收通道(直达,2026-09-29 补)**:vanilla 自带 DevConsole,且 `NDevConsole.cs:121` 明确
  `ModManager.IsRunningModded()` 即放行全部调试命令——跑 mod 的玩家天然拥有完整控制台。**反引号(`)开台**,
  `event <Entry>` 直达任意事件(`EventConsoleCmd` 走 `ModelDb.AllEvents`,含 mod 事件;Tab 补全;绕过 IsAllowed,
  直达时门控不拦)。本批验收命令:
  `event STS2_NAVIA_EVENT_HOMETOWN_MEMORY` / `..._FOREIGN_BALL` / `..._HEAVY_RAIN`,
  `event STONE_OF_ALL_TIME` / `TRIAL` / `SPIRALING_WHIRLPOOL` / `BUGSLAYER` / `TEA_MASTER`,
  `event COLORFUL_PHILOSOPHERS`(顺带验 NAVIA 金色选项)。
  Loadout(BaseLib 系可视化面板)的 NEventSelectScreen 是等价的图形化替代,二选一即可。
