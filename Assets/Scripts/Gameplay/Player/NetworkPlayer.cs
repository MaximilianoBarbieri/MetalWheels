using System;
using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer Local { get; private set; }

    private NickNameBarLife _myItemUI;
    private LifeHandler _lifeHandler;
    
    //public event Action OnPlayerDespawned = delegate { };

    [Networked, OnChangedRender(nameof(OnNickNameChanged))]
    string NickName { get; set; }



    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            // Configuramos nuestro nickname en el servidor
            RPC_SetNewName(PlayerData.Nickname);
        }
        else
        {
            // Sólo para los demás jugadores
            _myItemUI = NickNameBarLifeManager.Instance.CreateNewItem(this);
            _myItemUI.Init(NickName, transform);

            // Conectamos LifeHandler → UI global
            _lifeHandler = GetComponent<LifeHandler>();
            _lifeHandler.GetMyUI(_myItemUI);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetNewName(string newNickName)
    {
        NickName = newNickName;
    }

    void OnNickNameChanged()
    {
        if (_myItemUI != null)
            _myItemUI.UpdateNickName(NickName);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        //OnPlayerDespawned();
        // Limpiar UI global al despawnear
        if (_myItemUI != null) Destroy(_myItemUI.gameObject);
    }
}