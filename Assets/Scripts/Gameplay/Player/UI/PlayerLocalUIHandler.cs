using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLocalUIHandler : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public Image healthBar;
    public Image nitroBar;

    private NetworkCharacterControllerCustom _controller;
    private ModelPlayer _model;
    private LifeHandler _lifeHandler;

    public void Init(ModelPlayer model, NetworkCharacterControllerCustom characterController, LifeHandler lifeHandler)
    {
        _model = model;
        _controller = characterController;
        _lifeHandler = lifeHandler;

        healthBar.fillAmount = 1f;

        _lifeHandler.OnLifeUpdate += UpdateHealthUI;
    }

    void Update()
    {
        speedText.text = $"{_controller.Velocity.magnitude:F1} km/h";
        nitroBar.fillAmount = _model.Nitro;
    }

    void UpdateHealthUI(float normalizedLife)
    {
        healthBar.fillAmount = normalizedLife;
    }
}