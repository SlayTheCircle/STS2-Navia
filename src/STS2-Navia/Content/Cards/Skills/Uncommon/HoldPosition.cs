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

namespace NaviaMod.Content.Cards;

/// <summary>
/// 据守阵地(罕见,1 费技能):获得 9 点格挡;使手牌中一张牌获得消耗。
/// 升级:格挡 12。选牌复用 vanilla CardSelectCmd.FromHand,但提示用自定义 loc 键
/// (STS2_NAVIA_TO_GAIN_EXHAUST,card_selection 表):行为是「获得消耗」而非「被消耗」,
/// 不能沿用原版 TO_EXHAUST 文案。加消耗走原生 CardModel.AddKeyword(写 LocalKeywords,对局内即时生效)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class HoldPosition : NaviaCardBase
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(9m, ValueProp.Move),
    };

    public HoldPosition()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        // 手牌无可选牌时 FromHand 直接返回空集,必须判空(参考 vanilla BurningPact)。
        CardModel? chosen = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_GAIN_EXHAUST"), 1), context: choiceContext, player: base.Owner, filter: null, source: this)).FirstOrDefault();
        if (chosen != null)
        {
            chosen.AddKeyword(CardKeyword.Exhaust);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(3m);
    }
}
