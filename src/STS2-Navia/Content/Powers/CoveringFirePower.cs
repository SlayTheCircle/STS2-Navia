using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 掩护轰炸(先古能力):回合开始时,按当前[装填]层数获得等量**本回合**力量——
/// 走 ±StrengthPower(回合末如数移除,记 _grantedThisTurn 精确对冲,即使中途装填变化也不多扣)。
/// 战斗结束引擎清 Power,无泄漏。StackType=Single。
/// </summary>
[RegisterPower]
public sealed class CoveringFirePower : NaviaPowerBase
{
    private int _grantedThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        _grantedThisTurn = 0;
        if (side != base.Owner.Side || !base.Owner.IsAlive)
        {
            return;
        }
        int load = base.Owner.GetPowerAmount<LoadPower>();
        if (load > 0)
        {
            _grantedThisTurn = load;
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, load, base.Owner, null);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side && _grantedThisTurn > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -_grantedThisTurn, base.Owner, null);
            _grantedThisTurn = 0;
        }
    }
}
