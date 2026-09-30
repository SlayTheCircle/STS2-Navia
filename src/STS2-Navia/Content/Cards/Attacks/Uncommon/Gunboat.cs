using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 坚船利炮(罕见,X 费攻击):对所有敌人造成 7 点伤害 X 次。若已有[装填],这张卡的伤害值
/// 提升等量的值(当前每有 1 层装填伤害 +1,不消耗装填)。升级:伤害 7→9。
/// X 费=vanilla Whirlwind/Eradicate 范式(<c>HasEnergyCostX</c> + 构造费 0 + <c>ResolveEnergyXValue</c>)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class Gunboat : NaviaCardBase
{
    protected override bool HasEnergyCostX => true;

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
        new DamageVar(7m, ValueProp.Move),
        // 预览感知三件套:显示值 = CalculationBase + ExtraDamage × 当前装填层数(不消耗装填,
        // 无需「先算后耗」,但实际伤害仍必须在攻击执行前用 Calculate 求值);CalculatedDamageVar
        // 的面板会过 Hook.ModifyDamage(力量/虚弱/附魔),与实际出伤一致。
        new CalculationBaseVar(7m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner?.Creature?.GetPowerAmount<LoadPower>() ?? 0),
    };

    public Gunboat()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 全体敌人没有单一目标,Calculate 传 null(与面板预览同口径,技能只影响出伤方)。
        decimal total = base.DynamicVars.CalculatedDamage.Calculate(null);
        await DamageCmd.Attack(total).WithHitCount(ResolveEnergyXValue()).FromCard(this, cardPlay)
            .TargetingAllOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 三件套卡升级:Damage 与 CalculationBase 同步 +2;「每层 +1」不变(手册 §6)。
        base.DynamicVars.Damage.UpgradeValueBy(2m);
        base.DynamicVars.CalculationBase.UpgradeValueBy(2m);
    }
}
