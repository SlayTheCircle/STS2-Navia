# 现行卡表

<!-- 生成文件:scripts/export-card-table.py 从源码 ctor 与 zhs 本地化生成,勿手改。 -->
<!-- 过时校验:check.sh 调用 --check;再生成:python3 scripts/export-card-table.py -->

共 91 张（含衍生 token）。效果文本为当前运行文本;升级数值以源码与游戏内为准。
稀有度颜色对照：普通=白卡，罕见=蓝卡，稀有=金卡。
原案数值与设计过程见[技术历史](../history/design/README.md)。

## 初始卡（4）

| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |
|---|---|---|---|---|
| 防御 | 初始 | 1 | 技能 | 获得{Block:diff()}点[gold]格挡[/gold]。 |
| 快速装填 | 初始 | 1 | 技能 | 获得{Block:diff()}点[gold]格挡[/gold]，[gold]装填[/gold]{LoadPower:diff()}。 |
| 打击 | 初始 | 1 | 攻击 | 造成{Damage:diff()}点伤害。 |
| 铳弹齐射 | 初始 | 1 | 攻击 | 造成{Damage:diff()}点伤害。消耗全部[gold]装填[/gold]，每消耗1层，伤害+{ExtraDamage:diff()}。{InCombat:
（当前共造成{CalculatedDamage:diff()}点伤害）\|} |

## 攻击（30）

| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |
|---|---|---|---|---|
| 开辟航道 | 普通 | 2 | 攻击 | 造成{Damage:diff()}点伤害，[gold]礼炮轰鸣[/gold]{Salvo:diff()}。 |
| 双管齐下 | 普通 | 1 | 攻击 | 造成{Damage:diff()}点伤害{Hits:diff()}次。 |
| 黄金打击 | 普通 | 1 | 攻击 | 造成{Damage:diff()}点伤害。在手牌中生成{Moras:diff()}张[gold]闪耀摩拉[/gold]。 |
| 一枪爆头 | 普通 | 1 | 攻击 | 造成{Damage:diff()}点伤害。 |
| 空心弹头 | 普通 | 1 | 攻击 | 对所有敌人造成{Damage:diff()}点伤害，[gold]装填[/gold]{LoadPower:diff()}。 |
| 请君入瓮 | 普通 | 1 | 攻击 | 造成{Damage:diff()}点伤害，生成{Moras:diff()}张[gold]闪耀摩拉[/gold]加入手牌。 |
| 猛铳打击 | 普通 | 1 | 攻击 | 造成{Damage:diff()}点伤害。[gold]礼炮轰鸣[/gold]{Salvo:diff()}。 |
| 出其不意 | 普通 | 0 | 攻击 | 造成{Damage:diff()}点伤害，[gold]礼炮轰鸣[/gold]{Salvo:diff()}。 |
| 热情枪火 | 普通 | 1 | 攻击 | 造成{Damage:diff()}点伤害。如果有[gold]装填[/gold]，则消耗1层，获得{StrengthGain:diff()}点力量，再造成{BonusDamage:diff()}点伤害。 |
| 会长号令 | 普通 | 1 | 攻击 | 造成{Damage:diff()}点伤害。选择手中一张攻击牌，使其获得[gold]支援[/gold]：本场战斗中，该卡造成伤害+{SupportAmount:diff()}。 |
| 稳扎稳打 | 普通 | 2 | 攻击 | 造成{Damage:diff()}点伤害。获得{Block:diff()}点[gold]格挡[/gold]。
你的下一回合开始时[gold]格挡[/gold]不会消失。 |
| 阳伞打击 | 普通 | 1 | 攻击 | 造成{Damage:diff()}点伤害，[gold]装填[/gold]{LoadPower:diff()}。 |
| 枪刺拼杀 | 罕见 | 1 | 攻击 | 造成{Damage:diff()}点伤害。自身获得{SelfVulnerable:diff()}层[gold]易伤[/gold]，给予{VulnerablePower:diff()}层[gold]易伤[/gold]。 |
| 冒险收获 | 罕见 | 0 | 攻击 | 造成{Damage:diff()}点伤害。抽{Cards:diff()}张牌。将你的1张手牌放到[gold]抽牌堆[/gold]顶部。 |
| 扩容弹夹 | 罕见 | 2 | 攻击 | 造成{Damage:diff()}点伤害，[gold]装填[/gold]{LoadPower:diff()}。 |
| 豪掷千金 | 罕见 | 1 | 攻击 | 造成{Damage:diff()}点伤害{Hits:diff()}次。下一次打出[gold]闪耀摩拉[/gold]时，[gold]装填[/gold]1。 |
| 火力增幅 | 罕见 | 2 | 攻击 | 造成{Damage:diff()}点伤害，[gold]礼炮轰鸣[/gold]{Salvo:diff()}。将一张[gold]金花礼炮[/gold]加入你的手牌。 |
| 高歌猛进 | 罕见 | 1 | 攻击 | 造成{Damage:diff()}点伤害。抽牌堆中的1张随机攻击牌获得[gold]支援[/gold]：造成的伤害-{SupportAmount:diff()}。 |
| 横行灰河 | 罕见 | 2 | 攻击 | 造成{Damage:diff()}点伤害。手牌中每有一张带有[gold]支援[/gold]效果的牌，抽1张牌并恢复1点能量。 |
| 坚船利炮 | 罕见 | 0 | 攻击 | 对所有敌人造成{Damage:diff()}点伤害X次。你当前每有1层[gold]装填[/gold]，这张卡的伤害+{ExtraDamage:diff()}（不消耗装填）。{InCombat:
（当前每次造成{CalculatedDamage:diff()}点伤害）\|} |
| 伸出援手 | 罕见 | 1 | 攻击 | 造成{Damage:diff()}点伤害。本场战斗中，所有带有[gold]支援[/gold]效果的攻击牌造成的伤害+{AuraAmount:diff()}。 |
| 先发制人 | 罕见 | 0 | 攻击 | 造成{Damage:diff()}点伤害，给予{VulnerablePower:diff()}层[gold]易伤[/gold]，给予{WeakPower:diff()}层[gold]虚弱[/gold]。 |
| 例行检查 | 罕见 | 2 | 攻击 | 造成{Damage:diff()}点伤害。手牌中每有一张无色牌，伤害+{ExtraDamage:diff()}。{InCombat:
（当前共造成{CalculatedDamage:diff()}点伤害）\|} |
| 乘胜追击 | 罕见 | 0 | 攻击 | 当前每有1层[gold]装填[/gold]，造成{ExtraDamage:diff()}点伤害、获得等量[gold]格挡[/gold]、[gold]礼炮轰鸣[/gold]1。随后将[gold]装填[/gold]层数减半（向下取整）。{InCombat:
（当前共造成{CalculatedDamage:diff()}点伤害并获得等量格挡）\|} |
| 轻装上阵 | 罕见 | 1 | 攻击 | 造成{Damage:diff()}点伤害。如果你的手中有[gold]金花礼炮[/gold]，则选择一张消耗，再造成{Damage:diff()}点伤害，并获得与该卡造成伤害总量相同的[gold]格挡[/gold]。 |
| 风险投资 | 罕见 | 1 | 攻击 | 造成{Damage:diff()}点伤害。本回合内，你之后的每次命中这名敌人，都会在抽牌堆底部生成一张[gold]闪耀摩拉[/gold]。 |
| 利斧强袭 | 稀有 | 1 | 攻击 | 造成{Damage:diff()}点伤害，给予{VulnerablePower:diff()}层[gold]易伤[/gold]。你当前每有1层[gold]装填[/gold]，这张卡的伤害{ExtraDamage:diff()}点（不消耗装填）。{InCombat:
（当前造成{CalculatedDamage:diff()}点伤害）\|} |
| 最终突击 | 稀有 | 3 | 攻击 | 造成{Damage:diff()}点伤害。自身获得{VulnerablePower:diff()}层[gold]易伤[/gold]。 |
| 强制买断 | 稀有 | 2 | 攻击 | 将消耗堆中所有的[gold]闪耀摩拉[/gold]对一名敌人打出。每打出1张，获得{Block:diff()}点[gold]格挡[/gold]。 |
| 做空市场 | 稀有 | 0 | 攻击 | 造成{Damage:diff()}点伤害。本场战斗中，你每打出1张[gold]闪耀摩拉[/gold]，都会令这张卡的伤害+{MoraBonus:diff()}。 |

