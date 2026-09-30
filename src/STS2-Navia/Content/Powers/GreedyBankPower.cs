using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 贪婪枪火的金币池(隐藏增益,数值调整V1):每次触发(本回合内打出闪耀摩拉并消耗装填)累加金币,
/// 战斗胜利时经 <see cref="AfterCombatVictory"/> 一次性发放(vanilla 战斗胜利钩子,发放先于奖励界面;
/// 败北不发放)。回合结束不清空——「本回合内」限定的是触发窗口,累计持续到战斗结束。
/// </summary>
[RegisterPower]
public sealed class GreedyBankPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (base.Amount > 0 && base.Owner.Player is { } player)
        {
            await PlayerCmd.GainGold(base.Amount, player);
        }
    }
}
