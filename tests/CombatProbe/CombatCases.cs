using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
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
        await LoadV4();
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

    // 数值调整V4:力量等价、可叠加上限与危险改装重做的回合语义。
    // PowerCmd 链会经 CombatState.Wait 等待——测试模式下退化为 Task.Delay,避免裸进程的原生 Godot 调用。
    private static async Task LoadV4()
    {
        TestMode.IsOn = true;
        try
        {
            // 力量等价:vanilla StrengthPower 同款 dealer/props 口径,每 2 层 +1 向下取整。
            var p = Player();
            var load = Power<LoadPower>(p, 6);
            var other = Player().Creature;
            decimal Strength(decimal amount, ValueProp props, Creature dealer) =>
#if !NAVIA_GAME_0107_1
                load.ModifyDamageAdditive(null!, amount, props, dealer, null, null);
#else
                load.ModifyDamageAdditive(null!, amount, props, dealer, null);
#endif
            Require(Strength(6m, ValueProp.Move, p.Creature) == 3m, "Load strength bonus should be floor(stacks/2)");
            Require(Strength(6m, ValueProp.Move, other) == 0m, "Load strength must not apply to another dealer");
            Require(Strength(6m, ValueProp.Unblockable, p.Creature) == 0m, "Load strength must not apply to non-attack damage");
            load.SetAmount(5);
            Require(Strength(5m, ValueProp.Move, p.Creature) == 2m, "Odd stacks should floor");

            // 可叠加上限:6 + 3×标记层数;Gain 按该上限截断。第二份膛线在游戏内经由
            // ModifyAmount 叠到同一实例,这里直接 SetAmount 模拟叠层后的状态。
            var q = Player();
            q.Creature.CombatState = new CombatState();
            Require(LoadPower.CapFor(q.Creature) == 6, "Default cap should be 6");
            var cap = Power<LoadCapUpPower>(q, 1);
            Require(LoadPower.CapFor(q.Creature) == 9, "First Rifled Barrel should raise cap to 9");
            cap.SetAmount(2);
            Require(LoadPower.CapFor(q.Creature) == 12, "Stacking cap should reach 12 with two marks");
            await LoadPower.Gain(new ThrowingPlayerChoiceContext(), q.Creature, 20, q.Creature, null);
            Require(q.Creature.GetPowerAmount<LoadPower>() == 12, "Gain should clamp to the stacked cap");

            // 危险改装:前 3 回合先失去再补满(补满标记在场时永续流失让位),此后永续流失 1 层。
            var r = Player();
            r.Creature.CombatState = new CombatState();
            var fill = Power<DangerousRetrofitFillPower>(r, 3);
            var drain = Power<DangerousRetrofitPower>(r, 1);
            await LoadPower.Gain(new ThrowingPlayerChoiceContext(), r.Creature, 6, r.Creature, null);
            var ctx = new ThrowingPlayerChoiceContext();
            Creature[] parts = [r.Creature];
            await fill.BeforeSideTurnStart(ctx, CombatSide.Player, parts, null!);
            Require(r.Creature.GetPowerAmount<LoadPower>() == 6 && fill.Amount == 2, "Fill turn at cap should stay at cap and count down");
            await drain.BeforeSideTurnStart(ctx, CombatSide.Player, parts, null!);
            Require(r.Creature.GetPowerAmount<LoadPower>() == 6, "Permanent drain must not double-dip while fill is active");
            await LoadPower.Gain(ctx, r.Creature, -3, r.Creature, null);
            await fill.BeforeSideTurnStart(ctx, CombatSide.Player, parts, null!);
            Require(r.Creature.GetPowerAmount<LoadPower>() == 6 && fill.Amount == 1, "Partial load should lose 1 then refill to cap");
            await fill.BeforeSideTurnStart(ctx, CombatSide.Player, parts, null!);
            Require(!r.Creature.HasPower<DangerousRetrofitFillPower>(), "Fill marker should remove itself at zero");
            await LoadPower.Gain(ctx, r.Creature, -2, r.Creature, null);
            await drain.BeforeSideTurnStart(ctx, CombatSide.Player, parts, null!);
            Require(r.Creature.GetPowerAmount<LoadPower>() == 3, "Permanent drain should take over after fill expires");
            Console.WriteLine("PASS loadV4: strength equivalence, stacking cap and retrofit turn flow");
        }
        finally
        {
            TestMode.IsOn = false;
        }
    }
}
