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
/// 利斧强袭(稀有,1 费攻击):造成 18 点伤害并给予 1 层易伤;当前每有 1 层装填,这张卡的伤害 -2(不消耗装填)。
/// 升级:伤害 23(CalculationBase 同步 +5),易伤 2;每层 -2 不变。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class AxeOnslaught : NaviaCardBase
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
        new DamageVar(18m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
        // 预览感知三件套(负缩放用 -2):面板过 Hook.ModifyDamage,与实际出伤一致。
        new CalculationBaseVar(18m),
        new ExtraDamageVar(-2m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner?.Creature?.GetPowerAmount<LoadPower>() ?? 0),
    };

    public AxeOnslaught()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        // 本卡不消耗装填,但保持「先算后打」:实际伤害必须在攻击执行前用 Calculate 求值。
        decimal total = base.DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        await DamageCmd.Attack(total).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(5m);
        base.DynamicVars.CalculationBase.UpgradeValueBy(5m);
        base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
