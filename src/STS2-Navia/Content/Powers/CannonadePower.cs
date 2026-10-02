using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Mechanics;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 炮火连天的光环(可见增益,数值调整V1):每回合你第一次打出[金花礼炮]时,使接下来所有
/// [金花礼炮]的伤害次数增加能力份数。层数=能力份数，累计次数从零起算，经 <see cref="Salvo.BoostHits"/> 即时抬升
/// 既有礼炮实例,后续生成的礼炮由 CreateInHand 按累计值起算。
/// 每回合计数走 BeforeCardPlayed 计数 + BeforeSideTurnStart 重置(RosulaMethodPower 已验证范式)。
/// </summary>
[RegisterPower]
public sealed class CannonadePower : NaviaPowerBase
{
    private bool _triggeredThisTurn;
    private CardPlay? _pendingFirstCannon;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar("HitsBonus", 0m) };

    public int HitsBonus => (int)DynamicVars["HitsBonus"].BaseValue;

    internal void AccumulateHits(int value)
    {
        AssertMutable();
        DynamicVars["HitsBonus"].BaseValue += value;
        InvokeDisplayAmountChanged();
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // 只数自己打出的礼炮(含被自动打出/从其它牌堆打出的:打出瞬间都位于 Play 牌堆)。
        if (!_triggeredThisTurn && cardPlay.Card is GoldenRoseCannon
            && cardPlay.GetPlayer().Creature == base.Owner
            && cardPlay.Card.Pile?.Type is PileType.Hand or PileType.Play)
        {
            _triggeredThisTurn = true;
            _pendingFirstCannon = cardPlay;
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 用出牌记录身份绑定首炮；嵌套自动出牌不会覆盖它，重复派发也不重复结算。
        if (!ReferenceEquals(_pendingFirstCannon, cardPlay))
        {
            return Task.CompletedTask;
        }
        _pendingFirstCannon = null;
        if (base.Owner.Player is { } player)
        {
            Flash();
            Salvo.BoostHits(player, base.Amount);
        }
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner))
        {
            _triggeredThisTurn = false;
            _pendingFirstCannon = null;
        }
        return Task.CompletedTask;
    }
}
