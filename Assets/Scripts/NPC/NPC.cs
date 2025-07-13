using System;
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
    [HideInInspector] public Node currentNode;
    [HideInInspector] public Animator animator;

    [Header("Interacción")]
    //public float interactionRange = 2f;
    private HashSet<InteractableNPC> _interactablesInRange = new();

    private HashSet<CharacterController> _carsInRange = new();

    private const int CarDistance = 5;
    private float _life;

    //public Transform target;
    public float speed;
    public float speedRotation;

    public FiniteStateMachine fsm;

    [SerializeField] internal Idle_NPC idleNpc;
    [SerializeField] internal Walk_NPC walkNpc;
    [SerializeField] internal Escape_NPC escapeNpc;
    [SerializeField] internal Talk_NPC talkNpc;
    [SerializeField] internal Sitdown_NPC sitdownNpc;
    [SerializeField] internal Death_NPC deathNpc;

    private void Start()
    {
        fsm = new FiniteStateMachine(idleNpc, StartCoroutine);

        // Seguridad por si algún estado no está asignado
        if (idleNpc == null) Debug.LogError("[NPC] Estado Idle no asignado.");
        if (walkNpc == null) Debug.LogWarning("[NPC] Estado Walk no asignado.");
        if (escapeNpc == null) Debug.LogWarning("[NPC] Estado Escape no asignado.");

        fsm.Active = true;
        fsm.TransitionTo(idleNpc);
    }

    private void Update() => fsm.Update(); // Llama al estado actual (solo UpdateLoop y ProcessInput)

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

#endregion