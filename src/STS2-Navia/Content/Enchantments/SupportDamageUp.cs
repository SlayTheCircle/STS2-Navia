using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NaviaMod.Content.Enchantments;

/// <summary>
/// 火力支援(「会长号令」施加):被附魔的攻击牌造成的伤害 +Amount(本场战斗)。
/// 走 EnchantDamageAdditive 专用钩子——先于遗物/能力等其他伤害修改,预览口径一致。
/// </summary>
[RegisterEnchantment]
public sealed class SupportDamageUp : NaviaSupportEnchantment
{
    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props) => Amount;
}
