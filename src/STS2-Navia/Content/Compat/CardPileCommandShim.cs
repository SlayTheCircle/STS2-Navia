using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace NaviaMod.Content.Compat;

/// <summary>跨玩家抽牌的双版本入口；0.111 起不阻塞来源动作，0.107.1 使用普通抽牌流程。</summary>
internal static class NaviaCardPileCommandShim
{
    public static async Task DrawForPlayer(PlayerChoiceContext choiceContext, decimal count, Player player, CardModel source)
    {
#if NAVIA_GAME_0107_1
        await CardPileCmd.Draw(choiceContext, count, player);
#else
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, count, player, source);
#endif
    }
}
