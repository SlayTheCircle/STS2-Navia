using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Relics;
using NaviaMod.Content.Visuals;
using STS2RitsuLib.Interop.AutoRegistration;
using static Fixture;

internal static class VisualCases
{
    public static async Task Run()
    {
        Require(typeof(NaviaVisualHooks).GetCustomAttribute<RegisterSingletonAttribute>() != null, "Visual singleton lacks discovery registration");
        var hook = new NaviaVisualHooks();
        var state = new CombatState();
        var a = Player(); var b = Player(); var other = Player(new Ironclad());
        var allies = (List<Creature>)typeof(CombatState).GetField("_allies", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(state)!;
        allies.AddRange(new[] { a.Creature, b.Creature, other.Creature });
        var listeners = ModHelper.IterateAllCombatStateSubscribers(state).OfType<NaviaVisualHooks>().ToArray();
        Require(listeners.Length == 1 && ReferenceEquals(listeners[0], hook), "Visual singleton is absent or duplicated in native subscription stream");
        var ctx = new ThrowingPlayerChoiceContext();
        await listeners[0].BeforeCombatStart();
        Require(Boundaries.Visuals.Single().Action == "Prewarm", "Combat prewarm missing");
        foreach (RelicModel? relic in new RelicModel?[] { new RosulaEmblem(), new RosulaFragrance(), null })
        {
            Field(a, "_relics", relic is null ? new List<RelicModel>() : new List<RelicModel> { relic });
            Boundaries.Visuals.Clear();
            foreach (var listener in ModHelper.IterateAllCombatStateSubscribers(state))
            {
                await listener.AfterPlayerTurnStart(ctx, a);
                await listener.AfterPlayerTurnStart(ctx, b);
                await listener.AfterPlayerTurnStart(ctx, other);
                await listener.AfterCardPlayed(ctx, Play(Card<GoldenRoseCannon>(a)));
                await listener.AfterCardPlayed(ctx, Play(Card<GoldenRoseCannon>(other)));
                await listener.AfterDamageReceived(ctx, a.Creature, null!, default, null, null);
                await listener.AfterDeath(ctx, b.Creature, false, 0);
            }
            Require(Boundaries.Visuals.SequenceEqual(new[] {
                ("Refresh", (Creature?)a.Creature), ("Refresh", (Creature?)b.Creature),
                ("PlaySkillPose", (Creature?)a.Creature), ("PlayHurt", (Creature?)a.Creature), ("Refresh", (Creature?)b.Creature)
            }), "Visual routing depends on relic or targets another character");
        }
        Console.WriteLine("PASS visuals: native subscription, prewarm, both relics/no relic and multiplayer routing");
    }
}
