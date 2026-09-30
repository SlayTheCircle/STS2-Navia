using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 炮身加固的护甲(可见增益,数值调整V1,本场战斗):你的[金花礼炮]每对 1 个敌人造成 1 次伤害,
/// 获得等于层数的格挡。走 <see cref="AfterDamageGiven"/> 伤害造成钩子:按 DamageResult 逐次命中
/// 触发(多段炮=多次,全体炮=每敌各计),判据= dealer 是持有者 + cardSource 是金花礼炮 +
/// 实际造成伤害(未被格挡完全吸收,vanilla EnvenomPower 同口径)。无递归风险:本钩子只获得格挡,
/// 不产生新的伤害事件。层数=每次命中的格挡值(基准 3,升级 4)。
/// </summary>
[RegisterPower]
public sealed class ReinforcedBarrelPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == base.Owner
            && cardSource is GoldenRoseCannon
            && props.IsPoweredAttack()
            && result.UnblockedDamage > 0m)
        {
            Flash();
            // 能力来源的格挡走 Unpowered(不吃敏捷类修正),与 UnitedFrontPower 同口径。
            await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
        }
    }
}
