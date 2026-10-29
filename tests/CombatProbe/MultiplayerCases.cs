using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Powers;
using static Fixture;

internal static class MultiplayerCases
{
    public static async Task Run()
    {
        var selector = new TestCardSelector();
        using IDisposable selectorScope = CardSelectCmd.UseSelector(selector);
        TestMode.IsOn = true;
        Canonical<SupportBlockDrawOnPlay>();
        try
        {
            ConstraintsAndUpgrades();
            await PresidentCommissionAndSupport(selector);
            await PublicAccountWithdrawal(selector);
            await FirelineSupport();
            await GoldenRoseSalute(selector);
            await RosulaAlliance();
        }
        finally
        {
            TestMode.IsOn = false;
            Boundaries.ResetMultiplayer();
        }
    }

    private static void ConstraintsAndUpgrades()
    {
        var p = Player();
        CardModel[] cards =
        [
            Card<FirelineSupport>(p), Card<PresidentCommission>(p), Card<PublicAccountWithdrawal>(p),
            Card<GoldenRoseSalute>(p), Card<RosulaAlliance>(p),
        ];
        Require(cards.All(c => c.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly),
            "Every Mirror addition must be multiplayer-only");

        var fireline = (FirelineSupport)cards[0];
        fireline.UpgradeInternal();
        Require(fireline.DynamicVars.Damage.BaseValue == 11 && fireline.DynamicVars["MaxSpend"].BaseValue == 2 + 1
            && fireline.DynamicVars["BlockPerLoad"].BaseValue == 5, "Fireline upgrade values differ from Mirror");

        var president = (PresidentCommission)cards[1];
        president.UpgradeInternal();
        Require(president.EnergyCost.GetAmountToSpend() == 0 && president.DynamicVars["SupportTier"].BaseValue == 2,
            "President Commission upgrade values differ from Mirror");

        var withdrawal = (PublicAccountWithdrawal)cards[2];
        withdrawal.UpgradeInternal();
        Require(withdrawal.Keywords.Contains(CardKeyword.Retain), "Public Account Withdrawal should gain Retain");

        var salute = (GoldenRoseSalute)cards[3];
        Require(salute.TargetType == TargetType.AnyEnemy, "Base Salute should target an enemy");
        salute.UpgradeInternal();
        Require(salute.DynamicVars["Salvo"].BaseValue == 4 && salute.TargetType == TargetType.RandomEnemy,
            "Upgraded Salute should use Salvo 4 and a random enemy");

        var alliance = (RosulaAlliance)cards[4];
        alliance.UpgradeInternal();
        Require(alliance.DynamicVars["RosulaAlliancePower"].BaseValue == 2, "Rosula Alliance should draw two when upgraded");
        Console.WriteLine("PASS multiplayer constraints: five cards and all Mirror upgrade values");
    }

    private static SupportBlockDrawOnPlay Enchant(CardModel card, int tier)
    {
        var enchant = (SupportBlockDrawOnPlay)Canonical<SupportBlockDrawOnPlay>().ToMutable();
        card.EnchantInternal(enchant, tier);
        enchant.ModifyCard();
        return enchant;
    }

