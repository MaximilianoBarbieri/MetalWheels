using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    public GameObject nicknamePanel, selectCarPanel, roomPanel;
    public InputField nicknameInput;
    public Button startGameButton;
    public Text connectedPlayersText;

    private void Start()
    {
        nicknamePanel.SetActive(true);
        selectCarPanel.SetActive(false);
        roomPanel.SetActive(false);
    }

    public void OnNicknameEntered()
    {
        PlayerData.Nickname = nicknameInput.text;
        nicknamePanel.SetActive(false);
        selectCarPanel.SetActive(true);
    }

    public void OnCarSelected(int carIndex)
    {
        PlayerData.CarSelected = carIndex;
        selectCarPanel.SetActive(false);
        roomPanel.SetActive(true);
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
