using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 武器保养(罕见,1 费能力,数值调整V1):将 1 张去除了[消耗]的[金花礼炮]加入你的手牌。
/// 升级:获得固有。生成走 <see cref="GoldenRoseCannon.CreateInHand"/>(按礼炮轰鸣/炮火连天累计值起算),
/// 再实例级移除消耗关键词(手册 §6:实例关键词增删走 Add/RemoveKeyword)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class WeaponMaintenance : NaviaCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public WeaponMaintenance()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        if (base.CombatState is ICombatState combatState)
        {
            GoldenRoseCannon? cannon = await GoldenRoseCannon.CreateInHand(base.Owner, combatState);
            cannon?.RemoveKeyword(CardKeyword.Exhaust);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
