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
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 追加订单(稀有,1 费技能):将手牌中一张牌的带有消耗的复制品加入手牌。消耗。升级:费用 1→0。
/// 选牌复用 vanilla CardSelectCmd.FromHand,但提示用自定义 loc 键(STS2_NAVIA_TO_COPY,
/// card_selection 表):行为是「复制」而非消耗,不能沿用原版 TO_EXHAUST 文案。
/// 过滤器限定攻击/技能/能力——排除状态与诅咒(给诅咒做消耗版复制品与设计意图不符)。
/// 复制走 CreateCloneForPlayer + AddKeyword(Exhaust)(vanilla DualWield 范式),
/// AddGeneratedCardToCombat 入手牌。手牌无可选牌时 FromHand 返回空集,判空即无操作。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class AdditionalOrder : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public AdditionalOrder()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? chosen = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_COPY"), 1), context: choiceContext, player: base.Owner, filter: (CardModel c) => c.Type is CardType.Attack or CardType.Skill or CardType.Power, source: this)).FirstOrDefault();
        if (chosen != null)
        {
#if NAVIA_GAME_0107_1
            // 0.107.1 无 CreateCloneForPlayer(0.111 多人移交 API);CreateClone 保留原 owner,本处同人复制语义等价。
            CardModel copy = chosen.CreateClone();
#else
            CardModel copy = chosen.CreateCloneForPlayer(base.Owner);
#endif
            copy.AddKeyword(CardKeyword.Exhaust);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
