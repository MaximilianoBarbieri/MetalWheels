using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LobbyUI : MonoBehaviour
{
    [Header("NetworkRunnerHandler")]
    [SerializeField] private NetworkRunnerHandler _networkHandler;

    [Header("Panels")]
    [SerializeField] private GameObject _initialPanel;
    [SerializeField] private GameObject _sessionPanel;
    [SerializeField] private GameObject _hostPanel;
    [SerializeField] private GameObject _statusPanel;
    
    [Header("Texts")]
    [SerializeField] private TMP_Text _statusText;
    
    [Header("InputFields")]
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private TMP_InputField _hostSessionNameInput;
    
    [Header("Buttons")]
    [SerializeField] private Button _carA;
    [SerializeField] private Button _carB;
    [SerializeField] private Button _backBTN; //botone de volver hacia atras.
    [SerializeField] private Button _joinLobbyBTN; //me lleva al panel con lista de salas
    [SerializeField] private Button _goToHostPanelBTN;//me lleva al panel para ponerle nombre a mi sala
    [SerializeField] private Button _hostBTN;//btn para terminar de crear una sala (donde soy host)
    
    
    private void Start()
    {
        _joinLobbyBTN.onClick.AddListener(Btn_JoinLobby);
        _goToHostPanelBTN.onClick.AddListener(Btn_ShowHostPanel);
        _hostBTN.onClick.AddListener(Btn_CreateGameSession);

        _networkHandler.OnJoinedLobby += () =>
        {
            _statusPanel.SetActive(false);
            _sessionPanel.SetActive(true);
        };
        
        //TODO: Usar PlayerData en vez de PlayerPrefs 
        _carA.onClick.AddListener(() =>
        {
            PlayerData.CarSelected = 0;
            //PlayerPrefs.SetInt("PlayerSelected", 0);
        });
        
        _carB.onClick.AddListener(() =>
        {
            PlayerData.CarSelected = 1;
            //PlayerPrefs.SetInt("PlayerSelected", 1));
        });
    }
    
    void Btn_JoinLobby()
    {
        _networkHandler.JoinLobby();

        //TODO: Usar PlayerData en vez de PlayerPrefs 
        //PlayerPrefs.SetString("UserNickName", _nicknameInput.text);
        PlayerData.Nickname = _nicknameInput.text;

        /*_initialPanel.SetActive(false);
        _statusPanel.SetActive(true);*/
        ShowPanel(_statusPanel);

        _statusText.text = "Joining Lobby...";
    }
    
    void Btn_ShowHostPanel()
    {
        _sessionPanel.SetActive(false);
        _hostPanel.SetActive(true);
    }
    
    void Btn_CreateGameSession()
    {
        _hostBTN.interactable = false;
        _networkHandler.CreateGame(_hostSessionNameInput.text, "Gameplay");
    }


    private void Btn_Back()
    {
        //TODO: hacer algo parecido a esto que tenia
        //ir del panel actual al anterior, en caso de no haber mas paneles volver a la scene "MainMenu"
        //tener en cuenta que hay un panel que crea una sala, si se vuelve atras en ese panel borrar la sala creada (de ser necesario)
        /*if (roomPanel.activeSelf)
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
        }*/
    }
    
    //TODO: modificar este metodo para demostrar visualmente cuando de los dos botones de autos seleccionaste.
    // no hacer que se deshabilite el auto que no se selecciono, sino que sea algo visual como un "disableColor"
    /*public void OnCarSelected(int carIndex)
    {
        PlayerData.CarSelected = carIndex;
        selectedCar = carIndex;

        // Mostrar cuál está seleccionado
        highlightA.SetActive(carIndex == 0);
        highlightB.SetActive(carIndex == 1);

        // Habilitar continuar
        continueButton.interactable = true;
    }*/
    
    private void ShowPanel(GameObject panelToShow)
    {
        _initialPanel.SetActive(false);
        _sessionPanel.SetActive(false);
        _hostPanel.SetActive(false);
        _statusPanel.SetActive(false);

        panelToShow.SetActive(true);
    }
    
    /*-----------------------------------------------------------------------------------*/
    
    
    /*private void UpdatePlayerCount()
    {
        if (runner != null)
        {
            UpdateConnectedPlayers(runner.ActivePlayers.Count());
        }
    }*/
    



    /*public void UpdateConnectedPlayers(int count)
    {
        connectedPlayersText.text = $"Jugadores conectados: {count}";
        startGameButton.interactable = (count >= 2);
    }*/
}