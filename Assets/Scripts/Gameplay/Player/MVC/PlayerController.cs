using System.Collections;
using Cinemachine;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
Escucha el evento de GameManager y se desactiva si corresponde.

Alternativamente, el GameManager puede hacer un FindObjectsOfType<PlayerController>() y desactivar directamente.
 */

[RequireComponent(typeof(NetworkCharacterControllerCustom))]
[RequireComponent(typeof(LifeHandler))]
[RequireComponent(typeof(WeaponHandler))]
public class PlayerController : NetworkBehaviour
{
    private ModelPlayer _model;

    private NetworkCharacterControllerCustom _myCharacterController;
    private WeaponHandler _myWeaponHandler;
    private LifeHandler _myLifeHandler;

    public CinemachineVirtualCamera vCamPrefab;
    private CinemachineVirtualCamera myCam;
    
    [SerializeField] private float minCrashForce = 7f;
    [SerializeField] private int crashDamage = 30;
    
    //UI player
    [SerializeField] private PlayerLocalUIHandler playerLocalUIPrefab;
    
    private void Awake()
    {
        _model = GetComponent<ModelPlayer>();
        
        _myCharacterController = GetComponent<NetworkCharacterControllerCustom>();
        _myWeaponHandler = GetComponent<WeaponHandler>();
        _myLifeHandler = GetComponent<LifeHandler>();
        
        _myLifeHandler.OnDead += () => {/* opcional: cámara, efectos, etc */ };

        _myLifeHandler.OnRespawn += () =>
        {
            /* opcional: reposicionar, limpiar estado */
            _myCharacterController.Teleport(transform.position);
        };
    }

    public override void Spawned()
    {
        // Si es mi propio jugador
        if (!Object.HasInputAuthority) return;
        
        if (vCamPrefab != null)
        {
            myCam = Instantiate(vCamPrefab);

            // Buscar el CameraTarget (empty hijo del auto) o el propio transform
            Transform camTarget = transform.Find("CameraTarget");
            if (camTarget == null) camTarget = transform;

            myCam.Follow = camTarget;
            myCam.LookAt = camTarget;
        }
        
        if (playerLocalUIPrefab != null)
        {
            Debug.Log("PlayerController -> Model inicializado antes de ejecutar global canvas: " + _model.MaxHealth);
            Canvas globalCanvas = GameObject.Find("Canvas_LocalUI").GetComponent<Canvas>();
            var uiInstance = Instantiate(playerLocalUIPrefab, globalCanvas.transform);
            uiInstance.Init(_model,this, _myCharacterController, _myLifeHandler);
            
            // Forzar update
            _myLifeHandler?.UpdateUI();
        }
        
        GameManager.OnGameStateChanged += OnGameStateChanged;
        // Ajusta el estado inicial del controller
        if (GameManager.Instance != null) OnGameStateChanged(GameManager.Instance.CurrentState);
    }

    public override void FixedUpdateNetwork()
    {

        _model.UpdateStats(Runner.DeltaTime);

        if (!GetInput(out NetworkInputData networkInputData)) return;
        if (_model.IsDead || _model.IsStunned) return;

        // Movimiento
        Vector3 moveDirection = new Vector3(
            networkInputData.movementInputHorizontal,
            0,
            networkInputData.movementInputVertical
        );

        // ------ NITRO ------
        bool usingNitro = false;
        float acceleration = _myCharacterController.carAcceleration;
        float maxSpeed = _myCharacterController.carMaxSpeed;

        if (networkInputData.isNitroPressed && _model.CurrentNitro > 0)
        {
            float nitroToConsume = 25f * Runner.DeltaTime;
            if (_model.ConsumeNitro(nitroToConsume))
            {
                usingNitro = true;
                acceleration *= 1.5f;
                maxSpeed *= 2f;
            }
        }

        // Llama a Move pasando los valores correctos
        _myCharacterController.Move(moveDirection, acceleration, maxSpeed);
        

        //JUMP
        if (networkInputData.isJumpPressed)
            _myCharacterController.Jump();

        //SHOOT NORMAL
        if (networkInputData.isShootNormalPressed)
        {
            _myWeaponHandler.FireNormal();
        }

        //SHOOT SPECIAL
        if (networkInputData.isShootSpecialPressed)
        {
            // Solo disparar si tiene munición especial
            if (_model.SpecialAmmo != ModelPlayer.SpecialType.None)
            {
                _myWeaponHandler.FireSpecial(_model.SpecialAmmo);
                _model.SetSpecial(ModelPlayer.SpecialType.None); // Gasta la especial
            }
        }
        
        //DEBUG TAKE DAMAGE
        if (networkInputData.isTakeDamagePressed)
        {
            _myLifeHandler.ModifyLife(-25);
        }

    }

    #region RPC Methods

    // Nuevo método llamado desde el botón de la UI local:
    public void OnGoToMainMenuPressed()
    {
        if (Runner.IsServer)
            RPC_ShowHostDisconnectedPanel();
        else
        {
            FindObjectOfType<PlayerLocalUIHandler>().ShowLocalDisconnectPanel("Disconnecting");
            StartCoroutine(DisconnectAfterDelay());
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowHostDisconnectedPanel()
    {
        var uiHandler = FindObjectOfType<PlayerLocalUIHandler>();
        if(uiHandler != null)
        {
            string message = Runner.IsServer ? "Disconnecting" : "Host disconnected going to main menu";
            uiHandler.ShowLocalDisconnectPanel(message);
        }
        StartCoroutine(ReturnAllPlayersToMainMenuWithDelay());
    }

    private IEnumerator ReturnAllPlayersToMainMenuWithDelay()
    {
        yield return new WaitForSeconds(2f);
        // Destruye la UI Local antes de cambiar de escena
        var localUI = FindObjectOfType<PlayerLocalUIHandler>();
        if(localUI != null) Destroy(localUI.gameObject);

        if (Runner.IsServer)
            Runner.Shutdown();

        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator DisconnectAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        // Destruye la UI Local antes de cambiar de escena
        var localUI = FindObjectOfType<PlayerLocalUIHandler>();
        if(localUI != null) Destroy(localUI.gameObject);

        Runner.Disconnect(Runner.LocalPlayer);
        SceneManager.LoadScene("MainMenu");
    }

    #endregion
    
    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    
    void OnGameStateChanged(GameState state)
    {
        // Solo bloquea el movimiento del jugador local
        if (!Object.HasInputAuthority) return;

        if (state == GameState.WaitingForPlayers)
            enabled = false;
        else if (state == GameState.Playing)
            enabled = true;
        else if (state == GameState.Ended)
            enabled = false;
    }

    // Daño por colisión/crash
    /*private void OnCollisionEnter(Collision collision)
    {
        if (!HasInputAuthority && _model.IsDead) return;

        ModelPlayer otherModel = collision.gameObject.GetComponent<ModelPlayer>();
        if (otherModel != null && collision.contacts.Length > 0)
        {
            Vector3 contactNormal = collision.contacts[0].normal;
            float forwardDot = Vector3.Dot(contactNormal, -transform.forward);
            if (forwardDot > 0.7f && collision.relativeVelocity.magnitude > minCrashForce)
            {
                otherModel.ModifyLife(-crashDamage, Object.InputAuthority);
            }
        }
    }*/
}