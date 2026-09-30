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
using STS2RitsuLib.Keywords;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.PotionPools;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Potions;

/// <summary>
/// 刺玫佳酿(普通,战斗内):装填 3。
/// ——很多人不知道的是,卡雷斯会长生前很喜欢品酒。
/// 给予装填一律走 <see cref="LoadPower.Gain"/> 统一入口(自动处理 6/9 层上限)。
/// </summary>
[RegisterPotion(typeof(NaviaPotionPool))]
public sealed class RosulaWine : NaviaPotionBase
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Loads", 3m) };

    /// <summary>药水没有 CanonicalKeywords,机制名词的悬停解释经 AdditionalHoverTips 挂关键词词条(等价于卡面的「装填」横幅;ExtraHoverTips 在 ModPotionTemplate 已封死,由基类统一拼装)。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            List<IHoverTip> tips = new List<IHoverTip>();
            if (ModKeywordRegistry.TryGetCardKeyword(NaviaKeywords.Load, out CardKeyword keyword))
            {
                tips.Add(HoverTipFactory.FromKeyword(keyword));
            }
            return tips;
        }
    }

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("a4263c"));
        await LoadPower.Gain(choiceContext, target, (int)base.DynamicVars["Loads"].BaseValue, base.Owner.Creature, null);
    }
}
