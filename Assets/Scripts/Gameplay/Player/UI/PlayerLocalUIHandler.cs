using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLocalUIHandler : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public Image healthBar;
    public Image nitroBar;

    //private ModelPlayer modelPlayer;
    private NetworkCharacterControllerCustom _controller;
    private LifeHandler _lifeHandler;

    public void Init(ModelPlayer model, NetworkCharacterControllerCustom characterController, LifeHandler lifeHandler)
    {
        _controller = characterController;
        _lifeHandler = lifeHandler;

        // Barra llena al inicio
        healthBar.fillAmount = 1;
        // suscribo al evento
        _lifeHandler.OnLifeUpdate += norm => healthBar.fillAmount = norm * model.MaxHealth;
    }

    void Update()
    {
        speedText.text = $"{_controller.Velocity.magnitude:F1} km/h";
        nitroBar.fillAmount = _controller.Object.TryGetBehaviour<ModelPlayer>(out var mp) ? mp.Nitro : 0f;
    }
}