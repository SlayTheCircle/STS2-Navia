using System;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace NaviaMod.Content.Visuals.CombatAnimation;

/// <summary>战斗动作优先级与回位规则；状态属于单个战斗形象，不能跨玩家共享。</summary>
public sealed class NaviaAnimationGraph
{
    private readonly Func<bool> _isAlive;
    private readonly Func<bool> _isLowHealth;
    private readonly ModAnimState _idle = new("idle", true);
    private readonly ModAnimState _low = new("low_health_loop", true);
    private readonly ModAnimState _relaxed = new("relaxed_loop", true);
    private readonly ModAnimState _dead = new("die");
    private readonly ModAnimState _revive = new("revive");

    public ModAnimStateMachine Machine { get; }

    public NaviaAnimationGraph(IAnimationBackend backend, Func<bool> isAlive, Func<bool> isLowHealth)
    {
        _isAlive = isAlive;
        _isLowHealth = isLowHealth;
        Machine = new ModAnimStateMachine(backend);
        Machine.AddAnyState("Attack", new ModAnimState("attack"), CanAct);
        Machine.AddAnyState("Cast", new ModAnimState("cast"), CanAct);
        Machine.AddAnyState("PowerUp", new ModAnimState("cast"), CanAct);
        Machine.AddAnyState("Hit", new ModAnimState("hurt"), CanAct);
        Machine.AddAnyState("Dead", _dead, () => Machine.Current != _dead);
        Machine.AddAnyState("Revive", _revive, () => _isAlive() && Machine.Current == _dead);
        Machine.AddAnyState("Relaxed", _relaxed, CanAct);
        Machine.AddAnyState("Idle", _low, () => CanRefreshIdle() && _isLowHealth());
        Machine.AddAnyState("Idle", _idle, () => CanRefreshIdle() && !_isLowHealth());
        // Choose the return pose at completion so healing/damage during an attack cannot queue stale HP state.
        Machine.AnimationCompleted += state =>
        {
            if (!state.IsLooping && state != _dead && Machine.Current == state)
                Machine.Start(_isAlive() ? RestState() : _dead);
        };
        Machine.Start(_isAlive() ? RestState() : _dead);
    }

    public void RefreshIdle()
    {
        if (CanRefreshIdle() && Machine.Current != RestState())
            Machine.SetTrigger("Idle");
    }

    public void PlaySkillIfIdle()
    {
        // Some skills have no native Cast trigger. Never replace an attack or cast they already started.
        if (CanRefreshIdle()) Machine.SetTrigger("Cast");
    }

    private ModAnimState RestState() => _isLowHealth() ? _low : _idle;
    private bool CanAct() => _isAlive() && Machine.Current != _dead && Machine.Current != _revive;
    private bool CanRefreshIdle() => CanAct() && Machine.Current?.IsLooping == true;
}
