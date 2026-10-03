# 世界线与原版角色联动

原案叙事见[世界线设计](../history/design/worldline.md)与[先古对话](../history/design/ancient-dialogues.md)。当前代码位于 [NaviaStory](../../src/STS2-Navia/Content/Timeline/NaviaStory.cs) 及四个 Epoch；注册条件挂在 [Navia](../../src/STS2-Navia/Content/Characters/Navia.cs) 上。

## 四章与三池门控

故事与纪元**必须继承 RitsuLib 脚手架基类**：NaviaStory 继承 ModStoryTemplate（Id 由 StoryKey 推导，章节顺序来自 RegisterStoryEpoch 的绑定而非手写数组）；四章分别继承 ModEpochTemplate 及 Relic／Potion／CardUnlockEpochTemplate（UnlockText 与 QueueUnlocks 由模板提供，每章解锁物恰好三件以满足原版展示文案读取前三项）。原版时间线屏幕的槽位合并按 `is ModEpochTemplate` 过滤，直接继承 EpochModel 会出现"解锁链照常触发但整列不渲染"——该契约不可绕过。每章 Id 为 STS2_NAVIA_EPOCH_1 至 _4，StoryId 为 Navia。

| 章 | 当前揭示条件 | 奖励 | 时间线列 |
|---|---|---|---|
| 启程 | 完成一局娜维娅 | 剧情节点；角色安装后直接可选 | Blight2 |
| 荒疫 | 娜维娅首胜 | RosulaMusket、GoldenRibbon、Macaron | Flourish2 |
| 好奇心 | 娜维娅累计击败三个首领 | Fonta、RosulaWine、FirearmLubricant | Flourish3 |
| 尖塔的尽头 | 娜维娅进阶一通关 | RifledBarrel、ArmsDealer、ForcedBuyout | Invitation5 |

各章通过 AutoTimelineSlot 落位；模板密封了 Era/EraPosition，由布局注册表解析（列内首个空闲位置），不要手工覆写。奖励 UI 与内容可获得性分别接线：模板 QueueUnlocks 负责揭示界面；NaviaRelicPool.GetUnlockedRelics、NaviaPotionPool.GetUnlockedPotions 和 NaviaCardPool.FilterThroughEpochs 按对应纪元过滤内容，与纪元类的 *UnlockTypes 类型数组共用同一来源。

变更章节奖励时同时更新对应纪元的 UnlockTypes 数组、池过滤、两语言文本及验收场景。节奏与图像映射的最终设计确认见 [STATUS](../../STATUS.md)。

## 会徽与欧罗巴斯之触

StartingRelics 恒返回 RosulaEmblem。会徽使用 RegisterTouchOfOrobasRefinement(typeof(RosulaFragrance)) 注册获取时替换关系，野蔷薇采用 Starter 稀有度，不进入普通稀有掉落。

不要按“以前遇见过欧罗巴斯”的相遇记录更换开局遗物；该历史方案已作废，原因见[方案取舍](../history/decisions.md)。原版升级表对未接入角色有回退行为，验收必须实际取得欧罗巴斯之触并观察替换结果。

## 古老牙齿与先古卡

RegisterArchaicToothTranscendence 挂在初始特色卡上：

- [VolleyFire](../../src/STS2-Navia/Content/Cards/Basic/VolleyFire.cs)：铳弹齐射→枪炮轰鸣。
- [QuickReload](../../src/STS2-Navia/Content/Cards/Basic/QuickReload.cs)：快速装填→极速装填。

极速装填（原「神速装填」）是原案之外的新增内容，当前数值与待确认范围见[对照说明](../history/design/implementation-notes.md)。修改初始卡、转化目标或稀有度时检查原版遗物调用路径与先古池的结果。

## 先古对话与资源

RitsuLib 根据 localization/{lang}/ancients.json 自动组装对话。键族为 <先古Entry>.talk.STS2_NAVIA_CHARACTER_NAVIA.<序号>-<行号>.char／ancient，续接键使用 .next。连续编号与说话者后缀须符合解析约定；改变对话分组时中英同时维护。`.next` 是**逐行键**：每轮除末行外各行都要有，缺失时游戏会把键名原文回显在界面角落（2026-10-01 修复的存量缺口）。**行号统一带 `r` 后缀**（同轮全行一致，含 .next）：全族对话皆为可重复——轮次精确匹配用尽后从已解锁轮次随机重放，与原版行为一致（2026-10-01 设计者定案）。**同轮变体**：多段对话可共享同一 VisitIndex，游戏在候选中随机挑选一段播放（坦克斯两段即此形态，2026-10-01 设计者确认）；序号>0 的变体段需 `<序号>-visit` 控制键显式回指轮次（如 `1-visit`=0），否则会被默认映射成后续轮次。建筑师终局（THE_ARCHITECT）走同一机制：对话序号即登顶轮次，另有可选的 -visit／-attack／-startattack／-endattack 键控制轮次与攻防编排（缺省为轮次顺延、结尾建筑师动作）；无键时 RitsuLib 会以空对话兜底并告警。拜访语义：各先古按「角色×先古」计次，但**整个存档的首次遇见（任意角色）由全角色共享的 firstVisitEver 通用台词占用**——纯新档上各族的第 0 轮因此不可达，与原版角色行为一致；第 N 次遇见显示 VisitIndex=N-1 的轮次。

当前覆盖 Neow、Darv、Pael、Orobas、Tezcatara、Nonupeipe、Tanx、Vakuu 与建筑师终局，与原案逐句对齐（2026-10-01 全族审计）。

纪元肖像是**两个独立资源槽**：大图按全局 res://images/timeline/epoch_portraits/sts2_navia_epoch_<序号>.png 推导（成品放 assets/global/，由 pack_mod.gd 保留全局路径，不能套用普通 Mod 前缀）；缩略图原版走 epoch_atlas 图集、mod 无图集条目会显示 NOPE，须经各纪元类的 AssetProfile.PackedPortraitPath 覆盖指向 res://STS2-Navia/images/timeline/ 下的派生图（scripts/art/stories.sh 从大图裁切 272×174）。启程章立绘与三张原案世界线图均已接入。

## 验收边界

分别确认新档与已有档的角色可用性、揭示条件、奖励 UI、三池过滤、纪元图、先古对话、欧罗巴斯之触和古老牙齿。调试揭示或控制台 ancient <Entry> 有助于直达画面与结果，仍需自然进度验证门控。

本页说明当前接线；文档整理不新增游戏验收证据。既有验收与待复验项由 STATUS 维护。
