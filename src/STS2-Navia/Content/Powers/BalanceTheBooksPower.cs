using System.Collections.Generic;
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
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 亏空平账(可见增益):每当你获得负面状态时,在你的抽牌堆中生成 2 张[闪耀摩拉]。
/// 「获得负面状态」= <c>AfterPowerAmountChanged</c> 里 power.Type == <see cref="PowerType.Debuff"/>
/// 且增量&gt;0(PowerCmd.Apply/ModifyAmount 两条路都会广播该钩,来源不限:敌人施加/自发均算;
/// 多人协作时只认自己身上的)。生成进抽牌堆:combatState.CreateCard + AddGeneratedCardsToCombat
/// (PileType.Draw,写法同 <see cref="FinancialMarketPower"/>)。层数无含义(Single 标记,固定 1)。
/// </summary>
[RegisterPower]
public sealed class BalanceTheBooksPower : NaviaPowerBase
{
    /// <summary>每次获得负面状态生成的摩拉数。</summary>
    public const int MorasPerDebuff = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Type != PowerType.Debuff || amount <= 0m || power.Owner != base.Owner)
        {
            return;
        }
        Flash();
        await CreateMorasInDrawPile(base.Owner.Player);
    }

    private async Task CreateMorasInDrawPile(Player? player)
    {
        ICombatState? combatState = base.CombatState;
        if (player == null || combatState == null || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }
        List<ShiningMora> moras = new List<ShiningMora>(MorasPerDebuff);
        for (int i = 0; i < MorasPerDebuff; i++)
        {
            moras.Add(combatState.CreateCard<ShiningMora>(player));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(moras, PileType.Draw, player);
    }
}
