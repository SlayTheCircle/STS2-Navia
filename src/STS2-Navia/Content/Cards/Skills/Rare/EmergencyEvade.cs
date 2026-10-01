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
/// 紧急避险(稀有,0 费技能,数值调整V3):消耗全部[装填],每消耗 2 层获得 8 点格挡,并在下回合装填 1。
/// 升级:每 2 层格挡提升至 10。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class EmergencyEvade : NaviaCardBase
{
    public override bool GainsBlock => true;

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
        // 预览感知三件套(格挡侧):CalculatedBlockVar 的面板会过格挡修正钩子(敏捷等),与实际获得一致。
        new CalculationBaseVar(0m),
        new CalculationExtraVar(8m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => Math.Floor((card.Owner?.Creature?.GetPowerAmount<LoadPower>() ?? 0) / 2m)),
    };

    public EmergencyEvade()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = base.Owner.Creature;
        // 先算后耗:格挡总量以消耗前的装填层数计算(每 2 层 1 份)。
        int load = creature.GetPowerAmount<LoadPower>();
        decimal block = base.DynamicVars.CalculatedBlock.Calculate(cardPlay.Target);
        if (load > 0)
        {
            await LoadPower.Gain(choiceContext, creature, -load, creature, this);
        }
        if (block > 0m)
        {
            await CreatureCmd.GainBlock(creature, block, ValueProp.Move, cardPlay);
        }
        // 「下回合装填 1」:隐藏标记 Power,见 EmergencyEvadePower。
        await PowerCmd.Apply<EmergencyEvadePower>(choiceContext, creature, 1m, creature, this);
    }

    protected override void OnUpgrade()
    {
        // 每 2 层 8→10:只抬升「每份」的增量,基准仍为 0。
        base.DynamicVars["CalculationExtra"].UpgradeValueBy(2m);
    }
}
