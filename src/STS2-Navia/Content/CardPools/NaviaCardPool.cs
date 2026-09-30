using System.Linq;
using Godot;
using NaviaMod.Content.Timeline;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace NaviaMod.Content.CardPools;

/// <summary>
/// 娜维娅卡池(TypeList 模式):池成员由各卡类上的 [RegisterCard(typeof(NaviaCardPool))] 特性自动聚合,
/// 本类只负责主题属性。增删卡 = 增删卡类文件,无需改动本文件。
/// </summary>
public sealed class NaviaCardPool : TypeListCardPoolModel, IModColorfulPhilosophersCardPool
{
    // 金色主题:vanilla 全部卡框共用一张底图,颜色即 HSV 着色参数
    // (校准:红 h.025 / 橙 h.12 / 绿 h.32 / 蓝 h.55 / 粉 h.965;金 ≈ 橙往黄偏一点、饱和略降)。
    // 进游戏目视后可微调这三个数。能量图标由已交付金玫瑰母版派生。
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateHsvShaderMaterial(0.15f, 1.2f, 1.2f);

    public override string Title => "navia";

    public override string EnergyColorName => "navia";

    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    public override string? BigEnergyIconPath => "res://STS2-Navia/images/energy/navia_energy_big.png";

    public override string? TextEnergyIconPath => "res://STS2-Navia/images/energy/navia_energy_text.png";

    public override Color DeckEntryCardColor => new Color("E8B23A");

    public override Color EnergyOutlineColor => new Color("8A6210");

    public override bool IsColorless => false;

    // 世界线门控(RegentCardPool.cs 同款):第四章·尖塔的尽头揭示前,章内 3 卡不进奖励/商店池。
    protected override System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.Models.CardModel> FilterThroughEpochs(
        MegaCrit.Sts2.Core.Unlocks.UnlockState unlockState,
        System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.Models.CardModel> cards)
    {
        var list = cards.ToList();
        if (!unlockState.IsEpochRevealed<Navia4Epoch>())
        {
            list.RemoveAll(c => Navia4Epoch.CardUnlockTypes.Any(t => MegaCrit.Sts2.Core.Models.ModelDb.GetId(t) == c.Id));
        }
        return list;
    }
}
