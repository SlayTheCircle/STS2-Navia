using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 牟取利益(永久增益):[闪耀摩拉]额外造成 3(升级 5)点伤害。
/// 层数即「额外伤害值」;多次打出会叠加(与 Accuracy 同规则)。
/// </summary>
[RegisterPower]
public sealed class ProfiteeringPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override decimal ModifyDamageAdditiveCore(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? card)
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
}
