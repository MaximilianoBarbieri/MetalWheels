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
    
    private const int CarDistance = 5;

    private float _life;

    public Transform target;
    public Animator animator;
    public float speed;
    public float speedRotation;

    public FiniteStateMachine fsm;

    [SerializeField] private Idle_NPC idleNpc;
    [SerializeField] private Walk_NPC walkNpc;
    [SerializeField] private Escape_NPC escapeNpc;
    [SerializeField] private Talk_NPC talkNpc;
    [SerializeField] private Sitdown_NPC sitdownNpc;
    [SerializeField] private Death_NPC deathNpc;
    private Queue<GoapAction> _currentPlan = new();
    private List<GoapAction> _actions;
    public WorldState worldState;

    //Pathfinding
    
    List<GameObject> nodes = NodeGenerator.Instance.GetNodes();
    
    private void Start()
    {
        fsm = new FiniteStateMachine(idleNpc, StartCoroutine);

        // Activamos la FSM
        fsm.Active = true;
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
    public InteractableNPC GetClosestInteractable()
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
    public bool HasCarNearby => _carsInRange.Any(car =>
        car != null && Vector3.Distance(transform.position, car.transform.position) < CarDistance);
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