using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Networked] public int damage { get; set; }
    [Networked] public ModelPlayer.SpecialType specialType { get; set; } = ModelPlayer.SpecialType.None;
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifeTime = 3f;

    public override void Spawned()
    {
        // Auto destruir después de cierto tiempo (en red)
        Invoke(nameof(SelfDestruct), lifeTime);
    }

    private void Update()
    {
        // Movimiento simple hacia adelante
        transform.Translate(Vector3.forward * (speed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return; // Solo el host aplica daño y destruye

        var targetModel = other.GetComponent<ModelPlayer>();
        if (targetModel != null && !targetModel.IsDead)
        {
            int dmg = damage;
            // Lógica extra para proyectiles especiales
            if (specialType == ModelPlayer.SpecialType.Stun)
            {
                // Aplicar un efecto stun si tenés (agregar lógica más adelante)
            }
            else if (specialType == ModelPlayer.SpecialType.Fire)
            {
                // Podés aplicar daño extra o quemadura aquí
                dmg += 20;
            }

            // Modifica la vida y pasa el autor como atacante
            targetModel.ModifyLife(-dmg, Object.InputAuthority);

            Runner.Despawn(Object);
        }
        // Si toca cualquier otra cosa sólida, también se destruye
        else if (!other.isTrigger)
        {
            Runner.Despawn(Object);
        }
    }

    private void SelfDestruct()
    {
        if (Object != null && Object.IsValid)
            Runner.Despawn(Object);
    }
}