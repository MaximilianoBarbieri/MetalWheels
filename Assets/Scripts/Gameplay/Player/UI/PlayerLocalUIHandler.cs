using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/*
Se suscribe a los eventos del GameManager en Init()

En cada callback activa/desactiva paneles o actualiza textos.

Expone métodos como SetVictoryPanelActive(bool), SetWaitingPanelActive(bool), etc.
*/
public class PlayerLocalUIHandler : MonoBehaviour
{
    [Header("PROPERTIES")] 
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI timerText;
    public Image healthBar;
    public Image nitroBar;

    [Header("PANELS")] 
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject waitingPanel;
    public GameObject hostDisconnectedPanel;
    
    [Header("TEXT PANELS")] 
    public TextMeshProUGUI hostDisconnectedText;

    private PlayerController _controller;
    private ModelPlayer _model;
    private NetworkCharacterControllerCustom _networkController;
    private LifeHandler _lifeHandler;
    
    [Header("BUTTONS")]
    public Button goToMainMenuButton;

    private bool isWinner = false; // flag para UI
    
    public void Init(ModelPlayer model, PlayerController controller, NetworkCharacterControllerCustom characterController, LifeHandler lifeHandler)
    {
        _model = model;
        _controller = controller;
        _networkController = characterController;
        _lifeHandler = lifeHandler;
        
        _lifeHandler.OnLifeUpdate += UpdateHealthUI;
        GameManager.OnGameStateChanged += HandleGameStateChanged;
        GameManager.OnTimerChanged += UpdateTimerUI;
        GameManager.OnHostDisconnected += ShowHostDisconnectedPanel;
        GameManager.OnWinnerChanged += HandleWinnerChanged;
        
        // Setear UI inicial según estado actual
        if (GameManager.Instance != null)
        {
            HandleGameStateChanged(GameManager.Instance.CurrentState);
            HandleWinnerChanged(GameManager.Instance.Winner);
        }
        
        goToMainMenuButton.onClick.AddListener(OnGoToMainMenuPressed);
        
        // Delay para sincronización de estado al ingresar el Client
        StartCoroutine(DelayedInitialUIRefresh());

        // Usar Clamp y chequeo
        float normalizedLife =
            _model.MaxHealth > 0 ? Mathf.Clamp01((float)_model.CurrentHealth / _model.MaxHealth) : 0f;
        healthBar.fillAmount = normalizedLife;

        // Usar Clamp y chequeo
        float normalizedNitro = _model.MaxNitro > 0 ? Mathf.Clamp01(_model.CurrentNitro / _model.MaxNitro) : 0f;
        nitroBar.fillAmount = normalizedNitro;
    }

    void Update()
    {
        speedText.text = $"{_networkController.Velocity.magnitude:F1} km/h";
        killsText.text = $"Kills: {_model.Kills.ToString()}";
        nitroBar.fillAmount = Mathf.Clamp01(_model.CurrentNitro / _model.MaxNitro);
    }

    void UpdateHealthUI(float normalizedLife)
    {
        healthBar.fillAmount = normalizedLife;
    }

    void HandleGameStateChanged(GameState state)
    {
        // Forzá update de isWinner cada vez que cambie el estado por si NetworkPlayer.Local aún no estaba
        if (GameManager.Instance != null && NetworkPlayer.Local != null)
        {
            isWinner = (GameManager.Instance.Winner == NetworkPlayer.Local.Object.InputAuthority);
        }

        Debug.Log($"[UI] HandleGameStateChanged recibido: {state}, isWinner: {isWinner}, player: {gameObject.name}");

        victoryPanel.SetActive(state == GameState.Ended && isWinner);
        defeatPanel.SetActive(state == GameState.Ended && !isWinner);
        waitingPanel.SetActive(state == GameState.WaitingForPlayers);

        UpdateGoToMainMenuButtonVisibility();
    }

    void HandleWinnerChanged(PlayerRef winner)
    {
        if (NetworkPlayer.Local != null)
            isWinner = (winner == NetworkPlayer.Local.Object.InputAuthority);

        Debug.Log($"[UI] HandleWinnerChanged: winner={winner}, yo={NetworkPlayer.Local?.Object.InputAuthority}, isWinner={isWinner}");
    }

    void UpdateTimerUI(float t)
    {
        timerText.text = Mathf.CeilToInt(t).ToString();
    }

    private void ShowHostDisconnectedPanel()
    {
        hostDisconnectedPanel.SetActive(true);
    }
    
    IEnumerator DelayedInitialUIRefresh()
    {
        // Espera a que GameManager.Instance esté sincronizado y el CurrentState sea Playing
        float timeout = 2f;
        float elapsed = 0f;
        while (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.WaitingForPlayers)
        {
            elapsed += Time.deltaTime;
            if (elapsed > timeout)
                break;
            yield return null;
        }
        // Forzar update una vez más
        if (GameManager.Instance != null)
            HandleGameStateChanged(GameManager.Instance.CurrentState);
    }
    
    private void UpdateGoToMainMenuButtonVisibility()
    {
        bool shouldShow =
            (victoryPanel != null && victoryPanel.activeSelf) ||
            (defeatPanel != null && defeatPanel.activeSelf) ||
            (waitingPanel != null && waitingPanel.activeSelf);

        if (goToMainMenuButton != null) goToMainMenuButton.gameObject.SetActive(shouldShow);
    }
    
    private void OnGoToMainMenuPressed()
    {
        _controller.OnGoToMainMenuPressed();
    }
    
    public void ShowLocalDisconnectPanel(string message)
    {
        if (hostDisconnectedPanel != null)
        {
            hostDisconnectedPanel.SetActive(true);
            hostDisconnectedPanel.GetComponentInChildren<TextMeshProUGUI>().text = message;
        }

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(false);

        if (goToMainMenuButton != null) goToMainMenuButton.interactable = false;
    }

    void OnDestroy()
    {
        _lifeHandler.OnLifeUpdate -= UpdateHealthUI;

        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        GameManager.OnTimerChanged -= UpdateTimerUI;
        GameManager.OnHostDisconnected -= ShowHostDisconnectedPanel;
        GameManager.OnWinnerChanged -= HandleWinnerChanged;
    }
}