using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using static MoodsNpc;
using static GoapActionName;

public class NPCGoap : NetworkBehaviour, IGridEntity
{
    public WorldState WorldState;

    private NPC npc;
    private List<GoapAction> _actions;
    private Queue<GoapAction> _currentPlan = new();

    private Coroutine currentPlanRoutine;

    public event Action<IGridEntity> OnMove;

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    [SerializeField] private SpatialGrid spatialGrid;

    public override void Spawned()
    {
        base.Spawned();

        npc = GetComponent<NPC>();

        WorldState ??= new WorldState();

        WorldState.Steps = 0;
        WorldState.MaxSteps = 5;
        WorldState.SpeedRotation = 5f;
        WorldState.Life = 100f;
        WorldState.Mood = Waiting;

        _actions = CreateActions();

        StartCoroutine(RunPlanLoop());
    }

    private void Start()
    {
        if (spatialGrid.isInitialized)
            spatialGrid.UpdateEntity(this);
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        OnMove?.Invoke(this);
    }

    private void FixedUpdate()
    {
//        Debug.Log($"Mi mood actual es => {WorldState?.Mood ?? "No iniciado"}");
    }

    private WorldState GetCurrentWorldState()
    {
        WorldState.InteractionType = npc.CurrentInteractable
            ? npc.CurrentInteractable.type
            : InteractionType.OnlyForPath;

        // WorldState.CarInRange = !NodeGenerator.Instance?.GetZoneForNode(npc.CurrentNode)!?.IsSafe ?? false;

        WorldState.CurrentCar ??= npc.GetClosestCarIfHit();
        WorldState.Impacted = WorldState.CurrentCar;

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
                Precondition = s => s.CarInRange &&
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

        // 2. Condición para Escape
        if (state.CarInRange && state.Mood != NotSafe)
            return s => s.Mood == NotSafe; //"Safe"

        // 3. Condición para Damage
        if (state.Impacted && state.Mood != Injured)
            return s => s.Mood == Injured;

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

    private IEnumerator TransitionToCoroutine(IState nextState)
    {
        if (npc.Fsm == null) yield break;

        npc.Fsm.TransitionTo(nextState);
    }
}