using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 众志成城(可见增益):每当你打出一张闪耀摩拉,获得等同于本 Power 层数的格挡。
/// 「打出闪耀摩拉」通过监听 <see cref="MoraCounterPower"/> 的层数变化感知:
/// 每打出一张 <c>ShiningMora.CountMoraPlayed</c> 会 +1(计数满 3 后的 -3 回落不会触发,delta&lt;=0 早退)。
/// 层数本身承载「每张格挡值」(基准 3,升级 5)。
/// </summary>
[RegisterPower]
public sealed class UnitedFrontPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // amount 为本次增量(每打出一张摩拉恰好 +1);只响应自己身上的摩拉计数,多人协作时不吃队友的计数。
        if (power is MoraCounterPower && amount > 0m && power.Owner == base.Owner)
        {
            Flash();
            await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
        }
    }
}
