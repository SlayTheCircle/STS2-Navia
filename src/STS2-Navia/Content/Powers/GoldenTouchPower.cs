using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 生财有道(可见增益):每回合,你第一次打出[闪耀摩拉]时,将一张它的带有虚无的复制品置入弃牌堆。
/// 「每回合第一次」用回合计数字段在 <c>AfterSideTurnEnd</c> 重置(vanilla JugglingPower 同款);
/// 复制走 <c>CardModel.CreateCloneForPlayer</c>(JugglingPower/DualWield 范式,此刻摩拉在出牌堆,
/// 属战斗牌堆可克隆),加虚无走原生 <c>AddKeyword</c>,入弃牌堆走 <c>AddGeneratedCardToCombat</c>。
/// 层数无含义(Single 标记,固定 1)。
/// </summary>
[RegisterPower]
public sealed class GoldenTouchPower : NaviaPowerBase
{
    private bool _moraPlayedThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (_moraPlayedThisTurn
            || cardPlay.Player.Creature != base.Owner
            || cardPlay.Card is not ShiningMora)
        {
            return;
        }
        // 标记先落地:「这一张就是本回合的第一张摩拉」,生成与否是另一回事(战斗收尾时不再补牌)。
        _moraPlayedThisTurn = true;
        ICombatState? combatState = base.CombatState;
        Player? player = base.Owner.Player;
        if (player == null || combatState == null || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }
        Flash();
        CardModel copy = cardPlay.Card.CreateCloneForPlayer(player);
        copy.AddKeyword(CardKeyword.Ethereal);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Discard, player);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            _moraPlayedThisTurn = false;
        }
        return Task.CompletedTask;
    }
}
