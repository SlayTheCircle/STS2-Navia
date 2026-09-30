using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Timeline;
using NaviaMod.Content.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Timeline.Scaffolding;

namespace NaviaMod.Content.Timeline;

/// <summary>
/// 第四章·尖塔的尽头(卡牌纪元,Ironclad2Epoch 同款):解锁 穿心膛线/军火大亨/强制买断。
/// 揭示条件=用娜维娅通关进阶 1([UnlockEpochAfterAscensionOneWin],vanilla 第七章同型);
/// 池过滤见 NaviaCardPool.FilterThroughEpochs。
/// 基类 CardUnlockEpochTemplate 自动提供 UnlockText/QueueUnlocks。
/// </summary>
[RegisterEpoch]
[RegisterStoryEpoch(typeof(NaviaStory))]
[AutoTimelineSlot(EpochEra.Invitation5)]
public sealed class Navia4Epoch : CardUnlockEpochTemplate
{
    /// <summary>解锁物类型(单一来源):模板 QueueUnlocks/UnlockText 与池门控共用。</summary>
    public static readonly Type[] CardUnlockTypes =
    {
        typeof(RifledBarrel), typeof(ArmsDealer), typeof(ForcedBuyout),
    };

    public override string Id => "STS2_NAVIA_EPOCH_4";

    public override string StoryId => "Navia";

    /// <summary>缩略图槽覆盖(原版 epoch_atlas 图集无 mod 条目会 NOPE);大图走原版全局路径推导。</summary>
    public override EpochAssetProfile AssetProfile => new(
        PackedPortraitPath: "res://STS2-Navia/images/timeline/sts2_navia_epoch_4_thumb.png");

    protected override IEnumerable<Type> CardTypes => CardUnlockTypes;
}
