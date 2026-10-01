using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

// 0.107.1 稳定版垫片(其一):FromCard 的 CardPlay 参数为 0.111 新增(动作同步用)。
// 扩展类放进游戏命名空间,既有调用点零改动获得同形重载;0.111 原生实例方法优先于扩展,不受影响。

namespace MegaCrit.Sts2.Core.Commands;

internal static class NaviaAttackCommandShim
{
#if NAVIA_GAME_0107_1
    public static AttackCommand FromCard(this AttackCommand command, CardModel card, CardPlay? cardPlay)
        => command.FromCard(card);
#endif
}
