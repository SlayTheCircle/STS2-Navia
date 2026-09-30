using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NaviaMod.Content.Enchantments;

/// <summary>
/// 免费支援(「超额支出」施加):被附魔的技能牌费用变为 0,并获得消耗(本场战斗)。
/// OnEnchant 在附魔刷新(如降级重算/反序列化)时会重跑,写法必须幂等:
/// - 费用清零用 Tezcatara's Ember 同款写法(目标为绝对 0,重跑时 addend=0 直接早退);
/// - AddKeyword 写入 LocalKeywords(HashSet),重复添加天然幂等(Royally Approved 同款)。
/// </summary>
[RegisterEnchantment]
public sealed class SupportFreeExhaust : NaviaSupportEnchantment
{
    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Skill;

    protected override void OnEnchant()
    {
        base.Card.EnergyCost.UpgradeBy(-base.Card.EnergyCost.GetWithModifiers(CostModifiers.None));
        base.Card.AddKeyword(CardKeyword.Exhaust);
    }
}
