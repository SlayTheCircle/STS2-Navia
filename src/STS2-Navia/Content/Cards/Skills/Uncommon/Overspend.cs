using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 超额支出(罕见,0 费技能):选择手中一张技能牌,使其获得支援:该卡本场战斗中费用为 0,
/// 并获得消耗。费用清零与加消耗都落在 SupportFreeExhaust.OnEnchant(刷新重跑幂等)。
/// 升级:获得固有(RefinedCharge 加保留同款 OnUpgrade.AddKeyword 写法)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class Overspend : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Support);
            return set;
        }
    }

    public Overspend()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 手牌无可选牌(无未附魔技能牌)时 FromHand 返回空集,判空即可,不软锁。
        CardModel? chosen = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_GAIN_SUPPORT_SKILL"), 1),
            context: choiceContext,
            player: base.Owner,
            filter: c => c.Type == CardType.Skill && c.Enchantment == null,
            source: this)).FirstOrDefault();
        if (chosen != null)
        {
            CardCmd.Enchant<SupportFreeExhaust>(chosen, 0m);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
