using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 例行检查(罕见,2 费攻击):造成 12 点伤害。手牌中每有一张无色牌,伤害 +4。
/// 升级:伤害 15,每张 +5。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class RoutineInspection : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
        // 预览感知三件套:面板过 Hook.ModifyDamage(力量/虚弱/附魔),与实际出伤一致。
        new CalculationBaseVar(12m),
        new ExtraDamageVar(4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner != null
            ? CardPile.GetCards(card.Owner, PileType.Hand).Count((CardModel c) => c.VisualCardPool.IsColorless)
            : 0),
    };

    public RoutineInspection()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        decimal total = base.DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        await DamageCmd.Attack(total).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
        base.DynamicVars["CalculationBase"].UpgradeValueBy(3m);
        base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}
