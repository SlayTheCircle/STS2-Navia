using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Events;
using NaviaMod.Content.Characters;

namespace NaviaMod.Content.Events;

/// <summary>
/// 给 5 个原版事件追加娜维娅专属选项(设计:特殊事件扩充)。
/// 注入点:EventModel.GenerateInitialOptionsWrapper 基方法后缀——五个目标事件均未覆写它(已核实),
/// Ancient 事件覆写了该方法,天然不受影响。仅当事件归属者是娜维娅时追加。
/// 补丁由游戏 ModManager 自动 PatchAll:官方语义是无 ModInitializerAttribute 的模组程序集
/// 一律自动 Harmony.PatchAll(见 ModManager 加载流程),本类是程序集中唯一的补丁类。
/// SetEventFinished 是 protected,外部 handler 经缓存的反射委托调用(每次启动仅反射一次)。
/// </summary>
[HarmonyPatch(typeof(EventModel), "GenerateInitialOptionsWrapper")]
internal static class VanillaEventNaviaOptions
{
    private static readonly Action<EventModel, LocString>? FinishDelegate = CreateFinishDelegate();

    private static Action<EventModel, LocString>? CreateFinishDelegate()
    {
        MethodInfo? method = typeof(EventModel).GetMethod("SetEventFinished",
            BindingFlags.Instance | BindingFlags.NonPublic, new[] { typeof(LocString) });
        if (method == null)
        {
            Log.Error("[Navia] 找不到 EventModel.SetEventFinished——游戏版本不兼容,原版事件扩充将不可用");
            return null;
        }
        return (Action<EventModel, LocString>)Delegate.CreateDelegate(typeof(Action<EventModel, LocString>), method);
    }

    [HarmonyPostfix]
    private static void AppendNaviaOptions(EventModel __instance, ref IReadOnlyList<EventOption> __result)
    {
        try
        {
            if (FinishDelegate == null || __instance.Owner == null || __instance.Owner.Character is not Navia)
            {
                return;
            }
            EventOption? extra = __instance switch
            {
                StoneOfAllTime => new EventOption(__instance, () => BreakBoulderAsync(__instance),
                    "STONE_OF_ALL_TIME.pages.INITIAL.options.NAVIA_BREAK_BOULDER", HoverTipFactory.FromEnchantment<Glam>()),
                Trial => new EventOption(__instance, () => RebukeAsync(__instance),
                    "TRIAL.pages.INITIAL.options.NAVIA_REBUKE"),
                SpiralingWhirlpool => new EventOption(__instance, () => ReminisceAsync(__instance),
                    "SPIRALING_WHIRLPOOL.pages.INITIAL.options.NAVIA_REMINISCE"),
                Bugslayer => new EventOption(__instance, () => ShareExperienceAsync(__instance),
                    "BUGSLAYER.pages.INITIAL.options.NAVIA_SHARE_EXPERIENCE"),
                TeaMaster => new EventOption(__instance, () => ShareTeaAsync(__instance),
                    "TEA_MASTER.pages.INITIAL.options.NAVIA_SHARE_TEA"),
                _ => null,
            };
            if (extra == null)
            {
                return;
            }
            if (__result is List<EventOption> list)
            {
                list.Add(extra);
            }
            else
            {
                __result = __result.Append(extra).ToList();
            }
        }
        catch (Exception e)
        {
            Log.Error($"[Navia] 原版事件选项追加失败({__instance.GetType().Name}): {e}");
        }
    }

    /// <summary>推动巨石:选 1 张卡附魔华彩 1 层(WaterloggedScriptorium 的附魔范式)。</summary>
    private static async Task BreakBoulderAsync(EventModel evt)
    {
        Player player = evt.Owner!;
        EnchantmentModel glam = ModelDb.Enchantment<Glam>();
        CardModel? card = (await CardSelectCmd.FromDeckForEnchantment(player, glam, 1,
            new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Enchant<Glam>(card, 1m);
        }
        FinishDelegate!.Invoke(evt, new LocString("events", "STONE_OF_ALL_TIME.pages.NAVIA_BREAK_BOULDER.description"));
    }

    /// <summary>审判庭:选 1 张卡移除(ZenWeaver 的移除范式)。</summary>
    private static async Task RebukeAsync(EventModel evt)
    {
        Player player = evt.Owner!;
        await CardPileCmd.RemoveFromDeck(
            (await CardSelectCmd.FromDeckForRemoval(player, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1))).ToList());
        FinishDelegate!.Invoke(evt, new LocString("events", "TRIAL.pages.NAVIA_REBUKE.description"));
    }

    /// <summary>涡旋水潭:选 1 张卡升级(AromaOfChaos 的 MaintainControl 范式)。</summary>
    private static async Task ReminisceAsync(EventModel evt)
    {
        Player player = evt.Owner!;
        CardModel? card = (await CardSelectCmd.FromDeckForUpgrade(player,
            new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Upgrade(card);
        }
        FinishDelegate!.Invoke(evt, new LocString("events", "SPIRALING_WHIRLPOOL.pages.NAVIA_REMINISCE.description"));
    }

    /// <summary>虫子克星:随机遗物(UnrestSite 的 PullNextRelicFromFront 范式,随机稀有度)。</summary>
    private static async Task ShareExperienceAsync(EventModel evt)
    {
        Player player = evt.Owner!;
        RelicModel relic = RelicFactory.PullNextRelicFromFront(player).ToMutable();
        await RelicCmd.Obtain(relic, player);
        FinishDelegate!.Invoke(evt, new LocString("events", "BUGSLAYER.pages.NAVIA_SHARE_EXPERIENCE.description"));
    }

    /// <summary>茶艺大师:回 5 血 + 得 50 金。</summary>
    private static async Task ShareTeaAsync(EventModel evt)
    {
        Player player = evt.Owner!;
        await CreatureCmd.Heal(player.Creature, 5m);
        await PlayerCmd.GainGold(50m, player);
        FinishDelegate!.Invoke(evt, new LocString("events", "TEA_MASTER.pages.NAVIA_SHARE_TEA.description"));
    }
}
