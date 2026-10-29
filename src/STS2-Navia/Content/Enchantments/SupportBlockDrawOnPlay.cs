using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.Compat;

namespace NaviaMod.Content.Enchantments;

/// <summary>委托支援(「会长委托」施加):被附魔的牌打出时，所有存活玩家获得格挡并抽牌。Amount 为档位，1 档为 4 格挡/抽 1，2 档为 7 格挡/抽 2。</summary>
[RegisterEnchantment]
public sealed class SupportBlockDrawOnPlay : NaviaSupportEnchantment
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(4m, ValueProp.Move),
        new CardsVar(1),
    };

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        Creature owner = base.Card.Owner.Creature;
        foreach (Creature teammate in owner.CombatState!.GetTeammatesOf(owner).Where(c => c.IsAlive && c.Player != null))
        {
            await CreatureCmd.GainBlock(teammate, base.DynamicVars.Block, cardPlay);
            await NaviaCardPileCommandShim.DrawForPlayer(choiceContext, base.DynamicVars.Cards.BaseValue, teammate.Player!, base.Card);
        }
    }

    public override void RecalculateValues()
    {
        base.DynamicVars.Block.BaseValue = 4m + (base.Amount - 1m) * 3m;
        base.DynamicVars.Cards.BaseValue = base.Amount;
    }
}
