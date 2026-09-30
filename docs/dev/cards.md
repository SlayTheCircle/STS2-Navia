# 卡牌、Power 与支援

设计依据见[公开设计资料](../history/design/README.md)，当前状态见 [STATUS](../../STATUS.md)，环境与协作规则见[贡献指南](../../CONTRIBUTING.md)。本页维护当前写法，不要求内部批次分工或临时 staging 交付。

## 0. 修改范围

1. 卡牌继承 NaviaCardBase，具名 Power 使用对应 Base 或明确的隐藏标记类型，按注册特性接入。
2. 可以修改所需共享实现、本地化和资源映射；内容、运行文本及对应设计差异在同一变更更新。TypeList 池无需手工追加每张卡。
3. 本地运行与变更相称的检查；DLL 编译不要求私有素材。命令见[构建管线](pipeline.md)。
4. 新增原案外内容须在设计对照与花名册审计的新增集合注明；玩法调整按已确认设计处理。

卡牌目录按[目录组织约定](../history/design/card-organization.md)收纳。普通池使用 Attacks／Skills／Powers 下的稀有度子目录；Basic、Ancient、Tokens 单独集中。声明仍使用 NaviaMod.Content.Cards，注册特性、复合 ID 和图片文件名与类名保持原有关系。新增卡牌直接放入相应子目录，检查脚本递归枚举。

## 1. 专有名词关键词(名词解释系统)

专有名词悬停解释分两类,**引用哪个名词就挂哪类**:

- **机制名词**(装填/礼炮轰鸣)→ 关键词横幅+文字解释框,在 `CanonicalKeywords` 挂
  (注意:签名是 `public override IEnumerable<CardKeyword>`——vanilla CardModel 的公开虚属性,protected 会 CS0507;
  已有原生关键词的卡在同一覆写里合并):
```csharp
public override IEnumerable<CardKeyword> CanonicalKeywords
{
    get
    {
        HashSet<CardKeyword> set = new HashSet<CardKeyword> { CardKeyword.Exhaust /* 原生关键词 */ };
        NaviaKeywords.AddTo(set, NaviaKeywords.Load); // 按提到的名词挂
        return set;
    }
}
```
- **卡牌名词**(金花礼炮/闪耀摩拉)→ **卡面预览**(原版「精准」引小刀同款),在 `AdditionalHoverTips` 挂
  (IHoverTip 不可单值直赋,必须包数组):
```csharp
protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };
```

解释文本的唯一权威来源:`localization/{lang}/card_keywords.json`(键 `STS2_NAVIA_KEYWORD_*.title/.description`,当前包含装填、礼炮轰鸣、支援)。
**修改机制实现时必须同步核对解释文本**,文案不准就是事故。描述正文里的名词用 `[gold]名词[/gold]` 高亮。

## 2. 现有子系统 API

| 调用 | 用途 |
|---|---|
| `LoadPower.Gain(ctx, creature, amount, applier, cardSource)` | 获得/消耗装填(**唯一入口**,amount 负数=消耗;自动处理上限 6/9) |
| `LoadPower.CapFor(creature)` | 当前装填上限 |
| `creature.GetPowerAmount<LoadPower>()` / `GetPower<LoadPower>()` | 读层数/读实例 |
| `Salvo.Fire(combatState, ctx, player, value)` | 礼炮轰鸣(数值调整V1 勘误):全部牌堆中已存在的金花礼炮伤害即时+value,不累计不继承;手牌无礼炮时另生成一张(从基础值起算,仅吃炮火连天次数累计) |
| `GoldenRoseCannon.CreateInHand(player, combatState)` | 手牌生成一张金花礼炮 |
| `ShiningMora.CreateInHand(player, count, combatState)` | 手牌生成 count 张闪耀摩拉(返回生成列表) |
| `ShiningMora.CountMoraPlayed(ctx, creature, cardSource)` | 摩拉打出计数(满 3 装填 1;生成不需要调,打出时才调) |
| 隐藏标记 Power | `MoraCounterPower`(摩拉计数,`AfterPowerAmountChanged` 可监听它的层数变化来感知「摩拉被打出」)、`LoadCapUpPower`(上限 9 标记) |

