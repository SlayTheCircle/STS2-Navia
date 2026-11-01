using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.TestSupport;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Powers;
using static Fixture;

// 消耗装填的攻击必须保留卡面已展示的装填力量等价；其余修正仍走原版管线一次。
internal static class VolleyFireCases
{
    public static async Task Run()
    {
        Boundaries.ExecuteAttacks = true;
        TestMode.IsOn = true;
        try
        {
            await Case<VolleyFire>("Volley Fire / 0 load", 0, 7m, 4m);
            await Case<VolleyFire>("Volley Fire / 1 load", 1, 7m, 4m);
            await Case<VolleyFire>("Volley Fire / 2 load", 2, 7m, 4m);
            await Case<VolleyFire>("Volley Fire / 6 load", 6, 7m, 4m);
            await Case<VolleyFire>("Volley Fire upgraded / 6 load", 6, 10m, 5m, upgrade: true);
            await Case<VolleyFire>("Volley Fire / strength and vulnerable", 6, 7m, 4m, strength: 2, vulnerable: true);
            await Case<VolleyFire>("Volley Fire / multiplicative enchantment", 6, 7m, 4m, corrupted: true);
            await Case<CannonRoar>("Cannon Roar / 6 load", 6, 12m, 7m);
        }
        finally
        {
            Boundaries.ExecuteAttacks = false;
            Boundaries.ObservedHpLossTarget = null;
            Boundaries.ObservedHpLoss = null;
            TestMode.IsOn = false;
        }
        Console.WriteLine("PASS volley fire: consumed-load damage matches previews, including upgrades, hooks and enchantments");
    }

    private static async Task Case<T>(string name, int load, decimal baseDamage, decimal perLoad,
        bool upgrade = false, int strength = 0, bool vulnerable = false, bool corrupted = false) where T : CardModel, new()
    {
        var player = Player();
        var targetPlayer = Player();
        PrepareHooks(player);
        PrepareHooks(targetPlayer);
        Combat(player, targetPlayer);
        var card = Card<T>(player);
        if (upgrade)
        {
            card.UpgradeInternal();
        }
        if (load > 0)
        {
            Power<LoadPower>(player, load);
        }
        if (strength > 0)
        {
            Power<StrengthPower>(player, strength);
        }
        if (vulnerable)
        {
            Power<VulnerablePower>(targetPlayer);
        }
        if (corrupted)
        {
            var enchant = (Corrupted)Canonical<Corrupted>().ToMutable();
            card.EnchantInternal(enchant, 1);
            enchant.ModifyCard();
        }

        Creature target = targetPlayer.Creature;
        card.UpdateDynamicVarPreview(CardPreviewMode.Normal, target, card.DynamicVars);
        decimal preview = card.DynamicVars.CalculatedDamage.PreviewValue;
        decimal expected = ((baseDamage + perLoad * load) * (corrupted ? 1.5m : 1m)
            + Math.Floor(load / LoadPower.StacksPerDexterity) + strength) * (vulnerable ? 1.5m : 1m);

        Boundaries.ObservedHpLossTarget = target;
        Boundaries.ObservedHpLoss = null;
        try
        {
            await OnPlay(card, new ThrowingPlayerChoiceContext(), Play(card, target: target));
            throw new InvalidOperationException($"{name}: attack did not reach the HP-loss boundary");
        }
        catch (HpLossObservedException)
        {
            // 在真实伤害管线给出最终扣血值后停止，避免启动 Godot 视觉与历史场景。
        }
        finally
        {
            Boundaries.ObservedHpLossTarget = null;
        }

        Require(Boundaries.ObservedHpLoss.HasValue, $"{name}: no actual damage observed");
        decimal actual = Boundaries.ObservedHpLoss ?? throw new InvalidOperationException($"{name}: no actual damage observed");
        Require(preview == expected, $"{name}: preview {preview}, expected {expected}");
        Require(actual == preview, $"{name}: preview {preview}, actual {actual}");
        Require(player.Creature.GetPowerAmount<LoadPower>() == 0, $"{name}: did not consume all load");
        Console.WriteLine($"PASS {name}: preview=actual={actual}");
    }

    private static void PrepareHooks(Player player)
    {
        Field(player, "<IsActiveForHooks>k__BackingField", true);
        Field(player, "_potionSlots", new List<PotionModel?>());
        Field(player.PlayerCombatState!, "<OrbQueue>k__BackingField", new OrbQueue(player));
    }
}
