using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 娜维娅卡牌统一基线:按类名约定解析卡图 res://STS2-Navia/images/cards/&lt;类名&gt;.png
/// (素材由 scripts/prep-art.sh 产出、经 Godot 导入后随 pck 分发,路径布局照 Hikari 实证)。
/// 资源不存在时(如美术返工中)回落基类占位图,不产生坏纹理。
/// 新增卡牌一律继承本类,不要直接继承 ModCardTemplate。
/// </summary>
public abstract class NaviaCardBase : ModCardTemplate
{
    public NaviaCardBase(int cost, CardType type, CardRarity rarity, TargetType target)
        : base(cost, type, rarity, target)
    {
    }

    public override string? CustomPortraitPath
    {
        get
        {
            string path = $"res://{ModEntry.ModId}/images/cards/{GetType().Name}.png";
            return Godot.ResourceLoader.Exists(path) ? path : base.CustomPortraitPath;
        }
    }
}
