using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using static MoodsNpc;
using static GoapActionName;
using UnityEngine;

public class NPCGoap : MonoBehaviour
{
    private Queue<GoapAction> _currentPlan = new();

    private List<GoapAction> _actions;

    [SerializeField] public WorldState worldState;

    private NPC npc;

    private void Awake()
    {
        npc = GetComponent<NPC>();

        if (worldState == null)
            worldState = new WorldState();

        worldState.steps = 0; // o el valor que quieras
        worldState.maxsteps = 5;
        worldState.life = 100f;
        worldState.mood = Safe; // o Safe, según el caso

        _actions = CreateActions();

        StartCoroutine(RunPlanLoop());
    }

    private WorldState GetCurrentWorldState()
    {
        var carNearby = npc.HasCarNearby;

        // Actualizamos datos dinámicos
        worldState.interactionType = npc.currentInteractable != null
            ? npc.currentInteractable.type
            : InteractionType.OnlyForPath;

        worldState.carInRange = carNearby;
        //worldState.interactionType = interactionType;
        worldState.mood = (worldState.mood == NotSafe && !carNearby) ? Safe : worldState.mood;

        return worldState;
    }

    private List<GoapAction> CreateActions()
    {
        return new List<GoapAction>
        {
            new GoapAction
            {
                Name = IdleGoapNpc,
                Precondition = s => !s.carInRange && s.steps < s.maxsteps && s.mood != LightRest,
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
                Precondition = s => !s.carInRange && s.steps == s.maxsteps && s.mood == LightRest,
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.steps = 0;
                    ns.mood = Exploring;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.walkNpc),
                Cost = 3
            },

            new GoapAction
            {
                Name = SitdownGoapNpc,
                Precondition = s =>
                    !s.carInRange && s.steps == s.maxsteps && s.interactionType == InteractionType.Sit &&
                    s.mood == LightRest,
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.mood = Relaxed;
                    //ns.steps = ns.maxsteps; // Recuperación completa al sentarse
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.sitdownNpc),
                Cost = 2
            },

            new GoapAction
            {
                Name = TalkGoapNpc,
                Precondition = s =>
                    !s.carInRange && s.steps == s.maxsteps && s.interactionType == InteractionType.Talk &&
                    s.mood == LightRest,
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
                Precondition = s => s.carInRange && s.mood == NotSafe,
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.mood = Safe;
                    return ns;
                },
                Execute = () => TransitionToCoroutine(npc.escapeNpc),
                Cost = 1 // Prioridad alta
            },

            new GoapAction
            {
                Name = DeathGoapNpc,
                Precondition = s => s.life <= 0f,
                Effect = s => s.Clone(),
                Execute = () => TransitionToCoroutine(npc.deathNpc),
                Cost = 0 // Máxima prioridad
            }
        };
    }

    private List<GoapAction> GetAvailableActions() => _actions;

    // private Func<WorldState, bool> SelectGoal(WorldState state)
    // {
    //     // 1. Si la vida es 0 o menos, el objetivo es permanecer muerto.
    //     //    Así el NPC no intentará ninguna acción posterior.
    //     if (state.life <= 0f)
    //         return s => s.life <= 0f; // Meta: estar muerto (meta terminal)
    //
    //     // 2. Si hay un auto cerca y aún no está a salvo, el objetivo es ponerse a salvo.
    //     //    Esto fuerza a que escape, ignorando otras acciones hasta estar seguro.
    //     if (state.carInRange && state.mood == NotSafe)
    //         return s => s.mood == Safe; // Meta: estar a salvo
    //
    //     // 3. Si hay una oportunidad de hablar y aún no habló, el objetivo es completar esa charla.
    //     if (state.interactionType == InteractionType.Talk && state.mood != Curious)
    //         return s => s.mood == Curious; // Meta: haber hablado
    //
    //     // 4. Si puede sentarse y aún no está sentado, el objetivo es sentarse.
    //     if (state.interactionType == InteractionType.Sit && state.mood != Relaxed)
    //         return s => s.mood == Relaxed; // Meta: estar sentado
    //
    //     // 5. Meta por defecto: si tengo steps completos, quiero moverme.
    //     if (state.steps >= state.maxsteps)
    //         return s => s.steps < state.maxsteps; // Meta: haber gastado steps
    //
    //     if (state.steps == 0)
    //         return s => s.steps > 0;
    //
    //     // 6. Si no hay nada más que hacer, no tengo meta real.
    //     return s => true;
    // }

    private Func<WorldState, bool> SelectGoal(WorldState state)
    {
        // 1. NPC muerto
        if (state.life <= 0f)
            return s => s.life <= 0f;

        // 2. Escape si hay autos cerca
        if (state.carInRange && state.mood != Safe)
            return s => s.mood == Safe;

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
        if (state.steps < state.maxsteps && state.mood != LightRest)
            return s => s.mood == LightRest && s.steps == s.maxsteps;

        // Por defecto, no hay objetivo real
        return s => true;
    }


    private IEnumerator RunPlanLoop()
    {
        while (true)
        {
            var current = GetCurrentWorldState();

            // Verificamos condiciones críticas
            if ((current.carInRange && current.mood == NotSafe))
                _currentPlan.Clear(); // Cancelamos el plan actual

            if (_currentPlan.Count == 0)
            {
                var goal = SelectGoal(current);
                var plan = GoapPlanner.Plan(current, goal, GetAvailableActions()).FirstOrDefault();

                if (plan != null)
                    _currentPlan = new Queue<GoapAction>(plan);
                else
                    Debug.Log("No plan found.");
            }

            if (_currentPlan.Count > 0)
            {
                var action = _currentPlan.Dequeue();
                yield return StartCoroutine(action.Execute());
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