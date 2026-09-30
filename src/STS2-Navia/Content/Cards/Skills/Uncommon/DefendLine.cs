using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 捍卫战线(罕见,3 费技能):获得 20 点格挡,获得 1 层虚弱;下个回合开始时获得 1 点能量。
/// 升级:格挡 27。延迟能量直接复用 vanilla EnergyNextTurnPower(下回合能量重置时补发并自移除,
/// 与 Hegemony/ChargeBattery 同款),无需自建 Power。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class DefendLine : NaviaCardBase
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(20m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
        new EnergyVar(1),
    };

    public DefendLine()
        : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<WeakPower>(choiceContext, base.Owner.Creature, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, base.Owner.Creature, base.DynamicVars.Energy.IntValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(7m);
    }
}
