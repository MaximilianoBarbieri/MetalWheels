using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLocalUIHandler : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public Slider healthSlider;
    public Slider nitroSlider;

    //private ModelPlayer modelPlayer;
    private NetworkCharacterControllerCustom _controller;
    private LifeHandler _lifeHandler;

    public void Init(ModelPlayer model, NetworkCharacterControllerCustom characterController, LifeHandler lifeHandler)
    {
        _controller = characterController;
        _lifeHandler = lifeHandler;

        healthSlider.maxValue = model.MaxHealth;
        // suscribo al evento
        _lifeHandler.OnLifeUpdate += norm => healthSlider.value = norm * model.MaxHealth;
    }

    void Update()
    {
        speedText.text = $"{_controller.Velocity.magnitude:F1} km/h";
        nitroSlider.value = _controller.Object.TryGetBehaviour<ModelPlayer>(out var mp) ? mp.Nitro : 0f;
    }
}