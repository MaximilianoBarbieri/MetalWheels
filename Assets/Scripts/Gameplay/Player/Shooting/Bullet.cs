using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

[RequireComponent(typeof(NetworkRigidbody3D))]
public class Bullet : NetworkBehaviour
{
    private NetworkRigidbody3D _networkRb;

    TickTimer _lifeTimeTickTimer = TickTimer.None;

    [SerializeField] private byte _damage;
    
    private void Awake()
    {
        _networkRb = GetComponent<NetworkRigidbody3D>();
    }

    public override void Spawned()
    {
        _networkRb.Rigidbody.AddForce(transform.forward * 20, ForceMode.VelocityChange);

        if (Object.HasStateAuthority)
        {
            _lifeTimeTickTimer = TickTimer.CreateFromSeconds(Runner, 2);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        if (_lifeTimeTickTimer.Expired(Runner))
        {
            DespawnObject();
        }
    }

    void DespawnObject()
    {
        _lifeTimeTickTimer = TickTimer.None;
        
        Runner.Despawn(Object);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Object && Object.HasStateAuthority)
        {
            if (other.TryGetComponent(out LifeHandler lifeHandler))
            {
                lifeHandler.ModifyLife(_damage);
            }
            
            DespawnObject();
        }
    }
}
