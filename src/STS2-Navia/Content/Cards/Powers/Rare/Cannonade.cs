using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 炮火连天(稀有,1 费能力,数值调整V1):每回合你第一次打出[金花礼炮]时,使接下来所有
/// [金花礼炮]的伤害次数+1。升级:获得固有。逻辑在 <see cref="CannonadePower"/>。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class Cannonade : NaviaCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public Cannonade()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<CannonadePower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级获得固有必须走 OnUpgrade→AddKeyword(手册 §6:条件式 CanonicalKeywords 会被缓存吞掉)。
        AddKeyword(CardKeyword.Innate);
    }
}
