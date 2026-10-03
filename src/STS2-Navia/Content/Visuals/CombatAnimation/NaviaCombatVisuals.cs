using System;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine.Backends;

namespace NaviaMod.Content.Visuals.CombatAnimation;

/// <summary>绑定原生 AnimationPlayer 和生物生命状态；游戏动作通过 RitsuLib 原生触发路由。</summary>
public static class NaviaCombatVisuals
{
    public const string RigPath = "res://STS2-Navia/scenes/characters/rig/navia_combat.tscn";
    private static readonly ConditionalWeakTable<Node, NaviaAnimationGraph> Graphs = new();

    public static ModAnimStateMachine Create(Node visualsRoot)
    {
        if (Graphs.TryGetValue(visualsRoot, out var existing)) return existing.Machine;
        var creatureNode = visualsRoot.GetParent() as NCreature
            ?? throw new InvalidOperationException("Navia combat visual must belong to NCreature.");
        Creature creature = creatureNode.Entity;
        var player = visualsRoot.GetNode<AnimationPlayer>("Visuals/Rig/AnimationPlayer");
        var backend = new GodotAnimationPlayerBackend(player);
        var graph = new NaviaAnimationGraph(backend, () => creature.IsAlive,
            () => creature.CurrentHp <= creature.MaxHp * 0.25m);
        Graphs.Add(visualsRoot, graph);
        visualsRoot.TreeExiting += () =>
        {
            graph.Machine.Dispose();
            backend.Dispose();
            Graphs.Remove(visualsRoot);
        };
        return graph.Machine;
    }

    public static void Prewarm()
    {
        if (IsMainThread) _ = ResourceLoader.Load<PackedScene>(RigPath);
    }

    public static void Refresh(Creature? creature)
    {
        if (!IsMainThread || creature == null) return;
        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node?.Visuals is not { } visuals) return;
        _ = Create(visuals);
        if (Graphs.TryGetValue(visuals, out var graph)) graph.RefreshIdle();
    }

    public static void PlaySkill(Creature creature)
    {
        if (!IsMainThread || !creature.IsAlive) return;
        var visuals = NCombatRoom.Instance?.GetCreatureNode(creature)?.Visuals;
        if (visuals == null) return;
        _ = Create(visuals);
        if (Graphs.TryGetValue(visuals, out var graph)) graph.PlaySkillIfIdle();
    }

    private static bool IsMainThread => OS.GetThreadCallerId() == OS.GetMainThreadId();
}