## 技能（32）

| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |
|---|---|---|---|---|
| 军火流通 | 普通 | 1 | 技能 | [gold]礼炮轰鸣[/gold]{Salvo:diff()}。丢弃1张手牌，若丢弃了[gold]金花礼炮[/gold]，获得1点力量，对所有敌人造成{BonusDamage:diff()}点伤害，[gold]装填[/gold]1。 |
| 软硬兼施 | 普通 | 1 | 技能 | 给予所有敌人{WeakPower:diff()}层[gold]虚弱[/gold]，再给予所有敌人{VulnerablePower:diff()}层[gold]易伤[/gold]。 |
| 抛射弹壳 | 普通 | 0 | 技能 | 获得{Block:diff()}点[gold]格挡[/gold]。如果有至少3层[gold]装填[/gold]，消耗1层，抽1张牌。如果[gold]装填[/gold]层数超过3，则改为消耗2层，抽2张牌。 |
| 金融防线 | 普通 | 0 | 技能 | 获得{Block:diff()}点[gold]格挡[/gold]。生成{Moras:diff()}张[gold]闪耀摩拉[/gold]加入手牌。 |
| 率直作风 | 普通 | 1 | 技能 | 抽{Cards:diff()}张牌。选择抽牌堆中的一张牌，使其获得[gold]支援[/gold]：该卡获得[gold]消耗[/gold]。 |
| 简易护甲 | 普通 | 0 | 技能 | 获得{Block:diff()}点[gold]格挡[/gold]。 |
| 补充物资 | 普通 | 1 | 技能 | 消耗你所有的[gold]装填[/gold]，每消耗2层，获得1点能量。 |
| 鸣枪示意 | 普通 | 0 | 技能 | 消耗至多{MaxSpend:diff()}层[gold]装填[/gold]，每消耗1层，抽1张牌，[gold]礼炮轰鸣[/gold]1。 |
| 捍卫战线 | 罕见 | 3 | 技能 | 获得{Block:diff()}点[gold]格挡[/gold]，获得{WeakPower:diff()}层[gold]虚弱[/gold]。
在下个回合获得{Energy:energyIcons()}。 |
| 金融市场 | 罕见 | 1 | 技能 | 本回合内，你每打出一张技能牌，就在抽牌堆底部生成一张[gold]闪耀摩拉[/gold]。 |
| 募集资金 | 罕见 | 1 | 技能 | 选择抽牌堆中的1张牌，使其获得[gold]支援[/gold]：打出时，在手牌中生成{SupportAmount:diff()}张[gold]闪耀摩拉[/gold]。 |
| 华丽开场 | 罕见 | 0 | 技能 | 生成{Moras:diff()}张[gold]闪耀摩拉[/gold]加入手牌，抽{Cards:diff()}张牌。 |
| 灰河硝烟 | 罕见 | 0 | 技能 | 消耗你手中全部的状态牌与诅咒牌。每消耗1张，[gold]礼炮轰鸣[/gold]1。 |
| 高额回报 | 罕见 | 0 | 技能 | 本回合内，你每次打出或丢弃带有[gold]支援[/gold]效果的牌，都会在手牌中生成一张[gold]闪耀摩拉[/gold]。 |
| 据守阵地 | 罕见 | 1 | 技能 | 获得{Block:diff()}点[gold]格挡[/gold]。使手牌中一张牌获得[gold]消耗[/gold]。 |
| 固若金汤 | 罕见 | 2 | 技能 | 获得{CalculatedBlock:diff()}点[gold]格挡[/gold]。当前每有1层[gold]装填[/gold]，格挡+{CalculationExtra:diff()}。 |
| 邻里互助 | 罕见 | 1 | 技能 | 消耗你手牌中的任意张牌。选择抽牌堆中等量的牌，使其获得[gold]支援[/gold]：打出时获得{SupportAmount:diff()}点[gold]格挡[/gold]。 |
| 超额支出 | 罕见 | 0 | 技能 | 选择手中一张技能牌，使其获得[gold]支援[/gold]：该卡本场战斗中费用为0，并获得[gold]消耗[/gold]。 |
| 兜售枪火 | 罕见 | 1 | 技能 | [gold]装填[/gold]{LoadPower:diff()}。本回合内，[gold]闪耀摩拉[/gold]额外造成{MoraBonus:diff()}点伤害。 |
| 高价买入 | 罕见 | 1 | 技能 | 你手牌中每有1张[gold]金花礼炮[/gold]，抽1张牌。 |
| 精制装药 | 罕见 | 0 | 技能 | [gold]礼炮轰鸣[/gold]{Salvo:diff()}。如果有[gold]装填[/gold]，则消耗1层，将此卡返回你的手牌。 |
| 铸剑为犁 | 罕见 | 0 | 技能 | 消耗手中1张[gold]金花礼炮[/gold]，获得2点能量。 |
| 紧急调度 | 罕见 | 1 | 技能 | 抽{Cards:diff()}张牌。丢弃其中所有费用为0的牌。 |
| 砥兵备战 | 罕见 | 0 | 技能 | 抽1张牌。[gold]装填[/gold]1。[gold]礼炮轰鸣[/gold]1。在手牌中生成1张[gold]闪耀摩拉[/gold]。 |
| 追加订单 | 稀有 | 1 | 技能 | 将手牌中一张牌的带有[gold]消耗[/gold]的复制品加入手牌。 |
| 破甲兵装 | 稀有 | 1 | 技能 | 失去你当前的所有[gold]格挡[/gold]。每失去3点，就对所有敌人造成{Damage:diff()}点伤害。
你的下一回合开始时[gold]格挡[/gold]不会消失。 |
| 紧急避险 | 稀有 | 0 | 技能 | 消耗全部[gold]装填[/gold]，每消耗2层获得{CalculationExtra:diff()}点[gold]格挡[/gold]，并在下回合[gold]装填[/gold]1。{InCombat:
（当前共获得{CalculatedBlock:diff()}点格挡）\|} |
| 庆贺礼炮 | 稀有 | 2 | 技能 | 选择手中一张[gold]金花礼炮[/gold]，将其对随机敌人打出3次。 |
| 贪婪枪火 | 稀有 | 1 | 技能 | 本回合内，你每次打出[gold]闪耀摩拉[/gold]，都会消耗1层[gold]装填[/gold]，在战斗结束时额外获得{Gold:diff()}金币。 |
| 热情昂扬 | 稀有 | 2 | 技能 | 选择抽牌堆中的1张牌，使其获得[gold]支援[/gold]：打出带有支援效果的牌时，若其仍在手牌中，自动将其打出。 |
| 火力覆盖 | 稀有 | 1 | 技能 | 若手牌中已有[gold]金花礼炮[/gold]，使手牌中所有[gold]金花礼炮[/gold]的伤害次数+1。然后[gold]礼炮轰鸣[/gold]{Salvo:diff()}。 |
| 鸟枪换炮 | 稀有 | 1 | 技能 | 消耗手牌中所有的[gold]闪耀摩拉[/gold]，每消耗1张，获得1点能量。 |

