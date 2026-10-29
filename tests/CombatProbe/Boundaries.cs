using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NaviaMod.Content.Visuals.CombatAnimation;

// 隔离抽牌动画、原生日志与视觉输出；不替换能力钩子、克隆、次数更新和订阅实现。
internal static class Boundaries
{
    public static readonly List<Player> Draws = [];
    public static readonly List<decimal> DrawAmounts = [];
    public static readonly List<CardModel> ExhaustedCards = [];
    public static readonly List<(CardModel Card, Creature? Target)> AutoPlays = [];
    public static readonly List<(string Action, Creature? Creature)> Visuals = [];
    public static Action<CardModel, Creature?>? AutoPlayEffect;

    public static void Install()
    {
        var harmony = new Harmony("navia.combat.probe");
        void Patch(MethodBase method, string prefix) => harmony.Patch(method,
            prefix: new HarmonyMethod(typeof(Boundaries), prefix));
        Patch(typeof(Godot.OS).GetMethod("GetCmdlineArgs")!, nameof(CommandLine));
        Patch(typeof(Godot.OS).GetMethod("HasFeature")!, nameof(Feature));
        Patch(typeof(MegaCrit.Sts2.Core.Logging.ConsoleLogPrinter).GetMethod("Print")!, nameof(Skip));
        Patch(typeof(CardPileCmd).GetMethod("Draw", [typeof(PlayerChoiceContext), typeof(decimal), typeof(Player), typeof(bool)])!, nameof(Draw));
#if !NAVIA_GAME_0107_1
        Patch(typeof(CardPileCmd).GetMethod("DrawWithoutBlockingOnOtherPlayers")!, nameof(DrawWithoutBlocking));
#endif
        Patch(typeof(CardPileCmd).GetMethod("AddGeneratedCardToCombat")!, nameof(AddGenerated));
        Patch(typeof(CreatureCmd).GetMethod("GainBlock", [typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(CardPlay), typeof(bool)])!, nameof(GainBlock));
        Patch(typeof(AttackCommand).GetMethod("Execute")!, nameof(AttackExecute));
        Patch(typeof(CardCmd).GetMethod("Exhaust")!, nameof(Exhaust));
        Patch(typeof(CardCmd).GetMethod("AutoPlay")!, nameof(AutoPlay));
        Patch(typeof(CombatManager).GetProperty("IsInProgress")!.GetMethod!, nameof(InProgress));
        Patch(typeof(CombatManager).GetProperty("IsOverOrEnding")!.GetMethod!, nameof(NotEnding));
        // 0.107.1 的 IsEnding 在无战斗状态时会空引用;探针不启动战斗,恒 false。
        Patch(typeof(CombatManager).GetProperty("IsEnding")!.GetMethod!, nameof(NotEnding));
        foreach (string name in new[] { "Refresh", "PlaySkill" })
            Patch(typeof(NaviaCombatVisuals).GetMethod(name)!, nameof(Visual));
        Patch(typeof(NaviaCombatVisuals).GetMethod("Prewarm")!, nameof(Prewarm));
    }

    public static void ResetMultiplayer()
    {
        Draws.Clear();
        DrawAmounts.Clear();
        ExhaustedCards.Clear();
        AutoPlays.Clear();
        AutoPlayEffect = null;
    }

    public static bool CommandLine(ref string[] __result) { __result = []; return false; }
    public static bool Feature(ref bool __result) { __result = false; return false; }
    public static bool InProgress(ref bool __result) { __result = true; return false; }
    public static bool NotEnding(ref bool __result) { __result = false; return false; }
    public static bool Skip() => false;
    public static bool Draw(decimal count, Player player, ref Task<IEnumerable<CardModel>> __result)
    {
        Draws.Add(player);
        DrawAmounts.Add(count);
        __result = Task.FromResult(Enumerable.Empty<CardModel>());
        return false;
    }
#if !NAVIA_GAME_0107_1
    public static bool DrawWithoutBlocking(decimal count, Player player, ref Task __result)
    {
        Draws.Add(player);
        DrawAmounts.Add(count);
        __result = Task.CompletedTask;
        return false;
    }
#endif
    public static bool AddGenerated(CardModel card, PileType newPileType, ref Task<CardPileAddResult> __result)
    {
        Fixture.AddToPile(card, newPileType);
        __result = Task.FromResult(new CardPileAddResult { success = true, cardAdded = card });
        return false;
    }
    public static bool GainBlock(Creature creature, decimal amount, ref Task<decimal> __result)
    {
        creature.GainBlockInternal(amount);
        __result = Task.FromResult(amount);
        return false;
    }
    public static bool AttackExecute(AttackCommand __instance, ref Task<AttackCommand> __result)
    {
        __result = Task.FromResult(__instance);
        return false;
    }
#if NAVIA_GAME_0107_1
    public static bool Exhaust(CardModel card, ref Task __result)
    {
        ExhaustedCards.Add(card);
        __result = Task.CompletedTask;
        return false;
    }
#else
    public static bool Exhaust(CardModel card, ref Task<CardPileAddResult?> __result)
    {
        ExhaustedCards.Add(card);
        __result = Task.FromResult<CardPileAddResult?>(null);
        return false;
    }
#endif
    public static bool AutoPlay(CardModel card, Creature? target, ref Task __result)
    {
        AutoPlays.Add((card, target));
        AutoPlayEffect?.Invoke(card, target);
        __result = Task.CompletedTask;
        return false;
    }
    public static bool Visual(Creature creature, MethodBase __originalMethod)
    {
        Visuals.Add((__originalMethod.Name, creature));
        return false;
    }
    public static bool Prewarm() { Visuals.Add(("Prewarm", null)); return false; }
}
