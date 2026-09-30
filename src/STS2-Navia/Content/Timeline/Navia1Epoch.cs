using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Timeline.Scaffolding;

namespace NaviaMod.Content.Timeline;

/// <summary>
/// 第一章·启程(杂项纪元,DarvEpoch 同款):娜维娅的冒险起点。
/// 角色**不锁**(设计裁定 2026-09-29:玩家可自由选择),本章仅作剧情节点,
/// 揭示条件=完成一局娜维娅(Navia 类上的 [UnlockEpochAfterRunAs])。
/// 基类须为 ModEpochTemplate:时间线槽位合并补丁按 `is ModEpochTemplate` 过滤,
/// 且 Era/EraPosition 由布局注册表解析([AutoTimelineSlot] 注册,勿手工 override——模板已密封)。
/// </summary>
[RegisterEpoch]
[RegisterStoryEpoch(typeof(NaviaStory))]
[AutoTimelineSlot(EpochEra.Blight2)]
public sealed class Navia1Epoch : ModEpochTemplate
{
    public override string Id => "STS2_NAVIA_EPOCH_1";

    public override string StoryId => "Navia";

    /// <summary>缩略图槽覆盖:原版缩略图走 epoch_atlas 图集,mod 无条目会 NOPE;
    /// 大图留空走原版 timeline/epoch_portraits 全局路径推导(prep-art 已供图)。</summary>
    public override EpochAssetProfile AssetProfile => new(
        PackedPortraitPath: "res://STS2-Navia/images/timeline/sts2_navia_epoch_1_thumb.png");

    public override void QueueUnlocks()
    {
        LocString locString = new LocString("epochs", Id + ".unlock");
        NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
    }
}
