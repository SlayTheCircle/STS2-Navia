using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Compat;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Powers;

/// <summary>刺玫同盟:每回合持有者首次打出支援牌后，其他存活玩家各抽 Amount 张牌。以出牌记录身份保护嵌套自动出牌时的首次资格。</summary>
[RegisterPower]
public sealed class RosulaAlliancePower : NaviaPowerBase
{
    private bool _triggeredThisTurn;
    private CardPlay? _pendingFirstSupport;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Support);

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (!_triggeredThisTurn
            && cardPlay.Card.Enchantment is NaviaSupportEnchantment
            && cardPlay.GetPlayer().Creature == base.Owner
            && cardPlay.Card.Pile?.Type is PileType.Hand or PileType.Play)
        {
            _triggeredThisTurn = true;
            _pendingFirstSupport = cardPlay;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!ReferenceEquals(_pendingFirstSupport, cardPlay))
        {
            return;
        }
        _pendingFirstSupport = null;
        Flash();
        foreach (Creature teammate in base.Owner.CombatState!.GetTeammatesOf(base.Owner).Where(c => c.IsAlive && c.Player != null && c != base.Owner))
        {
            await NaviaCardPileCommandShim.DrawForPlayer(choiceContext, base.Amount, teammate.Player!, cardPlay.Card);
        }
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner))
        {
            _triggeredThisTurn = false;
            _pendingFirstSupport = null;
        }
        return Task.CompletedTask;
    }
}
