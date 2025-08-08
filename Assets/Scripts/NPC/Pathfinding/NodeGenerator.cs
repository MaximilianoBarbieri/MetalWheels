using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NodeGenerator : NetworkBehaviour
{
    [Header("Tamaño de la grilla (X-Z)")] [SerializeField]
    private Vector2Int gridSize;

    [Header("Tamaño para la zona segura (X-Z)")] [SerializeField]
    private int segmentX = 5;

    [SerializeField] private int segmentZ = 4;

    [Header("Espaciado entre nodos")] [SerializeField]
    private int nodeSpacing;

    [Header("Prefab del nodo")] [SerializeField]
    private GameObject nodePrefab;

    [Header("Generación Lazy")] [SerializeField]
    int nodesPerFrame = 100;

    [SerializeField] int neighborOpsPerFrame = 200;
    [SerializeField] int zoneOpsPerFrame = 200;

    [Header("Lista de interactuables en la escena")] [SerializeField]
    private List<InteractableNPC> interactables;
    
    private Coroutine _generationRoutine;

    private List<NetworkObject> _nodes;
    private List<SafeZone> _zones = new();

    private Action _onGridCreated;
    public static event Action OnGameReady;
    public static NodeGenerator Instance { get; private set; }
    private bool IsReady => _nodes != null && _nodes.Count == gridSize.x * gridSize.y;
    private NetworkRunner runner => FindObjectOfType<NetworkRunner>();


    public override void Spawned()
    {
        base.Spawned();

        if (Instance == null) Instance = this;

        if (runner.IsServer)
            GetNodes();
    }

    private void GetNodes()
    {
        if (!HasStateAuthority) return;

        if (!IsReady && _generationRoutine == null)
            _generationRoutine = StartCoroutine(GenerateGridLazy());
    }

    public void Register(InteractableNPC interactable)
    {
        if (!interactables.Contains(interactable))
            interactables.Add(interactable);

        interactable.AssignClosestNode(2f);
    }

    private IEnumerator GenerateGridLazy()
    {
        _nodes = new List<NetworkObject>();
        int created = 0;

        foreach (var node in Generators.GenerateNodes(gridSize, nodeSpacing, transform.position, nodePrefab, runner))
        {
            _nodes.Add(node);
            created++;

            if (created % nodesPerFrame == 0)
                yield return null;
        }

        yield return DetectAllNeighborsLazy();

        yield return GenerateZonesLazy();

        yield return ActivateInteractableLazy();

        _generationRoutine = null;

        OnGameReady?.Invoke();
    }

    private IEnumerator DetectAllNeighborsLazy()
    {
        var nodeComps = _nodes.Select(n => n.GetComponent<Node>()).Where(n => n != null).ToList();
        int ops = 0;

        foreach (var n in nodeComps)
        {
            n.DetectNeighbors(nodeSpacing);
            if (++ops >= neighborOpsPerFrame)
            {
                ops = 0;
                yield return null;
            }
        }
    }

    private IEnumerator GenerateZonesLazy()
    {
        _zones.Clear();
        int w = gridSize.x / segmentX;
        int h = gridSize.y / segmentZ;

        var nodeComps = _nodes.Select(n => n.GetComponent<Node>()).Where(n => n != null).ToList();

        var zoneGrid = new SafeZone[segmentX, segmentZ];
        for (int x = 0; x < segmentX; x++)
        for (int z = 0; z < segmentZ; z++)
            _zones.Add(zoneGrid[x, z] = new SafeZone { segmentIndex = new Vector2Int(x, z) });

        int ops = 0;
        foreach (var node in nodeComps)
        {
            int xIndex =
                Mathf.Clamp(Mathf.FloorToInt((node.transform.position.x - transform.position.x) / (w * nodeSpacing)), 0,
                    segmentX - 1);
            int zIndex =
                Mathf.Clamp(Mathf.FloorToInt((node.transform.position.z - transform.position.z) / (h * nodeSpacing)), 0,
                    segmentZ - 1);

            zoneGrid[xIndex, zIndex].nodes.Add(node);

            if (++ops >= zoneOpsPerFrame)
            {
                ops = 0;
                yield return null;
            }
        }

        foreach (var zone in _zones) zone.neighbors.Clear();
        for (int x = 0; x < segmentX; x++)
        for (int z = 0; z < segmentZ; z++)
        {
            var zc = zoneGrid[x, z];
            if (x > 0) zc.neighbors.Add(zoneGrid[x - 1, z]);
            if (x < segmentX - 1) zc.neighbors.Add(zoneGrid[x + 1, z]);
            if (z > 0) zc.neighbors.Add(zoneGrid[x, z - 1]);
            if (z < segmentZ - 1) zc.neighbors.Add(zoneGrid[x, z + 1]);

            yield return null;
        }
    }

    private IEnumerator ActivateInteractableLazy()
    {
        foreach (var i in interactables)
        {
            i.AssignClosestNode(2f);
            yield return null;
        }
    }

    public SafeZone GetZoneForNode(Node node) => _zones.FirstOrDefault(z => z.nodes.Contains(node));

    public void ClearGrid()
    {
        if (!HasStateAuthority || _nodes == null) return;

        foreach (var node in _nodes.Where(n => n != null))
        {
            var netObj = node.GetComponent<NetworkObject>();
            if (netObj != null)
                runner.Despawn(netObj);
            else
                Destroy(node);
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

public class SafeZone
{
    public List<Node> nodes = new();
    public Vector2Int segmentIndex;
    public List<SafeZone> neighbors = new();
}