using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 装填上限提升标记(隐藏增益):存在时装填上限从 6 提升至 9。
/// 由「穿心膛线」(内容批次实装)施加;<see cref="LoadPower.Gain"/> 读取本标记决定当前上限。
/// </summary>
[RegisterPower]
public sealed class LoadCapUpPower : PowerModel
{
    public const int UpgradedCap = 9;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;
}
