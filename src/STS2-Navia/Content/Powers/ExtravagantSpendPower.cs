using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 豪掷千金的一次性标记(可见增益):下一次打出[闪耀摩拉]时按层数装填,随后自移除。
/// 通过监听 <see cref="MoraCounterPower"/> 的层数增量(delta &gt; 0)感知「摩拉被打出」。
/// 同回合重复打出会叠层合并;触发窗口无时限,直到打出摩拉为止。
/// </summary>
[RegisterPower]
public sealed class ExtravagantSpendPower : NaviaPowerBase
{

    /// <summary>文本提及机制名词,挂关键词词条悬停解释。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Load);
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is MoraCounterPower && amount > 0m && power.Owner == base.Owner)
        {
            await LoadPower.Gain(choiceContext, base.Owner, (int)base.Amount, base.Owner, cardSource);
            await PowerCmd.Remove(this);
        }
    }
}
