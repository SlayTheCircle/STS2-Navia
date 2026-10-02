using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using NaviaMod.Content.Characters;

namespace NaviaMod.Content.Visuals;

/// <summary>视觉生命周期独立于可替换的遗物；单例只分发事件，不保存玩家或场景节点。</summary>
[RegisterSingleton]
public sealed class NaviaVisualHooks : HookedSingletonModel
{
    public NaviaVisualHooks() : base(HookType.Combat) { }

    public override Task BeforeCombatStart()
    {
        if (CurrentCombatState?.Players.Any(player => player.Character is Navia) == true)
        {
            NaviaCharacterVisuals.Prewarm();
        }
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Character is Navia)
        {
            NaviaCharacterVisuals.Refresh(player.Creature);
        }
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target.Player?.Character is Navia && CombatManager.Instance.IsInProgress)
        {
            NaviaCharacterVisuals.PlayHurt(target);
        }
        return Task.CompletedTask;
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature.Player?.Character is Navia)
        {
            NaviaCharacterVisuals.Refresh(creature);
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player player = cardPlay.GetPlayer();
        if (player.Character is Navia)
        {
            NaviaCharacterVisuals.PlaySkillPose(player.Creature);
        }
        return Task.CompletedTask;
    }
}
