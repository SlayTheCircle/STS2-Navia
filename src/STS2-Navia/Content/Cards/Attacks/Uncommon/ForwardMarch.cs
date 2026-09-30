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
using NaviaMod.Content.Enchantments;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 高歌猛进(罕见,1 费攻击):造成 16 点伤害;抽牌堆中的 1 张随机攻击牌获得支援:造成的伤害 -4。
/// 升级:伤害 20(支援值不变)。
/// 随机必须用游戏内确定性 RNG:RunState.Rng.CombatCardSelection(vanilla 专用于「战斗中随机选牌」,
/// True Grit/Improvement 同源),严禁 System.Random(联机回放会炸)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class ForwardMarch : NaviaCardBase
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
        new DamageVar(16m, ValueProp.Move),
        new DynamicVar("SupportAmount", 4m),
    };

    public ForwardMarch()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 候选=抽牌堆中未附魔的攻击牌(与会长号令的过滤器同式);列表顺序取牌堆原生顺序,回放确定。
        List<CardModel> candidates = PileType.Draw.GetPile(base.Owner).Cards
            .Where(c => c.Type == CardType.Attack && c.Enchantment == null)
            .ToList();
        CardModel? randomCard = base.Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (randomCard != null)
        {
            CardCmd.Enchant<SupportDamageDown>(randomCard, base.DynamicVars["SupportAmount"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
