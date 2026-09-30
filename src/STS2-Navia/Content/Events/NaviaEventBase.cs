using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using NaviaMod.Content.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Events;

/// <summary>
/// 娜维娅事件统一基线。限定事件仅在娜维娅在场时出现——事件文案深度绑定枫丹/刺玫会背景,
/// 不宜对异邦角色出现;多人时只要有一名娜维娅即放行(事件归属者的体验优先)。
/// 事件立绘/背景暂以占位接入(见 prep-art 的 EVENTS 表与各子类 AssetProfile),
/// 美工资产到位后替换路径即可;布局走原版 default_event_layout,无需自定义。
/// </summary>
public abstract class NaviaEventBase : ModEventTemplate
{
    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.Any(p => p.Character is Navia);
    }
}
