using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 通货膨胀(进场钩子 + 打出钩子):之后进场的闪耀摩拉伤害 -1(实例 BaseValue 直改,Salvo 抬升同款手法),
/// 打出时抽 1。已在场摩拉不改——「接下来生成的」口径,由 AfterCardEnteredCombat 天然保证。
/// StackType=Single:重复施加不叠(再挂一张通胀不把 -1 变 -2)。
/// </summary>
[RegisterPower]
public sealed class InflationPower : NaviaPowerBase
{
    private readonly HashSet<CardModel> _adjusted = new HashSet<CardModel>();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card is ShiningMora && card.Owner == base.Owner.Player)
        {
            card.DynamicVars.Damage.BaseValue -= 1m;
            _adjusted.Add(card);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_adjusted.Contains(cardPlay.Card))
        {
            await CardPileCmd.Draw(choiceContext, 1m, base.Owner.Player);
        }
    }
}
