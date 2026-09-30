using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Mechanics;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 猛铳打击(普通,1 费攻击):造成 7 点伤害,礼炮轰鸣 1。升级:伤害 10。
/// 现行卡表见 docs/design/cards.md（生成）；原案与升级参考存 docs/history/design/。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class MightyShot : NaviaCardBase
{
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Salvo);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar("Salvo", 1m),
    };

    public MightyShot()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await Salvo.Fire(base.CombatState, choiceContext, base.Owner, base.DynamicVars["Salvo"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
