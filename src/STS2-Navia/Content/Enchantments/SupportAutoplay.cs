using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NaviaMod.Content.Enchantments;

/// <summary>
/// 号角支援(「热情昂扬」施加):被附魔的牌在手牌中时,每当其拥有者打出一张带支援效果的牌,
/// 自动(免费)将该牌打出(本场战斗)。附魔不限卡型;目标由 AutoPlay 内部按 TargetType 随机指定
/// (AnyEnemy/AnyAlly 用 CombatTargets 确定性 RNG,自身/无目标卡传 null)。
/// 触发判据用家族判据 cardPlay.Card.Enchantment is NaviaSupportEnchantment(引擎会把战斗牌堆中
/// 各卡牌的附魔一并列入 combat hook 监听者,本钩子因此能收到 AfterCardPlayed)。
/// 防递归:被自动打出的牌随即离开手牌,Card.Pile==Hand 不再成立,链式触发自然终止;
/// 两张同挂本附魔的牌互相链打时,后打出者收到触发时也已被前一次链打消耗掉手牌状态,同样终止。
/// </summary>
[RegisterEnchantment]
public sealed class SupportAutoplay : NaviaSupportEnchantment
{
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 未挂在卡上 / 被附魔的牌不在手牌(已打出、被消耗、还没抽到)→ 不触发。
        // 「本卡打出后已不在手牌」正是递归的终止条件。
        if (!base.HasCard || base.Card.Pile?.Type != PileType.Hand)
        {
            return;
        }
        // 只响应被附魔牌的拥有者自己打出的、带支援效果(家族判据)的其他牌。
        if (cardPlay.Card == base.Card || cardPlay.Card.Enchantment is not NaviaSupportEnchantment || cardPlay.Card.Owner != base.Card.Owner)
        {
            return;
        }
        await CardCmd.AutoPlay(choiceContext, base.Card, null);
    }
}
