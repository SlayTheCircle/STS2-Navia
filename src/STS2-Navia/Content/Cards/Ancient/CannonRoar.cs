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
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 枪炮轰鸣(先古,1 费攻击,数值调整V4):造成 12 点伤害。消耗全部[装填],每消耗 1 层,伤害 +7,给予 1 层易伤。
/// 升级:伤害 16。先古稀有度进池即修达弗/DustyTome 的空 Ancient 池崩溃(奥罗巴斯给予的先古卡)。
/// 三件套同构自铳弹齐射(VolleyFire)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class CannonRoar : NaviaCardBase
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
        new DamageVar(12m, ValueProp.Move),
        new CalculationBaseVar(12m),
        new ExtraDamageVar(7m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner?.Creature?.GetPowerAmount<LoadPower>() ?? 0),
        new PowerVar<VulnerablePower>(1m),
    };

    public CannonRoar()
        : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        Creature creature = base.Owner.Creature;
        // 先算总伤再消耗装填(Calculate 实时重读层数,先消耗就会算成基准值)。
        decimal total = base.DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        int load = creature.GetPowerAmount<LoadPower>();
        if (load > 0)
        {
            await LoadPower.Gain(choiceContext, creature, -load, creature, this);
        }
        await DamageCmd.Attack(total).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);
        base.DynamicVars["CalculationBase"].UpgradeValueBy(4m);
    }
}
