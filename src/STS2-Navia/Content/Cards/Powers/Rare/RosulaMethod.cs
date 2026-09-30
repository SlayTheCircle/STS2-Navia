using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 刺玫手段(稀有,2 费能力,数值调整V1):每回合,你打出的第一张带有[gold]支援[/gold]效果的牌可以免费打出。
/// 升级:费用 3→2。逻辑在 <see cref="RosulaMethodPower"/>(费用视 0 走 vanilla FreeAttackPower 的
/// TryModifyEnergyCostInCombatLate 范式,每回合计数在 BeforeSideTurnStart 重置)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class RosulaMethod : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Support);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public RosulaMethod()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<RosulaMethodPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级:2 费 → 1 费(vanilla 降费写法)。
        base.EnergyCost.UpgradeBy(-1);
    }
}
