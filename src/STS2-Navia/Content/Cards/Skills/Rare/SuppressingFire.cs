using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Mechanics;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 火力覆盖(稀有,1 费技能):礼炮轰鸣 3。如果手牌中有[金花礼炮],则使手牌中所有[金花礼炮]的伤害次数 +1。
/// 升级:礼炮轰鸣 5。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class SuppressingFire : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Salvo);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Salvo", 3m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public SuppressingFire()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先判定再开火:礼炮轰鸣在手牌无金花礼炮时会生成一张,
        // 「手中有礼炮」的条件以打出前的手牌状态为准,新生成的礼炮不享受次数加成。
        bool hadCannons = CardPile.GetCards(base.Owner, PileType.Hand).OfType<GoldenRoseCannon>().Any();
        await Salvo.Fire(base.CombatState, choiceContext, base.Owner, base.DynamicVars["Salvo"].BaseValue);
        if (hadCannons)
        {
            foreach (GoldenRoseCannon cannon in CardPile.GetCards(base.Owner, PileType.Hand).OfType<GoldenRoseCannon>())
            {
                cannon.DynamicVars["Hits"].BaseValue += 1m;
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Salvo"].UpgradeValueBy(2m);
    }
}
