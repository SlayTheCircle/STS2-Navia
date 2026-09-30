using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 庆贺礼炮(稀有,2 费技能):选择手中一张[金花礼炮],将其对随机敌人打出 3 次。消耗。
/// 升级:获得保留。「打出」走 <see cref="CardCmd.AutoPlay"/> 完整打出管线(免费,礼炮自身的
/// 伤害/命中次数/摩拉计数等全部照常结算);目标对 AnyEnemy 卡传 null,由 AutoPlay 内部用
/// CombatTargets 确定性 RNG 逐次指定随机敌人(SupportAutoplay 同款管线)。
/// 同一实例重复打出可行:金花礼炮自带消耗,第一炮后位于消耗堆,而 vanilla Bombardment
/// (每回合从消耗堆把自己打出)已验证 AutoPlay 对消耗堆中的卡合法——因此不需要副本方案。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class FestiveCannon : NaviaCardBase
{
    private const int Plays = 3;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public FestiveCannon()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 手中无礼炮时不弹选牌框,直接落空(参考 TravelLight 的判空守卫)。
        if (!CardPile.GetCards(base.Owner, PileType.Hand).OfType<GoldenRoseCannon>().Any())
        {
            return;
        }
        CardModel? selection = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_PLAY_CANNON"), 1), context: choiceContext, player: base.Owner, filter: (CardModel c) => c is GoldenRoseCannon, source: this)).FirstOrDefault();
        if (selection == null)
        {
            return;
        }
        for (int i = 0; i < Plays; i++)
        {
            // 免费真·打出;每炮独立随机指定目标,战斗结束/自身死亡时 AutoPlay 内部自行短路。
            await CardCmd.AutoPlay(choiceContext, selection, null);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级获得保留必须走 OnUpgrade→AddKeyword(手册 §6)。
        AddKeyword(CardKeyword.Retain);
    }
}
