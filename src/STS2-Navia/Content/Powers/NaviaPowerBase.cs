using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 娜维娅能力(Power)统一基线:图标按类名约定解析 res://STS2-Navia/images/powers/&lt;类名&gt;.png
/// (prep-art 产出 256²)。资源缺失时回落基类占位图。
/// 必须同时供小图(CustomIconPath→buff 条图集槽)与大图(CustomBigIconPath→叠层闪烁/获得与消失
/// 头顶提示等 VFX,走 PowerModel.BigIcon)——只喂小图时静态正常、动效全是 NOPE(实测事故)。
/// 新增可见 Power 一律继承本类,不要直接继承 ModPowerTemplate;隐藏 Power(内部标记)继承 PowerModel 即可。
/// </summary>
public abstract class NaviaPowerBase : ModPowerTemplate
{
    private string? ResolveIcon()
    {
        string path = $"res://{ModEntry.ModId}/images/powers/{GetType().Name}.png";
        return Godot.ResourceLoader.Exists(path) ? path : null;
    }

    public override string? CustomIconPath => ResolveIcon() ?? base.CustomIconPath;

    public override string? CustomBigIconPath => ResolveIcon() ?? base.CustomBigIconPath;
}
