using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 炮身加固(罕见,1 费能力,数值调整V1):你的[金花礼炮]每对 1 个敌人造成 1 次伤害,获得 3 点格挡。
/// 升级:每次命中格挡 3→4。逻辑在 <see cref="ReinforcedBarrelPower"/>(本场战斗持续)。
/// 稀有度保持罕见(蓝)——设计稿「罕见」即 Uncommon,此前批次的「对齐金卡」为术语误读,已改回。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class ReinforcedBarrel : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ReinforcedBarrelPower>(3m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public ReinforcedBarrel()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<ReinforcedBarrelPower>(choiceContext, base.Owner.Creature, base.DynamicVars["ReinforcedBarrelPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["ReinforcedBarrelPower"].UpgradeValueBy(1m);
    }
}
