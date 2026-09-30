using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Events;

/// <summary>
/// 故乡的回忆(1 层事件,Overgrowth):草坪生火小憩,回忆枫丹。
/// 晴天=移除 1 张卡(DoorsOfLightAndDark 范式);阴天=移除 2 张卡 + [愧疚]入卡组
/// (Wellspring 的 AddCursesToDeck 官方入口——设计原文写「手牌」,但场外无手牌,以卡组为准)。
/// </summary>
[RegisterActEvent(typeof(Overgrowth))]
public sealed class HometownMemory : NaviaEventBase
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://STS2-Navia/images/events/HometownMemory.png",
        BackgroundScenePath: "res://scenes/events/background_scenes/unrest_site.tscn");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new List<EventOption>
        {
            new EventOption(this, Sunny, InitialOptionKey("SUNNY")),
            new EventOption(this, Overcast, InitialOptionKey("OVERCAST"), HoverTipFactory.FromCardWithCardHoverTips<Guilty>()),
        };
    }

    private async Task Sunny()
    {
        await CardPileCmd.RemoveFromDeck(
            (await CardSelectCmd.FromDeckForRemoval(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1))).ToList());
        SetEventFinished(PageDescription("SUNNY"));
    }

    private async Task Overcast()
    {
        await CardPileCmd.RemoveFromDeck(
            (await CardSelectCmd.FromDeckForRemoval(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2))).ToList());
        await CardPileCmd.AddCursesToDeck(new[] { ModelDb.Card<Guilty>() }, base.Owner);
        SetEventFinished(PageDescription("OVERCAST"));
    }
}
