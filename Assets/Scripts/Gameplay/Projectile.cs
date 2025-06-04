using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public int damage = 20;
    public float speed = 25f;

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthController health))
        {
            health.TakeDamage(damage, Object.InputAuthority);
            Runner.Despawn(Object);
        }
    }
}
