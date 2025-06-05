using Fusion;
using UnityEngine;

// Guarda stats y lógica pura del player
public class ModelPlayer : NetworkBehaviour
{
    [Networked] public int MaxHealth { get; set; }
    [Networked] public int CurrentHealth { get; set; }
    [Networked] public float MaxSpeed { get; set; }
    [Networked] public float Nitro { get; set; }
    [Networked] public int Kills { get; set; }
    [Networked] public int Deaths { get; set; }
    [Networked] public SpecialType SpecialAmmo { get; set; }

    public enum SpecialType { None, Stun, Fire }

    public void InitStats(int carType)
    {
        if (carType == 0) // CarA: veloz, poca vida
        {
            MaxHealth = 100;
            MaxSpeed = 18f;
        }
        else // CarB: lento, mucha vida
        {
            MaxHealth = 200;
            MaxSpeed = 12f;
        }
        CurrentHealth = MaxHealth;
        Nitro = 1f; // Valor entre 0-1 para usar con barra
        SpecialAmmo = SpecialType.None;
    }

    public void ModifyLife(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
    }

    public void SetSpecial(SpecialType type)
    {
        SpecialAmmo = type;
    }
}