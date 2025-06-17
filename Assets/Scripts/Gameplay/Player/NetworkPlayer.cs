using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer Local { get; private set; }

    /*private NickNameBarLifeItem _myItemUI;*/

    public event Action OnPlayerDespawned = delegate { };

    [Networked] private NetworkString<_16> NickName { get; set; }

    /*LifeHandler lifeHandler;

    public LifeHandler SendMyLiFeHandler() => lifeHandler;*/

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasInputAuthority)
        {
            Local = this;

            RPC_SetNewName(PlayerPrefs.GetString("UserNickName"));

            //GetComponentInChildren<MeshRenderer>().material.color = Color.blue; //TODO: ACA PODRIAMOS SETEARLE AL OTRO PLAYER OTRA MESH
        }
        else
        {
            //GetComponentInChildren<MeshRenderer>().material.color = Color.red; //TODO: ACA PODRIAMOS SETEARLE AL OTRO PLAYER OTRA MESH
            UpdateNickname();
        }
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(NickName):
                    UpdateNickname();
                    break;
            }
        }
    }

    void UpdateNickname()
    {
       // _myItemUI?.UpdateNickName(NickName.Value);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetNewName(string newNickName)
    {
        NickName = newNickName;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        OnPlayerDespawned();
    }
}