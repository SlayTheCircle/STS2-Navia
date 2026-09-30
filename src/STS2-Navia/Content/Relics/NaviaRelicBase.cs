using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 娜维娅遗物统一基线:三槽图标按类名约定解析(prep-art 产出 256² 主图 + 描边变体)。
/// 主图 res://STS2-Navia/images/relics/&lt;类名&gt;.png,描边 &lt;类名&gt;_outline.png,大图复用主图。
/// 遗物条会同时渲染主图标与描边槽,悬停/图鉴用大图——三槽都要供,
/// 缺槽会露出游戏的 NOPE 缺图纹理。资源缺失时返回 null 走原版解析,不产生坏纹理。
/// 新增遗物一律继承本类,不要直接继承 ModRelicTemplate。
/// </summary>
public abstract class NaviaRelicBase : ModRelicTemplate
{
    public override string? CustomIconPath => ResolveIcon(".png");

    public override string? CustomIconOutlinePath => ResolveIcon("_outline.png");

    public override string? CustomBigIconPath => ResolveIcon(".png");

    private string? ResolveIcon(string suffix)
    {
        string path = $"res://{ModEntry.ModId}/images/relics/{GetType().Name}{suffix}";
        return Godot.ResourceLoader.Exists(path) ? path : null;
    }
}
