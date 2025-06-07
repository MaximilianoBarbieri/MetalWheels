using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;

    // Player Spawns
    [Header("Player Spawns")] 
    [SerializeField] private Transform[] playerSpawnPoints;
    private List<int> usedPlayerIndices = new();

    // Item Spawns
    [Header("Item Spawns")] 
    [SerializeField] private Transform[] itemSpawnPoints;
    [SerializeField] private NetworkPrefabRef[] itemPrefabs;
    [SerializeField] private float itemSpawnInterval = 3f; // segundos

    [Networked, Capacity(20)] private NetworkArray<NetworkObject> spawnedItems { get; }

    private float itemSpawnTimer;

    private void Awake()
    {
        Instance = this;
    }

    // --- PLAYER SPAWN ---
    public Transform GetFreePlayerSpawnPoint()
    {
        List<int> available = new();
        for (int i = 0; i < playerSpawnPoints.Length; i++)
        {
            if (!usedPlayerIndices.Contains(i))
                available.Add(i);
        }

        if (available.Count == 0)
            usedPlayerIndices.Clear();

        int index = available.Count > 0
            ? available[Random.Range(0, available.Count)]
            : Random.Range(0, playerSpawnPoints.Length);
        usedPlayerIndices.Add(index);
        return playerSpawnPoints[index];
    }

    // --- ITEM SPAWN LOGIC ---
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        itemSpawnTimer += Runner.DeltaTime;
        if (itemSpawnTimer >= itemSpawnInterval)
        {
            TrySpawnItems();
            itemSpawnTimer = 0f;
        }
    }

    private void TrySpawnItems()
    {
        for (int i = 0; i < itemSpawnPoints.Length; i++)
        {
            if (spawnedItems[i] == null)
            {
                int prefabIndex = Random.Range(0, itemPrefabs.Length);
                var itemObj = Runner.Spawn(itemPrefabs[prefabIndex], itemSpawnPoints[i].position, Quaternion.identity);
                spawnedItems.Set(i, itemObj);
            }
        }
    }

    public void NotifyItemPicked(NetworkObject itemObj)
    {
        for (int i = 0; i < spawnedItems.Length; i++)
        {
            if (spawnedItems[i] == itemObj)
            {
                spawnedItems.Set(i, default);
                break;
            }
        }
    }
}