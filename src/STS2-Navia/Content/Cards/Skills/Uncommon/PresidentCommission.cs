using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Cards;

/// <summary>会长委托(罕见,1 费技能,联机限定):选择另一名玩家，由其选择自己的一张手牌获得全体格挡与抽牌的支援。消耗。升级:费用 0，支援数值提升。</summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class PresidentCommission : NaviaCardBase
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool GainsBlock => true;

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
        new DynamicVar("SupportTier", 1m),
    };

    public PresidentCommission()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
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

        bool CanEnchant(CardModel card) => ModelDb.Enchantment<SupportBlockDrawOnPlay>().CanEnchant(card);
        if (!CardPile.GetCards(ally, PileType.Hand).Any(CanEnchant))
        {
            return;
        }
        CardModel? chosen = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_ALLY_TO_GAIN_SUPPORT"), 1),
            context: choiceContext,
            player: ally,
            filter: CanEnchant,
            source: this)).FirstOrDefault();
        if (chosen != null)
        {
            CardCmd.Enchant<SupportBlockDrawOnPlay>(chosen, base.DynamicVars["SupportTier"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
        base.DynamicVars["SupportTier"].UpgradeValueBy(1m);
    }
}
