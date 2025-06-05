using Fusion;
using UnityEngine;

public class ModelPlayer : NetworkBehaviour
{
    [Networked] public int MaxHealth { get; set; }
    [Networked] public int CurrentHealth { get; set; }
    [Networked] public float MaxSpeed { get; set; }
    [Networked] public float Nitro { get; set; }
    [Networked] public int Kills { get; set; }
    [Networked] public int Deaths { get; set; }
    [Networked] public SpecialType SpecialAmmo { get; set; }
    [Networked] public bool IsDead { get; set; }
    [Networked] public float RespawnTimer { get; set; }

    public enum SpecialType { None, Stun, Fire }

    public void InitStats(int carType)
    {
        if (carType == 0)
        {
            MaxHealth = 100;
            MaxSpeed = 18f;
        }
        else
        {
            MaxHealth = 200;
            MaxSpeed = 12f;
        }
        CurrentHealth = MaxHealth;
        Nitro = 1f;
        SpecialAmmo = SpecialType.None;
        IsDead = false;
        RespawnTimer = 0f;
    }

    public void ModifyLife(int amount, PlayerRef? attacker = null)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        if (CurrentHealth <= 0)
        {
            Die(attacker);
        }
    }

    private void Die(PlayerRef? attacker)
    {
        IsDead = true;
        RespawnTimer = 1.0f; // 1 segundo para respawn

        if (attacker != null && Runner.TryGetPlayerObject(attacker.Value, out NetworkObject playerObj))
        {
            if (playerObj.TryGetBehaviour(out ModelPlayer attackerModel))
            {
                attackerModel.Kills++;
            }
        }
        Deaths++;
    }

    public void RespawnAt(Vector3 pos, Quaternion rot)
    {
        CurrentHealth = MaxHealth;
        transform.position = pos;
        transform.rotation = rot;
        IsDead = false;
        RespawnTimer = 0f;
        SpecialAmmo = SpecialType.None;
    }

    public void SetSpecial(SpecialType type)
    {
        SpecialAmmo = type;
    }
}