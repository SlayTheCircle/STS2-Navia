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
/// 抛射弹壳(普通,0 费技能):获得 4 点格挡。装填至少 3 层→消耗 1 层抽 1 张;
/// 超过 3 层(即 ≥4)→改为消耗 2 层抽 2 张。升级:格挡 7。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class EjectedShell : NaviaCardBase
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
        new BlockVar(4m, ValueProp.Move),
    };

    public EjectedShell()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = base.Owner.Creature;
        await CreatureCmd.GainBlock(creature, base.DynamicVars.Block, cardPlay);
        // 先读层数再消耗:抽牌数量与消耗层数在读取时一并确定。
        int load = creature.GetPowerAmount<LoadPower>();
        if (load > 3)
        {
            await LoadPower.Gain(choiceContext, creature, -2, creature, this);
            await CardPileCmd.Draw(choiceContext, 2m, base.Owner);
        }
        else if (load >= 3)
        {
            await LoadPower.Gain(choiceContext, creature, -1, creature, this);
            await CardPileCmd.Draw(choiceContext, 1m, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(3m);
    }
}
