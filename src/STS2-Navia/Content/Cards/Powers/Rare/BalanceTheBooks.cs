using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 亏空平账(稀有,1 费能力):每当你获得负面状态时,在你的抽牌堆中生成 2 张[闪耀摩拉]。
/// 升级:费用 1→0。逻辑在 <see cref="BalanceTheBooksPower"/>(AfterPowerAmountChanged 判
/// power.Type == Debuff 的正向增量——敌人施加与自发负面状态均触发)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class BalanceTheBooks : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public BalanceTheBooks()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<BalanceTheBooksPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
