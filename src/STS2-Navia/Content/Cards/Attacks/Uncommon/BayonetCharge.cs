using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 枪刺拼杀(罕见,1 费攻击):造成 13 点伤害,自身获得 1 层易伤,给予 2 层易伤。
/// 升级:伤害 16,给予易伤 3 层(自身易伤恒 1 不随升级变化)。
/// 同型易伤两份数值必须给 PowerVar 起不同键名(DynamicVarSet 重复键直接抛),
/// 自体那份走自定义名 "SelfVulnerable"(vanilla MadScience 的 "SappingVulnerable" 同款手法)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class BayonetCharge : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(13m, ValueProp.Move),
        new PowerVar<VulnerablePower>(2m),
        new PowerVar<VulnerablePower>("SelfVulnerable", 1m),
    };

    public BayonetCharge()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        // 自体易伤不影响本卡的出伤(易伤只改受到的伤害),按描述顺序在伤害后施加即可。
        await PowerCmd.Apply<VulnerablePower>(choiceContext, base.Owner.Creature, base.DynamicVars["SelfVulnerable"].BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
        base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
