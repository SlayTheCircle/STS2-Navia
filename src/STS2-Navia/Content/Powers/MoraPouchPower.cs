using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 摩拉袋子(隐藏增益,由遗物 MoraPouch 在战斗开始时施加):闪耀摩拉额外造成 2 点伤害。
/// 层数即「额外伤害值」;伤害加成参考本仓 PeddlingFirearmsPower 的 ModifyDamageAdditive 写法
/// (区分攻方归属、只对受力量等加成的攻击生效、只对闪耀摩拉生效)。
/// 不可见(IsVisibleInternal=false)故无需本地化;随战斗结束自动消散,无需手动清理。
/// </summary>
[RegisterPower]
public sealed class MoraPouchPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

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
}
