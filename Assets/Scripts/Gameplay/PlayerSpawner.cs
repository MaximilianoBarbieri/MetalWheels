using System;
using System.Collections.Generic;
using System.Text;
using Fusion;
using Fusion.Sockets;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : NetworkBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef[] _playerPrefabs;

    [SerializeField] private Transform[] spawnPoints;

    private CharacterInputHandler _characterInputHandler;
    [Networked] [Capacity(4)] private NetworkDictionary<PlayerRef, int> UsedSpawnIndices => default;
    
    public override void Spawned()
    {
        base.Spawned();

        if (!Runner.IsServer) return;
        Runner.AddCallbacks(this);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        #region Obtain lobby data

        // Obtener data del jugador que entra
        var selectedCar = 0; // valor def
        var nickname = "Player"; // valor def

        var tokenBytes = runner.GetPlayerConnectionToken(player);
        if (tokenBytes != null)
        {
            var json = Encoding.UTF8.GetString(tokenBytes);
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

        var spawnPoint = GetFreeSpawnPoint(player);
        runner.Spawn(_playerPrefabs[selectedCar], spawnPoint.position, Quaternion.identity, player);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (!NetworkPlayer.Local) return;

        _characterInputHandler ??= NetworkPlayer.Local.GetComponent<CharacterInputHandler>();

        input.Set(_characterInputHandler.GetLocalInputs());
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        if (UsedSpawnIndices.ContainsKey(player)) UsedSpawnIndices.Remove(player);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        runner.Shutdown();
    }

    private Transform GetFreeSpawnPoint(PlayerRef player)
    {
        List<int> available = new();

        for (var i = 0; i < spawnPoints.Length; i++)
            if (!UsedSpawnIndices.ContainsValue(i))
                available.Add(i);

        if (available.Count == 0)
        {
            Debug.LogWarning("Todos los puntos están ocupados. Se reutilizará uno al azar.");
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        var index = available[Random.Range(0, available.Count)];
        UsedSpawnIndices.Add(player, index);

        return spawnPoints[index];
    }

    #region Unused callbacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    #endregion
}