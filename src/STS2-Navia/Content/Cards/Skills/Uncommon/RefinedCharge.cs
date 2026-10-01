using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 精制装药(稀有,0 费技能,数值调整V3):礼炮轰鸣 2。如果有[装填],则消耗 1 层,将此卡返回你的手牌。
/// 升级:获得保留。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class RefinedCharge : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Load, NaviaKeywords.Salvo);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Salvo", 2m),
    };

    public RefinedCharge()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Salvo.Fire(base.CombatState, choiceContext, base.Owner, base.DynamicVars["Salvo"].BaseValue);
        Creature creature = base.Owner.Creature;
        if (creature.GetPowerAmount<LoadPower>() > 0)
        {
            await LoadPower.Gain(choiceContext, creature, -1, creature, this);
        }
    }

    /// <summary>
    /// 卡牌打出后的去向在 OnPlay 之前结算:此处的装填层数与 OnPlay 里判定/消耗用的是同一份(消耗前)状态,
    /// 两边条件天然一致。仅当本会进弃牌堆且有装填时改道手牌,不影响虚无/消耗等其它去向。
    /// 0.111 走 CardLocation(牌堆+位置);0.107.1 钩子只返回 PileType,改道手牌语义保留、置底位置精度丢失。
    /// </summary>
#if !NAVIA_GAME_0107_1
    protected override CardLocation GetResultLocationForCardPlay()
    {
        CardLocation location = base.GetResultLocationForCardPlay();
        if (location.pileType == PileType.Discard && base.Owner.Creature.GetPowerAmount<LoadPower>() > 0)
        {
            location.pileType = PileType.Hand;
            location.position = CardPilePosition.Bottom;
        }
        return location;
    }
#else
    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType pile = base.GetResultPileTypeForCardPlay();
        return pile == PileType.Discard && base.Owner.Creature.GetPowerAmount<LoadPower>() > 0 ? PileType.Hand : pile;
    }
#endif

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
