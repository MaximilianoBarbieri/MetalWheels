using Fusion;
using UnityEngine;

public class ViewPlayer : MonoBehaviour
{
    [SerializeField] private Renderer carRenderer;
    [SerializeField] private GameObject stunFX;
    [SerializeField] private GameObject nitroFX;
    [SerializeField] private GameObject damageFX;
    [SerializeField] private GameObject fireFX;
    [SerializeField] private GameObject burnFX;

    private ModelPlayer _model;
    private LifeHandler _lifeHandler;
    private WeaponHandler _weaponHandler;
    
    private void Awake()
    {
        _model = GetComponent<ModelPlayer>();
        _lifeHandler = GetComponent<LifeHandler>();
        _weaponHandler = GetComponent<WeaponHandler>();
    }

    private void Start()
    {
        // FX de Daño
        if (_lifeHandler != null)
            _lifeHandler.OnTakeDamageFX += PlayDamageFX;

        // FX de disparo
        if (_weaponHandler != null)
            _weaponHandler.OnShoot += PlayFireFX;
    }

    private void OnDestroy()
    {
        if (_lifeHandler != null)
            _lifeHandler.OnTakeDamageFX -= PlayDamageFX;
        if (_weaponHandler != null)
            _weaponHandler.OnShoot -= PlayFireFX;
    }

    private void Update()
    {
        // FX de Stun (activo mientras IsStunned)
        if (stunFX != null)
            stunFX.SetActive(_model != null && _model.IsStunned);
        
        // FX de Burn (activo mientras IsBurning)
        if (burnFX != null)
            burnFX.SetActive(_model != null && _model.IsBurning);

        // FX de Nitro (activo mientras se usa nitro)
        if (nitroFX != null)
            nitroFX.SetActive(_model != null && _model.IsNitroActive);
    }

    public void PlayDamageFX()
    {
        if (damageFX)
        {
            damageFX.SetActive(false);
            damageFX.SetActive(true);
        }
    }

    private void PlayFireFX()
    {
        if (!fireFX) return;
        fireFX.SetActive(false);
        fireFX.SetActive(true);
    }

    public void SetCarMaterial(Material mat)
    {
        if (carRenderer) carRenderer.material = mat;
    }
}