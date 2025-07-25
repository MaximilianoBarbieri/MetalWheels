using UnityEngine;
using Cinemachine;

public class PlayerCameraHandler : MonoBehaviour
{
    [Header("References")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform targetFollow; // el auto a seguir

    [Header("Shake")]
    public float shakeDuration = 0.5f;
    public float shakeAmplitude = 2f;
    public float shakeFrequency = 2f;
    private float shakeTimer;
    private CinemachineBasicMultiChannelPerlin perlin;

    [Header("Follow Distance")]
    public float baseFollowDistance = 8f;
    public float maxFollowDistance = 12f; // cuanto se aleja al acelerar a fondo
    public float minFollowDistance = 6f; // cuanto se acerca al frenar fuerte
    public float distanceLerpSpeed = 5f;

    private CinemachineTransposer transposer;

    void Awake()
    {
        if (!virtualCamera) virtualCamera = GetComponent<CinemachineVirtualCamera>();
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (perlin != null)
        {
            perlin.m_AmplitudeGain = 0f;
            perlin.m_FrequencyGain = shakeFrequency;
        }
    }

    void Update()
    {
        // -- SHAKE --
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (perlin != null)
                perlin.m_AmplitudeGain = shakeAmplitude;
        }
        else
        {
            if (perlin != null)
                perlin.m_AmplitudeGain = 0f;
        }

        // -- FOLLOW DISTANCE DINÁMICA --
        if (targetFollow != null && transposer != null)
        {
            float speed = 0f;

            // Podés obtener la velocidad del auto accediendo a su Rigidbody o Model
            // Por ejemplo:
            Rigidbody rb = targetFollow.GetComponent<Rigidbody>();
            if (rb != null)
                speed = rb.velocity.magnitude;

            // Suponiendo que tu auto tiene una velocidad máxima conocida (ej: 20)
            float maxSpeed = 20f;

            // Acelerando: aleja la cámara, Frenando: acerca la cámara
            float targetDistance = Mathf.Lerp(baseFollowDistance, maxFollowDistance, speed / maxSpeed);
            // Si querés que se acerque más al frenar, podés interpolar también hacia minFollowDistance si la velocidad cae bruscamente, o según el input de freno.

            Vector3 followOffset = transposer.m_FollowOffset;
            followOffset.z = Mathf.Lerp(followOffset.z, -targetDistance, Time.deltaTime * distanceLerpSpeed); // Negativo si la cámara está "atrás" del auto
            transposer.m_FollowOffset = followOffset;
        }
    }

    public void Shake(float magnitude = 1f)
    {
        shakeAmplitude = 2f * magnitude;
        shakeTimer = shakeDuration * magnitude;
    }
}