金花礼炮手牌实例的可变属性:`DynamicVars.Damage.BaseValue`(伤害)、`DynamicVars["Hits"].BaseValue`(次数)——直接抬升即可(参考 `Salvo.cs`)。

### 2a. 支援系(附魔底盘,vanilla 原生)

施加型支援卡 = 附魔类 + 施加卡两张东西;协同型卡只做查询。样例:`PresidentOrder`(会长号令) + `SupportDamageUp`(火力支援)。

| 调用/成员 | 用途 |
|---|---|
| `CardCmd.Enchant<T>(card, amount)` | 给卡施加附魔(T : EnchantmentModel;不可附魔会抛,选牌过滤器先排除) |
| `card.Enchantment is NaviaSupportEnchantment` | 协同卡查询「带有支援效果的牌」(**唯一判据**,别用具体附魔型判) |
| 继承 `NaviaSupportEnchantment` | 支援附魔基类:HasExtraCardText 已开、图标自动解析、标记家族 |
| `EnchantDamageAdditive/Multiplicative`、`EnchantBlockAdditive/Multiplicative`、`EnchantPlayCount` | 修改被附魔卡的伤害/格挡/打出次数——**必须用这组专用钩子**(先于遗物/能力钩子),不要用 Modify* |
| `OnPlay(ctx, cardPlay)` | 被附魔的卡打出时触发(如「打出时生成摩拉」类支援) |
| `OnEnchant()` + `RecalculateValues()` | 附魔即改卡(如费用/关键词类支援,类比升级) |
| `CanEnchantCardType(CardType)` | 限定可附魔卡型(如仅攻击) |
| loc 表 `enchantments` | 键 `STS2_NAVIA_ENCHANTMENT_<类名蛇形大写>.{title,description,extraCardText}`,extraCardText 会附加显示在被附魔卡面上,`{Amount}` 可用 |

硬约束:
- **作用域=本场战斗,由引擎保证**(战斗牌堆是主卡组的克隆,战斗内附魔随 CombatState 消亡)——不要写清场钩子;
- 一卡一附魔槽:已附魔的卡不能再次获得支援(协同「换支援」设计需先 `CardCmd.ClearEnchantment`);
- 图标:专属图按类名 `images/enchantments/<类名>.png`，缺图回落共享徽记 `navia_support.png`。当前九种支援统一使用徽记，母版由素材脚本的 [图标模块](../../scripts/art/icons.sh)派生；差异由附加文本表达。

## 3. 卡牌类模板与风格

以 `Content/Cards/Attacks/Common/PassionFire.cs` / `UmbrellaStrike.cs` 为基准:sealed class 继承 `NaviaCardBase`(不要直接继承 `ModCardTemplate`——基线类按类名约定解析卡图 `res://STS2-Navia/images/cards/<类名>.png`,缺图自动回退占位),构造器传 `(费用, CardType, CardRarity, TargetType)`,`OnPlay`/`OnUpgrade` 覆写,XML doc 注释写设计文案。引用按实际使用的类型维护，可参考 `UmbrellaStrike.cs`。

映射表:
- 类型:攻击=Attack 技能=Skill 能力=Power
- 稀有度:**普通=Common,罕见=Uncommon,稀有=Rare**
- 关键词(加进 `CanonicalKeywords`):消耗=Exhaust 虚无=Ethereal 固有=Innate 保留=Retain
- 打击类卡必须显式声明 `protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };`。原版打击木偶等联动读取 `Tags`，不按本地化名称或 C# 类名判断；本模组对应打击/阳伞打击/猛铳打击/黄金打击，升级版沿用同一标记。新增或改名时按设计确认归属，不能按某种语言的标题自动推断。
- 目标:自身=Self 单体敌人=AnyEnemy
- 获得格挡的卡:`public override bool GainsBlock => true;`
- 多段攻击:`DamageCmd.Attack(...).WithHitCount(n)`;全体敌人参考 `Shiv.cs` 的 `TargetingAllOpponents(combatState)`

