using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 闪耀摩拉打出计数(隐藏增益):每累计 3 层,自动消耗 3 层并装填 1。
/// 逻辑在 <see cref="Cards.ShiningMora.CountMoraPlayed"/> 中驱动,本类只承载持久层数。
/// 需要感知「摩拉被打出」的效果(众志成城/囤积物资/摩拉袋子等)请订阅本类的层数变化或在其驱动点挂接。
/// </summary>
[RegisterPower]
public sealed class MoraCounterPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;
}