## 能力（20）

| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |
|---|---|---|---|---|
| 生财有道 | 罕见 | 1 | 能力 | 每回合，你第一次打出[gold]闪耀摩拉[/gold]时，将一张它的带有[gold]虚无[/gold]的复制品置入弃牌堆。 |
| 囤积物资 | 罕见 | 2 | 能力 | 回合开始时，手牌中每有1张[gold]闪耀摩拉[/gold]，[gold]礼炮轰鸣[/gold]{HoardingSuppliesPower:diff()}。 |
| 牟取利益 | 罕见 | 2 | 能力 | [gold]闪耀摩拉[/gold]现在会额外造成{MoraBonus:diff()}点伤害。 |
| 炮身加固 | 罕见 | 1 | 能力 | 你的[gold]金花礼炮[/gold]每对1个敌人造成1次伤害，获得{ReinforcedBarrelPower:diff()}点[gold]格挡[/gold]。 |
| 众志成城 | 罕见 | 2 | 能力 | 每当你打出一张[gold]闪耀摩拉[/gold]，获得{UnitedFrontPower:diff()}点[gold]格挡[/gold]。 |
| 武器保养 | 罕见 | 1 | 能力 | 将1张去除了[gold]消耗[/gold]的[gold]金花礼炮[/gold]加入你的手牌。 |
| 意外事故 | 稀有 | 2 | 能力 | 你每消耗1张[gold]金花礼炮[/gold]，都获得{AccidentalBlastPower:diff()}点[gold]格挡[/gold]，[gold]装填[/gold]1。 |
| 军火大亨 | 稀有 | 2 | 能力 | 回合开始时，[gold]礼炮轰鸣[/gold]{Salvo:diff()}。 |
| 亏空平账 | 稀有 | 1 | 能力 | 每当你获得负面状态时，在你的抽牌堆中生成2张[gold]闪耀摩拉[/gold]。 |
| 炮火连天 | 稀有 | 1 | 能力 | 每回合你第一次打出[gold]金花礼炮[/gold]时，使接下来所有[gold]金花礼炮[/gold]的伤害次数+1。 |
| 回收利息 | 稀有 | 2 | 能力 | 你每获得3层[gold]装填[/gold]，便获得1点能量。 |
| 指挥形态 | 稀有 | 3 | 能力 | 每有1张牌被消耗，[gold]装填[/gold]{CommandStancePower:diff()}。 |
| 危险改装 | 稀有 | 2 | 能力 | [gold]装填[/gold]{LoadPower:diff()}。每回合开始时，失去1层[gold]装填[/gold]。 |
| 引导轰炸 | 稀有 | 2 | 能力 | [gold]礼炮轰鸣[/gold]{Salvo:diff()}。每当你打出[gold]金花礼炮[/gold]时，抽1张牌。 |
| 高压弹膛 | 稀有 | 1 | 能力 | 每当你的[gold]装填[/gold]减少1层（无论何种原因），对所有敌人造成{HighPressureChamberPower:diff()}点伤害。 |
| 通货膨胀 | 稀有 | 2 | 能力 | 你接下来生成的[gold]闪耀摩拉[/gold]伤害降低1点，并会在打出时抽1张牌。 |
| 乐观估测 | 稀有 | 2 | 能力 | 每当你打出一张带有[gold]支援[/gold]效果的牌，抽1张牌。 |
| 穿心膛线 | 稀有 | 1 | 能力 | [gold]装填[/gold]{LoadPower:diff()}。[gold]装填[/gold]的层数上限提升至9层。 |
| 刺玫手段 | 稀有 | 2 | 能力 | 每回合，你打出的第一张带有[gold]支援[/gold]效果的牌可以免费打出。 |
| 饱和爆破 | 稀有 | 2 | 能力 | [gold]金花礼炮[/gold]现在会攻击所有敌人。 |

## 先古强化（3）

| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |
|---|---|---|---|---|
| 枪炮轰鸣 | 先古 | 1 | 攻击 | 造成{Damage:diff()}点伤害。消耗全部[gold]装填[/gold]，每消耗1层，伤害+{ExtraDamage:diff()}，给予{VulnerablePower:diff()}层[gold]易伤[/gold]。{InCombat:
（当前共造成{CalculatedDamage:diff()}点伤害）\|} |
| 掩护轰炸 | 先古 | 2 | 能力 | 回合开始时，自身每有1层[gold]装填[/gold]，便在本回合获得1点力量。 |
| 神速装填 | 先古 | 1 | 技能 | 获得{Block:diff()}点[gold]格挡[/gold]，[gold]装填[/gold]{LoadPower:diff()}。 |

## 衍生（2）

| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |
|---|---|---|---|---|
| 金花礼炮 | 衍生 | 0 | 攻击 | 造成{Damage:diff()}点伤害{Hits:diff()}次。 |
| 闪耀摩拉 | 衍生 | 0 | 攻击 | 造成{Damage:diff()}点伤害。每打出3张[gold]闪耀摩拉[/gold]，[gold]装填[/gold]1。 |
