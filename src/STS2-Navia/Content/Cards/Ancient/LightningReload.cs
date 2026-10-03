using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 极速装填(先古,1 费技能,数值调整V4;原名「神速装填」,2026-09-30 定稿、V4 改名重做):
/// 获得 10 点格挡,将[装填]补至上限(读 <see cref="LoadPower.CapFor"/>,穿心膛线叠加时自动到 12/15)。
/// 升级:格挡 12。仍是快速装填的先古强化版(古老牙齿转化);进池兼修达弗空 Ancient 池。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class LightningReload : NaviaCardBase
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
        new BlockVar(10m, ValueProp.Move),
    };

    public LightningReload()
        : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        Creature creature = base.Owner.Creature;
        await CreatureCmd.GainBlock(creature, base.DynamicVars.Block, play);
        // 补至上限:缺口 = 当前上限 - 现有层数,Gain 内部再按上限截断一次,天然幂等。
        int deficit = LoadPower.CapFor(creature) - creature.GetPowerAmount<LoadPower>();
        if (deficit > 0)
        {
            await LoadPower.Gain(choiceContext, creature, deficit, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(2m);
    }
}
