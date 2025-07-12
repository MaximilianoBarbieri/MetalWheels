using Fusion;
using UnityEngine;

public class NetworkCharacterControllerCustom : NetworkCharacterController
{
    [Header("Movimiento tipo auto")]
    public float carAcceleration = 800f;
    public float carRotationSpeed = 100f;
    public float carMaxSpeed = 500f;
    public float carBraking = 8f;
    
    [Header("CurrentNitro")]
    public float nitroAccelerationMultiplier = 2f;
    public float nitroMaxSpeed = 20f;

    public void Move(Vector3 inputDirection, float acceleration, float maxSpeed)
    {
        float deltaTime = Runner.DeltaTime;
        Vector3 previousPosition = transform.position;
        Vector3 moveVelocity = Velocity;

        float v = inputDirection.z;
        float h = inputDirection.x;

        if (Grounded && moveVelocity.y < 0) moveVelocity.y = 0f;

        moveVelocity.y += gravity * deltaTime;

        // Movimiento hacia adelante
        Vector3 forwardMove = transform.forward * v * acceleration * deltaTime;

        if (v != 0) {
            float turn = h * carRotationSpeed * deltaTime;
            transform.Rotate(0, turn, 0);
        }

        Vector3 horizontalVelocity = moveVelocity;
        horizontalVelocity.x = forwardMove.x;
        horizontalVelocity.z = forwardMove.z;

        Vector3 horizontalOnly = new Vector3(horizontalVelocity.x, 0, horizontalVelocity.z);
        if (horizontalOnly.magnitude > maxSpeed) {
            horizontalOnly = horizontalOnly.normalized * maxSpeed;
            horizontalVelocity.x = horizontalOnly.x;
            horizontalVelocity.z = horizontalOnly.z;
        }

        if (Mathf.Abs(v) < 0.1f) {
            horizontalVelocity.x = Mathf.Lerp(horizontalVelocity.x, 0, carBraking * deltaTime);
            horizontalVelocity.z = Mathf.Lerp(horizontalVelocity.z, 0, carBraking * deltaTime);
        }

        moveVelocity.x = horizontalVelocity.x;
        moveVelocity.z = horizontalVelocity.z;

        Controller.Move(moveVelocity * deltaTime);
        Velocity = (transform.position - previousPosition) * Runner.TickRate;
        Grounded = Controller.isGrounded;
    }
}