using Fusion;
using UnityEngine;

// Controla entradas y comunica Model <-> View
public class ControllerPlayer : NetworkBehaviour
{
    [SerializeField] private ModelPlayer model;
    [SerializeField] private ViewPlayer view;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform shootPoint;

    private float jumpCooldown = 2f;
    private float nextJumpTime = 0f;
    private bool canNitro => model.Nitro > 0f;

    public override void Spawned()
    {
        model.InitStats(PlayerData.CarSelected);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        // Movimiento tipo auto: aceleración + giro
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        Vector3 forward = transform.forward * v * model.MaxSpeed;
        rb.AddForce(forward, ForceMode.Acceleration);
        if (Mathf.Abs(v) > 0.1f)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, h * 2f, 0));

        // Jump con cooldown
        if (Input.GetKeyDown(KeyCode.Space) && Runner.SimulationTime >= nextJumpTime)
        {
            rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
            nextJumpTime = (float)Runner.SimulationTime + jumpCooldown;
        }

        // Nitro
        if (Input.GetKey(KeyCode.LeftShift) && canNitro)
        {
            rb.AddForce(transform.forward * model.MaxSpeed * 0.7f, ForceMode.Acceleration);
            model.Nitro -= Runner.DeltaTime * 0.3f;
            model.Nitro = Mathf.Clamp01(model.Nitro);
        }

        // Shoot normal
        if (Input.GetKeyDown(KeyCode.O))
        {
            // Instanciar proyectil normal aquí
        }

        // Shoot special
        if (Input.GetKeyDown(KeyCode.P) && model.SpecialAmmo != ModelPlayer.SpecialType.None)
        {
            // Instanciar proyectil especial según tipo y vaciar SpecialAmmo
            model.SpecialAmmo = ModelPlayer.SpecialType.None;
        }
    }
}