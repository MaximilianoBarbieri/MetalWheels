using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    public GameObject nicknamePanel, selectCarPanel, roomPanel;
    public InputField nicknameInput;
    public Button startGameButton;
    public Button continueButton;
    public Button backButton;
    public Text connectedPlayersText;

    private void Start()
    {
        ShowPanel(nicknamePanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        nicknamePanel.SetActive(false);
        selectCarPanel.SetActive(false);
        roomPanel.SetActive(false);

        panelToShow.SetActive(true);
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
    }

    public void OnCarSelected(int carIndex)
    {
        PlayerData.CarSelected = carIndex;
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