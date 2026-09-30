using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NaviaMod.Content.Enchantments;

/// <summary>
/// 火力压制支援(「高歌猛进」施加):被附魔的攻击牌造成的伤害 -Amount(本场战斗)。
/// 与 SupportDamageUp 同走 EnchantDamageAdditive 专用钩子——先于遗物/能力等其他伤害修改,
/// 预览口径一致;负值增量合法,最终下限由伤害管线统一钳制。
/// </summary>
[RegisterEnchantment]
public sealed class SupportDamageDown : NaviaSupportEnchantment
{
    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props) => -Amount;
}
