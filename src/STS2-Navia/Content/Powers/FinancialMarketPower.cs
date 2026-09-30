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
/// 金融市场(临时可见增益):本回合内你每打出一张技能牌,在抽牌堆底部生成 1 张[闪耀摩拉]。
/// 自己回合结束时自移除(时点写法参考 vanilla OneTwoPunchPower);层数即「每张技能牌生成的摩拉数」。
/// </summary>
[RegisterPower]
public sealed class FinancialMarketPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Skill && cardPlay.Player.Creature == base.Owner)
        {
            await CreateMorasInDrawPile(base.Owner.Player);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Remove(this);
        }
    }

    private async Task CreateMorasInDrawPile(Player? player)
    {
        ICombatState? combatState = base.CombatState;
        if (player == null || combatState == null || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }
        int count = (int)base.Amount;
        List<ShiningMora> moras = new List<ShiningMora>(count);
        for (int i = 0; i < count; i++)
        {
            moras.Add(combatState.CreateCard<ShiningMora>(player));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(moras, PileType.Draw, player);
    }
}
