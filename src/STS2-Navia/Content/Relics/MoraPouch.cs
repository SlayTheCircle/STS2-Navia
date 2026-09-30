using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using NaviaMod.Content.Powers;
using NaviaMod.Content.RelicPools;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 摩拉袋子(Uncommon):闪耀摩拉会额外造成 2 点伤害。
/// 战斗开始时对自己施加隐藏增益 MoraPouchPower(层数=额外伤害值),施加时点走
/// BeforeCombatStart(vanilla BeltBuckle 同款「战斗开始施加型遗物」写法);
/// 增益随战斗结束自动消散,无需手动清理。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
public sealed class MoraPouch : NaviaRelicBase
{
    private const int BonusDamage = 2;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<MoraPouchPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, BonusDamage, base.Owner.Creature, null);
    }
}
