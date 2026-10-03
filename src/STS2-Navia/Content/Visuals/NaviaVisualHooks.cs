using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using NaviaMod.Content.Characters;
using NaviaMod.Content.Visuals.CombatAnimation;

namespace NaviaMod.Content.Visuals;

/// <summary>独立于遗物的预热与待机生命状态刷新；攻击／受击／死亡由游戏原生触发路由。</summary>
[RegisterSingleton]
public sealed class NaviaVisualHooks : HookedSingletonModel
{
    public NaviaVisualHooks() : base(HookType.Combat) { }

    public override Task BeforeCombatStart()
    {
        if (CurrentCombatState?.Players.Any(player => player.Character is Navia) == true)
            NaviaCombatVisuals.Prewarm();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Character is Navia) NaviaCombatVisuals.Refresh(player.Creature);
        return Task.CompletedTask;
    }

    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature.Player?.Character is Navia) NaviaCombatVisuals.Refresh(creature);
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player player = cardPlay.GetPlayer();
        if (player.Character is Navia && cardPlay.Card.Type == CardType.Skill)
            NaviaCombatVisuals.PlaySkill(player.Creature);
        return Task.CompletedTask;
    }
}
