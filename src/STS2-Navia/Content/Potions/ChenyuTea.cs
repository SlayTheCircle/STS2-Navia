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
using NaviaMod.Content.Cards;
using NaviaMod.Content.PotionPools;

namespace NaviaMod.Content.Potions;

/// <summary>
/// 沉玉茶露(罕见,战斗内):将 3 张闪耀摩拉加入你的手牌。
/// ——沉玉谷的茶叶可以从柔灯港直接运送到枫丹。自从娜维娅从沉玉谷度假归来,她就深深爱上了这种饮料。
/// 生成摩拉直接复用 <see cref="ShiningMora.CreateInHand"/>(自带战斗结束弃置与打出计数接线)。
/// </summary>
[RegisterPotion(typeof(NaviaPotionPool))]
public sealed class ChenyuTea : NaviaPotionBase
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Moras", 3m) };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("9fd9a8"));
        await ShiningMora.CreateInHand(target.Player, (int)base.DynamicVars["Moras"].BaseValue, target.CombatState);
    }
}
