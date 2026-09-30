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

namespace NaviaMod.Content.Cards;

/// <summary>
/// 紧急调度(罕见,1 费技能):抽 6 张牌,丢弃其中所有费用为 0 的牌。
/// 「其中」按 CardPileCmd.Draw 的返回值精确圈定——只丢弃这次抽上来的 0 费牌,
/// 不波及手牌里原有的 0 费牌(vanilla Scrape 的镜像写法,那边留 0 费、弃非 0 费)。
/// 升级:费用 1→0。
/// </summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class UrgentDispatch : NaviaCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(6),
    };

    public UrgentDispatch()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.IntValue, base.Owner);
        // 费用判定用含全部修正的实付费用(vanilla Scrape/AllForOne 同口径);X 费牌不算 0 费。
        List<CardModel> zeroCost = drawn.Where((CardModel c) => c.EnergyCost.GetWithModifiers(CostModifiers.All) == 0 && !c.EnergyCost.CostsX).ToList();
        // 多张弃牌必须走 IEnumerable 重载(单卡重载循环调用会打乱 Sly 等弃牌钩子的时序)。
        await CardCmd.Discard(choiceContext, zeroCost);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