## 4. 实战踩坑清单(违反会出运行时 Bug)

1. **CalculatedVar 三件套**:卡面显示「每有 X 则 +Y」类动态数值,必须同时声明基准/每份/计算三件,
   显示值 = 基准 + 每份 × 份数。**三件缺一,卡牌创建时直接 KeyNotFoundException**(游戏内实测崩过)。
   multiplier 必须是静态 lambda 且空安全:`(card, _) => card.Owner?.Creature?.GetPowerAmount<LoadPower>() ?? 0`。
   **负缩放可用**:利斧强袭的「每层装填 -2」用负每份值。
   **伤害/格挡缩放卡必须用预览感知变体**(参考 vanilla PerfectedStrike/Stack 与铳弹齐射现状):
   - 伤害:`CalculationBaseVar(基准)` + **`ExtraDamageVar(每份)`** + **`CalculatedDamageVar(ValueProp.Move)`**;OnPlay 读 `base.DynamicVars.CalculatedDamage.Calculate(target)`
   - 格挡:`CalculationBaseVar(基准)` + `CalculationExtraVar(每份)` + **`CalculatedBlockVar(ValueProp.Move)`**;OnPlay 读 `base.DynamicVars.CalculatedBlock.Calculate(target)`
   - 通用 `CalculatedVar` 的面板**不含**力量/虚弱/敏捷修正,只用于非伤害格挡的自定义数值(如礼炮轰鸣值)。
2. **先算后耗**:凡「消耗装填结算效果」的卡,必须在消耗**之前**读取/计算数值(参考 `VolleyFire.OnPlay` 的顺序与注释;实测踩过:后算会把面板值算成基准值)。
3. `PowerCmd.Apply<T>(...)` 返回 `Task<T?>`,战斗结束等时点会返回 null,接返回值必须判空。
4. 消耗装填一律走 `LoadPower.Gain(ctx, creature, -n, ...)`,不要直接 `PowerCmd.ModifyAmount`(绕过上限规则没关系,负数本来不截断,但统一入口便于将来埋点)。
5. 自定义 Power 若不可见(`protected override bool IsVisibleInternal => false;`)不需要本地化;可见 Power 必须在中英本地化给 `STS2_NAVIA_POWER_<类名蛇形大写>.title/.description/.smartDescription` 三条(smartDescription 用 `[blue]{Amount}[/blue]` 引用当前层数,风格见 `localization/zhs/powers.json` 的 LOAD_POWER)。
6. 「下回合/回合开始/回合结束」时点钩子:覆写 `BeforeSideTurnStart`/`AfterSideTurnStart`(重置类,参考 `RosulaEmblem.cs`)或 `AfterSideTurnEnd`/回合结束钩子;参与者判断 `participants.Contains(base.Owner.Creature)`。
7. 监听其它 Power 层数变化:覆写 `AfterPowerAmountChanged(PlayerChoiceContext, PowerModel, decimal, Creature, CardModel?)`(签名以 `AbstractModel.cs` 为准),判断 `power is MoraCounterPower` 等即可感知「摩拉被打出」(delta>0)。
8. 给手牌中某张卡动态加关键词:查 vanilla `CardModel.AddKeyword`(ModKeywordRegistry 文档提及的原生 API)。

## 5. 本地化文本规范

- 最终键 = 复合 ID `STS2_NAVIA_CARD_<类名蛇形大写>.title/.description`(Power 为 `STS2_NAVIA_POWER_<类名蛇形大写>` 的 title/description/smartDescription 三件套)。此规则用于本模组内容；原版事件追加选项仍使用原版 Entry 下的键。
- 占位符:`{Damage:diff()} {Block:diff()} {自定义变量名:diff()} {LoadPower:diff()}`(PowerVar 的占位符是**类名**)。
- 富文本:`[gold]关键词[/gold]` 高亮;`{InCombat:\n(战斗内才显示的说明)|}` 条件段。
- **横幅关键词严禁在描述正文复述**(实测事故:闪耀摩拉「保留/消耗」横幅+正文双显示)——卡自带的
  消耗/保留/固有/虚无由关键词横幅渲染,正文只写行为;动作句不算复述(「消耗手中一张金花礼炮」可以)。
  升级获得的关键词同理(升级后横幅自动出现,正文永不写)。
