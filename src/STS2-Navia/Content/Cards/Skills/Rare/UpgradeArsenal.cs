using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 鸟枪换炮(稀有,1 费技能,数值调整V1):消耗手牌中所有闪耀摩拉(真正移除,送入消耗堆),
/// 每消耗 1 张获得 1 点能量。升级:费用 0。
/// 注意:这里的「消耗」走 <c>CardCmd.Exhaust</c> 移动到消耗堆,与摩拉被打出后的自然消耗无关,
/// 不会触发 <see cref="MoraCounterPower"/> 计数。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class UpgradeArsenal : NaviaCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public UpgradeArsenal()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先取快照再逐张移除:Exhaust 会把手牌实例挪进消耗堆,边遍历边移除会破坏集合。
        List<ShiningMora> moras = CardPile.GetCards(base.Owner, PileType.Hand).OfType<ShiningMora>().ToList();
        int count = moras.Count;
        foreach (ShiningMora mora in moras)
        {
            await CardCmd.Exhaust(choiceContext, mora);
        }
        if (count > 0)
        {
            await PlayerCmd.GainEnergy(count, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
