using System.Reflection;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using NaviaMod.Content.Characters;

// 只组装战斗所需的真实玩家/牌堆；不启动存档、网络或 Godot 场景。
internal static class Fixture
{
    public static void Field(object instance, string name, object? value) =>
        instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(instance, value);

    public static Player Player(CharacterModel? character = null)
    {
        var player = (Player)RuntimeHelpers.GetUninitializedObject(typeof(Player));
        Field(player, "<Character>k__BackingField", character ?? new Navia());
        Field(player, "<Creature>k__BackingField", new Creature(player, 75, 75));
        Field(player, "_runPiles", Array.Empty<CardPile>());
        Field(player, "_relics", new List<RelicModel>());
        var state = (PlayerCombatState)RuntimeHelpers.GetUninitializedObject(typeof(PlayerCombatState));
        foreach (var (name, type) in new[] { ("Hand", PileType.Hand), ("DrawPile", PileType.Draw),
                     ("DiscardPile", PileType.Discard), ("ExhaustPile", PileType.Exhaust), ("PlayPile", PileType.Play) })
            Field(state, $"<{name}>k__BackingField", new CardPile(type));
        Field(player, "<PlayerCombatState>k__BackingField", state);
        return player;
    }

    public static T Canonical<T>() where T : AbstractModel, new()
    {
        ModelDb.Inject(typeof(T));
        return ModelDb.GetById<T>(ModelDb.GetId(typeof(T)));
    }

    public static T Power<T>(Player player, int amount = 1) where T : PowerModel, new()
    {
        var power = (T)Canonical<T>().ToMutable();
        power.ApplyInternal(player.Creature, amount);
        return power;
    }

    public static T Card<T>(Player player, PileType pile = PileType.Hand) where T : CardModel, new()
    {
        var card = (T)Canonical<T>().ToMutable();
        card.Owner = player;
        AddToPile(card, pile);
        return card;
    }

    public static void AddToPile(CardModel card, PileType pile)
    {
        // 静默填充真实牌堆，绕开 UI 和 StateTracker。
        ((List<CardModel>)typeof(CardPile).GetField("_cards", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(CardPile.Get(pile, card.Owner))!).Add(card);
    }

    public static CardPlay Play(CardModel card, bool auto = false) => new()
    {
        Card = card,
#if !NAVIA_GAME_0107_1
        Player = card.Owner,
#endif
        Target = null, ResultPile = PileType.Exhaust, Resources = default!, IsAutoPlay = auto,
        PlayIndex = 0, PlayCount = 1,
    };

    public static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
