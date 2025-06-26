using System;
using System.Collections;
using Fusion;
using UnityEngine;

public class LifeHandler : NetworkBehaviour
{
    [SerializeField] private GameObject _playerVisual;

    [Networked, OnChangedRender(nameof(OnLifeChanged))]
    private byte CurrentLife { get; set; }

    private const byte MAX_LIFE = 100;
    private const byte MAX_DEADS = 2;

    private byte _currentDeads;

    [Networked] NetworkBool IsDead { get; set; }

    private ChangeDetector _changeDetector;

    public event Action<bool> OnDeadChange = delegate { };
    public event Action<float> OnLifeUpdate = delegate { };
    public event Action OnRespawn = delegate { };
    public event Action OnDespawn = delegate { };

    private NickNameBarLife _myItemUI;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        CurrentLife = MAX_LIFE;
    }

    public void GetMyUI(NickNameBarLife item) => _myItemUI = item;

    //Al morir, si es mi primera vez, revivir a los 2 segundos
    //Si es mi segunda vez, desconectar al jugador
    public void TakeDamage(byte dmg)
    {
        if (dmg > CurrentLife) dmg = CurrentLife;

        CurrentLife -= dmg;

        if (CurrentLife == 0)
        {
            _currentDeads++;
            if (_currentDeads == MAX_DEADS)
            {
                DisconnectPlayer();
            }
            else
            {
                StartCoroutine(Server_RespawnCooldown());
                IsDead = true;
            }
        }
    }

    IEnumerator Server_RespawnCooldown()
    {
        yield return new WaitForSeconds(2f);

        Server_Revive();
    }

    void Server_Revive()
    {
        OnRespawn();
        IsDead = false;
        CurrentLife = MAX_LIFE;
    }

    void DisconnectPlayer()
    {
        if (!Object.HasInputAuthority)
            Runner.Disconnect(Object.InputAuthority);

        Runner.Despawn(Object);
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(IsDead):
                    OnDeadChanged();
                    break;
            }
        }
    }

    void OnDeadChanged()
    {
        if (IsDead)
            RemoteDead();
        else
            RemoteRespawn();

        OnDeadChange(IsDead);
    }

    void RemoteDead()
    {
        _playerVisual.SetActive(false);
    }

    void RemoteRespawn()
    {
        _playerVisual.SetActive(true);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        OnDespawn();
    }

    void OnLifeChanged()
    {
        //_myItemUI.UpdateLifeBar(CurrentLife / MAX_LIFE);
    }
}