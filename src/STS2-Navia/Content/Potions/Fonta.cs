using System;
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
/// 增加上限时沿用原版等量治疗;额外治疗按使用后的上限超出目标角色初始上限的部分计算,含本瓶新增上限。
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
        await CreatureCmd.GainMaxHp(target, maxHpGain);
        int extraMaxHp = Math.Max(0, target.MaxHp - target.Player!.Character.StartingHp);
        int bonusHeal = (int)(extraMaxHp / HealPerExtraMaxHp);
        if (bonusHeal > 0)
        {
            await CreatureCmd.Heal(target, bonusHeal);
        }
    }
}
