using Cinemachine;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterControllerCustom))]
[RequireComponent(typeof(WeaponHandler))]
[RequireComponent(typeof(LifeHandler))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private ModelPlayer _model;

    private NetworkCharacterControllerCustom _myCharacterController;
    private WeaponHandler _myWeaponHandler;

    public CinemachineVirtualCamera vCamPrefab;
    private CinemachineVirtualCamera myCam;

    //OLD vars
    [SerializeField] public ModelPlayer model;

    [SerializeField] private float jumpCooldown = 2f;
    [SerializeField] private float minCrashForce = 7f;
    [SerializeField] private int crashDamage = 30;

    private float nextJumpTime = 0f;


    public override void Spawned()
    {
        // Solo instanciar cámara si es mi propio jugador
        if (!Object.HasInputAuthority) return;
        if (vCamPrefab == null) return;

        myCam = Instantiate(vCamPrefab);

        // Buscar el CameraTarget (empty hijo del auto) o el propio transform
        Transform camTarget = transform.Find("CameraTarget");
        if (camTarget == null) camTarget = transform;

        myCam.Follow = camTarget;
        myCam.LookAt = camTarget;
    }

    private void Awake()
    {
        _myCharacterController = GetComponent<NetworkCharacterControllerCustom>();
        _myWeaponHandler = GetComponent<WeaponHandler>();

        var lifeHandler = GetComponent<LifeHandler>();

        lifeHandler.OnDeadChange += isDead =>
        {
            _myCharacterController.Controller.enabled = !isDead;
            enabled = !isDead;
        };

        lifeHandler.OnRespawn += () => { _myCharacterController.Teleport(transform.position); };
    }

    public override void FixedUpdateNetwork()
    {
        #region NEW

        model.UpdateStats(Runner.DeltaTime);

        if (!GetInput(out NetworkInputData networkInputData))
        {
            Debug.LogWarning("❌ GetInput NO devuelve datos en este cliente.");
            return;
        }
        else
        {
            Debug.Log(
                $"✅ GetInput OK - H: {networkInputData.movementInputHorizontal}, V: {networkInputData.movementInputVertical}");
        }

        if (model.IsDead || model.IsStunned) return;

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
            //_myWeaponHandler.Fire();
        }

        //SHOOT SPECIAL
        if (networkInputData.isShootSpecialPressed)
        {
            //_myWeaponHandler.Fire();
        }

        #endregion

        #region OLD

        /*//OLD
        if (!HasInputAuthority) return;

        // Respawn timer lógica
        if (model.IsDead)
        {
            model.RespawnTimer -= Runner.DeltaTime;
            if (model.RespawnTimer <= 0)
            {
                // Buscar spawn point libre y respawnear
                //TODO: VER COMO HACER PARA PASARLE EL SPAWNPOINT AL QUE DEBE IR sin pisarse con los otros players
                //Transform spawn = SpawnManager.Instance.GetFreePlayerSpawnPoint();
                //model.RespawnAt(spawn.position, spawn.rotation);
                Vector3 spawn = Vector3.zero;
                model.RespawnAt(spawn, new Quaternion(0,0,0,0));
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        // Salto con cooldown
        if (Input.GetKeyDown(KeyCode.Space) && Runner.SimulationTime >= nextJumpTime)
        {
            rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
            nextJumpTime = (float)Runner.SimulationTime + jumpCooldown;
        }

        // Nitro
        if (Input.GetKey(KeyCode.LeftShift) && model.Nitro > 0f)
        {
            rb.AddForce(transform.forward * model.MaxSpeed * 0.7f, ForceMode.Acceleration);
            model.Nitro -= Runner.DeltaTime * 0.3f;
            model.Nitro = Mathf.Clamp01(model.Nitro);
        }

        // Shoot normal
        if (Input.GetKeyDown(KeyCode.O))
        {
            Runner.Spawn(projectileNormalPrefab, shootPoint.position, shootPoint.rotation, Object.InputAuthority);
        }

        // Shoot special
        if (Input.GetKeyDown(KeyCode.P) && model.SpecialAmmo != ModelPlayer.SpecialType.None)
        {
            var proj = Runner.Spawn(projectileSpecialPrefab, shootPoint.position, shootPoint.rotation, Object.InputAuthority);
            if (proj != null && proj.GetComponent<Projectile>() != null)
                proj.GetComponent<Projectile>().specialType = model.SpecialAmmo;
            model.SpecialAmmo = ModelPlayer.SpecialType.None;
        }*/

        #endregion
    }

    // Daño por colisión/crash
    private void OnCollisionEnter(Collision collision)
    {
        if (!HasInputAuthority && model.IsDead) return;

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