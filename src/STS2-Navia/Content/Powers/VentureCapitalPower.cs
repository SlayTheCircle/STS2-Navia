using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 风险投资(施加给敌人的临时减益):本回合内你(施加者)每次命中这名敌人,
/// 在你的抽牌堆生成 1 张[闪耀摩拉]。施加者回合结束时自移除;敌人死亡时随其消亡。
/// 层数即「每次命中生成的摩拉数」。
/// </summary>
[RegisterPower]
public sealed class VentureCapitalPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || dealer == null || dealer != base.Applier || !props.IsPoweredAttack())
        {
            return;
        }
        await CreateMorasInDrawPile(dealer.Player);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (base.Applier != null && participants.Contains(base.Applier))
        {
            await PowerCmd.Remove(this);
        }
    }

    private async Task CreateMorasInDrawPile(Player? player)
    {
        ICombatState? combatState = base.CombatState;
        if (player == null || combatState == null || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }
        int count = (int)base.Amount;
        List<ShiningMora> moras = new List<ShiningMora>(count);
        for (int i = 0; i < count; i++)
        {
            moras.Add(combatState.CreateCard<ShiningMora>(player));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(moras, PileType.Draw, player);
    }
}
