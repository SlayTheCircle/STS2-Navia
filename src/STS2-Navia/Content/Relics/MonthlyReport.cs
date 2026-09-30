using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Powers;
using NaviaMod.Content.RelicPools;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 刺玫会月度报表(Uncommon):回合开始时,消耗 1 层[装填],获得 1 点能量。
/// 钩子照抄 vanilla HappyFlower:AfterSideTurnStart + participants 判断(仅自己回合触发)。
/// 装填为 0 时直接返回——不触发也不得能量;消耗走 <see cref="LoadPower.Gain"/> 统一入口
/// (若同时持有「裁断」,该消耗还会联动其等量礼炮轰鸣)。能量用 <see cref="PlayerCmd.GainEnergy"/>。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
public sealed class MonthlyReport : NaviaRelicBase
{
    private const int EnergyGain = 1;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(base.Owner.Creature) || base.Owner.Creature.GetPowerAmount<LoadPower>() <= 0)
        {
            return;
        }
        Flash();
        await LoadPower.Gain(new ThrowingPlayerChoiceContext(), base.Owner.Creature, -1, base.Owner.Creature, null);
        await PlayerCmd.GainEnergy(EnergyGain, base.Owner);
    }
}
