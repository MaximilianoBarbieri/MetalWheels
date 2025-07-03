using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using static MoodsNpc;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Interacción")] public float interactionRange = 2f;
    private HashSet<GameObject> _interactablesInRange = new();

    private float _life;

    public Transform target;
    public Animator animator;
    public float speed;
    public float speedRotation;

    private FiniteStateMachine _fsm;

    [SerializeField] private Idle_NPC idleNpc;
    [SerializeField] private Walk_NPC walkNpc;
    [SerializeField] private Escape_NPC escapeNpc;
    [SerializeField] private Talk_NPC talkNpc;
    [SerializeField] private Sitdown_NPC sitdownNpc;
    [SerializeField] private Death_NPC deathNpc;

    private List<GoapAction> _actions;

    private void Start()
    {
        _fsm = new FiniteStateMachine(idleNpc, StartCoroutine);

        // Activamos la FSM
        _fsm.Active = true;

        _actions = CreateActions();
        // Disparamos el primer plan
        StartCoroutine(RunPlanLoop());

        #region OldFSM

        // _fsm.AddTransition(Utils.ToIdleNpc, walkNpc, idleNpc);
        // _fsm.AddTransition(Utils.ToIdleNpc, escapeNpc, idleNpc);
        // _fsm.AddTransition(Utils.ToIdleNpc, talkNpc, idleNpc);
        // _fsm.AddTransition(Utils.ToIdleNpc, sitdownNpc, idleNpc);
        //
        // _fsm.AddTransition(Utils.ToWalkNpc, idleNpc, walkNpc);
        // _fsm.AddTransition(Utils.ToWalkNpc, escapeNpc, walkNpc);
        // _fsm.AddTransition(Utils.ToWalkNpc, talkNpc, walkNpc);
        // _fsm.AddTransition(Utils.ToWalkNpc, sitdownNpc, walkNpc);
        //
        // _fsm.AddTransition(Utils.ToEscapeNpc, idleNpc, escapeNpc);
        // _fsm.AddTransition(Utils.ToEscapeNpc, walkNpc, escapeNpc);
        // _fsm.AddTransition(Utils.ToEscapeNpc, talkNpc, escapeNpc);
        // _fsm.AddTransition(Utils.ToEscapeNpc, sitdownNpc, escapeNpc);
        //
        // _fsm.AddTransition(Utils.ToTalkNpc, idleNpc, talkNpc);
        // _fsm.AddTransition(Utils.ToTalkNpc, walkNpc, talkNpc);
        // _fsm.AddTransition(Utils.ToTalkNpc, escapeNpc, talkNpc);
        // _fsm.AddTransition(Utils.ToTalkNpc, sitdownNpc, talkNpc);
        //
        // _fsm.AddTransition(Utils.ToSitdownNpc, idleNpc, sitdownNpc);
        // _fsm.AddTransition(Utils.ToSitdownNpc, walkNpc, sitdownNpc);
        // _fsm.AddTransition(Utils.ToSitdownNpc, escapeNpc, sitdownNpc);
        // _fsm.AddTransition(Utils.ToSitdownNpc, talkNpc, sitdownNpc);
        //
        // _fsm.AddTransition(Utils.ToDeathNpc, idleNpc, deathNpc);
        // _fsm.AddTransition(Utils.ToDeathNpc, walkNpc, deathNpc);
        // _fsm.AddTransition(Utils.ToDeathNpc, escapeNpc, deathNpc);
        // _fsm.AddTransition(Utils.ToDeathNpc, talkNpc, deathNpc);
        // _fsm.AddTransition(Utils.ToDeathNpc, sitdownNpc, deathNpc);

        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent<InteractableNPC>(out var interactable))
            return;

        if (interactable.type != InteractionType.NoInteractable)
            _interactablesInRange.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.TryGetComponent<InteractableNPC>(out var interactable))
            return;

        if (interactable.type != InteractionType.NoInteractable)
            _interactablesInRange.Remove(other.gameObject);
    }

    public GameObject GetClosestInteractable() //TODO: PODRIA INTEGRARLO A UN SPATIAL GRID
    {
        return _interactablesInRange
            .Where(obj => obj != null)
            .OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position))
            .FirstOrDefault();
    }

    public WorldState GetCurrentWorldState()
    {
        var closest = GetClosestInteractable();
        var type = closest ? closest.GetComponent<InteractableNPC>()?.type : null;

        return new WorldState
        {
            life = _life,
            carInRange = closest != null,
            interactionType = type
        };
    }

    public List<GoapAction> CreateActions()
    {
        return new List<GoapAction>
        {
            new GoapAction
            {
                Name = "Idle",
                Precondition = s => !s.carInRange && s.steps == 0,
                Effect = s => s.Clone(), //AWAIT 3 SEG -> RECUPERO 3 STEPS DE GOLPE
                Execute = () => (TransitionToCoroutine(idleNpc)),
                Cost = 4
            },
            new GoapAction
            {
                Name = "Walk",
                Precondition = s => !s.carInRange && s.steps > 0,
                Effect = s =>
                    s.Clone(), //PERDER STEPS -> PARA LLEGAR A LA POSICION DESEADA TODO: SI TENGO UN STOCK DE 20 STEPS, QUIERO QUE TOME UN VALOR RANDOM DE 0 HASTA SU MAXVALUE PARA MOVERSE
                Execute = () => (TransitionToCoroutine(walkNpc)),
                Cost = 3
            },
            new GoapAction
            {
                Name = "Talk",
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
                Name = "Sitdown",
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
                Name = "Escape",
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
                Name = "Death",
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
        if (state.carInRange && state.mood != Safe)
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

    private Queue<GoapAction> _currentPlan = new();

    public IEnumerator RunPlanLoop()
    {
        while (true)
        {
            var current = GetCurrentWorldState();

            // Verificamos condiciones críticas
            if ((current.carInRange && current.mood != Safe))
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
        _fsm.TransitionTo(nextState);
        yield break;
    }
}