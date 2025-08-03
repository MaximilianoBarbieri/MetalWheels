using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using Fusion;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Serialization;
using static MoodsNpc;
using static GoapActionName;

public class NPCGoap : NetworkBehaviour
{
    public WorldState WorldState = new();

    private NPC npc;
    private List<GoapAction> _actions;
    private Queue<GoapAction> _currentPlan = new();

    private Coroutine currentPlanRoutine;

    public override void Spawned()
    {
        base.Spawned();

        npc = GetComponent<NPC>();

        WorldState.Steps = 0;
        WorldState.MaxSteps = 5;
        WorldState.SpeedRotation = 5f;
        WorldState.Life = 100f;
        WorldState.Mood = Waiting;
        WorldState.CarInRange = false;

        _actions = CreateActions();

        StartCoroutine(RunPlanLoop());
    }

    private void FixedUpdate()
    {
//        Debug.Log($"[{name}] CarInRange = {WorldState.CarInRange}");
//        Debug.Log($"Mi mood actual es => {WorldState?.Mood ?? "No iniciado"}");
    }


    private WorldState GetCurrentWorldState()
    {
        var (inRange, dir) = npc.IsPlayerQueryInRange(1f);

        WorldState.Impacted = inRange;
        WorldState.DirectionToFly = dir;

        Debug.Log($"Current World State Impacted {WorldState.Impacted}");

        WorldState.InteractionType = npc.currentInteractable
            ? npc.currentInteractable.type
            : InteractionType.OnlyForPath;

        WorldState.CarInRange = npc.IsInAnyPlayerQuery();

        Debug.Log($"Car In Range {WorldState.CarInRange}");

        return WorldState;
    }

    private List<GoapAction> CreateActions()
    {
        return new List<GoapAction>
        {
            new GoapAction //Idle
            {
                Name = IdleGoapNpc,
                Precondition = s => !s.CarInRange &&
                                    s.IsMoodNot(Dying, Injured, NotSafe) &&
                                    s.IsMoodOneOf(Waiting),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.Mood = LightRest;
                    ns.Steps = ns.MaxSteps;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.idleNpc),
                Cost = 3
            },

            new GoapAction //Walk
            {
                Name = WalkGoapNpc,
                Precondition = s => !s.CarInRange &&
                                    s.Steps == s.MaxSteps &&
                                    s.IsMoodNot(Dying, Injured, NotSafe) &&
                                    s.IsMoodOneOf(LightRest),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.Mood = Exploring;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.walkNpc),
                Cost = 3
            },

            new GoapAction //Talk
            {
                Name = SitdownGoapNpc,
                Precondition = s => !s.CarInRange && s.Steps == s.MaxSteps &&
                                    s.InteractionType == InteractionType.Sit &&
                                    s.IsMoodNot(Dying, Injured, NotSafe) &&
                                    s.IsMoodOneOf(LightRest),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.Mood = Relaxed;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.sitDownNpc),
                Cost = 2
            },

            new GoapAction //Sitdown
            {
                Name = TalkGoapNpc,
                Precondition = s => !s.CarInRange && s.Steps == s.MaxSteps &&
                                    s.InteractionType == InteractionType.Talk &&
                                    s.IsMoodNot(Dying, Injured, NotSafe) &&
                                    s.IsMoodOneOf(LightRest),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.Mood = Curious;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.talkNpc),
                Cost = 2
            },

            new GoapAction //Escape
            {
                Name = EscapeGoapNpc,
                Precondition = s => s.CarInRange && !s.Impacted &&
                                    s.IsMoodNot(Dying, Injured, NotSafe) &&
                                    s.IsMoodOneOf(Waiting, LightRest, Exploring, Relaxed, Curious),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.Mood = NotSafe;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.escapeNpc),
                Cost = 1
            },

            new GoapAction //Damage
            {
                Name = DamageGoapNpc,
                Precondition = s => s.Impacted &&
                                    s.IsMoodNot(Dying, Injured) &&
                                    s.IsMoodOneOf(Waiting, NotSafe, LightRest, Exploring, Relaxed, Curious),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.Mood = Injured;
                    ns.Life -= 25; //Reemplazar por un utils de Cars que diga "Damage a los NPC [Hasta entonces, asumimos que es 25]"
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.damageNpc),
                Cost = 0
            },

            new GoapAction //Death
            {
                Name = DeathGoapNpc,
                Precondition = s => s.Life <= 0f,
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.Mood = Dying;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.deathNpc),
                Cost = 0
            },
        };
    }

    private List<GoapAction> GetAvailableActions() => _actions;

    private Func<WorldState, bool> SelectGoal(WorldState state)
    {
        // 1. Condición para Death
        if (state.Life <= 0f)
            return s => s.Mood == Dying;
        
        // 2. Condición para Damage
        if (state.Impacted && state.Mood != Injured)
            return s => s.Mood == Injured;
       
        // 3. Condición para Escape
        if (state.CarInRange && state.Mood != NotSafe)
            return s => s.Mood == NotSafe; //"Safe"

        // 4. Condición para Talk
        if (state.InteractionType == InteractionType.Talk && state.Mood != Curious)
            return s => s.Mood == Curious;

        // 5. Condición para Sitdown
        if (state.InteractionType == InteractionType.Sit && state.Mood != Relaxed)
            return s => s.Mood == Relaxed;

        // 6. Condición para Walk
        if (state.Steps >= state.MaxSteps && state.Mood != Exploring)
            return s => s.Mood == Exploring;

        //7. Condición para Idle
        if (!state.CarInRange &&
            state.Steps < state.MaxSteps &&
            state.Mood == Waiting)
            return s => s.Mood == LightRest;

        // Por defecto, no hay objetivo real
        return s => s.Mood == Waiting;
    }

    private IEnumerator RunPlanLoop()
    {
        while (true)
        {
            var current = GetCurrentWorldState();

            bool needsReplanning = _currentPlan.Count == 0;

            if (needsReplanning)
            {
                var goal = SelectGoal(current);
                var plan = GoapPlanner.Plan(current, goal, GetAvailableActions()).FirstOrDefault();

                if (plan != null)
                    _currentPlan = new Queue<GoapAction>(plan);
            }

            if (_currentPlan.Count > 0)
            {
                var action = _currentPlan.Dequeue();

                if (currentPlanRoutine != null)
                    StopCoroutine(currentPlanRoutine);

                yield return currentPlanRoutine = StartCoroutine(action.Execute());
            }

            yield return null;
        }
    }

    public void ForceReplan()
    {
        _currentPlan.Clear(); // ✅ esto forzará a que se replantee el siguiente frame
    }

    
    private IEnumerator TransitionToCoroutine(IState nextState)
    {
        if (npc.Fsm == null) yield break;

        npc.Fsm.TransitionTo(nextState);
    }
}