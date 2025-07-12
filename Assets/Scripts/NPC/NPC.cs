using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using static MoodsNpc;
using static GoapActionName;
using UnityEngine;

/*Gestión de colisiones y estados cercanos (correcto aquí).
FSM para manejar los estados específicos del NPC (Idle, Walk, etc.) (correcto aquí).

Lógica GOAP (objetivos, acciones, planificación) (conviene separarlo).*/
public class NPC : MonoBehaviour
{
    [Header("Interacción")]
    //public float interactionRange = 2f;
    private HashSet<InteractableNPC> _interactablesInRange = new();

    private HashSet<CharacterController> _carsInRange = new();

    private Queue<GoapAction> _currentPlan = new();

    private const int CarDistance = 5;

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
    
    public WorldState worldState;

    //Pathfinding
    
    List<GameObject> nodes = NodeGenerator.Instance.GetNodes();
    
    private void Start()
    {
        _fsm = new FiniteStateMachine(idleNpc, StartCoroutine);

        // Activamos la FSM
        _fsm.Active = true;

        _actions = CreateActions();
        
        // Disparamos el primer plan
        StartCoroutine(RunPlanLoop());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<InteractableNPC>(out var interactable))
        {
            if (interactable.type != InteractionType.OnlyForPath)
                _interactablesInRange.Add(interactable);
        }

        if (other.gameObject.TryGetComponent<CharacterController>(out var car))
            _carsInRange.Add(car);
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<InteractableNPC>(out var interactable))
        {
            if (interactable.type != InteractionType.OnlyForPath)
                _interactablesInRange.Remove(interactable);
        }

        if (other.gameObject.TryGetComponent<CharacterController>(out var car))
            _carsInRange.Remove(car);
    }

    /// <summary>
    /// Devuelve el interactable mas cercano al NPC basandose en su distancia
    /// </summary>
    /// <returns></returns>
    private InteractableNPC GetClosestInteractable()
    {
        return _interactablesInRange
            .Where(obj => obj != null)
            .OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position))
            .FirstOrDefault();
    }

    /// <summary>
    /// Verifica si hay algun vehiculo cerca del NPC, basandose en su CarDistance
    /// </summary>
    /// <returns></returns>
    private bool HasCarNearby => _carsInRange.Any(car =>
        car != null && Vector3.Distance(transform.position, car.transform.position) < CarDistance);

    /// <summary>
    /// Obtenemos el estado actual del Mundo
    /// </summary>
    /// <returns></returns>
    public WorldState GetCurrentWorldState()
    {
        var carNearby = HasCarNearby;
        var interactionType = GetClosestInteractable()?.type ?? InteractionType.OnlyForPath;

        // Actualizamos datos dinámicos
        worldState.carInRange = carNearby;
        worldState.interactionType = interactionType;
        worldState.mood = (worldState.mood == NotSafe && !carNearby) ? Safe : worldState.mood;

        return worldState;
    }

    /// <summary>
    /// Creamos las acciones correspondientes a nuestro plan de GOAP
    /// </summary>
    /// <returns></returns>
    public List<GoapAction> CreateActions()
    {
        return new List<GoapAction>
        {
            new GoapAction
            {
                Name = IdleGoapNpc,
                Precondition = s => !s.carInRange && s.steps == 0,
                Effect = s => {
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
        _fsm.TransitionTo(nextState);
        yield break;
    }
}

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