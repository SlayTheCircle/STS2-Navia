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
/// 神速装填(先古,1 费技能):快速装填的先古强化版——同机制双槽纯强化(格挡 3→8、装填 1→2,无新增语义),
/// 对齐 vanilla「古老牙齿」转化范式(痛击→破击:纯数值强化)。进池兼修达弗空 Ancient 池。
/// 临时设计(2026-09-30,用户代设计者定稿):数值/名称后续可改,爆炸面=本文件+loc 两条+prep-art 占位。
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
        new BlockVar(8m, ValueProp.Move),
        new PowerVar<LoadPower>(2m),
    };

    public LightningReload()
        : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, play);
        await LoadPower.Gain(choiceContext, base.Owner.Creature, (int)base.DynamicVars["LoadPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(3m);
    }
}
