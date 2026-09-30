using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.PotionPools;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Potions;

/// <summary>
/// 乐斯(稀有,仅战斗内):恢复你全部的生命值。本场战斗中,每个回合结束时,失去 1 点最大生命值。
/// ——过去为了追查这种危险饮品的下落,娜维娅险些付出生命的代价。
/// 限定战斗内饮用:引擎不允许场外挂 Power(PowerCmd.Apply 对无 CombatState 的目标静默返回,
/// vanilla Ambergris 同款边界),与其借遗物当宿主不如照原版口径收紧使用时点(2026-09-29 用户裁定)。
/// </summary>
[RegisterPotion(typeof(NaviaPotionPool))]
public sealed class Lesse : NaviaPotionBase
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new PowerVar<LesseWitheringPower>(1m) };

    /// <summary>悬停预览下一场战斗将生效的减益(再生药水引 Power 词条同款;ExtraHoverTips 在 ModPotionTemplate 已封死,改经 AdditionalHoverTips 供基类拼装)。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<LesseWitheringPower>() };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("86c06c"));
        await CreatureCmd.Heal(target, target.MaxHp - target.CurrentHp);
        await PowerCmd.Apply<LesseWitheringPower>(choiceContext, target, base.DynamicVars["LesseWitheringPower"].BaseValue, base.Owner.Creature, null);
    }
}
