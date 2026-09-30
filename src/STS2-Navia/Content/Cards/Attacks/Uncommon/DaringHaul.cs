using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 冒险收获(罕见,0 费攻击):造成 2 点伤害,抽 1 张牌,再将你的 1 张手牌放到抽牌堆顶部。
/// 置顶 = CardPileCmd.Add(..., PileType.Draw, CardPilePosition.Top)(vanilla PhotonCut/Headbutt 同款);
/// 选牌提示用自定义 card_selection 键 STS2_NAVIA_TO_TOP_OF_DRAW(行为与原版「放到抽牌堆顶」一致,
/// 沿用本模组自定义键惯例,不用 CardModel.SelectionScreenPrompt——那要求 cards 表有对应键,缺了直接抛)。
/// 升级:抽 1→2。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class DaringHaul : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(2m, ValueProp.Move),
        new CardsVar(1),
    };

    public DaringHaul()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
        // 手牌为空时 FromHand 返回空集,Add 对空集是安全的空操作(vanilla PhotonCut 原样直传)。
        await CardPileCmd.Add(await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_TOP_OF_DRAW"), 1), context: choiceContext, player: base.Owner, filter: null, source: this), PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
