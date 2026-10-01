using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 破甲兵装(稀有,1 费技能,数值调整V1 升级+保留):失去当前所有格挡,每失去 3 点就对所有敌人造成 4 点伤害;
/// 下个回合开始时你的格挡不会消失。升级:每次伤害 5。
/// 技能卡直接造成全体伤害:参考 vanilla Conflagration 的 WithHitCount + TargetingAllOpponents 写法。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class ArmorPiercer : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(4m, ValueProp.Move),
    };

    public ArmorPiercer()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature creature = base.Owner.Creature;
        // 先算后耗:命中数必须在失去格挡之前按当前格挡计算(否则永远是 0)。
        int block = creature.Block;
        int hits = block / 3;
        if (block > 0)
        {
#if NAVIA_GAME_0107_1
            // 0.107.1 的 LoseBlock 无上下文/移除者参数,语义降级为仅失去格挡。
            await CreatureCmd.LoseBlock(creature, block);
#else
            await CreatureCmd.LoseBlock(choiceContext, creature, block, creature);
#endif
        }
        if (hits > 0)
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this, cardPlay)
                .TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
        }
        await PowerCmd.Apply<OneTurnBlockPersistPower>(choiceContext, creature, 1, creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(1m);
        // 数值调整V1:升级后获得保留(手册 §6:升级加关键词走 OnUpgrade→AddKeyword)。
        AddKeyword(CardKeyword.Retain);
    }
}
