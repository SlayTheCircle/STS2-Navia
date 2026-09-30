using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Enchantments;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 伸出援手的光环(可见增益):本场战斗中,所有带有[gold]支援[/gold]效果的攻击牌造成的伤害 +Amount。
/// 走能力链 <c>ModifyDamageAdditive</c>(vanilla <c>StrengthPower</c> 范式)——与附魔的
/// <c>EnchantDamageAdditive</c> 是不同链,合法叠加;dealer 限定为持有者,敌人打我们的伤害不吃加成。
/// 层数=每张牌的伤害加成(基础 2,升级 3;重复施加自然叠层)。
/// </summary>
[RegisterPower]
public sealed class LendAHandPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (base.Owner != dealer
            || cardSource?.Enchantment is not NaviaSupportEnchantment
            || cardSource.Type != CardType.Attack
            || !props.IsPoweredAttack())
        {
            return 0m;
        }
        return base.Amount;
    }
}
