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
/// 危险改装(稀有,2 费能力):装填 6;每回合开始时失去 1 层[装填]。
/// 升级:费用降为 1(数值不变)。回合衰减逻辑在 <see cref="DangerousRetrofitPower"/>。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class DangerousRetrofit : NaviaCardBase
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
        new PowerVar<LoadPower>(6m),
    };

    public DangerousRetrofit()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await LoadPower.Gain(choiceContext, base.Owner.Creature, (int)base.DynamicVars["LoadPower"].BaseValue, base.Owner.Creature, this);
        // 「每回合开始时失去 1 层」由隐藏标记承担;重复打出时标记叠层合并,仍只失去 1 层/回合。
        await PowerCmd.Apply<DangerousRetrofitPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级:2 费 → 1 费(vanilla 降费写法)。
        base.EnergyCost.UpgradeBy(-1);
    }
}
