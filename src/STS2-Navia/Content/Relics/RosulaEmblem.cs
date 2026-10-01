using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.ValueProps;
using NaviaMod.Content.Powers;
using NaviaMod.Content.RelicPools;
using NaviaMod.Content.Visuals;

namespace NaviaMod.Content.Relics;

/// <summary>
/// 刺玫会徽(初始遗物):每回合你第一次获得格挡时,装填 1。
/// 兼任角色视觉驱动宿主(每局常驻):回合开始/受击/死亡/出牌时驱动
/// <see cref="Visuals.NaviaCharacterVisuals"/> 的四态换装与动效(无 Spine 2D 路线)。
/// TODO:进阶遗物「仅留余香的野蔷薇」(战斗开始时装填 3)实装时参照本类。
/// </summary>
[RegisterRelic(typeof(NaviaRelicPool))]
[RegisterTouchOfOrobasRefinement(typeof(RosulaFragrance))] // 欧罗巴斯之触:会徽→仅留余香的野蔷薇(vanilla RefinementUpgrades 的 mod 接线;不接会被兜底换成圆环饰)
public sealed class RosulaEmblem : NaviaRelicBase
{
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

    // ---- 角色视觉驱动(宿主=本遗物,每局常驻) ----

    public override Task AfterObtained()
    {
        // 开局获得初始遗物即预热四态立绘(此时无关卡画面),消除进战斗时立绘延迟出现。
        NaviaCharacterVisuals.Prewarm();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == base.Owner)
        {
            NaviaCharacterVisuals.Refresh(player.Creature);
        }
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == base.Owner.Creature && CombatManager.Instance.IsInProgress)
        {
            NaviaCharacterVisuals.PlayHurt(target);
        }
        return Task.CompletedTask;
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature == base.Owner.Creature)
        {
            NaviaCharacterVisuals.Refresh(creature);
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.GetPlayer().Creature == base.Owner.Creature)
        {
            NaviaCharacterVisuals.PlaySkillPose(cardPlay.GetPlayer().Creature);
        }
        return Task.CompletedTask;
    }
}
