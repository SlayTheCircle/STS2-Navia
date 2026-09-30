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
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 乘胜追击(罕见,0 费攻击,数值调整V1):当前每有 1 层[装填],造成 4 点伤害、获得 4 点格挡、[礼炮轰鸣]1;
/// 结算后将[装填]层数减半(向下取整)。升级:每层伤害/格挡提升至 6。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class SurgingPursuit : NaviaCardBase
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Load, NaviaKeywords.Salvo);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        // 预览感知三件套:面板过 Hook.ModifyDamage(力量/虚弱/附魔),与实际出伤一致。
        new CalculationBaseVar(0m),
        new ExtraDamageVar(4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner?.Creature?.GetPowerAmount<LoadPower>() ?? 0),
    };

    public SurgingPursuit()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        Creature creature = base.Owner.Creature;
        // 先算后耗:所有每层效果(伤害/格挡/礼炮)都以结算前的装填层数为准,减半放在最后。
        int load = creature.GetPowerAmount<LoadPower>();
        decimal total = base.DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        if (total > 0m)
        {
            await DamageCmd.Attack(total).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            // 每层格挡与每层伤害同值(基础 4/升级 6),直接复用伤害总额。
            await CreatureCmd.GainBlock(creature, total, ValueProp.Move, cardPlay);
        }
        // 「每层礼炮轰鸣 1」:逐层触发(手牌无礼炮时先生成一张,后续每次抬升其伤害,见 Salvo)。
        for (int i = 0; i < load; i++)
        {
            await Salvo.Fire(base.CombatState, choiceContext, base.Owner, 1m);
        }
        // 减半(向下取整):保留 load / 2 层,消耗其余部分;在全部每层效果结算完毕后执行。
        int consumed = load - load / 2;
        if (consumed > 0)
        {
            await LoadPower.Gain(choiceContext, creature, -consumed, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 每层 4→6:只抬升「每份」的增量,基准仍为 0。
        base.DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }
}
