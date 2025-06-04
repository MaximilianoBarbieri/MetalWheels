using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    public GameObject nicknamePanel, selectCarPanel, roomPanel;
    public TMP_InputField nicknameInput;
    public Button startGameButton;
    public Button continueButton;
    public Button backButton;
    public TMP_Text connectedPlayersText;
    
    [SerializeField] private GameObject highlightA;
    [SerializeField] private GameObject highlightB;
    private int selectedCar = -1;
    
    private NetworkRunner runner;

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        ShowPanel(nicknamePanel);
        InvokeRepeating(nameof(UpdatePlayerCount), 0.5f, 1f);
    }
    
    private void UpdatePlayerCount()
    {
        if (runner != null)
        {
            UpdateConnectedPlayers(runner.ActivePlayers.Count());
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        nicknamePanel.SetActive(false);
        selectCarPanel.SetActive(false);
        roomPanel.SetActive(false);

        panelToShow.SetActive(true);
        
        // Mostrar botones según panel activo
        continueButton.gameObject.SetActive(panelToShow == nicknamePanel || panelToShow == selectCarPanel);
        backButton.gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(panelToShow == roomPanel);
    }

    public void OnContinue()
    {
        if (nicknamePanel.activeSelf)
        {
            if (!string.IsNullOrEmpty(nicknameInput.text))
            {
                PlayerData.Nickname = nicknameInput.text;
                ShowPanel(selectCarPanel);
            }
        }
        else if (selectCarPanel.activeSelf)
        {
            // Esperamos que se haya seleccionado un auto antes
            ShowPanel(roomPanel);
        }
    }

    public void OnBack()
    {
        if (roomPanel.activeSelf)
        {
            ShowPanel(selectCarPanel);
        }
        else if (selectCarPanel.activeSelf)
        {
            ShowPanel(nicknamePanel);
        }
        else if (nicknamePanel.activeSelf)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void OnCarSelected(int carIndex)
    {
        PlayerData.CarSelected = carIndex;
        selectedCar = carIndex;

        // Mostrar cuál está seleccionado
        highlightA.SetActive(carIndex == 0);
        highlightB.SetActive(carIndex == 1);

        // Habilitar continuar
        continueButton.interactable = true;
    }

    public void UpdateConnectedPlayers(int count)
    {
        connectedPlayersText.text = $"Jugadores conectados: {count}";
        startGameButton.interactable = (count >= 2);
    }

    public void OnStartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}