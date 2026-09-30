using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 掩护轰炸(先古,2 费能力,虚无):回合开始时,自身每有 1 层[装填],便在本回合获得 1 点力量。
/// 升级:移除虚无(达弗给予的先古卡)。力量加成走 ±StrengthPower(回合末如数移除),
/// 不新造 TemporaryStrengthPower 子类——那需要额外的图标与 loc,且 vanilla 共享资产即可正确显示。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class CoveringFire : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword> { CardKeyword.Ethereal };
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    public CoveringFire()
        : base(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<CoveringFirePower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
