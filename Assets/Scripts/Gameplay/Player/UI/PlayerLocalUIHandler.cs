using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLocalUIHandler : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public Slider healthSlider;
    public Slider nitroSlider;

    private ModelPlayer modelPlayer;
    private NetworkCharacterControllerCustom controller;

    void Update()
    {
        if (modelPlayer == null || controller == null) return;

        speedText.text = $"{controller.Velocity.magnitude:F1} km/h";
        healthSlider.maxValue = modelPlayer.MaxHealth;
        healthSlider.value = modelPlayer.CurrentHealth;
        nitroSlider.value = modelPlayer.Nitro;
    }

    public void Init(ModelPlayer model, NetworkCharacterControllerCustom characterController)
    {
        modelPlayer = model;
        controller = characterController;
    }
}