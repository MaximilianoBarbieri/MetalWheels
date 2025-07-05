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

    public event Action<float> OnLifeUpdate = delegate { };
    public event Action OnRespawn = delegate { };
    public event Action OnDead = delegate { };

    public override void Spawned()
    {
        _model = GetComponent<ModelPlayer>();
        CurrentLife = _model.CurrentHealth;
        
        
        UpdateUI(); // <-- AGREGADO
    }

    public void GetMyUI(PlayerGlobalUIHandler ui)
    {
        _globalUI = ui;
        UpdateUI();
    }

    public void ModifyLife(int delta)
    {
        if (_model.IsDead) return;
        
        // Modifico en el modelo, el cual maneja el estado real
        _model.ModifyLife(delta);

        // Actualizo la propiedad local networked
        CurrentLife = _model.CurrentHealth;
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
        OnRespawn?.Invoke();
    }
}