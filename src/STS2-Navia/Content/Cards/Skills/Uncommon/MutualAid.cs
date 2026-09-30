using System;
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
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 邻里互助(罕见,1 费技能,数值调整V1,设计者定夺有消耗):消耗你手牌中的任意张牌(允许选 0 张,此时后续跳过);
/// 选择抽牌堆中等量的牌,使其获得支援:打出时获得 4 点格挡。升级:格挡 6。
/// 手牌多选消耗:CardSelectCmd.FromHand 的 0..N 区间多选(vanilla Guards 先例)逐张 CardCmd.Exhaust;
/// 抽牌堆等量选牌走 FromCombatPile(Cleanse 先例),(提示, n) 构造=必须恰好选 n 张,
/// 可选数不足 n 时管线自动全选,不软锁。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class MutualAid : NaviaCardBase
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
        new DynamicVar("SupportAmount", 4m),
    };

    public MutualAid()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 任意张 = 0..近无穷区间(vanilla Guards/GamblersBrew 同款);手牌为空时 FromHand 返回空集。
        List<CardModel> toExhaust = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_EXHAUST_ANY"), 0, 999999999),
            context: choiceContext,
            player: base.Owner,
            filter: null,
            source: this)).ToList();
        foreach (CardModel card in toExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        if (toExhaust.Count == 0)
        {
            return;
        }

        IEnumerable<CardModel> chosen = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: PileType.Draw.GetPile(base.Owner),
            player: base.Owner,
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_GAIN_SUPPORT_DRAW"), toExhaust.Count),
            filter: c => ModelDb.Enchantment<SupportBlockOnPlay>().CanEnchant(c));
        foreach (CardModel card in chosen)
        {
            CardCmd.Enchant<SupportBlockOnPlay>(card, base.DynamicVars["SupportAmount"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["SupportAmount"].UpgradeValueBy(2m);
    }
}
