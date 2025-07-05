using Fusion;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _bulletNormalPrefab;
    [SerializeField] private NetworkPrefabRef _bulletStunPrefab;
    [SerializeField] private NetworkPrefabRef _bulletFirePrefab;

    [SerializeField] private Transform _firingPositionTransform;
    [SerializeField] private ParticleSystem _shootingParticles;

    private double  _nextFireTimeNormal = 0;
    [SerializeField] private float fireCooldown = 1f; // 1 segundo para fire
    
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
        if (Runner.SimulationTime < _nextFireTimeNormal) {
            Debug.Log($"[Cooldown] ¡Todavía no podés disparar! Faltan: {_nextFireTimeNormal - Runner.SimulationTime:F2}s");
            return;
        }
        if (Runner.SimulationTime < _nextFireTimeNormal) return;

        _spawnedBullet = !_spawnedBullet;
        Runner.Spawn(_bulletNormalPrefab, _firingPositionTransform.position, transform.rotation);

        _nextFireTimeNormal = Runner.SimulationTime + fireCooldown;
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