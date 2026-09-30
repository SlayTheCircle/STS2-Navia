using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 铸剑为犁(罕见,0 费技能,数值调整V1):消耗手中 1 张[金花礼炮],获得 2 点能量。
/// 升级:获得保留。手中无礼炮则整体无效(轻装上阵同款前置判定)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class SwordsToPlowshares : NaviaCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public SwordsToPlowshares()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CardPile.GetCards(base.Owner, PileType.Hand).OfType<GoldenRoseCannon>().Any())
        {
            return;
        }
        CardModel? cannon = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            context: choiceContext,
            player: base.Owner,
            filter: c => c is GoldenRoseCannon,
            source: this)).FirstOrDefault();
        if (cannon == null)
        {
            return;
        }
        await CardCmd.Exhaust(choiceContext, cannon);
        await PlayerCmd.GainEnergy(2, base.Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
