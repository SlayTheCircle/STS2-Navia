using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace NaviaMod.Content.Keywords;

/// <summary>
/// 娜维娅专有名词的悬停解释规范:
/// - 机制名词(装填/礼炮轰鸣)→ 本处关键词(卡面横幅 + 文字解释框,走 card_keywords 表);
/// - 卡牌名词(金花礼炮/闪耀摩拉)→ **卡面预览**,挂法:`AdditionalHoverTips => HoverTipFactory.FromCard<GoldenRoseCannon>()`(原版「精准」引小刀同款)。
/// 解释文本唯一权威来源:localization/{lang}/card_keywords.json(键 = 下列常量 + ".title/.description")。
/// 修改机制实现时必须同步核对解释文本,文案不准就是事故。
/// </summary>
public static class NaviaKeywords
{
    /// <summary>装填:最多6层(穿心膛线后9),每2层视为1点敏捷,可被卡牌消耗。</summary>
    public const string Load = "STS2_NAVIA_KEYWORD_LOAD";

    /// <summary>礼炮轰鸣:无炮生成金花礼炮,有炮则全体强化。</summary>
    public const string Salvo = "STS2_NAVIA_KEYWORD_SALVO";

    /// <summary>支援:施加在另一张卡上的战斗内强化,一卡一个,战斗结束消失。</summary>
    public const string Support = "STS2_NAVIA_KEYWORD_SUPPORT";

    /// <summary>把已注册的关键词(若注册表可用)并入卡牌关键词集合。未注册时静默跳过,不抛错。</summary>
    public static void AddTo(HashSet<CardKeyword> set, params string[] ids)
    {
        foreach (string id in ids)
        {
            if (ModKeywordRegistry.TryGetCardKeyword(id, out CardKeyword keyword))
            {
                set.Add(keyword);
            }
        }
    }

    /// <summary>
    /// 把已注册关键词转为悬停提示。遗物/能力/药水/附魔等非卡牌模型的 <c>ExtraHoverTips</c> 用
    /// (卡牌本体走 CanonicalKeywords,由卡牌悬停管线自动展开;这些模型没有该管线,须显式挂)。
    /// </summary>
    public static IEnumerable<IHoverTip> HoverTips(params string[] ids) => ids.ToHoverTips();
}
