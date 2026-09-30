using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Mechanics;

/// <summary>
/// 礼炮轰鸣(数值调整V1,设计者勘误 2026-09-30):使卡组中所有[当前已存在]的金花礼炮伤害提升 X——
/// 即时收益,不累计、不继承:每张礼炮只吃它存在期间触发的轰鸣,之后生成的礼炮从 2 伤白板起算
/// (伤害成长保留在实例上属正常;持续的次数成长只有炮火连天,见 <see cref="BoostHits"/>)。
/// 触发时若手牌中没有金花礼炮,则生成一张。所有「［礼炮轰鸣］N」的来源都应调用 <see cref="Fire"/>。
/// </summary>
public static class Salvo
{
    public static async Task Fire(ICombatState? combatState, PlayerChoiceContext choiceContext, Player player, decimal value)
    {
        if (combatState == null || value <= 0m || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }
        // 即时抬升全部牌堆中已存在的礼炮实例(实例随战斗结束弃置,不会泄漏到主卡组)。
        foreach (GoldenRoseCannon cannon in AllCannons(player))
        {
            cannon.DynamicVars.Damage.BaseValue += value;
        }
        if (CardPile.GetCards(player, PileType.Hand).OfType<GoldenRoseCannon>().Any())
        {
            return;
        }
        await GoldenRoseCannon.CreateInHand(player, combatState);
    }

    /// <summary>炮火连天的伤害次数加成(持续成长,「接下来所有」):累计+value 并即时抬升既有礼炮实例;
    /// 调用前提是炮火连天已在场,层数供后续生成的礼炮经 CreateInHand 起算。</summary>
    public static async Task BoostHits(PlayerChoiceContext choiceContext, Player player, decimal value)
    {
        Creature creature = player.Creature;
        CannonadePower? power = creature.GetPower<CannonadePower>();
        if (power == null)
        {
            return;
        }
        await PowerCmd.ModifyAmount(choiceContext, power, value, creature, null);
        foreach (GoldenRoseCannon cannon in AllCannons(player))
        {
            cannon.DynamicVars["Hits"].BaseValue += value;
        }
    }

    /// <summary>当前礼炮伤害次数累计加成(炮火连天层数,未在场为 0),供生成新礼炮起算。</summary>
    public static int HitsBonus(Player player) => player.Creature.GetPowerAmount<CannonadePower>();

    private static IEnumerable<GoldenRoseCannon> AllCannons(Player player)
    {
        return CardPile.GetCards(player, PileType.Draw, PileType.Hand, PileType.Discard, PileType.Exhaust, PileType.Play)
            .OfType<GoldenRoseCannon>();
    }
}
