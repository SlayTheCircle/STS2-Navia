using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 兜售枪火(临时可见增益):本回合内[闪耀摩拉]额外造成 3(升级 5)点伤害。
/// 层数即「额外伤害值」;自己回合结束时自移除(时点写法参考 vanilla OneTwoPunchPower,
/// 伤害加成参考 AccuracyPower 的 ModifyDamageAdditive)。
/// </summary>
[RegisterPower]
public sealed class PeddlingFirearmsPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? card, CardPlay? cardPlay)
    {
        if (base.Owner != dealer)
        {
            return 0m;
        }
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }
        if (card is not ShiningMora)
        {
            return 0m;
        }
        return base.Amount;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
