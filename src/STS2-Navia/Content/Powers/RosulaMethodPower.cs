using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Enchantments;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 刺玫手段(可见增益):每回合,你打出的第一张带有[gold]支援[/gold]效果的牌可以免费打出。
/// 费用视 0 走 vanilla <c>FreeAttackPower</c> 范式:<c>TryModifyEnergyCostInCombatLate</c> 改费 +
/// <c>BeforeCardPlayed</c> 计数;每回合计数在 <c>BeforeSideTurnStart</c> 重置(vanilla <c>SlothPower</c> 范式)。
/// 层数无含义(Single 标记,固定 1)。
/// </summary>
[RegisterPower]
public sealed class RosulaMethodPower : NaviaPowerBase
{
    private int _supportCardsPlayedThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != base.Owner
            || _supportCardsPlayedThisTurn > 0
            || card.Enchantment is not NaviaSupportEnchantment
            || card.Pile?.Type is not (PileType.Hand or PileType.Play))
        {
            return false;
        }
        modifiedCost = 0m;
        return true;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner
            && cardPlay.Card.Enchantment is NaviaSupportEnchantment
            && cardPlay.Card.Pile?.Type is PileType.Hand or PileType.Play)
        {
            _supportCardsPlayedThisTurn++;
            Flash();
        }
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner))
        {
            _supportCardsPlayedThisTurn = 0;
        }
        return Task.CompletedTask;
    }
}
