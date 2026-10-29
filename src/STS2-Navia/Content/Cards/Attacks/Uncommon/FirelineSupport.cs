using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>火线接应(罕见,1 费攻击,联机限定):造成 7 点伤害；消耗至多 2 层装填，每层使其他玩家获得 5 点格挡。升级:伤害 11，消耗上限 3。</summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class FirelineSupport : NaviaCardBase
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

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
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar("MaxSpend", 2m),
        new BlockVar("BlockPerLoad", 5m, ValueProp.Move),
    };

    public FirelineSupport()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        Creature creature = base.Owner.Creature;
        int spend = Math.Min(creature.GetPowerAmount<LoadPower>(), (int)base.DynamicVars["MaxSpend"].BaseValue);
        if (spend <= 0)
        {
            return;
        }
        await LoadPower.Gain(choiceContext, creature, -spend, creature, this);
        decimal block = base.DynamicVars["BlockPerLoad"].BaseValue * spend;
        foreach (Creature teammate in creature.CombatState!.GetTeammatesOf(creature).Where(c => c.IsAlive && c.Player != null && c != creature))
        {
            await CreatureCmd.GainBlock(teammate, block, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);
        base.DynamicVars["MaxSpend"].UpgradeValueBy(1m);
    }
}
