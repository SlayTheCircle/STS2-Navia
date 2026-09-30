using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 高额回报(罕见,0 费技能):本回合内,你每次打出或丢弃带有[gold]支援[/gold]效果的牌,
/// 都会在手牌中生成一张[gold]闪耀摩拉[/gold]。升级:获得保留。
/// 「丢弃」=弃牌堆口径(discard),不含消耗;逻辑在 <see cref="HighPayoutPower"/>
/// (打出钩 AfterCardPlayed + 弃牌钩 AfterCardDiscarded;打出走 CardCmd 之外的移堆路径,不会双计)。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class HighPayout : NaviaCardBase
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

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<ShiningMora>() };

    public HighPayout()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HighPayoutPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    // 升级获得保留:必须走 OnUpgrade→AddKeyword(写进 LocalKeywords 缓存集,运行时/读档回放/战斗克隆三路全通);
    // 严禁 IsUpgraded 条件式 CanonicalKeywords——它只在 _keywords 缓存首次物化时求值一次,升级后永不重算(实测事故)。
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
