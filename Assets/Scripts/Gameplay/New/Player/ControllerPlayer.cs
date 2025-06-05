using Fusion;
using UnityEngine;

public class ControllerPlayer : NetworkBehaviour
{
    [SerializeField] public ModelPlayer model;
    [SerializeField] public ViewPlayer view;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectileNormalPrefab;
    [SerializeField] private GameObject projectileSpecialPrefab;

    [SerializeField] private float jumpCooldown = 2f;
    [SerializeField] private float minCrashForce = 7f;
    [SerializeField] private int crashDamage = 30;

    private float nextJumpTime = 0f;

    public override void Spawned()
    {
        model.InitStats(PlayerData.CarSelected);
    }

    public override void FixedUpdateNetwork()
    {
        Debug.Log("FIXED UPDATE");
        
        if (!HasInputAuthority) return;

        // Respawn timer lógica
        if (model.IsDead)
        {
            model.RespawnTimer -= Runner.DeltaTime;
            if (model.RespawnTimer <= 0)
            {
                // Buscar spawn point libre y respawnear
                Transform spawn = SpawnManager.Instance.GetFreeSpawnPoint();
                model.RespawnAt(spawn.position, spawn.rotation);
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        // Movimiento tipo auto
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        Vector3 forward = transform.forward * v * model.MaxSpeed;
        rb.AddForce(forward, ForceMode.Acceleration);
        if (Mathf.Abs(v) > 0.1f)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, h * 2f, 0));

        // Salto con cooldown
        if (Input.GetKeyDown(KeyCode.Space) && Runner.SimulationTime >= nextJumpTime)
        {
            rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
            nextJumpTime = (float)Runner.SimulationTime + jumpCooldown;
        }

        // Nitro
        if (Input.GetKey(KeyCode.LeftShift) && model.Nitro > 0f)
        {
            rb.AddForce(transform.forward * model.MaxSpeed * 0.7f, ForceMode.Acceleration);
            model.Nitro -= Runner.DeltaTime * 0.3f;
            model.Nitro = Mathf.Clamp01(model.Nitro);
        }

        // Shoot normal
        if (Input.GetKeyDown(KeyCode.O))
        {
            Runner.Spawn(projectileNormalPrefab, shootPoint.position, shootPoint.rotation, Object.InputAuthority);
        }

        // Shoot special
        if (Input.GetKeyDown(KeyCode.P) && model.SpecialAmmo != ModelPlayer.SpecialType.None)
        {
            var proj = Runner.Spawn(projectileSpecialPrefab, shootPoint.position, shootPoint.rotation, Object.InputAuthority);
            if (proj != null && proj.GetComponent<Projectile>() != null)
                proj.GetComponent<Projectile>().specialType = model.SpecialAmmo;
            model.SpecialAmmo = ModelPlayer.SpecialType.None;
        }
    }

    // Daño por colisión/crash
    private void OnCollisionEnter(Collision collision)
    {
        if (!HasInputAuthority || model.IsDead) return;

        // Detectar si es otro auto/jugador
        ModelPlayer otherModel = collision.gameObject.GetComponent<ModelPlayer>();
        if (otherModel != null && collision.contacts.Length > 0)
        {
            Vector3 contactNormal = collision.contacts[0].normal;
            float forwardDot = Vector3.Dot(contactNormal, -transform.forward);
            if (forwardDot > 0.7f && collision.relativeVelocity.magnitude > minCrashForce)
            {
                otherModel.ModifyLife(-crashDamage, Object.InputAuthority);
            }
        }
    }
}