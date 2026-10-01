using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Enchantments;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 会长号令(普通,1 费攻击):造成 3 点伤害;选择手中一张攻击牌,使其获得支援:本场战斗中
/// 该卡造成伤害 +2。升级:伤害 5,支援 +3。
/// 支援走 vanilla 附魔槽(CardCmd.Enchant);目标限定手牌中未持有附魔的攻击牌
/// (vanilla 单附魔槽,已附魔的卡 CanEnchant 会拒绝,选牌过滤器提前排除避免选了白选)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class PresidentOrder : NaviaCardBase
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

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("SupportAmount", 2m),
    };

    public PresidentOrder()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 手牌无可选牌时 FromHand 直接返回空集(参考 HoldPosition/BurningPact),判空即可,不软锁。
        CardModel? chosen = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_GAIN_SUPPORT"), 1),
            context: choiceContext,
            player: base.Owner,
            filter: c => c.Type == CardType.Attack && c.Enchantment == null,
            source: this)).FirstOrDefault();
        if (chosen != null)
        {
            CardCmd.Enchant<SupportDamageUp>(chosen, base.DynamicVars["SupportAmount"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
        base.DynamicVars["SupportAmount"].UpgradeValueBy(1m);
    }
}
