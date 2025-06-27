using System;
using System.Collections;
using Fusion;
using UnityEngine;

public class LifeHandler : NetworkBehaviour
{
    [SerializeField] private GameObject _playerVisual;

    [Networked, OnChangedRender(nameof(OnLifeChanged))] 
    private byte CurrentLife { get; set; }

    [Networked] private NetworkBool IsDead { get; set; }

    private byte _maxLife;
    private ChangeDetector _detector;

    private NickNameBarLife _globalUI;

    public event Action<float> OnLifeUpdate = delegate { };
    public event Action OnRespawn = delegate { };
    public event Action OnDead = delegate { };

    public override void Spawned()
    {
        _detector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // Obtengo el max life del ModelPlayer
        var model = GetComponent<ModelPlayer>();
        _maxLife = (byte)model.MaxHealth;
        CurrentLife = _maxLife;
    }

    // NetworkPlayer se encargará de llamarme tras crear la UI
    public void GetMyUI(NickNameBarLife ui)
    {
        _globalUI = ui;
        // primera actualización para que no se quede vacía
        var norm = (float)CurrentLife / _maxLife;
        _globalUI.UpdateLifeBar(norm);
    }

    // Método público para dañar o curar
    public void ModifyLife(int delta)
    {
        if (IsDead) return;

        var newLife = Mathf.Clamp(CurrentLife + delta, 0, _maxLife);
        CurrentLife = (byte)newLife;
    }

    // Cuando cambia CurrentLife en la red
    public override void Render()
    {
        foreach (var change in _detector.DetectChanges(this))
            if (change == nameof(CurrentLife))
                OnLifeChanged();
    }

    void OnLifeChanged()
    {
        float norm = (float)CurrentLife / _maxLife;
        // UI global
        _globalUI?.UpdateLifeBar(norm);
        // evento para UI local
        OnLifeUpdate(norm);

        if (CurrentLife == 0 && !IsDead)
        {
            // muero por primera vez
            IsDead = true;
            OnDead?.Invoke();
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(2f);
        // respawneo
        IsDead = false;
        CurrentLife = _maxLife;
        OnRespawn?.Invoke();
    }
}