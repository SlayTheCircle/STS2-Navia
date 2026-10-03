using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NaviaMod.Content.Visuals.CombatAnimation;

// 隔离抽牌动画、原生日志与视觉输出；不替换能力钩子、克隆、次数更新和订阅实现。
internal static class Boundaries
{
    public static readonly List<Player> Draws = [];
    public static readonly List<(string Action, Creature? Creature)> Visuals = [];

    public static void Install()
    {
        var harmony = new Harmony("navia.combat.probe");
        void Patch(MethodBase method, string prefix) => harmony.Patch(method,
            prefix: new HarmonyMethod(typeof(Boundaries), prefix));
        Patch(typeof(Godot.OS).GetMethod("GetCmdlineArgs")!, nameof(CommandLine));
        Patch(typeof(Godot.OS).GetMethod("HasFeature")!, nameof(Feature));
        Patch(typeof(MegaCrit.Sts2.Core.Logging.ConsoleLogPrinter).GetMethod("Print")!, nameof(Skip));
        Patch(typeof(CardPileCmd).GetMethod("Draw", [typeof(PlayerChoiceContext), typeof(decimal), typeof(Player), typeof(bool)])!, nameof(Draw));
        Patch(typeof(CardPileCmd).GetMethod("AddGeneratedCardToCombat")!, nameof(AddGenerated));
        Patch(typeof(CombatManager).GetProperty("IsInProgress")!.GetMethod!, nameof(InProgress));
        Patch(typeof(CombatManager).GetProperty("IsOverOrEnding")!.GetMethod!, nameof(NotEnding));
        // 0.107.1 的 IsEnding 在无战斗状态时会空引用;探针不启动战斗,恒 false。
        Patch(typeof(CombatManager).GetProperty("IsEnding")!.GetMethod!, nameof(NotEnding));
        foreach (string name in new[] { "Refresh", "PlaySkill" })
            Patch(typeof(NaviaCombatVisuals).GetMethod(name)!, nameof(Visual));
        Patch(typeof(NaviaCombatVisuals).GetMethod("Prewarm")!, nameof(Prewarm));
    }

    public static bool CommandLine(ref string[] __result) { __result = []; return false; }
    public static bool Feature(ref bool __result) { __result = false; return false; }
    public static bool InProgress(ref bool __result) { __result = true; return false; }
    public static bool NotEnding(ref bool __result) { __result = false; return false; }
    public static bool Skip() => false;
    public static bool Draw(Player player, ref Task<IEnumerable<CardModel>> __result)
    {
        Draws.Add(player);
        __result = Task.FromResult(Enumerable.Empty<CardModel>());
        return false;
    }
    public static bool AddGenerated(CardModel card, PileType newPileType, ref Task<CardPileAddResult> __result)
    {
        Fixture.AddToPile(card, newPileType);
        __result = Task.FromResult(new CardPileAddResult { success = true, cardAdded = card });
        return false;
    }
    public static bool Visual(Creature creature, MethodBase __originalMethod)
    {
        Visuals.Add((__originalMethod.Name, creature));
        return false;
    }
    public static bool Prewarm() { Visuals.Add(("Prewarm", null)); return false; }
}
