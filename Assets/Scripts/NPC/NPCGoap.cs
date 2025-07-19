using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using Unity.Services.Analytics;
using UnityEngine;
using static MoodsNpc;
using static GoapActionName;
using BitStream = Fusion.Protocol.BitStream;

public class NPCGoap : MonoBehaviour
{
    public WorldState worldState;

    private NPC npc;
    private List<GoapAction> _actions;
    private Queue<GoapAction> _currentPlan = new();

    private Coroutine currentPlanRoutine;

    private void Awake()
    {
        npc = GetComponent<NPC>();

        if (worldState == null)
            worldState = new WorldState();

        worldState.steps = 0;
        worldState.maxsteps = 5;
        worldState.life = 100f;
        worldState.mood = Waiting;

        _actions = CreateActions();

        StartCoroutine(RunPlanLoop());
    }

    private WorldState GetCurrentWorldState()
    {
        worldState.interactionType = npc.currentInteractable != null
            ? npc.currentInteractable.type
            : InteractionType.OnlyForPath;

        //worldState.carInRange = npc.ClosestCar() != null;

        worldState.carInRange = !NodeGenerator.Instance.GetZoneForNode(npc.CurrentNode)?.IsSafe ?? false;
        
        Debug.Log("Car in Range " + $"{worldState.carInRange}");

        return worldState;
    }

    private void Update() => Debug.Log("Mi mood actual es => " + $"{worldState.mood}");

    private List<GoapAction> CreateActions()
    {
        return new List<GoapAction>
        {
            new GoapAction
            {
                Name = IdleGoapNpc,
                Precondition = s => !s.carInRange &&
                                    s.IsMoodNot(Dying, NotSafe) &&
                                    s.IsMoodOneOf(Waiting),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.mood = LightRest;
                    ns.steps = ns.maxsteps;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.idleNpc),
                Cost = 3
            },

            new GoapAction
            {
                Name = WalkGoapNpc,
                Precondition = s => !s.carInRange &&
                                    s.steps == s.maxsteps &&
                                    s.IsMoodNot(Dying, NotSafe) &&
                                    s.IsMoodOneOf(LightRest),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.mood = Exploring;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.walkNpc),
                Cost = 3
            },

            new GoapAction
            {
                Name = SitdownGoapNpc,
                Precondition = s => !s.carInRange && s.steps == s.maxsteps &&
                                    s.interactionType == InteractionType.Sit &&
                                    s.IsMoodNot(Dying, NotSafe) &&
                                    s.IsMoodOneOf(LightRest),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.mood = Relaxed;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.sitdownNpc),
                Cost = 2
            },

            new GoapAction
            {
                Name = TalkGoapNpc,
                Precondition = s => !s.carInRange && s.steps == s.maxsteps &&
                                    s.interactionType == InteractionType.Talk &&
                                    s.IsMoodNot(Dying, NotSafe) &&
                                    s.IsMoodOneOf(LightRest),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.mood = Curious;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.talkNpc),
                Cost = 2
            },
            new GoapAction
            {
                Name = EscapeGoapNpc,
                Precondition = s => s.carInRange &&
                                    s.IsMoodNot(Dying, NotSafe) &&
                                    s.IsMoodOneOf(Waiting, LightRest, Exploring, Relaxed, Curious),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.mood = NotSafe;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.escapeNpc),
                Cost = 1
            },
            new GoapAction
            {
                Name = DeathGoapNpc,
                Precondition = s => s.life <= 0f,
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.mood = Dying;
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
        // 1. NPC muerto
        if (state.life <= 0f)
            return s => s.mood == Dying;

        // 2. Escape si hay autos cerca
        if (state.carInRange && state.mood != NotSafe)
            return s => s.mood == NotSafe; //"Safe"

        //if (!state.carInRange && state.mood == NotSafe)
        //    return s => s.mood == Waiting;

        // 3. Charla si no está curioso
        if (state.interactionType == InteractionType.Talk && state.mood != Curious)
            return s => s.mood == Curious;

        // 4. Sentarse si no está relajado
        if (state.interactionType == InteractionType.Sit && state.mood != Relaxed)
            return s => s.mood == Relaxed;

        // 5. Si aún no exploró
        if (state.steps >= state.maxsteps && state.mood != Exploring)
            return s => s.mood == Exploring;

        //6. Si tengo que recargar pasos despeus de una accion
        if (!state.carInRange &&
            state.steps < state.maxsteps &&
            state.mood == Waiting)
            return s => s.mood == LightRest;

        // Por defecto, no hay objetivo real
        return s => s.mood == Waiting;
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
                // else
                //     Debug.Log("No plan found.");
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


    private IEnumerator TransitionToCoroutine(IState nextState)
    {
        npc.fsm.TransitionTo(nextState);
        yield break;
    }
}