using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NodeGenerator : NetworkBehaviour
{
    [Header("Tamaño de la grilla (X-Z)")] 
    
    [SerializeField] private Vector2Int gridSize;

    [Header("Tamaño para la zona segura (X-Z)")] 
    
    [SerializeField] private int segmentX = 5;
    [SerializeField] private int segmentZ = 4;

    [Header("Espaciado entre nodos")] 
    
    [SerializeField] private int nodeSpacing;

    [Header("Prefab del nodo")] 
    
    [SerializeField] private GameObject nodePrefab;

    [Header("Generación Lazy")] 
    
    [SerializeField] private int nodesPerFrame;

    [Header("Lista de interactuables en la escena")] 
    
    [SerializeField] private List<InteractableNPC> interactables;


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

    private IEnumerator GenerateGridLazy()
    {
        _nodes = new List<NetworkObject>();
        Vector3 origin = transform.position;
        float y = origin.y;
        int created = 0;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                Vector3 pos = new Vector3(origin.x + x * nodeSpacing, y, origin.z + z * nodeSpacing);

                NetworkObject node = Runner.Spawn(nodePrefab, pos, Quaternion.identity, Runner.LocalPlayer, null);
                node.name = $"Node_{x}_{z}";
                _nodes.Add(node);

                created++;
                if (created % nodesPerFrame == 0)
                    yield return null;
            }
        }

        _generationRoutine = null;

        _onGridCreated?.Invoke();
    }

    private void DetectAllNeighbors() => _nodes.Select(n => n.GetComponent<Node>())
                                                           .Where(n => n != null)
                                                           .ToList()
                                                           .ForEach(n => n.DetectNeighbors(nodeSpacing));
    

    public void Register(InteractableNPC interactable)
    {
        if (!interactables.Contains(interactable))
            interactables.Add(interactable);

        interactable.AssignClosestNode(2f);
    }

    private void ActivateInteractable() => interactables.ToList().ForEach(i => i.AssignClosestNode(2f));

    private void GenerateSafeZones()
    {
        _zones.Clear();
        int w = gridSize.x / segmentX;
        int h = gridSize.y / segmentZ;

        foreach (var node in _nodes.Select(n => n.GetComponent<Node>()))
        {
            int xIndex = Mathf.FloorToInt((node.transform.position.x - transform.position.x) / (w * nodeSpacing));
            int zIndex = Mathf.FloorToInt((node.transform.position.z - transform.position.z) / (h * nodeSpacing));

            xIndex = Mathf.Clamp(xIndex, 0, segmentX - 1);
            zIndex = Mathf.Clamp(zIndex, 0, segmentZ - 1);

            Vector2Int zoneIndex = new(xIndex, zIndex);

            var zone = _zones.FirstOrDefault(z => z.segmentIndex == zoneIndex);
            if (zone == null)
            {
                zone = new SafeZone { segmentIndex = zoneIndex };
                _zones.Add(zone);
            }

            zone.nodes.Add(node);
        }

        DetectZoneNeighbors();
    }

    public SafeZone GetZoneForNode(Node node) => _zones.FirstOrDefault(z => z.nodes.Contains(node));

    private void DetectZoneNeighbors()
    {
        foreach (var zone in _zones)
        {
            zone.neighbors.Clear();

            foreach (var otherZone in _zones)
            {
                if (zone == otherZone) continue;

                bool areNeighbors = zone.nodes.Any(node =>
                    node.neighbors.Any(n => otherZone.nodes.Contains(n)));

                if (areNeighbors)
                    zone.neighbors.Add(otherZone);
            }
        }
        
        OnGameReady.Invoke();
    }
    
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
    
    private void OnEnable()
    {
        _onGridCreated += ActivateInteractable;
        _onGridCreated += DetectAllNeighbors;
        _onGridCreated += GenerateSafeZones;
    }

    private void OnDisable()
    {
        _onGridCreated -= ActivateInteractable;
        _onGridCreated -= DetectAllNeighbors;
        _onGridCreated -= GenerateSafeZones;
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