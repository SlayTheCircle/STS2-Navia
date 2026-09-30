using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 轻装上阵(罕见,1 费攻击,数值调整V1):造成 9 点伤害。如果你的手中有[金花礼炮],则选择一张消耗,
/// 再造成 9 点伤害,并获得与该卡造成伤害总量相同(即两次伤害之和)的格挡。
/// 升级:两段伤害均提升至 12。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class TravelLight : NaviaCardBase
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(9m, ValueProp.Move),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public TravelLight()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        AttackCommand firstHit = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (!CardPile.GetCards(base.Owner, PileType.Hand).OfType<GoldenRoseCannon>().Any())
        {
            return;
        }
        CardModel selection = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), context: choiceContext, player: base.Owner, filter: (CardModel c) => c is GoldenRoseCannon, source: this)).FirstOrDefault();
        if (selection == null)
        {
            return;
        }
        await CardCmd.Exhaust(choiceContext, selection);
        AttackCommand secondHit = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        // 格挡 = 该卡两次伤害的实际总量(含溢出),与 vanilla「钳身拳」的结算口径一致。
        decimal totalDamage = SumDamage(firstHit) + SumDamage(secondHit);
        await CreatureCmd.GainBlock(base.Owner.Creature, totalDamage, ValueProp.Move, cardPlay);
    }

    /// <summary>汇总一次攻击的实际伤害(参照 vanilla Fisticuffs:TotalDamage + OverkillDamage)。</summary>
    private static decimal SumDamage(AttackCommand attack)
    {
        return attack.Results.SelectMany((List<DamageResult> r) => r).Sum((DamageResult r) => r.TotalDamage + r.OverkillDamage);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
