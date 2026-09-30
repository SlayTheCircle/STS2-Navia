using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 募集资金(罕见,1 费技能):选择抽牌堆中的 1 张牌,使其获得支援:打出时,在手牌中生成
/// 2 张闪耀摩拉。消耗。升级:生成 3 张。
/// 抽牌堆选牌走 vanilla CardSelectCmd.FromCombatPile(vanilla Cleanse 先例);
/// 过滤器用 ModelDb.Enchantment&lt;T&gt;().CanEnchant 提前排除状态/诅咒/任务牌与已附魔牌
/// (vanilla 单附魔槽,不可附魔的牌 CardCmd.Enchant 会抛,选了白选)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class Fundraising : NaviaCardBase
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
        new DynamicVar("SupportAmount", 2m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public Fundraising()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽牌堆无可选牌时 FromCombatPile 直接返回空集(管线内置),判空即可,不软锁。
        CardModel? chosen = (await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: PileType.Draw.GetPile(base.Owner),
            player: base.Owner,
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_GAIN_SUPPORT_DRAW"), 1),
            filter: c => ModelDb.Enchantment<SupportMoraOnPlay>().CanEnchant(c))).FirstOrDefault();
        if (chosen != null)
        {
            CardCmd.Enchant<SupportMoraOnPlay>(chosen, base.DynamicVars["SupportAmount"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["SupportAmount"].UpgradeValueBy(1m);
    }
}
