using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 砥兵备战(罕见,0 费技能):抽 1 张牌。[装填]1。[礼炮轰鸣]1。在手牌中生成 1 张[闪耀摩拉]。消耗。
/// 升级:获得固有(OnUpgrade→AddKeyword,手册 §6 铁律;消耗是基础关键词,进 CanonicalKeywords)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class WarPreparation : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword> { CardKeyword.Exhaust };
            NaviaKeywords.AddTo(set, NaviaKeywords.Load, NaviaKeywords.Salvo);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public WarPreparation()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 1m, base.Owner);
        await LoadPower.Gain(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        await Salvo.Fire(base.CombatState, choiceContext, base.Owner, 1m);
        await ShiningMora.CreateInHand(base.Owner, 1, base.CombatState);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
