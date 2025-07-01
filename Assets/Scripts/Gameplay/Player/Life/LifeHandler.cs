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
    private NickNameBarLife _globalUI;

    public event Action<float> OnLifeUpdate = delegate { };
    public event Action OnRespawn = delegate { };
    public event Action OnDead = delegate { };

    public override void Spawned()
    {
        _model = GetComponent<ModelPlayer>();
        CurrentLife = _model.CurrentHealth;
    }

    public void GetMyUI(NickNameBarLife ui)
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

        if (_model.IsDead)
        {
            OnDead?.Invoke();
            StartCoroutine(RespawnRoutine());
        }
    }

    void UpdateUI()
    {
        float norm = (float)CurrentLife / _model.MaxHealth;
        _globalUI?.UpdateLifeBar(norm);
        OnLifeUpdate(norm);
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(_model.RespawnTimer);
        OnRespawn?.Invoke();
    }
}