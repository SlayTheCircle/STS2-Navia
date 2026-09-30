using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 引导轰炸的光环(可见增益):每当你打出一张[金花礼炮],抽等同于层数的牌。
/// 打出判据= <see cref="AfterCardPlayed"/> + cardSource 是 <c>GoldenRoseCannon</c>
/// (含被自动打出/复制体,入口同 <see cref="OptimisticForecastPower"/>)。作用域=本场战斗。
/// 层数=每次抽牌数(固定 1;重复打出能力自然叠层)。
/// </summary>
[RegisterPower]
public sealed class GuidedBombardmentPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is GoldenRoseCannon && cardPlay.Player.Creature == base.Owner)
        {
            Flash();
            await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player!);
        }
    }
}
