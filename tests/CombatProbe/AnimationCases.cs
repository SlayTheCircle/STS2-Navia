using Godot;
using NaviaMod.Content.Characters;
using NaviaMod.Content.Visuals.CombatAnimation;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;
using static Fixture;

// Protect death priority, stale low-health return poses and cross-player sharing using the actual RitsuLib state machine.
internal static class AnimationCases
{
    public static void Run()
    {
        Require(typeof(IModCreatureCombatAnimationStateMachineFactory).IsAssignableFrom(typeof(Navia)), "Native animation factory missing");
        bool alive = true, low = false;
        var output = new Backend();
        var graph = new NaviaAnimationGraph(output, () => alive, () => low);
        var machine = graph.Machine;
        Require(output.Current == "idle", "Initial idle missing");
        machine.SetTrigger("Attack");
        graph.PlaySkillIfIdle(); Require(output.Current == "attack", "Skill fallback interrupted a native attack");
        low = true; graph.RefreshIdle();
        Require(output.Current == "attack", "HP refresh interrupted attack");
        output.Finish(); Require(output.Current == "low_health_loop", "Attack returned to stale HP state");
        low = false; graph.RefreshIdle(); Require(output.Current == "idle", "Healing did not restore idle");
        graph.PlaySkillIfIdle(); Require(output.Current == "cast", "Idle skill fallback missing");
        output.Finish();
        machine.SetTrigger("PowerUp"); Require(output.Current == "cast", "Ability trigger absent");
        machine.SetTrigger("Hit"); Require(output.Current == "hurt", "Hit did not interrupt cast");
        alive = false; machine.SetTrigger("Dead");
        Require(output.Current == "die", "Death did not win priority");
        output.Finish(); Require(output.Current == "die", "Death returned to idle");
        foreach (string trigger in new[] { "Hit", "Attack", "Cast", "PowerUp", "Idle", "Relaxed", "Revive" })
            machine.SetTrigger(trigger);
        Require(output.Current == "die", "Dead state accepted ordinary trigger");
        alive = true; machine.SetTrigger("Revive");
        machine.SetTrigger("Idle"); machine.SetTrigger("Attack");
        Require(output.Current == "revive", "Revive got interrupted by normal action");
        output.Finish(); Require(output.Current == "idle", "Revive did not return to idle");
        var second = new Backend();
        var other = new NaviaAnimationGraph(second, () => true, () => true);
        machine.SetTrigger("Attack");
        Require(second.Current == "low_health_loop", "Animation state shared between players");
        machine.Dispose(); other.Machine.Dispose();
        Console.WriteLine("PASS animations: native factory, HP-sensitive return, interrupt, death terminal, revive and player isolation");
    }

    private sealed class Backend : IAnimationBackend
    {
        public Node? OwnerNode => null;
        public string Current { get; private set; } = "";
        public event Action<string>? Started;
        public event Action<string>? Completed;
        public event Action<string>? Interrupted;
        public bool HasAnimation(string id) => true;
        public void Play(string id, bool loop)
        {
            if (Current.Length > 0) Interrupted?.Invoke(Current);
            Current = id; Started?.Invoke(id);
        }
        public void Queue(string id, bool loop) => throw new InvalidOperationException("Graph must choose rest state at completion");
        public void Finish() => Completed?.Invoke(Current);
    }
}
