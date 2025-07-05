using Fusion;
using UnityEngine;

public class NetworkCharacterControllerCustom : NetworkCharacterController
{
    [Header("Movimiento tipo auto")]
    public float carAcceleration = 800f;
    public float carRotationSpeed = 100f;
    public float carMaxSpeed = 500f;
    public float carBraking = 8f;
    
    [Header("Nitro")]
    public float nitroAccelerationMultiplier = 2f;
    public float nitroMaxSpeed = 20f;

    public override void Move(Vector3 inputDirection)
    {
        float deltaTime = Runner.DeltaTime;
        Vector3 previousPosition = transform.position;
        Vector3 moveVelocity = Velocity;

        float v = inputDirection.z; 
        float h = inputDirection.x; 

        // Si estamos en el suelo y cayendo, cancelamos la caída
        if (Grounded && moveVelocity.y < 0)
            moveVelocity.y = 0f;

        // Aplicar gravedad manual
        moveVelocity.y += gravity * deltaTime;
        
        //Check Nitro
        bool isUsingNitro = false;
        if (Object.HasInputAuthority && Runner.TryGetInputForPlayer(Object.InputAuthority, out NetworkInputData inputData))
        {
            isUsingNitro = inputData.isNitroPressed;
        }
        
        // Aceleración con o sin nitro
        float currentAcceleration = isUsingNitro ? carAcceleration * nitroAccelerationMultiplier : carAcceleration;
        float currentMaxSpeed = isUsingNitro ? nitroMaxSpeed : carMaxSpeed;

        // Movimiento hacia adelante
        Vector3 forwardMove = transform.forward * v * currentAcceleration * deltaTime;
        
        if (v != 0)
        {
            // Rotación mientras se mueve
            float turn = h * carRotationSpeed * deltaTime;
            transform.Rotate(0, turn, 0);
        }

        // Aplicar aceleración al movimiento horizontal
        Vector3 horizontalVelocity = moveVelocity;
        horizontalVelocity.x = forwardMove.x;
        horizontalVelocity.z = forwardMove.z;

        // Limitar velocidad máxima (según si hay nitro)
        Vector3 horizontalOnly = new Vector3(horizontalVelocity.x, 0, horizontalVelocity.z);
        if (horizontalOnly.magnitude > currentMaxSpeed)
        {
            horizontalOnly = horizontalOnly.normalized * carMaxSpeed;
            horizontalVelocity.x = horizontalOnly.x;
            horizontalVelocity.z = horizontalOnly.z;
        }

        // Frenado si no hay input vertical
        if (Mathf.Abs(v) < 0.1f)
        {
            horizontalVelocity.x = Mathf.Lerp(horizontalVelocity.x, 0, carBraking * deltaTime);
            horizontalVelocity.z = Mathf.Lerp(horizontalVelocity.z, 0, carBraking * deltaTime);
        }

        // Asignar nueva velocidad
        moveVelocity.x = horizontalVelocity.x;
        moveVelocity.z = horizontalVelocity.z;

        // Mover con el CharacterController
        Controller.Move(moveVelocity * deltaTime);

        // Calcular la velocidad real
        Velocity = (transform.position - previousPosition) * Runner.TickRate;
        Grounded = Controller.isGrounded;
    }
}