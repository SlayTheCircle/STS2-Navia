using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 饱和爆破(查询式标记):在场时,金花礼炮的打出改为对所有敌人结算。
/// 逻辑在 GoldenRoseCannon.OnPlay 里按 GetPower&lt;SaturationBlastPower&gt; 分支(礼炮是我们自己的类,
/// 直改比挂钩子干净)。StackType=Single:二进制语义不叠加。
/// </summary>
[RegisterPower]
public sealed class SaturationBlastPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}
