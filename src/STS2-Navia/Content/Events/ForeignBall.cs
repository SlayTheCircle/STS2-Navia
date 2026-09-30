using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NaviaMod.Content.Events;

/// <summary>
/// 异邦人的舞会(2 层事件,Hive)。
/// 共舞=变化 1 张(AromaOfChaos 的 LetGo 同款);晚宴=回 20 血 + 随机药水
/// (角色药池 + 共享药池随机,PotionCourier 的 Ransack 同款);赠礼=失 100 金 + 普通遗物,
/// 金币不足时选项锁死(RanwidTheElder 的 null handler 锁定范式)。
/// </summary>
[RegisterActEvent(typeof(Hive))]
public sealed class ForeignBall : NaviaEventBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new HealVar(20m),
        new DynamicVar("GiftGold", 100m),
    };

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://STS2-Navia/images/events/ForeignBall.png",
        BackgroundScenePath: "res://scenes/events/background_scenes/round_tea_party.tscn");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = new()
        {
            new EventOption(this, Dance, InitialOptionKey("DANCE"), HoverTipFactory.Static(StaticHoverTip.Transform)),
            new EventOption(this, Feast, InitialOptionKey("FEAST")),
        };
        if (base.Owner.Gold >= (int)base.DynamicVars["GiftGold"].BaseValue)
        {
            options.Add(new EventOption(this, Gift, InitialOptionKey("GIFT")));
        }
        else
        {
            options.Add(new EventOption(this, null, InitialOptionKey("GIFT_LOCKED")));
        }
        return options;
    }

    private async Task Dance()
    {
        CardModel? card = (await CardSelectCmd.FromDeckForTransformation(base.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))).FirstOrDefault();
        if (card != null)
        {
            await CardCmd.TransformToRandom(card, base.Rng, CardPreviewStyle.EventLayout);
        }
        SetEventFinished(PageDescription("DANCE"));
    }

    private async Task Feast()
    {
        await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
        IEnumerable<PotionModel> pool = base.Owner.Character.PotionPool.GetUnlockedPotions(base.Owner.UnlockState)
            .Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(base.Owner.UnlockState));
        PotionModel? potion = base.Owner.PlayerRng.Rewards.NextItem(pool);
        if (potion != null)
        {
            await RewardsCmd.OfferCustom(base.Owner,
                new List<Reward> { new PotionReward(potion.ToMutable(), base.Owner) });
        }
        SetEventFinished(PageDescription("FEAST"));
    }

    private async Task Gift()
    {
        await PlayerCmd.LoseGold(base.DynamicVars["GiftGold"].BaseValue, base.Owner, GoldLossType.Spent);
        RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner, RelicRarity.Common).ToMutable();
        await RelicCmd.Obtain(relic, base.Owner);
        SetEventFinished(PageDescription("GIFT"));
    }
}
