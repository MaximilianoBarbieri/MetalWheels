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
    
    public PlayerRef OwnerPlayerRef { get; private set; }

    public void SetOwner(PlayerRef playerRef)
    {
        OwnerPlayerRef = playerRef;
    }

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

        // 1) Si choca con PLAYER
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out LifeHandler lifeHandler))
            {
                // Es mi propio dueño, ignorar el daño
                if (lifeHandler.PlayerRef == OwnerPlayerRef) return;
                
                Debug.Log($"[Bullet] Haciendo daño a {other.name} de {OwnerPlayerRef.ToString()}");
                lifeHandler.ModifyLife(-_damage, OwnerPlayerRef);

                // Efecto STUN
                if (_specialType == ModelPlayer.SpecialType.Stun)
                {
                    var model = lifeHandler.GetComponent<ModelPlayer>();
                    model.Stun(3f); // 3 segundos de stun
                }

                // Efecto Fire
                if (_specialType == ModelPlayer.SpecialType.Fire)
                {
                    var model = lifeHandler.GetComponent<ModelPlayer>();
                    model.Burn(3f, 1f, OwnerPlayerRef);
                }
            }

            PlayImpactFX();
            DespawnObject();
            return;
        }

        // 2) Si choca con OBJECT o NPC, solo se destruye
        if (other.CompareTag("Object") || other.CompareTag("NPC"))
        {
            PlayImpactFX();
            DespawnObject();
            return;
        }

        // Si querés, podés agregar lógica para otros tags acá
    }

    // Método para manejar efectos visuales al impactar
    void PlayImpactFX()
    {
        if (_impactParticles != null)
        {
            _impactParticles.transform.position = transform.position;
            _impactParticles.Play();
        }
        if (_specialType == ModelPlayer.SpecialType.Fire && _fireEffectPrefab != null)
        {
            Instantiate(_fireEffectPrefab, transform.position, Quaternion.identity);
        }
    }

}