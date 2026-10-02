using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.ValueProps;
using NaviaMod.Content.Powers;
using NaviaMod.Content.RelicPools;
using MegaCrit.Sts2.Core.HoverTips;
using NaviaMod.Content.Keywords;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 刺玫会徽(初始遗物):每回合你第一次获得格挡时,装填 1。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
[RegisterTouchOfOrobasRefinement(typeof(RosulaFragrance))] // 欧罗巴斯之触:会徽→仅留余香的野蔷薇(vanilla RefinementUpgrades 的 mod 接线;不接会被兜底换成圆环饰)
public sealed class RosulaEmblem : NaviaRelicBase
{

    /// <summary>文本提及机制名词,挂关键词词条悬停解释。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Load);
    private bool _loadedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Starter;

    private bool LoadedThisTurn
    {
        get => _loadedThisTurn;
        set
        {
            AssertMutable();
            _loadedThisTurn = value;
        }
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner.Creature))
        {
            LoadedThisTurn = false;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature != base.Owner.Creature || amount <= 0m || LoadedThisTurn || !CombatManager.Instance.IsInProgress)
        {
            return;
        }
        LoadedThisTurn = true;
        Flash();
        await LoadPower.Gain(new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1, base.Owner.Creature, null);
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        LoadedThisTurn = false;
        return Task.CompletedTask;
    }

}
