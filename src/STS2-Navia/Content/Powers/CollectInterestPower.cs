using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 回收利息(可见增益):你每获得 3 层[装填],便获得 1 点能量。
/// 监听 <see cref="LoadPower"/> 的层数变化:<c>AfterPowerAmountChanged</c> 的 amount 即本次增量,
/// 仅累计正向获得(delta&gt;0),消耗/负向变化不回退进度——与 <see cref="HighPressureChamberPower"/>
/// 只认负增量的口径互为镜像。跨回合进度用私有字段承载(先例:RosulaMethodPower 的回合计数;
/// 战斗内状态不落盘)。层数无含义(Single 标记,固定 1),能量走 <c>PlayerCmd.GainEnergy</c>。
/// </summary>
[RegisterPower]
public sealed class CollectInterestPower : NaviaPowerBase
{
    /// <summary>每获得多少层装填回 1 点能量。</summary>
    public const int LoadsPerEnergy = 3;

    /// <summary>距下一次回能还差几层装填(0..2)。</summary>
    private int _loadsGained;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not LoadPower || amount <= 0m || power.Owner != base.Owner)
        {
            return;
        }
        _loadsGained += (int)amount;
        while (_loadsGained >= LoadsPerEnergy)
        {
            _loadsGained -= LoadsPerEnergy;
            Flash();
            await PlayerCmd.GainEnergy(1m, base.Owner.Player!);
        }
    }
}
