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
/// 高压弹膛(稀有,1 费能力,虚无):每当你消耗 1 层[装填],对所有敌人造成 3 点伤害。
/// 升级:移除虚无(伤害不变)。逻辑在 <see cref="HighPressureChamberPower"/>。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class HighPressureChamber : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword> { CardKeyword.Ethereal };
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<HighPressureChamberPower>(3m),
    };

    public HighPressureChamber()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<HighPressureChamberPower>(choiceContext, base.Owner.Creature, base.DynamicVars["HighPressureChamberPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级仅移除虚无(vanilla Apparition 同款写法);每层伤害保持 3。
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
