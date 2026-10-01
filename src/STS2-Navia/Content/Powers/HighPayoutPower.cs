using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Enchantments;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 高额回报的回合内标记(可见增益):本回合内,你每次打出或丢弃带有[gold]支援[/gold]效果的牌,
/// 都会在手牌中生成 <see cref="Amount"/> 张[gold]闪耀摩拉[/gold]。
/// 打出走 <c>AfterCardPlayed</c>;「丢弃」= 弃牌堆口径,走 <c>AfterCardDiscarded</c>
/// (vanilla 仅在 CardCmd.Discard 路径触发该钩,打出落弃牌堆不会双计;消耗走 Exhaust 钩,不触发)。
/// 自己回合结束时自移除(时点写法同 <see cref="FinancialMarketPower"/>)。
/// </summary>
[RegisterPower]
public sealed class HighPayoutPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.GetPlayer().Creature == base.Owner && cardPlay.Card.Enchantment is NaviaSupportEnchantment)
        {
            Flash();
            await ShiningMora.CreateInHand(base.Owner.Player!, (int)base.Amount, base.CombatState);
        }
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.Owner.Creature == base.Owner && card.Enchantment is NaviaSupportEnchantment)
        {
            Flash();
            await ShiningMora.CreateInHand(base.Owner.Player!, (int)base.Amount, base.CombatState);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
