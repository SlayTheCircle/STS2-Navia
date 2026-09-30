using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 指挥形态(可见增益):每有 1 张你的牌被消耗,装填 {Amount}。
/// 钩 <c>AfterCardExhausted</c>(vanilla DarkEmbracePower 同款入口),装填经由 <see cref="LoadPower.Gain"/> 走上限规则。
/// </summary>
[RegisterPower]
public sealed class CommandStancePower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Owner.Creature == base.Owner)
        {
            Flash();
            await LoadPower.Gain(choiceContext, base.Owner, (int)base.Amount, base.Owner, null);
        }
    }
}
