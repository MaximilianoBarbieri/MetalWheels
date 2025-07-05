using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

[RequireComponent(typeof(NetworkRigidbody3D))]
public class Bullet : NetworkBehaviour
{
    private NetworkRigidbody3D _networkRb;
    private TickTimer _lifeTimeTickTimer = TickTimer.None;

    [Header("Bullet Settings")]
    [SerializeField] private float _force = 150f; // Ajustá el valor en cada prefab
    [SerializeField] private byte _damage = 25;
    [SerializeField] private ModelPlayer.SpecialType _specialType = ModelPlayer.SpecialType.None;

    [Header("Special FX")]
    [SerializeField] private ParticleSystem _impactParticles;
    [SerializeField] private GameObject _fireEffectPrefab;

    private void Awake()
    {
        _networkRb = GetComponent<NetworkRigidbody3D>();
    }

    public override void Spawned()
    {
        _networkRb.Rigidbody.AddForce(transform.forward * _force, ForceMode.VelocityChange);

        if (Object.HasStateAuthority)
            _lifeTimeTickTimer = TickTimer.CreateFromSeconds(Runner, 2);
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
        if (!Object || !Object.HasStateAuthority) return;

        bool didHit = false;

        // Si golpea un player/enemigo
        if (other.TryGetComponent(out LifeHandler lifeHandler))
        {
            lifeHandler.ModifyLife(_damage);

            // Efecto STUN
            if (_specialType == ModelPlayer.SpecialType.Stun)
            {
                var model = lifeHandler.GetComponent<ModelPlayer>();
                model?.Stun(2f); // 2 segundos de stun
            }

            // Efecto FIRE (ejemplo simple: daño extra o DOT futuro)
            if (_specialType == ModelPlayer.SpecialType.Fire)
            {
                var model = lifeHandler.GetComponent<ModelPlayer>();
                // Aquí podrías implementar un efecto DOT o quemadura
                // Por ahora, simplemente podrías aplicar más daño
                // model?.ApplyBurningEffect(); // Si tuvieras este método
            }

            didHit = true;
        }

        // Spawnea partículas de impacto si tenés
        if (_impactParticles != null && didHit)
        {
            _impactParticles.transform.position = transform.position;
            _impactParticles.Play();
        }

        // Si querés dejar una marca de fuego, instanciala
        if (_specialType == ModelPlayer.SpecialType.Fire && _fireEffectPrefab != null && didHit)
        {
            Instantiate(_fireEffectPrefab, transform.position, Quaternion.identity);
        }

        DespawnObject();
    }
}