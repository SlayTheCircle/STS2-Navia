using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 指挥形态(稀有,3 费能力):每有 1 张你的牌被消耗,装填 1。
/// 升级:费用降为 2(数值不变)。逻辑在 <see cref="CommandStancePower"/>。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class CommandStance : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<CommandStancePower>(1m),
    };

    public CommandStance()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<CommandStancePower>(choiceContext, base.Owner.Creature, base.DynamicVars["CommandStancePower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级:3 费 → 2 费(vanilla 降费写法)。
        base.EnergyCost.UpgradeBy(-1);
    }
}
