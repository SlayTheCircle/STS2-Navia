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
using MegaCrit.Sts2.Core.Models;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 紧急避险的延迟装填标记(可见):自己所在阵营的下一次回合开始时按层数装填,随后自移除。
/// 由 <see cref="Cards.EmergencyEvade"/> 施加;同回合重复打出会叠层合并,下回合一次性装填总和。
/// </summary>
[RegisterPower]
public sealed class EmergencyEvadePower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner))
        {
            // 该钩子无 PlayerChoiceContext 参数,装填不需要玩家抉择,用 Throwing 上下文即可。
            await LoadPower.Gain(new ThrowingPlayerChoiceContext(), base.Owner, (int)base.Amount, base.Owner, null);
            await PowerCmd.Remove(this);
        }
    }
}
