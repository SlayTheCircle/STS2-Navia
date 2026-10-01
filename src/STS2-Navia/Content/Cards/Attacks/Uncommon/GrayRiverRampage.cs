using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 横行灰河(罕见,1 费攻击,数值调整V3):造成 6 点伤害。手牌中每有一张带有[gold]支援[/gold]效果的牌,
/// 抽 1 张牌并恢复 1 点能量。升级:伤害 9,费用 0。
/// 「带有支援效果的牌」判据 = <c>card.Enchantment is NaviaSupportEnchantment</c>(支援系唯一判据)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class GrayRiverRampage : NaviaCardBase
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
        new DamageVar(6m, ValueProp.Move),
    };

    public GrayRiverRampage()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 本卡打出时已离开手牌,不会数进自己;伤害先结算,再按手牌支援数抽牌回能。
        int supportCount = CardPile.GetCards(base.Owner, PileType.Hand)
            .Count(c => c.Enchantment is NaviaSupportEnchantment);
        if (supportCount > 0)
        {
            await CardPileCmd.Draw(choiceContext, supportCount, base.Owner);
            await PlayerCmd.GainEnergy(supportCount, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
        base.EnergyCost.UpgradeBy(-1);
    }
}
