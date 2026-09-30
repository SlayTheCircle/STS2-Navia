using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Enchantments;

/// <summary>
/// 摩拉支援(「募集资金」施加):被附魔的牌每次打出时,在手牌中生成 Amount 张闪耀摩拉(本场战斗)。
/// 走附魔的 OnPlay 钩子(vanilla 卡牌打出管线在卡自身 OnPlay 后调用 Enchantment.OnPlay,每次打出各触发一次);
/// 生成入口复用 <see cref="ShiningMora.CreateInHand"/>(含战斗结束/数量非法的静默短路)。
/// </summary>
[RegisterEnchantment]
public sealed class SupportMoraOnPlay : NaviaSupportEnchantment
{
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await ShiningMora.CreateInHand(base.Card.Owner, base.Amount, base.Card.CombatState);
    }
}
