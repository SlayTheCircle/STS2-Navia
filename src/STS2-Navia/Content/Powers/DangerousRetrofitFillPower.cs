using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 危险改装的补满倒数(可见,数值调整V4):层数=剩余补满回合数(打出时 3)。
/// 回合开始时先失去 1 层[装填],再将[装填]补至当前上限,然后自减 1 层;归零即移除,
/// 此后回合流失由 <see cref="DangerousRetrofitPower"/> 接管——两个 Power 用在场判断互斥,
/// 不依赖回合钩子的迭代顺序。
/// </summary>
[RegisterPower]
public sealed class DangerousRetrofitFillPower : NaviaPowerBase
{

    /// <summary>文本提及机制名词,挂关键词词条悬停解释。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Load);
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(base.Owner))
        {
            return;
        }
        // 先失去(有装填才失去,避免负数),后补满——设计者定夺的结算顺序。
        if (base.Owner.GetPowerAmount<LoadPower>() > 0)
        {
            await LoadPower.Gain(choiceContext, base.Owner, -1, base.Owner, null);
        }
        int deficit = LoadPower.CapFor(base.Owner) - base.Owner.GetPowerAmount<LoadPower>();
        if (deficit > 0)
        {
            await LoadPower.Gain(choiceContext, base.Owner, deficit, base.Owner, null);
        }
        // 自减(原版 Decrement 习语,避免 Apply 的 applier 匹配问题);归零时框架自动移除本 Power。
        await PowerCmd.Decrement(this);
    }
}
