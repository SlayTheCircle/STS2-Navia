using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 铳弹齐射(初始卡,1 费攻击,数值调整V4):造成 7 点伤害。消耗全部[装填],每消耗 1 层,伤害 +4。
/// 升级:伤害 10,每层 +5。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
[RegisterArchaicToothTranscendence(typeof(CannonRoar))] // 古老牙齿:铳弹齐射→枪炮轰鸣(vanilla Bash→Break 同型的先古化接线)
public sealed class VolleyFire : NaviaCardBase
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
        new DamageVar(7m, ValueProp.Move),
        // 预览感知三件套:显示值 = CalculationBase + ExtraDamage × multiplier;CalculatedDamageVar
        // 的 UpdateCardPreview 会过 Hook.ModifyDamage(力量/虚弱/附魔),面板与实际出伤一致。
        new CalculationBaseVar(7m),
        new ExtraDamageVar(4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner?.Creature?.GetPowerAmount<LoadPower>() ?? 0),
    };

    public VolleyFire()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        Creature creature = base.Owner.Creature;
        // 必须在消耗装填之前计算总伤:Calculate 实时重读装填层数,先消耗就会算成基准值。
        decimal total = base.DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        int load = creature.GetPowerAmount<LoadPower>();
        if (load > 0)
        {
            // 消耗装填统一走 LoadPower.Gain 负数入口(施工手册 §4.4)。
            await LoadPower.Gain(choiceContext, creature, -load, creature, this);
        }
        await DamageCmd.Attack(total).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
        base.DynamicVars["CalculationBase"].UpgradeValueBy(3m);
        base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}
