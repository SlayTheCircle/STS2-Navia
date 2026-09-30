using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 补充物资(普通,1 费技能,数值调整V1):保留。消耗你所有[装填],每消耗 2 层,获得 1 点能量。
/// 升级:费用 1→0。先读层数再消耗(手册 §4.2「先算后耗」),能量回 floor(层数/2)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class Resupply : NaviaCardBase
{
    /// <summary>每消耗多少层装填回 1 点能量。</summary>
    private const int LoadsPerEnergy = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword> { CardKeyword.Retain };
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    public Resupply()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = base.Owner.Creature;
        // 先读后耗:消耗量按打出瞬间的装填全量结算,面板/实际一致。
        int load = creature.GetPowerAmount<LoadPower>();
        if (load <= 0)
        {
            return;
        }
        await LoadPower.Gain(choiceContext, creature, -load, creature, this);
        int energy = load / LoadsPerEnergy;
        if (energy > 0)
        {
            await PlayerCmd.GainEnergy(energy, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
