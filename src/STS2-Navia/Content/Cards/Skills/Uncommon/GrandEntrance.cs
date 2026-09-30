using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 华丽开场(罕见=蓝,0 费技能,数值调整V1):固有。生成 2 张闪耀摩拉,抽 1 张牌。消耗。升级:生成 3 张。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class GrandEntrance : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate, CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Moras", 2m),
        new CardsVar(1),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public GrandEntrance()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ShiningMora.CreateInHand(base.Owner, (int)base.DynamicVars["Moras"].BaseValue, base.CombatState);
        await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Moras"].UpgradeValueBy(1m);
    }
}
