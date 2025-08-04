using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using Fusion;
using UnityEngine;
using static MoodsNpc;
using static GoapActionName;

public class NPCGoap : NetworkBehaviour
{
    private NPC _npc;

    public WorldState WorldState = new();
    private WorldState _lastState;

    private Coroutine _currentPlanRoutine;

    private List<GoapAction> _actions;
    private Queue<GoapAction> _currentPlan = new();

    public override void Spawned()
    {
        base.Spawned();

        _npc = GetComponent<NPC>();

        WorldState.Steps = 0;
        WorldState.MaxSteps = 5;
        WorldState.SpeedRotation = 5f;
        WorldState.Life = 100f;
        WorldState.Mood = Waiting;
        WorldState.CarInRange = false;

        _actions = CreateActions();

        StartCoroutine(RunPlanLoop());
    }

    private WorldState GetCurrentWorldState()
    {
        var (inRange, dir) = _npc.IsPlayerQueryInRange(1f);

        WorldState.Impacted = inRange;
        
        WorldState.DirectionToFly = dir;
        
        WorldState.InteractionType = _npc.currentInteractable ? _npc.currentInteractable.type
                                                                   : InteractionType.OnlyForPath;
        WorldState.CarInRange = _npc.IsInAnyPlayerQuery();
        
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
                Execute = () => TransitionToCoroutine(_npc.idleNpc),
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
                Execute = () => TransitionToCoroutine(_npc.walkNpc),
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
                Execute = () => TransitionToCoroutine(_npc.sitDownNpc),
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
                Execute = () => TransitionToCoroutine(_npc.talkNpc),
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
                Execute = () => TransitionToCoroutine(_npc.escapeNpc),
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
                    return ns;
                },
                Execute = () => TransitionToCoroutine(_npc.damageNpc),
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
                Execute = () => TransitionToCoroutine(_npc.deathNpc),
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

            bool needsReplanning = _currentPlan.Count == 0 || WorldStateChanged(current, _lastState);

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

                if (_currentPlanRoutine != null)
                    StopCoroutine(_currentPlanRoutine);

                yield return _currentPlanRoutine = StartCoroutine(action.Execute());
            }

            _lastState = current.Clone(); 
            yield return null;
        }
    }

    private bool WorldStateChanged(WorldState a, WorldState b) => a.Life != b.Life ||
                                                                  a.Steps != b.Steps ||
                                                                  a.CarInRange != b.CarInRange ||
                                                                  a.Impacted != b.Impacted ||
                                                                  a.Mood != b.Mood ||
                                                                  a.InteractionType != b.InteractionType;
    
    private IEnumerator TransitionToCoroutine(IState nextState)
    {
        if (_npc.Fsm == null) yield break;

        _npc.Fsm.TransitionTo(nextState);
    }
}