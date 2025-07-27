using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public Node CurrentNode => GetCurrentNode();
    public Animator animator => GetComponent<Animator>();

    [Header("Interacción")] public InteractableNPC currentInteractable;

    private List<CharacterController> _carsInRange = new();
    private HashSet<InteractableNPC> _interactablesInRange = new();

    public FiniteStateMachine fsm;

    [SerializeField] internal Idle_NPC idleNpc;
    [SerializeField] internal Walk_NPC walkNpc;
    [SerializeField] internal Sitdown_NPC sitdownNpc;
    [SerializeField] internal Talk_NPC talkNpc;
    [SerializeField] internal Escape_NPC escapeNpc;
    [SerializeField] internal Death_NPC deathNpc;

    private void Start()
    {
        fsm = new FiniteStateMachine(idleNpc, StartCoroutine);
        fsm.Active = true;
    }

    private void Update() => fsm.Update(); // Llama al estado actual (solo UpdateLoop y ProcessInput)

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
    private CharacterController ClosestCar() =>
        _carsInRange
            .Where(car => car != null)
            .OrderBy(car => Vector3.Distance(transform.position, car.transform.position))
            .FirstOrDefault(); //Metodo auxiliar, por si necesito este obj

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
    private IEnumerator FollowPath(List<Node> path, float speed,float speedRotation, Action<int> onStep = null)
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
    public IEnumerator MoveTo(Node target, float speed,float speedRotation, Action<int> onStep = null)
    {
        List<Node> path = null;
        yield return GeneratePath(CurrentNode, target, result => path = result);

        yield return FollowPath(path, speed, speedRotation, onStep);
    }

    /// <summary>
    /// Uso exclusivo para testeo, se utiliza en el NPCGoapEditor [Para el inspector de Unity]
    /// </summary>
    /// <param name="life"></param>
    /// <param name="value"></param>
    public void ModifyLife(float life, int value) => life += value;
}