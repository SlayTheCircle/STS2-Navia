using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.ValueProps;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 装填(数值调整V4):每有 2 层,获得格挡时便 +1、攻击伤害 +1(等同敏捷与力量的加成,
/// 但不被计为敏捷/力量——不会被偷取/移除);部分卡牌可消耗装填获得额外效果。
/// 默认上限 6 层;「穿心膛线」每层标记额外 +3(可叠加)。
/// </summary>
[RegisterPower]
public sealed class LoadPower : NaviaPowerBase
{
    public const int DefaultCap = 6;

    public const decimal StacksPerDexterity = 2m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>当前装填上限:6 + 3 × <see cref="LoadCapUpPower"/>层数(数值调整V4:可叠加的 +3)。</summary>
    public static int CapFor(Creature creature)
    {
        return DefaultCap + LoadCapUpPower.CapPerStack * creature.GetPowerAmount<LoadCapUpPower>();
    }

    /// <summary>
    /// 获得(或消耗,amount 为负)装填的唯一入口:正向增益会按当前上限截断。
    /// 所有给予装填的卡牌/遗物/药水都应经由这里调用,以统一执行上限规则。
    /// </summary>
    public static async Task Gain(PlayerChoiceContext choiceContext, Creature target, int amount, Creature? applier, CardModel? cardSource)
    {
        if (amount > 0)
        {
            int current = target.GetPowerAmount<LoadPower>();
            amount = Math.Min(amount, CapFor(target) - current);
            if (amount <= 0)
            {
                return;
            }
        }
        await PowerCmd.Apply<LoadPower>(choiceContext, target, amount, applier, cardSource);
    }

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null)
        {
            if (cardSource.Owner.Creature != base.Owner)
            {
                return 0m;
            }
        }
        else if (base.Owner != target)
        {
            return 0m;
        }
        if (!props.IsPoweredCardOrMonsterMoveBlock())
        {
            return 0m;
        }
        return Math.Floor(base.Amount / StacksPerDexterity);
    }

    /// <summary>
    /// 力量等价(数值调整V4):vanilla <see cref="MegaCrit.Sts2.Core.Models.Powers.StrengthPower"/> 同款口径——
    /// 只加自己造成的卡牌攻击伤害(<c>IsPoweredAttack</c>),格挡/Power 伤害/队友出牌不吃加成。
    /// </summary>
    protected override decimal ModifyDamageAdditiveCore(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? card)
    {
        if (base.Owner != dealer || !props.IsPoweredAttack())
        {
            return 0m;
        }
        return Math.Floor(base.Amount / StacksPerDexterity);
    }
}
