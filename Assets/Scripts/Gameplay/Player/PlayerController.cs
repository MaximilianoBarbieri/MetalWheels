using Cinemachine;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterControllerCustom))]
[RequireComponent(typeof(LifeHandler))]
/*[RequireComponent(typeof(WeaponHandler))]*/
public class PlayerController : NetworkBehaviour
{
    private ModelPlayer _model;

    private NetworkCharacterControllerCustom _myCharacterController;
    private WeaponHandler _myWeaponHandler;
    private LifeHandler _myLifeHandler;

    public CinemachineVirtualCamera vCamPrefab;
    private CinemachineVirtualCamera myCam;
    
    [SerializeField] private float jumpCooldown = 2f;
    [SerializeField] private float minCrashForce = 7f;
    [SerializeField] private int crashDamage = 30;

    private float nextJumpTime = 0f;
    
    //UI player
    [SerializeField] private PlayerLocalUIHandler playerLocalUIPrefab;

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
            Canvas globalCanvas = GameObject.Find("Canvas_LocalUI").GetComponent<Canvas>();
            var uiInstance = Instantiate(playerLocalUIPrefab, globalCanvas.transform);
            uiInstance.Init(_model, _myCharacterController, _myLifeHandler);
        }
        else
        {
            Debug.LogError("Prefab PlayerLocalUI no asignado");
        }
    }

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

    public override void FixedUpdateNetwork()
    {
        #region NEW

        _model.UpdateStats(Runner.DeltaTime);

        if (!GetInput(out NetworkInputData networkInputData))
        {
            Debug.LogWarning("❌ GetInput NO devuelve datos en este cliente.");
            return;
        }

        if (_model.IsDead || _model.IsStunned) return;

        //MOVIMIENTO
        Vector3 moveDirection = new Vector3(
            networkInputData.movementInputHorizontal,
            0,
            networkInputData.movementInputVertical
        );
        _myCharacterController.Move(moveDirection);

        //JUMP
        if (networkInputData.isJumpPressed)
            _myCharacterController.Jump();

        //NITRO
        //Todo: no deberia hacer falta xq se ejecuta dentro de _myCharacterController.Move
        /*if (networkInputData.isNitroPressed)
        {
            _myCharacterController.Move(moveDirection);
        }*/

        //SHOOT NORMAL
        if (networkInputData.isShootNormalPressed)
        {
            _myLifeHandler.ModifyLife(-25);
            //_myWeaponHandler.Fire();
        }

        //SHOOT SPECIAL
        if (networkInputData.isShootSpecialPressed)
        {
            _myLifeHandler.ModifyLife(25);
            //_myWeaponHandler.Fire();
        }

        #endregion
    }

    // Daño por colisión/crash
    private void OnCollisionEnter(Collision collision)
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
    }
}