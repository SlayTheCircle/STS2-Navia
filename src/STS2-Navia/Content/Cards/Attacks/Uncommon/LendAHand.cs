using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 伸出援手(罕见,1 费攻击,数值调整V1,设计者定夺有消耗):造成 4 点伤害。本场战斗中,所有带有[gold]支援[/gold]效果的攻击牌
/// 造成的伤害 +2(光环)。升级:伤害 6,光环 +3。光环逻辑在 <see cref="LendAHandPower"/>
/// (ModifyDamageAdditive 能力链,与附魔 EnchantDamageAdditive 是不同链,合法叠加)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class LendAHand : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword> { CardKeyword.Exhaust };
            NaviaKeywords.AddTo(set, NaviaKeywords.Support);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(4m, ValueProp.Move),
        new DynamicVar("AuraAmount", 2m),
    };

    public LendAHand()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<LendAHandPower>(choiceContext, base.Owner.Creature, base.DynamicVars["AuraAmount"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
        base.DynamicVars["AuraAmount"].UpgradeValueBy(1m);
    }
}
