using MegaCrit.Sts2.Core.Entities.Players;

// 0.107.1 稳定版垫片(其二):CardPlay.Player 属性为 0.111 新增。
// 统一经 GetPlayer 取「打出者」,调用点两目标共用同一形;0.111 走原生属性,0.107.1 由卡牌 Owner 等价取值。

namespace MegaCrit.Sts2.Core.Entities.Cards;

internal static class NaviaCardPlayShim
{
#if !NAVIA_GAME_0107_1
    public static Player GetPlayer(this CardPlay cardPlay)
        => cardPlay.Player;
#else
    public static Player GetPlayer(this CardPlay cardPlay)
        => cardPlay.Card.Owner;
#endif
}
