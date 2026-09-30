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
/// 生财有道(罕见,1 费能力):每回合,你第一次打出[闪耀摩拉]时,将一张它的带有虚无的复制品
/// 置入弃牌堆。升级:获得保留(OnUpgrade→AddKeyword,手册 §6 铁律)。逻辑在 <see cref="GoldenTouchPower"/>。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class GoldenTouch : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public GoldenTouch()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<GoldenTouchPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
