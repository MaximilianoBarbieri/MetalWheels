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
    [SerializeField] private Idle_NPC idleNpc;
    [SerializeField] private Walk_NPC walkNpc;
    [SerializeField] private Escape_NPC escapeNpc;
    [SerializeField] private Talk_NPC talkNpc;
    [SerializeField] private Sitdown_NPC sitdownNpc;
    [SerializeField] private Death_NPC deathNpc;

    private Queue<GoapAction> _currentPlan = new();

    private List<GoapAction> _actions;

    public WorldState worldState;

    private NPC npc;

    private void Awake() => npc = GetComponent<NPC>();

    private void Start()
    {
        _actions = CreateActions();

        StartCoroutine(RunPlanLoop());
    }

    private WorldState GetCurrentWorldState()
    {
        var carNearby = npc.HasCarNearby;
        var interactionType = npc.GetClosestInteractable()?.type ?? InteractionType.OnlyForPath;

        // Actualizamos datos dinámicos
        worldState.carInRange = carNearby;
        worldState.interactionType = interactionType;
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
                Precondition = s => !s.carInRange && s.steps == 0,
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.steps = Mathf.Min(ns.steps + 3, ns.maxsteps); // Simula ganancia
                    return ns;
                },
                Execute = () => TransitionToCoroutine(idleNpc),
                Cost = 4
            },
            new GoapAction
            {
                Name = WalkGoapNpc,
                Precondition = s => !s.carInRange && s.steps > 0,
                Effect = s =>
                    s.Clone(), //PERDER STEPS -> PARA LLEGAR A LA POSICION DESEADA TODO: SI TENGO UN STOCK DE 20 STEPS, QUIERO QUE TOME UN VALOR RANDOM DE 0 HASTA SU MAXVALUE PARA MOVERSE
                Execute = () => (TransitionToCoroutine(walkNpc)),
                Cost = 3
            },
            new GoapAction
            {
                Name = TalkGoapNpc,
                Precondition = s => !s.carInRange && s.interactionType == InteractionType.Talk && s.mood != Curious,
                Effect =
                    s => //QUEDARSE QUIETO Y SIMULAR UNA CONVERSACION CON UN "FALSO" NPC TODO: ESTOS PODRIAN SPAWNEAR LAS "CONVERSACIONES"/"GLOBOS DE TEXTO" COMO UNICA RESPONSABILIDAD
                    {
                        var ns = s.Clone();
                        ns.mood = Curious;
                        return ns;
                    },
                Execute = () => (TransitionToCoroutine(talkNpc)),
                Cost = 2
            },
            new GoapAction
            {
                Name = SitdownGoapNpc,
                Precondition = s => !s.carInRange && s.interactionType == InteractionType.Sit && s.mood != Relaxed,
                Effect = s => //ME MANTENGO 5 SEGUNDOS, PERO RECUPERO TODOS MIS STEPS + RECUPERO TODA MI STAMINA
                {
                    var ns = s.Clone();
                    ns.mood = Relaxed;
                    return ns;
                },
                Execute = () => (TransitionToCoroutine(sitdownNpc)),
                Cost = 2
            },
            new GoapAction
            {
                Name = EscapeGoapNpc,
                Precondition = s => s.carInRange && s.mood != Safe,
                Effect =
                    s => //TODO: DIVIDIR LA ESCENA EN SEGMENTOS, PARA ESCAPAR, DEBO ENCONTRAR UN SEGMENTO DONDE NO HAYA NINGUN VEHICULO 
                    {
                        var ns = s.Clone();
                        ns.mood = Safe;
                        return ns;
                    },
                Execute = () => (TransitionToCoroutine(escapeNpc)),
                Cost = 1
            },
            new GoapAction
            {
                Name = DeathGoapNpc,
                Precondition = s => s.life <= 0f,
                Effect = s => s.Clone(),
                Execute = () => (TransitionToCoroutine(deathNpc)),
                Cost = 0
            },
        };
    }

    private List<GoapAction> GetAvailableActions() => _actions;

    private Func<WorldState, bool> SelectGoal(WorldState state)
    {
        // 1. Si la vida es 0 o menos, el objetivo es permanecer muerto.
        //    Así el NPC no intentará ninguna acción posterior.
        if (state.life <= 0f)
            return s => s.life <= 0f; // Meta: estar muerto (meta terminal)

        // 2. Si hay un auto cerca y aún no está a salvo, el objetivo es ponerse a salvo.
        //    Esto fuerza a que escape, ignorando otras acciones hasta estar seguro.
        if (state.carInRange && state.mood == NotSafe)
            return s => s.mood == Safe; // Meta: estar a salvo

        // 3. Si hay una oportunidad de hablar y aún no habló, el objetivo es completar esa charla.
        if (state.interactionType == InteractionType.Talk && state.mood != Curious)
            return s => s.mood == Curious; // Meta: haber hablado

        // 4. Si puede sentarse y aún no está sentado, el objetivo es sentarse.
        if (state.interactionType == InteractionType.Sit && state.mood != Relaxed)
            return s => s.mood == Relaxed; // Meta: estar sentado

        // 5. Meta por defecto: Simplemente seguir existiendo y no hacer nada especial.
        return s => true; // No hay meta "de mejora", solo existir.
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