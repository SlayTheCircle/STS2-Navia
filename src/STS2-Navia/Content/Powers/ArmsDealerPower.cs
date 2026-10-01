using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Mechanics;
using MegaCrit.Sts2.Core.HoverTips;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 军火大亨的光环(可见增益,数值调整V1):回合开始时,[礼炮轰鸣]层数值(升级后 2)。
/// 层数=每次轰鸣的数值,经 <see cref="Salvo.Fire"/> 统一结算。
/// </summary>
[RegisterPower]
public sealed class ArmsDealerPower : NaviaPowerBase
{

    /// <summary>文本提及机制名词,挂关键词词条悬停解释。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Salvo);
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Side && base.Owner.IsAlive && base.Owner.Player is { } player)
        {
            Flash();
            await Salvo.Fire(combatState, choiceContext, player, base.Amount);
        }
    }
}
