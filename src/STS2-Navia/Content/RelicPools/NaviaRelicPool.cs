using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using NaviaMod.Content.Timeline;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.RelicPools;

/// <summary>娜维娅遗物池(TypeList 模式):成员由 [RegisterRelic(typeof(NaviaRelicPool))] 特性聚合。</summary>
public sealed class NaviaRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "navia";

    public override Color LabOutlineColor => new Color("E8B23A");

    // 世界线门控(IroncladRelicPool.cs 同款):第二章·荒疫揭示前,章内 3 遗物不进掉落池。
    public override System.Collections.Generic.IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
    {
        var list = base.AllRelics.ToList();
        if (!unlockState.IsEpochRevealed<Navia2Epoch>())
        {
            list.RemoveAll(r => Navia2Epoch.RelicUnlockTypes.Any(t => ModelDb.GetId(t) == r.Id));
        }
        return list;
    }
}
