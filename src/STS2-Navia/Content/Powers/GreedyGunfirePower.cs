using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 贪婪枪火的回合内增益(可见,数值调整V1):本回合内,你每次打出[闪耀摩拉],都会消耗 1 层[装填],
/// 在战斗结束时额外获得 {Amount} 金币(层数=每张摩拉的金币值,基准 3,升级 5)。
/// 金币累计进隐藏的 <see cref="GreedyBankPower"/>(跨回合持续),由其在战斗胜利时统一发放;
/// 消耗判定取打出前快照(BeforeCardPlayed),与摩拉自身「每 3 张装填 1」的结算顺序无冲突。
/// 自己回合结束时自移除(时点写法同 FinancialMarketPower)。
/// </summary>
[RegisterPower]
public sealed class GreedyGunfirePower : NaviaPowerBase
{

    /// <summary>文本提及机制名词,挂关键词词条悬停解释。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Load);
    /// <summary>打出摩拉那一刻是否持有装填(决定这张摩拉付不付 1 层、计不计金币)。</summary>
    private bool _hadLoadWhenMoraPlayed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.GetPlayer().Creature == base.Owner && cardPlay.Card is ShiningMora)
        {
            _hadLoadWhenMoraPlayed = base.Owner.GetPowerAmount<LoadPower>() > 0;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.GetPlayer().Creature == base.Owner && cardPlay.Card is ShiningMora)
        {
            if (_hadLoadWhenMoraPlayed)
            {
                Flash();
                await LoadPower.Gain(choiceContext, base.Owner, -1, base.Owner, cardPlay.Card);
                GreedyBankPower? bank = base.Owner.GetPower<GreedyBankPower>();
                if (bank == null)
                {
                    await PowerCmd.Apply<GreedyBankPower>(choiceContext, base.Owner, base.Amount, base.Owner, cardPlay.Card);
                }
                else
                {
                    await PowerCmd.ModifyAmount(choiceContext, bank, base.Amount, base.Owner, cardPlay.Card);
                }
            }
            _hadLoadWhenMoraPlayed = false;
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
