using Fusion;
using UnityEngine;

public class ModelPlayer : NetworkBehaviour
{
    [Networked] public int MaxHealth { get; private set; }
    [Networked] public int CurrentHealth { get; private set; }
    [Networked] public float MaxNitro { get; set; }
    [Networked] public float CurrentNitro { get; set; } 
    [Networked] public int Kills { get; private set; }
    [Networked] public SpecialType SpecialAmmo { get; private set; }
    [Networked] public bool IsDead { get; private set; }
    [Networked] public float RespawnTimer { get; private set; }
    [Networked] public float StunTimer { get; private set; }
    [Networked] public bool IsStunned { get; private set; }
    [Networked] public bool IsBurning { get; private set; }
    [Networked] public float BurnTimer { get; private set; }
    [Networked] public float BurnTickTimer { get; private set; }
    [Networked] private PlayerRef BurnAttacker { get; set; }
    [Networked] public int CarType { get; set; }
    public enum SpecialType { None, Stun, Fire }
    
    private LifeHandler _lifeHandler;
    
    [Networked, OnChangedRender(nameof(OnNitroChanged))]
    public bool IsNitroActive { get; set; }
    [Networked, OnChangedRender(nameof(OnDamageFXChanged))]
    public int DamageFXCounter { get; set; }

    
    public override void Spawned()
    {
        _lifeHandler = GetComponent<LifeHandler>();
    }
    
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
    
    public void UpdateStats(float deltaTime)
    {
        UpdateStun(deltaTime);
        UpdateBurn(deltaTime);
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
        Debug.Log($"[Die] Ejecutando Die en {name} | StateAuthority: {HasStateAuthority} | Attacker: {attacker}");

        IsDead = true;
        RespawnTimer = 1.0f;

        if (!HasStateAuthority) return;

        if (attacker != null)
        {
            if (Runner.TryGetPlayerObject(attacker.Value, out NetworkObject playerObj))
            {
                Debug.Log($"[Die] Encontrado NetworkObject para attacker: {attacker} -> {playerObj.name}");
                if (playerObj.TryGetBehaviour(out ModelPlayer attackerModel))
                {
                    attackerModel.AddKill();
                    Debug.Log($"[Die] Kill sumada a: {playerObj.name}");
                }
                else
                {
                    Debug.Log("[Die] No se encontró ModelPlayer en el NetworkObject del atacante.");
                }
            }
            else
            {
                Debug.Log("[Die] No se encontró NetworkObject para attacker PlayerRef.");
            }
        }
    }


    private void AddKill()
    {
        Kills++;
        Debug.Log($"[ModelPlayer] Nueva Kill, total={Kills}");
    }

    public void RespawnAt(Vector3 pos, Quaternion rot)
    {
        CurrentHealth = MaxHealth;
        CurrentNitro = MaxNitro;
        transform.position = pos;
        transform.rotation = rot;
        IsDead = false;
        IsStunned = false;
        IsBurning = false;
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
    
    public void Burn(float duration, float tickInterval, PlayerRef attacker)
    {
        IsBurning = true;
        BurnTimer = duration;
        BurnTickTimer = tickInterval;
        BurnAttacker = attacker;
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
    
    private void UpdateBurn(float deltaTime)
    {
        if (!IsBurning) return;

        BurnTimer -= deltaTime;
        BurnTickTimer -= deltaTime;

        if (BurnTickTimer <= 0f)
        {
            if (_lifeHandler != null)
            {
                _lifeHandler.ModifyLife(-5, BurnAttacker); // Usamos LifeHandler aquí
            }

            BurnTickTimer = 1f;
        }

        if (BurnTimer <= 0f)
        {
            IsBurning = false;
            BurnTimer = 0f;
        }
    }
    
    private void OnNitroChanged()
    {
        // Vacío si usás Update en ViewPlayer, o podés llamar FX directo acá
    }

    private void OnDamageFXChanged()
    {
        var view = GetComponent<ViewPlayer>();
        if (view != null) view.PlayDamageFX();
    }
}