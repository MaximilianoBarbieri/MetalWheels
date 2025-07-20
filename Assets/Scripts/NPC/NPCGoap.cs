using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using UnityEngine;
using static MoodsNpc;
using static GoapActionName;

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

        worldState.Steps = 0;
        worldState.MaxSteps = 5;
        worldState.SpeedRotation = 5f;
        worldState.Life = 100f;
        worldState.Mood = Waiting;

        _actions = CreateActions();

        StartCoroutine(RunPlanLoop());
    }

    private void Update() => Debug.Log("Mi mood actual es => " + $"{worldState.Mood}");

    private WorldState GetCurrentWorldState()
    {
        worldState.InteractionType = npc.currentInteractable != null
            ? npc.currentInteractable.type
            : InteractionType.OnlyForPath;

        worldState.CarInRange = !NodeGenerator.Instance.GetZoneForNode(npc.CurrentNode)?.IsSafe ?? false;

        return worldState;
    }

    private List<GoapAction> CreateActions()
    {
        return new List<GoapAction>
        {
            new GoapAction
            {
                Name = IdleGoapNpc,
                Precondition = s => !s.CarInRange &&
                                    s.IsMoodNot(Dying, NotSafe) &&
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

            new GoapAction
            {
                Name = WalkGoapNpc,
                Precondition = s => !s.CarInRange &&
                                    s.Steps == s.MaxSteps &&
                                    s.IsMoodNot(Dying, NotSafe) &&
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

            new GoapAction
            {
                Name = SitdownGoapNpc,
                Precondition = s => !s.CarInRange && s.Steps == s.MaxSteps &&
                                    s.InteractionType == InteractionType.Sit &&
                                    s.IsMoodNot(Dying, NotSafe) &&
                                    s.IsMoodOneOf(LightRest),
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.Mood = Relaxed;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.sitdownNpc),
                Cost = 2
            },

            new GoapAction
            {
                Name = TalkGoapNpc,
                Precondition = s => !s.CarInRange && s.Steps == s.MaxSteps &&
                                    s.InteractionType == InteractionType.Talk &&
                                    s.IsMoodNot(Dying, NotSafe) &&
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
            new GoapAction
            {
                Name = EscapeGoapNpc,
                Precondition = s => s.CarInRange &&
                                    s.IsMoodNot(Dying, NotSafe) &&
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
            new GoapAction
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
        // 1. NPC muerto
        if (state.Life <= 0f)
            return s => s.Mood == Dying;

        // 2. Escape si hay autos cerca
        if (state.CarInRange && state.Mood != NotSafe)
            return s => s.Mood == NotSafe; //"Safe"

        //if (!state.carInRange && state.mood == NotSafe)
        //    return s => s.mood == Waiting;

        // 3. Charla si no está curioso
        if (state.InteractionType == InteractionType.Talk && state.Mood != Curious)
            return s => s.Mood == Curious;

        // 4. Sentarse si no está relajado
        if (state.InteractionType == InteractionType.Sit && state.Mood != Relaxed)
            return s => s.Mood == Relaxed;

        // 5. Si aún no exploró
        if (state.Steps >= state.MaxSteps && state.Mood != Exploring)
            return s => s.Mood == Exploring;

        //6. Si tengo que recargar pasos despeus de una accion
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