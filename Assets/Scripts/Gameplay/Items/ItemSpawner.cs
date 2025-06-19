using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private NetworkPrefabRef[] itemPrefabs;
    [SerializeField] private float respawnDelay = 3f;

    [Networked, Capacity(20)] private NetworkArray<NetworkObject> spawnedItems { get; }

    private bool isFirstSpawnDone;

    private struct PendingRespawn
    {
        public int index;
        public float timer;
    }

    private List<PendingRespawn> pendingRespawns = new();

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (!isFirstSpawnDone)
        {
            InitialSpawn();
            isFirstSpawnDone = true;
        }

        UpdatePendingRespawns();
    }

    private void InitialSpawn()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnedItems[i] == null)
            {
                SpawnItemAt(i);
            }
        }
    }

    public void NotifyItemPicked(NetworkObject item)
    {
        if (!HasStateAuthority) return;

        for (int i = 0; i < spawnedItems.Length; i++)
        {
            if (spawnedItems[i] == item)
            {
                spawnedItems.Set(i, default); // liberar slot
                pendingRespawns.Add(new PendingRespawn { index = i, timer = 0f });
                break;
            }
        }
    }

    private void UpdatePendingRespawns()
    {
        for (int i = pendingRespawns.Count - 1; i >= 0; i--)
        {
            var entry = pendingRespawns[i];
            entry.timer += Runner.DeltaTime;

            if (entry.timer >= respawnDelay)
            {
                SpawnItemAt(entry.index);
                pendingRespawns.RemoveAt(i);
            }
            else
            {
                pendingRespawns[i] = entry;
            }
        }
    }

    private void SpawnItemAt(int index)
    {
        int prefabIndex = Random.Range(0, itemPrefabs.Length);
        var itemObj = Runner.Spawn(itemPrefabs[prefabIndex], spawnPoints[index].position, Quaternion.identity);

        if (itemObj.TryGetComponent<IItemPickup>(out var pickup))


            pickup.SetSpawner(this);

        spawnedItems.Set(index, itemObj);
    }
}