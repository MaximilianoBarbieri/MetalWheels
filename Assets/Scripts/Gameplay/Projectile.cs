using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Header("General")]
    [SerializeField] private float _despawnTime = 5f;
    [SerializeField] private float _speed = 20f;
    [SerializeField] private byte _damage;

    [Header("Prefab References")]
    [SerializeField] private NetworkPrefabRef _normalBullet;
    [SerializeField] private NetworkPrefabRef _stunBullet;
    [SerializeField] private NetworkPrefabRef _fireBullet;
    
    private TickTimer _life;
    private Vector3 _direction;
    
    public override void Spawned()
    {
        _life = TickTimer.CreateFromSeconds(Runner, _despawnTime);
        //TODO: hacer que el damage varie dependiendo el tipo de bullet
        _damage = 25;
    }

    public void Init(Vector3 dir )
    {
        _direction = dir.normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (_life.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        transform.position += _direction * _speed * Runner.DeltaTime;
    }

    public void Launch(NetworkRunner runner, Vector3 position, Quaternion rotation, ModelPlayer.SpecialType bulletType, PlayerRef owner)
    {
        NetworkPrefabRef prefabToUse = bulletType switch
        {
            ModelPlayer.SpecialType.Stun => _stunBullet,
            ModelPlayer.SpecialType.Fire => _fireBullet,
            _ => _normalBullet
        };

        runner.Spawn(
            prefabToUse,
            position,
            rotation,
            owner,
            (NetworkRunner r, NetworkObject obj) =>
            {
                var proj = obj.GetComponent<Projectile>();
                proj.Init(rotation * Vector3.forward);
            });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;
        
        if (other.TryGetComponent(out LifeHandler lifeHandler))
        {
            //TODO: hacer que el damage varie dependiendo el tipo de bullet
            lifeHandler.TakeDamage(_damage);
        }
        
        // Acá podés agregar efectos específicos según tipo (stun, fire, etc.)
        Runner.Despawn(Object);
    }
}