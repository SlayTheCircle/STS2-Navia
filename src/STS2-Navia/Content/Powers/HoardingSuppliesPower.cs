using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Mechanics;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 囤积物资(可见增益,数值调整V1,设计者定夺回合开始):回合开始时,手牌中每有 1 张闪耀摩拉,
/// [礼炮轰鸣]等同于本 Power 层数的值。层数承载「每张轰鸣值」(基准 1,升级 2)。
/// 时点=BeforeSideTurnStart(掩护轰炸/军火大亨同期);手牌快照在抽牌前,符合「回合开始时」语义。
/// </summary>
[RegisterPower]
public sealed class HoardingSuppliesPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Side || !participants.Contains(base.Owner) || !base.Owner.IsAlive)
        {
            return;
        }
        int moras = CardPile.GetCards(base.Owner.Player, PileType.Hand).OfType<ShiningMora>().Count();
        if (moras > 0 && base.Owner.Player is { } player)
        {
            Flash();
            for (int i = 0; i < moras; i++)
            {
                await Salvo.Fire(combatState, choiceContext, player, base.Amount);
            }
        }
    }
}
