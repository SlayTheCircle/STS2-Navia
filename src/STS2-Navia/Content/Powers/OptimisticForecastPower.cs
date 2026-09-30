using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Enchantments;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 乐观估测(可见增益):每当你打出一张带有[gold]支援[/gold]效果的牌,抽 <see cref="Amount"/> 张牌。
/// 打出后钩 <c>AfterCardPlayed</c> + <c>Enchantment is NaviaSupportEnchantment</c> 判据
/// (入口同 <see cref="FinancialMarketPower"/>)。作用域=本场战斗。
/// </summary>
[RegisterPower]
public sealed class OptimisticForecastPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature == base.Owner && cardPlay.Card.Enchantment is NaviaSupportEnchantment)
        {
            Flash();
            await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player!);
        }
    }
}
