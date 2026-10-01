using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.Powers;
using NaviaMod.Content.RelicPools;
using MegaCrit.Sts2.Core.HoverTips;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 裁断(Rare):消耗[装填]时,获得等量的[礼炮轰鸣]。
/// 监听 <see cref="LoadPower"/> 的 AfterPowerAmountChanged 且 amount&lt;0(decimal 取绝对值分层)。
/// 「等量」= 对消耗总量调一次 <see cref="Salvo.Fire"/>——绝不能逐层循环开火:
/// Fire 的语义是「无礼炮则生成一张,有则全体+value」,逐层开火会生成多张礼炮而非抬高伤害。
/// 消耗来源不限(卡牌消耗、月度报表的回合消耗都会触发);战斗收尾的 Power 清理
/// 被 Hook 派发器的 IsOverOrEnding 短路挡住,不会误发礼炮。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
public sealed class Verdict : NaviaRelicBase
{

    /// <summary>文本提及机制名词,挂关键词词条悬停解释。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Load, NaviaKeywords.Salvo);
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount < 0m && power is LoadPower && power.Owner == base.Owner.Creature && CombatManager.Instance.IsInProgress)
        {
            int spent = Math.Abs((int)amount);
            if (spent > 0)
            {
                Flash();
                await Salvo.Fire(base.Owner.Creature.CombatState, choiceContext, base.Owner, spent);
            }
        }
    }
}
