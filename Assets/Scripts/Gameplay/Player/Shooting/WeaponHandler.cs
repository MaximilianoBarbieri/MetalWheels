using Fusion;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _bulletNormalPrefab;
    [SerializeField] private NetworkPrefabRef _bulletStunPrefab;
    [SerializeField] private NetworkPrefabRef _bulletFirePrefab;

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

    public void FireNormal()
    {
        if (!HasStateAuthority) return;
        _spawnedBullet = !_spawnedBullet;
        Runner.Spawn(_bulletNormalPrefab, _firingPositionTransform.position, transform.rotation);
    }

    public void FireSpecial(ModelPlayer.SpecialType specialType)
    {
        if (!HasStateAuthority) return;

        var prefabToUse = specialType switch
        {
            ModelPlayer.SpecialType.Stun => _bulletStunPrefab,
            ModelPlayer.SpecialType.Fire => _bulletFirePrefab
        };

        if (prefabToUse != null)
        {
            _spawnedBullet = !_spawnedBullet;
            Runner.Spawn(prefabToUse, _firingPositionTransform.position, transform.rotation);
        }
    }

    void RemoteParticles()
    {
        if (_shootingParticles != null)
            _shootingParticles.Play();
    }
}