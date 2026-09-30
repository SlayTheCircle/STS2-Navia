using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Powers;

/// <summary>
/// 意外事故的护甲(可见增益,数值调整V1):你每消耗 1 张[金花礼炮](无论何种方式——打出自然消耗、
/// 铸剑为犁/轻装上阵等指定消耗),都获得等同于层数的格挡并装填 1。
/// 走 <see cref="AfterCardExhausted"/>(vanilla DarkEmbracePower 同款钩子);
/// 层数=每张礼炮的格挡值(基准 6,升级 9)。
/// </summary>
[RegisterPower]
public sealed class AccidentalBlastPower : NaviaPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card is GoldenRoseCannon && card.Owner.Creature == base.Owner && base.Owner.IsAlive)
        {
            Flash();
            // 能力来源的格挡走 Unpowered(不吃敏捷类修正),与 UnitedFrontPower 同口径。
            await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
            await LoadPower.Gain(choiceContext, base.Owner, 1, base.Owner, null);
        }
    }
}
