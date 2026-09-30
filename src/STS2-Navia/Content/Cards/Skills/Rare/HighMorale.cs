using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 热情昂扬(稀有,2 费技能):选择抽牌堆中的 1 张牌,使其获得支援:打出有支援效果的牌时,
/// 若该牌在手牌中,自动将其打出(免费,目标随机)。消耗。升级:费用 1。
/// 自动打出的实现在 SupportAutoplay 附魔上(AfterCardPlayed 钩子),本卡只负责施加;
/// 附魔 Amount 无数值含义,占位 1。支援判据为家族判据 is NaviaSupportEnchantment。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class HighMorale : NaviaCardBase
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

    public HighMorale()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? chosen = (await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: PileType.Draw.GetPile(base.Owner),
            player: base.Owner,
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_GAIN_SUPPORT_DRAW"), 1),
            filter: c => ModelDb.Enchantment<SupportAutoplay>().CanEnchant(c))).FirstOrDefault();
        if (chosen != null)
        {
            CardCmd.Enchant<SupportAutoplay>(chosen, 1m);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
