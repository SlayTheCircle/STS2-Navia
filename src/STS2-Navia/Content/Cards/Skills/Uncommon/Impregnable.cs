using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 固若金汤(罕见,2 费技能):获得 11 点格挡;每有 1 层装填,获得的格挡 +1。
/// 升级:基础 15,每层 +2。卡面走 CalculatedBlock 三件套(写法参考 vanilla Stack),
/// OnPlay 用 Calculate 结果 + Props 给格挡。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class Impregnable : NaviaCardBase
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        // CalculatedVar 契约:显示值 = CalculationBase + CalculationExtra × multiplier(三件缺一不可)
        new CalculationBaseVar(11m),
        new CalculationExtraVar(1m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner?.Creature?.GetPowerAmount<LoadPower>() ?? 0),
    };

    public Impregnable()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.CalculatedBlock.Calculate(cardPlay.Target), base.DynamicVars.CalculatedBlock.Props, cardPlay);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.CalculationBase.UpgradeValueBy(4m);
        base.DynamicVars.CalculationExtra.UpgradeValueBy(1m);
    }
}
