using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 一次性格挡保留(可见增益):自身回合开始时的格挡清除被跳过一次,随后自减 1 层,归零即移除。
/// 机制仿 vanilla BlurPower(回合开始顺序:ClearBlock → AfterBlockCleared → AfterSideTurnStart)。
/// 「稳扎稳打」「破甲兵装」共用;叠加层数可连续跳过多次清除。
/// </summary>
[RegisterPower]
public sealed class OneTurnBlockPersistPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldClearBlock(Creature creature)
    {
        // 返回 false 即阻止该生物的回合开始格挡清除;只保护自己身上的格挡。
        return base.Owner != creature;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 此时本回合的 ClearBlock 已被上面的钩子跳过,自减 1 层(1 层时随之移除)。
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Decrement(this);
        }
    }
}
