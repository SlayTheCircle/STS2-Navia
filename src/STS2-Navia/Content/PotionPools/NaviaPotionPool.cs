using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using NaviaMod.Content.Timeline;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.PotionPools;

/// <summary>娜维娅药水池(TypeList 模式):成员由 [RegisterPotion(typeof(NaviaPotionPool))] 特性聚合。</summary>
public sealed class NaviaPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "navia";

    public override Color LabOutlineColor => new Color("E8B23A");

    // 世界线门控(RegentPotionPool 同款):第三章·好奇心揭示前,章内 3 药水不进奖励池。
    public override System.Collections.Generic.IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
    {
        var list = base.AllPotions.ToList();
        if (!unlockState.IsEpochRevealed<Navia3Epoch>())
        {
            list.RemoveAll(p => Navia3Epoch.PotionUnlockTypes.Any(t => ModelDb.GetId(t) == p.Id));
        }
        return list;
    }
}
