using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 高价买入(罕见,1 费技能,数值调整V1):你手牌中每有 1 张[金花礼炮],抽 1 张牌。
/// 升级:费用 0。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class PremiumPurchase : NaviaCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public PremiumPurchase()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int cannons = CardPile.GetCards(base.Owner, PileType.Hand).OfType<GoldenRoseCannon>().Count();
        if (cannons > 0)
        {
            await CardPileCmd.Draw(choiceContext, cannons, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
