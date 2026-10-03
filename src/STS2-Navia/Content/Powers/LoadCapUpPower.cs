using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 装填上限提升标记(隐藏增益,数值调整V4):每层使装填上限 +3,可叠加(第二张穿心膛线 → 上限 12)。
/// 由「穿心膛线」施加;<see cref="LoadPower.CapFor"/> 按层数计算当前上限。
/// </summary>
[RegisterPower]
public sealed class LoadCapUpPower : PowerModel
{
    public const int CapPerStack = 3;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;
}
