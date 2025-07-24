using System;
using System.Collections;
using Fusion;
using UnityEngine;

public class LifeHandler : NetworkBehaviour
{
    [SerializeField] private GameObject _playerVisual;

    [Networked, OnChangedRender(nameof(OnLifeChanged))] 
    private int CurrentLife { get; set; }

    private ModelPlayer _model;
    private PlayerGlobalUIHandler _globalUI;
    
    public event Action OnTakeDamageFX = delegate { };
    public event Action<float> OnLifeUpdate = delegate { };
    public event Action OnRespawn = delegate { };
    public event Action OnDead = delegate { };

    public override void Spawned()
    {
        _model = GetComponent<ModelPlayer>();
        CurrentLife = _model.CurrentHealth;
        
        OnDead += DisableVisuals; //se puede agregar efectos visuales aca
        OnRespawn += EnableVisuals; //se puede agregar efectos visuales aca
        
        UpdateUI(); // <-- AGREGADO
    }
    
    private void DisableVisuals() => _playerVisual.SetActive(false);
    private void EnableVisuals() => _playerVisual.SetActive(true);

    public void GetMyUI(PlayerGlobalUIHandler ui)
    {
        _globalUI = ui;
        UpdateUI();
    }

    public void ModifyLife(int delta, PlayerRef? attacker = null)
    {
        if (_model.IsDead) return;
        _model.ModifyLife(delta, attacker);
        CurrentLife = _model.CurrentHealth;
        
        if (delta < 0) {
            _model.DamageFXCounter++;
        }
    }

    void OnLifeChanged()
    {
        UpdateUI();

        if (!_model.IsDead) return;
        OnDead?.Invoke();
        StartCoroutine(RespawnRoutine());
    }

    public void UpdateUI()
    {
        if (_model == null || _model.MaxHealth <= 0)
        {
            // Si el modelo no existe todavía, poné la barra vacía hasta que exista.
            _globalUI?.UpdateLifeBar(0f);
            OnLifeUpdate(0f);
            return;
        }
        float normalizedLife = Mathf.Clamp01((float)CurrentLife / _model.MaxHealth);
        _globalUI?.UpdateLifeBar(normalizedLife);
        OnLifeUpdate(normalizedLife);
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(_model.RespawnTimer);

        //Obtengo el SpawnPoint seleccionado en el spawner por el player
        Vector3 respawnPos = PlayerSpawner.Instance.GetSpawnPointForPlayer(Object.InputAuthority);
        Quaternion respawnRot = Quaternion.identity;
        _model.RespawnAt(respawnPos, respawnRot);

        OnRespawn?.Invoke();
    }
}