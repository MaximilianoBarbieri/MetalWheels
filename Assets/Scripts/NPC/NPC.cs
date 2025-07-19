using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private Node _currentNode;

    public Node CurrentNode
    {
        get
        {
            if (_currentNode == null)
            {
                _currentNode = NodeGenerator.Instance.GetNodes()
                    .Select(go => go.GetComponent<Node>())
                    .OrderBy(n => Vector3.Distance(transform.position, n.transform.position))
                    .FirstOrDefault();
            }

            return _currentNode;
        }
        set => _currentNode = value;
    }

    public Animator animator => GetComponent<Animator>();

    [Header("Interacción")] [SerializeField]
    private List<CharacterController> _carsInRange = new();

    public InteractableNPC currentInteractable;

    private HashSet<InteractableNPC> _interactablesInRange = new();

    [Header("Parametros")] public float speed;
    public float speedRotation;

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

    // public Node GetCurrentNode() => NodeGenerator.Instance.GetNodes()
    //     .Select(go => go.GetComponent<Node>())
    //     .OrderBy(n => Vector3.Distance(transform.position, n.transform.position))
    //     .FirstOrDefault(); // Asigna el nodo más cercano

    /// <summary>
    /// Devuelve el interactable mas cercano al NPC basandose en su distancia
    /// </summary>
    /// <returns></returns>
    public InteractableNPC GetClosestInteractable() =>
        _interactablesInRange
            .Where(obj => obj != null)
            .OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position))
            .FirstOrDefault();

    /// <summary>
    /// Verifica si hay algun vehiculo cerca del NPC, basandose en su CarDistance
    /// </summary>
    /// <returns></returns>
    public CharacterController ClosestCar() =>
        _carsInRange
            .Where(car => car != null)
            .OrderBy(car => Vector3.Distance(transform.position, car.transform.position))
            .FirstOrDefault();

    public List<Node> debugPath;


    /// <summary>
    /// Movimiento por AStar
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="goalNode"></param>
    /// <returns></returns>
    public IEnumerator MoveAlongPath(Node startNode, Node goalNode)
    {
        Debug.Log("----------- MoveAlongPath -----------");

        bool finished = false;
        List<Node> path = null;

        var astar = new AStar<Node>();

        astar.OnPathCompleted += result =>
        {
            path = result.ToList();
            debugPath = path;
            finished = true;
        };

        astar.OnCantCalculate += () =>
        {
            Debug.LogWarning("No se pudo calcular el camino con AStar.");
            finished = true;
        };

        yield return StartCoroutine(astar.Run(
            startNode,
            n => n == goalNode,
            n => n.neighbors.Select(neighbor =>
                new WeightedNode<Node>(neighbor, Vector3.Distance(n.transform.position, neighbor.transform.position))),
            n => Vector3.Distance(n.transform.position, goalNode.transform.position)
        ));

        while (!finished)
            yield return null;

        if (path == null) yield break;


        foreach (var node in path)
        {
            while (Vector3.Distance(transform.position, node.transform.position) > 0.1f)
            {
                Vector3 dir = (node.transform.position - transform.position).normalized;
                transform.position += dir * speed * Time.deltaTime;
                transform.forward = Vector3.Lerp(transform.forward, dir, speedRotation * Time.deltaTime);
                yield return null;
            }

            CurrentNode = node;
        }
    }

    /// <summary>
    /// Uso exclusivo para testeo, se utiliza en el NPCGoapEditor [Para el inspector de Unity]
    /// </summary>
    /// <param name="life"></param>
    /// <param name="value"></param>
    public void ModifyLife(float life, int value) => life += value;

    private void OnDrawGizmos()
    {
        if (debugPath == null || debugPath.Count == 0) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < debugPath.Count - 1; i++)
        {
            if (debugPath[i] != null && debugPath[i + 1] != null)
                Gizmos.DrawLine(debugPath[i].transform.position, debugPath[i + 1].transform.position);
        }

        foreach (var node in debugPath)
        {
            if (node != null)
                Gizmos.DrawSphere(node.transform.position, 0.2f);
        }
    }
}