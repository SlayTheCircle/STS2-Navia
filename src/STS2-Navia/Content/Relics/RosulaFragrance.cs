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
/// 仅留余香的野蔷薇(Rare):战斗开始时,装填 3。每回合你第一次获得格挡时,装填 1。
/// 刺玫会徽的强化版:回合格挡触发逻辑与会徽一致(参照 RosulaEmblem,但不继承它——
/// 直接继承 NaviaRelicBase,避免捆绑初始遗物的语义);新增「战斗开始时装填 3」走
/// BeforeCombatStart(vanilla Anchor 同款战斗开始时点),经由 LoadPower.Gain 统一入口。
/// 注:设计上的「进阶遗物」解锁接线延后,本批先按 Rare 掉落物实装。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
public sealed class RosulaFragrance : NaviaRelicBase
{

    /// <summary>文本提及机制名词,挂关键词词条悬停解释。</summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => NaviaKeywords.HoverTips(NaviaKeywords.Load);
    private const int CombatStartLoad = 3;

    private bool _loadedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Starter; // 世界线裁定 2026-09-29:转正为进阶初始遗物(欧罗巴斯相遇后开局携带),不再作稀有掉落

    private bool LoadedThisTurn
    {
        get => _loadedThisTurn;
        set
        {
            AssertMutable();
            _loadedThisTurn = value;
        }
    }

    public override async Task BeforeCombatStart()
    {
        Flash();
        await LoadPower.Gain(new ThrowingPlayerChoiceContext(), base.Owner.Creature, CombatStartLoad, base.Owner.Creature, null);
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
