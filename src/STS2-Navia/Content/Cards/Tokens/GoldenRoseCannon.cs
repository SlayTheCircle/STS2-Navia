using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models.CardPools;
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 金花礼炮(token,无色,0 费——数值调整V1):造成 2 点伤害 3 次。消耗。
/// 由「礼炮轰鸣」在手牌无炮时生成——从 2 伤白板起算(轰鸣=即时收益,设计者勘误 2026-09-30),
/// 仅炮火连天的次数累计随生成应用(见 <see cref="CreateInHand"/>)。
/// 「饱和爆破」在场时全体结算(见 OnPlay 分支);「武器保养」的去消耗礼炮另行生成,本类无需参与。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class GoldenRoseCannon : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(2m, ValueProp.Move),
        // 命中次数做成变量:火力覆盖等卡直接抬升手牌内实例的 Hits,无需改动本类
        new DynamicVar("Hits", 3m),
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    /// <summary>「饱和爆破」在场时免选目标(vanilla Shiv/FanOfKnives 同款:TargetType 虚属性按 Power 存在性
    /// 动态返回,AllEnemies 即无单选提示;canonical 实例/无主时回落单体口径)。</summary>
    public override TargetType TargetType
    {
        get
        {
            if (!HasSaturationBlast)
            {
                return TargetType.AnyEnemy;
            }
            return TargetType.AllEnemies;
        }
    }

    private bool HasSaturationBlast
    {
        get
        {
            if (base.IsMutable && base.Owner != null)
            {
                return base.Owner.Creature.HasPower<SaturationBlastPower>();
            }
            return false;
        }
    }

    public GoldenRoseCannon()
        : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hits = (int)base.DynamicVars["Hits"].BaseValue;
        // 「饱和爆破」在场:全体结算(每敌各吃每段伤害);TargetType 已动态转 AllEnemies,免选目标。
        if (HasSaturationBlast)
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this, cardPlay)
                .TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            return;
        }
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public static async Task<GoldenRoseCannon?> CreateInHand(Player owner, ICombatState combatState)
    {
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return null;
        }
        GoldenRoseCannon cannon = combatState.CreateCard<GoldenRoseCannon>(owner);
        // 数值调整V1 勘误:新生成的礼炮从 2 伤白板起算——礼炮轰鸣是即时收益,只抬触发时已存在的实例;
        // 伤害不从基础值起算的持续成长只有炮火连天的次数(「接下来所有」),此处只应用次数累计。
        cannon.DynamicVars["Hits"].BaseValue += Salvo.HitsBonus(owner);
        await CardPileCmd.AddGeneratedCardToCombat(cannon, PileType.Hand, owner);
        return cannon;
    }
}
