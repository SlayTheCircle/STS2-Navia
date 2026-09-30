using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.RelicPools;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 明黄缎带(Common):战斗开始时,[礼炮轰鸣] 2。
/// 钩子组合照抄 vanilla Akabeko:AfterSideTurnStart + TurnNumber&lt;=1 即「战斗首个回合开始时」,
/// PlayerCombatState 每场战斗重建,回合号自动归零,无需 AfterCombatEnd 重置标记。
/// Salvo 需要 player 与 combatState,钩子参数直接供 combatState,player 取 base.Owner。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
public sealed class GoldenRibbon : NaviaRelicBase
{
    private const decimal SalvoAmount = 2m;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner.Creature) && base.Owner.PlayerCombatState.TurnNumber <= 1)
        {
            Flash();
            await Salvo.Fire(combatState, new ThrowingPlayerChoiceContext(), base.Owner, SalvoAmount);
        }
    }
}