    private static async Task PresidentCommissionAndSupport(TestCardSelector selector)
    {
        Boundaries.ResetMultiplayer();
        var a = Player(); var b = Player(); var dead = Player();
        Combat(a, b, dead);
        dead.Creature.SetCurrentHpInternal(0);
        var commission = Card<PresidentCommission>(a);
        var host = Card<StrikeNavia>(b);
        selector.PrepareToSelect([host]);
        var play = Play(commission, target: b.Creature);
        var ctx = new ThrowingPlayerChoiceContext();
        await OnPlay(commission, ctx, play);
        Require(ReferenceEquals(host.Owner, b) && host.Enchantment is SupportBlockDrawOnPlay,
            "Commission must enchant the ally's own card without transferring ownership");
        var enchant = (SupportBlockDrawOnPlay)host.Enchantment!;
        Require(enchant.DynamicVars.Block.BaseValue == 4 && enchant.DynamicVars.Cards.BaseValue == 1,
            "Base Commission support should be 4 Block and 1 draw");
        await enchant.OnPlay(ctx, Play(host));
        Require(a.Creature.Block == 4 && b.Creature.Block == 4 && dead.Creature.Block == 0,
            "Commission support must affect every living player including its owner");
        Require(Boundaries.Draws.Count == 2 && Boundaries.Draws.Contains(a) && Boundaries.Draws.Contains(b)
            && Boundaries.DrawAmounts.All(v => v == 1), "Commission support draws for the wrong players");
        var upgraded = Enchant(Card<StrikeNavia>(b), 2);
        Require(upgraded.DynamicVars.Block.BaseValue == 7 && upgraded.DynamicVars.Cards.BaseValue == 2,
            "Upgraded Commission support should be 7 Block and 2 draw");

        Boundaries.ResetMultiplayer();
        var selfHost = Card<StrikeNavia>(a);
        selector.PrepareToSelect([selfHost]);
        var selfPlay = Play(commission, target: a.Creature);
        await OnPlay(commission, ctx, selfPlay);
        Require(selfHost.Enchantment == null && Boundaries.Draws.Count == 0 && a.Creature.Block == 4,
            "Commission must reject a self target outside the normal UI");
        var deadHost = Card<StrikeNavia>(dead);
        selector.PrepareToSelect([deadHost]);
        await OnPlay(commission, ctx, Play(commission, target: dead.Creature));
        Require(deadHost.Enchantment == null, "Commission must reject a dead ally target outside the normal UI");
        Console.WriteLine("PASS president commission: ally ownership, all-player effect, upgrade tiers and dead/self guards");
    }

    private static async Task PublicAccountWithdrawal(TestCardSelector selector)
    {
        Boundaries.ResetMultiplayer();
        selector.Cleanup();
        var a = Player(); var b = Player(); var dead = Player(); Combat(a, b, dead);
        dead.Creature.SetCurrentHpInternal(0);
        var card = Card<PublicAccountWithdrawal>(a);
        var mora1 = Card<ShiningMora>(a); var mora2 = Card<ShiningMora>(a);
        var ctx = new ThrowingPlayerChoiceContext();
        var play = Play(card, target: b.Creature);
        selector.PrepareToSelect(Array.Empty<CardModel>());
        await OnPlay(card, ctx, play);
        Require(b.PlayerCombatState!.Energy == 0 && Boundaries.ExhaustedCards.Count == 0,
            "Zero Mora selection should be a no-op");
        selector.PrepareToSelect([mora1, mora2]);
        await OnPlay(card, ctx, play);
        Require(b.PlayerCombatState!.Energy == 2 && a.PlayerCombatState!.Energy == 0
            && Boundaries.ExhaustedCards.SequenceEqual(new CardModel[] { mora1, mora2 }),
            "Each exhausted Mora should give exactly one energy to the selected ally");
        Boundaries.ResetMultiplayer();
        selector.PrepareToSelect([mora1]);
        await OnPlay(card, ctx, Play(card, target: a.Creature));
        await OnPlay(card, ctx, Play(card, target: dead.Creature));
        Require(a.PlayerCombatState!.Energy == 0 && b.PlayerCombatState!.Energy == 2
            && dead.PlayerCombatState!.Energy == 0 && Boundaries.ExhaustedCards.Count == 0,
            "Public Account must reject self and dead ally targets without exhausting Mora or granting energy");
        selector.Cleanup();
        Console.WriteLine("PASS public account: optional zero/two Mora, exact exhaust set, ally-only energy and dead/self guards");
    }

    private static async Task FirelineSupport()
    {
        Boundaries.ResetMultiplayer();
        var a = Player(); var b = Player(); var dead = Player(); Combat(a, b, dead);
        dead.Creature.SetCurrentHpInternal(0);
        Power<LoadPower>(a, 5);
        var card = Card<FirelineSupport>(a);
        var play = Play(card, target: b.Creature);
        await OnPlay(card, new ThrowingPlayerChoiceContext(), play);
        Require(a.Creature.GetPowerAmount<LoadPower>() == 3, "Fireline should consume min(current Load, two)");
        Require(a.Creature.Block == 0 && b.Creature.Block == 10 && dead.Creature.Block == 0,
            "Fireline should grant five Block per spent Load only to other living players");
        Console.WriteLine("PASS fireline: Load cap, per-stack Block and self/dead filtering");
    }

