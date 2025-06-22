using Fusion;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    //TODO: aca irian los 2 o 3 bullet prefabs (normal, special [stun & fire]) 
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private Transform _firingPositionTransform;
    [SerializeField] private ParticleSystem _shootingParticles;

    [Networked] NetworkBool _spawnedBullet { get; set; }
    
    private ChangeDetector _changeDetector;
    
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(_spawnedBullet):
                    RemoteParticles();
                    break;
            }
        }
    }


    public void Fire()
    {
        if (!HasStateAuthority) return;
        
        _spawnedBullet = !_spawnedBullet;
        
        Runner.Spawn(_bulletPrefab, _firingPositionTransform.position, transform.rotation);
    }

    void RemoteParticles()
    {
        _shootingParticles.Play();
    }
}