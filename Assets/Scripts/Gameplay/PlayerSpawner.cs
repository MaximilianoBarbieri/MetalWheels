using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : NetworkBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef[] _playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [Networked, Capacity(4)] 
    private NetworkDictionary<PlayerRef, int> UsedSpawnIndices => default;

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

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        int selection = PlayerData.CarSelected;
        NetworkPrefabRef prefab = _playerPrefabs[selection];
        Debug.Log("OnPlayerJoined");

        if (runner.IsServer)
        {
            Debug.Log("OnPlayerJoined: runner.IsServer");
            Transform point = GetFreeSpawnPoint(player);
            runner.Spawn(prefab, point.transform.position, Quaternion.identity, player);
        }
        
        if (PlayerPrefs.GetInt("PlayerSelected", 0).Equals(0))
            Debug.Log("Seleccionado CAR1");
        else
            Debug.Log("Seleccionado CAR2");
    }
    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (UsedSpawnIndices.ContainsKey(player)) UsedSpawnIndices.Remove(player);
    }
    
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        runner.Shutdown();
    }

    #region Unused Runner Callbacks

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    #endregion
}