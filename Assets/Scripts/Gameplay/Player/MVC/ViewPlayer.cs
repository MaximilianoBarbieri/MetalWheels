using UnityEngine;

public class ViewPlayer : MonoBehaviour
{
    [SerializeField] private Renderer carRenderer;
    [SerializeField] private GameObject stunFX;
    [SerializeField] private GameObject nitroFX;
    [SerializeField] private GameObject damageFX;
    [SerializeField] private GameObject fireFX;
    
    private ModelPlayer _model;
    private LifeHandler _lifeHandler;
    private WeaponHandler _weaponHandler;
    
    private bool isNitroActive;

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

        // FX de nitro
        NitroEvents.OnNitroStateChanged += OnNitroStateChanged;
    }
    
    private void OnDestroy()
    {
        if (_lifeHandler != null)
            _lifeHandler.OnTakeDamageFX -= PlayDamageFX;
        if (_weaponHandler != null)
            _weaponHandler.OnShoot -= PlayFireFX;

        NitroEvents.OnNitroStateChanged -= OnNitroStateChanged;
    }
    
    private void Update()
    {
        // FX de Stun (activo mientras IsStunned)
        if (stunFX != null)
            stunFX.SetActive(_model != null && _model.IsStunned);

        // FX de Nitro (activo mientras se usa nitro)
        if (nitroFX != null)
            nitroFX.SetActive(isNitroActive);
    }

    public void PlayStunFX() { if (stunFX) stunFX.SetActive(true); }
    public void PlayNitroFX() { if (nitroFX) nitroFX.SetActive(true); }
    public void PlayDamageFX() { if (damageFX) { damageFX.SetActive(false); damageFX.SetActive(true); } } // Reset para efectos breves
    public void PlayFireFX() { if (fireFX) { fireFX.SetActive(false); fireFX.SetActive(true); } }
    public void SetCarMaterial(Material mat) { if (carRenderer) carRenderer.material = mat; }

    private void OnNitroStateChanged(ModelPlayer model, bool active)
    {
        // Solo activo mi FX si soy yo
        if (model == _model) isNitroActive = active;
    }
}