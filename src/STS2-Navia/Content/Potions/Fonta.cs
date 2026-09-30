using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.PotionPools;

namespace NaviaMod.Content.Potions;

/// <summary>
/// 枫达(普通,任意时点):获得 5 点最大生命值。每有 3 点额外的生命值上限,额外恢复 1 点生命值。
/// ——清爽枫达,畅饮世界!
/// 设计结算:「额外的生命值上限」指本药水带来的 5 点,基础回复为 0,故总回复 = 5/3 向下取整 = 1。
/// 实现注意:vanilla <c>CreatureCmd.GainMaxHp</c>(果汁同款)会自动回复等量生命,与本设计「基础回 0」冲突,
/// 因此改用 <c>SetMaxHp</c> 只抬上限,再按「每 3 点上限回 1 点」单独结算回复。
/// </summary>
[RegisterPotion(typeof(NaviaPotionPool))]
public sealed class Fonta : NaviaPotionBase
{
    private const decimal HealPerExtraMaxHp = 3m;

    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.AnyTime;

    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new MaxHpVar(5m) };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("7ce8d4"));
        decimal maxHpGain = base.DynamicVars.MaxHp.BaseValue;
        await CreatureCmd.SetMaxHp(target, target.MaxHp + maxHpGain);
        int bonusHeal = (int)(maxHpGain / HealPerExtraMaxHp);
        if (bonusHeal > 0)
        {
            await CreatureCmd.Heal(target, bonusHeal);
        }
    }
}
