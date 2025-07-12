using Fusion;
using UnityEngine;

public class ModelPlayer : NetworkBehaviour
{
    [Networked] public int MaxHealth { get; private set; }
    [Networked] public int CurrentHealth { get; private set; }
    [Networked] public float MaxNitro { get; set; }
    [Networked] public float CurrentNitro { get; set; } 
    [Networked] public int Kills { get; private set; }
    [Networked] public int Deaths { get; private set; }
    [Networked] public SpecialType SpecialAmmo { get; private set; }
    [Networked] public bool IsDead { get; private set; }
    [Networked] public float RespawnTimer { get; private set; }
    [Networked] public bool IsStunned { get; private set; }
    [Networked] public float StunTimer { get; private set; }
    [Networked] public int CarType { get; set; }

    public enum SpecialType { None, Stun, Fire }
    
    public void InitStats(int carType)
    {
        CarType = carType;
        MaxHealth = CarType == 0 ? 100 : 200;
        CurrentHealth = MaxHealth;
        CurrentNitro = MaxNitro; 
        SpecialAmmo = SpecialType.None;
        IsDead = false;
        RespawnTimer = 0f;
    }

    public bool ConsumeNitro(float amount)
    {
        if (CurrentNitro >= amount) {
            CurrentNitro -= amount;
            CurrentNitro = Mathf.Max(0, CurrentNitro);
            return true;
        }
        CurrentNitro = 0;
        return false;
    }
    
    public void AddNitro(float amount)
    {
        CurrentNitro = Mathf.Min(CurrentNitro + amount, MaxNitro);
    }

    public void UpdateStats(float deltaTime)
    {
        UpdateStun(deltaTime);
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
        CurrentNitro = MaxNitro;
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

    public void Stun(float duration)
    {
        IsStunned = true;
        StunTimer = duration;
    }

    private void UpdateStun(float deltaTime)
    {
        if (!IsStunned) return;
        
        StunTimer -= deltaTime;
        if (StunTimer <= 0f)
        {
            IsStunned = false;
            StunTimer = 0f;
        }
    }
}