using Fusion;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    WaitingForPlayers,
    Playing,
    Ended,
    HostDisconnected
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameState> OnGameStateChanged = delegate { };
    public static event Action<float> OnTimerChanged = delegate { };
    public static event Action OnHostDisconnected = delegate { };
    public static event Action<PlayerRef> OnWinnerChanged = delegate { };

    [Networked, OnChangedRender(nameof(OnGameStateChangedRender))]
    public GameState CurrentState { get; private set; }

    [Networked, OnChangedRender(nameof(OnTimerChangedRender))]
    public float Timer { get; private set; }

    [Networked, OnChangedRender(nameof(OnWinnerChangedRender))]
    public PlayerRef Winner { get; private set; }

    private float maxTimer = 120f;

    public override void Spawned()
    {
        if (Instance == null) Instance = this;

        if (HasStateAuthority)
        {
            Timer = maxTimer;
            CurrentState = GameState.WaitingForPlayers;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        int playerCount = Runner.ActivePlayers.Count();
        if (CurrentState == GameState.WaitingForPlayers && playerCount >= 1)
        {
            CurrentState = GameState.Playing;
        }

        if (CurrentState == GameState.Playing)
        {
            Timer -= Runner.DeltaTime;

            if (Timer <= 0)
            {
                Timer = 0;
                CurrentState = GameState.Ended;
                // Buscar ganador solo si aún no está seteado
                if (Winner == default)
                    CalculateAndSetWinner();
            }
        }
    }

    private void CalculateAndSetWinner()
    {
        // Busca todos los ModelPlayer vivos
        var players = FindObjectsOfType<ModelPlayer>();

        // Si nadie está vivo, elegí el que más kills tenía, o empate por health
        var winner = players
            .OrderByDescending(p => p.Kills)
            .ThenByDescending(p => p.CurrentHealth)
            .FirstOrDefault();

        if (winner != null)
            Winner = winner.Object.InputAuthority;
        else
            Winner = default;
    }

    private void OnGameStateChangedRender()
    {
        Debug.Log($"[GameManager] OnGameStateChangedRender: {CurrentState}");
        OnGameStateChanged?.Invoke(CurrentState);
    }

    private void OnTimerChangedRender()
    {
        OnTimerChanged?.Invoke(Timer);
    }

    private void OnWinnerChangedRender()
    {
        OnWinnerChanged?.Invoke(Winner);
    }

    public static void NotifyHostDisconnected()
    {
        OnHostDisconnected?.Invoke();
    }

    #region SessionManager

    public void OnPlayerRequestedExit()
    {
        if (Runner.IsServer)
            RPC_ShowDisconnectPanelForAll();
        else
            ShowLocalDisconnectAndExit();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ShowDisconnectPanelForAll()
    {
        string message = Runner.IsServer ? "Disconnecting" : "Host disconnected going to main menu";

        PlayerLocalUIHandler ui = FindObjectOfType<PlayerLocalUIHandler>();
        if (ui != null)
            ui.ShowLocalDisconnectPanel(message);

        StartCoroutine(ReturnToMainMenuDelayed());
    }

    void ShowLocalDisconnectAndExit()
    {
        PlayerLocalUIHandler ui = FindObjectOfType<PlayerLocalUIHandler>();
        if (ui != null)
            ui.ShowLocalDisconnectPanel("Disconnecting");

        StartCoroutine(DisconnectDelayed());
    }

    IEnumerator ReturnToMainMenuDelayed()
    {
        yield return new WaitForSeconds(2f);
        CleanupAndReturn();
    }

    IEnumerator DisconnectDelayed()
    {
        yield return new WaitForSeconds(2f);
        if(Runner != null)
            Runner.Disconnect(Runner.LocalPlayer);

        CleanupAndReturn();
    }

    void CleanupAndReturn()
    {
        // Destruir UI Local
        var ui = FindObjectOfType<PlayerLocalUIHandler>();
        if (ui != null) Destroy(ui.gameObject);

        // Destruir CinemachineVirtualCamera local
        var cinemachineCam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        if (cinemachineCam != null) Destroy(cinemachineCam.gameObject);

        // Destruir UI global (PlayerGlobalUIHandler) si existe
        var globalUI = FindObjectOfType<PlayerGlobalUIHandler>();
        if (globalUI != null) Destroy(globalUI.gameObject);

        if (Runner.IsServer)
            Runner.Shutdown();

        SceneManager.LoadScene("MainMenu");
    }

    #endregion
}