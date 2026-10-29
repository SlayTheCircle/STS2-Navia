using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>公账支取(罕见,0 费技能,联机限定):消耗自己手中至多 2 张闪耀摩拉，每张使目标队友获得 1 点能量。升级:保留。</summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class PublicAccountWithdrawal : NaviaCardBase
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("MaxExhaust", 2m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public PublicAccountWithdrawal()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        Player? ally = cardPlay.Target.Player;
        if (ally == null || ally == base.Owner || !ally.Creature.IsAlive)
        {
            return;
        }

        List<CardModel> selected = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_EXHAUST_MORA_ALLY"), 0, (int)base.DynamicVars["MaxExhaust"].BaseValue),
            context: choiceContext,
            player: base.Owner,
            filter: c => c is ShiningMora,
            source: this)).ToList();
        foreach (CardModel card in selected)
        {
            await CardCmd.Exhaust(choiceContext, card);
            await PlayerCmd.GainEnergy(1, ally);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
