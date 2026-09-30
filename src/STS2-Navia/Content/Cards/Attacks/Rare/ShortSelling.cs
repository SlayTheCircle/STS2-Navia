using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 做空市场(稀有,0 费攻击):造成 3 点伤害。本场战斗中,你每打出 1 张[闪耀摩拉],
/// 这张卡的伤害 +3。升级:伤害 4,每张摩拉加成 4。
/// 卡牌实例在手牌/牌堆中都会收到战斗钩子,故直接覆写钩子自增基础伤害(无需三件套:增量是永久性的)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class ShortSelling : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("MoraBonus", 3m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public ShortSelling()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // MoraCounterPower 每打出一张摩拉 +1(满 3 清零时为 -3,被 amount>0 过滤)。
        if (power is MoraCounterPower && amount > 0m && power.Owner == base.Owner.Creature)
        {
            base.DynamicVars.Damage.BaseValue += base.DynamicVars["MoraBonus"].BaseValue;
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(1m);
        base.DynamicVars["MoraBonus"].UpgradeValueBy(1m);
    }
}
