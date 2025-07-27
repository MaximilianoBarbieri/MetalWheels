using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGlobalUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Image healthBar;      // Image tipo “Fill”
    [SerializeField] private Vector3 offset = new(0f, 6f, 0f);

    private Camera _cam;
    private Transform _target;

    void Awake()
    {
        _cam = Camera.main;
    }

    // Inicializa el nickname y asigna el target a seguir.

    public void Init(string nickname, Transform playerTransform)
    {
        nicknameText.text = nickname;
        _target = playerTransform;

        // Desparenciar de cualquier padre
        transform.SetParent(null, false);

        // Posición inicial
        transform.position = _target.position + offset;

        // Barra llena al inicio
        healthBar.fillAmount = 1f;
    }

    // Para cambiar el texto del nickname en tiempo real si fuera necesario.
    public void UpdateNickName(string newNick)
    {
        nicknameText.text = newNick;
    }

    // Llamado desde LifeHandler para actualizar la barra de vida.
    public void UpdateLifeBar(float normalizedLife)
    {
        healthBar.fillAmount = Mathf.Clamp01(normalizedLife);
    }

    // Actualiza la posición y rotación cada frame.
    void LateUpdate()
    {
        if (_target == null) return;

        // 1) Sigue la posición del jugador + offset
        transform.position = _target.position + offset;

        // 2) Billboard que apunte su frente hacia la cámara
        Vector3 dirToCam = transform.position - _cam.transform.position;
        transform.rotation = Quaternion.LookRotation(dirToCam);
    }
}