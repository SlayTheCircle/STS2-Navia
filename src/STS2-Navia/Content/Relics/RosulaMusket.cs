using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.ValueProps;
using NaviaMod.Content.Powers;
using NaviaMod.Content.RelicPools;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 刺玫会特制铳枪(Common):每当你获得 1 层[装填],对所有敌人造成 2 点伤害。
/// 监听 <see cref="LoadPower"/> 的 AfterPowerAmountChanged:amount>0 即获得——
/// <see cref="LoadPower.Gain"/> 已按上限截断后才落层,事件里的 amount 即实际增量,不会虚报溢出部分。
/// 逐层独立结算(一次获得 2 层 = 两次全体伤害),与高压弹膛的「每层一次」语义对称,
/// 文案「每当你获得 1 层」按逐层触发理解;伤害用 ValueProp.Unpowered,不吃力量/虚弱修正。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
public sealed class RosulaMusket : NaviaRelicBase
{
    private const int DamagePerStack = 2;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount > 0m && power is LoadPower && power.Owner == base.Owner.Creature && CombatManager.Instance.IsInProgress)
        {
            int stacks = (int)amount;
            for (int i = 0; i < stacks; i++)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, base.Owner.Creature.CombatState.HittableEnemies, DamagePerStack, ValueProp.Unpowered, base.Owner.Creature);
            }
        }
    }
}