    private static async Task GoldenRoseSalute(TestCardSelector selector)
    {
        Boundaries.ResetMultiplayer();
        var a = Player(); var b = Player(); var dead = Player(); Combat(a, b, dead);
        dead.Creature.SetCurrentHpInternal(0);
        var card = Card<GoldenRoseSalute>(a);
        var cannon = Card<GoldenRoseCannon>(a);
        var unrelated = Card<GoldenRoseCannon>(a);
        selector.PrepareToSelect([cannon]);
        var play = Play(card, target: b.Creature);
        var ctx = new ThrowingPlayerChoiceContext();
        Boundaries.AutoPlayEffect = (played, target) =>
        {
            foreach (int amount in new[] { 4, 3 })
            {
                var result = new DamageResult(b.Creature, ValueProp.Move) { UnblockedDamage = amount, OverkillDamage = 99 };
                card.AfterDamageGiven(ctx, a.Creature, result, ValueProp.Move, b.Creature, played).GetAwaiter().GetResult();
            }
            var other = new DamageResult(b.Creature, ValueProp.Move) { UnblockedDamage = 100 };
            card.AfterDamageGiven(ctx, a.Creature, other, ValueProp.Move, b.Creature, unrelated).GetAwaiter().GetResult();
        };
        await OnPlay(card, ctx, play);
        Require(cannon.DynamicVars.Damage.BaseValue == 4, "Salute should apply Salvo before playing the selected cannon");
        Require(Boundaries.AutoPlays.SequenceEqual([(cannon, b.Creature)]), "Salute should AutoPlay exactly the selected cannon at its target");
        Require(a.Creature.Block == 0 && b.Creature.Block == 7 && dead.Creature.Block == 0,
            "Salute should sum only the selected cannon's unblocked damage, excluding overkill and unrelated hits");
        Require(typeof(GoldenRoseSalute).GetField("_playedCannon", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(card) == null,
            "Salute must clear its damage-collection marker after AutoPlay");
        Console.WriteLine("PASS golden salute: Salvo order, selected-cannon damage sum, overkill rejection and cleanup");
    }

    private static async Task RosulaAlliance()
    {
        Boundaries.ResetMultiplayer();
        var a = Player(); var b = Player(); var dead = Player(); Combat(a, b, dead);
        dead.Creature.SetCurrentHpInternal(0);
        var power = Power<RosulaAlliancePower>(a, 2);
        var other = Card<StrikeNavia>(b); Enchant(other, 1);
        var first = Card<StrikeNavia>(a); Enchant(first, 1);
        var nested = Card<StrikeNavia>(a, PileType.Play); Enchant(nested, 1);
        var ctx = new ThrowingPlayerChoiceContext();
        var otherPlay = Play(other);
        await power.BeforeCardPlayed(otherPlay); await power.AfterCardPlayed(ctx, otherPlay);
        Require(Boundaries.Draws.Count == 0, "Alliance must ignore another player's Support card");
        var firstPlay = Play(first); var nestedPlay = Play(nested, auto: true);
        await power.BeforeCardPlayed(firstPlay);
        await power.BeforeCardPlayed(nestedPlay);
        await power.AfterCardPlayed(ctx, nestedPlay);
        Require(Boundaries.Draws.Count == 0, "Nested Support play steals the alliance's first-play eligibility");
        await power.AfterCardPlayed(ctx, firstPlay);
        await power.AfterCardPlayed(ctx, firstPlay);
        Require(Boundaries.Draws.SequenceEqual([b]) && Boundaries.DrawAmounts.SequenceEqual([2m]),
            "Alliance should resolve once, for other living players only, at its stacked amount");
        await power.BeforeSideTurnStart(ctx, CombatSide.Player, [a.Creature], null!);
        await power.BeforeCardPlayed(nestedPlay); await power.AfterCardPlayed(ctx, nestedPlay);
        Require(Boundaries.Draws.SequenceEqual([b, b]) && Boundaries.DrawAmounts.All(v => v == 2),
            "Alliance should reset next turn and count autoplay");
        Console.WriteLine("PASS rosula alliance: owner guard, nested identity, no repeat, turn reset and autoplay");
    }
}
