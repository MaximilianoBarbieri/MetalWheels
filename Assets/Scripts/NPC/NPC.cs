using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class NPC : NetworkBehaviour, IGridEntity
{
    public Node CurrentNode => GetCurrentNode();
    public Rigidbody Rigidbody => GetComponent<Rigidbody>();
    public Animator Animator => GetComponent<Animator>();
    private static SpatialGrid SpatialGrid => FindObjectOfType<SpatialGrid>();

    public event Action<IGridEntity> OnMove;

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }
    
    [Header("Interacción")] 
    
    public InteractableNPC currentInteractable;

    private HashSet<InteractableNPC> _interactablesInRange = new();
    private List<CharacterController> _carsInRange = new();

    [Header("Estados de FSM")] 
    
    public FiniteStateMachine Fsm;

    [SerializeField] internal Idle_NPC idleNpc;
    [SerializeField] internal Walk_NPC walkNpc;
    [SerializeField] internal Sitdown_NPC sitDownNpc;
    [SerializeField] internal Talk_NPC talkNpc;
    [SerializeField] internal Escape_NPC escapeNpc;
    [SerializeField] internal Damage_NPC damageNpc;
    [SerializeField] internal Death_NPC deathNpc;

    private void Start()
    {
        if (SpatialGrid.isInitialized) SpatialGrid.UpdateEntity(this);
    }

    public override void Spawned() => Fsm = new FiniteStateMachine(idleNpc, StartCoroutine);

    private void Update()
    {
        OnMove?.Invoke(this);
        Fsm?.Update();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<InteractableNPC>(out var interactable))
        {
            if (interactable.type != InteractionType.OnlyForPath)
                _interactablesInRange.Add(interactable);
        }

        if (other.gameObject.TryGetComponent<CharacterController>(out var car))
        {
            if (!_carsInRange.Contains(car))
                _carsInRange.Add(car);
        }
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

    public bool IsInAnyPlayerQuery() => SpatialGrid?.players.Any(p => p.CurrentlyInDanger
        .Contains(this)) == true;
    
    public (bool inRange, Vector3 player) IsPlayerQueryInRange(float maxDistance)
    {
        foreach (var playerQuery in SpatialGrid?.players)
        {
            var controller = playerQuery.gameObject;
            
            if (Vector3.Distance(transform.position, controller.transform.position) <= maxDistance)
            {
            
                Debug.Log($"La distancia entre {controller} y {name} es {Vector3.Distance(transform.position, controller.transform.position) <= maxDistance} ");
    
                return (true, controller.transform.position);
            }
        }

        return (false, Vector3.zero);
    }

    /// <summary>
    /// Devuelve el nodo mas cercano
    /// </summary>
    /// <returns></returns>
    private Node GetCurrentNode()
    {
        var colliders = Physics.OverlapSphere(transform.position, 5f);

        return colliders
            .Select(c => c.GetComponent<Node>())
            .Where(n => n != null)
            .OrderBy(n => Vector3.Distance(transform.position, n.transform.position))
            .FirstOrDefault();
    }

    /// <summary>
    /// Devuelve el interactable mas cercano
    /// </summary>
    /// <returns></returns>
    public InteractableNPC GetClosestInteractable() =>
        _interactablesInRange
            .Where(obj => obj != null)
            .OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position))
            .FirstOrDefault();

    /// <summary>
    /// Devuelve el vehiculo mas cercano
    /// </summary>
    /// <returns></returns>
    public CharacterController GetClosestCarIfHit()
    {
        var car = _carsInRange
            .Where(c => c != null)
            .OrderBy(c => Vector3.Distance(transform.position, c.transform.position))
            .FirstOrDefault();

        if (car == null)
            return null;

        float distance = Vector3.Distance(transform.position, car.transform.position);
        return (distance <= 0.75f) ? car : null;
    }

    /// <summary>
    /// Generacion del camino para AStart
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="goalNode"></param>
    /// <returns></returns>
    private IEnumerator GeneratePath(Node start, Node goal, Action<List<Node>> onPathFound)
    {
        List<Node> result = null;

        yield return AStar.CalculatePath(
            start,
            node => node == goal,
            node => node.neighbors.Select(n => new WeightedNode<Node>(n, 1f)),
            node => Vector3.Distance(node.transform.position, goal.transform.position),
            onComplete: path => result = path,
            onFail: () => Debug.LogWarning("NPC no encontró camino")
        );

        onPathFound?.Invoke(result);
    }

    /// <summary>
    /// Recorro el camino de AStar
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator FollowPath(List<Node> path, float speed, float speedRotation, Action<int> onStep = null)
    {
        if (path == null || path.Count == 0)
            yield break;

        foreach (var node in path)
        {
            while (Vector3.Distance(transform.position, node.transform.position) > 0.1f)
            {
                Vector3 dir = (node.transform.position - transform.position).normalized;
                transform.position += dir * speed * Time.deltaTime;
                transform.forward = Vector3.Lerp(transform.forward, dir, speedRotation * Time.deltaTime);
                yield return null;
            }

            onStep?.Invoke(1); // 🔁 le avisás que hiciste un paso
        }
    }


    /// <summary>
    /// Corrutina optimizada para ejecutar la busqueda del camino + recorrerlo
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator MoveTo(Node target, float speed, float speedRotation, Action<int> onStep = null)
    {
        List<Node> path = null;
        yield return GeneratePath(CurrentNode, target, result => path = result);

        yield return FollowPath(path, speed, speedRotation, onStep);
    }

    private void ActivateFsm() => Fsm.Active = true;

    private void OnEnable()
    {
        NodeGenerator.OnGameReady += ActivateFsm;
    }

    private void OnDisable()
    {
        NodeGenerator.OnGameReady -= ActivateFsm;
    }


    /// <summary>
    /// Uso exclusivo para testeo, se utiliza en el NPCGoapEditor [Para el inspector de Unity]
    /// </summary>
    /// <param name="life"></param>
    /// <param name="value"></param>
    public void ModifyLife(float life, int value) => life += value;

}