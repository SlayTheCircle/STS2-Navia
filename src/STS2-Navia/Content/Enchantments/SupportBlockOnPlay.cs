using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NaviaMod.Content.Enchantments;

/// <summary>
/// 掩体支援(「邻里互助」施加):被附魔的牌每次打出时,获得 Amount 点格挡(本场战斗)。
/// 照 vanilla Adroit(打出得格挡的附魔)全套管线:BlockVar(ValueProp.Move)+ RecalculateValues,
/// 走 CreatureCmd.GainBlock 完整格挡获取流程(受敏捷等修正)。
/// 注意不能用 EnchantBlockAdditive——那个钩子只加到「该卡自身的格挡获取」上,
/// 攻击牌不获得格挡时钩子根本不触发,格挡必须在这里主动结算。
/// </summary>
[RegisterEnchantment]
public sealed class SupportBlockOnPlay : NaviaSupportEnchantment
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(0m, ValueProp.Move),
    };

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await CreatureCmd.GainBlock(base.Card.Owner.Creature, base.DynamicVars.Block, cardPlay);
    }

    public override void RecalculateValues()
    {
        base.DynamicVars.Block.BaseValue = base.Amount;
    }
}
