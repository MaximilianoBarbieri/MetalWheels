using Fusion;
using UnityEngine;

public class HealthController : NetworkBehaviour
{
    [Networked] private int hp { get; set; } = 100;

    public void TakeDamage(int amount, PlayerRef source)
    {
        if (!HasStateAuthority) return;
        hp -= amount;
        if (hp <= 0)
        {
            Runner.Despawn(Object);

            if (Runner.TryGetPlayerObject(source, out NetworkObject playerObject))
            {
                if (playerObject.TryGetBehaviour(out PlayerController controller))
                {
                    controller.Kills++;
                }
            }
        }
    }
}