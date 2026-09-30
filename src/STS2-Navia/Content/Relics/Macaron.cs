using System.Collections.Generic;
using System.Threading.Tasks;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.RelicPools;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 美味的马卡龙(Common,局外遗物):拾起时,将你的最大生命值提升 2。
/// 每当你将 3 张卡加入你的卡组,提升 1 点最大生命值。
/// 拾起时点走 AfterObtained + HasUponPickupEffect(vanilla Pear 同款);
/// 「将卡加入卡组」监听 AfterCardChangedPiles,只认进入主牌库(PileType.Deck)的本方卡,
/// 卡牌奖励/事件/商店加卡都会经过 CardPileCmd.Add 派发本钩子(vanilla DarkstonePeriapt 同款);
/// 计数用 SavedProperty 持久化(vanilla HappyFlower 同款,不存档会丢进度),计数器显示进度(每 3 张)。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
public sealed class Macaron : NaviaRelicBase
{
    private const int CardsPerBonus = 3;

    private const decimal MaxHpOnPickup = 2m;

    private const decimal MaxHpPerBonus = 1m;

    private int _cardsAddedToDeck;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool HasUponPickupEffect => true;

    public override bool ShowCounter => true;

    public override int DisplayAmount => CardsAddedToDeck % CardsPerBonus;

    [SavedProperty]
    public int CardsAddedToDeck
    {
        get => _cardsAddedToDeck;
        set
        {
            AssertMutable();
            _cardsAddedToDeck = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterObtained()
    {
        Flash();
        await CreatureCmd.GainMaxHp(base.Owner.Creature, MaxHpOnPickup);
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        CardPile? pile = card.Pile;
        if (pile == null || pile.Type != PileType.Deck || card.Owner != base.Owner)
        {
            return;
        }
        CardsAddedToDeck++;
        if (CardsAddedToDeck % CardsPerBonus == 0)
        {
            Flash();
            await CreatureCmd.GainMaxHp(base.Owner.Creature, MaxHpPerBonus);
        }
    }
}
