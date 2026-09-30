using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 强制买断(稀有,2 费攻击):将消耗牌堆中所有的[闪耀摩拉]对一名敌人真·逐张打出。
/// 走 CardCmd.AutoPlay 完整打出管线:摩拉自身的伤害(含兜售枪火等加成)、每 3 张装填 1、
/// 众志成城/高价买入等「打出摩拉」联动全部照常触发;摩拉自带消耗,打完后回到消耗堆。
/// 每打出 1 张,获得 2 点格挡。升级:费用 1。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class ForcedBuyout : NaviaCardBase
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(2m, ValueProp.Move),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public ForcedBuyout()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        // 快照消耗堆中的摩拉后逐张真·打出(伤害/计数/联动全在摩拉自身的打出管线里结算)。
        List<ShiningMora> moras = CardPile.GetCards(base.Owner, PileType.Exhaust).OfType<ShiningMora>().ToList();
        foreach (ShiningMora mora in moras)
        {
            await CardCmd.AutoPlay(choiceContext, mora, cardPlay.Target);
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
