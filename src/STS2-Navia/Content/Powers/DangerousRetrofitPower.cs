using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 危险改装的回合衰减标记(可见):自己所在阵营的每回合开始时失去 1 层[装填]。
/// 由 <see cref="Cards.DangerousRetrofit"/> 施加,持续整场战斗。
/// </summary>
[RegisterPower]
public sealed class DangerousRetrofitPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 有装填才失去,避免层数被推成负数。
        if (participants.Contains(base.Owner) && base.Owner.GetPowerAmount<LoadPower>() > 0)
        {
            await LoadPower.Gain(choiceContext, base.Owner, -1, base.Owner, null);
        }
    }
}
