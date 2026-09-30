using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 软硬兼施(普通,1 费技能):给予所有敌人 1 层虚弱,再给予所有敌人 1 层易伤。消耗。
/// 升级:虚弱与易伤层数均提升至 2。
/// 全体施加走 combatState.HittableEnemies + PowerCmd.Apply 的多目标重载(vanilla BagOfMarbles 同款);
/// 「先虚再易」按描述顺序两次调用。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class CarrotAndStick : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<WeakPower>(1m),
        new PowerVar<VulnerablePower>(1m),
    };

    public CarrotAndStick()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState is not { } combatState)
        {
            return;
        }
        await PowerCmd.Apply<WeakPower>(choiceContext, combatState.HittableEnemies, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, combatState.HittableEnemies, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Weak.UpgradeValueBy(1m);
        base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
