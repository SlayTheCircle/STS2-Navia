using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.Powers;
using static Fixture;

internal static class CombatCases
{
    public static async Task Run()
    {
        Boundaries.Install();
        await Inflation();
        await Cannonade();
        await VisualCases.Run();
    }

    private static async Task Inflation()
    {
        var a = Player(); var b = Player();
        var canonical = new InflationPower();
        var pa = (InflationPower)canonical.ToMutable(); pa.ApplyInternal(a.Creature, 1);
        var pb = (InflationPower)canonical.ToMutable(); pb.ApplyInternal(b.Creature, 1);
        var set = typeof(InflationPower).GetField("_adjusted", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Require(!ReferenceEquals(set.GetValue(pa), set.GetValue(pb)) && !ReferenceEquals(set.GetValue(pa), set.GetValue(canonical)), "Inflation clones share records");
        var old = Card<ShiningMora>(a);
        var fresh = Card<ShiningMora>(a);
        await pa.AfterCardEnteredCombat(fresh);
        await pa.AfterCardEnteredCombat(fresh);
        await pb.AfterCardEnteredCombat(fresh);
        Require(fresh.DynamicVars.Damage.BaseValue == 2 && old.DynamicVars.Damage.BaseValue == 3, "Inflation changes old cards or applies damage twice");
        var ctx = new ThrowingPlayerChoiceContext();
        await pa.AfterCardPlayed(ctx, Play(old));
        await pa.AfterCardPlayed(ctx, Play(fresh));
        await pb.AfterCardPlayed(ctx, Play(fresh));
        Require(Boundaries.Draws.SequenceEqual(new[] { a }), "Inflation draws for another player or old mora");
        // 就算牌转移给队友，记录本身也不能绕过出牌者归属检查。
        typeof(CardModel).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(fresh, b);
        await pa.AfterCardPlayed(ctx, Play(fresh));
        Require(Boundaries.Draws.Count == 1, "Transferred mora triggers original owner's draw");
        var next = (InflationPower)canonical.ToMutable(); next.ApplyInternal(Player().Creature, 1);
        Require(((HashSet<CardModel>)set.GetValue(next)!).Count == 0, "New combat inherits mora records");
        Console.WriteLine("PASS inflation: clone isolation, owner guard, old/new mora and repeated entry");
    }

    private static async Task Cannonade()
    {
        var p = Player(); var power = Power<CannonadePower>(p);
        var cards = new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust, PileType.Play }
            .Select(pile => Card<GoldenRoseCannon>(p, pile)).ToArray();
        Require(Salvo.HitsBonus(p) == 0 && cards.All(c => c.DynamicVars["Hits"].BaseValue == 3), "Cannonade grants hits on application");
        var ctx = new ThrowingPlayerChoiceContext();
        var first = Play(cards[0]); var nested = Play(cards[4], auto: true);
        await power.BeforeCardPlayed(first);
        await power.BeforeCardPlayed(nested);
        await power.AfterCardPlayed(ctx, nested);
        Require(Salvo.HitsBonus(p) == 0, "Nested cannon steals first trigger");
        await power.AfterCardPlayed(ctx, first);
        await power.AfterCardPlayed(ctx, first);
        Require(Salvo.HitsBonus(p) == 1 && cards.All(c => c.DynamicVars["Hits"].BaseValue == 4), "First cannon should add exactly one hit to every pile");
        power.SetAmount(2);
        Require(Salvo.HitsBonus(p) == 1, "Additional ability changes accumulated hits");
        var later = Play(cards[0]);
        await power.BeforeCardPlayed(later); await power.AfterCardPlayed(ctx, later);
        Require(Salvo.HitsBonus(p) == 1, "Second play re-triggers in same turn");
        await power.BeforeSideTurnStart(ctx, CombatSide.Player, new[] { p.Creature }, null!);
        var auto = Play(cards[4], auto: true);
        await power.BeforeCardPlayed(auto); await power.AfterCardPlayed(ctx, auto);
        Require(power.Amount == 2 && Salvo.HitsBonus(p) == 3 && cards.All(c => c.DynamicVars["Hits"].BaseValue == 6), "Two abilities should add two hits next turn");
        var generated = await GoldenRoseCannon.CreateInHand(p, new CombatState());
        Require(generated?.DynamicVars["Hits"].BaseValue == 6, "New cannon does not inherit accumulated bonus");
        Require(Power<CannonadePower>(Player()).HitsBonus == 0, "Cannonade leaks cumulative hits to next combat");
        Console.WriteLine("PASS cannonade: zero initial bonus, every pile, nested/auto/repeated plays and stacking");
    }
}
