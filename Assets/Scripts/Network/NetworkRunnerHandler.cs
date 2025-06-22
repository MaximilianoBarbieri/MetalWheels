using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner _runnerPrefab;
    NetworkRunner _currentRunner;
    public event Action OnJoinedLobby = delegate { };
    public event Action<List<SessionInfo>> OnSessionListUpdate = delegate { };

    
    #region LOBBY
    
    public void JoinLobby()
    {
        if (_currentRunner) Destroy(_currentRunner.gameObject);

        _currentRunner = Instantiate(_runnerPrefab);
        
        _currentRunner.AddCallbacks(this);

        JoinLobbyAsync();
    }

    async void JoinLobbyAsync()
    {
        var result = await _currentRunner.JoinSessionLobby(SessionLobby.Custom, "Normal Lobby");

        if (!result.Ok)
            Debug.LogError("[Custom error] Unable to Join Lobby");
        else
        {
            Debug.Log("[Custom Msg] Joined Lobby");
            OnJoinedLobby();
        }
    }
    
    #endregion

    
    #region Join / Create Game

    public async void CreateGame(string sessionName, string sceneName)
    {
        await InitializeGame(GameMode.Host, sessionName, SceneUtility.GetBuildIndexByScenePath($"Scenes/{sceneName}"));
    }
    
    public async void JoinGame(SessionInfo sessionInfo)
    {
        await InitializeGame(GameMode.Client, sessionInfo.Name, SceneManager.GetActiveScene().buildIndex);
    }
    
    async Task InitializeGame(GameMode gameMode, string sessionName, int sceneIndex)
    {
        // Crear un token con la información del jugador
        var playerData = new Dictionary<string, object>
        {
            { "PlayerSelected", PlayerPrefs.GetInt("PlayerSelected", 0) },
            { "PlayerNickName", PlayerPrefs.GetString("PlayerNickName", "Player") }
        };
        
        // Serializar el token
        string json = JsonConvert.SerializeObject(playerData);
        byte[] connectionToken = Encoding.UTF8.GetBytes(json);
        
        _currentRunner.ProvideInput = true;

        var result = await _currentRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            Scene =  SceneRef.FromIndex(sceneIndex),
            SessionName = sessionName,
            ConnectionToken = connectionToken // Aquí pasamos el token
        });
        
        if (!result.Ok)
        {
            Debug.LogError("[Custom error] Unable to start game");
        }
        else
        {
            Debug.Log("[Custom Msg] Game started");
        }
    }

    #endregion
    
    
    #region Used Runner Callbacks
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        OnSessionListUpdate(sessionList);
    }
    
    #endregion
    
    #region Unused Runner Callbacks
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
    
    #endregion
}