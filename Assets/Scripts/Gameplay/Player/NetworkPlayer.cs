using System;
using UnityEngine;
using Fusion;
public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer Local { get; private set; }

    private NickNameBarLife _myItemUI;

    public event Action OnPlayerDespawned = delegate { };

    [Networked, OnChangedRender(nameof(OnNickNameChanged))]
    string NickName { get; set; }

    LifeHandler lifeHandler;

    public LifeHandler SendMyLiFeHandler() => lifeHandler;

    public override void Spawned()
    {
        _myItemUI = NickNameBarLifeManager.Instance.CreateNewItem(this);
        lifeHandler = GetComponent<LifeHandler>();
        lifeHandler.GetMyUI(_myItemUI);

        if (Object.HasInputAuthority)
        {
            Local = this;

            RPC_SetNewName(PlayerPrefs.GetString("PlayerNickName"));
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetNewName(string newNickName)
    {
        NickName = newNickName;
    }

    void OnNickNameChanged()
    {
        _myItemUI.UpdateNickName(NickName);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        OnPlayerDespawned();
    }
}