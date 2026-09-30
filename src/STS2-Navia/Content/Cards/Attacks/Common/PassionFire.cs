using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 热情枪火(普通,1 费攻击,数值调整V1):造成 8 点伤害。如果有[装填],则消耗 1 层,获得 1 点力量,
/// 再造成 3 点伤害(两段均为攻击伤害,天然吃力量加成)。升级:伤害 10,力量 2。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class PassionFire : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar("BonusDamage", 3m),
        new DynamicVar("StrengthGain", 1m),
    };

    public PassionFire()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        Creature creature = base.Owner.Creature;
        if (creature.GetPowerAmount<LoadPower>() > 0)
        {
            await LoadPower.Gain(choiceContext, creature, -1, creature, this);
            // 数值调整V1:消耗装填改为得力量(先得力量,再打出第二段——第二段同吃力量)。
            await PowerCmd.Apply<StrengthPower>(choiceContext, creature, base.DynamicVars["StrengthGain"].BaseValue, creature, this);
            await DamageCmd.Attack(base.DynamicVars["BonusDamage"].BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
        base.DynamicVars["StrengthGain"].UpgradeValueBy(1m);
    }
}
