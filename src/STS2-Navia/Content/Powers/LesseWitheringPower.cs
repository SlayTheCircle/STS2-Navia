using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 乐斯侵蚀(可见减益):每个回合结束时,失去 {Amount} 点最大生命值。
/// 由「乐斯」战斗内饮用直接生效(药水已限定 CombatOnly),
/// 战斗结束随玩家 Power 被清空而移除。「每个回合」按己方回合结束结算(每轮一次),敌方回合不触发。
/// 最大生命损失走 vanilla <c>CreatureCmd.LoseMaxHp</c>:上限低于当前生命时自动转化为等量不可格挡伤害。
/// </summary>
[RegisterPower]
public sealed class LesseWitheringPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            Flash();
            await CreatureCmd.LoseMaxHp(choiceContext, base.Owner, base.Amount, isFromCard: false);
        }
    }
}
