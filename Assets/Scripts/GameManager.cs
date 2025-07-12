using Fusion;
using System;
using System.Linq;
using UnityEngine;

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
        if (CurrentState == GameState.WaitingForPlayers && playerCount >= 2)
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
}