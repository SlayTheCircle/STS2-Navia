using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Mechanics;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 军火流通(普通,1 费技能,数值调整V1):[礼炮轰鸣]2,然后丢弃 1 张手牌;若丢弃的是金花礼炮,
/// 获得 1 点力量、对所有敌人造成 5 点伤害并装填 1(结算顺序:轰鸣先于丢弃)。升级:轰鸣 3,伤害 8。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class ArmsCirculation : NaviaCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Load, NaviaKeywords.Salvo);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Salvo", 2m),
        new DynamicVar("BonusDamage", 5m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public ArmsCirculation()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 数值调整V1:结算顺序改为轰鸣在先、丢弃在后;丢弃金花礼炮额外得 1 力量。
        await Salvo.Fire(base.CombatState, choiceContext, base.Owner, base.DynamicVars["Salvo"].BaseValue);
        IEnumerable<CardModel> discarded = await CardSelectCmd.FromHandForDiscard(
            choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), null, this);
        await CardCmd.Discard(choiceContext, discarded);
        if (discarded.FirstOrDefault() is GoldenRoseCannon)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
            await DamageCmd.Attack(base.DynamicVars["BonusDamage"].BaseValue).FromCard(this, cardPlay)
                .TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await LoadPower.Gain(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Salvo"].UpgradeValueBy(1m);
        base.DynamicVars["BonusDamage"].UpgradeValueBy(3m);
    }
}
