using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 危险改装(稀有,1 费能力,数值调整V4 重做):接下来 3 个回合开始时,先失去 1 层[装填],
/// 再将其补至当前上限;此后每回合开始时仅失去 1 层[装填](永久)。
/// 升级:获得[固有]。补满倒数在 <see cref="DangerousRetrofitFillPower"/>,
/// 永久流失在 <see cref="DangerousRetrofitPower"/>(两者用在场判断互斥,不依赖钩子顺序)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class DangerousRetrofit : NaviaCardBase
{
    public const int FillTurns = 3;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    public DangerousRetrofit()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        // 补满倒数 3 层;重复打出时层数累加(更多补满回合),流失仍只 1 层/回合。
        await PowerCmd.Apply<DangerousRetrofitFillPower>(choiceContext, base.Owner.Creature, FillTurns, base.Owner.Creature, this);
        await PowerCmd.Apply<DangerousRetrofitPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级:获得固有(手册 §6:升级加关键词走 OnUpgrade→AddKeyword)。
        AddKeyword(CardKeyword.Innate);
    }
}
