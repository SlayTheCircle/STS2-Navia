using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.ValueProps;
using NaviaMod.Content.Powers;
using NaviaMod.Content.RelicPools;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 精致的阳伞(Rare):每当你在装填层数达到上限时获得装填,都会获得与当前装填数相等的格挡。
/// 实现说明:LoadPower.Gain 对「已达上限仍试图获得」的情形会在截断到 0 后直接 return
/// (不经过 PowerCmd.Apply,所有层数变化钩子均感知不到),在不改共享文件的前提下无法捕捉;
/// 故本类监听 AfterPowerAmountChanged,在「一次正向装填把装填恰好填到上限」时触发
/// (即 LoadPower.Gain 发生正向截断、实际落地的情形),获得与当前装填数(=上限 6/9)相等的格挡,
/// 走 CreatureCmd.GainBlock + ValueProp.Unpowered(vanilla CloakClasp 的非卡牌格挡写法)。
/// 上述限制已在交付 manifest 的 notes 中写明,由集成者协调是否在 LoadPower.Gain 埋信号。
/// 触发不会无限递归:填满到上限后,后续格挡→装填的联动再入 Gain 会在上限处被截断为空操作。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
public sealed class ElegantParasol : NaviaRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not LoadPower || amount <= 0m || power.Owner != base.Owner.Creature || !CombatManager.Instance.IsInProgress)
        {
            return;
        }
        Creature creature = base.Owner.Creature;
        int current = creature.GetPowerAmount<LoadPower>();
        if (current >= LoadPower.CapFor(creature))
        {
            Flash();
            await CreatureCmd.GainBlock(creature, current, ValueProp.Unpowered, null);
        }
    }
}
