using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 金融市场(罕见,1 费技能):本回合内,你每打出一张技能牌,就在抽牌堆生成一张[闪耀摩拉]。
/// 升级:费用 0,并获得保留。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class FinancialMarket : NaviaCardBase
{
    // 升级获得保留走 OnUpgrade→AddKeyword(见下);CanonicalKeywords 条件式会被 _keywords
    // 首次物化缓存吞掉,升级后永不重算——旧写法的保留从未生效过(费用 1→0 掩盖了它)。

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public FinancialMarket()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FinancialMarketPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
        AddKeyword(CardKeyword.Retain);
    }
}
