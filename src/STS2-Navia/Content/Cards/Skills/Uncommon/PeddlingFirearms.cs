using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 兜售枪火(罕见,1 费技能):装填 1。本回合内,[闪耀摩拉]额外造成 3 点伤害。
/// 升级:额外伤害 5。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class PeddlingFirearms : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            // 描述提到「装填」,挂关键词横幅+解释框。
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<LoadPower>(1m),
        new DynamicVar("MoraBonus", 3m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public PeddlingFirearms()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = base.Owner.Creature;
        await LoadPower.Gain(choiceContext, creature, (int)base.DynamicVars["LoadPower"].BaseValue, creature, this);
        await PowerCmd.Apply<PeddlingFirearmsPower>(choiceContext, creature, base.DynamicVars["MoraBonus"].BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["MoraBonus"].UpgradeValueBy(2m);
    }
}
