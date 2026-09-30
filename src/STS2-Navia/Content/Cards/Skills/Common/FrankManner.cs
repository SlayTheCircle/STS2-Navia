using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 率直作风(普通,1 费技能):抽 2 张牌。选择抽牌堆中的一张牌,使其获得支援:该卡获得消耗。
/// 升级:抽 3 张牌。抽牌堆选牌走 CardSelectCmd.FromCombatPile(vanilla 秘术/秘技同款,
/// PileType.Draw.GetPile 取战斗抽牌堆);过滤器排除已附魔牌与状态/诅咒/任务
/// (vanilla CanEnchant 拒绝这三类,先排除避免「选了白选」)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class FrankManner : NaviaCardBase
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

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2),
    };

    public FrankManner()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先抽再选:抽上来的牌进手牌、离开抽牌堆,与文案顺序一致。
        await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);

        // 抽牌堆无可选牌时 FromCombatPile 返回空集,判空即可,不软锁。
        CardModel? chosen = (await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: PileType.Draw.GetPile(base.Owner),
            player: base.Owner,
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_GAIN_SUPPORT_DRAW_PILE"), 1),
            filter: c => c.Enchantment == null
                && (c.Type == CardType.Attack || c.Type == CardType.Skill || c.Type == CardType.Power))).FirstOrDefault();
        if (chosen != null)
        {
            CardCmd.Enchant<SupportGainExhaust>(chosen, 0m);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
