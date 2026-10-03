using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 穿心膛线(罕见,1 费能力,数值调整V4):装填 1;装填的层数上限 +3 层(可叠加,第二张 → 12)。
/// 升级:装填 3。上限标记复用既有 LoadCapUpPower,按层数计算(LoadPower.CapFor 自动识别)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class RifledBarrel : NaviaCardBase
{
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
        new PowerVar<LoadPower>(1m),
    };

    public RifledBarrel()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = base.Owner.Creature;
        // 先挂上限标记再装填,保证 LoadPower.Gain 按新上限 9 截断。
        await PowerCmd.Apply<LoadCapUpPower>(choiceContext, creature, 1, creature, this);
        await LoadPower.Gain(choiceContext, creature, (int)base.DynamicVars["LoadPower"].BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["LoadPower"].UpgradeValueBy(2m);
    }
}
