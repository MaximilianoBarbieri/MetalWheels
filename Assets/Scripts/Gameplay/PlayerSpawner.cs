using System;
using System.Collections.Generic;
using System.Text;
using Fusion;
using Fusion.Sockets;
using Newtonsoft.Json;
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
        if (!Runner.IsServer) return;
        
        #region Obtain lobby data
        // Obtener data del jugador que entra
        int selectedCar = 0; // valor def
        string nickname = "Player"; // valor def

        byte[] tokenBytes = Runner.GetPlayerConnectionToken(player);
        if (tokenBytes != null)
        {
            string json = Encoding.UTF8.GetString(tokenBytes);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (data != null)
            {
                if (data.TryGetValue("PlayerSelected", out var carSelectedObj))
                    selectedCar = (int)(long)carSelectedObj;

                if (data.TryGetValue("PlayerNickName", out var nicknameObj))
                    nickname = nicknameObj.ToString();

                Debug.Log("SELECTED CAR: " + selectedCar);
                Debug.Log("NICKNAME: " + nickname);
            }
            else
            {
                Debug.Log("data null");
            }
        }

        #endregion

        Transform spawnPoint = GetFreeSpawnPoint(player);
        Runner.Spawn(_playerPrefabs[selectedCar], spawnPoint.position, Quaternion.identity, player);
        /*NetworkObject playerObj = Runner.Spawn(_playerPrefabs[selectedCar], spawnPoint.position, Quaternion.identity, player);

        int selection = PlayerPrefs.GetInt("PlayerSelected");
        NetworkPrefabRef prefab = _playerPrefabs[selection];

        Debug.Log("OnPlayerJoined");
        Debug.Log("Seleccionado CAR: " + selection);

        if (Runner.IsServer)
        {
            Debug.Log("OnPlayerJoined: runner.IsServer");
            Transform point = GetFreeSpawnPoint(player);
            Runner.Spawn(prefab, point.transform.position, Quaternion.identity, player);
        }*/
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (UsedSpawnIndices.ContainsKey(player)) UsedSpawnIndices.Remove(player);
    }
}