using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef[] _playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [Networked, Capacity(4)] private NetworkDictionary<PlayerRef, int> UsedSpawnIndices => default;

    private Transform GetFreeSpawnPoint(PlayerRef player)
    {
        List<int> available = new();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!UsedSpawnIndices.ContainsValue(i))
                available.Add(i);
        }

        if (available.Count == 0)
        {
            Debug.LogWarning("Todos los puntos están ocupados. Se reutilizará uno al azar.");
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        int index = available[Random.Range(0, available.Count)];
        UsedSpawnIndices.Add(player, index);

        return spawnPoints[index];
    }

    public void PlayerJoined(PlayerRef player)
    {
        int selection = PlayerPrefs.GetInt("PlayerSelected");
        NetworkPrefabRef prefab = _playerPrefabs[selection];
        Debug.Log("OnPlayerJoined");

        if (Runner.IsServer)
        {
            Debug.Log("OnPlayerJoined: runner.IsServer");
            Transform point = GetFreeSpawnPoint(player);
            Runner.Spawn(prefab, point.transform.position, Quaternion.identity, player);
        }


        Debug.Log("Seleccionado CAR: " + selection);
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (UsedSpawnIndices.ContainsKey(player)) UsedSpawnIndices.Remove(player);
    }
}