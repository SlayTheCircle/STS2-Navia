using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Timeline;
using NaviaMod.Content.Potions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Timeline.Scaffolding;

namespace NaviaMod.Content.Timeline;

/// <summary>
/// 第三章·好奇心(药水纪元,Regent4Epoch 同款):解锁 枫达/刺玫佳酿/铳枪润滑油。
/// 揭示条件=用娜维娅累计击败 3 个首领([UnlockEpochAfterBossVictories(3)]);
/// 池过滤见 NaviaPotionPool.GetUnlockedPotions。
/// 基类 PotionUnlockEpochTemplate 自动提供 UnlockText/QueueUnlocks。
/// </summary>
[RegisterEpoch]
[RegisterStoryEpoch(typeof(NaviaStory))]
[AutoTimelineSlot(EpochEra.Flourish3)]
public sealed class Navia3Epoch : PotionUnlockEpochTemplate
{
    /// <summary>解锁物类型(单一来源):模板 QueueUnlocks/UnlockText 与池门控共用。</summary>
    public static readonly Type[] PotionUnlockTypes =
    {
        typeof(Fonta), typeof(RosulaWine), typeof(FirearmLubricant),
    };

    public override string Id => "STS2_NAVIA_EPOCH_3";

    public override string StoryId => "Navia";

    /// <summary>缩略图槽覆盖(原版 epoch_atlas 图集无 mod 条目会 NOPE);大图走原版全局路径推导。</summary>
    public override EpochAssetProfile AssetProfile => new(
        PackedPortraitPath: "res://STS2-Navia/images/timeline/sts2_navia_epoch_3_thumb.png");

    protected override IEnumerable<Type> PotionTypes => PotionUnlockTypes;
}
