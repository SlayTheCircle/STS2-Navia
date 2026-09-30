using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Potions;

/// <summary>
/// 娜维娅药水统一基线:图标双槽按类名约定解析(prep-art 产出 256² 主图 + 描边变体)。
/// 主图 res://STS2-Navia/images/potions/&lt;类名&gt;.png,描边 &lt;类名&gt;_outline.png。
/// 资源缺失时返回 null 走原版解析。新增药水一律继承本类,不要直接继承 ModPotionTemplate。
/// </summary>
public abstract class NaviaPotionBase : ModPotionTemplate
{
    public override string? CustomImagePath => ResolveIcon(".png");

    public override string? CustomOutlinePath => ResolveIcon("_outline.png");

    private string? ResolveIcon(string suffix)
    {
        string path = $"res://{ModEntry.ModId}/images/potions/{GetType().Name}{suffix}";
        return Godot.ResourceLoader.Exists(path) ? path : null;
    }
}
