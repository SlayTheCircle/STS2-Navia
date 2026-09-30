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
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 鸣枪示意(普通,0 费技能,数值调整V1):消耗至多 3 层[装填],每消耗 1 层,抽 1 张牌,[礼炮轰鸣]1。
/// 升级:至多 4 层。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class WarningShot : NaviaCardBase
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
        new DynamicVar("MaxSpend", 3m),
    };

    public WarningShot()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = base.Owner.Creature;
        // 先读层数再消耗:实际消耗 = min(当前装填, 上限);每层抽 1 + 礼炮轰鸣 1。
        int load = creature.GetPowerAmount<LoadPower>();
        int spend = Math.Min(load, (int)base.DynamicVars["MaxSpend"].BaseValue);
        if (spend > 0)
        {
            await LoadPower.Gain(choiceContext, creature, -spend, creature, this);
            await CardPileCmd.Draw(choiceContext, spend, base.Owner);
            for (int i = 0; i < spend; i++)
            {
                await Salvo.Fire(base.CombatState, choiceContext, base.Owner, 1m);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["MaxSpend"].UpgradeValueBy(1m);
    }
}
