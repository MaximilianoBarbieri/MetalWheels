using Fusion;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private NetworkPrefabRef[] itemPrefabs;
    [SerializeField] private float interval = 3f;
    [Networked, Capacity(20)] private NetworkArray<NetworkObject> spawnedItems { get; }

    private float timer;

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        timer += Runner.DeltaTime;
        if (timer >= interval)
        {
            SpawnMissingItems();
            timer = 0f;
        }
    }

    //TODO: modificar este metodo
    //algo superior tiene que saber que spawnpoint obtuvo cada player para no pisar los puntos de spawn
    private void SpawnMissingItems()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnedItems[i] == null)
            {
                int prefabIndex = Random.Range(0, itemPrefabs.Length);
                var itemObj = Runner.Spawn(itemPrefabs[prefabIndex], spawnPoints[i].position, Quaternion.identity);
                spawnedItems.Set(i, itemObj);
            }
        }
    }

    public void NotifyItemPicked(NetworkObject item)
    {
        for (int i = 0; i < spawnedItems.Length; i++)
        {
            if (spawnedItems[i] == item)
            {
                spawnedItems.Set(i, default);
                break;
            }
        }
    }
}