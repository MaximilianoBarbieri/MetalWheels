using Fusion;
using UnityEngine;

public class GameTimer : NetworkBehaviour
{
    [Networked] private float timeLeft { get; set; } = 120f;
    private bool ended = false;

    public override void FixedUpdateNetwork()
    {
        if (ended) return;
        timeLeft -= Runner.DeltaTime;
        if (timeLeft <= 0)
        {
            EndGame();
            ended = true;
        }
    }

    private void EndGame()
    {
        var players = FindObjectsOfType<PlayerController>();
        PlayerController winner = null;
        int maxKills = -1;
        foreach (var p in players)
        {
            if (p.Kills > maxKills)
            {
                winner = p;
                maxKills = p.Kills;
            }
        }

        foreach (var p in players)
        {
            var isWinner = p == winner;
            p.GetComponent<EndGameUI>().ShowEndScreen(isWinner);
        }
    }
}
