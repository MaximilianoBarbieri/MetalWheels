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
        
        // Usar Clamp y chequeo
        float normalizedLife = _model.MaxHealth > 0 ? Mathf.Clamp01((float)_model.CurrentHealth / _model.MaxHealth) : 0f;
        healthBar.fillAmount = normalizedLife;
        
        // Usar Clamp y chequeo
        float normalizedNitro= _model.MaxNitro > 0 ? Mathf.Clamp01(_model.CurrentNitro / _model.MaxNitro) : 0f;
        nitroBar.fillAmount = normalizedNitro;

        _lifeHandler.OnLifeUpdate += UpdateHealthUI;
    }

    void Update()
    {
        speedText.text = $"{_controller.Velocity.magnitude:F1} km/h";
        nitroBar.fillAmount = Mathf.Clamp01(_model.CurrentNitro / _model.MaxNitro);
    }

    void UpdateHealthUI(float normalizedLife)
    {
        healthBar.fillAmount = normalizedLife;
    }
}