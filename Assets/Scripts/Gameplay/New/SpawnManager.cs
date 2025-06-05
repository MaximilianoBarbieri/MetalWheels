using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;

    [SerializeField] private Transform[] spawnPoints;
    private List<int> usedIndices = new();

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetFreeSpawnPoint()
    {
        List<int> available = new();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedIndices.Contains(i))
                available.Add(i);
        }

        if (available.Count == 0)
            usedIndices.Clear();

        int index = available.Count > 0 ? available[Random.Range(0, available.Count)] : Random.Range(0, spawnPoints.Length);
        usedIndices.Add(index);
        return spawnPoints[index];
    }
}