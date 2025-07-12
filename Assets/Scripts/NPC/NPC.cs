using System.Collections.Generic;
using System.Linq;
using FSM;
using static StatesNpc;
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

    //public Transform target;
    public Animator animator;
    public float speed;
    public float speedRotation;

    public FiniteStateMachine fsm;

    [SerializeField] internal Idle_NPC idleNpc;
    [SerializeField] internal Walk_NPC walkNpc;
    [SerializeField] internal Escape_NPC escapeNpc;
    [SerializeField] internal Talk_NPC talkNpc;
    [SerializeField] internal Sitdown_NPC sitdownNpc;
    [SerializeField] internal Death_NPC deathNpc;

    //Pathfinding

    //List<GameObject> nodes = NodeGenerator.Instance.GetNodes();

    private void Start()
    {
        currentNode = NodeGenerator.Instance.GetNodes()
            .Select(go => go.GetComponent<Node>())
            .OrderBy(n => Vector3.Distance(transform.position, n.transform.position))
            .FirstOrDefault(); // Asigna el nodo más cercano

        // Activamos la FSM
        fsm = new FiniteStateMachine(idleNpc, StartCoroutine);
        
        initFSM();
        
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

    public Node currentNode;

    void initFSM()
    {
        fsm.AddTransition(ToIdleNpc, walkNpc, idleNpc);
        fsm.AddTransition(ToIdleNpc, escapeNpc, idleNpc);
        fsm.AddTransition(ToIdleNpc, talkNpc, idleNpc);
        fsm.AddTransition(ToIdleNpc, sitdownNpc, idleNpc);

        fsm.AddTransition(ToWalkNpc, idleNpc, walkNpc);
        fsm.AddTransition(ToWalkNpc, escapeNpc, walkNpc);
        fsm.AddTransition(ToWalkNpc, talkNpc, walkNpc);
        fsm.AddTransition(ToWalkNpc, sitdownNpc, walkNpc);

        fsm.AddTransition(ToEscapeNpc, idleNpc, escapeNpc);
        fsm.AddTransition(ToEscapeNpc, walkNpc, escapeNpc);
        fsm.AddTransition(ToEscapeNpc, talkNpc, escapeNpc);
        fsm.AddTransition(ToEscapeNpc, sitdownNpc, escapeNpc);

        fsm.AddTransition(ToTalkNpc, idleNpc, talkNpc);
        fsm.AddTransition(ToTalkNpc, walkNpc, talkNpc);
        fsm.AddTransition(ToTalkNpc, escapeNpc, talkNpc);
        fsm.AddTransition(ToTalkNpc, sitdownNpc, talkNpc);

        fsm.AddTransition(ToSitdownNpc, idleNpc, sitdownNpc);
        fsm.AddTransition(ToSitdownNpc, walkNpc, sitdownNpc);
        fsm.AddTransition(ToSitdownNpc, escapeNpc, sitdownNpc);
        fsm.AddTransition(ToSitdownNpc, talkNpc, sitdownNpc);

        fsm.AddTransition(ToDeathNpc, idleNpc, deathNpc);
        fsm.AddTransition(ToDeathNpc, walkNpc, deathNpc);
        fsm.AddTransition(ToDeathNpc, escapeNpc, deathNpc);
        fsm.AddTransition(ToDeathNpc, talkNpc, deathNpc);
        fsm.AddTransition(ToDeathNpc, sitdownNpc, deathNpc);
    }
}

#region OldFSM

 

#endregion