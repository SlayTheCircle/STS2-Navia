using System;
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
/// 高压弹膛(可见增益):每当你消耗 1 层[装填],对所有敌人造成 {Amount} 点伤害(层数 = 每层消耗的伤害值)。
/// 联动 <see cref="LoadPower"/> 的层数变化:AfterPowerAmountChanged 的 amount 即本次增减量,负数为消耗。
/// </summary>
[RegisterPower]
public sealed class HighPressureChamberPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount < 0m && power is LoadPower && power.Owner == base.Owner)
        {
            // 按消耗层数逐次结算(每次独立对全体敌人造成 Amount 点伤害)。
            int times = Math.Abs((int)amount);
            for (int i = 0; i < times; i++)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, base.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner);
            }
        }
    }
}
