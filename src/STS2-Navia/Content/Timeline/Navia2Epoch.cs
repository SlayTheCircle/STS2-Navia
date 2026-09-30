using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Timeline;
using NaviaMod.Content.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Timeline.Scaffolding;

namespace NaviaMod.Content.Timeline;

/// <summary>
/// 第二章·荒疫(遗物纪元,Defect3Epoch 同款):解锁 刺玫会特制铳枪/明黄缎带/美味的马卡龙。
/// 揭示条件=用娜维娅通关一次([UnlockEpochAfterWinAs]);池过滤见 NaviaRelicPool.GetUnlockedRelics。
/// 基类 RelicUnlockEpochTemplate 自动提供 UnlockText/QueueUnlocks(解锁展示文案读前三项,故每章恰好 3 件)。
/// </summary>
[RegisterEpoch]
[RegisterStoryEpoch(typeof(NaviaStory))]
[AutoTimelineSlot(EpochEra.Flourish2)]
public sealed class Navia2Epoch : RelicUnlockEpochTemplate
{
    /// <summary>解锁物类型(单一来源):模板 QueueUnlocks/UnlockText 与池门控共用。</summary>
    public static readonly Type[] RelicUnlockTypes =
    {
        typeof(RosulaMusket), typeof(GoldenRibbon), typeof(Macaron),
    };

    public override string Id => "STS2_NAVIA_EPOCH_2";

    public override string StoryId => "Navia";

    /// <summary>缩略图槽覆盖(原版 epoch_atlas 图集无 mod 条目会 NOPE);大图走原版全局路径推导。</summary>
    public override EpochAssetProfile AssetProfile => new(
        PackedPortraitPath: "res://STS2-Navia/images/timeline/sts2_navia_epoch_2_thumb.png");

    protected override IEnumerable<Type> RelicTypes => RelicUnlockTypes;
}