- 原案表达意图，当前双语文案须准确说明实际行为。调整后同步更新设计差异，不能把原案旧文本不加核对地覆盖回当前本地化。

## 6. 升级(OnUpgrade)规范

- 数值升级:`DynamicVars.Damage.UpgradeValueBy(n)` / `DynamicVars["变量名"].UpgradeValueBy(n)` / `DynamicVars.Block.UpgradeValueBy(n)`。
- 三件套卡:升级要**同步 CalculationBase 与 Damage**(各 +n),「每份」只加增量——**注意键名随三件套类型而变**:伤害卡是 `DynamicVars.ExtraDamage`,格挡卡才是 `DynamicVars["CalculationExtra"]`(写错键名会在升级时 KeyNotFoundException,炸断卡牌初始化管线——手牌消失/打出不清理的整个链条都会断,实测事故)。
- 费用升级使用 `base.EnergyCost.UpgradeBy(delta)`；例如饱和爆破减一费，见 [SaturationBlast](../../src/STS2-Navia/Content/Cards/Powers/Rare/SaturationBlast.cs)。
- **升级增删关键词必须走 `OnUpgrade → AddKeyword/RemoveKeyword`**(`CardModel.LocalKeywords` 把 `CanonicalKeywords` 的结果快照缓存,首次访问后永不重算——条件式 `IsUpgraded ? X : 空` 的 `CanonicalKeywords` 三条路全死:运行时升级不生效/读档回放不生效/战斗克隆拷的是旧集;vanilla 读档靠回放 `UpgradeInternal()→OnUpgrade` 还原,Add/RemoveKeyword 恰好三路全通。实测事故:高额回报/金融市场「升级获得保留」从未生效)。反面示例已从本仓库清除,别再引入。

## 7. 变更交付

实现文件、两语言本地化、必要的资源映射和设计差异直接在公开仓库维护。运行公开源码检查，必要时编译并实测行为；报告命令、结果和未验证部分。不要将临时片段或本机文件作为贡献者的必需输入。

## 8. 设计意图 → 实现模式的既有约定

- 「礼炮轰鸣 N」= `Salvo.Fire(...)`;「装填 N」= `LoadPower.Gain(..., N, ...)`。
- 「打出 X 张摩拉触发」类效果:监听 `MoraCounterPower` 的 `AfterPowerAmountChanged`。
- 「本回合内」的兜售枪火、高价买入和金融市场使用 `AfterSideTurnEnd`，在所属阵营回合结束时移除；见 [PeddlingFirearmsPower](../../src/STS2-Navia/Content/Powers/PeddlingFirearmsPower.cs)。下一回合触发等其他能力按各自语义选生命周期，不能统一套用下次回合开始清理。
- 「消耗一张手牌/弃牌」:参考 `HiddenDaggers.cs` 的 `CardSelectCmd.FromHandForDiscard`。
- 「生成摩拉到抽牌堆」:`combatState.CreateCard<ShiningMora>(player)` + `CardPileCmd.AddGeneratedCardsToCombat(moras, PileType.DrawPile, player)`(PileType 枚举有 DrawPile,写法参考 `ShiningMora.CreateInHand`)。

### 持续效果与目标变化

- **持续型关键词授予**(「力量在场时你的 X 牌拥有 Y」):Power 覆写 `TryModifyKeywordsInCombat(card, keywords)`
  (HexPower 范式)——动态、全牌堆、随力量消失。**不要**逐实例 AddKeyword(其他牌堆的卡回来就漏,实测事故)。
  AddKeyword 只用于一次性永久变化(升级/附魔即改)。
- **改变卡的目标行为**(单体↔全体):卡类覆写 **TargetType 虚属性**按 Power 存在性返回(Shiv/FanOfKnives 范式,
  `IsMutable && Owner != null` 守卫),AllEnemies 即免选目标;OnPlay 用同一守卫分支结算管线。
