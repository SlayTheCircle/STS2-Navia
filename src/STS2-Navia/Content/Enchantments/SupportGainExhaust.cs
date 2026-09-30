using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NaviaMod.Content.Enchantments;

/// <summary>
/// 消耗支援(「率直作风」施加):被附魔的牌获得消耗(本场战斗)。
/// OnEnchant → CardModel.AddKeyword(vanilla Royally Approved 加关键词同款;
/// LocalKeywords 为 HashSet,附魔刷新重跑时重复添加天然幂等;据守阵地同款加法)。
/// 不限卡型:攻击/技能/能力均可(状态/诅咒/任务在施加卡的选牌过滤器里已排除)。
/// </summary>
[RegisterEnchantment]
public sealed class SupportGainExhaust : NaviaSupportEnchantment
{
    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Exhaust);
    }
}
