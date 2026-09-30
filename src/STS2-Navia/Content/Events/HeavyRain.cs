using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Events;

/// <summary>
/// 大雨倾盆(3 层事件,Glory)。
/// 冒雨前进=随机移除 1 张(与 FromDeckForRemoval 同口径:IsRemovable 过滤,随机不经选牌);
/// 等待雨势减小=普通遗物 + [睡眠不佳]入卡组(UnrestSite 同款入口)。
/// </summary>
[RegisterActEvent(typeof(Glory))]
public sealed class HeavyRain : NaviaEventBase
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://STS2-Navia/images/events/HeavyRain.png",
        BackgroundScenePath: "res://scenes/events/background_scenes/drowning_beacon.tscn");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new List<EventOption>
        {
            new EventOption(this, PressOn, InitialOptionKey("PRESS_ON")),
            new EventOption(this, WaitForRainToEase, InitialOptionKey("WAIT"), HoverTipFactory.FromCardWithCardHoverTips<PoorSleep>()),
        };
    }

    private async Task PressOn()
    {
        List<CardModel> pool = base.Owner.Deck.Cards.Where(c => c.IsRemovable).ToList();
        CardModel? victim = pool.Count > 0 ? base.Rng.NextItem(pool) : null;
        if (victim != null)
        {
            await CardPileCmd.RemoveFromDeck(new[] { victim });
        }
        SetEventFinished(PageDescription("PRESS_ON"));
    }

    private async Task WaitForRainToEase()
    {
        RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner, RelicRarity.Common).ToMutable();
        await RelicCmd.Obtain(relic, base.Owner);
        await CardPileCmd.AddCursesToDeck(new[] { ModelDb.Card<PoorSleep>() }, base.Owner);
        SetEventFinished(PageDescription("WAIT"));
    }
}
