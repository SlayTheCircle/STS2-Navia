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
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.PotionPools;

namespace NaviaMod.Content.Potions;

/// <summary>
/// 铳枪润滑油(罕见,战斗内):礼炮轰鸣 3。
/// ——枪支需要仔细保养,平时多照顾它们,关键时刻才能指望它们照顾我们。
/// 「礼炮轰鸣 N」统一走 <see cref="Salvo.Fire"/>:无炮生成金花礼炮,有炮则全体强化。
/// </summary>
[RegisterPotion(typeof(NaviaPotionPool))]
public sealed class FirearmLubricant : NaviaPotionBase
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Salvo", 3m) };

    /// <summary>药水没有 CanonicalKeywords,机制名词的悬停解释经 AdditionalHoverTips 挂关键词词条(等价于卡面的「礼炮轰鸣」横幅;ExtraHoverTips 在 ModPotionTemplate 已封死,由基类统一拼装)。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            List<IHoverTip> tips = new List<IHoverTip>();
            if (ModKeywordRegistry.TryGetCardKeyword(NaviaKeywords.Salvo, out CardKeyword keyword))
            {
                tips.Add(HoverTipFactory.FromKeyword(keyword));
            }
            return tips;
        }
    }

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("c8963e"));
        await Salvo.Fire(target.CombatState, choiceContext, target.Player, base.DynamicVars["Salvo"].BaseValue);
    }
}
