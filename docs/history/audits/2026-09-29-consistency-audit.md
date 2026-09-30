# 卡面行为与描述一致性静态审查报告(2026-09-29)

> 历史快照：调查日期 2026-09-29，游戏目标 0.111.0，RitsuLib 0.6.2。保留当时的证据、建议与复核理由；源代码行号、文本和模型数量可能已变化。CONFIRMED 指当时复核，不代表当前仍有缺陷。
> 本轮仅整理文档，未逐项重做审计或核定全部修复状态。原文“无 high”仅表示当时静态审查未报告 high，不能推断游戏无崩溃。末尾“16 个 Power／6 个隐藏”为旧快照；当前数量及验收见 [STATUS](../../../STATUS.md)，现行写法见[卡牌手册](../../dev/cards.md)。

> 当时发现总计 51 条:medium 11 / low 18 / info 22;复核裁定 CONFIRMED 27 / DISMISSED 6 / 未复核直报 18(均为 low/info)。
> 当时静态审查未报告 high 级发现。装填／礼炮轰鸣解释文本经当时逐句核对通过；不由此推断游戏内无崩溃或严重数值错误。

## foundation(覆盖 18 项)

### [medium/CONFIRMED] ShiningMora — keyword
- **问题**: 卡面文案提到「装填」但 CanonicalKeywords 未挂 NaviaKeywords.Load，名词解释缺失且无 [gold] 高亮
- **位置**: `src/STS2-Navia/Content/Cards/ShiningMora.cs:33`
- **建议**: 在 CanonicalKeywords 中并入 NaviaKeywords.AddTo(set, NaviaKeywords.Load)(与 Exhaust/Retain 合并为 HashSet 返回),并把文案中「装填」改为 [gold]装填[/gold];eng 侧同步加 [gold]Load[/gold]。
- **复核补充**: 发现本身细节全部准确(行号、引文、对照卡、施工手册条款),suggestion 可直接执行,仅补充三点:(1) 修 ShiningMora 时把 CanonicalKeywords 改为 `HashSet<CardKeyword> set = new HashSet<CardKeyword> { CardKeyword.Exhaust, CardKeyword.Retain }; NaviaKeywords.AddTo(set, NaviaKeywords.Load); return set;`,并在 usings 加 `using NaviaMod.Content.Keywords;`;文案「每打出3张闪耀摩拉，装填1。」改为「每打出3张[gold]闪耀摩拉[/gold]，[gold]装填[/gold]1。」(闪耀摩拉作为卡牌名词,同 mod 内 PeddlingFirearms/Ext…

### [low/CONFIRMED] VolleyFire — mechanics
- **问题**: 消耗装填绕过 LoadPower.Gain 统一入口,直接 GetPower<LoadPower>()+PowerCmd.ModifyAmount
- **位置**: `src/STS2-Navia/Content/Cards/VolleyFire.cs:63`
- **建议**: 改为 `await LoadPower.Gain(choiceContext, creature, -load, creature, this);` 并删除判空块(GetPower 可省)。
- **复核补充**: 两处细节微修,不改结论:(a) 引文范围实为 VolleyFire.cs:57-65(非 57-64);(b) 证据中「vanilla PowerCmd.cs:160/249 显示 Apply 与 ModifyAmount 都触发 Hook.AfterPowerAmountChanged」表述略不精确——第 160 行位于 Apply 的实例重载(全新施加路径),当目标已有同名 power 时 Apply<T> 在第 88 行委托 ModifyAmount,钩子只从第 249 行触发一次;本场景 load>0 保证 power 已存在,两路径均经第 249 行,「监听不受影响」的结论不变。建议修法照抄 EmergencyEvade.cs:60:`if (load > 0) { await LoadPower.Gain(choiceContext, creature, -load, crea…

### [info/CONFIRMED] LoadPower — text-behavior
- **问题**: 「每有2层,视为拥有1点敏捷」仅在格挡加成语义上与 DexterityPower 等价,对按类型读取/偷取/清除敏捷的原版效果不可见
- **位置**: `src/STS2-Navia/Content/Powers/LoadPower.cs:54`
- **建议**: 实现可不动;如追求解释文本严格准确,可将「视为拥有1点敏捷」改为「每有2层,获得格挡时+1(效果同敏捷,但不会被偷取或移除)」。
- **复核补充**: 两处细节修正,不影响结论方向: (a) 文案实际是 6 处而非 4 处——zhs/eng 的 powers.json 各还有一条 smartDescription:「当前{Amount}层,每有2层提供1点敏捷」/"Currently {Amount} stacks: every 2 stacks grant 1 Dexterity",「提供/grant」比「视为/count as」更倾向真实给予,略加重过度承诺。(b) UnsettlingLamp 是五条引证中的弱例且定性不准: 它不是按类型读取敏捷的效果,而是对你卡牌施加给敌方的任意可见 debuff 翻倍(检查 power.GetTypeForAmount(amount)!=Debuff,非 `is DexterityPower`;DexterityPower 只出现在其防双吃的文档注释里);且全部 LoadPower.Gain 调…

### [info/CONFIRMED] LoadPower — placeholder
- **问题**: 类注释残留过时 TODO:「穿心膛线」实装后上限提升至 9——该卡已实装,上限 9 已生效
- **位置**: `src/STS2-Navia/Content/Powers/LoadPower.cs:17`
- **建议**: 把 TODO 改为陈述句(「穿心膛线(RifledBarrel)经 LoadCapUpPower 将上限提至 9」),避免误导后续维护者以为该链路未接通。
- **复核补充**: 无需修正行号与引文,全部精确。补充两点:(1) 修正建议可采纳,推荐措辞与 LoadCapUpPower.cs:9 既有陈述句风格保持一致,如「『穿心膛线』(RifledBarrel) 经 LoadCapUpPower 将上限提至 9(见 CapFor)」;(2) 同步微调 LoadPower.cs:16 的「部分卡牌可消耗装填」若后续有变动可一并复查,本次不涉及。纯注释级 info 问题,无需动态测试。

## defense(覆盖 8 项)

### [medium/CONFIRMED] HoldPosition — text-behavior
- **问题**: 选牌提示复用原版「选择1张牌来消耗」文案,但选中的牌并不会被消耗,只是获得消耗关键词,UI 文案与实际行为相反
- **位置**: `src/STS2-Navia/Content/Cards/HoldPosition.cs:43`
- **建议**: 为 CardSelectorPrefs 使用自定义 LocString(如 card_selection 表新增 GAIN_EXHAUST 键,文案「选择1张牌,使其获得[gold]消耗[/gold]。」),不要复用 TO_EXHAUST;或仿 vanilla 无先例时用 HoverTipFactory.Static 静态提示。
- **复核补充**: 发现主体成立,细节补充三点:(1) 触发条件——CardSelectCmd.cs L840-841 在「手牌恰好 1 张可选牌且 RequireManualConfirmation=false(Min==Max==1)」时会自动选中而不显示提示,故误导性标题只在手牌 ≥2 张时出现,不影响定性。(2) 修复建议中「仿 vanilla 用 HoverTipFactory.Static 静态提示」不可行:选牌标题来自 prefs.Prompt 渲染(NPlayerHand.cs L630),vanilla 没有"选牌获得关键词"的先例(Goopy 附魔是 AddKeyword(Exhaust) 但无选牌弹窗),应走自定义 LocString 路线。(3) 建议的具体可行做法已验证:LocManager.cs L468-475 会把 mod 的同名表 MergeWith 进 vanilla 表(…

### [low/DISMISSED] DefendLine — keyword
- **问题**: zhs 描述中「虚弱」缺少 [gold] 高亮,不符合原版关键词高亮惯例,且与 eng 版(已高亮)不一致
- **位置**: `localization/zhs/cards.json:27`
- **建议**: 改为「获得{WeakPower:diff()}层[gold]虚弱[/gold]。」与 eng 及原版惯例对齐。
- **复核补充**: 无需修改。若后续批卡新增含「虚弱」的 zhs 文案,保持 [gold]虚弱[/gold] 包裹即可(与 DEFY/COMET/FALLING_STAR 及 eng 侧惯例一致)。

### [low/DISMISSED] AxeOnslaught — keyword
- **问题**: zhs 描述中「易伤」缺少 [gold] 高亮,与 eng 版(已高亮)及原版惯例不一致
- **位置**: `localization/zhs/cards.json:31`
- **建议**: 改为「给予{VulnerablePower:diff()}层[gold]易伤[/gold]。」

### [low/CONFIRMED] HoldPosition — design-mismatch
- **问题**: 文案提到「消耗」但未挂任何名词解释(无 Exhaust 悬停提示),原版同类卡(Brand/BurningPact)都挂 HoverTipFactory.FromKeyword(Exhaust)
- **位置**: `src/STS2-Navia/Content/Cards/HoldPosition.cs:25`
- **建议**: 增加 `protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };`(注意本卡无原生关键词,不要挂进 CanonicalKeywords 横幅)。
- **复核补充**: 方向与修法均成立,仅两处细节修正:(a) 引文行号小漂移——BurningPact 的 ExtraHoverTips 覆写在 L17 而非 L20(Brand L20-24 准确);发现写的 file:line 指向 HoldPosition.cs L25(类声明行),实际关键证据在 localization/zhs/cards.json L25(巧合同号)。(b) 属性名澄清:vanilla CardModel 的虚属性是 ExtraHoverTips,RitsuLib ModCardTemplate 在其上另设 AdditionalHoverTips 作为模组扩展点(经 ExtraHoverTips 管线汇入 CardModel.HoverTips)——建议代码用 AdditionalHoverTips 与本 mod 五张兄弟卡及 cards-guide §1 完全一致,可直接编译,无…

### [info/CONFIRMED] OneTurnBlockPersistPower — other
- **问题**: 一次性格挡保留被实现为隐藏 Power,玩家看不到「格挡保留」待生效状态;与原版同机制的 BlurPower(可见+Flash)表现不同
- **位置**: `src/STS2-Navia/Content/Powers/OneTurnBlockPersistPower.cs:25`
- **建议**: 保持现状可接受;若希望玩家在两卡之间能确认保留状态,可改可见(需补 zhs/eng 三件套)或至少补 AfterPreventingBlockClear 的 Flash 反馈。
- **复核补充**: 1) 「机制本体与 BlurPower.cs L20-45 逐行等价」措辞稍强:ShouldClearBlock 写法不同(vanilla L20-27 为 if-return 两分支,mod L30 为单行 `return base.Owner != creature;`),语义等价但非逐行;且 L20-45 区间含 mod 未实现的 AfterPreventingBlockClear(L29-37),发现自己也承认此差异,无实质误导。2) 发现漏列第三处表现差异:vanilla BlurPower 还有 ExtraHoverTips = StaticHoverTip.Block(L18,悬停显示 Block 名词解释),mod 同样没有;若做表现层对齐应一并补。3) 采纳可见化建议时的键名修正:施工手册 §4.5「<类名蛇形大写> 三件套」描述已过时,mod powers.json 现…

### [info/未复核] AxeOnslaught — mechanics
- **问题**: 同时声明 DamageVar(18) 与 CalculationBaseVar(18),实际出手伤害只取 CalculationBase+(-2)×装填;未来任何「提升某卡伤害」的效应若改 DamageVar 将不影响实际伤害与面板总值
- **位置**: `src/STS2-Navia/Content/Cards/AxeOnslaught.cs:43`
- **建议**: 备忘: 后续实装支援系「伤害+2」或遗物类增益时,需确认其写入 CalculationBase 还是 DamageVar,否则本卡(及 VolleyFire/RoutineInspection)会静默丢失增益。

## load-core(覆盖 11 项)

### [medium/CONFIRMED] EmergencyEvade / EmergencyEvadePower — text-behavior
- **问题**: 同回合打出第二张「紧急避险」时,下回合仍只装填1而非2:触发时硬编码 1,忽略了叠层后的 base.Amount
- **位置**: `src/STS2-Navia/Content/Powers/EmergencyEvadePower.cs:32`
- **建议**: 把硬编码 1 改为 `(int)base.Amount`(幂等:标记每次施放 +1,叠到几层就装填几层),或改用 Instanced 实例型 Power 让每次施放独立结算。
- **复核补充**: 发现本身无需修正(行号、数值、机制、可达性全部准确)。补充三点: 1. 修复建议采纳第一方案即可:EmergencyEvadePower.cs:32 的硬编码 1 改为 `(int)base.Amount`,与同 mod CommandStancePower.cs:26 及 vanilla BlockNextTurnPower/StarNextTurnPower 的结算方式一致;无需改 Instanced 实例型(Counter 叠层 + 读 Amount 是既有惯例,Instanced 反而偏离 vanilla 同型 Power 的写法)。同时必须同步更新 EmergencyEvadePower.cs:16 的 XML 注释——它现在把「仍只装填 1」记录为预期行为,会误导后续维护;若作者真的想要不叠加,出路是改文案(如「下回合装填1(不叠加)」)而非维持现状。 2. 验收期动态测试(补…

### [low/CONFIRMED] SurgingPursuit — text-behavior
- **问题**: 「获得等量格挡」不成立:获得格挡时装填尚未减半,LoadPower 的每2层+1敏捷会额外加到格挡上
- **位置**: `src/STS2-Navia/Content/Cards/SurgingPursuit.cs:66`
- **建议**: 二选一:把减半挪到 GainBlock 之前(需保持 total 预先计算,不影响先算后耗);或接受敏捷联动并在文案中去掉「等量」字样(如「获得{CalculationExtra}点格挡」)。动态验收时重点测这张卡。
- **复核补充**: 方向与细节全部属实(行号 66/77、文案引文、vanilla 链路、36+3 算例、EmergencyEvade 对照均核对无误),但 suggestion 的方案一不可行:把减半挪到 GainBlock 之前并不能恢复「等量」——减半后生物仍保留 floor(load/2) 层装填,GainBlock 时仍会吃到 floor(保留层数/2) 的残余敏捷加成(例:6 层→保留 3 层→格挡 36+1=37,仍≠36)。真正能达成相等的修法:(a) 该处 GainBlock 改传 ValueProp.Unpowered(mod 已有同款先例:UnitedFrontPower.cs:32、HoardingSuppliesPower.cs:41 均用 Unpowered 规避 powered 格挡修正);或 (b) 保留敏捷联动、改文案去掉「等量」——括号内如需显示真实总格挡,应新增 vani…

### [low/CONFIRMED] SurgingPursuit — mechanics
- **问题**: 战斗内预览「当前共造成X点伤害」不含力量/虚弱/附魔:用的是通用 CalculatedVar 而非 vanilla 的 CalculatedDamageVar
- **位置**: `src/STS2-Navia/Content/Cards/SurgingPursuit.cs:45`
- **建议**: 若要预览精确,改用 CalculationBaseVar + ExtraDamageVar + CalculatedDamageVar(ValueProp.Move) 三件套(与现有 loc 键 {CalculatedDamage}/{CalculationExtra} 兼容性需核对:CalculatedDamageVar.GetExtraVar 取的是 ExtraDamage 而非 CalculationExtra,需同步换 loc 占位符或保持现状仅记录)。
- **复核补充**: 发现方向与细节均无误,补三点修正/扩充:(a) 同款问题实为五张卡而非三张——EmergencyEvade.cs:44 用 `new CalculatedVar("CalculatedBlock")`,对应 vanilla CalculatedBlockVar(其 UpdateCardPreview 会跑 Hook.ModifyBlock),即敏捷类加成同样不进格挡预览;SurgingPursuit 的「并获得等量格挡」括号文案同理(CreatureCmd.cs:681 的 GainBlock 结算路径会跑 Hook.ModifyBlock,预览不会)。(b) 对 VolleyFire/AxeOnslaught/RoutineInspection 还有内部不一致:卡面主句 {Damage} 是普通 DamageVar,其 UpdateCardPreview 会跑 Hook.ModifyDa…

### [low/CONFIRMED] HighPressureChamber / HighPressureChamberPower — text-behavior
- **问题**: 「每当你消耗1层装填」实际为「任意装填层数减少都触发」:危险改装的回合衰减(文案动词是「失去」)也会触发全体伤害
- **位置**: `src/STS2-Navia/Content/Powers/HighPressureChamberPower.cs:27`
- **建议**: 若设计意图确为「仅主动消耗」,需要在 LoadPower 上区分消耗/失去两类入口(如在 Gain 加 cause 参数);若「任意减少」即意图,建议在高压弹膛文案或装填关键词解释中写明「装填减少时」以消除歧义。
- **复核补充**: 方向与细节均坐实,仅两处补充精化:(1) 修正建议可更省事——现有调用点已自带区分信号:所有主动消耗(EjectedShell.cs:57/62、WarningShot.cs:54、EmergencyEvade.cs:60、RefinedCharge.cs:49、PassionFire.cs:57、SurgingPursuit.cs:77)的 LoadPower.Gain 都把卡牌作为 cardSource 传入,而 DangerousRetrofitPower.cs:31 传 null;故在 AfterPowerAmountChanged 里检查 cardSource != null 即可立刻区分「主动消耗/被动失去」,无需先给 Gain 加 cause 参数(加参数仍是更稳健的长期方案,可防止未来非卡牌主动消耗的漏判)。(2) 发现末句「乘胜追击的减半触发礼炮联动」措辞略偏:减半触发的…

### [low/DISMISSED] CommandStance / CommandStancePower — text-behavior
- **问题**: 文案「每有1张牌被消耗」未限定归属,实现只统计「你的」牌(合作模式队友消耗的牌不触发)
- **位置**: `src/STS2-Navia/Content/Powers/CommandStancePower.cs:23`
- **建议**: zhs 改为「每有1张你的牌被消耗」、eng 改为 "Whenever one of your cards is exhausted",与实现及 vanilla 惯例对齐。

### [info/未复核] DangerousRetrofit / DangerousRetrofitPower — mechanics
- **问题**: 重复打出危险改装:衰减不叠加(硬编码每回合-1),叠加的+6 装填又被上限截断,第二张几乎完全空转
- **位置**: `src/STS2-Navia/Content/Powers/DangerousRetrofitPower.cs:31`
- **建议**: 如希望多张有意义的叠加语义,可让衰减按 Amount 计;否则维持现状即可,无需改文案。

### [info/未复核] HighPressureChamberPower / CommandStancePower — text-behavior
- **问题**: 两条可见 Power 的 .description 硬编码数值(「3点伤害」/「装填1」),叠层后与实际不符;战斗内悬停走 smartDescription({Amount})无此问题
- **位置**: `localization/zhs/powers.json:6`
- **建议**: 可将 .description 中硬编码数值也改为 {Amount} 占位(参考 LOAD_POWER.description 的写法),或维持现状(影响面极小)。

### [info/未复核] SurgingPursuit — mechanics
- **问题**: 每层伤害/格挡以「单次聚合打击」结算(一次 4×load 的攻击),不是逐层多段命中;与文案读法一致,但影响按命中次数触发的跨卡联动
- **位置**: `src/STS2-Navia/Content/Cards/SurgingPursuit.cs:62`
- **建议**: 无需改动;动态验收与后续设计「每层一次命中」类新卡时注意此口径差异。

## mora-basic(覆盖 12 项)

### [low/CONFIRMED] InvitationUrn / FinancialLine / GrandEntrance — text-behavior
- **问题**: 三张卡中文描述只写「生成N张闪耀摩拉」,未注明生成位置,实际行为是加入手牌(同组黄金打击的措辞与三卡英文文案均写明了手牌)
- **位置**: `localization/zhs/cards.json:67`
- **建议**: 三张卡 zhs 统一为「在手牌中生成{Moras:diff()}张[gold]闪耀摩拉[/gold]」,与黄金打击及英文版对齐
- **复核补充**: 无需事实修正。仅补充落地细节:三卡句式各异,统一时注意 GRAND_ENTRANCE 为「固有。在手牌中生成{Moras:diff()}张[gold]闪耀摩拉[/gold]，抽{Cards:diff()}张牌。消耗。」(位置限定词插入首句),INVITATION_URN 的逗号连接句相应为「…点伤害，在手牌中生成…」,FINANCIAL_LINE 为「…格挡。在手牌中生成…」。另建议顺带把设计文档(docs/design/card-roster.txt（原案对应三条生成摩拉的卡）)同步补上位置措辞,避免后续批次再按原文抄出同样欠规范文案。

### [low/CONFIRMED] UnitedFrontPower / HoardingSuppliesPower — keyword
- **问题**: zhs smartDescription 中「闪耀摩拉」「格挡」未加 [gold] 高亮,同表其他词条(兜售枪火/高价买入等)与英文版均做了高亮,违反施工约定的名词高亮规范
- **位置**: `localization/zhs/powers.json:28`
- **建议**: 给两条 smartDescription 的「闪耀摩拉」「格挡」补 [gold]…[/gold] 高亮
- **复核补充**: 两处细节修正:(1) 实际键名带 STS2_NAVIA_POWER_ 前缀——zhs/powers.json:28 的键是 STS2_NAVIA_POWER_UNITED_FRONT_POWER.smartDescription,:31 是 STS2_NAVIA_POWER_HOARDING_SUPPLIES_POWER.smartDescription(发现中省略了前缀,行号本身准确)。(2) 补充遗漏面:这两条的 zhs .description 字段(zhs:27「每当你打出一张闪耀摩拉，获得[blue]3[/blue]点格挡。」与 zhs:30)同样无高亮,而 eng 对应 description(eng:27/eng:30)有 [gold] 高亮;不过 zhs 全部 10 条 power .description 本就统一无高亮(非这两条独有),且 vanilla 在 smart…

### [low/DISMISSED] GoldenStrike / InvitationUrn / FinancialLine / GrandEntrance — translation
- **问题**: 英文「Add … into your Hand」介词搭配不地道(应为 Add … to your hand),且 [gold]Hand[/gold] 高亮了非关键词词汇而中文对应文本无高亮,中英富文本不对称
- **位置**: `localization/eng/cards.json:65`
- **建议**: 改为「Add {Moras:diff()} [gold]Shining Mora[/gold] to your hand.」,并去掉 Hand 的 gold(或中英同步)
- **复核补充**: 若要采纳残存的「中英同步」分支,修正方向应与发现相反:保留英文原文(Add ... into your [gold]Hand[/gold].)不动,改中文侧——zhs/cards.json 全文件 0 处 [gold]手牌[/gold](vanilla zhs 为 110 处带标 vs 8 处不带),建议 GOLDEN_STRIKE 等卡「在手牌中生成」→「在你的[gold]手牌[/gold]中生成」,并考虑让 67/69/71 像 vanilla BLADE_DANCE 中文那样显式写出目的地(「添加…到你的[gold]手牌[/gold]」),同时可顺带统一 HOLD_POSITION/FIRE_AMPLIFICATION 等其余手牌提及处。这是独立的中文侧规范问题,不构成原发现所述的英文缺陷。

### [info/DISMISSED] UnitedFrontPower / HoardingSuppliesPower — text-behavior
- **问题**: Power 的 .description 硬编码基准值 3,升级(5)或叠层(6)后在少数走非 smart 描述的展示路径会显示过时数值;战斗内悬停走 smartDescription 的 {Amount},玩家常规视角不受影响
- **位置**: `localization/zhs/powers.json:27`
- **建议**: 可将 .description 中硬编码数值改为 {Amount} 占位(AddDumbVariablesToDescription 已注入 Amount),或维持现状待全项目统一处理
- **复核补充**: 维持现状,不建议改为 {Amount}:在 .description 唯一会被用到的场景(FromPower/HoverTips 非 mutable 分支)中,{Amount} 解析到的是规范 Power 实例的默认 0 而非卡牌 PowerVar 的 3/4/5,会把「获得3点格挡」变成「获得0点格挡」。唯一能让 {Amount} 有意义的形态是调用方经 HoverTipFactory.FromPower<T>(amount) 显式传 override——若未来某张卡/词条想在悬停预览里引用这两个 Power 并传入当前值,届时再为该调用点定制即可,无需现在动 powers.json。

## mora-advanced(覆盖 14 项)

### [medium/CONFIRMED] ExtravagantSpendPower — mechanics
- **问题**: 同回合打出两张豪掷千金时,隐藏 Power 叠成单实例 Amount=2,但处理器硬编码装填 1,第二张卡的『装填1』丢失
- **位置**: `src/STS2-Navia/Content/Powers/ExtravagantSpendPower.cs:29`
- **建议**: 改为 `LoadPower.Gain(choiceContext, base.Owner, (int)base.Amount, base.Owner, cardSource)`;单张时 Amount=1 语义不变,叠层时正确叠加。
- **复核补充**: 发现本身无需修正(行号 29、引文、卡名、机制描述均准确)。补充两点加强:(a) 故障窗口不止"同回合"——ExtravagantSpendPower 是 Buff 且无回合衰减,只要两张豪掷千金在场时未打过摩拉,跨回合先后打出同样叠成 Amount=2 并丢失一份承诺;(b) 建议的修复 `LoadPower.Gain(..., (int)base.Amount, ...)` 与 mod 内 CommandStancePower.cs:26 的既有写法一致,且语义上等价于把该 Power 改为 PowerInstanceType.Instanced(两实例会在同一次摩拉广播中各装填 1,合计同一张摩拉装填 2),两种修法收敛于同一结果,采纳其一即可。验收期动态验证步骤:进战斗用调试/控制台连续打出两张豪掷千金(不打摩拉),确认只存在一个 ExtravagantSpendPower 实例(…

### [medium/CONFIRMED] ForcedBuyout — text-behavior
- **问题**: 文案说『将消耗堆中的闪耀摩拉对一名敌人打出』,代码实为直接伤害结算并把这些摩拉移出战斗——不视为打出摩拉、摩拉也不留在消耗堆,文案未说明这两点差异
- **位置**: `src/STS2-Navia/Content/Cards/ForcedBuyout.cs:45`
- **建议**: 按工作约定保留代码行为、修文案:如『视为依次打出……每张造成3点伤害(不触发打出摩拉的效果,这些摩拉随后离开本场战斗)』;若设计上希望联动,则改走真实出牌管线。
- **复核补充**: 方向与全部实质论点正确,仅两处细节需修正、一处建议需补充:(a) 行号锚点:45 行只是快照消耗堆的 GetCards 行,真正的「绕开管线」在 50-57 行(50 行 RemoveFromCombat、51-57 行直接 Attack/GainBlock 循环);所引『这些摩拉随后从消耗堆移除』出自类头 XML doc 注释(20 行),45 行处的行内注释是『快照消耗堆中的摩拉后先移出消耗堆,再逐张结算』。(b) 建议中「改走真实出牌管线」并非现成可用:vanilla PlayCardAction.cs:67-72 对不在 PileType.Hand 的卡静默丢弃,消耗堆里的摩拉无法直接走 PlayCardAction;若要联动,可行路线是逐张调用公开的 CardModel.OnPlayWrapper(ctx, target, isAutoPlay:true, resources)(…

### [medium/CONFIRMED] PeddlingFirearms — keyword
- **问题**: 兜售枪火文案提到『装填』但未覆写 CanonicalKeywords 挂 NaviaKeywords.Load,卡面缺少装填的关键词横幅与解释框
- **位置**: `src/STS2-Navia/Content/Cards/PeddlingFirearms.cs:25`
- **建议**: 参照 ExtravagantSpend.cs 增加 CanonicalKeywords 覆写并 `NaviaKeywords.AddTo(set, NaviaKeywords.Load)`。
- **复核补充**: 发现主体完全准确,仅三处细节补充: (1) 对比卡文件名有一次笔误「ExtravagagantSpend.cs」应为 ExtravagantSpend.cs(其建议处拼写正确,不影响结论)。(2) PeddlingFirearms.cs 第 15 行其实已存在未被使用的 `using NaviaMod.Content.Keywords;`(佐证这是遗漏而非有意省略),因此修复只需按建议增加 CanonicalKeywords 覆写,无需改 using——注意签名须为 `public override IEnumerable<CardKeyword>`(cards-guide §1 提醒 protected 会 CS0507),可与现有代码风格一致: `public override IEnumerable<CardKeyword> CanonicalKeywords { get { Has…

### [low/CONFIRMED] VentureCapital — text-behavior
- **问题**: 风险投资自身那次 7 点命中不计入『每次命中这名敌人生成摩拉』(伤害先结算、增益后施加),文案未说明这一时序差异
- **位置**: `src/STS2-Navia/Content/Cards/VentureCapital.cs:45`
- **建议**: 文案补措辞,如『造成伤害。此后的本回合内,你每次命中这名敌人……』;或确认设计接受原版先伤后增益惯例则维持现状。
- **复核补充**: 维持 CONFIRMED 但补充三点修正/细化:(1) 行号精确化:OnPlay 声明于 39 行,攻击在 42-44 行,PowerCmd.Apply 在 45 行(发现写 41-46 指的是方法体,基本准确);文案实为「造成{Damage:diff()}点伤害」。(2) 最关键的补充事实:vanilla 先例决定处置方向——Strangle(紧勒,造成8点伤害+本回合每出一张牌该敌人失去2点生命)与 VentureCapital 逐行同构、同样自身不计、同样不加限定词,Knockdown 亦然,且 vanilla 全部卡牌文案 0 次使用「此后」类限定。因此本发现本质是"是否比 vanilla 更严格"的文案政策问题而非代码缺陷:采纳发现建议的选项 B(接受原版先伤后增益惯例、维持现状、不改代码)是完全正当的裁决;若倾向选项 A 改词,除卡牌描述外还需同步改 STS2_NAVIA_PO…

### [low/CONFIRMED] FinancialMarketPower — translation
- **问题**: 金融市场/风险投资的英文文案写 shuffle into draw pile,实际生成位置是抽牌堆底部(确定性置底,非洗入)
- **位置**: `src/STS2-Navia/Content/Powers/FinancialMarketPower.cs:58`
- **建议**: eng 改为 『add a Shining Mora to the bottom of your draw pile』(两张卡同改);zhs 可顺手写明『置于抽牌堆底部』。
- **复核补充**: 方向与主体全部正确,三处细节修正/补充:(a) 实际本地化键带前缀:cards.json 中为 STS2_NAVIA_CARD_FINANCIAL_MARKET.description / STS2_NAVIA_CARD_VENTURE_CAPITAL.description(发现里写的 FINANCIAL_MARKET.description 是简写;VENTURE_CAPITAL 卡文案实为 "into your draw pile" 而非 "into the draw pile","the" 出现在 power 文案里)。(b) 发现遗漏了同样错配的 4 条 power 工具提示字符串:localization/eng/powers.json:21-22(FINANCIAL_MARKET_POWER 的 description/smartDescription)与 :24-25(VE…

### [info/未复核] FinancialMarket — text-behavior
- **问题**: 打出金融市场本身也会立刻生成 1 张摩拉(自身是技能牌且钩子在其 OnPlay 之后触发)——与字面文案一致,属设计意图备忘
- **位置**: `src/STS2-Navia/Content/Cards/FinancialMarket.cs:41`
- **建议**: 确认这是期望手感(1 费换 1 摩拉入场)即可;若非设计本意,需在 AfterCardPlayed 里排除 cardPlay.Card==施加来源。

### [info/未复核] PeddlingFirearmsPower/ProfiteeringPower — translation
- **问题**: 兜售枪火/牟取利益的 Power 静态 description 硬编码『3』,升级后实际施加 5;战斗内 smartDescription 用 {Amount} 显示正确
- **位置**: `localization/zhs/powers.json:1`
- **建议**: 维持现状即可(原版同风格);若想绝对准确可把 description 的数字也改成占位符。

## salvo(覆盖 6 项)

### [low/CONFIRMED] SuppressingFire(火力覆盖) — text-behavior
- **问题**: 描述里第二次出现的「金花礼炮」未加 [gold] 高亮,与首次出现不一致
- **位置**: `localization/zhs/cards.json:81`
- **建议**: 将第二处「金花礼炮」也包上 [gold]…[/gold](eng 同理),或确认引擎关键词系统会自动高亮后续出现后保持现状。
- **复核补充**: 发现方向与细节均无误,行号(zhs/eng 各 81)、卡名、引文、设计文档出处全部核对一致。补充裁定发现 suggestion 中留的另一个分支:「引擎自动高亮后续出现」不成立(见 evidence 第 3 点),故唯一正确处置是手工补标签——zhs 将第二处「金花礼炮」包上 [gold]…[/gold];eng 直接照本 mod cards.json:85(意外事故)已有的复数标注先例写成 [gold]Golden Rose Cannons[/gold] 即可。顺带备注(超出本发现范围,供后续统一清理):zhs cards.json:19(闪耀摩拉)及 35/37 行括号提示内的「格挡」也存在同类裸词,验收时可一并按 vanilla 全标注惯例补齐。

### [info/CONFIRMED] SuppressingFire(火力覆盖) — mechanics
- **问题**: 「条件先判/快照手牌」语义已验证自洽:新生成的金花礼炮不享受次数+1
- **位置**: `src/STS2-Navia/Content/Cards/SuppressingFire.cs:49`
- **建议**: 无需改动;动态验收时顺手确认:空手打出→获得一张 2伤3次 的普通礼炮(无次数加成)。
- **复核补充**: 无需修正,发现的行号、数值与结论均准确。补充两点供动态验收扩大覆盖:(a) 建议把建议中的用例同时跑升级版——空手打出升级后的火力覆盖(礼炮轰鸣5)同样只会生成一张 2伤3次 的普通礼炮(Salvo 数值不影响生成炮的面板,这本身是关键词设计使然,可在验收时顺带确认文案理解);(b) 有炮场景验收点:手中已有 1 张金花礼炮时打出火力覆盖,应观察到该炮伤害 +3(+5) 且次数 3→4,且手牌不会新增第二张炮(Salvo.Fire 有炮分支只增益不生成)。

### [info/DISMISSED] FireAmplification(火力增幅) — mechanics
- **问题**: 空手打出会一回合获得两张金花礼炮(Salvo 生成一张 + 固定再加入一张)
- **位置**: `src/STS2-Navia/Content/Cards/FireAmplification.cs:57`
- **建议**: 若非本意,可改为:手中有炮→强化+加一张;无炮→只生成一张(即先判后并/或去掉固定 CreateInHand)。若就是本意,无需改动。
- **复核补充**: 附带细节修正(不影响裁定):发现 summary 的「空手打出」措辞不精确——触发条件是「手牌中无金花礼炮」,手牌不必为空(发现 evidence 部分用的「手牌无炮时」才是对的)。若设计侧将来仍想确认意图,验收期动态测试:进战斗用调试手段清空手牌中的金花礼炮后打出火力增幅,数手牌中新生成的金花礼炮数量(预期 2 张,均为基础伤害 2×3 次);再在手牌中预置 1 张金花礼炮打出,确认旧炮伤害 +2 且另进 1 张新基础炮。

### [info/CONFIRMED] TravelLight(轻装上阵) — mechanics
- **问题**: 格挡量=两次伤害实际值之和且含溢出(Overkill),与文案「造成伤害总量」的口径已核对
- **位置**: `src/STS2-Navia/Content/Cards/TravelLight.cs:65`
- **建议**: 无需改动;仅备忘:对残血敌人溢出部分也计入格挡,动态验收时可确认此口径符合预期。

### [info/CONFIRMED] RefinedCharge(精制装药) — mechanics
- **问题**: 返回手牌与消耗装填的条件一致性已验证:去向解析先于 OnPlay,两处读同一份(消耗前)装填状态
- **位置**: `src/STS2-Navia/Content/Cards/RefinedCharge.cs:57`
- **建议**: 无需改动;备忘:该卡 0 费+回手意味着有装填时可连打(每次礼炮轰鸣2+消耗1层),受装填层数天然限流,符合设计意图。
- **复核补充**: 仅一处细节修正(不影响结论):finding 称升级 AddKeyword(CardKeyword.Retain) 有「CalculatedGamble/Anointed 等 5 处 vanilla 先例」,实际为 12 处(Dredge/Anointed/ReaperForm/RoyalGamble/CalculatedGamble/Scrawl/Wish/Monologue/TimesUp/Wisp/GoldAxe/Misery),先例比声称的更充分。另附一条前瞻备忘:该一致性依赖「1881→1933 之间无人改装填」这一当前成立的全局不变量——若未来实装钩 BeforeCardPlayed/AfterModifyingCardPlayResultLocation 且给予装填的内容(如某遗物「打出卡牌时装填1」),将产生「消耗了却进弃牌堆」错位;建议届时在该钩子实现处或 cards-g…

## semantics(覆盖 35 项)

### [medium/CONFIRMED] SuppressingFire — text-behavior
- **问题**: 「手中有金花礼炮」条件在礼炮轰鸣之前快照判定,手牌原本无礼炮时新生成的礼炮不获得伤害次数+1,而文案语序(先礼炮轰鸣后条件)暗示结算礼炮后再判定,顺序阅读会得出'+1必然生效'的错误预期
- **位置**: `src/STS2-Navia/Content/Cards/SuppressingFire.cs:49`
- **建议**: 裁定:需改文案(快照先判是合理设计,防白拿次数)。把『如果手牌中有』改为『如果手牌中已有』(eng 同步 'If there is already a ... in your hand'),一词之改即可消除歧义;不建议改代码。
- **复核补充**: 方向与细节均成立,仅作三点补强:(a) 英文文本同样位于 localization/eng/cards.json:81,原句为 "If there is a [gold]Golden Rose Cannon[/gold] in your hand, all Golden Rose Cannons in your hand gain +1 hit."(发现中为节引,无实质出入);修复建议 'If there is already a ...' 可用。(b) 建议把 docs/design/card-roster.txt 中的火力覆盖条目 同步改为「如果手牌中已有」——当时手册规定中文文案来自设计文档,只改 localization 而不改设计源头,后续批次重导文案时会原样带回歧义。(c) 更彻底的消歧措辞可用「打出时手牌中已有」/ 'If th…

### [medium/CONFIRMED] ForcedBuyout — text-behavior
- **问题**: 文案称『打出』消耗堆中的闪耀摩拉,实现是本卡自行结算:不触发任何『打出摩拉』联动(每3张装填1的摩拉计数、众志成城格挡、做空市场增伤、兜售枪火/牟取利益的摩拉增伤),且摩拉被 RemoveFromCombat 永久移出战斗而非回到消耗堆,两点均未在卡面披露
- **位置**: `src/STS2-Navia/Content/Cards/ForcedBuyout.cs:50`
- **建议**: 裁定:需改文案。建议:『将消耗牌堆中所有的闪耀摩拉对一名敌人射出(不触发『打出闪耀摩拉』相关的效果)，每张造成{Damage:diff()}点伤害。每射出1张，获得{Block:diff()}点格挡。这些摩拉随后离开本场战斗。』eng 同步把 'Play all Shining Mora' 改为 'Fire all Shining Mora ... (does not trigger effects that care about playing Shining Mora)';若设计上希望吃到兜售枪火类加成则需改代码走真打出管线,但当前快照结算更安全,推荐改文案。
- **复核补充**: 方向与细节均无误,仅两点补充:(a) 行号小误:做空市场的监听方法实际是 ShortSelling.cs:49-57(发现写 49-53,关键判断在 52 行,不影响结论);(b) 量级澄清:闪耀摩拉是战斗内生成牌(CreateCard + AddGeneratedCardsToCombat,同 vanilla shiv 机制),本就不在局外卡组里,故 RemoveFromCombat 与"留在消耗堆"的差异只在本场战斗内(可见性、二次强制买断复用),不存在跨战斗的卡组损失——发现里"本场战斗内消失"的表述才是准确口径,建议修正文案时用"离开本场战斗"而非"移出卡组"。修复方向赞同改文案(zhs+eng 同步):现快照结算确定且安全;vanilla 虽有 PlayCardAction 但没有"从消耗堆直接打出"的现成管线,真打出需先移回手牌再入队,还要处理 0 费摩拉的目标选择与连锁触发,…

### [medium/CONFIRMED] PeddlingFirearmsPower/PremiumPurchasePower/FinancialMarketPower — mechanics
- **问题**: 三个写着『本回合内』的可见 Power 挂 AfterSideTurnStart(自己回合开始)才移除,整个敌方回合图标滞留无效果;PremiumPurchase 在敌方回合若有摩拉被消耗还会在『本回合』窗口外触发礼炮轰鸣——vanilla『this turn』Power 一律在自己回合结束(AfterSideTurnEnd)移除
- **位置**: `src/STS2-Navia/Content/Powers/PeddlingFirearmsPower.cs:46`
- **建议**: 裁定:需改代码——把三处移除时点改为 AfterSideTurnEnd(照抄本 mod VentureCapitalPower 的写法),图标随回合结束消失,行为窗口与『本回合内』文案严格一致。若刻意保留(如担心回合结束弃牌阶段的时序),则至少把文案里的『本回合内』删改为『在你下个回合开始前』,并验收期动态确认敌方回合消耗触发场景。
- **复核补充**: 发现方向正确但三处细节需修正: (1)「vanilla 一律在自己回合结束移除」过强,且所引 ConcoctPower 恰是反例:ConcoctPower.cs:29-35 的 guard 是 `if (base.Owner.Side != side) Remove`——在**对方阵营**回合结束才移除,即 vanilla 的 Concoct 图标同样滞留整个敌方回合(FlameBarrierPower 同,它是「Whenever you are attacked this turn」的反击刺,必须覆盖敌方回合)。敌方回合图标滞留本身在 vanilla 有先例;但这些先例最多存续到敌方回合结束,仍早于自己下回合开始,而 mod 三处等得更久。核心对比组(DoubleDamage/BorrowedTime+28/33 的多数派)依然成立。 (2)PremiumPurchase「敌方回合触发礼…

### [medium/CONFIRMED] ExtravagantSpendPower — design-mismatch
- **问题**: 『下一次打出闪耀摩拉时装填1』的挂起标记被隐藏且触发窗口无上限,玩家打出豪掷千金后无从得知该增益是否仍在——vanilla 同类 pending 型 Power 均可见
- **位置**: `src/STS2-Navia/Content/Powers/ExtravagantSpendPower.cs:23`
- **建议**: 裁定:建议改代码——去掉 IsVisibleInternal=>false 覆写,按属性式注册补 STS2_NAVIA_POWER_EXTRAVAGANT_SPEND_POWER.title/.description/.smartDescription 三键(zhs+eng)。若维持隐藏,属可辩护的设计取舍,但需在验收清单记录。
- **复核补充**: 方向与修法均成立,三点补充修正:(1) 表述精确化——该标记永不静默失效,打出豪掷千金后增益必然在下一次闪耀摩拉触发,所以玩家面临的是「armed/已消耗不可分辨、跨回合遗忘、无法据其规划(如是否重复打豪掷千金)」的状态不透明,而非增益丢失,严重度 medium 合理但属 UX/一致性而非行为错误。(2) 发现暗示本 mod 隐藏 Power 是孤例偏差,实际上 mod 有成文的隐藏标记约定(docs/cards.md §53「隐藏标记 Power」、§79),16 个 Power 中 6 个隐藏;修正为:其余 5 个隐藏 Power(摩拉计数/上限标记/永久衰减/单回合延迟触发)均为内部机械或短窗口,ExtravagantSpend 是其中唯一「卡牌赋予+无限期条件触发」型,恰是 vanilla 全族可见的那一类,故本条不改代码就应记入验收清单。(3) 建议改代码时一并核查叠…

### [medium/CONFIRMED] DangerousRetrofitPower — design-mismatch
- **问题**: 整场战斗持续、每回合开始扣 1 层装填的负面状态被隐藏,玩家打出危险改装后没有任何持续可见的提醒,与 vanilla『持续型 Power 一律可见』的惯例相悖
- **位置**: `src/STS2-Navia/Content/Powers/DangerousRetrofitPower.cs:24`
- **建议**: 裁定:建议改代码(可见化+补三键);至少应为 Debuff 型可见(现在 Type 是 Buff,若可见化建议同时评估 PowerType 归类,持续扣资源对玩家更像负面状态)。
- **复核补充**: 方向与主要细节全部属实,仅两处细化: 1. 措辞微调:「没有任何持续可见的提醒」略重——可见的 LoadPower 计数每回合确实会可见地 -1,玩家能看到衰减这一后果;缺失的是对成因/存续规则的持久提示(多张卡都会消耗装填,回合开始的无来源 -1 无法归因)。主张实质成立。 2. 修复模板与一个实现陷阱:照抄 vanilla BiasedCognition 三件套即可——(a) 删掉 IsVisibleInternal 覆写恢复可见;(b) Type 改 PowerType.Debuff;(c) BeforeSideTurnStart 扣装填前加 Flash();(d) 挂 ExtraHoverTips => HoverTipFactory.FromPower<LoadPower>()(需 using MegaCrit.Sts2.Core.HoverTips,参照 BiasedCogni…

### [low/未复核] SurgingPursuit — mechanics
- **问题**: 获得格挡时自身装填的敏捷等效加成会计入:实际格挡=每层值×L+floor(L/2),战斗内括注『并获得等量格挡』字面不符(少报 floor(L/2),方向有利玩家);符合 STS『获得X格挡=基础值再吃敏捷』惯例,非面板数值错误
- **位置**: `src/STS2-Navia/Content/Cards/SurgingPursuit.cs:66`
- **建议**: 裁定:correct-as-is(按基础值+修正的 STS 惯例),不改代码不改文案;列入验收期动态测试:步骤=6层装填打出乘胜追击,观察飘出的格挡数是否=24+3,并确认玩家理解来源。若追求字面严格,可在括注改为『获得{CalculatedDamage}点格挡(另受敏捷影响)』,不推荐。

### [low/未复核] AxeOnslaught(+SurgingPursuit/VolleyFire/RoutineInspection/EmergencyEvade 同模式) — mechanics
- **问题**: 伤害/格挡型计算卡用的是基类 CalculatedVar,其战斗内预览不含力量/易伤/敏捷等修正;括注『当前(共)造成X点伤害』在有力量时低于实际值——vanilla 专用的 CalculatedDamageVar/CalculatedBlockVar 预览会跑 Hook.ModifyDamage/ModifyBlock,mod 内 Impregnable 已正确用 CalculatedBlock…
- **位置**: `src/STS2-Navia/Content/Cards/AxeOnslaught.cs:45`
- **建议**: 裁定:可接受但建议统一——伤害侧改用 CalculatedDamageVar(注意其 GetExtraVar 取 DynamicVars.ExtraDamage,需把 CalculationExtraVar 改名 ExtraDamageVar 并同步 loc 的 {CalculationExtra:diff()}→{ExtraDamage:diff()},vanilla PERFECTED_STRIDE 文案即此结构)。不改也不算文案造假(括注是『当前』估算),归验收期动态确认项。

### [low/未复核] VentureCapital/FinancialMarketPower — translation
- **问题**: eng 文案 'shuffle a Shining Mora into your draw pile' 与实现不符:AddGeneratedCardsToCombat 缺省位置是置底(CardPilePosition.Bottom)而非洗入;zhs『在抽牌堆(中)生成』措辞中性、准确
- **位置**: `src/STS2-Navia/Content/Powers/VentureCapitalPower.cs:61`
- **建议**: 改 eng 措辞为 'add a Shining Mora to the bottom of your draw pile'(powers.json 同步);或若设计想要洗入,代码加 `CardPilePosition.Random` 参数——二选一,推荐改文案(置底对抽牌节奏更可控)。

### [low/未复核] OneTurnBlockPersistPower(附 MoraCounterPower) — design-mismatch
- **问题**: 『下回合格挡不消失』被隐藏,而其机制原型 vanilla BlurPower 是可见 Power(有标题/描述);摩拉计数 MoraCounterPower 隐藏导致『再打几张装填1』进度不可见——两者均为短时或信息已在卡面,属低优先级可见化建议
- **位置**: `src/STS2-Navia/Content/Powers/OneTurnBlockPersistPower.cs:25`
- **建议**: 裁定:建议(非必须)可见化并补键——OneTurnBlockPersist 抄 BLUR_POWER 文案结构;MoraCounter 可做成 Counter 型可见进度。若维持隐藏,记录为已知设计取舍。

### [info/未复核] TravelLight — placeholder
- **问题**: 同键占位符 {Damage:diff()} 在一条描述中出现两次,两处都会正确且相同地渲染——SmartFormat 对每个占位符独立求值,无『后者覆盖前者/只剩一个』问题,静态关闭
- **位置**: `src/STS2-Navia/Content/Cards/TravelLight.cs:46`
- **建议**: 裁定:correct-as-is,静态关闭,无需验收期专项测试。

### [info/未复核] AxeOnslaught — text-behavior
- **问题**: 负值 CalculationExtra:diff() 渲染为带符号的 '-2'(不取绝对值、不变号、无高亮色),zhs『这张卡的伤害-2点』可读、eng 'modified by -2' 更顺;升级三件套联动核对无误
- **位置**: `src/STS2-Navia/Content/Cards/AxeOnslaught.cs:44`
- **建议**: 裁定:correct-as-is。可选润色:zhs 改『这张卡的伤害降低2点』更口语,但需注意 {CalculationExtra:abs()} 不可用——AbsoluteValueFormatter.cs:25-36 只接受纯数值(decimal/int 等),不接 DynamicVar,写死文本或维持现状均可。

### [info/未复核] VentureCapitalPower — mechanics
- **问题**: 被完全格挡的命中也触发生成摩拉——与文案『每次命中』一致(未过滤是正确配对):vanilla 伤害管线中 AfterDamageReceived 只跳过目标死亡,不跳过 WasFullyBlocked;vanilla 想只算破防伤害时会显式过滤并把文案写成 'unblocked attack damage'
- **位置**: `src/STS2-Navia/Content/Powers/VentureCapitalPower.cs:31`
- **建议**: 裁定:correct-as-is,不需改代码也不需改文案。若未来设计意图收窄为『造成伤害的命中』,改文案为『每次对这名敌人造成伤害』并加 result.UnblockedDamage>0 过滤(参照 ConcoctPower)。

### [info/未复核] ArmorPiercer — mechanics
- **问题**: Skill 卡直接走 DamageCmd.Attack 无任何引擎限制、卡面渲染无误导:DamageVar 在 Skill 卡上正常显示伤害框;但与 vanilla『技能伤害=不可格挡的 HP 失去』惯例不同,本卡走完整攻击结算——力量/易伤会加成,敌方 ThornsPower 会反伤
- **位置**: `src/STS2-Navia/Content/Cards/ArmorPiercer.cs:48`
- **建议**: 裁定:完全没问题(能用,卡面不误导,无需改文案)。附验收期动态测试项:对带荆棘(ThornsPower)的敌人群使用本卡,确认反伤与力量加成符合设计预期;若设计不想要力量/荆棘交互,才需改代码(Unpowered 化)并同步文案为『敌人失去X点生命』。

### [info/未复核] HoardingSuppliesPower — mechanics
- **问题**: BeforeSideTurnEndEarly 取弃牌前手牌快照的时序正确,无多算/少算:回合结束顺序为 PhaseOne(VeryEarly→Early→BeforeSideTurnEnd)→PhaseTwo(FlushPlayerHand 弃牌)→AfterSideTurnEnd,Early 时手牌完整;且闪耀摩拉自带保留本就不会被弃
- **位置**: `src/STS2-Navia/Content/Powers/HoardingSuppliesPower.cs:31`
- **建议**: 裁定:correct-as-is,静态关闭。极端边角(回合结束阶段中途抽到的摩拉会计入与否)与 vanilla 同类行为一致,不必处理。

### [info/未复核] RosulaEmblem — mechanics
- **问题**: 『每回合第一次获得格挡』的回合标记在自己阵营回合开始(BeforeSideTurnStart+participants)重置,与 vanilla Kunai 等 per-turn 遗物完全同款;敌人回合获得格挡落在上一窗口内(未用则触发装填),符合引擎 per-turn 直觉,文案与设计文档逐字一致
- **位置**: `src/STS2-Navia/Content/Relics/RosulaEmblem.cs:41`
- **建议**: 裁定:correct-as-is。附验收期动态确认项:装备刺玫会徽,在敌方回合通过反伤/触发型效果获得格挡,确认装填行为与预期一致(两种解读——窗口内第一次 vs 仅自己回合内第一次——引擎取前者,vanilla 同)。

### [info/未复核] NaviaKeywords(LOAD/SALVO) — keyword
- **问题**: LOAD/SALVO 两条名词解释逐句与实现核对一致(关键机制说明通过);zhs/eng 语义一致
- **位置**: `localization/zhs/card_keywords.json:2`
- **建议**: 裁定:correct-as-is,静态关闭。备注:SALVO 解释未提『生成的那张进入手牌』——条件句已隐含手牌语境,不构成误导,可不改。

### [info/未复核] PowerVisibilityAudit(全部16个Power) — design-mismatch
- **问题**: 16 个 Power 可见性与 loc 键全量核对自洽:10 个可见(未覆写 IsVisibleInternal,默认 true)均有三键且键名与类名蛇形大写一致,6 个隐藏均显式覆写且无键;zhs/eng 各 30 键完全对齐,无『可见却无键』(high)情形;LoadCapUpPower(信息在装填 tooltip)/EmergencyEvadePower(一回合短时)隐藏合理
- **位置**: `src/STS2-Navia/Content/Powers/:1`
- **建议**: 审计结论:无 high;可见性/键位零缺失。隐藏 6 个中 4 个建议可见化(见各自条目),2 个维持现状;集成者原清单第 8 条所指『敌方回合图标仍可见』的三个 Power 已定位为 PeddlingFirearms/PremiumPurchase/FinancialMarket(见 medium 条目),EmergencyEvade/ExtravagantSpend/OneTurnBlockPersist 实为隐藏型,不含图标滞留问题。
