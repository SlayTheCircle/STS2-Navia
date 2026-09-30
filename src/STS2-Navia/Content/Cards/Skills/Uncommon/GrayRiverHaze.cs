using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Mechanics;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 灰河硝烟(罕见,0 费技能):消耗你手中全部的状态牌与诅咒牌(不含任务牌)。
/// 每消耗 1 张,[礼炮轰鸣]1——逐张触发:首张可能在手牌中生成金花礼炮,后续每次转为抬升其伤害(见 Salvo 语义)。
/// 升级:获得保留(必须 OnUpgrade→AddKeyword,手册 §6)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class GrayRiverHaze : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Salvo);
            return set;
        }
    }

    public GrayRiverHaze()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先快照再逐张结算(参考 AccidentalBlast):边遍历边改手牌会跳牌;
        // 状态/诅咒按 CardType 判定(Status=4/Curse=5),任务牌(Quest=6)不在其列。
        List<CardModel> targets = CardPile.GetCards(base.Owner, PileType.Hand)
            .Where((CardModel c) => c.Type == CardType.Status || c.Type == CardType.Curse)
            .ToList();
        foreach (CardModel card in targets)
        {
            await CardCmd.Exhaust(choiceContext, card);
            // 「每消耗 1 张,礼炮轰鸣 1」:逐次触发而非合计一次(礼炮语义差异见 Salvo)。
            await Salvo.Fire(base.CombatState, choiceContext, base.Owner, 1m);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
