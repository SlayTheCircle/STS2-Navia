using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Enchantments;

/// <summary>
/// 娜维娅「支援」附魔统一基线(标记家族):
/// - 支援 = 施加在另一张卡上的战斗内强化。协同卡用 `card.Enchantment is NaviaSupportEnchantment` 查询;
/// - 效果文案写在 loc 表 enchantments 的 extraCardText(基类默认开启 HasExtraCardText,卡面自动附加显示);
/// - 作用域=本场战斗,由引擎克隆边界自动保证:战斗开始时主卡组(PileType.Deck)克隆进战斗牌堆,
///   战斗内附魔随 CombatState 消亡,不需要也不应该写清场钩子;
/// - 图标按类名约定解析 res://STS2-Navia/images/enchantments/&lt;类名&gt;.png,
///   缺图回落共享徽记 navia_support.png(由统一支援徽记母版派生，效果差异由文本表达);
/// - 伤害/格挡/打出次数修改必须用 Enchant* 专用钩子(先于一切其他修改钩子运行),不要用 Modify*;
/// - 一张卡同时只能持有一个支援(vanilla 单附魔槽,非同型不可叠)。
/// </summary>
public abstract class NaviaSupportEnchantment : ModEnchantmentTemplate
{
    public override bool HasExtraCardText => true;
    /// <summary>文本提及机制名词,挂关键词词条悬停解释。</summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Support);


    private string? ResolveIcon()
    {
        string own = $"res://{ModEntry.ModId}/images/enchantments/{GetType().Name}.png";
        if (Godot.ResourceLoader.Exists(own))
        {
            return own;
        }
        string badge = $"res://{ModEntry.ModId}/images/enchantments/navia_support.png";
        return Godot.ResourceLoader.Exists(badge) ? badge : base.CustomIconPath;
    }

    public override string? CustomIconPath => ResolveIcon();
}
