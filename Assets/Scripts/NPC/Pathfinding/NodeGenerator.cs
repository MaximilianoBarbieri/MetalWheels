using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeGenerator : MonoBehaviour
{
    [Header("Tamaño de la grilla (X-Z)")] public Vector2Int gridSize = new Vector2Int(20, 20);

    [Header("Espaciado entre nodos")] public int nodeSpacing;

    [Header("Prefab del nodo")] public GameObject nodePrefab;

    [Header("Generación Lazy")] public int nodesPerFrame = 10;

    [Header("Lista de interactuables en la escena")] [SerializeField]
    private List<InteractableNPC> _interactables;

    private List<GameObject> _nodes;
    private Coroutine _generationRoutine;
    public bool IsReady => _nodes != null && _nodes.Count == gridSize.x * gridSize.y;

    public static NodeGenerator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        GetNodes();
    }

    public List<GameObject> GetNodes()
    {
        if (!IsReady && _generationRoutine == null)
            _generationRoutine = StartCoroutine(GenerateGridLazy());

        return _nodes;
    }

    private IEnumerator GenerateGridLazy()
    {
        _nodes = new List<GameObject>();

        Vector3 origin = transform.position;
        float y = origin.y;

        int created = 0;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                Vector3 pos = new Vector3(origin.x + x * nodeSpacing, y, origin.z + z * nodeSpacing);
                GameObject node = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
                node.name = $"Node_{x}_{z}";
                _nodes.Add(node);

                created++;
                if (created % nodesPerFrame == 0)
                    yield return null; // Espera un frame cada X nodos
            }
        }

        Debug.Log($"[Grid] Generación completada: {_nodes.Count} nodos.");
        _generationRoutine = null;

        DetectAllNeighbors(nodeSpacing);
    }

    private void DetectAllNeighbors(int distance)
    {
        foreach (var pn in _nodes.Select(node => node.GetComponent<Node>()).Where(pn => pn != null))
            pn.DetectNeighbors(distance);

        ActivateInteractable();
    }

    public void Register(InteractableNPC npc)
    {
        if (!_interactables.Contains(npc))
            _interactables.Add(npc);
    }

    private void ActivateInteractable() => _interactables.ToList().ForEach(i => i.AssignClosestNode(2f));

    public void ClearGrid()
    {
        if (_nodes == null) return;

        foreach (var node in _nodes)
        {
            if (node != null)
                DestroyImmediate(node);
        }

        _nodes.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3(gridSize.x * nodeSpacing, 0.1f, gridSize.y * nodeSpacing);
        Gizmos.DrawWireCube(transform.position + size / 2f, size);
    }
}